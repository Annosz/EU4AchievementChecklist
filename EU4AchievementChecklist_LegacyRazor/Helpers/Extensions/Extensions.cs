using System.Text.RegularExpressions;

namespace EU4AchievementChecklist.Helpers.Extensions
{
    public static class Extensions
    {
        public static ulong GetSteamId(this string value)
        {
            if (!ulong.TryParse(Regex.Match(value, @"\d+").Value, out ulong returnValue))
            {
                returnValue = 0;
            }

            return returnValue;
        }
    }
}
