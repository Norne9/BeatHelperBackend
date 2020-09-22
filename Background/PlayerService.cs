using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BeatHelperBackend.Data;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;
using LiteDB;

namespace BeatHelperBackend.Background
{
    public class PlayerService : BackgroundService
    {
        private readonly ILogger<PlayerService> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private readonly List<int> _targetPages = GetPages();
        private readonly LiteDatabase _database;

        public PlayerService(ILogger<PlayerService> logger, IHttpClientFactory clientFactory,
            LiteDbContext dbContext)
        {
            _logger = logger;
            _clientFactory = clientFactory;
            _database = dbContext.Database;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var page in _targetPages)
                {
                    // Wait
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    
                    // Get user id & score
                    string id;
                    try
                    {
                        (id, _) = (await GetUsersOnPage(page, stoppingToken))[0];
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Failed to get page #{page}!");
                        continue;
                    }
                    
                    // Get user songs
                    List<UserSong> songs;
                    double playerScore;
                    try
                    {
                        songs = await GetUserSongs(id, stoppingToken);
                        playerScore = songs.Select(s => s.Score).OrderByDescending(s => s)
                            .Take(8).Average();
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Failed to get user #{id}!");
                        continue;
                    }
                    
                    // Put to base
                    try
                    {
                        var songCol = _database.GetCollection<Song>();
                        foreach (var song in songs)
                        {
                            var dbSong = songCol.Query()
                                .Where(s => s.Hash == song.Hash && s.Difficulty == song.Difficulty)
                                .FirstOrDefault();
                            if (dbSong == null)
                            {
                                songCol.Insert(new Song()
                                {
                                    Hash = song.Hash,
                                    Difficulty = song.Difficulty,
                                    BestScore = song.Score,
                                    WorstScore = song.Score,
                                    ScoreToPass = playerScore,
                                    LastFounded = DateTime.Now
                                });
                            }
                            else
                            {
                                dbSong.BestScore = MoveValue(dbSong.BestScore, song.Score, true);
                                dbSong.WorstScore = MoveValue(dbSong.WorstScore, song.Score, false);
                                dbSong.ScoreToPass = MoveValue(dbSong.ScoreToPass, playerScore, false);
                                dbSong.LastFounded = DateTime.Now;
                                songCol.Update(dbSong);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Failed to update database!");
                    }
                }

                // Wait a lot
                await Task.Delay(10000, stoppingToken);
            }
        }

        private async Task<List<(string id, double score)>> GetUsersOnPage(int page, CancellationToken stoppingToken)
        {
            var result = new List<(string id, double score)>();
            
            using var client = _clientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            
            var resp = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get,
                $"https://scoresaber.com/global/{page}"), stoppingToken);
            var respStr = await resp.Content.ReadAsStringAsync();
                
            var doc = new HtmlDocument();
            doc.LoadHtml(respStr);

            var playerNodes =
                doc.DocumentNode.SelectNodes("/html/body/div/div/div/div/div[2]/div/div/table/tbody/tr");
            foreach (var player in playerNodes)
            {
                var name = player.SelectSingleNode("td[3]/a").Attributes["href"]
                    .Value.Replace("/u/","");
                var scoreText = player.SelectSingleNode("td[4]/span[1]")
                    .InnerText.Replace(",", "").Trim();
                
                var score = double.Parse(scoreText, CultureInfo.InvariantCulture);
                result.Add((name, score));
            }

            return result;
        }

        private async Task<List<UserSong>> GetUserSongs(string userId, CancellationToken stoppingToken)
        {
            var result = new List<UserSong>();
            
            using var client = _clientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            for (var i = 1; i <= 5; i++)
            {
                var resp = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get,
                    $"https://scoresaber.com/u/{userId}&page={i}&sort=1"), stoppingToken);
                var respStr = await resp.Content.ReadAsStringAsync();
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
                
                var doc = new HtmlDocument();
                doc.LoadHtml(respStr);

                var songs = doc.DocumentNode
                    .SelectNodes("/html/body/div/div/div/div[2]/div[2]/table/tbody/tr");
                foreach (var song in songs)
                {
                    var hash = song.SelectSingleNode("th[2]/div/div[1]/img")
                        .Attributes["src"].Value.Split('/').Last().Split('.').First();
                    var name = song.SelectSingleNode("th[2]/div/div[2]/a/span[1]").InnerText;
                    var difficulty = name.Split(' ').Last();
                    var scoreText = song.SelectSingleNode("th[3]/span[1]").InnerText;
                    var score = double.Parse(scoreText, CultureInfo.InvariantCulture);
                    
                    result.Add(new UserSong()
                    {
                        Hash = hash.ToUpper(),
                        Difficulty = difficulty,
                        Score = score
                    });
                }
            }

            return result;
        }
        
        private static List<int> GetPages()
        {
            var result = new List<int>();
            for (var i = 1; i < 10; i++)
                result.Add(i);
            for (var i = 10; i < 100; i += 5)
                result.Add(i);
            for (var i = 200; i < 400; i += 20)
                result.Add(i);
            return result;
        }

        private static double MoveValue(double from, double to, bool prefferUp)
        {
            const double low = 0.001;
            const double high = 0.01;

            double speed;
            if (prefferUp)
                speed = to > from ? high : low;
            else
                speed = to < from ? high : low;

            return to * speed + from * (1.0 - speed);
        }
    }
}