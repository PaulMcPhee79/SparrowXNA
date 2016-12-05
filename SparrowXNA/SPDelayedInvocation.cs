using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SparrowXNA
{
    class SPDelayedInvocation : ISPAnimatable
    {
        public SPDelayedInvocation(object target, double delay, Action action)
        {
            TotalTime = Math.Max(0.0001, delay);
            CurrentTime = 0;
            mAnimKey = SPJuggler.NextAnimKey();
            Target = target;
            DelayedInvoc = action;
        }

        #region Fields
        private double mCurrentTime;
        private uint mAnimKey;
        #endregion

        #region Properties
        public object Target { get; private set; }
        private Action DelayedInvoc { get; set; }
        public double TotalTime { get; private set; }
        public double CurrentTime {
            get
            {
                return mCurrentTime;
            }

            set
            {
                double previousTime = CurrentTime;
                mCurrentTime = Math.Min(TotalTime, value);

                if (previousTime < TotalTime && mCurrentTime >= TotalTime)
                    DelayedInvoc();
            }
        }

        public bool IsComplete { get { return CurrentTime >= TotalTime; } }
        public uint AnimKey { get { return mAnimKey; } }
        #endregion

        #region Methods
        public void AdvanceTime(double seconds)
        {
            CurrentTime += seconds;
        }
        #endregion
    }
}
