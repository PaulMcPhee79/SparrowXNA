using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace SparrowXNA
{
    public struct SPSize
    {
        public SPSize(int w = 0, int h = 0)
        {
            this.W = w;
            this.H = h;
        }

        public int W;
        public int H;
    }

    public struct SPSizeF
    {
        public SPSizeF(float w = 0, float h = 0)
        {
            this.W = w;
            this.H = h;
        }

        public float W;
        public float H;
    }

    public struct SPCoord
    {
        public SPCoord(int x = 0, int y = 0)
        {
            this.X = x;
            this.Y = y;
        }

        public int X;
        public int Y;
    }

    public struct SPRange
    {
        public SPRange(float from = 0f, float to = 0f)
        {
            this.From = from;
            this.To = to;
        }

        public float From;
        public float To;
    }

    public struct SPRangeInt
    {
        public SPRangeInt(int from = 0, int to = 0)
        {
            this.From = from;
            this.To = to;
        }

        public int From;
        public int To;
    }

    public static class SPUtils
    {
        public static bool HasMethod(this object objectToCheck, string methodName)
        {
            var type = objectToCheck.GetType();
            return type.GetMethod(methodName) != null;
        }

        public static MethodInfo GetMethod(this object objectToCheck, string methodName)
        {
            var type = objectToCheck.GetType();
            return type.GetMethod(methodName);
        }

        // Clamps a radian value between -pi and pi
        public static float ClampAngle(float radians)
        {
            while (radians < -MathHelper.Pi)
            {
                radians += MathHelper.TwoPi;
            }
            while (radians > MathHelper.Pi)
            {
                radians -= MathHelper.TwoPi;
            }
            return radians;
        }

        public static int NextPowerOfTwo(int val)
        {
            int result = 1;
            while (result < val) result *= 2;
            return result;
        }

        public static void DecomposeMatrix(ref Matrix matrix, out Vector2 position, out float rotation, out Vector2 scale)
        {
            Vector3 position3, scale3;
            Quaternion rotationQ;
            matrix.Decompose(out scale3, out rotationQ, out position3);
            Vector2 direction = Vector2.Transform(Vector2.UnitX, rotationQ);
            rotation = (float)Math.Atan2(direction.Y, direction.X);
            position = new Vector2(position3.X, position3.Y);
            scale = new Vector2(scale3.X, scale3.Y);
        }

        public static Color ColorFromColor(uint color)
        {
            return new Color(SPMacros.SP_COLOR_PART_RED((int)color), SPMacros.SP_COLOR_PART_GREEN((int)color), SPMacros.SP_COLOR_PART_BLUE((int)color));
        }

        public static bool PointInPoly(Vector2 point, List<Vector2> poly)
        {
            if (poly == null)
                return false;

            bool isInPoly = false;
            int i = 0, j = 0, numVertices = poly.Count;

            for (i = 0, j = numVertices - 1; i < numVertices; j = i++)
            {
                Vector2 pi = poly[i], pj = poly[j];

                if ((((pi.Y <= point.Y) && (point.Y < pj.Y)) ||
                        ((pj.Y <= point.Y) && (point.Y < pi.Y))) &&
                        (point.X < (pj.X - pi.X) * (point.Y - pi.Y) / (pj.Y - pi.Y) + pi.X))
                    isInPoly = !isInPoly;
            }

            return isInPoly;
        }
    }
}
