using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace SparrowXNA
{
#if true
    public class SPImage : SPQuad
    {
        public SPImage(SPTexture texture)
            : base(texture)
        {

        }
    }

#else
    public class SPImage : SPDisplayObject
    {
        public SPImage(SPTexture texture)
        {
            if (texture == null)
                throw new ArgumentNullException("Texture cannot be null.");
            mTexture = texture;
            mColor = Color.White;
            mSpriteEffect = SpriteEffects.None;
            mVertices = new Vector2[4];
            FillVertices(MathHelper.Max(texture.Width, texture.Frame.Width), MathHelper.Max(texture.Height, texture.Frame.Height));
        }

        #region Fields
        private SPTexture mTexture;
        private Color mColor;
        private SpriteEffects mSpriteEffect;
        private Vector2[] mVertices;
        #endregion

        #region Properties
        public SPTexture Texture { get { return mTexture; } set { mTexture = value; } }
        public Color Color { get { return mColor; } set { mColor = value; } }
        public SpriteEffects SpriteEffect { get { return mSpriteEffect; } set { mSpriteEffect = value; } }
        public override bool RequiresRenderTransform
        {
            get { return (mTexture != null) ? !mTexture.Frame.IsEmpty : false; }
        }
        public override Matrix PreRenderTransformationMatrix
        {
            get
            {
                if (RequiresRenderTransform)
                {
                    SPRectangle frame = mTexture.Frame;
                    Matrix transform = Matrix.CreateTranslation(-frame.X, -frame.Y, 0f);
                    transform = Matrix.CreateScale(mTexture.Width / frame.Width, mTexture.Height / frame.Height, 1f) * transform;
                    return transform;
                }
                else
                {
                    return Matrix.Identity;
                }
            }
        }
        #endregion

        #region Methods
        private void FillVertices(float width, float height)
        {
            for (int i = 0; i < mVertices.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        mVertices[i] = new Vector2(0, height);
                        break;
                    case 1:
                        mVertices[i] = new Vector2(0, 0);
                        break;
                    case 2:
                        mVertices[i] = new Vector2(width, height);
                        break;
                    case 3:
                        mVertices[i] = new Vector2(width, 0);
                        break;
                }
            }
        }

        public override SPRectangle BoundsInSpace(SPDisplayObject targetCoordinateSpace)
        {
            float minX = float.PositiveInfinity, maxX = float.NegativeInfinity, minY = float.PositiveInfinity, maxY = float.NegativeInfinity;

            if (targetCoordinateSpace == this) // Optimization
            {
                for (int i = 0; i < 4; ++i)
                {
                    float x = mVertices[i].X;
                    float y = mVertices[i].Y;
                    minX = MathHelper.Min(minX, x);
                    maxX = MathHelper.Max(maxX, x);
                    minY = MathHelper.Min(minY, y);
                    maxY = MathHelper.Max(maxY, y);
                }
            }
            else
            {
                Matrix transform = TransformationMatrixToSpace(targetCoordinateSpace);
                Vector2 point;

                for (int i = 0; i < 4; ++i)
                {
                    point = mVertices[i];
                    Vector2 transformedPoint = Vector2.Transform(point, transform);
                    float tfX = transformedPoint.X;
                    float tfY = transformedPoint.Y;

                    minX = MathHelper.Min(minX, tfX);
                    maxX = MathHelper.Max(maxX, tfX);
                    minY = MathHelper.Min(minY, tfY);
                    maxY = MathHelper.Max(maxY, tfY);
                }
            }

            return new SPRectangle(minX, minY, maxX - minX, maxY - minY);
        }

        public override void PreDraw(SPRenderSupport support)
        {
            base.PreDraw(support);

            if (Texture != null && Texture.Repeat)
                support.SamplerState = SamplerState.LinearWrap;
        }

        public override void Draw(GameTime gameTime, SPRenderSupport support, Matrix parentTransform)
        {
            PreDraw(support);
            SPEffecter effecter = support.IsUsingDefaultEffect ? null : support.CurrentEffecter;

            if (effecter == null || effecter.CustomDraw == null)
            {
                Matrix globalTransform = TransformationMatrix * parentTransform;
                support.DefaultTexture = Texture.Texture;
                support.AddImage(this, globalTransform);
            }
            else
            {
                effecter.CustomDraw(this, gameTime, support, parentTransform);
            }

            PostDraw(support);
        }
        #endregion
    }
#endif
}
