using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EU4AchievementChecklist.Helpers.Misc;
using EU4AchievementChecklist.Models;
using EU4AchievementChecklist.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EU4AchievementChecklist.Pages
{
    public class IndexModel : PageModel
    {
        private WikiService _wiki { get; set; }
        private SteamService _steam { get; set; }
        public List<Achievement> Achievements { get; set; } = new List<Achievement>();

        public bool SteamSignedIn { get; set; } = false;

        public string Sort { get; set; }
        public string NameSort { get; set; }
        public string DescriptionSort { get; set; }
        public string DifficultySort { get; set; }
        public string VersionSort { get; set; }
        public string PercentageSort { get; set; }
        public string AchievedSort { get; set; }

        public string Version { get; set; }
        public List<SelectListItem> VersionFilterList { get; set; }
        public string Difficulty { get; set; }
        public List<SelectListItem> DifficultyFilterList { get; set; }
        public bool? Achieved { get; set; }
        public List<SelectListItem> AchievedFilterList { get; set; }

        public IndexModel(WikiService wiki, SteamService steam)
        {
            _wiki = wiki;
            _steam = steam;
        }

        public async Task OnGetAsync(string sort, string version, string difficulty, bool? achieved)
        {
            Achievements = await _wiki.CreateWikiPart();

            await _steam.AttachPercentageToAchievements(Achievements);
            if (SteamSignedIn = await _steam.Authenticate(HttpContext))
            {
                await _steam.AttachCompletionToAchievements(Achievements);
            }

            FilterAchievements(version, difficulty, achieved);
            SortAchievements(sort);
        }

        private void FilterAchievements(string version, string difficulty, bool? achieved)
        {
            // Version filtering
            Version = version;

            VersionFilterList = Achievements.Select(a => a.Version).Distinct()
                .OrderBy(v => v, new Comparers.GameVersionComparer())
                .Select(v => new SelectListItem
                {
                    Value = v,
                    Text = v
                }).ToList();

            if (!string.IsNullOrEmpty(Version))
            {
                var versionComparer = new Comparers.GameVersionComparer();
                Achievements = Achievements.Where(a => versionComparer.Compare(a.Version, Version) <= 0).ToList();
            }


            // Difficulty filtering
            Difficulty = difficulty;

            DifficultyFilterList = Achievements.Select(a => a.Difficulty).Distinct()
                .OrderBy(d => d, new Comparers.DifficultyComparer())
                .Select(d => new SelectListItem
                {
                    Value = d,
                    Text = d
                }).ToList();

            if (!string.IsNullOrEmpty(Difficulty))
            {
                var difficultyComparer = new Comparers.DifficultyComparer();
                Achievements = Achievements.Where(a => difficultyComparer.Compare(a.Difficulty, Difficulty) <= 0).ToList();
            }


            // Difficulty filtering
            Achieved = achieved;

            AchievedFilterList = new List<SelectListItem>()
            {
                new SelectListItem() { Value = "True", Text = "achieved" },
                new SelectListItem() { Value = "False", Text = "not achieved" }
            };

            if (Achieved.HasValue)
            {
                Achievements = Achievements.Where(a => (Achieved.Value && a.Achieved) || (!Achieved.Value && !a.Achieved)).ToList();
            }
        }

        private void SortAchievements(string sort)
        {
            Sort = sort;

            NameSort = String.IsNullOrEmpty(Sort) ? "Name_desc" : "Name";
            DescriptionSort = Sort == "Description" ? "Description_desc" : "Description";
            DifficultySort = Sort == "Difficulty" ? "Difficulty_desc" : "Difficulty";
            VersionSort = Sort == "Version" ? "Version_desc" : "Version";
            PercentageSort = Sort == "Percentage" ? "Percentage_desc" : "Percentage";
            AchievedSort = Sort == "Achieved" ? "Achieved_desc" : "Achieved";

            switch (Sort)
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
                    Achievements = Achievements.OrderBy(a => a.Difficulty, new Comparers.DifficultyComparer()).ToList();
                    break;
                case "Difficulty_desc":
                    Achievements = Achievements.OrderByDescending(a => a.Difficulty, new Comparers.DifficultyComparer()).ToList();
                    break;
                case "Version":
                    Achievements = Achievements.OrderBy(a => a.Version, new Comparers.GameVersionComparer()).ToList();
                    break;
                case "Version_desc":
                    Achievements = Achievements.OrderByDescending(a => a.Version, new Comparers.GameVersionComparer()).ToList();
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
