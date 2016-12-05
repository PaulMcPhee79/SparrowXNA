using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SparrowXNA
{
    public static class SPMacros
    {
        public const float SP_FLOAT_EPSILON = 0.0001f;
        public const int SP_NOT_FOUND = -1;
        public const int SP_MAX_DISPLAY_TREE_DEPTH = 16;
        public const float PI = 3.14159265359f;
        public const float PI_HALF = 1.57079632679f;
        public const float TWO_PI = 6.28318530718f;

        public static float SP_R2D(float rad)
        {
            return (rad / (float)Math.PI * 180.0f);
        }

        public static float SP_D2R(float deg)
        {
            return (deg / 180.0f * (float)Math.PI);
        }

        public static bool SP_IS_FLOAT_EQUAL(float f1, float f2)
        {
            return (Math.Abs(f1 - f2) < SP_FLOAT_EPSILON);
        }

        public static bool SP_IS_DOUBLE_EQUAL(double d1, double d2)
        {
            return (Math.Abs(d1 - d2) < SP_FLOAT_EPSILON);
        }

        public static int SP_COLOR_PART_ALPHA(int color) { return ((color >> 24) & 0xff); }
        public static int SP_COLOR_PART_RED(int color) { return ((color >> 16) & 0xff); }
        public static int SP_COLOR_PART_GREEN(int color) { return ((color >> 8) & 0xff); }
        public static int SP_COLOR_PART_BLUE(int color) { return (color & 0xff); }
    }
}
