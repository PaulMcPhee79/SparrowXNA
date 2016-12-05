using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SparrowXNA
{
    public class SPTexture : IDisposable
    {
        public SPTexture(Texture2D texture, bool ownsTexture = false)
        {
            mTexture = texture;
            mOwnsTexture = ownsTexture;
            Frame = SPRectangle.Empty;
            mRepeat = false;
        }

        #region Fields
        private bool mIsDisposed = false;
        private bool mOwnsTexture;
        private Texture2D mTexture;
        private bool mRepeat;
        #endregion

        #region Properties
        public Texture2D Texture { get { return mTexture; } protected set { mTexture = value; } }
        public virtual SPRectangle Bounds { get { return new SPRectangle(0f, 0f, Width, Height); } }
        public SPRectangle Frame { get; set; }
        public virtual float Width { get { return mTexture.Width; } }
        public virtual float Height { get { return mTexture.Height; } }
        public virtual SPRectangle TexCoords { get { return new SPRectangle(0f, 0f, 1f, 1f); } }
        public bool Repeat
        {
            get { return mRepeat; }
            set
            {
                mRepeat = value;
            }
        }
        #endregion

        #region Methods
        public virtual void AdjustTextureCoordinates(Vector2[] src, Vector2[] dest, int numVertices)
        {
            for (int i = 0; i < numVertices; ++i)
            {
                dest[i] = src[i];
            }
        }

        public virtual void AdjustTextureCoordinates(SPVertexPositionNormalTextureColor[] src, SPVertexPositionNormalTextureColor[] dest, int offset, int numVertices)
        {
            for (int i = offset; i < offset + numVertices; ++i)
            {
                dest[i].TextureCoordinate = src[i].TextureCoordinate;
            }
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
                    if (mTexture != null)
                    {
                        if (mOwnsTexture)
                            mTexture.Dispose();
                        // else - Clients must manage disposal as this is most likely owned by a ContentManager
                        mTexture = null;
                    }
                }

                mIsDisposed = true;
            }
        }

        ~SPTexture()
        {
            Dispose(false);
        }
        #endregion
    }
}
