using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SparrowXNA
{
    public struct SPRectangle
    {
        public SPRectangle(float x, float y, float width, float height)
        {
            mX = x;
            mY = y;
            mWidth = width;
            mHeight = height;
        }

        public SPRectangle(Rectangle rect)
            : this(rect.X, rect.Y, rect.Width, rect.Height)
        {

        }

        #region Fields
        private float mX;
        private float mY;
        private float mWidth;
        private float mHeight;
        #endregion

        #region Properties
        public float X { get { return mX; } set { mX = value; } }
        public float Y { get { return mY; } set { mY = value; } }
        public float MinX { get { return X; } }
        public float MinY { get { return Y; } }
        public float MaxX { get { return X + Width; } }
        public float MaxY { get { return Y + Height; } }
        public float Width { get { return mWidth; } set { mWidth = value; } }
        public float Height { get { return mHeight; } set { mHeight = value; } }
        public float Area { get { return mWidth * mHeight; } }
        public Vector2 Origin { get { return new Vector2(mX, mY); } }
        public Vector2 Center { get { return new Vector2(mX + mWidth / 2, mY + mHeight / 2); } }
        public bool IsEmpty
        {
            get
            {
                return mWidth == 0 || mHeight == 0;
            }
        }
        public bool IsValid
        {
            get
            {
                return mWidth >= 0 && mHeight >= 0;
            }
        }
        public static SPRectangle Empty
        {
            get
            {
                return new SPRectangle(0, 0, 0, 0);
            }
        }
        #endregion

        #region Methods
        public bool Contains(float x, float y)
        {
            return x >= mX && y >= mY && x <= mX + mWidth && y <= mY + mHeight;
        }

        public bool Contains(Vector2 point)
        {
            return Contains(point.X, point.Y);
        }

        public bool ContainsRectangle(SPRectangle rectangle)
        {
            float rX = rectangle.X, rY = rectangle.Y;
            float rWidth = rectangle.Width, rHeight = rectangle.Height;

            return rX >= mX && rX + rWidth <= mX + mWidth && rY >= mY && rY + rHeight <= mY + mHeight;
        }

        public bool IntersectsRectangle(SPRectangle rectangle)
        {
            float rX = rectangle.X, rY = rectangle.Y;
            float rWidth = rectangle.Width, rHeight = rectangle.Height;

            bool outside = (rX <= mX && rX + rWidth <= mX) || (rX >= mX + mWidth && rX + rWidth >= mX + mWidth) ||
                (rY <= mY && rY + rHeight <= mY) || (rY >= mY + mHeight && rY + rHeight >= mY + mHeight);
            return !outside;
        }

        public SPRectangle IntersectionWithRectangle(SPRectangle rectangle)
        {
            float left = MathHelper.Max(mX, rectangle.X);
            float right = MathHelper.Min(mX + mWidth, rectangle.X + rectangle.Width);
            float top = MathHelper.Max(mY, rectangle.Y);
            float bottom = MathHelper.Min(mY + mHeight, rectangle.Y + rectangle.Height);

            if (left > right || top > bottom)
                return new SPRectangle(0, 0, 0, 0);
            else
                return new SPRectangle(left, top, right - left, bottom - top);
        }

        public SPRectangle UnionWithRectangle(SPRectangle rectangle)
        {
            float left = MathHelper.Min(mX, rectangle.X);
            float right = MathHelper.Max(mX + mWidth, rectangle.X + rectangle.Width);
            float top = MathHelper.Min(mY, rectangle.Y);
            float bottom = MathHelper.Max(mY + mHeight, rectangle.Y + rectangle.Height);
            return new SPRectangle(left, top, right - left, bottom - top);
        }

        public void SetEmpty()
        {
            mX = mY = mWidth = mHeight = 0f;
        }

        public bool IsEqualToRectangle(SPRectangle rectangle)
        {
            return SPMacros.SP_IS_FLOAT_EQUAL(mX, rectangle.X) && SPMacros.SP_IS_FLOAT_EQUAL(mY, rectangle.Y) &&
                SPMacros.SP_IS_FLOAT_EQUAL(mWidth, rectangle.Width) && SPMacros.SP_IS_FLOAT_EQUAL(mHeight, rectangle.Height);
        }

        public Rectangle ToRectangle()
        {
            return new Rectangle((int)mX, (int)mY, (int)mWidth, (int)mHeight);
        }

        public override string ToString()
        {
            return String.Format("(x: {0}, y: {1}, width: {2}, height: {3})", mX, mY, mWidth, mHeight);
        }
        #endregion
    }
}
