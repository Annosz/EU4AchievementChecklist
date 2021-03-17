using AspNet.Security.OpenId.Steam;
using EU4AchievementChecklist.Models;
using FuzzySharp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
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
        private const string _percentageCacheKey = "percentage_cache_key";
        private const string _achievedCacheKey = "achieved_cache_key";
        private readonly IMemoryCache _cache;

        private string SteamID { get; set; }

        public SteamService(IMemoryCache cache)
        {
            _cache = cache;
        }

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
            if (!_cache.TryGetValue(_percentageCacheKey, out List<SteamCommunity.GlobalAchievement> achievementPercentages))
            {
                SteamClient client = new SteamClient();
                client.Authenticator = APIKeyAuthenticator.ForProtectedResource(Environment.GetEnvironmentVariable("SteamAPIKey"));

                achievementPercentages = await SteamCommunity.GetGlobalAchievementPercentagesForAppAsync(client, 236850);

                _cache.Set(_percentageCacheKey, achievementPercentages, TimeSpan.FromHours(12));
            }

            Dictionary<string, string> achievementNameMatcher = new Dictionary<string, string>();
            foreach (var achievement in achievements)
            {
                int maxMatch = achievementPercentages.Max(ac => FuzzyMatchAPIName(ac.APIName, achievement.Name));
                achievementNameMatcher.Add(achievement.Name, achievementPercentages.FirstOrDefault(ac => FuzzyMatchAPIName(ac.APIName, achievement.Name) == maxMatch).APIName);
            }

            achievements.ForEach(a => a.Percentage = achievementPercentages.FirstOrDefault(ac => ac.APIName == achievementNameMatcher[a.Name])?.Percent ?? 0);
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

            if (!_cache.TryGetValue(String.Concat(_achievedCacheKey, "_", SteamID), out SteamCommunity.PlayerAchievements playerAchievements))
            {
                SteamClient client = new SteamClient();
                client.Authenticator = APIKeyAuthenticator.ForProtectedResource(Environment.GetEnvironmentVariable("SteamAPIKey"));

                playerAchievements = await SteamCommunity.GetPlayerAchievementsAsync(client, SteamID, 236850);

                _cache.Set(String.Concat(_achievedCacheKey, "_", SteamID), playerAchievements, TimeSpan.FromMinutes(2));
            }

            achievements.ForEach(a => a.Achieved = playerAchievements.Achievements.FirstOrDefault(ac => ac.Name == a.Name)?.IsAchieved ?? false);
        }

    }
}
