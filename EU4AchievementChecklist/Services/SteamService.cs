using AspNet.Security.OpenId.Steam;
using EU4AchievementChecklist.Models;
using FuzzySharp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using SteamSharp;
using SteamSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EU4AchievementChecklist.Services
{
    public class SteamService
    {
        private string SteamID { get; set; }

        public async Task<bool> Authenticate(HttpContext httpContext)
        {
            var authResult = await httpContext.AuthenticateAsync(SteamAuthenticationDefaults.AuthenticationScheme);
            if (authResult.Succeeded)
            {
                SteamID = authResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier).Split("/").Last();
                return true;
            }

            return false;
        }

        public async Task AttachPercentageToAchievements(List<Achievement> achievements)
        {
            SteamClient client = new SteamClient();
            client.Authenticator = APIKeyAuthenticator.ForProtectedResource(Environment.GetEnvironmentVariable("SteamAPIKey"));

            List<SteamCommunity.GlobalAchievement> response = await SteamCommunity.GetGlobalAchievementPercentagesForAppAsync(client, 236850);

            Dictionary<string, string> achievementNameMatcher = new Dictionary<string, string>();
            foreach (var achievement in achievements)
            {
                int maxMatch = response.Max(ac => FuzzyMatchAPIName(ac.APIName, achievement.Name));
                achievementNameMatcher.Add(achievement.Name, response.FirstOrDefault(ac => FuzzyMatchAPIName(ac.APIName, achievement.Name) == maxMatch).APIName);
            }

            achievements.ForEach(a => a.Percentage = response.FirstOrDefault(ac => ac.APIName == achievementNameMatcher[a.Name])?.Percent ?? 0);
        }

        private int FuzzyMatchAPIName(string apiName, string name)
        {
            Regex rgx = new Regex("[^a-zA-Z ]");
            string processedName = string.Concat("achievement_", rgx.Replace(name, "").Replace(" ", "_").ToLower());

            return Fuzz.Ratio(apiName, processedName);
        }

        public async Task AttachCompletionToAchievements(List<Achievement> achievements)
        {
            if (string.IsNullOrEmpty(SteamID))
                return;

            SteamClient client = new SteamClient();
            client.Authenticator = APIKeyAuthenticator.ForProtectedResource(Environment.GetEnvironmentVariable("SteamAPIKey"));

            SteamCommunity.PlayerAchievements response = await SteamCommunity.GetPlayerAchievementsAsync(client, SteamID, 236850);

            achievements.ForEach(a => a.Achieved = response.Achievements.FirstOrDefault(ac => ac.Name == a.Name)?.IsAchieved ?? false);
        }

    }
}
