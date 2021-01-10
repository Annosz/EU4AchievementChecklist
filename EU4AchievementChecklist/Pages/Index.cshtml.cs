using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EU4AchievementChecklist.Models;
using EU4AchievementChecklist.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EU4AchievementChecklist.Pages
{
    public class IndexModel : PageModel
    {
        public static List<string> DifficultyOrder = new List<string>() { "VE", "E", "M", "H", "VH", "I" };

        public WikiService _wiki { get; set; }
        public SteamService _steam { get; set; }
        public List<Achievement> Achievements { get; set; } = new List<Achievement>();

        public bool SteamSignedIn { get; set; } = false;

        public string NameSort { get; set; }
        public string DescriptionSort { get; set; }
        public string DifficultySort { get; set; }
        public string PercentageSort { get; set; }
        public string AchievedSort { get; set; }
        public string CurrentSort { get; set; }

        public IndexModel(WikiService wiki, SteamService steam)
        {
            _wiki = wiki;
            _steam = steam;
        }

        public async Task OnGetAsync(string sortOrder)
        {
            Achievements = await _wiki.CreateWikiPart();

            await _steam.AttachPercentageToAchievements(Achievements);

            if (SteamSignedIn = await _steam.Authenticate(HttpContext))
            {
                 await _steam.AttachCompletionToAchievements(Achievements);
            }

            OrderAchievements(sortOrder);
        }

        private void OrderAchievements(string sortOrder)
        {
            CurrentSort = sortOrder;

            NameSort = String.IsNullOrEmpty(sortOrder) ? "Name_desc" : "Name";
            DescriptionSort = sortOrder == "Description" ? "Description_desc" : "Description";
            DifficultySort = sortOrder == "Difficulty" ? "Difficulty_desc" : "Difficulty";
            PercentageSort = sortOrder == "Percentage" ? "Percentage_desc" : "Percentage";
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
                    Achievements = Achievements.OrderBy(a => DifficultyOrder.IndexOf(a.Difficulty)).ToList();
                    break;
                case "Difficulty_desc":
                    Achievements = Achievements.OrderByDescending(a => DifficultyOrder.IndexOf(a.Difficulty)).ToList();
                    break;
                case "Percentage":
                    Achievements = Achievements.OrderBy(a => a.Percentage).ToList();
                    break;
                case "Percentage_desc":
                    Achievements = Achievements.OrderByDescending(a => a.Percentage).ToList();
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
    }
}
