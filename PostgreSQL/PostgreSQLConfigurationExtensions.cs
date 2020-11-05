using System;
using DBGang.Configuration.PostgreSQL;

namespace Microsoft.Extensions.Configuration
{
    public static class PostgreSQLConfigurationExtensions
    {
        public static IConfigurationBuilder AddPostgreSQLConfiguration(this IConfigurationBuilder builder, string connectionString)
        {
            return AddPostgreSQLConfiguration(builder, connectionString, false);
        }

        public static IConfigurationBuilder AddPostgreSQLConfiguration(this IConfigurationBuilder builder, string connectionString, bool reloadOnChange)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            builder.Add(new PostgreSQLConfigurationSource(connectionString, reloadOnChange));

            return builder;
        }
    }
}
