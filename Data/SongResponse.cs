namespace BeatHelperBackend.Data
{
    public class SongResponse
    {
        public string Key { get; set; }
        public string Hash { get; set; }
        public string SongName { get; set; }
        public string Uploader { get; set; }
        public string Difficulty { get; set; }
        public double BestScore { get; set; }
        public double WorstScore { get; set; }
    }
}