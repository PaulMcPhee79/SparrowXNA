using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SparrowXNA
{
    // Multiple scenes drawn at once: http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.graphics.graphicsdevice.viewport
    // This may require multiple stages and SPRenderSupport would have to be careful not to clobber global GraphicsDevice state.

    /* Touches
     * 
     * You need to calculate the transform matrix for your sprite, invert that (so the transform now goes from world space -> local space) and
     * transform the mouse position by the inverted matrix.
     * 
     *  Matrix transform = Matrix.CreateScale(scale) * Matrix.CreateRotationZ(rotation) * Matrix.CreateTranslation(translation);
        Matrix inverseTransform = Matrix.Invert(transform);
        Vector3 transformedMousePosition = Vector3.Transform(mousePosition, inverseTransform);
     * 
     * Primitives Batch: http://www.koders.com/csharp/fidF69CB83EF5E006ED45C14107EAB65811B12EE2B3.aspx?s=zoom
     * 
     * Using Multiple Cores: http://msdn.microsoft.com/en-us/library/3e8s7xdd.aspx
     * 
     * 
     * Shawn Hargreaves: http://www.shawnhargreaves.com/blogindex.html
     *  - Profiling: http://blogs.msdn.com/b/shawnhar/archive/2009/07/07/profiling-with-stopwatch.aspx 
     * 
     * Debug tips: http://bittermanandy.wordpress.com/2008/07/30/efficient-development-part-four/ [DebuggerDisplay("Name = {m_name}")]
     * */

    public class SPStage : SPDisplayObjectContainer
    {
        #region Contructors

        public SPStage(GraphicsDevice device, Effect defaultEffect, Effect nonTexturedEffect, float width, float height, int vertexBufferCapacity = 4096)
        {
            mWidth = width;
            mHeight = height;
            mJuggler = new SPJuggler();
            mPrevTouchstamp = new TouchStamp();
            mRenderSupport = new SPRenderSupport(device, defaultEffect, nonTexturedEffect, (int)width, (int)height, vertexBufferCapacity);
            mTouchProcessor = new SPTouchProcessor(this);
        }

        #endregion

        #region Fields
        private float mWidth;
        private float mHeight;
        private SPRenderSupport mRenderSupport;
        private SPJuggler mJuggler;
        private SPHashSet<SPTouch> mTouches = new SPHashSet<SPTouch>();
        private SPTouchProcessor mTouchProcessor;
        private TouchStamp mPrevTouchstamp;
        #endregion

        #region Properties
        public SPRenderSupport RenderSupport { get { return mRenderSupport; } }
        public SPJuggler Juggler { get { return mJuggler; } private set { mJuggler = value; } }

        public override float Width
        {
            get { return mWidth; }
            set { throw new InvalidOperationException("Cannot set width of stage."); }
        }

        public override float Height
        {
            get { return mHeight; }
            set { throw new InvalidOperationException("Cannot set height of stage."); }
        }
        #endregion

        #region Methods
        // TODO: Need preprocessor directives to distinguish PC, XBox and Phone versions. Move to TouchProcessor.
        public void ProcessTouches(MouseState mouseState, double timestamp, Matrix transform)
        {
            SPHashSet<SPTouch> touches = mTouches;
            TouchStamp touchstamp = new TouchStamp(mouseState, timestamp, transform);

            touches.Clear();
            if (touchstamp.State.LeftButton == ButtonState.Pressed)
            {
                SPTouch touch = new SPTouch();
                touch.GlobalX = touchstamp.X;
                touch.GlobalY = touchstamp.Y;
                touch.Timestamp = timestamp;

                if (mPrevTouchstamp.State.LeftButton != ButtonState.Pressed)
                {
                    // Click
                    touch.PreviousGlobalX = touchstamp.X;
                    touch.PreviousGlobalX = touchstamp.Y;
                    touch.Phase = SPTouchPhase.Begun;
                }
                else
                {
                    if (touchstamp.X == mPrevTouchstamp.X && touchstamp.Y == mPrevTouchstamp.Y)
                    {
                        // Stationary
                        touch.Phase = SPTouchPhase.Stationary;
                    }
                    else
                    {
                        // Drag
                        touch.Phase = SPTouchPhase.Moved;
                    }

                    touch.PreviousGlobalX = mPrevTouchstamp.X;
                    touch.PreviousGlobalY = mPrevTouchstamp.Y;
                }

                touches.Add(touch);
            }
            else
            {
                if (mPrevTouchstamp.State.LeftButton == ButtonState.Pressed)
                {
                    // Release
                    SPTouch touch = new SPTouch();
                    touch.GlobalX = touchstamp.X;
                    touch.GlobalY = touchstamp.Y;
                    touch.Timestamp = timestamp;
                    touch.PreviousGlobalX = mPrevTouchstamp.X;
                    touch.PreviousGlobalY = mPrevTouchstamp.Y;
                    touch.Phase = SPTouchPhase.Ended;
                    touches.Add(touch);
                }
            }

            mTouchProcessor.ProcessTouches(touches);
            mPrevTouchstamp = touchstamp;
        }

        public void AdvanceTime(double seconds)
        {
            mJuggler.AdvanceTime(seconds);
        }

        public override void Draw(GameTime gameTime, SPRenderSupport support, Matrix parentTransform)
        {
            support.BeginBatch();
            base.Draw(gameTime, support, parentTransform);
            support.EndBatch();
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!mIsDisposed)
                {
                    try
                    {
                        if (disposing)
                        {
                            if (mRenderSupport != null)
                            {
                                mRenderSupport.Dispose();
                                mRenderSupport = null;
                            }

                            if (mJuggler != null)
                                mJuggler = null;
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
            finally
            {
                base.Dispose(disposing);
            }
        }
        #endregion

        private struct TouchStamp
        {
            public TouchStamp(MouseState mouseState, double timestamp, Matrix transform)
            {
                Vector2 mousePos = new Vector2(mouseState.X, mouseState.Y);
                mousePos = Vector2.Transform(mousePos, transform);

                mState = mouseState;
                mX = mousePos.X;
                mY = mousePos.Y;
                mTimestamp = timestamp;
            }

            private MouseState mState;
            public MouseState State { get { return mState; } set { mState = value; } }
            private float mX;
            public float X { get { return mX; } set { mX = value; } }
            private float mY;
            public float Y { get { return mY; } set { mY = value; } }
            private double mTimestamp;
            public double Timestamp { get { return mTimestamp; } set { mTimestamp = value; } }
        }
    }
}
