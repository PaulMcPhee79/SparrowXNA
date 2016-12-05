using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SparrowXNA
{
    public class SPCamera
    {
        /*
            // Transforming Mouse to Object Coordinates
            mState = Mouse.GetState();
            Vector2 mouse = new Vector2(mState.X, mState.Y);
            mouse = Vector2.Transform(mouse, cam.InverseTransform);
         * 
         * To use the mouse to interact with objects that are shown using the camera, first the
         * mousestate should be captured, the x and y coordinates must then be used to populate
         * a Vector2. This Vector2 can then be transformed using the inverse of the camera
         * transform matrix. Now the X and Y of the Vector2 are the X and Y coordinates of the
         * Mouse in object space. You can use these coordinates in the same way you normally use
         * the mouse X and Y coordinates.
        */

        #region Constructors

        public SPCamera(Vector2 viewDimensions)
        {
            mZoom = 1.0f;
            mScroll = 0;
            mRotation = 0.0f;
            mPos = Vector2.Zero;

            mViewDimensions = viewDimensions;
            mZoomFactor = 1.0f;
            mTranslatationFactor = 1.0f;
            mRotationFactor = 1.0f;
        }

        #endregion

        #region Fields

        protected float mZoom;
        protected Matrix mTransform;
        protected Matrix mInverseTransform;
        protected Vector2 mPos;
        protected float mRotation;
        protected Int32 mScroll;

        protected float mZoomFactor;
        protected float mTranslatationFactor;
        protected float mRotationFactor;
        protected Vector2 mViewDimensions;

        #endregion

        #region Properties

        public float Zoom
        {
            get { return mZoom; }
            set { mZoom = value; }
        }
        /// <summary>
        /// Camera View Matrix Property
        /// </summary>
        public Matrix Transform
        {
            get { return mTransform; }
            set { mTransform = value; }
        }
        /// <summary>
        /// Inverse of the view matrix, can be used to get objects screen coordinates
        /// from its object coordinates
        /// </summary>
        public Matrix InverseTransform
        {
            get { return mInverseTransform; }
        }
        public Vector2 Pos
        {
            get { return mPos; }
            set { mPos = value; }
        }
        public float Rotation
        {
            get { return mRotation; }
            set { mRotation = value; }
        }
        public float ZoomFactor
        {
            get { return mZoomFactor; }
            set { mZoomFactor = value; }
        }

        public float TranslatationFactor
        {
            get { return mTranslatationFactor; }
            set { mTranslatationFactor = value; }
        }

        public float RotationFactor
        {
            get { return mRotationFactor; }
            set { mRotationFactor = value; }
        }
        public Vector2 ViewDimensions
        {
            get { return mViewDimensions; }
            set { mViewDimensions = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Update the camera view
        /// </summary>
        public void Update()
        {
            //Call Camera Input
            Input();

            //Clamp zoom value
            mZoom = MathHelper.Clamp(mZoom, 0.0f, 10.0f);

            //Clamp rotation value
            mRotation = SPUtils.ClampAngle(mRotation);

            //Create view matrix
#if true
            mTransform = Matrix.CreateTranslation(-mViewDimensions.X / 2 + mPos.X, -mViewDimensions.Y / 2 + mPos.Y, 0) *
                Matrix.CreateScale(new Vector3(mZoom, mZoom, 1)) *
                Matrix.CreateRotationZ(mRotation) *
                Matrix.CreateTranslation(mViewDimensions.X / 2, mViewDimensions.Y / 2, 0);
#else
            mTransform = Matrix.Identity;
#endif
            //Update inverse matrix
            mInverseTransform = Matrix.Invert(mTransform);
        }

        /// <summary>
        /// Example Input Method, rotates using cursor keys and zooms using mouse wheel
        /// </summary>
        protected virtual void Input()
        {
#if WINDOWS
            MouseState _mState = Mouse.GetState();
            KeyboardState _keyState = Keyboard.GetState();

            //Check zoom
            if (_mState.ScrollWheelValue > mScroll)
            {
                mZoom += 0.05f * mZoomFactor;
                mScroll = _mState.ScrollWheelValue;
            }
            else if (_mState.ScrollWheelValue < mScroll)
            {
                mZoom -= 0.05f * mZoomFactor;
                mScroll = _mState.ScrollWheelValue;
            }
#endif

#if false
            //Check rotation
            if (_keyState.IsKeyDown(Keys.Left))
            {
                mRotation -= 0.1f * mRotationFactor;
            }
            if (_keyState.IsKeyDown(Keys.Right))
            {
                mRotation += 0.1f * mRotationFactor;
            }

            //Check Move
            if (_keyState.IsKeyDown(Keys.A))
            {
                mPos.X -= 5.0f * mTranslatationFactor;
            }
            if (_keyState.IsKeyDown(Keys.D))
            {
                mPos.X += 5.0f * mTranslatationFactor;
            }
            if (_keyState.IsKeyDown(Keys.W))
            {
                mPos.Y -= 5.0f * mTranslatationFactor;
            }
            if (_keyState.IsKeyDown(Keys.S))
            {
                mPos.Y += 5.0f * mTranslatationFactor;
            }
#endif
        }

        #endregion
    }
}
