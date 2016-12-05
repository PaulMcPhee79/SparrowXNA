using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace SparrowXNA
{
    public abstract class SPParticleResetter
    {
        #region Fields
        public Vector2 Origin = new Vector2();
        public SPRange X = new SPRange();
        public SPRange Y = new SPRange();
        public SPRange Alpha = new SPRange(1f, 1f);
        public SPRange Rotation = new SPRange();
        public SPRange ScaleX = new SPRange(1f, 1f);
        public SPRange ScaleY = new SPRange(1f, 1f);
        public SPRange ModFactorX = new SPRange(1f, 1f);
        public SPRange ModFactorY = new SPRange(1f, 1f);
        public SPRange TimeToLive = new SPRange(5f, 10f); // 5-10; not 0-0: In case not set, client can still see system
                                                          // for a while and does not falsely assume display list error.
        #endregion

        #region Methods
        public virtual void ResetParticle(SPParticle particle)
        {
            // Override me
        }
        #endregion
    }
}
