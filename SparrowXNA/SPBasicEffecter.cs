using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SparrowXNA
{
    public class SPBasicEffecter : SPEffecter
    {
        public SPBasicEffecter(Effect effect, CustomDraw customDraw)
            : base(effect, customDraw)
        {
            World = Matrix.Identity;
            View = Matrix.Identity;
            Projection = Matrix.Identity;
        }

        public SPBasicEffecter NonTexturedEffecter { get; set; }

        public override Effect Effect
        {
            get
            {
                return !TextureEnabled && NonTexturedEffecter != null ? NonTexturedEffecter.Effect : base.Effect;
            }
            set
            {
                base.Effect = value;
            }
        }

        private bool mTextureEnabled = true;
        public bool TextureEnabled
        {
            get { return mTextureEnabled; }
            set { mTextureEnabled = value; }
        }

        private Texture2D mTexture;
        public Texture2D Texture
        {
            get { return mTexture; }
            set
            {
                if (mTexture != value)
                {
                    mTexture = value;
                    EffectParameter param = EffectParameterForKey("tex");
                    if (param != null)
                        param.SetValue(value);
                }
            }
        }

        private Matrix mWorld;
        public Matrix World
        {
            get { return mWorld; }
            set
            {
                mWorld = value;
                EffectParameter param = EffectParameterForKey("World");
                if (param != null)
                    param.SetValue(value);

                if (NonTexturedEffecter != null)
                    NonTexturedEffecter.World = value;
            }
        }
        private Matrix mView;
        public Matrix View
        {
            get { return mView; }
            set
            {
                mView = value;
                EffectParameter param = EffectParameterForKey("View");
                if (param != null)
                    param.SetValue(value);

                if (NonTexturedEffecter != null)
                    NonTexturedEffecter.View = value;
            }
        }
        private Matrix mProjection;
        public Matrix Projection
        {
            get { return mProjection; }
            set
            {
                mProjection = value;
                EffectParameter param = EffectParameterForKey("Projection");
                if (param != null)
                    param.SetValue(value);

                if (NonTexturedEffecter != null)
                    NonTexturedEffecter.Projection = value;
            }
        }
    }
}
