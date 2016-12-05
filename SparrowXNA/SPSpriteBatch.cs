using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;


namespace SparrowXNA
{
    internal class SPSpriteBatch : SPRenderBatch
    {
        public SPSpriteBatch(SPRenderSupport support)
            : base(support)
        {
            mSupportRef = new WeakReference(support);
            mSpriteBatch = new SpriteBatch(support.GraphicsDevice);
            IsBatching = false;
            //mShouldDisableTexture = false;
        }

        #region Fields
        private bool mIsDisposed = false;
        //private bool mShouldDisableTexture;
        private SpriteBatch mSpriteBatch;
        private WeakReference mSupportRef;
        #endregion

        #region Properties
        private SPRenderSupport Support
        {
            get { return (SPRenderSupport)mSupportRef.Target; }
            set { mSupportRef.Target = value; }
        }

        public override bool IsBatching { get; protected set; }
        #endregion

        #region Methods
        public override void Begin()
        {
            if (IsBatching)
                throw new InvalidOperationException("End must be called before Begin can be called again.");
            IsBatching = true;
            SPRenderSupport support = Support;

            // SpriteBatch stomps the following render states: http://blogs.msdn.com/b/shawnhar/archive/2010/06/18/spritebatch-and-renderstates-in-xna-game-studio-4-0.aspx
            /*
            if (support.IsUsingDefaultEffect && !support.DefaultEffecter.TextureEnabled)
            {
                mShouldDisableTexture = true;
                support.DefaultEffecter.TextureEnabled = true;
            }
            */

            if (support != null)
            {
                mSpriteBatch.Begin(
                    SpriteSortMode.Deferred, support.CurrentBlendState,
                    support.SamplerState,
                    support.GraphicsDevice.DepthStencilState,
                    support.GraphicsDevice.RasterizerState,
                    null, //support.CurrentEffecter.Effect, // null
                    Matrix.Identity);
            }
        }

        public void AddImage(SPImage image, Matrix transform)
        {
#if false
            if (!IsBatching)
                throw new InvalidOperationException("Begin must be called before AddSprite can be called.");
            if (image == null || image.Texture == null)
                return;

            //if (image.RequiresRenderTransform)
            //    transform *= image.PreRenderTransformationMatrix;

            SPRectangle frame = image.Texture.Frame;
            Vector2 origin = new Vector2(frame.X, frame.Y);

            float rotation;
            Vector2 position, scale;
            SPUtils.DecomposeMatrix(ref transform, out position, out rotation, out scale);
            SPRectangle rect = image.Texture.Bounds;

            if (Support.SamplerState == SamplerState.LinearWrap)
            {
                rect.Width *= Math.Abs(scale.X);
                rect.Height *= Math.Abs(scale.Y);
                scale = Vector2.Normalize(scale);
            }

            mSpriteBatch.Draw(image.Texture.Texture, position, rect.ToRectangle(), image.Color * image.Alpha, rotation, origin, scale, image.SpriteEffect, 0f);
            //Debug.WriteLine(image.Texture.Bounds.ToString());
#endif
        }

        public void AddText(SPTextField textField, Matrix transform)
        {
            if (!IsBatching)
                throw new InvalidOperationException("Begin must be called before AddText can be called.");
            if (textField == null || textField.Font == null)
                return;
            textField.Compile();

            //float rotation;
            //Vector2 position, scale;
            //SPUtils.DecomposeMatrix(ref transform, out position, out rotation, out scale);

            if (textField.CachedBuilder != null)
            {
                StringBuilder sb = textField.CachedBuilder;
                foreach (SPTextField.TextLine textLine in textField.TextLines)
                {
                    float lineRotation;
                    Matrix lineTransform = textLine.TransformationMatrix * transform;
                    Vector2 linePosition, lineScale;
                    SPUtils.DecomposeMatrix(ref lineTransform, out linePosition, out lineRotation, out lineScale);
                    mSpriteBatch.DrawString(textField.Font, sb, linePosition, textField.Color * textField.Alpha,
                        lineRotation, Vector2.Zero, lineScale, textField.SpriteEffect, 0f);
                }
            }
            else
            {
                foreach (SPTextField.TextLine textLine in textField.TextLines)
                {
                    float lineRotation;
                    Matrix lineTransform = textLine.TransformationMatrix * transform;
                    Vector2 linePosition, lineScale;
                    SPUtils.DecomposeMatrix(ref lineTransform, out linePosition, out lineRotation, out lineScale);
                    mSpriteBatch.DrawString(textField.Font, textLine.Text, linePosition, textField.Color * textField.Alpha,
                        lineRotation, Vector2.Zero, lineScale, textField.SpriteEffect, 0f);
                }
            }

            //mSpriteBatch.DrawString(textField.Font, textField.Text, position, textField.Color, rotation, Vector2.Zero, scale, textField.SpriteEffect, 0f);
        }

        public override void End()
        {
            if (!IsBatching)
                throw new InvalidOperationException("Begin must be called before End can be called.");

            mSpriteBatch.End();

            /*
            SPRenderSupport support = Support;
            if (support.IsUsingDefaultEffect && mShouldDisableTexture)
            {
                support.DefaultEffecter.TextureEnabled = false;
                mShouldDisableTexture = false;
            }
            */

            IsBatching = false;
        }

        private void Flush()
        {
            if (IsBatching)
            {
                End();
                Begin();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!mIsDisposed)
            {
                try
                {
                    if (disposing)
                    {
                        Debug.Assert(!IsBatching, "Attempt to dispose SPSpriteBatch mid-batch.");
                        if (mSpriteBatch != null)
                        {
                            mSpriteBatch.Dispose();
                            mSpriteBatch = null;
                        }

                        Support = null; // Support may be disposed of after a Begin but before an End. We don't want to use it in this case.
                    }
                }
                catch (Exception)
                {
                    // Ignore
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }
        #endregion
    }
}
