using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SparrowXNA
{
    public delegate void SPTouchEventHandler(SPTouchEvent ev);

    public class SPTouchEvent : SPEvent
    {
        public const string SP_EVENT_TYPE_TOUCH = "touch";

        public SPTouchEvent(string eventType, SPHashSet<SPTouch> touches, bool bubbles = true)
            : base(eventType, bubbles)
        {
            mTouches = touches;
        }

        #region Fields
        private SPHashSet<SPTouch> mTouches;
        #endregion

        #region Properties
        public SPHashSet<SPTouch> Touches { get { return mTouches; } }
        public double Timestamp
        {
            get
            {
                double timestamp = 0.0;

                if (mTouches != null && mTouches.Count > 0)
                {
                    //SPTouch touch = mTouches.GetEnumerator().Current;
                    SPTouch touch = mTouches.Current;

                    if (touch != null)
                        timestamp = touch.Timestamp;
                }

                return timestamp;
            }
        }
        #endregion

        #region Methods
        public SPTouch AnyTouch(SPHashSet<SPTouch> touches = null)
        {
            SPTouch touch = null;

            if (touches == null)
                touches = mTouches;

            if (touches != null && touches.Count > 0)
            {
                //IEnumerator<SPTouch> en = mTouches.GetEnumerator();
                //en.MoveNext();
                //touch = en.Current;
                touch = mTouches.Current;
            }

            return touch;
        }

        public SPHashSet<SPTouch> TouchesWithTarget(SPDisplayObject target)
        {
            SPHashSet<SPTouch> touchesFound = new SPHashSet<SPTouch>();
            foreach (SPTouch touch in mTouches.EnumerableSet)
            {
                if (target == touch.Target || (target is SPDisplayObjectContainer && ((SPDisplayObjectContainer)target).ContainsChild(touch.Target)))
                    touchesFound.Add(touch);
            }
            return touchesFound;
        }

        public SPHashSet<SPTouch> TouchesWithTarget(SPDisplayObject target, SPTouchPhase phase)
        {
            SPHashSet<SPTouch> touchesFound = new SPHashSet<SPTouch>();
            foreach (SPTouch touch in mTouches.EnumerableSet)
            {
                if (touch.Phase == phase && (target == touch.Target || (target is SPDisplayObjectContainer && ((SPDisplayObjectContainer)target).ContainsChild(touch.Target))))
                    touchesFound.Add(touch);
            }
            return touchesFound;
        }
        #endregion
    }
}
