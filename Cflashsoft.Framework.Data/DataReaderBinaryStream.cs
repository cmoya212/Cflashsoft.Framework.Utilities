using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cflashsoft.Framework.Data
{
    /// <summary>
    /// Exposes a forward-only stream whose source is a DB binary column for use with FileStreamResult ActionResult in ASP.NET MVC.
    /// </summary>
    /// <remarks>
    /// For use with FileStreamResult ActionResult in ASP.NET MVC.
    /// Adapted from http://www.codeproject.com/Articles/140713/Download-and-Upload-Images-from-SQL-Server-via-ASP 
    /// </remarks>
    public class DataReaderBinaryStream : Stream
    {
        private bool _disposed = false;

        private IDataReader _reader = null;
        private DbDataReader _dbReader = null;
        private Stream _stream = null;
        private int _columnIndex = 0;
        private long _position = 0;
        private string _mimeType = null;

        /// <summary>
        /// Returns the mime type of the file represented by this stream.
        /// </summary>
        public string MimeType => _mimeType;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead => true;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public override long Length => throw new NotImplementedException();

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
        /// Inititializes a new instance of the DataReaderBinaryStream class.
        /// </summary>
        /// <param name="reader">The DataReader that contains the data.</param>
        /// <param name="binaryColumnIndex">The column position that contains the byte data.</param>
        public DataReaderBinaryStream(IDataReader reader, int binaryColumnIndex)
            : this(reader, binaryColumnIndex, "application/octet-stream")
        {

        }

        /// <summary>
        /// Inititializes a new instance of the DataReaderBinaryStream class.
        /// </summary>
        /// <param name="reader">The DataReader that contains the data.</param>
        /// <param name="binaryColumnIndex">The column position that contains the byte data.</param>
        /// <param name="mimeType">The mime type of the file contained in the byte data.</param>
        public DataReaderBinaryStream(IDataReader reader, int binaryColumnIndex, string mimeType)
        {
            _reader = reader;
            _dbReader = reader as DbDataReader;
            _columnIndex = binaryColumnIndex;
            _mimeType = mimeType;
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            //do nothing 
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

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
        /// Not supported. When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="NotImplementedException"></exception>
        public override void SetLength(long value) => throw new NotImplementedException();

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            long bytesRead = _reader.GetBytes(_columnIndex, _position, buffer, offset, count);

            _position += bytesRead;

            return (int)bytesRead;
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <exception cref="InvalidOperationException">The underlying DataReader does not support this function.</exception>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (_dbReader != null)
            {
                if (_stream == null)
                    _stream = _dbReader.GetStream(_columnIndex);

                int bytesRead = await _stream.ReadAsync(buffer, offset, count, cancellationToken);

                _position += bytesRead;

                return bytesRead;
            }
            else
            {
                return Read(buffer, offset, count);
            }
        }
        /// <summary>
        /// Not supported. When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

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
                    Stream stream = _stream;

                    if (stream != null)
                    {
                        stream.Dispose();
                        _stream = null;
                    }

                    IDataReader reader = _reader;

                    if (reader != null)
                    {
                        reader.Dispose();
                        _reader = null;
                        _dbReader = null;
                    }
                }

                base.Dispose(disposing);

                _disposed = true;
            }
        }
    }
}
