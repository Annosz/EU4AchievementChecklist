using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using AspNet.Security.OpenId.Steam;
using EU4AchievementChecklist.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteamSharp;
using SteamSharp.Authenticators;

namespace EU4AchievementChecklist.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        public List<Achievement> Achievements { get; set; } = new List<Achievement>();

        public string SteamID { get; set; }

        public string NameSort { get; set; }
        public string DescriptionSort { get; set; }
        public string DifficultySort { get; set; }
        public string AchievedSort { get; set; }
        public string CurrentSort { get; set; }

        public IndexModel() { }

        [System.Web.Mvc.ValidateAntiForgeryToken]
        public IActionResult OnPost(string sortOrder)
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, "Steam");
        }

        [System.Web.Mvc.ValidateAntiForgeryToken]
        public async Task OnGetAsync(string sortOrder)
        {
            await CreateWikiPart();

            var authResult = await HttpContext.AuthenticateAsync(SteamAuthenticationDefaults.AuthenticationScheme);
            if (authResult.Succeeded && User.Identity.IsAuthenticated)
            {
                SteamID = authResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier).Split("/").Last();
                await CreateSteamPart();
            }

            Order(sortOrder);
        }

        private void Order(string sortOrder)
        {
            CurrentSort = sortOrder;

            NameSort = String.IsNullOrEmpty(sortOrder) ? "Name_desc" : "Name";
            DescriptionSort = sortOrder == "Description" ? "Description_desc" : "Description";
            DifficultySort = sortOrder == "Difficulty" ? "Difficulty_desc" : "Difficulty";
            AchievedSort = sortOrder == "Achieved" ? "Achieved_desc" : "Achieved";

            switch (sortOrder)
            {
                case "Name":
                    Achievements = Achievements.OrderBy(a => a.Name).ToList();
                    break;
                case "Name_desc":
                    Achievements = Achievements.OrderByDescending(a => a.Name).ToList();
                    break;
                case "Description":
                    Achievements = Achievements.OrderBy(a => a.Description).ToList();
                    break;
                case "Description_desc":
                    Achievements = Achievements.OrderByDescending(a => a.Description).ToList();
                    break;
                case "Difficulty":
                    Achievements = Achievements.OrderBy(a => a.Difficulty).ToList();
                    break;
                case "Difficulty_desc":
                    Achievements = Achievements.OrderByDescending(a => a.Difficulty).ToList();
                    break;
                case "Achieved":
                    Achievements = Achievements.OrderBy(a => a.Achieved).ToList();
                    break;
                case "Achieved_desc":
                    Achievements = Achievements.OrderByDescending(a => a.Achieved).ToList();
                    break;
                default:
                    Achievements = Achievements.OrderBy(a => a.Name).ToList();
                    break;
            }
        }

        private async Task CreateWikiPart()
        {
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
        }

        private async Task CreateSteamPart()
        {
            if (string.IsNullOrEmpty(SteamID))
                return;

            SteamClient client = new SteamClient();
            client.Authenticator = APIKeyAuthenticator.ForProtectedResource(Environment.GetEnvironmentVariable("SteamAPIKey"));

            SteamCommunity.PlayerAchievements response = SteamCommunity.GetPlayerAchievements(client, SteamID, 236850);

            Achievements.ForEach(a => a.Achieved = response.Achievements.FirstOrDefault(ac => ac.Name == a.Name)?.IsAchieved ?? false);
        }
    }
}
