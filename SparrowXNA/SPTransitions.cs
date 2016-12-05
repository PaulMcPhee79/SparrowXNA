using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Easing functions from http://dojotoolkit.org and http://www.robertpenner.com/easing

namespace SparrowXNA
{
    public class SPTransitions
    {
        public const string SPLinear = "Linear";
        public const string SPRandomize = "Randomize";

        public const string SPEaseInLinear = "EaseInLinear";
        public const string SPEaseIn = "EaseIn";
        public const string SPEaseOut = "EaseOut";
        public const string SPEaseInOut = "EaseInOut";
        public const string SPEaseOutIn = "EaseOutIn";

        public const string SPEaseInBack = "EaseInBack";
        public const string SPEaseOutBack = "EaseOutBack";
        public const string SPEaseInOutBack = "EaseInOutBack";
        public const string SPEaseOutInBack = "EaseOutInBack";

        public const string SPEaseInElastic = "EaseInElastic";
        public const string SPEaseOutElastic = "EaseOutElastic";
        public const string SPEaseInOutElastic = "EaseInOutElastic";
        public const string SPEaseOutInElastic = "EaseOutInElastic";

        public const string SPEaseInBounce = "EaseInBounce";
        public const string SPEaseOutBounce = "EaseOutBounce";
        public const string SPEaseInOutBounce = "EaseInOutBounce";
        public const string SPEaseOutInBounce = "EaseOutInBounce";

        private static Random rng = new Random();

        public static float Linear(float ratio) { return ratio; }
        public static float Randomize(float ratio) { return (float)rng.NextDouble(); }

        public static float EaseInLinear(float ratio) { return ratio * ratio; }
        public static float EaseIn(float ratio) { return ratio * ratio * ratio; }
        public static float EaseOut(float ratio) { float invRatio = ratio - 1.0f; return invRatio * invRatio * invRatio + 1.0f; }
        public static float EaseInOut(float ratio) { return ((ratio < 0.5f) ? 0.5f * SPTransitions.EaseIn(ratio * 2.0f) : 0.5f * SPTransitions.EaseOut((ratio - 0.5f) * 2.0f) + 0.5f); }
        public static float EaseOutIn(float ratio) { return ((ratio < 0.5f) ? 0.5f * SPTransitions.EaseOut(ratio * 2.0f) : 0.5f * SPTransitions.EaseIn((ratio - 0.5f) * 2.0f) + 0.5f); }

        public static float EaseInBack(float ratio) { float s = 1.70158f; return (float)Math.Pow((double)ratio, 2.0) * ((s + 1.0f) * ratio - s); }
        public static float EaseOutBack(float ratio) { float invRatio = ratio - 1.0f, s = 1.70158f; return (float)Math.Pow((double)invRatio, 2.0) * ((s + 1.0f) * invRatio + s) + 1.0f; }
        public static float EaseInOutBack(float ratio) { return ((ratio < 0.5f) ? 0.5f * SPTransitions.EaseInBack(ratio * 2.0f) : 0.5f * SPTransitions.EaseOutBack((ratio - 0.5f) * 2.0f) + 0.5f); }
        public static float EaseOutInBack(float ratio) { return ((ratio < 0.5f) ? 0.5f * SPTransitions.EaseOutBack(ratio * 2.0f) : 0.5f * SPTransitions.EaseInBack((ratio - 0.5f) * 2.0f) + 0.5f); }

        public static float EaseInElastic(float ratio)
        {
            if (ratio == 0.0f || ratio == 1.0f) return ratio;
            else
            {
                float p = 0.3f;
                float s = p / 4.0f;
                float invRatio = ratio - 1.0f;
                return -1.0f * (float)Math.Pow(2.0f, 10.0 * invRatio) * (float)Math.Sin((invRatio - s) * Math.PI / p);
            }
        }

        public static float EaseOutElastic(float ratio)
        {
            if (ratio == 0.0f || ratio == 1.0f) return ratio;
            else
            {
                float p = 0.3f;
                float s = p / 4.0f;
                return -1.0f * (float)Math.Pow(2.0f, -10.0 * ratio) * (float)Math.Sin((ratio - s) * Math.PI / p) + 1.0f;
            }
        }

        public static float EaseInOutElastic(float ratio)
        {
            return (ratio < 0.5f) ? 0.5f * SPTransitions.EaseInElastic(ratio * 2.0f) : 0.5f * SPTransitions.EaseOutElastic((ratio - 0.5f) * 2.0f) + 0.5f;
        }

        public static float EaseOutInElastic(float ratio)
        {
            return (ratio < 0.5f) ? 0.5f * SPTransitions.EaseOutElastic(ratio * 2.0f) : 0.5f * SPTransitions.EaseInElastic((ratio - 0.5f) * 2.0f) + 0.5f;
        }

        public static float EaseInBounce(float ratio)
        {
            return 1.0f - SPTransitions.EaseOutBounce(1.0f - ratio);
        }

        public static float EaseOutBounce(float ratio)
        {
            float s = 7.5625f;
            float p = 2.75f;
            float l;

            if (ratio < (1.0f / p))
            {
                l = s * (float)Math.Pow(ratio, 2.0);
            }
            else
            {
                if (ratio < (2.0f / p))
                {
                    ratio -= 1.5f / p;
                    l = s * (float)Math.Pow(ratio, 2.0f) + 0.75f;
                }
                else
                {
                    ratio -= 2.625f / p;
                    l = s * (float)Math.Pow(ratio, 2.0f) + 0.984375f;
                }
            }

            return l;
        }

        public static float EaseInOutBounce(float ratio)
        {
            return (ratio < 0.5f) ? 0.5f * SPTransitions.EaseInBounce(ratio * 2.0f) : 0.5f * SPTransitions.EaseOutBounce((ratio - 0.5f) * 2.0f) + 0.5f;
        }

        public static float EaseOutInBounce(float ratio)
        {
            return (ratio < 0.5f) ? 0.5f * SPTransitions.EaseOutBounce(ratio * 2.0f) : 0.5f * SPTransitions.EaseInBounce((ratio - 0.5f) * 2.0f) + 0.5f;
        }
    }
}
