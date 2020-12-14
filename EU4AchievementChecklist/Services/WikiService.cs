using EU4AchievementChecklist.Models;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace EU4AchievementChecklist.Services
{
    public class WikiService
    {
        public async Task<List<Achievement>> CreateWikiPart()
        {
            List<Achievement> Achievements = new List<Achievement>();

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
                                    achievement.Image = cell.SelectSingleNode(".//img").Attributes["src"].Value;
                                    achievement.Name = cell.SelectSingleNode(".//div[@style='font-weight: bold; font-size:larger;']").InnerText;
                                    achievement.Description = cell.SelectSingleNode(".//div[@style='line-height: 1.2em; font-style: italic; font-size:smaller;']").InnerText;
                                    break;
                                case (1):
                                case (2):
                                case (3):
                                case (4):
                                case (5):
                                case (6):
                                    achievement.Difficulty = cell.InnerText;
                                    break;
                                default:
                                    break;
                            }
                        }

                        Achievements.Add(achievement);
                    }
                }
            }

            return Achievements;
        }
    }
}
