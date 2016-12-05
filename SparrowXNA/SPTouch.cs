using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SparrowXNA
{
    public enum SPTouchPhase
    {
        Begun = 0,
        Moved,
        Stationary,
        Ended,
        Cancelled
    }

    // TODO: Pool SPTouch objects.
    public class SPTouch
    {
        public SPTouch()
        {
            mTimestamp = 0.0;
            mGlobalX = mGlobalY = 0f;
            mPreviousGlobalX = mPreviousGlobalY = 0f;
            mTapCount = 0;
            mPhase = SPTouchPhase.Begun;
            mTarget = null;
        }

        #region Fields
        private double mTimestamp;
        private float mGlobalX;
        private float mGlobalY;
        private float mPreviousGlobalX;
        private float mPreviousGlobalY;
        private int mTapCount;
        private SPTouchPhase mPhase;
        private SPDisplayObject mTarget;
        #endregion

        #region Properties
        public double Timestamp { get { return mTimestamp; } internal set { mTimestamp = value; } }
        public float GlobalX { get { return mGlobalX; } internal set { mGlobalX = value; } }
        public float GlobalY { get { return mGlobalY; } internal set { mGlobalY = value; } }
        public float PreviousGlobalX { get { return mPreviousGlobalX; } internal set { mPreviousGlobalX = value; } }
        public float PreviousGlobalY { get { return mPreviousGlobalY; } internal set { mPreviousGlobalY = value; } }
        public int TapCount { get { return mTapCount; } internal set { mTapCount = value; } }
        public SPTouchPhase Phase { get { return mPhase; } internal set { mPhase = value; } }
        public SPDisplayObject Target { get { return mTarget; } internal set { mTarget = value; } }
        #endregion

        #region Methods
        public Vector2 LocationInSpace(SPDisplayObject space)
        {
            Vector2 point = new Vector2(mGlobalX, mGlobalY);
            Matrix transform = mTarget.Root.TransformationMatrixToSpace(space);
            return Vector2.Transform(point, transform);
        }

        public Vector2 PreviousLocationInSpace(SPDisplayObject space)
        {
            Vector2 point = new Vector2(mPreviousGlobalX, mPreviousGlobalY);
            Matrix transform = mTarget.Root.TransformationMatrixToSpace(space);
            return Vector2.Transform(point, transform);
        }
        #endregion
    }
}
