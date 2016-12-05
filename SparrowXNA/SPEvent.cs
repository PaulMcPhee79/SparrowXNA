using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SparrowXNA
{
    public delegate void SPEventHandler(SPEvent ev);
    //public delegate void SPEventHandler<T>(T ev) where T : SPEvent;

    public class SPEvent
    {
        public const string SP_EVENT_TYPE_ADDED = "added";
        public const string SP_EVENT_TYPE_ADDED_TO_STAGE = "addedToStage";
        public const string SP_EVENT_TYPE_REMOVED = "removed";
        public const string SP_EVENT_TYPE_REMOVED_FROM_STAGE = "removedFromStage";

        private static Dictionary<string, SPEvent> sCachedEvents = null;

        public static SPEvent SPEventWithType(string eventType, bool bubbles = false)
        {
            SPEvent ev = null;

            if (sCachedEvents == null)
                sCachedEvents = new Dictionary<string, SPEvent>(64);
            else if (sCachedEvents.ContainsKey(eventType))
                ev = sCachedEvents[eventType];

            if (ev == null)
            {
                ev = new SPEvent(eventType, bubbles);
                sCachedEvents[eventType] = ev;
            }

            return ev;
        }

        public SPEvent(string eventType, bool bubbles = false)
        {
            EventType = eventType;
            Bubbles = bubbles;
        }

        #region Properties
        public string EventType { get; private set; }
        public bool Bubbles { get; private set; }
        public object Data { get; set; }
        public SPEventDispatcher Target { get; internal set; }
        public SPEventDispatcher CurrentTarget { get; internal set; }

        internal bool StopsImmediatePropagation { get; set; }
        internal bool StopsPropagation { get; set; }
        #endregion

        #region Methods
        public void StopImmediatePropagation() { StopsImmediatePropagation = true; }
        public void StopPropagation() { StopsPropagation = true; }
        #endregion
    }
}
