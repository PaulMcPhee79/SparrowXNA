using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace SparrowXNA
{
    public class SPParticle : SPQuad
    {
        public SPParticle(SPTexture texture, double timeToLive)
            : base(texture)
        {
            mTotalTimeToLive = mTimeToLive = timeToLive;
            mTickTime = 0;
            IsVerticesCentered = true;
        }

        #region Fields
        private int mPoolIndex = -1;
        private double mTickTime;
        private double mTimeToLive;
        private double mTotalTimeToLive;
        private Vector2 mModFactor; // Modifies output of SPParticleBehavior as it is applied to this SPParticle.
        private static Vector2 kModFactor = new Vector2(1f, 1f);
        #endregion

        #region Properties
        public int PoolIndex { get { return mPoolIndex; } set { mPoolIndex = value; } }
        public bool IsComplete { get { return TickTime == 0 && TimeToLive == 0; } }
        public double TickTime { get { return mTickTime; } }
        public double TimeLived { get { return TotalTimeToLive - TimeToLive; } }
        public double TimeToLive { get { return mTimeToLive; } set { mTimeToLive = Math.Max(0, value); } }
        public double TotalTimeToLive
        {
            get { return mTotalTimeToLive; }
            set
            {
                mTotalTimeToLive = Math.Max(0, value);
                mTimeToLive = mTotalTimeToLive;
            }
        }
        public Vector2 ModFactor { get { return mModFactor; } set { mModFactor = value; } }
        #endregion

        #region Methods
        public bool IsBehaviorActive(SPParticleBehavior pb)
        {
            return pb != null && TimeLived > pb.Delay && TimeLived < pb.Delay + pb.Duration;
        }

        public void Loop()
        {
            TimeToLive = TotalTimeToLive;
        }

        public virtual void Tick(double time)
        {
            if (IsComplete)
                return;

            mTickTime = Math.Min(mTimeToLive, time);
            mTimeToLive -= mTickTime;
        }

        
        public virtual void ApplyTick(SPParticleBehavior pb)
        {
            if (IsComplete || !IsBehaviorActive(pb))
                return;

            Visible = true;

            float deltaTime = (float)mTickTime;
            float timeFactor = deltaTime / (float)pb.Duration;
            Vector2 delta = pb.AcceleratedDelta(TimeLived);
            Vector2 modFactor = SPParticleBehavior.IsOptionSet(SPParticleBehavior.kBitmapOptionIgnoresModFactor, pb) ? kModFactor : mModFactor;

            switch (pb.Type)
            {
                case SPParticleBehavior.ParticleBehaviorType.Displacement:
                    {
                        X += deltaTime * modFactor.X * delta.X;
                        Y += deltaTime * modFactor.Y * delta.Y;
                    }
                    break;
                case SPParticleBehavior.ParticleBehaviorType.RadialDisplacement:
                    {
                        float radius = modFactor.X * delta.X, angle = modFactor.Y; // modFactor.X * delta.Y;
                        X += timeFactor * radius * (float)Math.Cos(angle);
                        Y += timeFactor * radius * (float)Math.Sin(angle);
                    }
                    break;
                case SPParticleBehavior.ParticleBehaviorType.Rotation:
                    {
                        Rotation += deltaTime * modFactor.X * delta.X;
                    }
                    break;
                case SPParticleBehavior.ParticleBehaviorType.Alpha:
                    {
                        Alpha += deltaTime * modFactor.X * delta.X;
                    }
                    break;
                case SPParticleBehavior.ParticleBehaviorType.Scale:
                    {
                        ScaleX += deltaTime * modFactor.X * delta.X;
                        if (SPParticleBehavior.IsOptionSet(SPParticleBehavior.kBitmapOptionLockXYScale, pb))
                            ScaleY = ScaleX;
                        else
                            ScaleY += deltaTime * modFactor.Y * delta.Y;
                    }
                    break;
            }
        }
        #endregion

        #region ParticlePool
        public delegate SPParticle CreateParticle();
        private static Dictionary<string, SPParticle[]> s_ParticleBuffer = null;
        private static Dictionary<string, SPPoolIndexer> s_ParticleBufferIndexer = null;

        public static void PrimeParticleBufferWithSize(int size, CreateParticle producer, string tag)
        {
            if (size <= 0 || producer == null || tag == null)
                return;

            if (s_ParticleBuffer != null && s_ParticleBuffer.ContainsKey(tag))
                throw new ArgumentException("Duplicate tag in ParticlePool primer.");

            if (s_ParticleBuffer == null)
                s_ParticleBuffer = new Dictionary<string, SPParticle[]>();
            if (s_ParticleBufferIndexer == null)
                s_ParticleBufferIndexer = new Dictionary<string, SPPoolIndexer>();

            PurgeParticleBuffer(tag);

            int vertexCheckouts = size;
            SPParticle[] particleBuffer = new SPParticle[size];
            SPPoolIndexer particleBufferIndexer = new SPPoolIndexer(vertexCheckouts, tag);

            s_ParticleBuffer.Add(tag, particleBuffer);
            s_ParticleBufferIndexer.Add(tag, particleBufferIndexer);

            for (int i = 0; i < particleBufferIndexer.Capacity; ++i)
            {
                particleBuffer[i] = producer();
                particleBufferIndexer.InsertPoolIndex(i, i);
            }
        }

        internal static void PurgeParticleBuffer(string key)
        {
            if (key != null)
            {
                if (s_ParticleBufferIndexer != null && s_ParticleBufferIndexer.ContainsKey(key))
                    s_ParticleBufferIndexer.Remove(key);
                if (s_ParticleBuffer != null && s_ParticleBuffer.ContainsKey(key))
                    s_ParticleBuffer.Remove(key);
            }
        }

        public static SPParticle[] ParticleBufferForKey(string key)
        {
            if (s_ParticleBuffer != null && s_ParticleBuffer.ContainsKey(key))
                return s_ParticleBuffer[key];
            else
                return null;
        }

        public static SPPoolIndexer ParticleBufferIndexerForKey(string key)
        {
            if (s_ParticleBufferIndexer != null && s_ParticleBufferIndexer.ContainsKey(key))
                return s_ParticleBufferIndexer[key];
            else
                return null;
        }

        public static int CheckoutNextParticleBufferIndex(string key)
        {
            SPPoolIndexer indexer = ParticleBufferIndexerForKey(key);

#if DEBUG
            if (indexer.IndicesIndex > indexer.Capacity)
                Debug.WriteLine("~*~*~*~* SPParticle PoolIndex Empty: {0} *~*~*~*~", indexer.IndicesIndex);
#endif
            if (indexer != null)
                return indexer.CheckoutNextIndex();
            else
                return -1;
        }

        public static void CheckinParticleBufferIndex(string key, int index)
        {
            SPPoolIndexer indexer = ParticleBufferIndexerForKey(key);
            if (indexer != null)
                indexer.CheckinIndex(index);
        }
        #endregion
    }
}
