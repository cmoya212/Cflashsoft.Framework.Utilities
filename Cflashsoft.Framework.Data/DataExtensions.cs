using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Cflashsoft.Framework.Data
{
    /// <summary>
    /// Extensions for ADO.NET to simplify data access.
    /// </summary>
    public static class DataExtensions
    {
        public static IDataReader ExecuteQuery(this IDbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
        {
            return ExecuteQuery(cn, commandText, CommandType.Text, parameters);
        }

        public static IDataReader ExecuteQuery(this IDbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
        {
            return ExecuteQuery(cn, commandText, commandType, CommandBehavior.Default, parameters);
        }

        public static IDataReader ExecuteQuery(this IDbConnection cn, string commandText, CommandType commandType, CommandBehavior behavior, params (string ParameterName, object Value)[] parameters)
        {
            using (var cmd = CreateCommand(cn, commandText, commandType, parameters))
            {
                OpenIfClosed(cn);
                return cmd.ExecuteReader(behavior);
            }
        }

        public static Task<DbDataReader> ExecuteQueryAsync(this DbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
        {
            return ExecuteQueryAsync(cn, commandText, CommandType.Text, parameters);
        }

        public static Task<DbDataReader> ExecuteQueryAsync(this DbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
        {
            return ExecuteQueryAsync(cn, commandText, commandType, CommandBehavior.Default, parameters);
        }

        public static async Task<DbDataReader> ExecuteQueryAsync(this DbConnection cn, string commandText, CommandType commandType, CommandBehavior behavior, params (string ParameterName, object Value)[] parameters)
        {
            using (var cmd = CreateCommand(cn, commandText, commandType, parameters))
            {
                await OpenIfClosedAsync(cn);
                return await cmd.ExecuteReaderAsync(behavior);
            }
        }

        public static IDataReader ExecuteSequential(this IDbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
        {
            return ExecuteQuery(cn, commandText, commandType, CommandBehavior.SingleResult | CommandBehavior.SingleRow | CommandBehavior.SequentialAccess, parameters);
        }

        public static Task<DbDataReader> ExecuteSequentialAsync(this DbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
        {
            return ExecuteQueryAsync(cn, commandText, commandType, CommandBehavior.SingleResult | CommandBehavior.SingleRow | CommandBehavior.SequentialAccess, parameters);
        }

        public static int ExecuteNonQuery(this IDbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
        {
            return ExecuteNonQuery(cn, commandText, CommandType.Text, parameters);
        }

        public static int ExecuteNonQuery(this IDbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
        {
            using (var cmd = CreateCommand(cn, commandText, commandType, parameters))
            {
                OpenIfClosed(cn);
                return cmd.ExecuteNonQuery();
            }
        }

        public static Task<int> ExecuteNonQueryAsync(this DbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
        {
            return ExecuteNonQueryAsync(cn, commandText, CommandType.Text, parameters);
        }

        public static async Task<int> ExecuteNonQueryAsync(this DbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
        {
            using (var cmd = CreateCommand(cn, commandText, commandType, parameters))
            {
                await OpenIfClosedAsync(cn);
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public static object ExecuteScalar(this IDbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
        {
            return ExecuteScalar(cn, commandText, CommandType.Text, parameters);
        }

        public static object ExecuteScalar(this IDbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
        {
            using (var cmd = CreateCommand(cn, commandText, commandType, parameters))
            {
                OpenIfClosed(cn);

                object result = cmd.ExecuteScalar();

                if (result is DBNull)
                    return null;
                else
                    return result;
            }
        }

        public static Task<object> ExecuteScalarAsync(this DbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
        {
            return ExecuteScalarAsync(cn, commandText, CommandType.Text, parameters);
        }

        public static async Task<object> ExecuteScalarAsync(this DbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
        {
            using (var cmd = CreateCommand(cn, commandText, commandType, parameters))
            {
                await OpenIfClosedAsync(cn);

                object result = await cmd.ExecuteScalarAsync();

                if (result is DBNull)
                    return null;
                else
                    return result;
            }
        }

        public static IDbConnection OpenIfClosed(this IDbConnection cn)
        {
            if (cn.State != ConnectionState.Open)
                cn.Open();

            return cn;
        }

        public static async Task<DbConnection> OpenIfClosedAsync(this DbConnection cn)
        {
            if (cn.State != ConnectionState.Open)
                await cn.OpenAsync();

            return cn;
        }

        public static IDbCommand OpenIfClosed(this IDbCommand cmd)
        {
            OpenIfClosed(cmd.Connection);

            return cmd;
        }

        public static async Task<DbCommand> OpenIfClosedAsync(this DbCommand cmd)
        {
            await OpenIfClosedAsync(cmd.Connection);

            return cmd;
        }

        public static IDbCommand CreateCommand(this IDbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
        {
            return CreateCommand(cn, commandText, CommandType.Text, parameters);
        }

        public static IDbCommand CreateCommand(this IDbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
        {
            IDbCommand cmd = cn.CreateCommand();

            ConfigureCommand(cmd, commandText, commandType, parameters);

            return cmd;
        }

        public static DbCommand CreateCommand(this DbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
        {
            return CreateCommand(cn, commandText, CommandType.Text, parameters);
        }

        public static DbCommand CreateCommand(this DbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
        {
            DbCommand cmd = cn.CreateCommand();

            ConfigureCommand(cmd, commandText, commandType, parameters);

            return cmd;
        }

        public static IDbCommand ConfigureCommand(this IDbCommand cmd, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
        {
            cmd.CommandText = commandText;
            cmd.CommandType = commandType;

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    AddParameter(cmd, parameter.ParameterName, parameter.Value);
                }
            }

            return cmd;
        }

        public static IDbDataParameter AddParameter(this IDbCommand cmd, string parameterName, object value, DbType? paramType = null, int? size = null, byte? precision = null, byte? scale = null, ParameterDirection? direction = null, string sourceColumn = null, DataRowVersion? sourceVersion = null)
        {
            IDbDataParameter param = cmd.CreateParameter();
            param.ParameterName = parameterName;
            param.Value = value ?? DBNull.Value;
            if (paramType.HasValue)
                param.DbType = paramType.Value;
            if (size.HasValue)
                param.Size = size.Value;
            if (precision.HasValue)
                param.Precision = precision.Value;
            if (scale.HasValue)
                param.Scale = scale.Value;
            if (direction.HasValue)
                param.Direction = direction.Value;
            if (sourceColumn != null)
                param.SourceColumn = sourceColumn;
            if (sourceVersion.HasValue)
                param.SourceVersion = sourceVersion.Value;
            cmd.Parameters.Add(param);
            return param;
        }

        public static IEnumerable<KeyValuePair<string, Type>> GetReaderColumns(this IDataReader reader)
        {
            List<KeyValuePair<string, Type>> result = new List<KeyValuePair<string, Type>>();

            for (int i = 0; i < reader.FieldCount; i++)
                result.Add(new KeyValuePair<string, Type>(reader.GetName(i), reader.GetFieldType(i)));

            return result;
        }

        public static Dictionary<string, object> GetReaderRow(this IDataReader reader)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            for (int i = 0; i < reader.FieldCount; i++)
                result.Add(reader.GetName(i), reader.IsDBNull(i) ? null : reader.GetValue(i));

            return result;
        }

        public static IEnumerable<Dictionary<string, object>> AsEnumerable(this IDataReader reader, bool closeReader = true)
        {
            try
            {
                while (reader.Read())
                    yield return GetReaderRow(reader);
                //    yield return Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName, reader.GetValue);
            }
            finally
            {
                if (closeReader)
                    reader.Dispose();
            }
        }

        public static IEnumerable<T> AsEnumerable<T>(this IDataReader reader, Func<IDataReader, T> selector, bool closeReader = true)
        {
            try
            {
                while (reader.Read())
                    yield return selector(reader);
            }
            finally
            {
                if (closeReader)
                    reader.Dispose();
            }
        }

        public static List<Dictionary<string, object>> ToList(this IDataReader reader, bool closeReader = true)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            try
            {
                while (reader.Read())
                    result.Add(GetReaderRow(reader));
                //    result.Add(Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName, reader.GetValue));
            }
            finally
            {
                if (closeReader)
                    reader.Dispose();
            }

            return result;
        }

        public static List<T> ToList<T>(this IDataReader reader, Func<IDataReader, T> selector, bool closeReader = true)
        {
            List<T> result = new List<T>();

            try
            {
                while (reader.Read())
                    result.Add(selector(reader));
            }
            finally
            {
                if (closeReader)
                    reader.Dispose();
            }

            return result;
        }

        public static async Task<List<Dictionary<string, object>>> ToListAsync(this DbDataReader reader, bool closeReader = true)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            try
            {
                while (await reader.ReadAsync())
                    result.Add(GetReaderRow(reader));
                //    result.Add(Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName, reader.GetValue));
            }
            finally
            {
                if (closeReader)
                    reader.Dispose();
            }

            return result;
        }

        public static async Task<List<T>> ToListAsync<T>(this DbDataReader reader, Func<IDataReader, T> selector, bool closeReader = true)
        {
            List<T> result = new List<T>();

            try
            {
                while (await reader.ReadAsync())
                    result.Add(selector(reader));
            }
            finally
            {
                if (closeReader)
                    reader.Dispose();
            }

            return result;
        }

        public static Dictionary<string, object> FirstOrDefault(this IDataReader reader, bool closeReader = true)
        {
            Dictionary<string, object> result = null;

            try
            {
                if (reader.Read())
                    result = GetReaderRow(reader);
            }
            finally
            {
                if (closeReader)
                    reader.Dispose();
            }

            return result;
        }

        public static T FirstOrDefault<T>(this IDataReader reader, Func<IDataReader, T> selector, bool closeReader = true) 
        {
            try
            {
                if (reader.Read())
                    return selector(reader);
            }
            finally
            {
                if (closeReader)
                    reader.Dispose();
            }

            return default(T);
        }

        public static T? NullableFirstOrDefault<T>(this IDataReader reader, Func<IDataReader, T> selector, bool closeReader = true) where T : struct
        {
            try
            {
                if (reader.Read())
                    return selector(reader);
            }
            finally
            {
                if (closeReader)
                    reader.Dispose();
            }

            return null;
        }

        public static async Task<Dictionary<string, object>> FirstOrDefaultAsync(this DbDataReader reader, bool closeReader = true)
        {
            Dictionary<string, object> result = null;

            try
            {
                if (await reader.ReadAsync())
                    result = GetReaderRow(reader);
            }
            finally
            {
                if (closeReader)
                    reader.Dispose();
            }

            return result;
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this DbDataReader reader, Func<IDataReader, T> selector, bool closeReader = true)
        {
            try
            {
                if (await reader.ReadAsync())
                    return selector(reader);
            }
            finally
            {
                if (closeReader)
                    reader.Dispose();
            }

            return default(T);
        }

        public static async Task<T?> NullableFirstOrDefaultAsync<T>(this DbDataReader reader, Func<IDataReader, T> selector, bool closeReader = true) where T : struct
        {
            try
            {
                if (await reader.ReadAsync())
                    return selector(reader);
            }
            finally
            {
                if (closeReader)
                    reader.Dispose();
            }

            return null;
        }

        public static DataTable ToDataTable(this IDataReader reader, bool closeReader = true)
        {
            DataTable result = new DataTable();

            try
            {
                result.Load(reader);
            }
            finally
            {
                if (closeReader)
                    reader.Dispose();
            }

            return result;
        }

        public static async Task<DataTable> ToDataTableAsync(this DbDataReader reader, bool closeReader = true)
        {
            DataTable result = new DataTable();

            try
            {
                //bool headerLoaded = false;

                for (int i = 0; i < reader.FieldCount; i++)
                    result.Columns.Add(reader.GetName(i), reader.GetFieldType(i));

                while (await reader.ReadAsync())
                {
                    //if (!headerLoaded)
                    //{
                    //    for (int i = 0; i < reader.FieldCount; i++)
                    //        result.Columns.Add(reader.GetName(i), reader.GetFieldType(i));

                    //    headerLoaded = true;
                    //}

                    DataRow row = result.NewRow();

                    for (int i = 0; i < reader.FieldCount; i++)
                        row[i] = reader.GetValue(i);

                    result.Rows.Add(row);
                }
            }
            finally
            {
                if (closeReader)
                    reader.Dispose();
            }

            return result;
        }

        public static string GetNullableString(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? null : reader.GetString(i);
        }

        public static bool? GetNullableBoolean(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? (bool?)null : reader.GetBoolean(i);
        }

        public static byte? GetNullableByte(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? (byte?)null : reader.GetByte(i);
        }

        public static char? GetNullableChar(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? (char?)null : reader.GetChar(i);
        }

        public static DateTime? GetNullableDateTime(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? (DateTime?)null : reader.GetDateTime(i);
        }

        public static decimal? GetNullableDecimal(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? (decimal?)null : reader.GetDecimal(i);
        }

        public static double? GetNullableDouble(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? (double?)null : reader.GetDouble(i);
        }

        public static float? GetNullableFloat(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? (float?)null : reader.GetFloat(i);
        }

        public static Guid? GetNullableGuid(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? (Guid?)null : reader.GetGuid(i);
        }

        public static short? GetNullableInt16(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? (short?)null : reader.GetInt16(i);
        }

        public static int? GetNullableInt32(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? (int?)null : reader.GetInt32(i);
        }

        public static long? GetNullableInt64(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? (long?)null : reader.GetInt64(i);
        }

        public static string GetNullableString(this IDataReader reader, string name)
        {
            return GetNullableString(reader, reader.GetOrdinal(name));
        }

        public static bool? GetNullableBoolean(this IDataReader reader, string name)
        {
            return GetNullableBoolean(reader, reader.GetOrdinal(name));
        }

        public static byte? GetNullableByte(this IDataReader reader, string name)
        {
            return GetNullableByte(reader, reader.GetOrdinal(name));
        }

        public static char? GetNullableChar(this IDataReader reader, string name)
        {
            return GetNullableChar(reader, reader.GetOrdinal(name));
        }

        public static DateTime? GetNullableDateTime(this IDataReader reader, string name)
        {
            return GetNullableDateTime(reader, reader.GetOrdinal(name));
        }

        public static decimal? GetNullableDecimal(this IDataReader reader, string name)
        {
            return GetNullableDecimal(reader, reader.GetOrdinal(name));
        }

        public static double? GetNullableDouble(this IDataReader reader, string name)
        {
            return GetNullableDouble(reader, reader.GetOrdinal(name));
        }

        public static float? GetNullableFloat(this IDataReader reader, string name)
        {
            return GetNullableFloat(reader, reader.GetOrdinal(name));
        }

        public static Guid? GetNullableGuid(this IDataReader reader, string name)
        {
            return GetNullableGuid(reader, reader.GetOrdinal(name));
        }

        public static short? GetNullableInt16(this IDataReader reader, string name)
        {
            return GetNullableInt16(reader, reader.GetOrdinal(name));
        }

        public static int? GetNullableInt32(this IDataReader reader, string name)
        {
            return GetNullableInt32(reader, reader.GetOrdinal(name));
        }

        public static long? GetNullableInt64(this IDataReader reader, string name)
        {
            return GetNullableInt64(reader, reader.GetOrdinal(name));
        }

        public static string GetString(this IDataReader reader, string name)
        {
            return reader.GetString(reader.GetOrdinal(name));
        }

        public static bool GetBoolean(this IDataReader reader, string name)
        {
            return reader.GetBoolean(reader.GetOrdinal(name));
        }

        public static byte GetByte(this IDataReader reader, string name)
        {
            return reader.GetByte(reader.GetOrdinal(name));
        }

        public static char GetChar(this IDataReader reader, string name)
        {
            return reader.GetChar(reader.GetOrdinal(name));
        }

        public static DateTime GetDateTime(this IDataReader reader, string name)
        {
            return reader.GetDateTime(reader.GetOrdinal(name));
        }

        public static decimal GetDecimal(this IDataReader reader, string name)
        {
            return reader.GetDecimal(reader.GetOrdinal(name));
        }

        public static double GetDouble(this IDataReader reader, string name)
        {
            return reader.GetDouble(reader.GetOrdinal(name));
        }

        public static float GetFloat(this IDataReader reader, string name)
        {
            return reader.GetFloat(reader.GetOrdinal(name));
        }

        public static Guid GetGuid(this IDataReader reader, string name)
        {
            return reader.GetGuid(reader.GetOrdinal(name));
        }

        public static short GetInt16(this IDataReader reader, string name)
        {
            return reader.GetInt16(reader.GetOrdinal(name));
        }

        public static int GetInt32(this IDataReader reader, string name)
        {
            return reader.GetInt32(reader.GetOrdinal(name));
        }

        public static long GetInt64(this IDataReader reader, string name)
        {
            return reader.GetInt64(reader.GetOrdinal(name));
        }
    }
}