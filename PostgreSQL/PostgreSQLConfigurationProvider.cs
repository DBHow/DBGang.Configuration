using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DBGang.Configuration.PostgreSQL
{
    public class PostgreSQLConfigurationProvider : ConfigurationProvider
    {
        public PostgreSQLConfigurationSource Source { get; }

        public PostgreSQLConfigurationProvider(PostgreSQLConfigurationSource source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            
            if (Source.ReloadOnChange)
            {
                var watcher = new PostgreSQLConfigurationWatcher(Source.ConnectionString, LoadAsync);
                _ = watcher.Watch();
            }
        }

        public override void Load()
        {
            UtilHelper.RunSync(() => LoadAsync(reload: false));
        }

        protected async Task LoadAsync(bool reload)
        {
            if (reload)
            {
                Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            using var conn = new NpgsqlConnection(Source.ConnectionString);
            using var comm = new NpgsqlCommand("SELECT key, value FROM app_configuration", conn);

            await conn.OpenAsync();
            await comm.PrepareAsync();
            using var reader = await comm.ExecuteReaderAsync();

            if (reader.HasRows)
            {
                while (await reader.ReadAsync())
                {
                    Data.Add(reader.GetString(0), reader.GetString(1));
                }
            }

            await conn.CloseAsync();
        }

    }
}
