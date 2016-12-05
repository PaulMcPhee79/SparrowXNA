using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace SparrowXNA
{
    // !!! Usage warning !!!
    // Be aware that if you use this class on Windows, you will need to manually detect and handle lost device scenarios outside of Game.Draw.
    // Game handles lost devices automatically if you do all your drawing inside Draw, but Update is not guarded in the same way.
    // You can get a non-volatile copy of the rendertexture using the Texture property.

    // And don't render to texture from within Game.Draw unless you've first flushed the default RenderSupport's batch, else it
    // will attempt to flush without being the current render target.
    public class SPRenderTexture : IDisposable
    {
        private const int kMaxDimension = 2048;
        public SPRenderTexture(GraphicsDevice device, Effect defaultEffect, Effect nonTexturedEffect, float width, float height)
        {
            // XNA 4.0: http://blogs.msdn.com/b/shawnhar/archive/2010/03/26/rendertarget-changes-in-xna-game-studio-4-0.aspx

            mIsDrawing = false;
            mLegalWidth = Math.Min(kMaxDimension, Math.Max(1, (int)width)); //SPMacros.NextPowerOfTwo((int)width);
            mLegalHeight = Math.Min(kMaxDimension, Math.Max(1, (int)height)); // SPMacros.NextPowerOfTwo((int)height);

            mRenderTarget = new RenderTarget2D(
                device,
                mLegalWidth,
                mLegalHeight,
                true,
                device.PresentationParameters.BackBufferFormat,
                DepthFormat.None,
                4, // Free on XBOX
                RenderTargetUsage.DiscardContents); // .PreserveContents);
            mVolatileTexture = new SPTexture(mRenderTarget);
            if (mRenderSupport == null)
                mRenderSupport = new SPRenderSupport(device, defaultEffect, nonTexturedEffect, mLegalWidth, mLegalHeight, 0);
        }

        #region Fields
        private bool mIsDisposed = false;
        private bool mIsDrawing;
        private int mLegalWidth;
        private int mLegalHeight;
        private SPTexture mVolatileTexture;
        private RenderTarget2D mRenderTarget;
        private static SPRenderSupport mRenderSupport;
        private Action<SPRenderSupport> mDraw;
        private static Color[] mContent;
        #endregion

        #region Properties
        public SPTexture Texture
        {
            get
            {
                SPTexture texture = TempTextureWithTexture(new Texture2D(mRenderSupport.GraphicsDevice, mRenderTarget.Width, mRenderTarget.Height, false, SurfaceFormat.Color));
                return new SPTexture(texture.Texture, true);
            }
        }
        public SPTexture MippedTexture
        {
            get
            {
                Debug.Assert(!mIsDrawing, "Cannot use SPRenderTexture as a Texture while it is drawing.");

                if (mContent == null)
                    mContent = new Color[kMaxDimension * kMaxDimension];

                // Write the merged texture to a Texture2D, so we don't lose it when resizing the back buffer
                Texture2D mergedTexture = new Texture2D(mRenderSupport.GraphicsDevice, mRenderTarget.Width, mRenderTarget.Height, true, SurfaceFormat.Color);
                Color[] content = mContent; // new Color[mRenderTarget.Width * mRenderTarget.Height];

                for (int i = 0; i < mergedTexture.LevelCount; ++i)
                {
                    int mipWidth = (int)Math.Max(1, mRenderTarget.Width >> i);
                    int mipHeight = (int)Math.Max(1, mRenderTarget.Height >> i);

                    mRenderTarget.GetData<Color>(i, null, content, 0, mipWidth * mipHeight);
                    mergedTexture.SetData<Color>(i, null, content, 0, mipWidth * mipHeight);
                }

                return new SPTexture(mergedTexture, true);
            }
        }
        public SPTexture VolatileTexture { get { return mVolatileTexture; } }
        public RenderTarget2D RenderTarget { get { return mRenderTarget; } }
        #endregion

        #region Methods
        // Caution! These are temporary textures that re-use memory for efficiency. For a permanent texture, use the Texture property.
        public SPTexture TempTextureWithTexture(Texture2D texture)
        {
            Debug.Assert(!mIsDrawing, "Cannot use SPRenderTexture as a Texture while it is drawing.");
            Debug.Assert(texture != null && texture.Width == mRenderTarget.Width && texture.Height == mRenderTarget.Height,
                "Invalid args passed to SPRenderTexture::TextureWithTexture.");

            mRenderTarget.GraphicsDevice.Textures[0] = null;

            if (mContent == null)
                mContent = new Color[kMaxDimension * kMaxDimension];

            // Write the merged texture to a Texture2D, so we don't lose it when resizing the back buffer
            Texture2D mergedTexture = texture;
            Color[] content = mContent; // new Color[mRenderTarget.Width * mRenderTarget.Height];

            for (int i = 0; i < mergedTexture.LevelCount; ++i)
            {
                int mipWidth = (int)Math.Max(1, mRenderTarget.Width >> i);
                int mipHeight = (int)Math.Max(1, mRenderTarget.Height >> i);

                mRenderTarget.GetData<Color>(i, null, content, 0, mipWidth * mipHeight);
                mergedTexture.SetData<Color>(i, null, content, 0, mipWidth * mipHeight);
            }

            return new SPTexture(mergedTexture);
        }

        private void PreDraw()
        {
            if (mIsDrawing)
                throw new InvalidOperationException("Cannot call DrawObject from within a BundleDrawCalls callback.");
            mIsDrawing = true;

            if (mRenderSupport.RenderWidth != mLegalWidth || mRenderSupport.RenderHeight != mLegalHeight)
                mRenderSupport.SetDimensions(mLegalWidth, mLegalHeight);
            mRenderSupport.SetRenderTarget(mRenderTarget);
            GraphicsDevice device = mRenderTarget.GraphicsDevice;
            device.Clear(Color.Transparent);
        }

        private void PostDraw()
        {
            mRenderSupport.SetRenderTarget(null);
            mIsDrawing = false;
        }

        public void DrawObject(SPDisplayObject displayObject)
        {
            PreDraw();
            mRenderSupport.BeginBatch();
            SPRectangle bounds = displayObject.Bounds;
            displayObject.Draw(null, mRenderSupport, Matrix.CreateTranslation(-bounds.X, -bounds.Y, 0)); // Translation centers at origin
            mRenderSupport.EndBatch();
            PostDraw();
        }

        public void BundleDrawCalls(Action<SPRenderSupport> draw)
        {
            if (draw != null)
                mDraw = draw;

            if (mDraw != null)
            {
                PreDraw();
                mRenderSupport.BeginBatch();
                mDraw(mRenderSupport);
                mRenderSupport.EndBatch();
                PostDraw();
            }
        }

        public void RepeatBundleDrawCalls()
        {
            BundleDrawCalls(null);
        }

        public void ClearWithColor(Color color)
        {
            GraphicsDevice device = mRenderTarget.GraphicsDevice;
            device.SetRenderTarget(mRenderTarget);
            device.Clear(color);
            device.SetRenderTarget(null);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!mIsDisposed)
            {
                if (disposing)
                {
                    //if (mRenderSupport != null)
                    //{
                    //    mRenderSupport.Dispose();
                    //    mRenderSupport = null;
                    //}

                    if (mRenderTarget != null)
                    {
                        mRenderTarget.Dispose();
                        mRenderTarget = null;
                    }
                }

                mDraw = null;
                mIsDisposed = true;
            }
        }

        ~SPRenderTexture()
        {
            Dispose(false);
        }
        #endregion
    }
}
