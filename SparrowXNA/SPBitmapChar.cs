using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SparrowXNA
{
    public class SPBitmapChar
    {
        public SPBitmapChar(SPTexture texture) : this(0, texture, new Vector2(0), texture != null ? texture.Width : 0) { }

        public SPBitmapChar(int charID, SPTexture texture, Vector2 offset, float xAdvance)
        {
            mTexture = texture;
            mCharID = charID;
            mOffset = offset;
            mXAdvance = xAdvance;
        }

        #region Fields
        private SPTexture mTexture;
        private int mCharID;
        private Vector2 mOffset;
        private float mXAdvance;
        private Dictionary<int, float> mKernings;
        #endregion

        #region Properties
        public int CharID { get { return mCharID; } }
        public Vector2 Offset { get { return mOffset; } }
        public float XAdvance { get { return mXAdvance; } }
        public SPTexture Texture { get { return mTexture; } set { mTexture = value; } }
        #endregion

        #region Methods
        public void AddKerning(float amount, int charID)
        {
            if (mKernings == null)
                mKernings = new Dictionary<int, float>();
            mKernings[charID] = amount;
        }

        public float KerningToChar(int charID)
        {
            return mKernings != null && mKernings.ContainsKey(charID) ? mKernings[charID] : 0;
        }

        public SPImage CreateImage()
        {
            return new SPImage(Texture);
        }
        #endregion
    }
}
