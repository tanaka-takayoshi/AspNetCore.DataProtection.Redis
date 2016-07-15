using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AspNetCore.DataProtection.Redis
{
    public static class DataProtectionBuilderExtensions
    {
        /// <param name="redisConnectionString">
        /// The Redis connection string. e.g. "localhost", "server1:6379", "server1:6379,password=password,ConnectTimeout=10000"
        /// </param>
        public static IDataProtectionBuilder PersistKeysToRedis(this IDataProtectionBuilder builder, string redisConnectionString)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (redisConnectionString == null)
            {
                throw new ArgumentNullException(nameof(redisConnectionString));
            }

            if (redisConnectionString.Length == 0)
            {
                throw new ArgumentException("Redis connection string may not be empty.", nameof(redisConnectionString));
            }

            return builder.Use(ServiceDescriptor.Singleton<IXmlRepository>(services => new RedisXmlRepository(redisConnectionString, services.GetRequiredService<ILogger<RedisXmlRepository>>())));
        }

        public static IDataProtectionBuilder Use(this IDataProtectionBuilder builder, ServiceDescriptor descriptor)
        {
            // This algorithm of removing all other services of a specific type
            // before adding the new/replacement service is how the base ASP.NET
            // DataProtection bits work. Due to some of the differences in how
            // that base set of bits handles DI, it's better to follow suit
            // and work in the same way than to try and debug weird issues.
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            for (int i = builder.Services.Count - 1; i >= 0; i--)
            {
                if (builder.Services[i]?.ServiceType == descriptor.ServiceType)
                {
                    builder.Services.RemoveAt(i);
                }
            }

            builder.Services.Add(descriptor);
            return builder;
        }
    }
}
