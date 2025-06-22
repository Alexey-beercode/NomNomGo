using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace NomNomGo.IdentityService.Infrastructure.HealthChecks
{
    public class RabbitMQHealthCheck : IHealthCheck
    {
        private readonly string _host;
        private readonly string _virtualHost;
        private readonly string _username;
        private readonly string _password;

        public RabbitMQHealthCheck(string host, string virtualHost, string username, string password)
        {
            _host = host;
            _virtualHost = virtualHost;
            _username = username;
            _password = password;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _host,
                    VirtualHost = _virtualHost,
                    UserName = _username,
                    Password = _password,
                    RequestedHeartbeat = TimeSpan.FromSeconds(5),
                    AutomaticRecoveryEnabled = false
                };

                using var connection =await factory.CreateConnectionAsync();
                if (connection.IsOpen)
                {
                    using var channel =await connection.CreateChannelAsync();
                    return await Task.FromResult(HealthCheckResult.Healthy("RabbitMQ is healthy"));
                }
                
                return await Task.FromResult(HealthCheckResult.Degraded("RabbitMQ connection not open"));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(HealthCheckResult.Unhealthy("RabbitMQ health check failed", ex));
            }
        }
    }
}