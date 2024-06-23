using Amazon.S3.Model;
using Amazon.S3;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.IO;
using System.Threading;

namespace Cflashsoft.Framework.S3
{
    /// <summary>
    /// A writable stream that provides the ability to upload to S3 in memory efficient chunks.
    /// </summary>
    /// <remarks>
    /// Adapted from https://stackoverflow.com/questions/55065042/how-to-upload-stream-of-a-file-to-aws-s3-in-asp-net
    /// </remarks>
    public class S3UploadStream : Stream
    {
        private static readonly RecyclableMemoryStreamManager StreamManager = new RecyclableMemoryStreamManager();

        /* Note the that maximum size (as of now) of a file in S3 is 5TB so it isn't
         * safe to assume all uploads will work here.  MAX_PART_SIZE times MAX_PART_COUNT
         * is ~50TB, which is too big for S3. */
        private const long MIN_PART_LENGTH = 5L * 1024 * 1024; // all parts but the last this size or greater
        private const long MAX_PART_LENGTH = 5L * 1024 * 1024 * 1024; // 5GB max per PUT
        private const long MAX_PART_COUNT = 10000; // no more than 10,000 parts total
        private const long DEFAULT_PART_LENGTH = MIN_PART_LENGTH;

        private bool _disposed = false;
        private bool _completed = false;

        private IAmazonS3 _s3 = null;
        private string _s3BucketName = null;
        private string _s3Key = null;

        private long _partLength = DEFAULT_PART_LENGTH;
        private int _partCount = 0;
        private string _uploadId = null;

        private long _position = 0; // based on bytes written
        private long _length = 0; // based on bytes written or SetLength, whichever is larger (no truncation)

        private List<Task> _uploadTasks = new List<Task>();
        private ConcurrentDictionary<int, string> _partETags = new ConcurrentDictionary<int, string>();

        private RecyclableMemoryStream _currentStream = null;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead => false;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite => true;

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        public override long Length => _length = Math.Max(_length, _position);

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public override long Position
        {
            get => _position;
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// Inititializes a new instance of the S3UploadStream class.
        /// </summary>
        /// <param name="s3">The s3 client.</param>
        /// <param name="s3uri">Uri to s3 bucket and key.</param>
        /// <param name="partLength">Specifies the size of the chunks to send to s3.</param>
        public S3UploadStream(IAmazonS3 s3, string s3uri, long partLength = DEFAULT_PART_LENGTH)
            : this(s3, new Uri(s3uri), partLength)
        {
        }

        /// <summary>
        /// Inititializes a new instance of the S3UploadStream class.
        /// </summary>
        /// <param name="s3">The s3 client.</param>
        /// <param name="s3uri">Uri to s3 bucket and key.</param>
        /// <param name="partLength">Specifies the size of the chunks to send to s3.</param>
        public S3UploadStream(IAmazonS3 s3, Uri s3uri, long partLength = DEFAULT_PART_LENGTH)
            : this(s3, s3uri.Host, s3uri.LocalPath.Substring(1), partLength)
        {
        }

        /// <summary>
        /// Inititializes a new instance of the S3UploadStream class.
        /// </summary>
        /// <param name="s3">The s3 client.</param>
        /// <param name="bucket">The s3 bucket to upload to.</param>
        /// <param name="key">The s3 key for the data to upload.</param>
        /// <param name="partLength">Specifies the size of the chunks to send to s3.</param>
        public S3UploadStream(IAmazonS3 s3, string bucket, string key, long partLength = DEFAULT_PART_LENGTH)
        {
            _s3 = s3;
            _s3BucketName = bucket;
            _s3Key = key;
            _partLength = partLength;
        }

        /// <summary>
        /// Not supported. When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();

        /// <summary>
        /// Not supported. When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        public override void SetLength(long value)
        {
            _length = Math.Max(_length, value);
            _partLength = Math.Max(MIN_PART_LENGTH, Math.Min(MAX_PART_LENGTH, _length / MAX_PART_COUNT));
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count == 0)
                return;

            // write as much of the buffer as will fit to the current part, and if needed
            // allocate a new part and continue writing to it (and so on).
            var currentOffset = offset;
            var currentCount = Math.Min(count, buffer.Length - offset); // don't over-read the buffer, even if asked to

            do
            {
                if (_currentStream == null || _currentStream.Length >= _partLength)
                    Task.Run(() => StartNewPartAsync(CancellationToken.None)).Wait();

                var remaining = _partLength - _currentStream.Length;
                var length = Math.Min(currentCount, (int)remaining);

                _currentStream.Write(buffer, currentOffset, length);

                _position += length;

                currentCount -= length;
                currentOffset += length;
            } while (currentCount > 0);
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (count == 0)
                return;

            // write as much of the buffer as will fit to the current part, and if needed
            // allocate a new part and continue writing to it (and so on).
            var currentOffset = offset;
            var currentCount = Math.Min(count, buffer.Length - offset); // don't over-read the buffer, even if asked to

            do
            {
                if (_currentStream == null || _currentStream.Length >= _partLength)
                    await StartNewPartAsync(cancellationToken);

                var remaining = _partLength - _currentStream.Length;
                var length = Math.Min(currentCount, (int)remaining);

                await _currentStream.WriteAsync(buffer, currentOffset, length, cancellationToken);

                _position += length;

                currentCount -= length;
                currentOffset += length;
            } while (currentCount > 0);
        }

        private async Task StartNewPartAsync(CancellationToken cancellationToken)
        {
            if (_currentStream != null)
                await CommitUploadAsync(false, cancellationToken);

            _partLength = Math.Min(MAX_PART_LENGTH, Math.Max(_partLength, (_partCount / 2 + 1) * MIN_PART_LENGTH));
            _currentStream = StreamManager.GetStream("S3UploadStream", _partLength);
        }

        private async Task CommitUploadAsync(bool finishing, CancellationToken cancellationToken)
        {
            if ((_currentStream == null || _currentStream.Length < MIN_PART_LENGTH) && !finishing)
                return;

            if (_uploadId == null)
            {
                _uploadId = (await _s3.InitiateMultipartUploadAsync(new InitiateMultipartUploadRequest
                {
                    BucketName = _s3BucketName,
                    Key = _s3Key
                }, cancellationToken)).UploadId;
            }

            var currentStream = _currentStream;

            _currentStream = null;

            if (currentStream != null)
            {
                var partNumber = ++_partCount;

                currentStream.Seek(0, SeekOrigin.Begin);

                var s3 = _s3;
                var bucketName = _s3BucketName;
                var key = _s3Key;
                var uploadId = _uploadId;
                var partETags = _partETags;

                var upload = Task.Run(async () =>
                {
                    try
                    {
                        var response = await s3.UploadPartAsync(new UploadPartRequest
                        {
                            BucketName = bucketName,
                            Key = key,
                            UploadId = uploadId,
                            PartNumber = partNumber,
                            IsLastPart = finishing,
                            InputStream = currentStream
                        }, cancellationToken);

                        partETags.AddOrUpdate(partNumber, response.ETag, (n, s) => response.ETag);
                    }
                    finally
                    {
                        currentStream.Dispose();
                    }
                });

                _uploadTasks.Add(upload);
            }
        }

        private async Task CompleteUploadAsync(CancellationToken cancellationToken)
        {
            await Task.WhenAll(_uploadTasks.ToArray());

            if (this.Length > 0)
            {
                await _s3.CompleteMultipartUploadAsync(new CompleteMultipartUploadRequest
                {
                    BucketName = _s3BucketName,
                    Key = _s3Key,
                    PartETags = _partETags.Select(e => new PartETag(e.Key, e.Value)).ToList(),
                    UploadId = _uploadId
                }, cancellationToken);
            }
        }

        /// <summary>
        /// Causes any buffered data to be written and completes the multipart s3 upload.
        /// </summary>
        public override void Flush()
        {
            if (!_completed)
            {
                Task.Run(() => CommitUploadAsync(true, CancellationToken.None)).Wait();
                Task.Run(() => CompleteUploadAsync(CancellationToken.None)).Wait();

                _completed = true;
            }
        }

        /// <summary>
        /// Causes any buffered data to be written and completes the multipart s3 upload.
        /// </summary>
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            if (!_completed)
            {
                await CommitUploadAsync(true, cancellationToken);
                await CompleteUploadAsync(cancellationToken);

                _completed = true;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream" /> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Flush();
                }

                base.Dispose(disposing);

                _disposed = true;
            }
        }
    }
}
