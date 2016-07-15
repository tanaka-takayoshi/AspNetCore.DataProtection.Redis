# AspNetCore.DataProtection.Redis
Data Protection with Redis

This library is an Experimental implementation for ASP.NET Core Data Protection tool with Redis motivated by https://github.com/tillig/DataProtection .

## How to Use

Add ``Tanaka733.AspNetCore.DataProtection.Redis`` and other dependencies.
```
{
  "dependencies": {
    "Microsoft.AspNetCore.DataProtection": "1.0.0",
    "Tanaka733.AspNetCore.DataProtection.Redis": "0.1.2-alpha",
    "Microsoft.AspNetCore.Session": "1.0.0",
    "Ngonzalez.Microsoft.Extensions.Caching.Redis": "1.0.2",
    "Microsoft.Extensions.Logging.Console": "1.0.0",
    "Microsoft.Extensions.Logging.Debug": "1.0.0"
  },

  "frameworks": {
    "netcoreapp1.0": {
      "imports": [
        "dotnet5.6",
        "portable-net45+win8"
      ]
    }
  },
}
```

In Startup.cs
```
public void ConfigureServices(IServiceCollection services)
{
    var host = Environment.GetEnvironmentVariable("REDIS_HOST"); //retrieve Redis host from env or other way.
    services.AddDataProtection()  
        .PersistKeysToRedis(host);
        
    // When you use session
    services.AddSession();
    
    // When you also use distributecache in Redis
    services.AddDistributedRedisCache(options =>
     {
         options.Configuration = host;
     });
}
```
