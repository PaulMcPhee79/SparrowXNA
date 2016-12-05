using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace SparrowXNA
{
    public class SPParticleBehavior
    {
        public enum ParticleBehaviorType { Displacement, RadialDisplacement, Rotation, Alpha, Scale };

        public const int kBitmapOptionIgnoresModFactor = 1 << 0;
        public const int kBitmapOptionLockXYScale = 1 << 1;

        public SPParticleBehavior(ParticleBehaviorType type, string name = null)
        {
            mType = type;
            mName = name;
        }

        #region Fields
        private string mName;
        private ParticleBehaviorType mType;
        private int bitmapOptions;
        private double delay;
        private double duration;
        public Vector2 delta = new Vector2();
        public Vector2 deltaAccel = new Vector2();
        #endregion

        #region Properties
        public ParticleBehaviorType Type { get { return mType; } }
        public string Name { get { return mName; } }
        public bool IsDelayed { get { return delay > 0; } }
        public int BitmapOptions { get { return bitmapOptions; } private set { bitmapOptions = value; } }
        public double Delay { get { return delay; } set { delay = Math.Max(0, value); } }
        public double Duration { get { return duration; } set { duration = Math.Max(0, value); } }
        #endregion

        #region Methods
        public static void SetOption(int bitmapOption, SPParticleBehavior behavior)
        {
            if (behavior != null)
                behavior.BitmapOptions |= bitmapOption;
        }

        public static bool IsOptionSet(int bitmapOption, SPParticleBehavior behavior)
        {
            return behavior != null && (behavior.BitmapOptions & bitmapOption) == bitmapOption;
        }

        public float RatioApplied(double passedTime)
        {
            if (duration == 0)
                return 0;
            else
            {
                double durationActive = Math.Min(duration, Math.Max(0, passedTime - delay));
                return (float)(durationActive / duration);
            }
        }

        public virtual Vector2 AcceleratedDelta(double passedTime)
        {
            float durationActive = (float)Math.Min(duration, Math.Max(0, passedTime - delay));

            if (durationActive > 0)
                return new Vector2(delta.X + deltaAccel.X * durationActive, delta.Y + deltaAccel.Y * durationActive);
            else
                return new Vector2(0, 0);
        }
        #endregion
    }
}
