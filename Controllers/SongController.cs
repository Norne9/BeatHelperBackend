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
        private readonly LiteDbContext _database;

        public SongController(ILogger<SongController> logger, LiteDbContext dbContext)
        {
            _logger = logger;
            _database = dbContext;
        }

        public IEnumerable<SongResponse> Get([FromQuery] double avgScore, [FromQuery] double minScore)
        {
            using var db = _database.Open();
            _logger.LogTrace($"Request [{avgScore} | {minScore}]");
            var songCol = db.GetCollection<Song>();
            var dataCol = db.GetCollection<SongData>();
            
            var goodSongs = songCol.Query()
                .Where(s => s.ScoreToPass < avgScore && s.WorstScore > minScore)
                .ToEnumerable();

            var songs = from song in goodSongs
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
            var songList = songs.ToList();
            return songList;
        }
    }
}