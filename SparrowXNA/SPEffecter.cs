using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace SparrowXNA
{
    public delegate void CustomDraw(SPDisplayObject displayObject, GameTime gameTime, SPRenderSupport support, Matrix parentTransform);

    public class SPEffecter
    {
        public SPEffecter(Effect effect, CustomDraw customDraw, float factor = 1f)
        {
            mFactor = factor;
            Effect = effect;
            CustomDraw = customDraw;
            mEffectParams = new Dictionary<string, EffectParameter>(8);
        }

        #region Fields
        private Dictionary<string, EffectParameter> mEffectParams;
        #endregion

        #region Properties
        private float mFactor;
        public float Factor { get { return mFactor; } set { mFactor = value; } }
        public virtual Effect Effect { get; set; }
        public CustomDraw CustomDraw { get; set; }
        #endregion

        #region Methods
        public void AddEffectParameter(string key)
        {
            mEffectParams[key] = Effect.Parameters[key];
        }

        public EffectParameter EffectParameterForKey(string key)
        {
            if (!mEffectParams.ContainsKey(key))
            {
                if (Effect != null && Effect.Parameters.First(param => param.Name.Equals(key)) != null)
                    AddEffectParameter(key);
                else
                    return null;
            }
            
            return mEffectParams[key];
        }
        #endregion
    }
}
