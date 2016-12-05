using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SparrowXNA
{
    public class SPJuggler : ISPAnimatable
    {
        private static uint sNextAnimKey = 0;
        public static uint NextAnimKey()
        {
            return ++sNextAnimKey;
        }

        public SPJuggler(int capacity = 64)
        {
            mJuggling = false;
            mAnimKey = SPJuggler.NextAnimKey();
            mObjects = new Dictionary<uint, ISPAnimatable>(capacity);
            mIterObjects = new List<ISPAnimatable>(capacity);
            mTempGarbage = new List<ISPAnimatable>(capacity / 4);
            mAddQueue = new Dictionary<uint, ISPAnimatable>(capacity / 4);
            mRemoveQueue = new Dictionary<uint, ISPAnimatable>(capacity / 4);
            ElapsedTime = 0;
        }

        #region Fields
        private bool mJuggling;
        private uint mAnimKey;
        private List<ISPAnimatable> mIterObjects;
        private List<ISPAnimatable> mTempGarbage;
        private Dictionary<uint, ISPAnimatable> mObjects;
        private Dictionary<uint, ISPAnimatable> mAddQueue;
        private Dictionary<uint, ISPAnimatable> mRemoveQueue;
        #endregion

        #region Properties
        public object Target { get { return null; } }
        public double ElapsedTime { get; private set; }
        public bool IsComplete { get { return false; } }
        public uint AnimKey { get { return mAnimKey; } }
        #endregion

        #region Methods
        public void AdvanceTime(double seconds)
        {
            ElapsedTime += seconds;

            // We need to work with a copy since client-code could modify the collection during the enumeration
            mJuggling = true;
            foreach (ISPAnimatable obj in mIterObjects)
            {
				// Skip elements that have been removed during the iteration.
				if (mRemoveQueue.Count > 0 && mRemoveQueue.ContainsKey(obj.AnimKey))
					continue;
                obj.AdvanceTime(seconds);
            }
            mJuggling = false;

            for (int i = mIterObjects.Count - 1; i >= 0; --i)
            {
                if (mIterObjects[i].IsComplete)
                    RemoveObject(mIterObjects[i]);
            }

            foreach (KeyValuePair<uint, ISPAnimatable> kvp in mAddQueue)
            {
                mIterObjects.Add(kvp.Value);
                mObjects[kvp.Key] = kvp.Value;
            }

            foreach (KeyValuePair<uint, ISPAnimatable> kvp in mRemoveQueue)
            {
                mIterObjects.Remove(kvp.Value);
                mObjects.Remove(kvp.Key);
            }

            mAddQueue.Clear();
            mRemoveQueue.Clear();
        }

        public void AddObject(ISPAnimatable obj)
        {
            if (obj != null)
            {
                if (mJuggling)
                {
                    if (mRemoveQueue.ContainsKey(obj.AnimKey))
                        mRemoveQueue.Remove(obj.AnimKey);
                    if (!mObjects.ContainsKey(obj.AnimKey) && !mAddQueue.ContainsKey(obj.AnimKey))
                        mAddQueue[obj.AnimKey] = obj;
                }
                else if (!mObjects.ContainsKey(obj.AnimKey))
                {
                    mIterObjects.Add(obj);
                    mObjects[obj.AnimKey] = obj;
                }
            }
        }

        public void RemoveObject(ISPAnimatable obj)
        {
            if (obj == null || mRemoveQueue.ContainsKey(obj.AnimKey))
                return;

            if (mObjects.ContainsKey(obj.AnimKey) || mAddQueue.ContainsKey(obj.AnimKey))
            {
                if (mJuggling)
                {
                    mAddQueue.Remove(obj.AnimKey);
                    mRemoveQueue[obj.AnimKey] = obj;
                }
                else
                {
                    mIterObjects.Remove(obj);
                    mObjects.Remove(obj.AnimKey);
                }
            }
        }

        public void RemoveAllObjects()
        {
            if (mJuggling)
            {
                mAddQueue.Clear();
                mRemoveQueue.Clear();

                foreach (KeyValuePair<uint, ISPAnimatable> kvp in mObjects)
                    RemoveObject(kvp.Value);
            }
            else
            {
                mIterObjects.Clear();
                mObjects.Clear();
                mAddQueue.Clear();
                mRemoveQueue.Clear();
            }
        }

        public void RemoveTweensWithTarget(object obj)
        {
#if true
            if (obj == null) return;

            if (mJuggling)
            {
                foreach (KeyValuePair<uint, ISPAnimatable> kvp in mObjects)
                {
                    if (kvp.Value.Target == obj)
                        RemoveObject(kvp.Value);
                }

                foreach (KeyValuePair<uint, ISPAnimatable> kvp in mAddQueue)
                {
                    if (kvp.Value.Target == obj)
                        mTempGarbage.Add(kvp.Value);
                }

                foreach (ISPAnimatable anim in mTempGarbage)
                    RemoveObject(anim);
                mTempGarbage.Clear();
            }
            else
            {
                foreach (KeyValuePair<uint, ISPAnimatable> kvp in mObjects)
                {
                    if (kvp.Value.Target == obj)
                        mAddQueue[kvp.Key] = kvp.Value;
                }

                foreach (KeyValuePair<uint, ISPAnimatable> kvp in mAddQueue)
                    RemoveObject(kvp.Value);
                mAddQueue.Clear();
            }
#else
            if (obj == null) return;

            string propName = "Target";
            List<ISPAnimatable> remainingObjects = new List<ISPAnimatable>();

            foreach (ISPAnimatable currentObj in mObjects)
            {
                PropertyInfo prop = currentObj.GetType().GetProperty(propName);

                if (prop != null)
                {
                    Func<object> getter = (Func<object>)Delegate.CreateDelegate(typeof(Func<object>), currentObj, prop.GetGetMethod());

                    if (obj.Equals(getter()))
                        continue;
                }
                
                remainingObjects.Add(currentObj);
            }

            mObjects = remainingObjects;
#endif
        }

        public void DelayInvocation(object target, double delay, Action action)
        {
            SPDelayedInvocation delayedInvoc = new SPDelayedInvocation(target, delay, action);
            AddObject(delayedInvoc);
        }
        #endregion
    }
}
