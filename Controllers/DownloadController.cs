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
        private readonly LiteDatabase _database;

        public DownloadController(ILogger<DownloadController> logger, LiteDbContext dbContext)
        {
            _logger = logger;
            _database = dbContext.Database;
        }
        
        [HttpGet]
        public IEnumerable<string> Get()
        {
            var processed = _database.GetCollection<SongData>().Query()
                .Select(s => s.Hash).ToEnumerable();

            return _database.GetCollection<Song>().Query()
                .Where(s => !processed.Contains(s.Hash))
                .Select(s => s.Hash).ToEnumerable();
        }

        [HttpPost]
        public IActionResult Post([FromBody] SongData songData)
        {
            var dataCol = _database.GetCollection<SongData>();
            var record = dataCol.Query().Where(s => s.Hash == songData.Hash).FirstOrDefault();
            if (record != null)
            {
                return new BadRequestResult();
            }

            dataCol.Insert(songData);
            return new AcceptedResult();
        }
    }
}