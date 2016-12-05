using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SparrowXNA
{
    public class SPRandom
    {
        private static Random s_random = null;
        private static Random GetRandom()
        {
            if (s_random == null)
                s_random = new Random(Guid.NewGuid().GetHashCode());
            return s_random;
        }

        public static int NextRandom(int min, int max)
        {
            return GetRandom().Next(min, max + 1);
        }

        public static float NextRandom(float min, float max)
        {
            float rand = (float)GetRandom().NextDouble();
            return min + rand * (max - min);
        }

        public static int NextRandomSign()
        {
            int rand = GetRandom().Next(2);
            return rand == 0 ? -1 : 1;
        }

        public static bool NextRandomBoolean()
        {
            return NextRandomSign() == 1;
        }
    }
}
