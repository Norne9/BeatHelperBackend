using LiteDB;

namespace BeatHelperBackend.Data
{
    public class SongData
    {
        public ObjectId Id { get; set; }
        public string Key { get; set; }
        public string Hash { get; set; }
        public string SongName { get; set; }
        public string Uploader { get; set; }
    }
}