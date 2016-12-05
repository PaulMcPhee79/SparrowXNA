using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SparrowXNA
{
    internal class SPListenerStrong : SPListener
    {
        public Action<SPEvent> Action { get; private set; }

        public SPListenerStrong(Action<SPEvent> handler)
        {
            Action = handler;
        }

        public override bool Invoke(SPEvent ev)
        {
            Action<SPEvent> action = Action;

            if (action != null)
            {
                action.Invoke(ev);
                return true;
            }
            else
                return false;
        }

        public override bool IsListening(object target)
        {
            return target != null && target == Action.Target;
        }

        public override bool IsListening(Delegate eventHandler)
        {
            return eventHandler != null && eventHandler.Target == Action.Target && eventHandler.Method == Action.Method;
        }

        public override void Cleanup()
        {
            //Action = null;
        }
    }
}
