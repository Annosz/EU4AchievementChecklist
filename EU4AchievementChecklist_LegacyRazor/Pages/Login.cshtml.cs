using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EU4AchievementChecklist.Pages
{
    public class LoginModel : PageModel
    {
        public IActionResult OnGet(string sort, string version, string difficulty, string achieved)
        {
            return Challenge(new AuthenticationProperties { RedirectUri = Url.Page("/Index", pageHandler: null, values: new { sort, version, difficulty, achieved }, protocol: Request.Scheme) }, "Steam");
        }
    }
}
