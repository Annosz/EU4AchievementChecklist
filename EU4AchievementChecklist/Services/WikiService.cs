using EU4AchievementChecklist.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EU4AchievementChecklist.Services
{
    public class WikiService
    {
        private const string _wikiCacheKey = "wiki_cache_key";
        private const string _imageCacheKey = "image_cache_key";
        private readonly IMemoryCache _cache;

        public WikiService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<List<Achievement>> CreateWikiPart()
        {
            if (_cache.TryGetValue(_wikiCacheKey, out List<Achievement> Achievements))
            {
                return Achievements;
            }

            Achievements = new List<Achievement>();

            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync("https://eu4.paradoxwikis.com/Achievements"))
            using (HttpContent content = response.Content)
            {
                string htmlString = await content.ReadAsStringAsync();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(htmlString);

                foreach (HtmlNode table in doc.DocumentNode.SelectNodes("//table[contains(@class, 'sortable')]"))
                {
                    foreach (HtmlNode row in table.SelectNodes(".//tr[position()>1]"))
                    {
                        Achievement achievement = new Achievement();

                        HtmlNodeCollection htmlNodes = row.SelectNodes("td");
                        for (int i = 0; i < htmlNodes.Count; i++)
                        {
                            HtmlNode cell = htmlNodes[i];

                            switch (i)
                            {
                                case (0):
                                    string imageNode = cell.SelectSingleNode(".//img").Attributes["src"].Value;
                                    achievement.ImageName = imageNode.Substring(imageNode.LastIndexOf('/') + 1);
                                    achievement.Image = await GetImage(imageNode);
                                    achievement.Name = cell.SelectSingleNode(".//div[@style='font-weight: bold; font-size:larger;']").InnerText;
                                    achievement.Description = cell.SelectSingleNode(".//div[@style='line-height: 1.2em; font-style: italic; font-size:smaller;']").InnerText;
                                    break;
                                case (1):
                                case (2):
                                case (3):
                                case (4):
                                case (5):
                                    achievement.Version = Regex.Replace(cell.InnerText, @"\s+", "");
                                    break;
                                case (6):
                                    achievement.Difficulty = Regex.Replace(cell.InnerText, @"\s+", "");
                                    break;
                                default:
                                    break;
                            }
                        }

                        Achievements.Add(achievement);
                    }
                }
            }

            _cache.Set(_wikiCacheKey, Achievements, TimeSpan.FromDays(3));

            return Achievements;
        }

        private async Task<byte[]> GetImage(string fileName)
        {
            if (_cache.TryGetValue(_imageCacheKey, out byte[] image))
            {
                return image;
            }

            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(string.Concat("https://eu4.paradoxwikis.com", fileName)))
            using (HttpContent content = response.Content)
            {
                image = await content.ReadAsByteArrayAsync();

                _cache.Set(string.Concat(_imageCacheKey, "_", fileName), image, TimeSpan.FromDays(3650));

                return image;
            }
        }
    }
}
