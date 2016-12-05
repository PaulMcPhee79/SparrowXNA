using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SparrowXNA
{
    internal class SPListenerWeak : SPListener
    {
        private object StrongRef { get; set; }
        public WeakReference TargetRef { get; private set; }
        public MethodInfo Method { get; private set; }

        public SPListenerWeak(Delegate handler, bool isStrong)
        {
            TargetRef = new WeakReference(handler.Target);
            Method = handler.Method;
            StrongRef = (isStrong) ? handler.Target : null;
        }

        public SPListenerWeak(object target, MethodInfo methodInfo, bool isStrong)
        {
            TargetRef = new WeakReference(target);
            Method = methodInfo;
            StrongRef = (isStrong) ? target : null;
        }

        public override bool IsListening(object target)
        {
            return target != null && target == TargetRef.Target;
        }

        public override bool IsListening(Delegate eventHandler)
        {
            return eventHandler != null && eventHandler.Target == TargetRef.Target && eventHandler.Method.Equals(Method);
        }

        public override bool Invoke(SPEvent ev)
        {
            return Invoke_P(ev);
        }

        #pragma warning disable
        [CLSCompliant(false)]
        private bool Invoke_P(params object[] args)
        {
            object target = TargetRef.Target;

            // Static anonymous methods have a null target (isAlive == false), but so do non-static anonymous methods that have been garbage collected.
            try
            {
                Method.Invoke(target, args);
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("SPListener Invoke Exception: {0}", e.Message);
                return false;
            }
        }
        #pragma warning restore

        public override void Cleanup()
        {
            StrongRef = null;
        }
    }
}
