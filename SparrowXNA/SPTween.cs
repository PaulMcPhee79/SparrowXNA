using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using System.Diagnostics;

namespace SparrowXNA
{
    public enum SPLoopType
    {
        None = 0,
        Repeat,
        Reverse,
        ReversePause
    }

    public class SPTween : SPEventDispatcher, ISPAnimatable
    {
        public const string SP_EVENT_TYPE_TWEEN_STARTED = "tweenStarted";
        public const string SP_EVENT_TYPE_TWEEN_UPDATED = "tweenUpdated";
        public const string SP_EVENT_TYPE_TWEEN_COMPLETED = "tweenCompleted";

        public SPTween(object target, double time, string transition)
        {
            mDispatchesUpdateEvents = false;
            mTarget = target;
            mTotalTime = Math.Max(0.0001, time);
            mCurrentTime = 0;
            mDelay = 0;
            mRepeatDelay = 0;
            mProperties = new List<SPTweenedProperty>();
            mLoop = SPLoopType.None;
            mLoopCount = 0;
            mTag = 0;
            mAnimKey = SPJuggler.NextAnimKey();
            mTransition = transition;
            mTransitionFunc = (Func<float, float>)Delegate.CreateDelegate(typeof(Func<float, float>), typeof(SPTransitions), transition);
        }

        public SPTween(object target, double time) : this(target, time, SPTransitions.SPLinear) { }

        #region Fields
        private bool mDispatchesUpdateEvents;
        private object mTarget;
        private double mTotalTime;
        private double mCurrentTime;
        private double mDelay;
        private double mRepeatDelay;
        private SPLoopType mLoop;
        private int mLoopCount;
        private uint mTag;
        private uint mAnimKey;
        private string mTransition;
        private Func<float, float> mTransitionFunc;
        private List<SPTweenedProperty> mProperties;
        #endregion

        #region Properties
        public bool DispatchesUpdateEvents { get { return mDispatchesUpdateEvents; } private set { mDispatchesUpdateEvents = value; } }
        public object Target { get { return mTarget; } private set { mTarget = value; } }
        public double TotalTime { get { return mTotalTime; } private set { mTotalTime = value; } }
        private double CurrentTime { get { return mCurrentTime; } set { mCurrentTime = value; } }
        public double Delay { get { return mDelay; } set { CurrentTime = CurrentTime + mDelay - value; mDelay = value; } }
        public double RepeatDelay { get { return mRepeatDelay; } set { mRepeatDelay = value; } }
        public SPLoopType Loop { get { return mLoop; } set { mLoop = value; } }
        private int LoopCount { get { return mLoopCount; } }
        public uint Tag { get { return mTag; } set { mTag = value; } }
        public string Transition { get { return mTransition; } private set { mTransition = value; } }
        private Func<float, float> TransitionFunc { get { return mTransitionFunc; } set { mTransitionFunc = value; } }
        private List<SPTweenedProperty> Properties { get { return mProperties; } set { mProperties = value; } }
        public bool IsComplete { get { return (CurrentTime >= TotalTime && Loop == SPLoopType.None); } }
        public uint AnimKey { get { return mAnimKey; } }
        #endregion

        #region Methods
        public void AdvanceTime(double seconds)
        {
            if (seconds == 0.0 || (mLoop == SPLoopType.None && mCurrentTime == mTotalTime))
                return;

            if (mCurrentTime == mTotalTime)
            {
                mCurrentTime = (mLoop != SPLoopType.ReversePause || ((mLoopCount & 1) != 0)) ? -mRepeatDelay : 0.0;
                ++mLoopCount;
            }

            double previousTime = mCurrentTime;
            double restTime = mTotalTime - mCurrentTime;
            double carryOverTime = seconds > restTime ? seconds - restTime : 0.0;
            mCurrentTime = Math.Min(mTotalTime, mCurrentTime + seconds);

            if (mCurrentTime <= 0) return; // The delay is not over yet

            if (mLoopCount == 0 && previousTime <= 0 && mCurrentTime >= 0 && HasEventListenerForType(SP_EVENT_TYPE_TWEEN_STARTED))
                DispatchEvent(SPEvent.SPEventWithType(SP_EVENT_TYPE_TWEEN_STARTED));

            float ratio = (float)(mCurrentTime / mTotalTime);
            bool invertTransition = (mLoop == SPLoopType.Reverse && ((mLoopCount & 1) != 0));
            float transitionValue = invertTransition ? 1.0f - mTransitionFunc(1.0f - ratio) : mTransitionFunc(ratio);

            foreach (SPTweenedProperty prop in mProperties)
            {
                if (previousTime <= 0 && mCurrentTime > 0)
                    prop.StartValue = prop.CurrentValue;

                prop.CurrentValue = prop.StartValue + prop.Delta * transitionValue;
            }

            if (mDispatchesUpdateEvents) // Off by default (expensive)
            {
                if (HasEventListenerForType(SP_EVENT_TYPE_TWEEN_UPDATED))
                    DispatchEvent(SPEvent.SPEventWithType(SP_EVENT_TYPE_TWEEN_UPDATED));
            }

            if (previousTime < mTotalTime && mCurrentTime == mTotalTime)
            {
                if (mLoop == SPLoopType.Repeat)
                {
                    foreach (SPTweenedProperty prop in mProperties)
                        prop.CurrentValue = prop.StartValue;
                }
                else if (mLoop == SPLoopType.Reverse || mLoop == SPLoopType.ReversePause)
                {
                    foreach (SPTweenedProperty prop in mProperties)
                    {
                        prop.CurrentValue = prop.EndValue;
                        prop.EndValue = prop.StartValue;
                    }
                }

                if (HasEventListenerForType(SP_EVENT_TYPE_TWEEN_COMPLETED))
                    DispatchEvent(SPEvent.SPEventWithType(SP_EVENT_TYPE_TWEEN_COMPLETED));
            }

            AdvanceTime(carryOverTime);
        }

        public void AnimateProperty(string property, float targetValue)
        {
            if (Target != null)
                mProperties.Add(new SPTweenedProperty(mTarget, property, targetValue));
        }

        public void MoveTo(float x, float y)
        {
            AnimateProperty("x", x);
            AnimateProperty("y", y);
        }

        public void ScaleTo(float scale)
        {
            AnimateProperty("scaleX", scale);
            AnimateProperty("scaleY", scale);
        }

        public void Reset(bool resetLoopCount = true)
        {
            mCurrentTime = -mDelay;

            if (resetLoopCount)
                mLoopCount = 0;

            foreach (SPTweenedProperty property in mProperties)
                property.Reset();
        }

        public void Reset(double currentTime, double totalTime, bool resetLoopCount = true)
        {
            mCurrentTime = currentTime;

            if (resetLoopCount)
                mLoopCount = 0;
        }
        #endregion

        private sealed class SPTweenedProperty
        {
            private enum SPPropType
            {
                Byte = 0,
                Int32,
                UInt32,
                Int64,
                UInt64,
                Float,
                Double
            }

            public SPTweenedProperty(object target, string name, float endValue)
            {
                Target = target;
                EndValue = OrigEndValue = endValue;

                PropertyInfo prop = target.GetType().GetProperty(name);

                if (prop.PropertyType == typeof(float))
                {
                    PropType = SPPropType.Float;
                    GetterFloat = (Func<float>)Delegate.CreateDelegate(typeof(Func<float>), target, prop.GetGetMethod());
                    SetterFloat = (Action<float>)Delegate.CreateDelegate(typeof(Action<float>), target, prop.GetSetMethod());
                }
                else if (prop.PropertyType == typeof(double))
                {
                    PropType = SPPropType.Double;
                    GetterDouble = (Func<double>)Delegate.CreateDelegate(typeof(Func<double>), target, prop.GetGetMethod());
                    SetterDouble = (Action<double>)Delegate.CreateDelegate(typeof(Action<double>), target, prop.GetSetMethod());
                }
                else if (prop.PropertyType == typeof(Int32))
                {
                    PropType = SPPropType.Int32;
                    GetterInt32 = (Func<Int32>)Delegate.CreateDelegate(typeof(Func<Int32>), target, prop.GetGetMethod());
                    SetterInt32 = (Action<Int32>)Delegate.CreateDelegate(typeof(Action<Int32>), target, prop.GetSetMethod());
                }
                else if (prop.PropertyType == typeof(UInt32))
                {
                    PropType = SPPropType.UInt32;
                    GetterUInt32 = (Func<UInt32>)Delegate.CreateDelegate(typeof(Func<UInt32>), target, prop.GetGetMethod());
                    SetterUInt32 = (Action<UInt32>)Delegate.CreateDelegate(typeof(Action<UInt32>), target, prop.GetSetMethod());
                }
                else if (prop.PropertyType == typeof(Int64))
                {
                    PropType = SPPropType.Int64;
                    GetterInt64 = (Func<Int64>)Delegate.CreateDelegate(typeof(Func<Int64>), target, prop.GetGetMethod());
                    SetterInt64 = (Action<Int64>)Delegate.CreateDelegate(typeof(Action<Int64>), target, prop.GetSetMethod());
                }
                else if (prop.PropertyType == typeof(UInt64))
                {
                    PropType = SPPropType.UInt64;
                    GetterUInt64 = (Func<UInt64>)Delegate.CreateDelegate(typeof(Func<UInt64>), target, prop.GetGetMethod());
                    SetterUInt64 = (Action<UInt64>)Delegate.CreateDelegate(typeof(Action<UInt64>), target, prop.GetSetMethod());
                }
                else if (prop.PropertyType == typeof(byte))
                {
                    PropType = SPPropType.Byte;
                    GetterByte = (Func<byte>)Delegate.CreateDelegate(typeof(Func<byte>), target, prop.GetGetMethod());
                    SetterByte = (Action<byte>)Delegate.CreateDelegate(typeof(Action<byte>), target, prop.GetSetMethod());
                }
            }

            public SPTweenedProperty() : this(null, null, 0.0f) { }

            public object Target { get; private set; }
            private SPPropType PropType { get; set; }
            private Func<byte> GetterByte { get; set; }
            private Action<byte> SetterByte { get; set; }
            private Func<Int32> GetterInt32 { get; set; }
            private Action<Int32> SetterInt32 { get; set; }
            private Func<UInt32> GetterUInt32 { get; set; }
            private Action<UInt32> SetterUInt32 { get; set; }
            private Func<Int64> GetterInt64 { get; set; }
            private Action<Int64> SetterInt64 { get; set; }
            private Func<UInt64> GetterUInt64 { get; set; }
            private Action<UInt64> SetterUInt64 { get; set; }
            private Func<float> GetterFloat { get; set; }
            private Action<float> SetterFloat { get; set; }
            private Func<double> GetterDouble { get; set; }
            private Action<double> SetterDouble { get; set; }
            private bool mIsOrigStartValueInit = false;
            private float mOrigStartValue;
            private float OrigStartValue
            {
                get { return mOrigStartValue; }
                set
                {
                    if (!mIsOrigStartValueInit)
                    {
                        mOrigStartValue = value;
                        mIsOrigStartValueInit = true;
                    }
                }
            }
            private float OrigEndValue { get; set; }
            private float mStartValue;
            public float StartValue
            {
                get { return mStartValue; }
                set
                {
                    OrigStartValue = value;
                    mStartValue = value;
                }
            }
            public float EndValue { get; set; }
            public float Delta { get { return EndValue - StartValue; } }
            public float CurrentValue
            {
                get
                {
                    float currentVal = 0f;

                    switch (PropType)
                    {
                        case SPPropType.Float: currentVal = GetterFloat(); break;
                        case SPPropType.Double: currentVal = (float)GetterDouble(); break;
                        case SPPropType.Int32: currentVal = (float)GetterInt32(); break;
                        case SPPropType.UInt32: currentVal = (float)GetterUInt32(); break;
                        case SPPropType.Int64: currentVal = (float)GetterInt64(); break;
                        case SPPropType.UInt64: currentVal = (float)GetterUInt64(); break;
                        case SPPropType.Byte: currentVal = (float)GetterByte(); break;
                        default: currentVal = 0f; break;
                    }

                    return currentVal;
                }

                set
                {
                    switch (PropType)
                    {
                        case SPPropType.Float: SetterFloat(value); break;
                        case SPPropType.Double: SetterDouble((double)value); break;
                        case SPPropType.Int32: SetterInt32((Int32)(value > 0 ? value+0.5f : value-0.5f)); break;
                        case SPPropType.UInt32: SetterUInt32((UInt32)(value > 0 ? value + 0.5f : value - 0.5f)); break;
                        case SPPropType.Int64: SetterUInt64((UInt64)(value > 0 ? value+0.5f : value-0.5f)); break;
                        case SPPropType.UInt64: SetterInt64((Int64)(value > 0 ? value + 0.5f : value - 0.5f)); break;
                        case SPPropType.Byte: SetterByte((byte)(value > 0 ? value + 0.5f : value - 0.5f)); break;
                        default: break;
                    }
                }
            }

            public void Reset()
            {
                StartValue = OrigStartValue;
                EndValue = OrigEndValue;
            }
        }
    }
}
