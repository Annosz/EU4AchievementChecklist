using AspNet.Security.OpenId.Steam;
using EU4AchievementChecklist.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using SteamSharp;
using SteamSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

        public async Task AttachCompletionToAchievements(List<Achievement> achievements)
        {
            if (string.IsNullOrEmpty(SteamID))
                return;

            SteamClient client = new SteamClient();
            client.Authenticator = APIKeyAuthenticator.ForProtectedResource(Environment.GetEnvironmentVariable("SteamAPIKey"));

            SteamCommunity.PlayerAchievements response = SteamCommunity.GetPlayerAchievements(client, SteamID, 236850);

            achievements.ForEach(a => a.Achieved = response.Achievements.FirstOrDefault(ac => ac.Name == a.Name)?.IsAchieved ?? false);
        }

    }
}
