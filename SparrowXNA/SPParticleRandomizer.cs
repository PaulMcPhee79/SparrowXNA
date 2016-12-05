using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace SparrowXNA
{
    public class SPParticleRandomizer : SPParticleResetter
    {
        // TODO: Add range distribution functions similar to SPTransitions
        //  in order to add more variation to randomization.

        #region Methods
        public override void ResetParticle(SPParticle particle)
        {
            if (particle == null)
                return;

            particle.X = Origin.X + SPRandom.NextRandom(X.From, X.To);
            particle.Y = Origin.Y + SPRandom.NextRandom(Y.From, Y.To);
            particle.Alpha = SPRandom.NextRandom(Alpha.From, Alpha.To);
            particle.Rotation = SPRandom.NextRandom(Rotation.From, Rotation.To);
            particle.ScaleX = SPRandom.NextRandom(ScaleX.From, ScaleX.To);
            particle.ScaleY = SPRandom.NextRandom(ScaleY.From, ScaleY.To);
            particle.ModFactor = new Vector2(
                SPRandom.NextRandom(ModFactorX.From, ModFactorX.To),
                SPRandom.NextRandom(ModFactorY.From, ModFactorY.To));
            particle.TotalTimeToLive = SPRandom.NextRandom(TimeToLive.From, TimeToLive.To);
        }
        #endregion
    }
}
