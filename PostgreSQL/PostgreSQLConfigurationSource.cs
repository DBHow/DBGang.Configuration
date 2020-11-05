using Microsoft.Extensions.Configuration;

namespace DBGang.Configuration.PostgreSQL
{
    public class PostgreSQLConfigurationSource : IConfigurationSource
    {
        public string ConnectionString { get; set; }
        public bool ReloadOnChange { get; set; }

        public PostgreSQLConfigurationSource(string connectionString, bool reloadOnChange)
        {
            ConnectionString = connectionString;
            ReloadOnChange = reloadOnChange;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new PostgreSQLConfigurationProvider(this);
        }
    }
}
