using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace SparrowXNA
{
    internal class SPTouchProcessor
    {
        #region Contructors
        public SPTouchProcessor(SPDisplayObjectContainer root)
        {
            mRoot = root;
            mCurrentTouches = new SPHashSet<SPTouch>();
            mNewTouches = new SPHashSet<SPTouch>();
        }
        #endregion

        #region Fields
        private SPDisplayObjectContainer mRoot;
        SPHashSet<SPTouch> mCurrentTouches;
        SPHashSet<SPTouch> mNewTouches;
        #endregion

        #region Properties
        public SPDisplayObjectContainer Root { get { return mRoot; } set { mRoot = value; } }
        #endregion

        #region Methods
        public void ProcessTouches(SPHashSet<SPTouch> touches)
        {
            if (touches == null || touches.Count == 0)
                return;

            SPHashSet<SPTouch> processedTouches = mNewTouches;
            processedTouches.Clear();

            foreach (SPTouch touch in touches.EnumerableSet)
            {
                SPTouch currentTouch = null;

                foreach (SPTouch existingTouch in mCurrentTouches.EnumerableSet)
                {
                    if (existingTouch.Phase == SPTouchPhase.Ended || existingTouch.Phase == SPTouchPhase.Cancelled)
                        continue;

                    if ((existingTouch.GlobalX == touch.PreviousGlobalX && existingTouch.GlobalY == touch.PreviousGlobalY) ||
                        (existingTouch.GlobalX == touch.GlobalX && existingTouch.GlobalY == touch.GlobalY))
                    {
                        // Existing touch; update values
                        existingTouch.Timestamp = touch.Timestamp;
                        existingTouch.PreviousGlobalX = touch.PreviousGlobalX;  //existingTouch.GlobalX;
                        existingTouch.PreviousGlobalY = touch.PreviousGlobalY;  //existingTouch.GlobalY;
                        existingTouch.GlobalX = touch.GlobalX;
                        existingTouch.GlobalY = touch.GlobalY;
                        existingTouch.Phase = touch.Phase;
                        existingTouch.TapCount = touch.TapCount;

                        if (existingTouch.Target.Stage == null)
                        {
                            // Target could have been removed from the stage -> find new target in that case
                            Vector2 touchPosition = new Vector2(touch.GlobalX, touch.GlobalY);
                            existingTouch.Target = mRoot.HitTestPoint(touchPosition, true);
                        }

                        currentTouch = existingTouch;
                        break;
                    }
                }

                if (currentTouch == null)
                {
                    // New touch
                    currentTouch = new SPTouch();
                    currentTouch.Timestamp = touch.Timestamp;
                    currentTouch.GlobalX = touch.GlobalX;
                    currentTouch.GlobalY = touch.GlobalY;
                    currentTouch.PreviousGlobalX = touch.PreviousGlobalX;
                    currentTouch.PreviousGlobalY = touch.PreviousGlobalY;
                    currentTouch.Phase = touch.Phase;
                    currentTouch.TapCount = touch.TapCount;
                    Vector2 touchPosition = new Vector2(touch.GlobalX, touch.GlobalY);
                    currentTouch.Target = mRoot.HitTestPoint(touchPosition, true);

                    if (currentTouch.Target == null)
                        currentTouch = null;
                }

                if (currentTouch != null)
                    processedTouches.Add(currentTouch);
            }

            mCurrentTouches.Clear();

            // Dispatch events
            foreach (SPTouch touch in processedTouches.EnumerableSet)
            {
                if (touch.Target == null)
                    continue;

                SPTouchEvent touchEvent = new SPTouchEvent(SPTouchEvent.SP_EVENT_TYPE_TOUCH, processedTouches);
                touch.Target.DispatchEvent(touchEvent);
                mCurrentTouches.Add(touch);
            }
        }
        #endregion
    }
}
