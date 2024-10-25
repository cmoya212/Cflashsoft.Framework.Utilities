using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
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
            => ExecuteQuery(cn, commandText, CommandType.Text, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and builds an IDataReader.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>An IDataReader object.</returns>
        public static IDataReader ExecuteQuery(this IDbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
            => ExecuteQuery(cn, commandText, commandType, CommandBehavior.Default, parameters);

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
            => ExecuteQuery(cn, commandText, commandType, behavior,(int?)null, (IDbTransaction)null, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and builds an IDataReader.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="behavior">One of the CommandBehavior values.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>An IDataReader object.</returns>
        public static IDataReader ExecuteQuery(this IDbConnection cn, string commandText, CommandType commandType, CommandBehavior behavior, int? commandTimeout, IDbTransaction trx, params (string ParameterName, object Value)[] parameters)
        {
            using (var cmd = CreateCommand(cn, commandText, commandType, commandTimeout, trx, parameters))
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
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="behavior">One of the CommandBehavior values.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <returns>An IDataReader object.</returns>
        public static IDataReader ExecuteQuery(this IDbConnection cn, string commandText, CommandType commandType, CommandBehavior behavior, int? commandTimeout, IDbTransaction trx, IEnumerable<IDbDataParameter> parameters)
        {
            using (var cmd = CreateCommand(cn, commandText, commandType, commandTimeout, trx, parameters))
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
            => ExecuteQueryAsync(cn, commandText, CommandType.Text, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and builds an IDataReader.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>An IDataReader object.</returns>
        public static Task<DbDataReader> ExecuteQueryAsync(this DbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
            => ExecuteQueryAsync(cn, commandText, commandType, CommandBehavior.Default, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and builds an IDataReader.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="behavior">One of the CommandBehavior values.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>An IDataReader object.</returns>
        public static Task<DbDataReader> ExecuteQueryAsync(this DbConnection cn, string commandText, CommandType commandType, CommandBehavior behavior, params (string ParameterName, object Value)[] parameters)
            => ExecuteQueryAsync(cn, commandText, commandType, behavior, null, (DbTransaction)null, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and builds an IDataReader.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="behavior">One of the CommandBehavior values.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <returns>An IDataReader object.</returns>
        public static async Task<DbDataReader> ExecuteQueryAsync(this DbConnection cn, string commandText, CommandType commandType, CommandBehavior behavior, int? commandTimeout, DbTransaction trx, params (string ParameterName, object Value)[] parameters)
        {
            using (var cmd = CreateCommand(cn, commandText, commandType, commandTimeout, trx, parameters))
            {
                await OpenIfClosedAsync(cn);
                return await cmd.ExecuteReaderAsync(behavior);
            }
        }

        /// <summary>
        /// Executes the CommandText against the Connection and builds an IDataReader.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="behavior">One of the CommandBehavior values.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <returns>An IDataReader object.</returns>
        public static async Task<DbDataReader> ExecuteQueryAsync(this DbConnection cn, string commandText, CommandType commandType, CommandBehavior behavior, int? commandTimeout, DbTransaction trx, IEnumerable<IDbDataParameter> parameters)
        {
            using (var cmd = CreateCommand(cn, commandText, commandType, commandTimeout, trx, parameters))
            {
                await OpenIfClosedAsync(cn);
                return await cmd.ExecuteReaderAsync(behavior);
            }
        }

        /// <summary>
        /// Executes the CommandText against the Connection and builds an IDataReader to handle rows that contain a large binary column.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>An IDataReader object.</returns>
        /// <remarks>
        /// Builds the DataReader with CommandBehavior.SingleResult | CommandBehavior.SingleRow | CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection.
        /// </remarks>
        public static IDataReader ExecuteSequentialSingle(this IDbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
            => ExecuteQuery(cn, commandText, CommandType.Text, CommandBehavior.SingleResult | CommandBehavior.SingleRow | CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection, null, (IDbTransaction)null, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and builds an IDataReader to handle rows that contain a large binary column.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>An IDataReader object.</returns>
        /// <remarks>
        /// Builds the DataReader with CommandBehavior.SingleResult | CommandBehavior.SingleRow | CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection.
        /// </remarks>
        public static IDataReader ExecuteSequentialSingle(this IDbConnection cn, string commandText, CommandType commandType = CommandType.Text, params (string ParameterName, object Value)[] parameters)
            => ExecuteQuery(cn, commandText, commandType, CommandBehavior.SingleResult | CommandBehavior.SingleRow | CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection, null, (IDbTransaction)null, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and builds an IDataReader to handle rows that contain a large binary column.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>An IDataReader object.</returns>
        /// <remarks>
        /// Builds the DataReader with CommandBehavior.SingleResult | CommandBehavior.SingleRow | CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection.
        /// </remarks>
        public static IDataReader ExecuteSequentialSingle(this IDbConnection cn, string commandText, CommandType commandType = CommandType.Text, IEnumerable<IDbDataParameter> parameters = null)
            => ExecuteQuery(cn, commandText, commandType, CommandBehavior.SingleResult | CommandBehavior.SingleRow | CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection, null, (IDbTransaction)null, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and builds an IDataReader to handle rows that contain a large binary column.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>An IDataReader object.</returns>
        /// <remarks>
        /// Builds the DataReader with CommandBehavior.SingleResult | CommandBehavior.SingleRow | CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection.
        /// </remarks>
        public static Task<DbDataReader> ExecuteSequentialSingleAsync(this DbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
            => ExecuteQueryAsync(cn, commandText, CommandType.Text, CommandBehavior.SingleResult | CommandBehavior.SingleRow | CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection, null, (DbTransaction)null, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and builds an IDataReader to handle rows that contain a large binary column.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>An IDataReader object.</returns>
        /// <remarks>
        /// Builds the DataReader with CommandBehavior.SingleResult | CommandBehavior.SingleRow | CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection.
        /// </remarks>
        public static Task<DbDataReader> ExecuteSequentialSingleAsync(this DbConnection cn, string commandText, CommandType commandType = CommandType.Text, params (string ParameterName, object Value)[] parameters)
            => ExecuteQueryAsync(cn, commandText, commandType, CommandBehavior.SingleResult | CommandBehavior.SingleRow | CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection, null, (DbTransaction)null, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and builds an IDataReader to handle rows that contain a large binary column.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>An IDataReader object.</returns>
        /// <remarks>
        /// Builds the DataReader with CommandBehavior.SingleResult | CommandBehavior.SingleRow | CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection.
        /// </remarks>
        public static Task<DbDataReader> ExecuteSequentialSingleAsync(this DbConnection cn, string commandText, CommandType commandType = CommandType.Text, IEnumerable<IDbDataParameter> parameters = null)
            => ExecuteQueryAsync(cn, commandText, commandType, CommandBehavior.SingleResult | CommandBehavior.SingleRow | CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection, null, (DbTransaction)null, parameters);

        /// <summary>
        /// Executes an SQL statement against the Connection object of a .NET data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this IDbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
            => ExecuteNonQuery(cn, commandText, CommandType.Text, parameters);

        /// <summary>
        /// Executes an SQL statement against the Connection object of a .NET data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this IDbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
            => ExecuteNonQuery(cn, commandText, commandType, null, (IDbTransaction)null, parameters);

        /// <summary>
        /// Executes an SQL statement against the Connection object of a .NET data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this IDbConnection cn, string commandText, CommandType commandType, int? commandTimeout, IDbTransaction trx, params (string ParameterName, object Value)[] parameters)
        {
            using (var cmd = CreateCommand(cn, commandText, commandType, commandTimeout, trx, parameters))
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
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this IDbConnection cn, string commandText, CommandType commandType, int? commandTimeout, DbTransaction trx, IEnumerable<IDbDataParameter> parameters)
        {
            using (var cmd = CreateCommand(cn, commandText, commandType, commandTimeout, trx, parameters))
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
            => ExecuteNonQueryAsync(cn, commandText, CommandType.Text, parameters);

        /// <summary>
        /// Executes an SQL statement against the Connection object of a .NET data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>The number of rows affected.</returns>
        public static Task<int> ExecuteNonQueryAsync(this DbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
            => ExecuteNonQueryAsync(cn, commandText, commandType, null, (DbTransaction)null, parameters);

        /// <summary>
        /// Executes an SQL statement against the Connection object of a .NET data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <returns>The number of rows affected.</returns>
        public static async Task<int> ExecuteNonQueryAsync(this DbConnection cn, string commandText, CommandType commandType, int? commandTimeout, DbTransaction trx, params (string ParameterName, object Value)[] parameters)
        {
            using (var cmd = CreateCommand(cn, commandText, commandType, commandTimeout, trx, parameters))
            {
                await OpenIfClosedAsync(cn);
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Executes an SQL statement against the Connection object of a .NET data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <returns>The number of rows affected.</returns>
        public static async Task<int> ExecuteNonQueryAsync(this DbConnection cn, string commandText, CommandType commandType, int? commandTimeout, DbTransaction trx, IEnumerable<IDbDataParameter> parameters)
        {
            using (var cmd = CreateCommand(cn, commandText, commandType, commandTimeout, trx, parameters))
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
            => ExecuteScalar(cn, commandText, CommandType.Text, parameters);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this IDbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
            => ExecuteScalar(cn, commandText, commandType, null, (IDbTransaction)null, parameters);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this IDbConnection cn, string commandText, CommandType commandType, int? commandTimeout, IDbTransaction trx, params (string ParameterName, object Value)[] parameters)
        {
            using (var cmd = CreateCommand(cn, commandText, commandType, commandTimeout, trx, parameters))
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
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this IDbConnection cn, string commandText, CommandType commandType, int? commandTimeout, IDbTransaction trx, IEnumerable<IDbDataParameter> parameters)
        {
            using (var cmd = CreateCommand(cn, commandText, commandType, commandTimeout, trx, parameters))
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
            => ExecuteScalarAsync(cn, commandText, CommandType.Text, parameters);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        public static Task<object> ExecuteScalarAsync(this DbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
            => ExecuteScalarAsync(cn, commandText, commandType, null, (DbTransaction)null, parameters);

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        public static async Task<object> ExecuteScalarAsync(this DbConnection cn, string commandText, CommandType commandType, int? commandTimeout, DbTransaction trx, params (string ParameterName, object Value)[] parameters)
        {
            using (var cmd = CreateCommand(cn, commandText, commandType, commandTimeout, trx, parameters))
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
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        public static async Task<object> ExecuteScalarAsync(this DbConnection cn, string commandText, CommandType commandType, int? commandTimeout, DbTransaction trx, IEnumerable<IDbDataParameter> parameters)
        {
            using (var cmd = CreateCommand(cn, commandText, commandType, commandTimeout, trx, parameters))
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
        public static IDbCommand OpenConnectionIfClosed(this IDbCommand cmd)
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
        public static async Task<DbCommand> OpenConnectionIfClosedAsync(this DbCommand cmd)
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
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A IDbCommand object.</returns>
        public static IDbCommand CreateCommand(this IDbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
            => CreateCommand(cn, commandText, commandType, null, (IDbTransaction)null, parameters);

        /// <summary>
        /// Utility method to create and configure a IDbCommand object in a single call.
        /// </summary>
        /// <param name="cn">The connection to be associated with the command.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <returns>A IDbCommand object.</returns>
        public static IDbCommand CreateCommand(this IDbConnection cn, string commandText, CommandType commandType, int? commandTimeout, IDbTransaction trx, params (string ParameterName, object Value)[] parameters)
        {
            IDbCommand cmd = cn.CreateCommand();

            if (commandTimeout.HasValue)
                cmd.CommandTimeout = commandTimeout.Value;

            if (trx != null)
                cmd.Transaction = trx;

            ConfigureCommand(cmd, commandText, commandType, parameters);

            return cmd;
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
            => CreateCommand(cn, commandText, commandType, null, (DbTransaction)null, parameters);

        /// <summary>
        /// Utility method to create and configure a IDbCommand object in a single call.
        /// </summary>
        /// <param name="cn">The connection to be associated with the command.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <returns>A IDbCommand object.</returns>
        public static DbCommand CreateCommand(this DbConnection cn, string commandText, CommandType commandType, int? commandTimeout, DbTransaction trx, params (string ParameterName, object Value)[] parameters)
        {
            DbCommand cmd = cn.CreateCommand();

            if (commandTimeout.HasValue)
                cmd.CommandTimeout = commandTimeout.Value;

            if (trx != null)
                cmd.Transaction = trx;

            ConfigureCommand(cmd, commandText, commandType, parameters);

            return cmd;
        }

        /// <summary>
        /// Utility method to create and configure a IDbCommand object in a single call.
        /// </summary>
        /// <param name="cn">The connection to be associated with the command.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A IDbCommand object.</returns>
        public static IDbCommand CreateCommand(this IDbConnection cn, string commandText, CommandType commandType, IEnumerable<IDbDataParameter> parameters)
            => CreateCommand(cn, commandText, commandType, null, (IDbTransaction)null, parameters);

        /// <summary>
        /// Utility method to create and configure a IDbCommand object in a single call.
        /// </summary>
        /// <param name="cn">The connection to be associated with the command.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <returns>A IDbCommand object.</returns>
        public static IDbCommand CreateCommand(this IDbConnection cn, string commandText, CommandType commandType, int? commandTimeout, IDbTransaction trx, IEnumerable<IDbDataParameter> parameters)
        {
            IDbCommand cmd = cn.CreateCommand();

            if (commandTimeout.HasValue)
                cmd.CommandTimeout = commandTimeout.Value;

            if (trx != null )
                cmd.Transaction = trx;

            ConfigureCommand(cmd, commandText, commandType, parameters);

            return cmd;
        }

        /// <summary>
        /// Utility method to create and configure a IDbCommand object in a single call.
        /// </summary>
        /// <param name="cn">The connection to be associated with the command.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A IDbCommand object.</returns>
        public static DbCommand CreateCommand(this DbConnection cn, string commandText, CommandType commandType, IEnumerable<IDbDataParameter> parameters)
            => CreateCommand(cn, commandText, commandType, null, (DbTransaction)null, parameters);

        /// <summary>
        /// Utility method to create and configure a IDbCommand object in a single call.
        /// </summary>
        /// <param name="cn">The connection to be associated with the command.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <returns>A IDbCommand object.</returns>
        public static DbCommand CreateCommand(this DbConnection cn, string commandText, CommandType commandType, int? commandTimeout, DbTransaction trx, IEnumerable<IDbDataParameter> parameters)
        {
            DbCommand cmd = cn.CreateCommand();

            if (commandTimeout.HasValue)
                cmd.CommandTimeout = commandTimeout.Value;

            if (trx != null )
                cmd.Transaction = trx;

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
        /// <returns>A IDbCommand object for chaining.</returns>
        public static IDbCommand ConfigureCommand(this IDbCommand cmd, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
        {
            cmd.CommandText = commandText;
            cmd.CommandType = commandType;

            AddParameters(cmd, parameters);

            return cmd;
        }

        /// <summary>
        /// Utility method to create and configure a IDbCommand object in a single call.
        /// </summary>
        /// <param name="cmd">The command object to configure.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A IDbCommand object for chaining.</returns>
        public static IDbCommand ConfigureCommand(this IDbCommand cmd, string commandText, CommandType commandType, IEnumerable<IDbDataParameter> parameters)
        {
            cmd.CommandText = commandText;
            cmd.CommandType = commandType;

            AddParameters(cmd, parameters);

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
        /// <returns>A IDbCommand object for chaining.</returns>
        public static IDbCommand AddParameter(this IDbCommand cmd, string parameterName, object value, DbType? paramType = null, int? size = null, byte? precision = null, byte? scale = null, ParameterDirection? direction = null, string sourceColumn = null, DataRowVersion? sourceVersion = null)
        {
            cmd.Parameters.Add(CreateParameter(cmd, parameterName, value, paramType, size, precision, scale, direction, sourceColumn, sourceVersion));
            
            return cmd;
        }

        /// <summary>
        /// Utility method to add parameters to a IDbCommand object in a single call.
        /// </summary>
        /// <param name="cmd">The command object to add parameters to.</param>
        /// <param name="parameters">The parameters to add to the command.</param>
        /// <returns>A IDbCommand object for chaining.</returns>
        public static IDbCommand AddParameters(this IDbCommand cmd, params (string ParameterName, object Value)[] parameters)
        {
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    AddParameter(cmd, param.ParameterName, param.Value);
                }
            }

            return cmd;
        }

        /// <summary>
        /// Utility method to add parameters to a IDbCommand object in a single call.
        /// </summary>
        /// <param name="cmd">The command object to add parameters to.</param>
        /// <param name="parameters">The parameters to add to the command.</param>
        /// <returns>A IDbCommand object for chaining.</returns>
        public static IDbCommand AddParameters(this IDbCommand cmd, IEnumerable<IDbDataParameter> parameters)
        {
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.Add(param);
                }
            }

            return cmd;
        }

        /// <summary>
        /// Utility method to create a IDbDataParameter in a single call.
        /// </summary>
        /// <param name="cmd">The command object to create parameters for.</param>
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
        public static IDbDataParameter CreateParameter(this IDbCommand cmd, string parameterName, object value, DbType? paramType = null, int? size = null, byte? precision = null, byte? scale = null, ParameterDirection? direction = null, string sourceColumn = null, DataRowVersion? sourceVersion = null)
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

            return param;
        }

        /// <summary>
        /// Utility method to create a collection of IDbDataParameters in a single call.
        /// </summary>
        /// <param name="cmd">The command object to create parameters for.</param>
        /// <param name="parameters">The tuple collection to convert.</param>
        /// <returns>A list of IDbDataParameter items.</returns>
        public static List<IDbDataParameter> CreateParameters(this IDbCommand cmd, IEnumerable<(string ParameterName, object Value)> parameters)
        {
            List<IDbDataParameter> result = null;

            if (parameters != null)
            {
                result = new List<IDbDataParameter>();

                foreach (var param in parameters)
                {
                    result.Add(CreateParameter(cmd, param.ParameterName, param.Value));
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the columns in an IDataReader as a KeyValuePair collection.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <returns>A KeyValuePair collection.</returns>
        public static IEnumerable<KeyValuePair<string, Type>> GetReaderColumns(this IDataReader reader)
        {
            List<KeyValuePair<string, Type>> result = new List<KeyValuePair<string, Type>>(reader.FieldCount);

            for (int i = 0; i < reader.FieldCount; i++)
                result.Add(new KeyValuePair<string, Type>(reader.GetName(i), reader.GetFieldType(i)));

            return result;
        }

        /// <summary>
        /// Returns a row from an IDataReader as a Dictionary of column and value.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <returns>A Dictionary of columns and their value.</returns>
        public static Dictionary<string, object> GetReaderRow(this IDataReader reader)
        {
            Dictionary<string, object> result = new Dictionary<string, object>(reader.FieldCount);

            for (int i = 0; i < reader.FieldCount; i++)
                result.Add(reader.GetName(i), reader.IsDBNull(i) ? null : reader.GetValue(i));

            return result;
        }

        /// <summary>
        /// Enumerates an IDataReader and returns rows as Dictionary of column and value.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="closeReader">Indicates whether to close the connection when the enumeration completes.</param>
        /// <returns>Enumerable list of rows as dictionary column/value pairs.</returns>
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
        /// <returns>A List of rows as Dictionary column/value items.</returns>
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
        /// <example>
        /// <code>
        /// ToList((reader) => new
        /// {
        ///    Field1 = reader.GetNullableString(0), //by key works too
        ///    Field2 = reader.GetNullableInt32(1), //by key works too
        /// });
        /// </code>
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
        /// <returns>A List of rows as Dictionary column/value items.</returns>
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
        /// <example>
        /// <code>
        /// ToListAsync((reader) => new
        /// {
        ///    Field1 = reader.GetNullableString(0), //by key works too
        ///    Field2 = reader.GetNullableInt32(1), //by key works too
        /// });
        /// </code>
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
        /// Iterates through a DataReader and performs an action.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="action">Action to perform.</param>
        /// <param name="closeReader">Indicates whether to close the connection when the enumeration completes.</param>
        public static void ForEachRow(this IDataReader reader, Func<IDataReader, bool> action, bool closeReader = true)
        {
            try
            {
                while (reader.Read())
                    if (!action(reader))
                        return;
            }
            finally
            {
                if (closeReader)
                    reader.Dispose();
            }
        }

        /// <summary>
        /// Iterates through a DataReader and performs an action.
        /// </summary>
        /// <param name="reader">The DbDataReader.</param>
        /// <param name="action">Action to perform.</param>
        /// <param name="closeReader">Indicates whether to close the connection when the enumeration completes.</param>
        public static async Task ForEachRowAsync(this DbDataReader reader, Func<DbDataReader, Task<bool>> action, bool closeReader = true)
        {
            try
            {
                while (await reader.ReadAsync())
                    if (!await action(reader))
                        return;
            }
            finally
            {
                if (closeReader)
                    reader.Dispose();
            }
        }

        /// <summary>
        /// Enumerates an IDataReader and returns the first row as a Dictionary of column/value items.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="closeReader">Indicates whether to close the connection when the enumeration completes.</param>
        /// <returns>A Dictionary of column/value items that represent the first row in the resultset.</returns>
        /// <remarks>It is recommended to use the CommandBehavior.SingleRow option in the Execute portion.</remarks>
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
        /// <remarks>It is recommended to use the CommandBehavior.SingleRow option in the Execute portion.</remarks>
        /// <example>
        /// <code>
        /// FirstOrDefault((reader) => new
        /// {
        ///    Field1 = reader.GetNullableString(0), //by key works too
        ///    Field2 = reader.GetNullableInt32(1), //by key works too
        /// });
        /// </code>
        ///	See also NullableFirstOrDefault&lt;T&gt;
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
        /// <returns>A custom concrete or anonymous object.</returns>
        /// <remarks>
        /// Meant to be used with ExecuteSequentialStream(). If the DataReader contains no data, the reader (and connection) are closed, otherwise they are left open to be consumed by the selector.
        /// </remarks>
        /// <example>
        /// <code>
        /// FirstOrDefault((reader) => new
        /// {
        ///    Field1 = reader.GetNullableString(0), //by key works too
        ///    Field2 = reader.GetNullableInt32(1), //by key works too
        /// });
        /// </code>
        ///	See also NullableFirstOrDefault&lt;T&gt;
        /// </example>
        public static T FirstOrDefaultSequentialSingle<T>(this IDataReader reader, Func<IDataReader, T> selector)
        {
            bool hasData = false;

            try
            {
                if (reader.Read())
                {
                    var result = selector(reader);

                    hasData = true;

                    return result;
                }
            }
            finally
            {
                if (!hasData)
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
        /// <returns>A nullable custom concrete or anonymous object.</returns>
        /// <remarks>It is recommended to use the CommandBehavior.SingleRow option in the Execute portion.</remarks>
        /// <example>
        /// <code>
        /// NullableFirstOrDefault((reader) => new
        /// {
        ///    Field1 = reader.GetNullableString(0), //by key works too
        ///    Field2 = reader.GetNullableInt32(1), //by key works too
        /// });
        /// </code>
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
        /// <returns>A Dictionary of column/value items that represent the first row in the resultset.</returns>
        /// <remarks>It is recommended to use the CommandBehavior.SingleRow option in the Execute portion.</remarks>
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
        /// <remarks>It is recommended to use the CommandBehavior.SingleRow option in the Execute portion.</remarks>
        /// <example>
        /// <code>
        /// FirstOrDefaultAsync((reader) => new
        /// {
        ///    Field1 = reader.GetNullableString(0), //by key works too
        ///    Field2 = reader.GetNullableInt32(1), //by key works too
        /// });
        /// </code>
        /// See also NullableFirstOrDefaultAsync&lt;T&gt;
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
        /// <returns>A custom concrete or anonymous object.</returns>
        /// <remarks>
        /// Meant to be used with ExecuteSequentialStream(). If the DataReader contains no data, the reader (and connection) are closed, otherwise they are left open to be consumed by the selector.
        /// </remarks>
        /// <example>
        /// <code>
        /// FirstOrDefaultAsync((reader) => new
        /// {
        ///    Field1 = reader.GetNullableString(0), //by key works too
        ///    Field2 = reader.GetNullableInt32(1), //by key works too
        /// });
        /// </code>
        /// See also NullableFirstOrDefaultAsync&lt;T&gt;
        /// </example>
        public static async Task<T> FirstOrDefaultSequentialSingleAsync<T>(this DbDataReader reader, Func<IDataReader, T> selector)
        {
            bool hasData = false;

            try
            {
                if (await reader.ReadAsync())
                {
                    var result = selector(reader);

                    hasData = true;

                    return result;
                }
            }
            finally
            {
                if (!hasData)
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
        /// <returns>A nullable custom concrete or anonymous object.</returns>
        /// <remarks>It is recommended to use the CommandBehavior.SingleRow option in the Execute portion.</remarks>
        /// <example>
        /// <code>
        /// NullableFirstOrDefaultAAsync((reader) => new
        /// {
        ///    Field1 = reader.GetNullableString(0), //by key works too
        ///    Field2 = reader.GetNullableInt32(1), //by key works too
        /// });
        /// </code>
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
        /// <param name="name">The name of the DataTable.</param>
        /// <param name="closeReader">Indicates whether to close the connection when the enumeration completes.</param>
        /// <param name="useClassicDataTableLoad">If true, uses the classic, built-in ADO.NET DataTable.Load() function.</param>
        /// <returns>A DataTable.</returns>
        public static DataTable ToDataTable(this IDataReader reader, string name = null, bool closeReader = true, bool useClassicDataTableLoad = true)
        {
            DataTable result = new DataTable();

            try
            {
                if (name != null)
                    result.TableName = name;

                if (useClassicDataTableLoad)
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
        /// <param name="name">The name of the DataTable.</param>
        /// <param name="closeReader">Indicates whether to close the connection when the enumeration completes.</param>
        /// <returns>A DataTable.</returns>
        public static async Task<DataTable> ToDataTableAsync(this DbDataReader reader, string name = null, bool closeReader = true)
        {
            DataTable result = new DataTable();

            try
            {
                if (name != null)
                    result.TableName = name;

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
        /// Enumerates all the resultsets in an IDataReader and returns a DataSet with multiple DataTables.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="tables">An array of strings from which to retrieve table name information.</param>
        /// <param name="closeReader">Indicates whether to close the connection when the enumeration completes.</param>
        /// <param name="useClassicDataSetLoad">If true, uses the classic, built-in ADO.NET DataSet.Load() function.</param>
        /// <returns>A DataSet.</returns>
        public static DataSet ToDataSet(this IDataReader reader, string[] tables, bool closeReader = true, bool useClassicDataSetLoad = true)
        {
            DataSet result = new DataSet();

            try
            {
                if (useClassicDataSetLoad)
                {
                    result.Load(reader, LoadOption.PreserveChanges, tables);
                }
                else
                {
                    int tableIndex = 0;
                    bool continueLoad = true;

                    while (continueLoad)
                    {
                        result.Tables.Add(ToDataTable(reader, tables[tableIndex], closeReader: false, useClassicDataTableLoad: false));

                        tableIndex++;
                        continueLoad = reader.NextResult();
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
        /// Enumerates all the resultsets in an IDataReader and returns a DataSet with multiple DataTables.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="tables">An array of strings from which to retrieve table name information.</param>
        /// <param name="closeReader">Indicates whether to close the connection when the enumeration completes.</param>
        /// <returns>A DataSet.</returns>
        public static async Task<DataSet> ToDataSetAsync(this DbDataReader reader, string[] tables, bool closeReader = true)
        {
            DataSet result = new DataSet();

            try
            {
                int tableIndex = 0;
                bool continueLoad = true;

                while (continueLoad)
                {
                    result.Tables.Add(await ToDataTableAsync(reader, tables[tableIndex], closeReader: false));

                    tableIndex++;
                    continueLoad = await reader.NextResultAsync();
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
        /// Executes the CommandText against the Connection and returns rows as a List of Dictionary column/value items.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A List of rows as Dictionary column/value items.</returns>
        public static List<Dictionary<string, object>> ExecuteToList(this IDbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
            => ExecuteToList(cn, commandText, CommandType.Text, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns rows as a List of Dictionary column/value items.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A List of rows as Dictionary column/value items.</returns>
        public static List<Dictionary<string, object>> ExecuteToList(this IDbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
            => ExecuteToList(cn, commandText, commandType, (int?)null, (IDbTransaction)null, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns rows as a List of Dictionary column/value items.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A List of rows as Dictionary column/value items.</returns>
        public static List<Dictionary<string, object>> ExecuteToList(this IDbConnection cn, string commandText, CommandType commandType, int? commandTimeout, IDbTransaction trx, params (string ParameterName, object Value)[] parameters)
            => ToList(ExecuteQuery(cn, commandText, commandType, CommandBehavior.SingleResult, commandTimeout, trx, parameters));

        /// <summary>
        /// Executes the CommandText against the Connection and returns rows as a List of Dictionary column/value items.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A List of rows as Dictionary column/value items.</returns>
        public static Task<List<Dictionary<string, object>>> ExecuteToListAsync(this DbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
            => ExecuteToListAsync(cn, commandText, CommandType.Text, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns rows as a List of Dictionary column/value items.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A List of rows as Dictionary column/value items.</returns>
        public static Task<List<Dictionary<string, object>>> ExecuteToListAsync(this DbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
            => ExecuteToListAsync(cn, commandText, commandType, (int?)null, (DbTransaction)null, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns rows as a List of Dictionary column/value items.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A List of rows as Dictionary column/value items.</returns>
        public static async Task<List<Dictionary<string, object>>> ExecuteToListAsync(this DbConnection cn, string commandText, CommandType commandType, int? commandTimeout, DbTransaction trx, params (string ParameterName, object Value)[] parameters)
            => await ToListAsync(await ExecuteQueryAsync(cn, commandText, commandType, CommandBehavior.SingleResult, commandTimeout, trx, parameters));

        /// <summary>
        /// Executes the CommandText against the Connection and returns rows as custom concrete or anonymous objects.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <returns>A List of custom concrete or anonymous objects.</returns>
        /// <example>
        /// <code>
        /// (reader) => new
        /// {
        ///    Field1 = reader.GetNullableString(0), //by key works too
        ///    Field2 = reader.GetNullableInt32(1), //by key works too
        /// }
        /// </code>
        /// </example>
        public static List<T> ExecuteToList<T>(this IDbConnection cn, string commandText, Func<IDataReader, T> selector, params (string ParameterName, object Value)[] parameters)
            => ExecuteToList(cn, commandText, CommandType.Text, selector, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns rows as custom concrete or anonymous objects.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <returns>A List of custom concrete or anonymous objects.</returns>
        /// <example>
        /// <code>
        /// (reader) => new
        /// {
        ///    Field1 = reader.GetNullableString(0), //by key works too
        ///    Field2 = reader.GetNullableInt32(1), //by key works too
        /// }
        /// </code>
        /// </example>
        public static List<T> ExecuteToList<T>(this IDbConnection cn, string commandText, CommandType commandType, Func<IDataReader, T> selector, params (string ParameterName, object Value)[] parameters)
            => ExecuteToList(cn, commandText, commandType, (int?)null, (IDbTransaction)null, selector, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns rows as custom concrete or anonymous objects.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <returns>A List of custom concrete or anonymous objects.</returns>
        /// <example>
        /// <code>
        /// (reader) => new
        /// {
        ///    Field1 = reader.GetNullableString(0), //by key works too
        ///    Field2 = reader.GetNullableInt32(1), //by key works too
        /// }
        /// </code>
        /// </example>
        public static List<T> ExecuteToList<T>(this IDbConnection cn, string commandText, CommandType commandType, int? commandTimeout, IDbTransaction trx, Func<IDataReader, T> selector, params (string ParameterName, object Value)[] parameters)
            => ToList(ExecuteQuery(cn, commandText, commandType, CommandBehavior.SingleResult, commandTimeout, trx, parameters), selector);

        /// <summary>
        /// Executes the CommandText against the Connection and returns rows as custom concrete or anonymous objects.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <returns>A List of custom concrete or anonymous objects.</returns>
        /// <example>
        /// <code>
        /// (reader) => new
        /// {
        ///    Field1 = reader.GetNullableString(0), //by key works too
        ///    Field2 = reader.GetNullableInt32(1), //by key works too
        /// }
        /// </code>
        /// </example>
        public static Task<List<T>> ExecuteToListAsync<T>(this DbConnection cn, string commandText, Func<IDataReader, T> selector, params (string ParameterName, object Value)[] parameters)
            => ExecuteToListAsync(cn, commandText, CommandType.Text, selector, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns rows as custom concrete or anonymous objects.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <returns>A List of custom concrete or anonymous objects.</returns>
        /// <example>
        /// <code>
        /// (reader) => new
        /// {
        ///    Field1 = reader.GetNullableString(0), //by key works too
        ///    Field2 = reader.GetNullableInt32(1), //by key works too
        /// }
        /// </code>
        /// </example>
        public static Task<List<T>> ExecuteToListAsync<T>(this DbConnection cn, string commandText, CommandType commandType, Func<IDataReader, T> selector, params (string ParameterName, object Value)[] parameters)
            => ExecuteToListAsync(cn, commandText, commandType, (int?)null, (DbTransaction)null, selector, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns rows as custom concrete or anonymous objects.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <returns>A List of custom concrete or anonymous objects.</returns>
        /// <example>
        /// <code>
        /// (reader) => new
        /// {
        ///    Field1 = reader.GetNullableString(0), //by key works too
        ///    Field2 = reader.GetNullableInt32(1), //by key works too
        /// }
        /// </code>
        /// </example>
        public static async Task<List<T>> ExecuteToListAsync<T>(this DbConnection cn, string commandText, CommandType commandType, int? commandTimeout, DbTransaction trx, Func<IDataReader, T> selector, params (string ParameterName, object Value)[] parameters)
            => await ToListAsync(await ExecuteQueryAsync(cn, commandText, commandType, CommandBehavior.SingleResult, commandTimeout, trx, parameters), selector);

        /// <summary>
        /// Executes the CommandText against the Connection and returns rows as a List of Dictionary column/value items.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A Dictionary of column/value items that represent the first row in the resultset.</returns>
        public static Dictionary<string, object> ExecuteFirstOrDefault(this IDbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
            => ExecuteFirstOrDefault(cn, commandText, CommandType.Text, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns rows as a List of Dictionary column/value items.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A Dictionary of column/value items that represent the first row in the resultset.</returns>
        public static Dictionary<string, object> ExecuteFirstOrDefault(this IDbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
            => ExecuteFirstOrDefault(cn, commandText, commandType, (int?)null, (IDbTransaction)null, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns rows as a List of Dictionary column/value items.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A Dictionary of column/value items that represent the first row in the resultset.</returns>
        public static Dictionary<string, object> ExecuteFirstOrDefault(this IDbConnection cn, string commandText, CommandType commandType, int? commandTimeout, IDbTransaction trx, params (string ParameterName, object Value)[] parameters)
            => FirstOrDefault(ExecuteQuery(cn, commandText, commandType, CommandBehavior.SingleRow | CommandBehavior.SingleResult, commandTimeout, trx, parameters));

        /// <summary>
        /// Executes the CommandText against the Connection and returns rows as custom concrete or anonymous objects.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <returns>A custom concrete or anonymous object.</returns>
        /// <example>
        /// <code>
        /// (reader) => new
        /// {
        ///    Field1 = reader.GetNullableString(0), //by key works too
        ///    Field2 = reader.GetNullableInt32(1), //by key works too
        /// }
        /// </code>
        /// </example>
        public static T ExecuteFirstOrDefault<T>(this IDbConnection cn, string commandText, Func<IDataReader, T> selector, params (string ParameterName, object Value)[] parameters)
            => ExecuteFirstOrDefault(cn, commandText, CommandType.Text, selector, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns rows as custom concrete or anonymous objects.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <returns>A custom concrete or anonymous object.</returns>
        /// <example>
        /// <code>
        /// (reader) => new
        /// {
        ///    Field1 = reader.GetNullableString(0), //by key works too
        ///    Field2 = reader.GetNullableInt32(1), //by key works too
        /// }
        /// </code>
        /// </example>
        public static T ExecuteFirstOrDefault<T>(this IDbConnection cn, string commandText, CommandType commandType, Func<IDataReader, T> selector, params (string ParameterName, object Value)[] parameters)
            => ExecuteFirstOrDefault(cn, commandText, commandType, (int?)null, (IDbTransaction)null, selector, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns rows as custom concrete or anonymous objects.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="behavior">One of the CommandBehavior values.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <returns>A custom concrete or anonymous object.</returns>
        /// <example>
        /// <code>
        /// (reader) => new
        /// {
        ///    Field1 = reader.GetNullableString(0), //by key works too
        ///    Field2 = reader.GetNullableInt32(1), //by key works too
        /// }
        /// </code>
        /// </example>
        public static T ExecuteFirstOrDefault<T>(this IDbConnection cn, string commandText, CommandType commandType, int? commandTimeout, IDbTransaction trx, Func<IDataReader, T> selector, params (string ParameterName, object Value)[] parameters)
            => FirstOrDefault(ExecuteQuery(cn, commandText, commandType, CommandBehavior.SingleRow | CommandBehavior.SingleResult, commandTimeout, trx, parameters), selector);

        /// <summary>
        /// Executes the CommandText against the Connection and returns rows as a List of Dictionary column/value items.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A Dictionary of column/value items that represent the first row in the resultset.</returns>
        public static Task<Dictionary<string, object>> ExecuteFirstOrDefaultAsync(this DbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
            => ExecuteFirstOrDefaultAsync(cn, commandText, CommandType.Text, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns rows as a List of Dictionary column/value items.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A Dictionary of column/value items that represent the first row in the resultset.</returns>
        public static Task<Dictionary<string, object>> ExecuteFirstOrDefaultAsync(this DbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
            => ExecuteFirstOrDefaultAsync(cn, commandText, commandType, (int?)null, (DbTransaction)null, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns rows as a List of Dictionary column/value items.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="behavior">One of the CommandBehavior values.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A Dictionary of column/value items that represent the first row in the resultset.</returns>
        public static async Task<Dictionary<string, object>> ExecuteFirstOrDefaultAsync(this DbConnection cn, string commandText, CommandType commandType, int? commandTimeout, DbTransaction trx, params (string ParameterName, object Value)[] parameters)
            => await FirstOrDefaultAsync(await ExecuteQueryAsync(cn, commandText, commandType, CommandBehavior.SingleRow | CommandBehavior.SingleResult, commandTimeout, trx, parameters));

        /// <summary>
        /// Executes the CommandText against the Connection and returns rows as custom concrete or anonymous objects.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <returns>A custom concrete or anonymous object.</returns>
        /// <example>
        /// <code>
        /// (reader) => new
        /// {
        ///    Field1 = reader.GetNullableString(0), //by key works too
        ///    Field2 = reader.GetNullableInt32(1), //by key works too
        /// }
        /// </code>
        /// </example>
        public static Task<T> ExecuteFirstOrDefaultAsync<T>(this DbConnection cn, string commandText, Func<IDataReader, T> selector, params (string ParameterName, object Value)[] parameters)
            => ExecuteFirstOrDefaultAsync(cn, commandText, CommandType.Text, selector, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns rows as custom concrete or anonymous objects.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <returns>A custom concrete or anonymous object.</returns>
        /// <example>
        /// <code>
        /// (reader) => new
        /// {
        ///    Field1 = reader.GetNullableString(0), //by key works too
        ///    Field2 = reader.GetNullableInt32(1), //by key works too
        /// }
        /// </code>
        /// </example>
        public static Task<T> ExecuteFirstOrDefaultAsync<T>(this DbConnection cn, string commandText, CommandType commandType, Func<IDataReader, T> selector, params (string ParameterName, object Value)[] parameters)
            => ExecuteFirstOrDefaultAsync(cn, commandText, commandType, (int?)null, (DbTransaction)null, selector, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns rows as custom concrete or anonymous objects.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <returns>A custom concrete or anonymous object.</returns>
        /// <remarks>It is recommended to use the CommandBehavior.SingleRow option in the Execute portion.</remarks>
        /// <example>
        /// <code>
        /// (reader) => new
        /// {
        ///    Field1 = reader.GetNullableString(0), //by key works too
        ///    Field2 = reader.GetNullableInt32(1), //by key works too
        /// }
        /// </code>
        /// </example>
        public static async Task<T> ExecuteFirstOrDefaultAsync<T>(this DbConnection cn, string commandText, CommandType commandType, int? commandTimeout, DbTransaction trx, Func<IDataReader, T> selector, params (string ParameterName, object Value)[] parameters)
            => await FirstOrDefaultAsync(await ExecuteQueryAsync(cn, commandText, commandType, CommandBehavior.SingleRow | CommandBehavior.SingleResult, commandTimeout, trx, parameters), selector);

        /// <summary>
        /// Executes the CommandText against the Connection and returns the first row as a tuple or a custom concrete or anonymous object.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <returns>A tuple or a custom concrete or anonymous object.</returns>
        /// <example>
        /// <code>
        /// (reader) =>
        /// (
        ///    Field1: reader.GetNullableString(0), //by key works too
        ///    Field2: reader.GetNullableInt32(1), //by key works too
        /// )
        /// </code>
        /// </example>
        public static T? ExecuteNullableFirstOrDefault<T>(this IDbConnection cn, string commandText, Func<IDataReader, T> selector, params (string ParameterName, object Value)[] parameters) where T : struct
            => ExecuteNullableFirstOrDefault(cn, commandText, CommandType.Text, selector, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns the first row as a tuple or a custom concrete or anonymous object.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <returns>A tuple or a custom concrete or anonymous object.</returns>
        /// <example>
        /// <code>
        /// (reader) =>
        /// (
        ///    Field1: reader.GetNullableString(0), //by key works too
        ///    Field2: reader.GetNullableInt32(1), //by key works too
        /// )
        /// </code>
        /// </example>
        public static T? ExecuteNullableFirstOrDefault<T>(this IDbConnection cn, string commandText, CommandType commandType, Func<IDataReader, T> selector, params (string ParameterName, object Value)[] parameters) where T : struct
            => ExecuteNullableFirstOrDefault(cn, commandText, commandType, (int?)null, (IDbTransaction)null, selector, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns the first row as a tuple or a custom concrete or anonymous object.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <returns>A tuple or a custom concrete or anonymous object.</returns>
        /// <example>
        /// <code>
        /// (reader) =>
        /// (
        ///    Field1: reader.GetNullableString(0), //by key works too
        ///    Field2: reader.GetNullableInt32(1), //by key works too
        /// )
        /// </code>
        /// </example>
        public static T? ExecuteNullableFirstOrDefault<T>(this IDbConnection cn, string commandText, CommandType commandType, int? commandTimeout, IDbTransaction trx, Func<IDataReader, T> selector, params (string ParameterName, object Value)[] parameters) where T : struct
            => NullableFirstOrDefault(ExecuteQuery(cn, commandText, commandType, CommandBehavior.SingleRow | CommandBehavior.SingleResult, commandTimeout, trx, parameters), selector);

        /// <summary>
        /// Executes the CommandText against the Connection and returns the first row as a tuple or a custom concrete or anonymous object.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <returns>A tuple or a custom concrete or anonymous object.</returns>
        /// <example>
        /// <code>
        /// (reader) =>
        /// (
        ///    Field1: reader.GetNullableString(0), //by key works too
        ///    Field2: reader.GetNullableInt32(1), //by key works too
        /// )
        /// </code>
        /// </example>
        public static Task<T?> ExecuteNullableFirstOrDefaultAsync<T>(this DbConnection cn, string commandText, Func<IDataReader, T> selector, params (string ParameterName, object Value)[] parameters) where T : struct
            => ExecuteNullableFirstOrDefaultAsync(cn, commandText, CommandType.Text, selector, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns the first row as a tuple or a custom concrete or anonymous object.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <returns>A tuple or a custom concrete or anonymous object.</returns>
        /// <example>
        /// <code>
        /// (reader) =>
        /// (
        ///    Field1: reader.GetNullableString(0), //by key works too
        ///    Field2: reader.GetNullableInt32(1), //by key works too
        /// )
        /// </code>
        /// </example>
        public static Task<T?> ExecuteNullableFirstOrDefaultAsync<T>(this DbConnection cn, string commandText, CommandType commandType, Func<IDataReader, T> selector, params (string ParameterName, object Value)[] parameters) where T : struct
            => ExecuteNullableFirstOrDefaultAsync(cn, commandText, commandType, (int?)null, (DbTransaction)null, selector, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns the first row as a tuple or a custom concrete or anonymous object.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="selector">Function to convert the data.</param>
        /// <returns>A tuple or a custom concrete or anonymous object.</returns>
        /// <example>
        /// <code>
        /// (reader) =>
        /// (
        ///    Field1: reader.GetNullableString(0), //by key works too
        ///    Field2: reader.GetNullableInt32(1), //by key works too
        /// )
        /// </code>
        /// </example>
        public static async Task<T?> ExecuteNullableFirstOrDefaultAsync<T>(this DbConnection cn, string commandText, CommandType commandType, int? commandTimeout, DbTransaction trx, Func<IDataReader, T> selector, params (string ParameterName, object Value)[] parameters) where T : struct
            => await NullableFirstOrDefaultAsync(await ExecuteQueryAsync(cn, commandText, commandType, CommandBehavior.SingleRow | CommandBehavior.SingleResult, commandTimeout, trx, parameters), selector);

        /// <summary>
        /// Executes the CommandText against the Connection and iterates through a DataReader and performs an action.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="action">Function to convert the data.</param>
        public static void ExecuteForEachRow(this IDbConnection cn, string commandText, (string ParameterName, object Value)[] parameters, Func<IDataReader, bool> action)
            => ExecuteForEachRow(cn, commandText, CommandType.Text, parameters, action);

        /// <summary>
        /// Executes the CommandText against the Connection and iterates through a DataReader and performs an action.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="action">Function to convert the data.</param>
        public static void ExecuteForEachRow(this IDbConnection cn, string commandText, CommandType commandType, (string ParameterName, object Value)[] parameters, Func<IDataReader, bool> action)
            => ExecuteForEachRow(cn, commandText, commandType, (int?)null, (IDbTransaction)null, parameters, action);

        /// <summary>
        /// Executes the CommandText against the Connection and iterates through a DataReader and performs an action.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="action">Function to convert the data.</param>
        public static void ExecuteForEachRow(this IDbConnection cn, string commandText, CommandType commandType, int? commandTimeout, IDbTransaction trx, (string ParameterName, object Value)[] parameters, Func<IDataReader, bool> action)
            => ForEachRow(ExecuteQuery(cn, commandText, commandType, CommandBehavior.SingleResult, commandTimeout, trx, parameters), action);

        /// <summary>
        /// Executes the CommandText against the Connection and iterates through a DataReader and performs an action.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="action">Function to convert the data.</param>
        public static Task ExecuteForEachRowAync(this DbConnection cn, string commandText, (string ParameterName, object Value)[] parameters, Func<IDataReader, Task<bool>> action)
            => ExecuteForEachRowAsync(cn, commandText, CommandType.Text, parameters, action);

        /// <summary>
        /// Executes the CommandText against the Connection and iterates through a DataReader and performs an action.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="action">Function to convert the data.</param>
        public static Task ExecuteForEachRowAsync(this DbConnection cn, string commandText, CommandType commandType, (string ParameterName, object Value)[] parameters, Func<IDataReader, Task<bool>> action)
            => ExecuteForEachRowAsync(cn, commandText, commandType, (int?)null, (DbTransaction)null, parameters, action);

        /// <summary>
        /// Executes the CommandText against the Connection and iterates through a DataReader and performs an action.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="action">Function to convert the data.</param>
        public static async Task ExecuteForEachRowAsync(this DbConnection cn, string commandText, CommandType commandType, int? commandTimeout, DbTransaction trx, (string ParameterName, object Value)[] parameters, Func<IDataReader, Task<bool>> action)
            => await ForEachRowAsync(await ExecuteQueryAsync(cn, commandText, commandType, CommandBehavior.SingleResult, commandTimeout, trx, parameters), action);

        /// <summary>
        /// Executes the CommandText against the Connection and returns a DataTable.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A DataTable.</returns>
        public static DataTable ExecuteToDataTable(this IDbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
            => ExecuteToDataTable(cn, commandText, CommandType.Text, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns a DataTable.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A DataTable.</returns>
        public static DataTable ExecuteToDataTable(this IDbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
            => ExecuteToDataTable(cn, commandText, commandType, (int?)null, (IDbTransaction)null, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns a DataTable.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A DataTable.</returns>
        public static DataTable ExecuteToDataTable(this IDbConnection cn, string commandText, CommandType commandType, int? commandTimeout, IDbTransaction trx, params (string ParameterName, object Value)[] parameters)
            => ToDataTable(ExecuteQuery(cn, commandText, commandType, CommandBehavior.SingleResult, commandTimeout, trx, parameters));

        /// <summary>
        /// Executes the CommandText against the Connection and returns a DataTable.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A DataTable.</returns>
        public static Task<DataTable> ExecuteToDataTableAsync(this DbConnection cn, string commandText, params (string ParameterName, object Value)[] parameters)
            => ExecuteToDataTableAsync(cn, commandText, CommandType.Text, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns a DataTable.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A DataTable.</returns>
        public static Task<DataTable> ExecuteToDataTableAsync(this DbConnection cn, string commandText, CommandType commandType, params (string ParameterName, object Value)[] parameters)
            => ExecuteToDataTableAsync(cn, commandText, commandType, (int?)null, (DbTransaction)null, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns a DataTable.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <returns>A DataTable.</returns>
        public static async Task<DataTable> ExecuteToDataTableAsync(this DbConnection cn, string commandText, CommandType commandType, int? commandTimeout, DbTransaction trx, params (string ParameterName, object Value)[] parameters)
            => await ToDataTableAsync(await ExecuteQueryAsync(cn, commandText, commandType, CommandBehavior.SingleResult, commandTimeout, trx, parameters));

        /// <summary>
        /// Executes the CommandText against the Connection and returns a DataSet with multiple DataTables.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="tables">An array of strings from which to retrieve table name information.</param>
        /// <returns>A DataSet.</returns>
        public static DataSet ExecuteToDataSet(this IDbConnection cn, string commandText, string[] tables, params (string ParameterName, object Value)[] parameters)
            => ExecuteToDataSet(cn, commandText, CommandType.Text, tables, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns a DataSet with multiple DataTables.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="tables">An array of strings from which to retrieve table name information.</param>
        /// <returns>A DataSet.</returns>
        public static DataSet ExecuteToDataSet(this IDbConnection cn, string commandText, CommandType commandType, string[] tables, params (string ParameterName, object Value)[] parameters)
            => ExecuteToDataSet(cn, commandText, commandType, (int?)null, (IDbTransaction)null, tables, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns a DataSet with multiple DataTables.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="tables">An array of strings from which to retrieve table name information.</param>
        /// <returns>A DataSet.</returns>
        public static DataSet ExecuteToDataSet(this IDbConnection cn, string commandText, CommandType commandType, int? commandTimeout, IDbTransaction trx, string[] tables, params (string ParameterName, object Value)[] parameters)
            => ToDataSet(ExecuteQuery(cn, commandText, commandType, CommandBehavior.Default, commandTimeout, trx, parameters), tables);

        /// <summary>
        /// Executes the CommandText against the Connection and returns a DataSet with multiple DataTables.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="tables">An array of strings from which to retrieve table name information.</param>
        /// <returns>A DataSet.</returns>
        public static Task<DataSet> ExecuteToDataSetAsync(this DbConnection cn, string commandText, string[] tables, params (string ParameterName, object Value)[] parameters)
            => ExecuteToDataSetAsync(cn, commandText, CommandType.Text, tables, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns a DataSet with multiple DataTables.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="tables">An array of strings from which to retrieve table name information.</param>
        /// <returns>A DataSet.</returns>
        public static Task<DataSet> ExecuteToDataSetAsync(this DbConnection cn, string commandText, CommandType commandType, string[] tables, params (string ParameterName, object Value)[] parameters)
            => ExecuteToDataSetAsync(cn, commandText, commandType, (int?)null, (DbTransaction)null, tables, parameters);

        /// <summary>
        /// Executes the CommandText against the Connection and returns a DataSet with multiple DataTables.
        /// </summary>
        /// <param name="cn">The database connection to execute the query on. The connection will be opened if it is closed.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <param name="commandType">Indicates or specifies how the CommandText property is interpreted.</param>
        /// <param name="commandTimeout">The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</param>
        /// <param name="trx">The transaction to use for the command.</param>
        /// <param name="parameters">The parameters of the SQL statement or stored procedure.</param>
        /// <param name="tables">An array of strings from which to retrieve table name information.</param>
        /// <returns>A DataSet.</returns>
        public static async Task<DataSet> ExecuteToDataSetAsync(this DbConnection cn, string commandText, CommandType commandType, int? commandTimeout, DbTransaction trx, string[] tables, params (string ParameterName, object Value)[] parameters)
            => await ToDataSetAsync(await ExecuteQueryAsync(cn, commandText, commandType, CommandBehavior.Default, commandTimeout, trx, parameters), tables);

        /// <summary>
        /// Gets the string value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The string value of the specified field or NULL if the value is DbNull.</returns>
        public static string GetNullableString(this IDataReader reader, int i)
            => reader.IsDBNull(i) ? null : reader.GetString(i);

        /// <summary>
        /// Gets the bool value of the specified column if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>A nullable boolean.</returns>
        public static bool? GetNullableBoolean(this IDataReader reader, int i)
            => reader.IsDBNull(i) ? (bool?)null : reader.GetBoolean(i);

        /// <summary>
        /// Gets the 8-bit unsigned integer value of the specified column if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The nullable 8-bit unsigned integer value of the specified column.</returns>
        public static byte? GetNullableByte(this IDataReader reader, int i)
            => reader.IsDBNull(i) ? (byte?)null : reader.GetByte(i);

        /// <summary>
        /// Gets the character value of the specified column if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The nullable character value of the specified column.</returns>
        public static char? GetNullableChar(this IDataReader reader, int i)
            => reader.IsDBNull(i) ? (char?)null : reader.GetChar(i);

        /// <summary>
        /// Gets the date and time data value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The nullable date and time data value of the specified field.</returns>
        public static DateTime? GetNullableDateTime(this IDataReader reader, int i)
            => reader.IsDBNull(i) ? (DateTime?)null : reader.GetDateTime(i);

        /// <summary>
        /// Gets the fixed-position numeric value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The nullable fixed-position numeric value of the specified field.</returns>
        public static decimal? GetNullableDecimal(this IDataReader reader, int i)
            => reader.IsDBNull(i) ? (decimal?)null : reader.GetDecimal(i);

        /// <summary>
        /// Gets the double-precision floating point number of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The nullable double-precision floating point number of the specified field.</returns>
        public static double? GetNullableDouble(this IDataReader reader, int i)
            => reader.IsDBNull(i) ? (double?)null : reader.GetDouble(i);

        /// <summary>
        /// Gets the single-precision floating point number of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The nullable single-precision floating point number of the specified field.</returns>
        public static float? GetNullableFloat(this IDataReader reader, int i)
            => reader.IsDBNull(i) ? (float?)null : reader.GetFloat(i);

        /// <summary>
        /// Returns the GUID value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The nullable GUID value of the specified field.</returns>
        public static Guid? GetNullableGuid(this IDataReader reader, int i)
            => reader.IsDBNull(i) ? (Guid?)null : reader.GetGuid(i);

        /// <summary>
        /// Gets the 16-bit signed integer value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The nullable 16-bit signed integer value of the specified field.</returns>
        public static short? GetNullableInt16(this IDataReader reader, int i)
            => reader.IsDBNull(i) ? (short?)null : reader.GetInt16(i);

        /// <summary>
        /// Gets the 32-bit signed integer value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The nullable 32-bit signed integer value of the specified field.</returns>
        public static int? GetNullableInt32(this IDataReader reader, int i)
            => reader.IsDBNull(i) ? (int?)null : reader.GetInt32(i);

        /// <summary>
        /// Gets the 64-bit signed integer value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The nullable 64-bit signed integer value of the specified field.</returns>
        public static long? GetNullableInt64(this IDataReader reader, int i)
            => reader.IsDBNull(i) ? (long?)null : reader.GetInt64(i);

        /// <summary>
        /// Gets a stream to retrieve data from the specified column if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>A stream.</returns>
        public static Stream GetNullableStream(this DbDataReader reader, int i)
            => reader.IsDBNull(i) ? (Stream)null : reader.GetStream(i);

        /// <summary>
        /// Gets the string value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The string value of the specified field or NULL if the value is DbNull.</returns>
        public static string GetNullableString(this IDataReader reader, string name)
            => GetNullableString(reader, reader.GetOrdinal(name));

        /// <summary>
        /// Gets the bool value of the specified column if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>A nullable boolean.</returns>
        public static bool? GetNullableBoolean(this IDataReader reader, string name)
            => GetNullableBoolean(reader, reader.GetOrdinal(name));

        /// <summary>
        /// Gets the 8-bit unsigned integer value of the specified column if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The nullable 8-bit unsigned integer value of the specified column.</returns>
        public static byte? GetNullableByte(this IDataReader reader, string name)
            => GetNullableByte(reader, reader.GetOrdinal(name));

        /// <summary>
        /// Gets the character value of the specified column if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The nullable character value of the specified column.</returns>
        public static char? GetNullableChar(this IDataReader reader, string name)
            => GetNullableChar(reader, reader.GetOrdinal(name));

        /// <summary>
        /// Gets the date and time data value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The nullable date and time data value of the specified field.</returns>
        public static DateTime? GetNullableDateTime(this IDataReader reader, string name)
            => GetNullableDateTime(reader, reader.GetOrdinal(name));

        /// <summary>
        /// Gets the fixed-position numeric value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The nullable fixed-position numeric value of the specified field.</returns>
        public static decimal? GetNullableDecimal(this IDataReader reader, string name)
            => GetNullableDecimal(reader, reader.GetOrdinal(name));

        /// <summary>
        /// Gets the double-precision floating point number of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The nullable double-precision floating point number of the specified field.</returns>
        public static double? GetNullableDouble(this IDataReader reader, string name)
            => GetNullableDouble(reader, reader.GetOrdinal(name));

        /// <summary>
        /// Gets the single-precision floating point number of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The nullable single-precision floating point number of the specified field.</returns>
        public static float? GetNullableFloat(this IDataReader reader, string name)
            => GetNullableFloat(reader, reader.GetOrdinal(name));

        /// <summary>
        /// Returns the GUID value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The nullable GUID value of the specified field.</returns>
        public static Guid? GetNullableGuid(this IDataReader reader, string name)
            => GetNullableGuid(reader, reader.GetOrdinal(name));

        /// <summary>
        /// Gets the 16-bit signed integer value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The nullable 16-bit signed integer value of the specified field.</returns>
        public static short? GetNullableInt16(this IDataReader reader, string name)
            => GetNullableInt16(reader, reader.GetOrdinal(name));

        /// <summary>
        /// Gets the 32-bit signed integer value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The nullable 32-bit signed integer value of the specified field.</returns>
        public static int? GetNullableInt32(this IDataReader reader, string name)
            => GetNullableInt32(reader, reader.GetOrdinal(name));

        /// <summary>
        /// Gets the 64-bit signed integer value of the specified field if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The nullable 64-bit signed integer value of the specified field.</returns>
        public static long? GetNullableInt64(this IDataReader reader, string name)
            => GetNullableInt64(reader, reader.GetOrdinal(name));

        /// <summary>
        /// Gets a stream to retrieve data from the specified column if IsDBNull is false otherwise returns null.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>A stream.</returns>
        public static Stream GetNullableStream(this DbDataReader reader, string name)
            => GetNullableStream(reader, reader.GetOrdinal(name));

        /// <summary>
        /// Gets the string value of the specified field.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The string value of the specified field.</returns>
        /// <remarks>Shorthand for reader.Get...(reader.GetOrdinal(name)</remarks>
        public static string GetString(this IDataReader reader, string name)
            => reader.GetString(reader.GetOrdinal(name));

        /// <summary>
        /// Gets the bool value of the specified column.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>A boolean.</returns>
        /// <remarks>Shorthand for reader.Get...(reader.GetOrdinal(name)</remarks>
        public static bool GetBoolean(this IDataReader reader, string name)
            => reader.GetBoolean(reader.GetOrdinal(name));

        /// <summary>
        /// Gets the 8-bit unsigned integer value of the specified column.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The 8-bit unsigned integer value of the specified column.</returns>
        /// <remarks>Shorthand for reader.Get...(reader.GetOrdinal(name)</remarks>
        public static byte GetByte(this IDataReader reader, string name)
            => reader.GetByte(reader.GetOrdinal(name));

        /// <summary>
        /// Gets the character value of the specified column.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The character value of the specified column.</returns>
        /// <remarks>Shorthand for reader.Get...(reader.GetOrdinal(name)</remarks>
        public static char GetChar(this IDataReader reader, string name)
            => reader.GetChar(reader.GetOrdinal(name));

        /// <summary>
        /// Gets the date and time data value of the specified field.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The date and time data value of the specified field.</returns>
        /// <remarks>Shorthand for reader.Get...(reader.GetOrdinal(name)</remarks>
        public static DateTime GetDateTime(this IDataReader reader, string name)
            => reader.GetDateTime(reader.GetOrdinal(name));

        /// <summary>
        /// Gets the fixed-position numeric value of the specified field.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The fixed-position numeric value of the specified field.</returns>
        /// <remarks>Shorthand for reader.Get...(reader.GetOrdinal(name)</remarks>
        public static decimal GetDecimal(this IDataReader reader, string name)
            => reader.GetDecimal(reader.GetOrdinal(name));

        /// <summary>
        /// Gets the double-precision floating point number of the specified field.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The double-precision floating point number of the specified field.</returns>
        /// <remarks>Shorthand for reader.Get...(reader.GetOrdinal(name)</remarks>
        public static double GetDouble(this IDataReader reader, string name)
            => reader.GetDouble(reader.GetOrdinal(name));

        /// <summary>
        /// Gets the single-precision floating point number of the specified field.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The single-precision floating point number of the specified field.</returns>
        /// <remarks>Shorthand for reader.Get...(reader.GetOrdinal(name)</remarks>
        public static float GetFloat(this IDataReader reader, string name)
            => reader.GetFloat(reader.GetOrdinal(name));

        /// <summary>
        /// Returns the GUID value of the specified field.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The GUID value of the specified field.</returns>
        /// <remarks>Shorthand for reader.Get...(reader.GetOrdinal(name)</remarks>
        public static Guid GetGuid(this IDataReader reader, string name)
            => reader.GetGuid(reader.GetOrdinal(name));

        /// <summary>
        /// Gets the 16-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The 16-bit signed integer value of the specified field.</returns>
        /// <remarks>Shorthand for reader.Get...(reader.GetOrdinal(name)</remarks>
        public static short GetInt16(this IDataReader reader, string name)
            => reader.GetInt16(reader.GetOrdinal(name));

        /// <summary>
        /// Gets the 32-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The 32-bit signed integer value of the specified field.</returns>
        /// <remarks>Shorthand for reader.Get...(reader.GetOrdinal(name)</remarks>
        public static int GetInt32(this IDataReader reader, string name)
            => reader.GetInt32(reader.GetOrdinal(name));

        /// <summary>
        /// Gets the 64-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>The 64-bit signed integer value of the specified field.</returns>
        /// <remarks>Shorthand for reader.Get...(reader.GetOrdinal(name)</remarks>
        public static long GetInt64(this IDataReader reader, string name)
            => reader.GetInt64(reader.GetOrdinal(name));

        /// <summary>
        /// Gets a stream to retrieve data from the specified column.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>TA stream.</returns>
        /// <remarks>Shorthand for reader.Get...(reader.GetOrdinal(name)</remarks>
        public static Stream GetStream(this DbDataReader reader, string name)
            => reader.GetStream(reader.GetOrdinal(name));

        /// <summary>
        /// Returns a stream that will close the DataReader when it is closed.
        /// </summary>
        /// <param name="reader">The IDataReader.</param>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>A stream to the contents of the field.</returns>
        /// <remarks>Intended for use with large binary columns accessed via ExecuteSequentialStream or CommandBehavior CommandBehavior.SingleResult | CommandBehavior.SingleRow | CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection.</remarks>
        public static DataReaderBinaryStream GetDataReaderBinaryStream(this IDataReader reader, int i)
            => new DataReaderBinaryStream(reader, i);
    }
}