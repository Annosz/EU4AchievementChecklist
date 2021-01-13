using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EU4AchievementChecklist.Helpers.Misc
{
    public static class Comparers
    {
        public static List<string> DifficultyOrder = new List<string>() { "VE", "E", "M", "H", "VH", "I" };

        public class GameVersionComparer : IComparer<string>
        {
            public int Compare(string ver1, string ver2)
            {
                if (ver1.Length.CompareTo(ver2.Length) != 0)
                    return ver1.Length.CompareTo(ver2.Length);

                return double.Parse(ver1.Replace(".", ",")).CompareTo(double.Parse(ver2.Replace(".", ",")));
            }
        }

        public class DifficultyComparer : IComparer<string>
        {
            public int Compare(string diff1, string diff2)
            {
                return DifficultyOrder.IndexOf(diff1).CompareTo(DifficultyOrder.IndexOf(diff2));
            }
        }
    }
}
