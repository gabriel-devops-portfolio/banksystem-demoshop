namespace DemoShop.Web.Extensions
{
    using System;
    using BankSystem.Common.Database;
    using Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public static class DatabaseServiceExtensions
    {
        public static IServiceCollection AddDemoShopDatabase(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var rdsConfig = configuration.GetSection("RdsAuthentication").Get<RdsAuthenticationConfiguration>();

            if (rdsConfig == null || string.IsNullOrEmpty(rdsConfig.RdsEndpoint))
            {
                var legacyConnectionString = configuration.GetConnectionString("DefaultConnection");
                services.AddDbContext<DemoShopDbContext>(options =>
                    options.UseSqlServer(legacyConnectionString));

                return services;
            }

            services.AddSingleton<RdsIamAuthenticationHelper>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<RdsIamAuthenticationHelper>>();
                return new RdsIamAuthenticationHelper(
                    logger,
                    rdsConfig.RdsEndpoint,
                    rdsConfig.RdsPort,
                    rdsConfig.DbUser,
                    rdsConfig.AwsRegion,
                    rdsConfig.UseIamAuthentication,
                    rdsConfig.FallbackPassword);
            });

            services.AddDbContext<DemoShopDbContext>((sp, options) =>
            {
                var helper = sp.GetRequiredService<RdsIamAuthenticationHelper>();
                var logger = sp.GetRequiredService<ILogger<DemoShopDbContext>>();

                try
                {
                    var baseConnectionString = $"Server={rdsConfig.RdsEndpoint},{rdsConfig.RdsPort};" +
                                              $"Database={rdsConfig.DatabaseName};" +
                                              $"{rdsConfig.AdditionalParameters ?? "MultipleActiveResultSets=true;Encrypt=true;TrustServerCertificate=false"}";

                    var connectionString = helper.BuildConnectionStringAsync(
                        rdsConfig.DatabaseName,
                        baseConnectionString).GetAwaiter().GetResult();

                    options.UseSqlServer(connectionString);
                    logger.LogInformation("DemoShopDbContext configured successfully");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to configure DemoShopDbContext with RDS IAM authentication");

                    if (!string.IsNullOrEmpty(rdsConfig.LegacyConnectionString))
                    {
                        logger.LogWarning("Using legacy connection string as last resort");
                        options.UseSqlServer(rdsConfig.LegacyConnectionString);
                    }
                    else
                    {
                        throw;
                    }
                }
            });

            return services;
        }
    }
}
