using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace NomNomGo.IdentityService.Infrastructure.HealthChecks
{
    public class RedisHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;

        public RedisHealthCheck(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var redis = await ConnectionMultiplexer.ConnectAsync(_connectionString);
                var db = redis.GetDatabase();
                
                // Проверка с помощью пинга
                var pingResult = await db.PingAsync();
                
                return HealthCheckResult.Healthy($"Redis is healthy, ping: {pingResult.TotalMilliseconds}ms");
            }
            catch (RedisConnectionException ex)
            {
                return HealthCheckResult.Unhealthy("Redis connection failed", ex);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Redis health check failed", ex);
            }
        }
    }
}