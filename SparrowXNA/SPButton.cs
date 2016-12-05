using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace SparrowXNA
{
    public class SPButton : SPDisplayObjectContainer
    {
        public const string SP_EVENT_TYPE_TRIGGERED = "triggered";
        private const float SP_BUTTON_MAX_DRAG_DIST = 80f;

        #region Contructors

        public SPButton(SPTexture upState, SPTexture downState = null, string text = null)
        {
            mUpState = upState;
            mDownState = (downState != null) ? downState : upState;
            mContents = new SPSprite();
            mBackground = new SPImage(upState);
            mScaleWhenDown = 0.9f;
            mAlphaWhenDisabled = 0.5f;
            mEnabled = true;
            mIsDown = false;
            mContents.AddChild(mBackground);
            AddChild(mContents);
            AddEventListener(SPTouchEvent.SP_EVENT_TYPE_TOUCH, new SPTouchEventHandler(OnTouch));

            if (text != null)
                Text = text;
        }

        #endregion

        #region Fields
        private SPTexture mUpState;
        private SPTexture mDownState;

        protected SPSprite mContents;
        private SPImage mBackground;
        private SPBitmapTextField mTextField;

        private float mScaleWhenDown;
        private float mAlphaWhenDisabled;
        private bool mEnabled;
        private bool mIsDown;

        private string mFontName;
        private int mFontSize = 1;
        private Color mFontColor = Color.White;
        private Vector2 mTextOffset = Vector2.Zero;
        #endregion

        #region Properties
        public override float Width
        {
            get
            {
                return mBackground.Width;
            }
            set
            {
                mBackground.Width = value;
                CreateTextField(mTextField.Text);
            }
        }
        public override float Height
        {
            get
            {
                return mBackground.Height;
            }
            set
            {
                mBackground.Height = value;
                CreateTextField(mTextField.Text);
            }
        }
        public float ScaleWhenDown { get { return mScaleWhenDown; } set { mScaleWhenDown = value; } }
        public float AlphaWhenDisabled { get { return mAlphaWhenDisabled; } set { mAlphaWhenDisabled = value; } }
        public bool IsDown { get { return mIsDown; } protected set { mIsDown = value; } }
        public bool Enabled
        {
            get { return mEnabled; }
            set
            {
                mEnabled = value;

                if (mEnabled)
                {
                    mContents.Alpha = 1.0f;
                }
                else
                {
                    mContents.Alpha = mAlphaWhenDisabled;
                    ResetContents();
                }
            }
        }
        public string Text
        {
            get { return (mTextField != null) ? mTextField.Text : ""; }
            set { CreateTextField(value); }
        }
        public string FontName
        {
            get { return mFontName; }
            set
            {
                mFontName = value;
                if (mTextField != null)
                    mTextField.FontName = value;
            }
        }
        public int FontSize
        {
            get { return mFontSize; }
            set
            {
                if (value > 0)
                {
                    mFontSize = value;
                    if (mTextField != null)
                        mTextField.FontSize = value;
                }
            }
        }
        public Color FontColor
        {
            get { return mFontColor; }
            set
            {
                mFontColor = value;
                if (mTextField != null)
                    mTextField.Color = value;
            }
        }
        public Vector2 TextOffset
        {
            get { return mTextOffset; }
            set
            {
                mTextOffset = value;
                if (mTextField != null)
                    mTextField.Origin = value;
            }
        }
        public Color Color
        {
            get { return (mBackground != null) ? mBackground.Color : Color.White; }
            set { if (mBackground != null) mBackground.Color = value; }
        }
        public SPTexture UpState
        {
            get { return mUpState; }
            set
            {
                if (mUpState != value)
                {
                    mUpState = value;
                    if (!mIsDown) mBackground.Texture = value;
                }
            }
        }
        public SPTexture DownState
        {
            get { return mDownState; }
            set
            {
                if (mDownState != value)
                {
                    mDownState = value;
                    if (mIsDown) mBackground.Texture = value;
                }
            }
        }
        public SPRectangle TextBounds
        {
            get
            {
                if (mTextField != null)
                    return mTextField.Bounds;
                else
                    return SPRectangle.Empty;
            }
        }
        #endregion

        #region Event Handlers
        public virtual void OnTouch(SPTouchEvent touchEvent)
        {
            if (!mEnabled) return;
            SPTouch touch = touchEvent.AnyTouch(touchEvent.TouchesWithTarget(this));
            if (touch == null) return;

            if (touch.Phase == SPTouchPhase.Begun && !mIsDown)
            {
                PressContents();
            }
            else if (touch.Phase == SPTouchPhase.Moved && mIsDown)
            {
                // Reset buttons when user dragged too far away after pushing
                SPRectangle buttonRect = BoundsInSpace(Stage);

                if (touch.GlobalX < buttonRect.X - SP_BUTTON_MAX_DRAG_DIST ||
                    touch.GlobalY < buttonRect.Y - SP_BUTTON_MAX_DRAG_DIST ||
                    touch.GlobalX > buttonRect.X + buttonRect.Width + SP_BUTTON_MAX_DRAG_DIST ||
                    touch.GlobalY > buttonRect.Y + buttonRect.Height + SP_BUTTON_MAX_DRAG_DIST)
                {
                    ResetContents();
                }
            }
            else if (touch.Phase == SPTouchPhase.Ended && mIsDown)
            {
                ResetContents();
                DispatchEvent(SPEvent.SPEventWithType(SP_EVENT_TYPE_TRIGGERED));
            }
            else if (touch.Phase == SPTouchPhase.Cancelled && mIsDown)
            {
                ResetContents();
            }
        }
        #endregion

        #region Methods
        public void AddContent(SPDisplayObject displayObject)
        {
            mContents.AddChild(displayObject);
        }

        public void AutomatedButtonDepress()
        {
            if (mEnabled && Touchable && !mIsDown)
                PressContents();
        }

        public void AutomatedButtonRelease(bool dispatch = true)
        {
            if (mEnabled && Touchable && mIsDown)
            {
                ResetContents();

                if (dispatch)
                    DispatchEvent(SPEvent.SPEventWithType(SP_EVENT_TYPE_TRIGGERED, true));
            }
        }

        protected virtual void PressContents()
        {
            mBackground.Texture = mDownState;
            mContents.Scale = new Vector2(mScaleWhenDown, mScaleWhenDown);
            mContents.X = (1.0f - mScaleWhenDown) / 2.0f * mBackground.Width;
            mContents.Y = (1.0f - mScaleWhenDown) / 2.0f * mBackground.Height;
            IsDown = true;
        }

        protected virtual void ResetContents()
        {
            mIsDown = false;
            mBackground.Texture = mUpState;
            mContents.X = mContents.Y = 0;
            mContents.Scale = new Vector2(1f, 1f);
        }

        private void CreateTextField(string text)
        {
            if (mFontName == null)
                throw new InvalidOperationException("SPButton - attempt to use SPTextField label with no font set.");

            if (mTextField == null)
            {
                mTextField = new SPBitmapTextField((text != null) ? text : "", FontName, FontSize);
                mTextField.X = TextOffset.X + (mBackground.Width - mTextField.Width) / 2;
                mTextField.Y = TextOffset.Y + (mBackground.Height - mTextField.Height) / 2;
                mTextField.HAlign = SPTextField.SPHAlign.Center;
                mTextField.VAlign = SPTextField.SPVAlign.Center;
                mTextField.Touchable = false;
                mContents.AddChild(mTextField);
            }
            else
            {
                mTextField.Text = text;
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
                        RemoveEventListener(SPTouchEvent.SP_EVENT_TYPE_TOUCH, this);
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
