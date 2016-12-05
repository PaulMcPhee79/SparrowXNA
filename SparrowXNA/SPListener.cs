using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SparrowXNA
{
    internal class SPListener
    {
        public SPListener()
        {

        }

        public virtual bool Invoke(SPEvent ev)
        {
            return false;
        }

        public virtual bool Invoke(params object[] args)
        {
            return false;
        }

        public virtual bool IsListening(object target)
        {
            return false;
        }

        public virtual bool IsListening(Delegate eventHandler)
        {
            return false;
        }

        public virtual void Cleanup() { }
    }
}
