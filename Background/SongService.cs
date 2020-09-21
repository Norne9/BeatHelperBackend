using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BeatHelperBackend.Data;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace BeatHelperBackend.Background
{
    public class SongService : BackgroundService
    {
        private readonly ILogger<SongService> _logger;
        private readonly LiteDatabase _database;

        public SongService(ILogger<SongService> logger, LiteDbContext dbContext)
        {
            _database = dbContext.Database;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //https://beatsaver.com/api/search/text/0?q=E7F3AB4CEA241D3762BA28713B7F6CA5F4E5A1B5&?automapper=1
                
                List<string> hashes = new List<string>();
                var songCol = _database.GetCollection<SongData>("songs");
                
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}