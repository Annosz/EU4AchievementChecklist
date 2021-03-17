using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        private WikiService _wiki { get; set; }

        public ImageController(WikiService wiki)
        {
            _wiki = wiki;
        }

        [HttpGet("{name}")]
        [ResponseCache(Duration = 7200)]
        public async Task<FileContentResult> GetAsync(string name)
        {
            List<Achievement> Achievements = await _wiki.CreateWikiPart();

            int maxMatch = Achievements.Max(a => FuzzyMatchImageName(name, a.Name));
            byte[] image = Achievements.Find(a => FuzzyMatchImageName(name, a.Name) == maxMatch)?.Image;
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
