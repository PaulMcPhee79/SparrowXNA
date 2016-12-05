using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;

namespace SparrowXNA
{
    public class SPParticleSystem : IDisposable
    {
        private const int kDefaultParticleCapacity = 20;
        private const int kDefaultBehaviorCapacity = 5;

        public SPParticleSystem(SPDisplayObjectContainer container, int perBehaviorParticleCapacity = kDefaultParticleCapacity, int behaviorCapacity = kDefaultBehaviorCapacity)
        {
            if (container == null)
                throw new ArgumentNullException("SPParticleSystem c'tor: container argument cannot be null.");

            mContainer = container;
            mParticleCapacity = Math.Max(1, perBehaviorParticleCapacity);
            mBehaviorCapacity = Math.Max(1, behaviorCapacity);
            mParticles = new List<SPParticle>(ParticleCapacity * BehaviorCapacity);
            mBehaviors = new Dictionary<SPParticleBehavior, List<SPParticle>>(BehaviorCapacity);
            mTickQueue = new List<QueuedCommand>(BehaviorCapacity);
            mFreeList = new List<SPParticle>(ParticleCapacity);
        }

        #region Fields
        private string mName;
        protected bool mIsDisposed = false;
        private bool mTicking = false;
        private bool mLoop = false;
        private int mNumParticles;
        private int mParticleCapacity, mBehaviorCapacity;
        private double mTimeSinceReset;
        private List<SPParticle> mParticles;
        private Dictionary<SPParticleBehavior, List<SPParticle>> mBehaviors;
        private List<QueuedCommand> mTickQueue;
        private List<SPParticle> mFreeList;
        private SPDisplayObjectContainer mContainer;
        private SPParticleResetter mResetter;
        #endregion

        #region Properties
        public string Name { get { return mName; } set { mName = value; } }
        public bool Loop { get { return mLoop; } set { mLoop = value; } }
        public bool IsComplete { get { return !Loop && mFreeList.Count == NumParticles; } }
        protected bool IsTicking { get { return mTicking; } set { mTicking = value; } }
        public int ParticleCapacity { get { return mParticleCapacity; } }
        public int BehaviorCapacity { get { return mBehaviorCapacity; } }
        public int NumParticles { get { return mNumParticles; } private set { mNumParticles = value; } }
        public double TimeSinceReset { get { return mTimeSinceReset; } protected set { mTimeSinceReset = value; } }
        public SPDisplayObjectContainer Container { get { return mContainer; } }
        public SPParticleResetter Resetter { get { return mResetter; } set { mResetter = value; } }
        #endregion

        #region Methods
        private SPParticleBehavior GetParticleBehavior(string name)
        {
            if (name != null)
            {
                foreach (KeyValuePair<SPParticleBehavior, List<SPParticle>> kvp in mBehaviors)
                {
                    if (kvp.Key.Name != null && kvp.Key.Name.Equals(name))
                        return kvp.Key;
                }
            }

            return null;
        }

        public void AddParticle(SPParticle particle, string behaviorName)
        {
            AddParticle(particle, GetParticleBehavior(behaviorName));
        }

        public void AddParticle(SPParticle particle, SPParticleBehavior pb)
        {
            if (particle == null || pb == null)
                return;

            if (IsTicking)
                mTickQueue.Add(QueuedCommand.GetAddParticleCommand(pb, particle));
            else
            {
                if (!mBehaviors.ContainsKey(pb))
                    mBehaviors.Add(pb, new List<SPParticle>(ParticleCapacity));
                List<SPParticle> addList = mBehaviors[pb];
                if (!addList.Contains(particle))
                {
                    particle.Visible = false;
                    particle.RemoveFromParent();
                    Container.AddChild(particle);
                    addList.Add(particle);
                    if (!mParticles.Contains(particle))
                    {
                        mParticles.Add(particle);
                        ++NumParticles;
                    }
                }
            }
        }

        public void RemoveParticle(SPParticle particle, string behaviorName)
        {
            RemoveParticle(particle, GetParticleBehavior(behaviorName));
        }

        public void RemoveParticle(SPParticle particle, SPParticleBehavior pb)
        {
            if (particle == null || pb == null)
                return;

            if (IsTicking)
                mTickQueue.Add(QueuedCommand.GetRemoveParticleCommand(pb, particle));
            else
            {
                if (mBehaviors.ContainsKey(pb))
                {
                    List<SPParticle> removeList = mBehaviors[pb];
                    if (removeList.Contains(particle))
                    {
                        removeList.Remove(particle);

                        bool particleFound = false;
                        foreach (KeyValuePair<SPParticleBehavior, List<SPParticle>> kvp in mBehaviors)
                        {
                            if (kvp.Value != removeList && kvp.Value.Contains(particle))
                            {
                                particleFound = false;
                                break;
                            }
                        }

                        if (!particleFound)
                        {
                            mParticles.Remove(particle);
                            --NumParticles;
                        }
                    }
                }
            }
        }

        public void AddParticleBehavior(SPParticleBehavior pb, int capacity = -1)
        {
            if (pb == null)
                return;

            if (IsTicking)
                mTickQueue.Add(QueuedCommand.GetAddBehaviorCommand(pb, capacity));
            else
            {
                if (!mBehaviors.ContainsKey(pb))
                    mBehaviors.Add(pb, new List<SPParticle>(Math.Max(1, capacity <= 0 ? ParticleCapacity : capacity)));
            }
        }

        public void RemoveParticleBehavior(string behaviorName)
        {
            RemoveParticleBehavior(GetParticleBehavior(behaviorName));
        }

        public void RemoveParticleBehavior(SPParticleBehavior pb)
        {
            if (pb == null)
                return;

            if (IsTicking)
                mTickQueue.Add(QueuedCommand.GetRemoveBehaviorCommand(pb));
            else
            {
                if (mBehaviors.ContainsKey(pb))
                    mBehaviors.Remove(pb);
            }
        }

        protected virtual void ParticleBehaviorDidComplete(SPParticleBehavior pb)
        {
            if (pb != null)
                mBehaviors.Remove(pb);
        }

        protected void ProcessTickQueues()
        {
            foreach (QueuedCommand cmd in mTickQueue)
            {
                switch (cmd.Type)
                {
                    case QueuedCommand.CommandType.AddParticle:
                        AddParticle(cmd.Particle, cmd.Behavior);
                        break;
                    case QueuedCommand.CommandType.RemoveParticle:
                        RemoveParticle(cmd.Particle, cmd.Behavior);
                        break;
                    case QueuedCommand.CommandType.AddBehavior:
                        AddParticleBehavior(cmd.Behavior, cmd.Capacity);
                        break;
                    case QueuedCommand.CommandType.RemoveBehavior:
                        RemoveParticleBehavior(cmd.Behavior);
                        break;
                }
            }

            mTickQueue.Clear();
        }

        protected void ProcessFreeList()
        {
            foreach (SPParticle particle in mFreeList)
            {
                if (Resetter != null)
                    Resetter.ResetParticle(particle);
                particle.Visible = false;
            }

            mFreeList.Clear();
        }

        public virtual void Reset()
        {
            if (Resetter == null)
                return;

            foreach (SPParticle particle in mParticles)
            {
                Resetter.ResetParticle(particle);
                particle.Visible = false;
            }

            mFreeList.Clear();
            TimeSinceReset = 0;
        }

        public virtual void Tick(double time)
        {
            if (IsComplete)
                return;

            TimeSinceReset += time;

            {
                IsTicking = true;

                foreach (SPParticle particle in mParticles)
                {
                    if (!particle.IsComplete)
                    {
                        particle.Tick(time);

                        if (particle.IsComplete)
                        {
                            particle.Visible = false;
                            mFreeList.Add(particle);
                        }
                    }
                }

                foreach (KeyValuePair<SPParticleBehavior, List<SPParticle>> kvp in mBehaviors)
                {
                    SPParticleBehavior pb = kvp.Key;
                    List<SPParticle> particles = kvp.Value;

                    foreach (SPParticle particle in particles)
                        particle.ApplyTick(pb);
                }

                IsTicking = false;
            }

            if (Loop)
                ProcessFreeList();
            ProcessTickQueues();
        }

        public virtual void EmptyPool()
        {
            if (mParticles != null)
            {
                foreach (SPParticle particle in mParticles)
                {
                    if (particle.PoolIndex != -1)
                    {
                        particle.RemoveFromParent();
                        SPParticle.CheckinParticleBufferIndex(Name, particle.PoolIndex);
                    }
                }

                mParticles.Clear();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!mIsDisposed)
            {
                try
                {
                    if (disposing)
                    {
                        EmptyPool();
                    }
                }
                catch (Exception)
                {
                    // Ignore
                }
                finally
                {
                    mIsDisposed = true;
                }
            }
        }

        ~SPParticleSystem()
        {
            Dispose(false);
        }
        #endregion

        #region QueuedCommand
        private class QueuedCommand
        {
            public enum CommandType { AddParticle, RemoveParticle, AddBehavior, RemoveBehavior }

            public static QueuedCommand GetAddParticleCommand(SPParticleBehavior behavior, SPParticle particle = null)
            {
                QueuedCommand cmd = new QueuedCommand(CommandType.AddParticle, behavior, -1, particle);
                return cmd;
            }

            public static QueuedCommand GetRemoveParticleCommand(SPParticleBehavior behavior, SPParticle particle = null)
            {
                QueuedCommand cmd = new QueuedCommand(CommandType.RemoveParticle, behavior, -1, particle);
                return cmd;
            }

            public static QueuedCommand GetAddBehaviorCommand(SPParticleBehavior behavior, int capacity)
            {
                QueuedCommand cmd = new QueuedCommand(CommandType.AddBehavior, behavior, -1, null);
                return cmd;
            }

            public static QueuedCommand GetRemoveBehaviorCommand(SPParticleBehavior behavior)
            {
                QueuedCommand cmd = new QueuedCommand(CommandType.RemoveBehavior, behavior, -1, null);
                return cmd;
            }

            private QueuedCommand(CommandType type, SPParticleBehavior behavior, int capacity = -1, SPParticle particle = null)
            {
                this.Type = type;
                this.Capacity = capacity;
                this.Behavior = behavior;
                this.Particle = particle;
            }

            public CommandType Type;
            public int Capacity;
            public SPParticle Particle;
            public SPParticleBehavior Behavior;
        }
        #endregion
    }
}
