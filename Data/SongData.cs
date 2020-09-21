using System;
using LiteDB;

namespace BeatHelperBackend.Data
{
    public class SongData
    {
        public ObjectId Id { get; set; }
        public string Hash { get; set; }
        public string Name { get; set; }
        public string Difficulty { get; set; }
        public string DownloadUrl { get; set; }
        public double BestScore { get; set; }
        public double WorstScore { get; set; }
        public double ScoreToPass { get; set; }
        public DateTime LastFounded { get; set; }
    }
}