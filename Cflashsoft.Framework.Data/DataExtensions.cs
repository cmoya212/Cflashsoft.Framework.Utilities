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
        /// <summary>
        /// Executes the CommandText against the Connection and builds an IDataReader.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>An IDataReader object.</returns>
        public static IDataReader ExecuteQuery(this IDbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
        {
            return ExecuteQuery(cn, commandText, CommandType.Text, parameters);
        }

        /// <summary>
        /// Executes the CommandText against the Connection and builds an IDataReader.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>An IDataReader object.</returns>
        public static IDataReader ExecuteQuery(this IDbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
        {
            return ExecuteQuery(cn, commandText, commandType, CommandBehavior.Default, parameters);
        }

        /// <summary>
        /// Executes the CommandText against the Connection and builds an IDataReader.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="behavior">One of the CommandBehavior values.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>An IDataReader object.</returns>
        public static IDataReader ExecuteQuery(this IDbConnection cn, string commandText, CommandType commandType, CommandBehavior behavior, params (string ParameterName, object Value)[] parameters)
        {
            using (var cmd = CreateCommand(cn, commandText, commandType, parameters))
            {
                OpenIfClosed(cn);
                return cmd.ExecuteReader(behavior);
            }
        }

        /// <summary>
        /// Executes the CommandText against the Connection and builds an IDataReader.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>An IDataReader object.</returns>
        public static Task<DbDataReader> ExecuteQueryAsync(this DbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
        {
            return ExecuteQueryAsync(cn, commandText, CommandType.Text, parameters);
        }

        /// <summary>
        /// Executes the CommandText against the Connection and builds an IDataReader.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>An IDataReader object.</returns>
        public static Task<DbDataReader> ExecuteQueryAsync(this DbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
        {
            return ExecuteQueryAsync(cn, commandText, commandType, CommandBehavior.Default, parameters);
        }

        /// <summary>
        /// Executes the CommandText against the Connection and builds an IDataReader.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="behavior">One of the CommandBehavior values.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>An IDataReader object.</returns>
        public static async Task<DbDataReader> ExecuteQueryAsync(this DbConnection cn, string commandText, CommandType commandType, CommandBehavior behavior, params (string ParameterName, object Value)[] parameters)
        {
            using (var cmd = CreateCommand(cn, commandText, commandType, parameters))
            {
                await OpenIfClosedAsync(cn);
                return await cmd.ExecuteReaderAsync(behavior);
            }
        }

        /// <summary>
        /// Executes the CommandText against the Connection and builds an IDataReader and provides a way for the DataReader to handle rows that contain columns with large binary values.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>An IDataReader object.</returns>
        public static IDataReader ExecuteSequential(this IDbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
        {
            return ExecuteQuery(cn, commandText, commandType, CommandBehavior.SingleResult | CommandBehavior.SingleRow | CommandBehavior.SequentialAccess, parameters);
        }

        /// <summary>
        /// Executes the CommandText against the Connection and builds an IDataReader and provides a way for the DataReader to handle rows that contain columns with large binary values.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>An IDataReader object.</returns>
        public static Task<DbDataReader> ExecuteSequentialAsync(this DbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
        {
            return ExecuteQueryAsync(cn, commandText, commandType, CommandBehavior.SingleResult | CommandBehavior.SingleRow | CommandBehavior.SequentialAccess, parameters);
        }

        /// <summary>
        /// Executes an SQL statement against the Connection object of a .NET data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this IDbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
        {
            return ExecuteNonQuery(cn, commandText, CommandType.Text, parameters);
        }

        /// <summary>
        /// Executes an SQL statement against the Connection object of a .NET data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this IDbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
        {
            using (var cmd = CreateCommand(cn, commandText, commandType, parameters))
            {
                OpenIfClosed(cn);
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Executes an SQL statement against the Connection object of a .NET data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>The number of rows affected.</returns>
        public static Task<int> ExecuteNonQueryAsync(this DbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
        {
            return ExecuteNonQueryAsync(cn, commandText, CommandType.Text, parameters);
        }

        /// <summary>
        /// Executes an SQL statement against the Connection object of a .NET data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>The number of rows affected.</returns>
        public static async Task<int> ExecuteNonQueryAsync(this DbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
        {
            using (var cmd = CreateCommand(cn, commandText, commandType, parameters))
            {
                await OpenIfClosedAsync(cn);
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this IDbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
        {
            return ExecuteScalar(cn, commandText, CommandType.Text, parameters);
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
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

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        public static Task<object> ExecuteScalarAsync(this DbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
        {
            return ExecuteScalarAsync(cn, commandText, CommandType.Text, parameters);
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
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

        /// <summary>
        /// Utility method to open a DbConnection connection if it is currently closed.
        /// </summary>
        /// <param name="cn">The database connection to open.</param>
        /// <returns>The same connection object for chaining.</returns>
        public static IDbConnection OpenIfClosed(this IDbConnection cn)
        {
            if (cn.State != ConnectionState.Open)
                cn.Open();

            return cn;
        }

        /// <summary>
        /// Utility method to open a DbConnection connection if it is currently closed.
        /// </summary>
        /// <param name="cn">The database connection to open.</param>
        /// <returns>The same connection object for chaining.</returns>
        public static async Task<DbConnection> OpenIfClosedAsync(this DbConnection cn)
        {
            if (cn.State != ConnectionState.Open)
                await cn.OpenAsync();

            return cn;
        }

        /// <summary>
        /// Utility method to open a DbConnection connection if it is currently closed.
        /// </summary>
        /// <param name="cmd">The command whose connection will be opened.</param>
        /// <returns>The same command object for chaining.</returns>
        public static IDbCommand OpenIfClosed(this IDbCommand cmd)
        {
            IDbConnection cn = cmd.Connection;

            if (cn.State != ConnectionState.Open)
                cn.Open();

            return cmd;
        }

        /// <summary>
        /// Utility method to open a DbConnection connection if it is currently closed.
        /// </summary>
        /// <param name="cmd">The command whose connection will be opened.</param>
        /// <returns>The same command object for chaining.</returns>
        public static async Task<DbCommand> OpenIfClosedAsync(this DbCommand cmd)
        {
            DbConnection cn = cmd.Connection;

            if (cn.State != ConnectionState.Open)
                await cn.OpenAsync();

            return cmd;
        }

        /// <summary>
        /// Utility method to create and configure a IDbCommand object in a single call.
        /// </summary>
        /// <param name="cn">The connection to be associated with the command.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A IDbCommand object.</returns>
        public static IDbCommand CreateCommand(this IDbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
        {
            return CreateCommand(cn, commandText, CommandType.Text, parameters);
        }

        /// <summary>
        /// Utility method to create and configure a IDbCommand object in a single call.
        /// </summary>
        /// <param name="cn">The connection to be associated with the command.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A IDbCommand object.</returns>
        public static IDbCommand CreateCommand(this IDbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
        {
            IDbCommand cmd = cn.CreateCommand();

            ConfigureCommand(cmd, commandText, commandType, parameters);

            return cmd;
        }

        /// <summary>
        /// Utility method to create and configure a IDbCommand object in a single call.
        /// </summary>
        /// <param name="cn">The connection to be associated with the command.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A IDbCommand object.</returns>
        public static DbCommand CreateCommand(this DbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
        {
            return CreateCommand(cn, commandText, CommandType.Text, parameters);
        }

        /// <summary>
        /// Utility method to create and configure a IDbCommand object in a single call.
        /// </summary>
        /// <param name="cn">The connection to be associated with the command.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A IDbCommand object.</returns>
        public static DbCommand CreateCommand(this DbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
        {
            DbCommand cmd = cn.CreateCommand();

            ConfigureCommand(cmd, commandText, commandType, parameters);

            return cmd;
        }

        /// <summary>
        /// Utility method to create and configure a IDbCommand object in a single call.
        /// </summary>
        /// <param name="cmd">The command object to configure.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A IDbCommand object.</returns>
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

        /// <summary>
        /// Utility method to add parameters to a IDbCommand object in a single call.
        /// </summary>
        /// <param name="cmd">The command object to add parameters to.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="paramType">The DbType of the parameter.</param>
        /// <param name="size">The size of the parameter.</param>
        /// <param name="precision">Indicates the precision of numeric parameters.</param>
        /// <param name="scale">Indicates the scale of numeric parameters.</param>
        /// <param name="direction">Indicates whether the parameter is input-only, output-only, bidirectional, or a stored procedure return value parameter</param>
        /// <param name="sourceColumn">Indicates the name of the source column that is mapped to the DataSet and used for loading or returning the Value.</param>
        /// <param name="sourceVersion">Indicates the DataRowVersion to use when loading Value.</param>
        /// <returns>A IDbDataParameter object.</returns>
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

        /// <summary>
        /// Returns the columns in an IDataReader as a KeyValuePair collection.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <returns>A KeyValuePair collection.</returns>
        public static IEnumerable<KeyValuePair<string, Type>> GetReaderColumns(this IDataReader reader)
        {
            List<KeyValuePair<string, Type>> result = new List<KeyValuePair<string, Type>>();

            for (int i = 0; i < reader.FieldCount; i++)
                result.Add(new KeyValuePair<string, Type>(reader.GetName(i), reader.GetFieldType(i)));

            return result;
        }

        /// <summary>
        /// Returns a row from an IDataReader as a Dictionary of column and value.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <returns>A Dictionary of column and value.</returns>
        public static Dictionary<string, object> GetReaderRow(this IDataReader reader)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            for (int i = 0; i < reader.FieldCount; i++)
                result.Add(reader.GetName(i), reader.IsDBNull(i) ? null : reader.GetValue(i));

            return result;
        }

        /// <summary>
        /// Enumerates an IDataReader and returns rows as Dictionary of column and value.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="closeReader">Indicates whether to close the connection when the enumeration completes.</param>
        /// <returns>Enumerable of dictionary column/value pairs.</returns>
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

        /// <summary>
        /// Enumerates an IDataReader and returns rows as custom objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <param name="closeReader">Indicates whether to close the connection when the enumeration completes.</param>
        /// <returns>Enumerable of T.</returns>
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

        /// <summary>
        /// Enumerates an IDataReader and returns rows as a List of Dictionary column/value items.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="closeReader">Indicates whether to close the connection when the enumeration completes.</param>
        /// <returns>A List of Dictionary column/value items.</returns>
        public static List<Dictionary<string, object>> ToList(this IDataReader reader, bool closeReader = true)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            try
            {
                while (reader.Read())
                    result.Add(GetReaderRow(reader));
                    //result.Add(Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName, reader.GetValue));
            }
            finally
            {
                if (closeReader)
                    reader.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Enumerates an IDataReader and returns rows as custom concrete or anonymous objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <param name="closeReader">Indicates whether to close the connection when the enumeration completes.</param>
        /// <returns>A List of custom concrete or anonymous objects.</returns>
        /// <example><![CDATA[ ToList((reader) => new
        /// {
        ///	Field1 = reader.GetNullableString(0), //by key works too
        ///	Field2 = reader.GetNullableInt32(1), //by key works too
        /// });
        /// ]]>
        /// </example>
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

        /// <summary>
        /// Enumerates an IDataReader and returns rows as a List of Dictionary column/value items.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="closeReader">Indicates whether to close the connection when the enumeration completes.</param>
        /// <returns>A List of Dictionary column/value items.</returns>
        public static async Task<List<Dictionary<string, object>>> ToListAsync(this DbDataReader reader, bool closeReader = true)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            try
            {
                while (await reader.ReadAsync())
                    result.Add(GetReaderRow(reader));
                    //result.Add(Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName, reader.GetValue));
            }
            finally
            {
                if (closeReader)
                    reader.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Enumerates an IDataReader and returns rows as custom concrete or anonymous objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <param name="closeReader">Indicates whether to close the connection when the enumeration completes.</param>
        /// <returns>A List of custom concrete or anonymous objects.</returns>
        /// <example><![CDATA[ ToListAsync((reader) => new
        /// {
        ///	Field1 = reader.GetNullableString(0), //by key works too
        ///	Field2 = reader.GetNullableInt32(1), //by key works too
        /// });
        /// ]]>
        /// </example>
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

        /// <summary>
        /// Enumerates an IDataReader and returns the first row as a Dictionary of column/value items.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="closeReader">Indicates whether to close the connection when the enumeration completes.</param>
        /// <returns>A Dictionary of column/value items.</returns>
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

        /// <summary>
        /// Enumerates an IDataReader and returns the first row as a custom concrete or anonymous object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <param name="closeReader">Indicates whether to close the connection when the enumeration completes.</param>
        /// <returns>A custom concrete or anonymous object.</returns>
        /// <example><![CDATA[ FirstOrDefault((reader) => new
        /// {
        ///	Field1 = reader.GetNullableString(0), //by key works too
        ///	Field2 = reader.GetNullableInt32(1), //by key works too
        /// });
        /// 
        /// See also NullableFirstOrDefault<T>
        /// ]]>
        /// </example>
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

        /// <summary>
        /// Enumerates an IDataReader and returns the first row as a custom concrete or anonymous object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <param name="closeReader">Indicates whether to close the connection when the enumeration completes.</param>
        /// <returns>A custom concrete or anonymous object.</returns>
        /// <example><![CDATA[ NullableFirstOrDefault((reader) => new
        /// {
        ///	Field1 = reader.GetNullableString(0), //by key works too
        ///	Field2 = reader.GetNullableInt32(1), //by key works too
        /// });
        /// ]]>
        /// </example>
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

        /// <summary>
        /// Enumerates an IDataReader and returns the first row as a Dictionary of column/value items.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="closeReader">Indicates whether to close the connection when the enumeration completes.</param>
        /// <returns>A Dictionary of column/value items.</returns>
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

        /// <summary>
        /// Enumerates an IDataReader and returns the first row as a custom concrete or anonymous object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <param name="closeReader">Indicates whether to close the connection when the enumeration completes.</param>
        /// <returns>A custom concrete or anonymous object.</returns>
        /// <example><![CDATA[ FirstOrDefaultAsync((reader) => new
        /// {
        ///	Field1 = reader.GetNullableString(0), //by key works too
        ///	Field2 = reader.GetNullableInt32(1), //by key works too
        /// });
        /// 
        /// See also NullableFirstOrDefaultAsync<T>
        /// ]]>
        /// </example>
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

        /// <summary>
        /// Enumerates an IDataReader and returns the first row as a custom concrete or anonymous object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <param name="closeReader">Indicates whether to close the connection when the enumeration completes.</param>
        /// <returns>A custom concrete or anonymous object.</returns>
        /// <example><![CDATA[ NullableFirstOrDefaultAAsync((reader) => new
        /// {
        ///	Field1 = reader.GetNullableString(0), //by key works too
        ///	Field2 = reader.GetNullableInt32(1), //by key works too
        /// });
        /// ]]>
        /// </example>
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

        /// <summary>
        /// Enumerates an IDataReader and returns a DataTable.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="closeReader">Indicates whether to close the connection when the enumeration completes.</param>
        /// <param name="useDataTableLoad">If true, uses the classic ADO.NET DataTable.Load() function.</param>
        /// <returns>A DataTable.</returns>
        public static DataTable ToDataTable(this IDataReader reader, bool closeReader = true, bool useDataTableLoad = true)
        {
            DataTable result = new DataTable();

            try
            {
                if (useDataTableLoad)
                {
                    result.Load(reader);
                }
                else
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                        result.Columns.Add(reader.GetName(i), reader.GetFieldType(i));

                    while (reader.Read())
                    {
                        DataRow row = result.NewRow();

                        for (int i = 0; i < reader.FieldCount; i++)
                            row[i] = reader.GetValue(i);

                        result.Rows.Add(row);
                    }
                }
            }
            finally
            {
                if (closeReader)
                    reader.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Enumerates an IDataReader and returns a DataTable.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="closeReader">Indicates whether to close the connection when the enumeration completes.</param>
        /// <returns>A DataTable.</returns>
        public static async Task<DataTable> ToDataTableAsync(this DbDataReader reader, bool closeReader = true)
        {
            DataTable result = new DataTable();

            try
            {
                for (int i = 0; i < reader.FieldCount; i++)
                    result.Columns.Add(reader.GetName(i), reader.GetFieldType(i));

                while (await reader.ReadAsync())
                {
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

        /// <summary>
        /// Gets the string value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The string value of the specified field.</returns>
        public static string GetNullableString(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? null : reader.GetString(i);
        }

        /// <summary>
        /// Gets the bool value of the specified column if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>A boolean.</returns>
        public static bool? GetNullableBoolean(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? (bool?)null : reader.GetBoolean(i);
        }

        /// <summary>
        /// Gets the 8-bit unsigned integer value of the specified column if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The 8-bit unsigned integer value of the specified column.</returns>
        public static byte? GetNullableByte(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? (byte?)null : reader.GetByte(i);
        }

        /// <summary>
        /// Gets the character value of the specified column if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The character value of the specified column.</returns>
        public static char? GetNullableChar(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? (char?)null : reader.GetChar(i);
        }

        /// <summary>
        /// Gets the date and time data value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The date and time data value of the specified field.</returns>
        public static DateTime? GetNullableDateTime(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? (DateTime?)null : reader.GetDateTime(i);
        }

        /// <summary>
        /// Gets the fixed-position numeric value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The fixed-position numeric value of the specified field.</returns>
        public static decimal? GetNullableDecimal(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? (decimal?)null : reader.GetDecimal(i);
        }

        /// <summary>
        /// Gets the double-precision floating point number of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The double-precision floating point number of the specified field.</returns>
        public static double? GetNullableDouble(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? (double?)null : reader.GetDouble(i);
        }

        /// <summary>
        /// Gets the single-precision floating point number of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The single-precision floating point number of the specified field.</returns>
        public static float? GetNullableFloat(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? (float?)null : reader.GetFloat(i);
        }

        /// <summary>
        /// Returns the GUID value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The GUID value of the specified field.</returns>
        public static Guid? GetNullableGuid(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? (Guid?)null : reader.GetGuid(i);
        }

        /// <summary>
        /// Gets the 16-bit signed integer value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The 16-bit signed integer value of the specified field.</returns>
        public static short? GetNullableInt16(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? (short?)null : reader.GetInt16(i);
        }

        /// <summary>
        /// Gets the 32-bit signed integer value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The 32-bit signed integer value of the specified field.</returns>
        public static int? GetNullableInt32(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? (int?)null : reader.GetInt32(i);
        }

        /// <summary>
        /// Gets the 64-bit signed integer value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The 64-bit signed integer value of the specified field.</returns>
        public static long? GetNullableInt64(this IDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? (long?)null : reader.GetInt64(i);
        }

        /// <summary>
        /// Gets the string value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The string value of the specified field.</returns>
        public static string GetNullableString(this IDataReader reader, string name)
        {
            return GetNullableString(reader, reader.GetOrdinal(name));
        }

        /// <summary>
        /// Gets the bool value of the specified column if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>A boolean.</returns>
        public static bool? GetNullableBoolean(this IDataReader reader, string name)
        {
            return GetNullableBoolean(reader, reader.GetOrdinal(name));
        }

        /// <summary>
        /// Gets the 8-bit unsigned integer value of the specified column if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The 8-bit unsigned integer value of the specified column.</returns>
        public static byte? GetNullableByte(this IDataReader reader, string name)
        {
            return GetNullableByte(reader, reader.GetOrdinal(name));
        }

        /// <summary>
        /// Gets the character value of the specified column if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The character value of the specified column.</returns>
        public static char? GetNullableChar(this IDataReader reader, string name)
        {
            return GetNullableChar(reader, reader.GetOrdinal(name));
        }

        /// <summary>
        /// Gets the date and time data value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The date and time data value of the specified field.</returns>
        public static DateTime? GetNullableDateTime(this IDataReader reader, string name)
        {
            return GetNullableDateTime(reader, reader.GetOrdinal(name));
        }

        /// <summary>
        /// Gets the fixed-position numeric value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The fixed-position numeric value of the specified field.</returns>
        public static decimal? GetNullableDecimal(this IDataReader reader, string name)
        {
            return GetNullableDecimal(reader, reader.GetOrdinal(name));
        }

        /// <summary>
        /// Gets the double-precision floating point number of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The double-precision floating point number of the specified field.</returns>
        public static double? GetNullableDouble(this IDataReader reader, string name)
        {
            return GetNullableDouble(reader, reader.GetOrdinal(name));
        }

        /// <summary>
        /// Gets the single-precision floating point number of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The single-precision floating point number of the specified field.</returns>
        public static float? GetNullableFloat(this IDataReader reader, string name)
        {
            return GetNullableFloat(reader, reader.GetOrdinal(name));
        }

        /// <summary>
        /// Returns the GUID value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The GUID value of the specified field.</returns>
        public static Guid? GetNullableGuid(this IDataReader reader, string name)
        {
            return GetNullableGuid(reader, reader.GetOrdinal(name));
        }

        /// <summary>
        /// Gets the 16-bit signed integer value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The 16-bit signed integer value of the specified field.</returns>
        public static short? GetNullableInt16(this IDataReader reader, string name)
        {
            return GetNullableInt16(reader, reader.GetOrdinal(name));
        }

        /// <summary>
        /// Gets the 32-bit signed integer value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The 32-bit signed integer value of the specified field.</returns>
        public static int? GetNullableInt32(this IDataReader reader, string name)
        {
            return GetNullableInt32(reader, reader.GetOrdinal(name));
        }

        /// <summary>
        /// Gets the 64-bit signed integer value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The 64-bit signed integer value of the specified field.</returns>
        public static long? GetNullableInt64(this IDataReader reader, string name)
        {
            return GetNullableInt64(reader, reader.GetOrdinal(name));
        }

        /// <summary>
        /// Gets the string value of the specified field.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The string value of the specified field.</returns>
        public static string GetString(this IDataReader reader, string name)
        {
            return reader.GetString(reader.GetOrdinal(name));
        }

        /// <summary>
        /// Gets the bool value of the specified column.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>A boolean.</returns>
        public static bool GetBoolean(this IDataReader reader, string name)
        {
            return reader.GetBoolean(reader.GetOrdinal(name));
        }

        /// <summary>
        /// Gets the 8-bit unsigned integer value of the specified column.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The 8-bit unsigned integer value of the specified column.</returns>
        public static byte GetByte(this IDataReader reader, string name)
        {
            return reader.GetByte(reader.GetOrdinal(name));
        }

        /// <summary>
        /// Gets the character value of the specified column.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The character value of the specified column.</returns>
        public static char GetChar(this IDataReader reader, string name)
        {
            return reader.GetChar(reader.GetOrdinal(name));
        }

        /// <summary>
        /// Gets the date and time data value of the specified field.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The date and time data value of the specified field.</returns>
        public static DateTime GetDateTime(this IDataReader reader, string name)
        {
            return reader.GetDateTime(reader.GetOrdinal(name));
        }

        /// <summary>
        /// Gets the fixed-position numeric value of the specified field.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The fixed-position numeric value of the specified field.</returns>
        public static decimal GetDecimal(this IDataReader reader, string name)
        {
            return reader.GetDecimal(reader.GetOrdinal(name));
        }

        /// <summary>
        /// Gets the double-precision floating point number of the specified field.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The double-precision floating point number of the specified field.</returns>
        public static double GetDouble(this IDataReader reader, string name)
        {
            return reader.GetDouble(reader.GetOrdinal(name));
        }

        /// <summary>
        /// Gets the single-precision floating point number of the specified field.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The single-precision floating point number of the specified field.</returns>
        public static float GetFloat(this IDataReader reader, string name)
        {
            return reader.GetFloat(reader.GetOrdinal(name));
        }

        /// <summary>
        /// Returns the GUID value of the specified field.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The GUID value of the specified field.</returns>
        public static Guid GetGuid(this IDataReader reader, string name)
        {
            return reader.GetGuid(reader.GetOrdinal(name));
        }

        /// <summary>
        /// Gets the 16-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The 16-bit signed integer value of the specified field.</returns>
        public static short GetInt16(this IDataReader reader, string name)
        {
            return reader.GetInt16(reader.GetOrdinal(name));
        }

        /// <summary>
        /// Gets the 32-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The 32-bit signed integer value of the specified field.</returns>
        public static int GetInt32(this IDataReader reader, string name)
        {
            return reader.GetInt32(reader.GetOrdinal(name));
        }

        /// <summary>
        /// Gets the 64-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The 64-bit signed integer value of the specified field.</returns>
        public static long GetInt64(this IDataReader reader, string name)
        {
            return reader.GetInt64(reader.GetOrdinal(name));
        }
    }
}