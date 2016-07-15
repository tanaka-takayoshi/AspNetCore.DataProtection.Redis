using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AspNetCore.DataProtection.Redis
{
    public class RedisXmlRepository : IXmlRepository, IDisposable
    {
        
        public static readonly string RedisHashKey = "DataProtectionXmlRepository";
        private bool disposed = false;
        private IConnectionMultiplexer connection;

        public ILogger<RedisXmlRepository> Logger { get; }

        public RedisXmlRepository(string connectionString, ILogger<RedisXmlRepository> logger)
            : this(ConnectionMultiplexer.Connect(connectionString), logger)
        {
        }

        public RedisXmlRepository(IConnectionMultiplexer connection, ILogger<RedisXmlRepository> logger)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.connection = connection;
            this.Logger = logger;

            // Mask the password so it doesn't get logged.
            var configuration = Regex.Replace(this.connection.Configuration, @"password\s*=\s*[^,]*", "password=****", RegexOptions.IgnoreCase);
            this.Logger.LogDebug("Storing data protection keys in Redis: {RedisConfiguration}", configuration);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            var database = this.connection.GetDatabase();
            var hash = database.HashGetAll(RedisHashKey);
            var elements = new List<XElement>();

            if (hash == null || hash.Length == 0)
            {
                return elements.AsReadOnly();
            }

            foreach (var item in hash.ToStringDictionary())
            {
                elements.Add(XElement.Parse(item.Value));
            }

            this.Logger.LogDebug("Read {XmlElementCount} XML elements from Redis.", elements.Count);
            return elements.AsReadOnly();
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if (string.IsNullOrEmpty(friendlyName))
            {
                // The framework always passes in a name, but
                // the contract indicates this may be null or empty.
                friendlyName = Guid.NewGuid().ToString();
            }

            this.Logger.LogDebug("Storing XML element with friendly name {XmlElementFriendlyName}.", friendlyName);

            this.connection.GetDatabase().HashSet(RedisHashKey, friendlyName, element.ToString());
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.connection != null)
                    {
                        this.connection.Close();
                        this.connection.Dispose();
                    }
                }

                this.connection = null;
                this.disposed = true;
            }
        }
    }
}
