using System;
using BeatHelperBackend.Options;
using LiteDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BeatHelperBackend.Data
{
    public class LiteDbContext
    {
        private readonly string _connectionString;

        public LiteDbContext(IOptions<LiteDbOptions> options)
        {
            _connectionString = options.Value.ConnectionString;
        }

        public LiteDatabase Open() => new LiteDatabase(_connectionString);
    }
}