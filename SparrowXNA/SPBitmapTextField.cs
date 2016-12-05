using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SparrowXNA
{
    public class SPBitmapTextField : SPDisplayObjectContainer
    {
        public delegate SPBitmapFont FontSelect(string fontName, int fontSize, float displayScale, Dictionary<int, SPBitmapFont> fonts);

        public SPBitmapTextField(string text, string fontName, int fontSize, Color? color = null)
            : this(0, 0, text, fontName, fontSize, color) { }

        public SPBitmapTextField(float width, float height, string text, string fontName, int fontSize, Color? color = null)
        {
            mBaseWidth = width;
            mBaseHeight = height;
            mText = text;
            mFontSize = fontSize;
            mColor = color ?? Color.White;
            mHAlign = SPTextField.SPHAlign.Left;
            mVAlign = SPTextField.SPVAlign.Top;
            mBorderWidth = 0;
            mKerning = false;
            mFontName = fontName;

            mTextArea = new SPQuad(Math.Max(1, width), Math.Max(1, height));
            mTextArea.Visible = false;
            AddChild(mTextArea);

            mRequiresRedraw = true;
        }

        #region Fields
        protected int mFontSize;
        protected Color mColor;
        protected string mText;
        protected string mFontName;
        protected SPTextField.SPHAlign mHAlign;
        protected SPTextField.SPVAlign mVAlign;
        protected int mBorderWidth;
        protected float mBaseWidth;
        protected float mBaseHeight;
        protected bool mRequiresRedraw;
        protected bool mIsRenderedText;
        protected bool mKerning;
        protected bool mIsMultiColored = false;
        protected bool mIsTextOffset = false;
        private bool mIsLocalizable = true;
        protected SPQuad mTextArea;
        protected SPDisplayObject mContents;
        protected float mDisplayScale = 1f;
        protected Color[] mCharColors;
        protected static FontSelect mFontSelector;
        protected static Dictionary<string, Dictionary<int, SPBitmapFont>> s_BitmapFonts = null;
        #endregion

        #region Properties
        protected bool RequiresRedraw { get { return mRequiresRedraw; } set { mRequiresRedraw = value; } }
        public bool IsLocalizable { get { return mIsLocalizable; } set { mIsLocalizable = value; } }
        public bool IsMultiColored
        {
            get { return mIsMultiColored; }
            set
            {
                if (mIsMultiColored != value)
                {
                    mIsMultiColored = value;

                    if (mIsMultiColored)
                        InitCharColors(Color.White);
                    else
                        mCharColors = null;
                }
            }
        }
        public bool IsTextOffset
        {
            get { return mIsTextOffset; }
            set
            {
                if (mIsTextOffset != value)
                {
                    mIsTextOffset = value;

                    if (mContents != null && Font != null)
                    {
                        mContents.Origin = value ? Font.TextOffset : new Vector2(0);
                    }
                }
            }
        }
        public bool Kerning
        {
            get { return mKerning; }
            set
            {
                if (mKerning != value)
                {
                    mKerning = value;
                    RequiresRedraw = true;
                }
            }
        }
        public string Text
        {
            get { return mText; }
            set
            {
                if (mText == null || !mText.Equals(value))
                {
                    mText = value;

                    if (IsMultiColored)
                        InitCharColors(Color.White);

                    RequiresRedraw = true;
                }
            }
        }
        public string FontName
        {
            get { return mFontName; }
            set
            {
                if (mFontName == null || !mFontName.Equals(value))
                {
                    mFontName = value;
                    RequiresRedraw = true;
                }
            }
        }
        public int FontSize
        {
            get { return mFontSize; }
            set
            {
                if (mFontSize != value)
                {
                    mFontSize = value;
                    RequiresRedraw = true;
                }
            }
        }
        protected SPBitmapFont Font
        {
            get
            {
                if (RequiresRedraw)
                    RedrawContents();

                SPBitmapFont bitmapFont = null;

                if (mFontSelector != null)
                    bitmapFont = mFontSelector(FontName, FontSize, DisplayScale, s_BitmapFonts[FontName]);

                if (bitmapFont == null)
                    bitmapFont = NearestRegisteredFont(FontName, FontSize);

                return bitmapFont;

            }
        }
        public SPTextField.SPHAlign HAlign
        {
            get { return mHAlign; }
            set
            {
                if (mHAlign != value)
                {
                    mHAlign = value;
                    RequiresRedraw = true;
                }
            }
        }
        public SPTextField.SPVAlign VAlign
        {
            get { return mVAlign; }
            set
            {
                if (mVAlign != value)
                {
                    mVAlign = value;
                    RequiresRedraw = true;
                }
            }
        }
        public int BorderWidth
        {
            get { return mBorderWidth; }
            set
            {
                if (mBorderWidth != value)
                {
                    mBorderWidth = value;
                    RequiresRedraw = true;
                }
            }
        }
        public Color Color
        {
            get { return mColor; }
            set
            {
                if (mColor.PackedValue != value.PackedValue)
                {
                    mColor = value;
                    RequiresRedraw = true;
                }
            }
        }
        public SPRectangle TextBounds
        {
            get
            {
                if (RequiresRedraw) RedrawContents();
                return mTextArea.BoundsInSpace(Parent);
            }
        }
        public override float Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                mTextArea.Width = value;
                RequiresRedraw = true;
            }
        }
        public override float Height
        {
            get
            {
                return base.Height;
            }
            set
            {
                mTextArea.Height = value;
                RequiresRedraw = true;
            }
        }
        public SPRectangle BaseBounds
        {
            get
            {
                return new SPRectangle(X, Y, BaseWidth, BaseHeight);
            }
        }
        public Vector2 IconPosition
        {
            get
            {
                Vector2 tempVec = new Vector2(0);

                if (Text != null && Text.Length > 0)
                {
                    SPImage firstChar = CharAtIndex(0);
                    if (firstChar != null)
                    {
                        tempVec = firstChar.LocalToGlobal(new Vector2(0, firstChar.Height / 2));
                        tempVec = GlobalToLocal(tempVec);
                    }
                }

                return tempVec;
            }
        }
        public float BaseWidth { get { return mBaseWidth; } }
        public float BaseHeight { get { return mBaseHeight; } }
        protected float DisplayScale
        {
            get { return mDisplayScale; }
            private set
            {
                if (mDisplayScale != value)
                {
                    mDisplayScale = value;
                    RequiresRedraw = true;
                }
            }
        }
        public int LineCount
        {
            get
            {
                if (RequiresRedraw) RedrawContents();
                SPSprite lineContainer = LineContainer;
                return lineContainer != null ? lineContainer.NumChildren : 0;
            }
        }
#pragma warning disable 0162
        protected SPSprite LineContainer
        {
            get
            {
                do
                {
                    if (mContents == null)
                        break;
                    SPSprite outerContrainer = mContents as SPSprite;
                    if (outerContrainer == null || outerContrainer.NumChildren == 0)
                        break;
                    SPSprite lineContainer = outerContrainer.ChildAtIndex(0) as SPSprite;
                    if (lineContainer == null || lineContainer.NumChildren == 0)
                        break;
                    return lineContainer;
                } while (false);

                return null;
            }
        }
#pragma warning restore 0162
        #endregion

        #region Methods
        public static void SetFontSelector(FontSelect selector)
        {
            mFontSelector = selector;
        }

        protected void InitCharColors(Color color)
        {
            mCharColors = new Color[Math.Max(1, Text.Length)];
            for (int i = 0, iLimit = mCharColors.Length; i < iLimit; i++)
                mCharColors[i] = color;
        }

        protected SPImage CharAtIndex(int index)
        {
            if (index >= 0 && index < Text.Length)
            {
                SPSprite lineContainer = LineContainer;
                if (lineContainer != null)
                {
                    int currentIndex = 0;
                    for (int i = 0, numLines = lineContainer.NumChildren; i < numLines; i++)
                    {
                        SPSprite line = lineContainer.ChildAtIndex(i) as SPSprite;
                        if (line.NumChildren <= index)
                        {
                            currentIndex += line.NumChildren;
                            continue;
                        }

                        SPImage charImage = line.ChildAtIndex(index - currentIndex) as SPImage;
                        return charImage;
                    }
                }
            }

            return null;
        }

        public void SetColorAtIndex(Color color, int index)
        {
            SPImage charImage = CharAtIndex(index);
            if (charImage != null)
            {
                charImage.Color = color;

                if (!IsMultiColored)
                    IsMultiColored = true;
                mCharColors[index] = color;
            }
        }

        public void RefreshDisplayScale(bool forceImmediateRedraw = false)
        {
            float displayScale = ScaleY;
            SPDisplayObjectContainer parent = Parent;

            // Accumlate scalings up to the root parent (SPStage).
            while (parent != null)
            {
                displayScale *= parent.ScaleY;
                parent = parent.Parent;
            }

            if (DisplayScale != displayScale)
                DisplayScale = displayScale;

            if (forceImmediateRedraw)
                RedrawContents();
        }

        public override SPRectangle BoundsInSpace(SPDisplayObject targetCoordinateSpace)
        {
            return mTextArea.BoundsInSpace(targetCoordinateSpace);
        }

        public override void Draw(GameTime gameTime, SPRenderSupport support, Matrix parentTransform)
        {
            if (RequiresRedraw) RedrawContents();
            base.Draw(gameTime, support, parentTransform);
        }

        protected void RedrawContents()
        {
            RequiresRedraw = false; // Set before otherwise Properties will test for it and overflow the stack.
            SPDisplayObject temp = mContents;
            mContents = CreateComposedContents();
            AddChild(mContents);

            if (temp != null)
            {
                temp.RemoveFromParent();
                temp.Dispose();
                temp = null;
            }
        }

        protected virtual SPDisplayObject CreateComposedContents()
        {
            // No need to check for existence of a size because if there are no sizes, the FontName key will return null.
            if (s_BitmapFonts == null || !s_BitmapFonts.ContainsKey(FontName))
                throw new Exception("Bitmap font " + (FontName != null ? FontName : "NULL") + " not registered");

            SPBitmapFont bitmapFont = Font;

            SPDisplayObject contents = bitmapFont.CreateDisplayObject(
                BaseWidth,
                BaseHeight,
                Text,
                FontSize,
                Color,
                HAlign,
                VAlign,
                BorderWidth,
                Kerning);

            if (IsTextOffset)
                contents.Origin = bitmapFont.TextOffset;

            SPRectangle textBounds = (contents as SPDisplayObjectContainer).ChildAtIndex(0).Bounds;
            mTextArea.X = textBounds.X; mTextArea.Y = textBounds.Y;
            mTextArea.Width = textBounds.Width; mTextArea.Height = textBounds.Height;

            if (IsMultiColored)
                ApplyMultiColoring();

            return contents;
        }

        protected void ApplyMultiColoring()
        {
            if (mCharColors != null)
            {
                for (int i = 0, iLimit = mCharColors.Length; i < iLimit; i++)
                {
                    SPImage charImage = CharAtIndex(i);
                    if (charImage != null)
                        charImage.Color = mCharColors[i];
                    else
                        break;
                }
            }
        }

        public static SPBitmapFont NearestRegisteredFont(string fontName, int fontSize)
        {
            if (s_BitmapFonts == null || !s_BitmapFonts.ContainsKey(fontName))
                return null;

            float nearestScale = float.MaxValue;
            SPBitmapFont font = null;
            Dictionary<int, SPBitmapFont> fonts = s_BitmapFonts[fontName];
            foreach (KeyValuePair<int, SPBitmapFont> kvp in fonts)
            {
                float fontScale = Math.Abs(1f - (fontSize / (float)kvp.Key));
                if (fontScale < nearestScale)
                {
                    nearestScale = fontScale;
                    font = kvp.Value;
                }
            }

            return font;
        }

        public static SPBitmapFont RegisteredFont(string fontName, int fontSize = -1)
        {
            if (fontName == null || s_BitmapFonts == null || !s_BitmapFonts.ContainsKey(fontName)
                || (fontSize != -1 && !s_BitmapFonts[fontName].ContainsKey(fontSize)) || (fontSize == -1 && s_BitmapFonts[fontName].Count == 0))
                return null;
            else
            {
                if (fontSize == -1)
                {
                    Dictionary<int, SPBitmapFont>.ValueCollection.Enumerator it = s_BitmapFonts[fontName].Values.GetEnumerator();
                    if (it.MoveNext())
                        return it.Current;
                    else
                    {
#if DEBUG
                        throw new InvalidOperationException("SPBitmapFont::RegisteredFont - The Enumerator should be valid and should contain a value.");
#else
                        return null;
#endif
                    }
                }
                else
                    return s_BitmapFonts[fontName][fontSize];
            }
        }

        public static SPBitmapFont RegisterBitmapFont(string path, SPTexture texture, string fontName = null)
        {
            if (s_BitmapFonts == null) s_BitmapFonts = new Dictionary<string, Dictionary<int, SPBitmapFont>>();
            SPBitmapFont bitmapFont = new SPBitmapFont(path, texture, fontName);
            if (fontName == null) fontName = bitmapFont.Name;

            if (!s_BitmapFonts.ContainsKey(fontName))
                s_BitmapFonts.Add(fontName, new Dictionary<int, SPBitmapFont>());
            s_BitmapFonts[fontName][bitmapFont.Size] = bitmapFont;

            return bitmapFont;
        }

        public static void DeregisterBitmapFont(string fontName, int fontSize = -1)
        {
            if (fontName != null && s_BitmapFonts != null && s_BitmapFonts.ContainsKey(fontName))
            {
                if (fontSize == -1)
                    s_BitmapFonts.Remove(fontName);
                else if (s_BitmapFonts[fontName].ContainsKey(fontSize))
                {
                    s_BitmapFonts[fontName].Remove(fontSize);
                    if (s_BitmapFonts[fontName].Count == 0)
                        s_BitmapFonts.Remove(fontName);
                }

                if (s_BitmapFonts.Count == 0)
                    s_BitmapFonts = null;
            }
        }

        public static Vector2 MeasureString(string text, string fontName, int fontSize)
        {
            SPBitmapTextField temp = new SPBitmapTextField(text, fontName, fontSize);
            temp.RedrawContents();

            Vector2 strDimensions = new Vector2(temp.TextBounds.Width, temp.TextBounds.Height);
            temp.Dispose();

            return strDimensions;
        }
        #endregion
    }
}
