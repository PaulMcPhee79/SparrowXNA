using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace SparrowXNA
{
    public class SPSubTexture : SPTexture
    {
        public SPSubTexture(SPRectangle region, SPTexture texture)
            : base(texture.Texture)
        {
            mBaseTexture = texture;
            Clipping = new SPRectangle(region.X / texture.Width, region.Y / texture.Height, region.Width / texture.Width, region.Height / texture.Height);
        }

        #region Fields
        private SPTexture mBaseTexture;
        private SPRectangle mClipping;
        private SPRectangle mRootClipping;
        #endregion

        #region Properties
        public override float Width { get { return mBaseTexture.Width * mClipping.Width; } }
        public override float Height { get { return mBaseTexture.Height * mClipping.Height; } }
        public override SPRectangle TexCoords { get { return mClipping; } }
        public override SPRectangle Bounds
        {
            get
            {
                SPRectangle baseBounds = mBaseTexture.Bounds;

                return new SPRectangle(baseBounds.X + mClipping.X * baseBounds.Width,
                    baseBounds.Y + mClipping.Y * baseBounds.Height,
                    mClipping.Width * baseBounds.Width,
                    mClipping.Height * baseBounds.Height);
            }
        }
        public SPTexture BaseTexture { get { return mBaseTexture; } private set { mBaseTexture = value; } }
        public virtual SPRectangle Clipping
        {
            get
            {
                return mClipping;
            }

            set
            {
                mClipping = value;
                mRootClipping = value;

                SPTexture baseTexture = BaseTexture;

                if (baseTexture is SPSubTexture)
                {
                    SPSubTexture baseSubTexture = (SPSubTexture)baseTexture;
                    SPRectangle baseRootClipping = baseSubTexture.RootClipping;

                    mRootClipping.X = baseRootClipping.X + mRootClipping.X * baseRootClipping.Width;
                    mRootClipping.Y = baseRootClipping.Y + mRootClipping.Y * baseRootClipping.Height;
                    mRootClipping.Width *= baseRootClipping.Width;
                    mRootClipping.Height *= baseRootClipping.Height;
                }

                /* // By adding a RootClipping accessor, we can use the BaseTexture's clipping as a cache and avoid looping.
                while (baseTexture is SPSubTexture)
                {
                    SPSubTexture baseSubTexture = (SPSubTexture)baseTexture;
                    SPRectangle baseClipping = baseSubTexture.Clipping;

                    mRootClipping.X = baseClipping.X + mRootClipping.X * baseClipping.Width;
                    mRootClipping.Y = baseClipping.Y + mRootClipping.Y * baseClipping.Height;
                    mRootClipping.Width *= baseClipping.Width;
                    mRootClipping.Height *= baseClipping.Height;

                    baseTexture = baseSubTexture.BaseTexture;
                }
                 * */
            }
        }
        protected virtual SPRectangle RootClipping { get { return mRootClipping; } }
        #endregion

        #region Methods
        public override void AdjustTextureCoordinates(Vector2[] src, Vector2[] dest, int numVertices)
        {
            float clipX = mRootClipping.X, clipY = mRootClipping.Y;
            float clipWidth = mRootClipping.Width, clipHeight = mRootClipping.Height;

            for (int i = 0; i < numVertices; ++i)
            {
                dest[i] = new Vector2(clipX + src[i].X * clipWidth, clipY + src[i].Y * clipHeight);
            }
        }

        public override void AdjustTextureCoordinates(SPVertexPositionNormalTextureColor[] src, SPVertexPositionNormalTextureColor[] dest, int offset, int numVertices)
        {
            float clipX = mRootClipping.X, clipY = mRootClipping.Y;
            float clipWidth = mRootClipping.Width, clipHeight = mRootClipping.Height;

            int limit = offset + numVertices;
            for (int i = offset; i < limit; ++i)
            {
                Vector2 srcTexCoord = src[i].TextureCoordinate;
                dest[i].TextureCoordinate = new Vector2(clipX + srcTexCoord.X * clipWidth, clipY + srcTexCoord.Y * clipHeight);
            }
        }
        #endregion
    }
}
