using SteamSharp;

namespace EU4AchievementChecklist.Models
{
    public class Achievement
    {
        public string Image { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Difficulty { get; set; }
        public double Percentage { get; set; }
        public string PercentageString { get { return Percentage.ToString("0.0"); } }
        public bool Achieved { get; set; }
    }
}
