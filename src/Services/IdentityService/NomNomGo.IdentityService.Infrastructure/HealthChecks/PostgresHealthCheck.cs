using System.Data.Common;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace NomNomGo.IdentityService.Infrastructure.HealthChecks
{
    public class PostgresHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;
        private readonly int _commandTimeout;

        public PostgresHealthCheck(string connectionString, int commandTimeout = 10)
        {
            _connectionString = connectionString;
            _commandTimeout = commandTimeout;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1;";
                command.CommandTimeout = _commandTimeout;
                await command.ExecuteScalarAsync(cancellationToken);

                return HealthCheckResult.Healthy("PostgreSQL database is healthy");
            }
            catch (DbException ex)
            {
                return HealthCheckResult.Unhealthy("PostgreSQL database is unhealthy", ex);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("PostgreSQL health check failed", ex);
            }
        }
    }
}