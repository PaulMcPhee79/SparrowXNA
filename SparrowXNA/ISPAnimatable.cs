using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SparrowXNA
{
    public interface ISPAnimatable
    {
        bool IsComplete { get; }
        uint AnimKey { get; }
        object Target { get; }
        void AdvanceTime(double seconds);
    }
}
