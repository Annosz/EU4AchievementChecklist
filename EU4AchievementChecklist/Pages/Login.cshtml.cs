using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EU4AchievementChecklist.Pages
{
    public class LoginModel : PageModel
    {
        public IActionResult OnGet(string sortOrder)
        {
            return Challenge(new AuthenticationProperties { RedirectUri = Url.Page("/Index", pageHandler: null, values: new { sortOrder }, protocol: Request.Scheme) }, "Steam");
        }
    }
}
