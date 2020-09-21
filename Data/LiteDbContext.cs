using System;
using BeatHelperBackend.Options;
using LiteDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BeatHelperBackend.Data
{
    public class LiteDbContext : IDisposable
    {
        private readonly ILogger<LiteDbContext> _logger;
        public LiteDatabase Database { get; }

        public LiteDbContext(IOptions<LiteDbOptions> options, ILogger<LiteDbContext> logger)
        {
            Database = new LiteDatabase(options.Value.ConnectionString);
            _logger = logger;
            _logger.LogInformation("Database opened!");
        }

        public void Dispose()
        {
            Database?.Dispose();
            _logger.LogInformation("Database closed!");
        }
    }
}