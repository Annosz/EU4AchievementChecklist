using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Web;

namespace EU4AchievementChecklist.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login(string returnUrl)
        {
            return new ChallengeResult(
                "Steam",
                new AuthenticationProperties
                {
                    RedirectUri = returnUrl
                });
        }

        [HttpGet("signin-steam")]
        public IActionResult AfterLoginRedirection()
        {

            string steamId = HttpUtility
                .ParseQueryString(Request.QueryString.ToString())
                .Get("openid.identity")
                .Replace("https://steamcommunity.com/openid/id/", "");

            var cookieOptions = new CookieOptions { Expires = new DateTimeOffset(DateTime.Now.AddDays(1)) };

            HttpContext.Response.Cookies.Append("SteamId", steamId, cookieOptions);

            return RedirectPermanent("/achievements");
        }

        //public async Task<IActionResult> LoginCallback(string returnUrl)
        //{
        //    var authenticateResult = await HttpContext.AuthenticateAsync("External");

        //    if (!authenticateResult.Succeeded)
        //        return BadRequest();

        //    var claimsIdentity = new ClaimsIdentity("Application");

        //    claimsIdentity.AddClaim(authenticateResult.Principal.FindFirst(ClaimTypes.NameIdentifier));
        //    claimsIdentity.AddClaim(authenticateResult.Principal.FindFirst(ClaimTypes.Email));

        //    await HttpContext.SignInAsync(
        //        "Application",
        //        new ClaimsPrincipal(claimsIdentity));

        //    return LocalRedirect(returnUrl);
        //}
    }
}
