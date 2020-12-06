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
    public class DownloadController : ControllerBase
    {
        private readonly ILogger<DownloadController> _logger;
        private readonly LiteDbContext _database;

        public DownloadController(ILogger<DownloadController> logger, LiteDbContext dbContext)
        {
            _logger = logger;
            _database = dbContext;
        }
        
        [HttpGet]
        public IEnumerable<string> Get()
        {
            using var db = _database.Open();
            var processed = new HashSet<string>(
                db.GetCollection<SongData>().Query()
                    .Select(s => s.Hash.ToUpper()).ToEnumerable());

            var result = db.GetCollection<Song>().Query()
                .Select(s => s.Hash).ToEnumerable().Distinct()
                .Where(s => !processed.Contains(s.ToUpper()))
                .ToList();
            return result;
        }

        [HttpPost]
        public IActionResult Post([FromBody] SongData songData)
        {
            using var db = _database.Open();
            var dataCol = db.GetCollection<SongData>();
            var record = dataCol.Query().Where(s => s.Hash == songData.Hash.ToUpper()).FirstOrDefault();
            if (record != null)
            {
                return new BadRequestResult();
            }

            dataCol.Insert(songData);
            return new AcceptedResult();
        }
    }
}