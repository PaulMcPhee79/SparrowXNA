using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

namespace SparrowXNA
{
    public class SPEventDispatcher
    {
        #region Fields
        private Dictionary<string, List<SPListener>> mEventListeners;
        private List<SPListener> mRemainingListeners;
        #endregion

        #region Methods
        public void AddActionEventListener(string eventType, Action<SPEvent> eventHandler)
        {
            if (eventType == null)
                throw new ArgumentNullException("Event type cannot be null.");
            if (eventHandler == null)
                throw new ArgumentNullException("Delegate eventHandler cannot be null.");
            if (mEventListeners == null)
                mEventListeners = new Dictionary<string, List<SPListener>>();

            SPListener listener = new SPListenerStrong(eventHandler);

            /* When an event listener is added or removed, a new list is created instead of changing the current list.
             * The reason for this is that we can avoid creating a copy of the list in the dispatchEvent method, which
             * is called far more often than the add/remove listener methods.
             */

            if (!mEventListeners.ContainsKey(eventType))
            {
                List<SPListener> listeners = new List<SPListener>();
                listeners.Add(listener);
                mEventListeners.Add(eventType, listeners);
            }
            else
            {
                List<SPListener> listeners = new List<SPListener>(mEventListeners[eventType]);
                listeners.Add(listener);
                mEventListeners[eventType] = listeners;
            }
        }

        public void AddEventListener(string eventType, Delegate eventHandler, bool isStrong = false)
        {
            if (eventType == null)
                throw new ArgumentNullException("Event type cannot be null.");
            if (eventHandler == null)
                throw new ArgumentNullException("Delegate eventHandler cannot be null.");
            if (mEventListeners == null)
                mEventListeners = new Dictionary<string, List<SPListener>>();
            SPListener listener = new SPListenerWeak(eventHandler, isStrong);

            /* When an event listener is added or removed, a new list is created instead of changing the current list.
             * The reason for this is that we can avoid creating a copy of the list in the dispatchEvent method, which
             * is called far more often than the add/remove listener methods.
             */

            if (!mEventListeners.ContainsKey(eventType))
            {
                List<SPListener> listeners = new List<SPListener>();
                listeners.Add(listener);
                mEventListeners.Add(eventType, listeners);
            }
            else
            {
                List<SPListener> listeners = new List<SPListener>(mEventListeners[eventType]);
                listeners.Add(listener);
                mEventListeners[eventType] = listeners;
            }
        }

        public void AddEventListener(string eventType, object target, MethodInfo methodInfo, bool isStrong = false)
        {
            if (eventType == null)
                throw new ArgumentNullException("Event type cannot be null.");
            if (target == null)
                throw new ArgumentNullException("Target cannot be null.");
            if (methodInfo == null)
                throw new ArgumentNullException("MethodInfo cannot be null.");

            if (mEventListeners == null)
                mEventListeners = new Dictionary<string, List<SPListener>>();
            SPListener listener = new SPListenerWeak(target, methodInfo, isStrong);

            /* When an event listener is added or removed, a new list is created instead of changing the current list.
             * The reason for this is that we can avoid creating a copy of the list in the dispatchEvent method, which
             * is called far more often than the add/remove listener methods.
             */

            if (!mEventListeners.ContainsKey(eventType))
            {
                List<SPListener> listeners = new List<SPListener>();
                listeners.Add(listener);
                mEventListeners.Add(eventType, listeners);
            }
            else
            {
                List<SPListener> listeners = new List<SPListener>(mEventListeners[eventType]);
                listeners.Add(listener);
                mEventListeners[eventType] = listeners;
            }
        }

        public void RemoveEventListener(string eventType)
        {
            RemoveEventListener(eventType, null);
        }

        public void RemoveEventListener(string eventType, object target)
        {
            if (eventType == null || mEventListeners == null || !mEventListeners.ContainsKey(eventType))
                return;

            List<SPListener> listeners = mEventListeners[eventType];
            if (mRemainingListeners == null)
                mRemainingListeners = new List<SPListener>(listeners.Count);

            if (target != null)
            {
                foreach (SPListener listener in listeners)
                {
                    if (!listener.IsListening(target))
                        mRemainingListeners.Add(listener);
                    else
                        listener.Cleanup();
                }
            }

            if (mRemainingListeners.Count == 0)
                mEventListeners.Remove(eventType);
            else
            {
                mEventListeners[eventType] = mRemainingListeners;
                mRemainingListeners = listeners;

                if (mRemainingListeners != null)
                    mRemainingListeners.Clear();
            }
        }

        public void RemoveEventListener(string eventType, Delegate eventHandler)
        {
            if (eventType == null || mEventListeners == null || !mEventListeners.ContainsKey(eventType))
                return;

            List<SPListener> listeners = mEventListeners[eventType];
            if (mRemainingListeners == null)
                mRemainingListeners = new List<SPListener>(listeners.Count);

            if (eventHandler != null)
            {
                foreach (SPListener listener in listeners)
                {
                    if (!listener.IsListening(eventHandler))
                        mRemainingListeners.Add(listener);
                    else
                        listener.Cleanup();
                }
            }

            if (mRemainingListeners.Count == 0)
                mEventListeners.Remove(eventType);
            else
            {
                mEventListeners[eventType] = mRemainingListeners;
                mRemainingListeners = listeners;

                if (mRemainingListeners != null)
                    mRemainingListeners.Clear();
            }
        }

        public bool HasEventListenerForType(string eventType)
        {
            return (eventType != null && mEventListeners != null && mEventListeners.ContainsKey(eventType));
        }

        public void DispatchEvent(SPEvent ev)
        {
            if (ev == null || ev.EventType == null)
                return;

            List<SPListener> listeners = null, expiredListeners = null;

            if (mEventListeners != null && mEventListeners.ContainsKey(ev.EventType))
                listeners = mEventListeners[ev.EventType];

            SPEventDispatcher previousTarget = ev.Target;

            if (ev.Target == null || ev.CurrentTarget != null) ev.Target = this;
            ev.CurrentTarget = this;

            // Retain self (will the stack retain us?)
            SPEventDispatcher self = this;

            bool stopImmediatePropagation = false;

            if (listeners != null && listeners.Count != 0)
            {
                foreach (SPListener listener in listeners)
                {
                    bool status = listener.Invoke(ev);

                    if (!status)
                    {
                        if (expiredListeners == null)
                            expiredListeners = new List<SPListener>();
                        expiredListeners.Add(listener);
                    }

                    if (ev.StopsImmediatePropagation)
                    {
                        stopImmediatePropagation = true;
                        break;
                    }
                }
            }

            // Remove listeners whose delgates have been garbage collected
            if (expiredListeners != null)
            {
                foreach (SPListener listener in expiredListeners)
                    listeners.Remove(listener);
            }

            if (!stopImmediatePropagation)
            {
                ev.CurrentTarget = null; // This is how we can find out later if the event was redispatched

                if (ev.Bubbles && !ev.StopsPropagation && this is SPDisplayObject)
                {
                    SPDisplayObject target = this as SPDisplayObject;

                    if (target.Parent != null)
                        target.Parent.DispatchEvent(ev);
                }
            }

            ev.Target = previousTarget; //if (previousTarget != null) ev.Target = previousTarget;
            self = null;
        }
        #endregion
    }
}
