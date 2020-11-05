using System;
using System.Threading.Tasks;
using Npgsql;

namespace DBGang.Configuration.PostgreSQL
{
    class PostgreSQLConfigurationWatcher : IDisposable
    {
        private readonly string _connectionString;
        private readonly Func<bool, Task> _callback;

        private bool _disposed = false;
        private bool _expected = false;
        private TaskCompletionSource<bool> _tcs;
        private NpgsqlConnection _connection;

        public PostgreSQLConfigurationWatcher(string connectionString, Func<bool, Task> callback)
        {
            _connectionString = connectionString.Trim();
            if (!_connectionString.Contains("keepalive"))
            {
                _connectionString += _connectionString.EndsWith(";") ? "Keepalive=15;" : ";Keepalive=15";
            }

            _callback = callback;
        }

        private async Task InitAsync()
        {
            _tcs = new TaskCompletionSource<bool>();
            _connection = new NpgsqlConnection(_connectionString);

            _connection.Notification += (o, e) =>
            {
                _tcs.SetResult(true);
            };

            _connection.StateChange += async (o, e) =>
            {
                if (!_expected && (e.CurrentState == System.Data.ConnectionState.Broken || e.CurrentState == System.Data.ConnectionState.Closed))
                {
                    // Re-create connection when it's broken.
                    await Watch();
                }
            };

            await _connection.OpenAsync();
            using var command = new NpgsqlCommand("LISTEN configchange", _connection);
            await command.ExecuteNonQueryAsync();
        }

        public async Task Watch()
        {
            while (true)
            {
                try
                {
                    if (_connection == null || _connection.State != System.Data.ConnectionState.Open)
                    {
                        await InitAsync();
                        _expected = false;
                    }

                    await Task.WhenAll(_tcs.Task, _connection.WaitAsync());
                    _expected = true;
                    await _connection.CloseAsync();
                    await _callback(_tcs.Task.Result);
                }
                catch (NpgsqlException)
                {
                    // Trying to re-connect to server in every 5 seconds if not success
                    await Task.Delay(5000);
                    continue;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
                {
                    _connection.Close();
                }
            }

            _disposed = true;
        }
    }
}
