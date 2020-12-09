using SteamSharp;

namespace EU4AchievementChecklist.Models
{
    public class Achievement
    {
        public string Image { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Difficulty { get; set; }
        public bool Achieved { get; set; }
    }
}
