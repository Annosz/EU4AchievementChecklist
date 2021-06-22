using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EU4AchievementChecklist.Models;
using EU4AchievementChecklist.Services;
using FuzzySharp;
using Microsoft.AspNetCore.Mvc;

namespace EU4AchievementChecklist.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly WikiService _wiki;

        public ImageController(WikiService wiki)
        {
            _wiki = wiki;
        }

        [HttpGet("{name}")]
        [ResponseCache(Duration = 7200)]
        public async Task<FileContentResult> GetAsync(string name)
        {
            List<Achievement> achievements = await _wiki.CreateWikiPart();

            int maxMatch = achievements.Max(a => FuzzyMatchImageName(name, a.Name));
            byte[] image = achievements.Find(a => FuzzyMatchImageName(name, a.Name) == maxMatch)?.Image;
            return image != null
                ? new FileContentResult(image, "image/jpeg")
                : null;
        }

        private int FuzzyMatchImageName(string imageName, string name)
        {
            string decodedName = imageName.Replace("_", " ").Substring(0, imageName.Length - 4);

            return Fuzz.Ratio(decodedName, name);
        }
    }
}
