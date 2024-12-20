﻿using Cflashsoft.Framework.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace Cflashsoft.Framework.ApiKeyAuthentication
{
    /// <summary>
    /// Options to configure API key authentication.
    /// </summary>
    public class SqlAppApiKeysProviderOptions
    {
        /// <summary>
        /// Connection string for the SQL Server instance.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// If true, creates the database tables necessary for API key authentication.
        /// </summary>
        public bool InitializeDatabase { get; set; }
    }

    internal class SqlAppApiKeysProvider : IAppApiKeysProvider
    {
        private readonly SqlAppApiKeysProviderOptions _options = null;

        public SqlAppApiKeysProvider(SqlAppApiKeysProviderOptions options)
        {
            _options = options;

            if (_options.InitializeDatabase)
                InitializeDatabase();
        }

        public async Task<AppApiKeyModel> GetAppApiKeyInfoAsync(string apiKey, ApiKeyAuthenticationSchemeOptions options)
        {
            await using var cn = new SqlConnection(_options.ConnectionString);

            var appApiKeyInfo = await cn.ExecuteNullableFirstOrDefaultAsync("SELECT Id, Name FROM CfAuth_AppApiKeys WHERE ApiKey = @ApiKey AND Enabled = 1 AND (ExpiryDate IS NULL OR ExpiryDate > GETUTCDATE())",
                reader => 
                (
                    Id: reader.GetInt32(0),
                    Name: reader.GetString(1)
                ),
                ("ApiKey", apiKey));

            if (appApiKeyInfo.HasValue)
            {
                var roles = await cn.ExecuteToListAsync("SELECT r.Name FROM CfAuth_AppApiKeyRoles kr INNER JOIN CfAuth_AppRoles r ON kr.AppRoleId = r.Id WHERE kr.AppApiKeyId = @AppApiKeyId AND r.Enabled = 1",
                    reader => reader.GetString(0),
                    ("AppApiKeyId", appApiKeyInfo.Value.Id));

                return new AppApiKeyModel
                {
                    Name = appApiKeyInfo.Value.Name,
                    Key = apiKey,
                    Roles = roles.Count > 0 ? roles : null
                };
            }

            return null;
        }

        private void InitializeDatabase()
        {
            using var cn = new SqlConnection(_options.ConnectionString);

            if (!((int)cn.ExecuteScalar("IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CfAuth_AppApiKeys') BEGIN SELECT 1 AS Result END ELSE BEGIN SELECT 0 AS Result END") == 1))
            {
                var scripts = new[] {
                "Cflashsoft.Framework.ApiKeyAuthentication.SqlApiKeysProvider.SQL_Schema_Scripts.Create_Schema.txt",
                "Cflashsoft.Framework.ApiKeyAuthentication.SqlApiKeysProvider.SQL_Schema_Scripts.Seed_Roles_And_Keys.txt" };

                foreach (var script in scripts)
                    using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(script))
                    using (var reader = new StreamReader(stream))
                        foreach (var segment in reader.ReadToEnd().Split("\r\nGO", StringSplitOptions.RemoveEmptyEntries))
                            cn.ExecuteNonQuery(segment);
            }
        }
    }

    /// <summary>
    /// Extension methods for SqlAppApiKeysProvider.
    /// </summary>
    public static class SqlAppApiKeysProviderExtensions
    {
        /// <summary>
        /// Add SQL API key authentication with configured options such as the connection string to use.
        /// </summary>
        public static IServiceCollection AddSqlApiKeyAuthentication(this IServiceCollection services, Action<SqlAppApiKeysProviderOptions> configureOptions)
        {
            var options = new SqlAppApiKeysProviderOptions();

            configureOptions(options);

            services.AddSingleton(options);

            services.AddApiKeyAuthentication<SqlAppApiKeysProvider>();

            return services;
        }
    }
}
