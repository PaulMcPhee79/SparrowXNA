using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SparrowXNA
{
    public abstract class SPRenderBatch : IDisposable
    {
        public SPRenderBatch(SPRenderSupport support) { }
        public abstract void Begin();
        public abstract void End();
        public abstract bool IsBatching { get; protected set; }

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) { }

        ~SPRenderBatch()
        {
            Dispose(false);
        }
        #endregion
    }
}
