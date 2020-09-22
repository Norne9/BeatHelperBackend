using System.Collections.Generic;
using System.Linq;
using BeatHelperBackend.Data;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BeatHelperBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SongController : ControllerBase
    {
        private readonly ILogger<SongController> _logger;
        private readonly LiteDatabase _database;

        public SongController(ILogger<SongController> logger, LiteDbContext dbContext)
        {
            _logger = logger;
            _database = dbContext.Database;
        }

        public IEnumerable<SongResponse> Get([FromQuery] double avgScore, [FromQuery] double minScore)
        {
            _logger.LogTrace($"Request [{avgScore} | {minScore}]");
            var songCol = _database.GetCollection<Song>();
            var dataCol = _database.GetCollection<SongData>();
            
            var goodSongs = songCol.Query()
                .Where(s => s.ScoreToPass < avgScore && s.BestScore > minScore)
                .ToEnumerable();

            return from song in goodSongs
                let data = dataCol.Query()
                    .Where(s => s.Hash == song.Hash)
                    .FirstOrDefault()
                where data != null
                select new SongResponse
                {
                    Key = data.Key,
                    Hash = song.Hash,
                    SongName = data.SongName,
                    Uploader = data.Uploader,
                    Difficulty = song.Difficulty,
                    BestScore = song.BestScore,
                    WorstScore = song.WorstScore
                };
        }
    }
}