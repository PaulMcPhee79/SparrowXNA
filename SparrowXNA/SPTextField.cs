using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SparrowXNA
{
    public class SPTextField : SPDisplayObjectContainer
    {
        public enum SPHAlign
        {
            Left = 0,
            Center,
            Right
        }

        public enum SPVAlign
        {
            Top = 0,
            Center,
            Bottom
        }

        public const int kCachedMaxLen = 32;

        private static Dictionary<string, List<FontDescriptor>> s_RegisteredFonts = null;
        private static string s_newLine = "\n";
        private static string s_space = " ";
        private static string[] s_newLineArray = new string[] { s_newLine };
        private static string[] s_spaceArray = new string[] { s_space };

        private static int s_fontSizeAdjustment = 0;
        private static int s_NearestFontBias = 0;

        public static void SetNewLine(string newLine)
        {
            if (newLine.Length == 1)
                SPLocale.NewLineChar = newLine[0];
            s_newLine = newLine;
            s_newLineArray = new string[] { s_newLine };
        }

        public static void SetSpace(string space)
        {
            s_space = space;
            s_spaceArray = new string[] { s_space };
        }

        public static void SetFontSizeAdjustment(int value)
        {
            s_fontSizeAdjustment = value;
        }

        public static void SetNearestFontBias(int value)
        {
            s_NearestFontBias = value;
        }

        public static void PrimeTextCacheWithCapacity(int capacity)
        {
            TextLine.PrimeTextLineCacheWithCapacity(capacity);
        }

        public static Vector2 MeasureString(string text, string fontName, int fontSize)
        {
            if (text == null || fontName == null || fontSize <= 0)
                return Vector2.Zero;

            FontDescriptor fd = SPTextField.NearestRegisteredFont(fontName, fontSize);

            if (fd != null && fd.font != null)
            {
                // Ignore scaling. Draw will scale appropriately.
                //int baseFontSize = fd.size;
                //float fontPtScale = baseFontSize != 0 ? fontSize / (float)baseFontSize : 0;

                Vector2 textDimensions = fd.font.MeasureString(text);
                return new Vector2(textDimensions.X, textDimensions.Y);
            }
            else
                return Vector2.Zero;
        }

        public static SPTextField CachedSPTextField(float width, float height, string text, string fontName, int fontSize)
        {
            return new SPTextField(width, height, text, fontName, fontSize, true);
        }

        public SPTextField(float width, float height, string text, string fontName, int fontSize) : this(width, height, text, fontName, fontSize, false) { }

        public SPTextField(string text, string fontName, int fontSize = -1) : this(0, 0, text, fontName, fontSize) { }

        protected SPTextField(float width, float height, string text, string fontName, int fontSize, bool cached)
        {
            if (fontName == null || s_RegisteredFonts == null || SPTextField.RegisteredFont(fontName) == null)
                throw new InvalidOperationException("A font must be registered before it can be used.");

            mRequiresProcessing = true;
            mCached = cached;
            mFontName = fontName;
            mHAlign = SPHAlign.Center;
            mVAlign = SPVAlign.Center;
            mColor = Color.White;
            mLinesHelper = new List<string>();
            mWordsHelper = new List<string>();
            mLineBuilderHelper = new StringBuilder();
            mBaseFontSize = NearestRegisteredFont(fontName, fontSize + s_fontSizeAdjustment).size;
            mFontSize = fontSize + s_fontSizeAdjustment;
#if SANITIZE_TEXT
            mText = SPLocale.SanitizeText(text, fontName, mFontSize);
#else
            mText = text;
#endif
            mSpriteEffect = SpriteEffects.None;
            mVertices = new Vector2[4];
            mTextLines = new List<TextLine>();

            if (mText != null && Font != null && (width == 0 || height == 0))
            {
                List<string> lines = new List<string>();
                lines.AddRange(mText.Split(s_newLineArray, StringSplitOptions.RemoveEmptyEntries));

                Vector2 fontDimesions = Vector2.Zero;
                foreach (string line in lines)
                {
                    Vector2 lineDimensions = Font.MeasureString(line);
                    if (lineDimensions.X > fontDimesions.X)
                        fontDimesions.X = lineDimensions.X;
                    fontDimesions.Y += lineDimensions.Y;
                }

                fontDimesions.X *= FontPtScale.X;
                fontDimesions.Y *= FontPtScale.Y;

                if (width == 0)
                    width = fontDimesions.X;
                if (height == 0)
                    height = fontDimesions.Y;
            }

            if (cached)
            {
                mCachedBuilder = new StringBuilder(kCachedMaxLen, kCachedMaxLen);
                mCachedBuilder.Append(text);
            }

            // Font measurements are based on base font size, but we don't scale until draw phase,
            // so we compensate for this scaling difference here.
            Vector2 invFontScale = FontPtScale;
            invFontScale.X = 1f / invFontScale.X; invFontScale.Y = 1f / invFontScale.Y;
            mTextDimensions = new Vector2(width * invFontScale.X, height * invFontScale.Y);
            Compile();
            FillVertices();
        }

        #region Fields
        private bool mRequiresProcessing;
        private bool mIsLocalizable = true;
        private string mText;
        private string mFontName;
        private SPHAlign mHAlign;
        private SPVAlign mVAlign;
        private Vector2 mTextDimensions;
        private List<TextLine> mTextLines;
        private int mBaseFontSize;
        private int mFontSize;
        private Color mColor;
        private SpriteEffects mSpriteEffect;
        private Vector2[] mVertices;

        private List<string> mLinesHelper;
        private List<string> mWordsHelper;
        private StringBuilder mLineBuilderHelper;

        // Cached
        private bool mCached;
        private bool mDirtyCache;
        private string mCachedText;
        private StringBuilder mCachedBuilder;
        #endregion

        #region Properties
        public bool IsLocalizable { get { return mIsLocalizable; } set { mIsLocalizable = value; } }
        public string Text
        {
            get
            {
                if (mCached)
                {
                    if (mDirtyCache)
                    {
                        mCachedText = mCachedBuilder.ToString();
                        mDirtyCache = false;
                    }

                    return mCachedText;
                }
                else
                    return mText;
            }
            set
            {
                if (mCached)
                {
                    if (mCachedText == null || !mCachedText.Equals(value))
                    {
                        mCachedBuilder.Length = 0;
#if SANITIZE_TEXT
                        mCachedBuilder.Append(SPLocale.SanitizeText(value, FontName, FontSize));
#else
                        mCachedBuilder.Append(value);
#endif
                        mDirtyCache = true;

                        if (value == null)
                            ClearTextLines(mTextLines);
                        else
                            mRequiresProcessing = true;
                    }
                }
                else
                {
                    if (mText == null || !mText.Equals(value))
                    {
#if SANITIZE_TEXT
                        mText = SPLocale.SanitizeText(value, FontName, FontSize);
#else
                        mText = value;
#endif
                        if (value == null)
                            ClearTextLines(mTextLines);
                        else
                            mRequiresProcessing = true;
                    }
                }
            }
        }
        public StringBuilder CachedBuilder { get { return mCachedBuilder; } }
        public string FontName
        {
            get { return mFontName; }
            set
            {
                if (!mFontName.Equals(value))
                {
                    mFontName = value;
                    mRequiresProcessing = true;
                }
            }
        }
        public int FontSize
        {
            get { return mFontSize; }
            set
            {
                if (FontSize != value)
                {
                    // Undo old scale
                    Vector2 invFontScale = FontPtScale;
                    invFontScale.X = 1f / invFontScale.X; invFontScale.Y = 1f / invFontScale.Y;
                    mTextDimensions.X /= invFontScale.X; mTextDimensions.Y /= invFontScale.Y; 

                    // Update font size
                    mFontSize = Math.Max(1, value + s_fontSizeAdjustment);
                    mBaseFontSize = NearestRegisteredFont(mFontName, mFontSize).size;

                    // Apply new scale
                    invFontScale = FontPtScale;
                    invFontScale.X = 1f / invFontScale.X; invFontScale.Y = 1f / invFontScale.Y;
                    mTextDimensions.X *= invFontScale.X; mTextDimensions.Y *= invFontScale.Y;

                    FillVertices();
                    mRequiresProcessing = true;
                }
            }
        }
        public Vector2 FontPtScale { get { return new Vector2(mFontSize / (float)mBaseFontSize, mFontSize / (float)mBaseFontSize); } }
        public SpriteFont Font
        {
            get
            {
                if (mFontName != null)
                {
                    FontDescriptor fd = SPTextField.NearestRegisteredFont(mFontName, mFontSize);
                    return (fd != null) ? fd.font : null;
                }
                else
                    return null;
            }
        }
        public Color Color { get { return mColor; } set { mColor = value; } }
        public SPHAlign HAlign
        {
            get { return mHAlign; }
            set
            {
                if (mHAlign != value)
                {
                    mHAlign = value;
                    mRequiresProcessing = true;
                }
            }
        }
        public SPVAlign VAlign
        {
            get { return mVAlign; }
            set
            {
                if (mVAlign != value)
                {
                    mVAlign = value;
                    mRequiresProcessing = true;
                }
            }
        }
        public SpriteEffects SpriteEffect { get { return mSpriteEffect; } set { mSpriteEffect = value; } }
        internal List<TextLine> TextLines { get { return mTextLines; } }
        public SPRectangle TextBounds
        {
            get
            {
                return TextBoundsForLines(-1);
            }
        }
        public SPRectangle TrimmedTextBounds
        {
            get
            {
                if (mRequiresProcessing)
                    Compile();

                bool firstLine = true;
                SPRectangle rect = new SPRectangle();
                SpriteFont font = Font;
                Vector2 fontScale = FontPtScale;

                foreach (TextLine textLine in mTextLines)
                {
                    Vector2 lineDimensions = font.MeasureString(textLine.Text.Trim());
                    lineDimensions.X *= fontScale.X;
                    lineDimensions.Y *= fontScale.Y;

                    float xPos = XPositionForLineOfText(textLine.Text.Trim());

                    if (firstLine || xPos < rect.X)
                        rect.X = xPos;

                    if (lineDimensions.X > rect.Width)
                        rect.Width = lineDimensions.X;
                    rect.Height += lineDimensions.Y;

                    firstLine = false;
                }

                return rect;
            }
        }
        #endregion

        #region Methods
        public SPRectangle TextBoundsForLines(int lineCount)
        {
            if (mRequiresProcessing)
                Compile();

            bool firstLine = true;
            SPRectangle rect = new SPRectangle();
            SpriteFont font = Font;
            Vector2 fontScale = FontPtScale;

            int counter = 0;
            foreach (TextLine textLine in mTextLines)
            {
                Vector2 lineDimensions = font.MeasureString(textLine.Text);
                lineDimensions.X *= fontScale.X;
                lineDimensions.Y *= fontScale.Y;

                float xPos = XPositionForLineOfText(textLine.Text);

                if (firstLine || xPos < rect.X)
                    rect.X = xPos;

                if (lineDimensions.X > rect.Width)
                    rect.Width = lineDimensions.X;
                rect.Height += lineDimensions.Y;

                firstLine = false;

                if (++counter >= lineCount && lineCount != -1)
                    break;
            }

            return rect;
        }

        private void FillVertices()
        {
            float width = mTextDimensions.X, height = mTextDimensions.Y;

            for (int i = 0; i < mVertices.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        mVertices[i] = new Vector2(0, height);
                        break;
                    case 1:
                        mVertices[i] = new Vector2(0, 0);
                        break;
                    case 2:
                        mVertices[i] = new Vector2(width, height);
                        break;
                    case 3:
                        mVertices[i] = new Vector2(width, 0);
                        break;
                }
            }
        }

        public void ForceCompilation()
        {
            mRequiresProcessing = true;
        }

        private void CompileCachedText()
        {
            if (!mRequiresProcessing || Font == null || mCachedBuilder == null || mCachedBuilder.Length == 0)
                return;

            // if unset, init textDimensions
            if (SPMacros.SP_IS_FLOAT_EQUAL(mTextDimensions.X, 0) && SPMacros.SP_IS_FLOAT_EQUAL(mTextDimensions.Y, 0))
                mTextDimensions = Font.MeasureString(mCachedBuilder);

            // Add TextLine
            float height = Font.LineSpacing;
            float x = XPositionForLineOfText(mCachedBuilder);
            float y = 0;

            TextLine textLine = null;
            if (mTextLines.Count > 0)
            {
                textLine = mTextLines[0];
                textLine.TextHeight = height;
            }
            else
            {
                textLine = TextLine.GetTextLine(null, height);
                mTextLines.Add(textLine);
                AddChild(textLine);
            }

            textLine.X = x;
            textLine.Y = y;
            
            // Position textField within bounds
            LayoutText();

            mRequiresProcessing = false;
            mDirtyTransform = true;
        }

        public void Compile()
        {
            if (mCached)
            {
                CompileCachedText();
                return;
            }

            if (!mRequiresProcessing || mText == null || Font == null)
                return;

            ClearTextLines(mTextLines);

            bool singleLine = !mText.Contains(s_newLine) && !mText.Contains(s_space);
            SpriteFont font = Font;
            List<string> lines = mLinesHelper;
            StringBuilder lineBuilder = mLineBuilderHelper;

            lines.Clear();
            lineBuilder.Length = 0;

            // Compile
            if (singleLine)
            {
                // if unset, init textDimensions
                if (SPMacros.SP_IS_FLOAT_EQUAL(mTextDimensions.X, 0) && SPMacros.SP_IS_FLOAT_EQUAL(mTextDimensions.Y, 0))
                {
                    lines.Add(mText);
                    mTextDimensions = CalculateTextDimensions(lines);
                }

                AddTextLine(mText, font.LineSpacing);
            }
            else
            {
                // Potential multi-line text
                lines.AddRange(mText.Split(s_newLineArray, StringSplitOptions.RemoveEmptyEntries));

                // if unset, init textDimensions
                if (SPMacros.SP_IS_FLOAT_EQUAL(mTextDimensions.X, 0) && SPMacros.SP_IS_FLOAT_EQUAL(mTextDimensions.Y, 0))
                    mTextDimensions = CalculateTextDimensions(lines);

                List<string> words = mWordsHelper;
                Vector2 lineLen = Vector2.Zero, wordLen = Vector2.Zero;
                int wordIndex = 0, wordCount = 0;

                foreach (string line in lines)
                {
                    words.Clear();
                    words.AddRange(line.Split(s_spaceArray, StringSplitOptions.None));
                    wordIndex = 0;
                    wordCount = words.Count;

                    lineBuilder.Length = 0;
                    lineLen = Vector2.Zero;

                    foreach (string word in words)
                    {
                        string token = (wordIndex == wordCount - 1) ? word : word + s_space;
                        wordLen = font.MeasureString(token);

                        if ((wordLen.X + lineLen.X) > mTextDimensions.X)
                        {
                            token = word;
                            wordLen = font.MeasureString(token);
                        }

                        if ((wordLen.X + lineLen.X) >= (mTextDimensions.X + 1f))
                        {
                            AddTextLine(lineBuilder.ToString(), font.LineSpacing);
                            lineBuilder.Length = 0;

                            token = (wordIndex == wordCount - 1) ? word : word + s_space;
                            lineBuilder.Append(token);
                            wordLen = font.MeasureString(token);
                            lineLen.X = wordLen.X;
                        }
                        else
                        {
                            lineBuilder.Append(token);
                            lineLen.X += wordLen.X;
                        }

                        ++wordIndex;
                    }

                    AddTextLine(lineBuilder.ToString(), font.LineSpacing);
                }
            }

            // Position textField within bounds
            LayoutText();

            mRequiresProcessing = false;
            mDirtyTransform = true;
        }

        private void AddTextLine(string text, float height)
        {
            float x = XPositionForLineOfText(text);
            float y = mTextLines.Count * height;

            TextLine textLine = (mCached) ? TextLine.GetTextLine(text, height) : new TextLine(text, height);
            textLine.X = x;
            textLine.Y = y;
            mTextLines.Add(textLine);
            AddChild(textLine);
        }

        private Vector2 CalculateTextDimensions(List<string> textLines)
        {
            Vector2 dim = Vector2.Zero;

            foreach (string line in textLines)
            {
                Vector2 lineDim = Font.MeasureString(line);

                if (lineDim.X > dim.X)
                    dim.X = lineDim.X;
                dim.Y += lineDim.Y;
            }

            return dim;
        }

        private float XPositionForLineOfText(string text)
        {
            float x = 0;
            Vector2 textDim = Font.MeasureString(text);

            switch (mHAlign)
            {
                case SPHAlign.Center:
                    x = (mTextDimensions.X - textDim.X) / 2;
                    break;
                case SPHAlign.Left:
                    x = 0;
                    break;
                case SPHAlign.Right:
                    x = mTextDimensions.X - textDim.X;
                    break;
            }

            return x;
        }

        private float XPositionForLineOfText(StringBuilder builder)
        {
            float x = 0;
            Vector2 textDim = Font.MeasureString(builder);

            switch (mHAlign)
            {
                case SPHAlign.Center:
                    x = (mTextDimensions.X - textDim.X) / 2;
                    break;
                case SPHAlign.Left:
                    x = 0;
                    break;
                case SPHAlign.Right:
                    x = mTextDimensions.X - textDim.X;
                    break;
            }

            return x;
        }

        private void LayoutText()
        {
            float textHeight = 0, y = 0;

            foreach (TextLine textLine in mTextLines)
                textHeight += textLine.TextHeight;

            switch (mVAlign)
            {
                case SPVAlign.Center:
                    y = (mTextDimensions.Y - textHeight) / 2;
                    break;
                case SPVAlign.Top:
                    y = 0;
                    break;
                case SPVAlign.Bottom:
                    y = mTextDimensions.Y - textHeight;
                    break;
            }

            foreach (TextLine textLine in mTextLines)
                textLine.Y += y;
        }

        public override SPRectangle BoundsInSpace(SPDisplayObject targetCoordinateSpace)
        {
            float minX = float.PositiveInfinity, maxX = float.NegativeInfinity, minY = float.PositiveInfinity, maxY = float.NegativeInfinity;

            if (targetCoordinateSpace == this) // Optimization
            {
                for (int i = 0; i < 4; ++i)
                {
                    float x = mVertices[i].X;
                    float y = mVertices[i].Y;
                    minX = MathHelper.Min(minX, x);
                    maxX = MathHelper.Max(maxX, x);
                    minY = MathHelper.Min(minY, y);
                    maxY = MathHelper.Max(maxY, y);
                }
            }
            else
            {
                Matrix transform = TransformationMatrixToSpace(targetCoordinateSpace);
                Vector2 point;

                for (int i = 0; i < 4; ++i)
                {
                    point = mVertices[i];
                    Vector2 transformedPoint = Vector2.Transform(point, transform);
                    float tfX = transformedPoint.X;
                    float tfY = transformedPoint.Y;

                    minX = MathHelper.Min(minX, tfX);
                    maxX = MathHelper.Max(maxX, tfX);
                    minY = MathHelper.Min(minY, tfY);
                    maxY = MathHelper.Max(maxY, tfY);
                }
            }

            return new SPRectangle(minX, minY, maxX - minX, maxY - minY);
        }

        public static bool RegisteredFontContainsChar(char c, string fontName, int fontSize)
        {
            if (fontName == null)
                return false;

            FontDescriptor fd = NearestRegisteredFont(fontName, fontSize);
            return fd != null && fd.font.Characters.Contains(c);
        }

        private static FontDescriptor NearestRegisteredFont(string fontName, int fontSize)
        {
            if (s_RegisteredFonts != null && fontName != null && s_RegisteredFonts.ContainsKey(fontName))
            {
                int seekSize = fontSize + s_NearestFontBias, iter = 0, minIndex = -1, minDelta = 999999;
                List<FontDescriptor> fonts = s_RegisteredFonts[fontName];

                foreach (FontDescriptor font in fonts)
                {
                    int delta = Math.Abs(font.size - seekSize);
                    if (delta <= minDelta) // <= favours scaling down larger fonts instead of scaling up smaller fonts (when fonts are added smallest to largest).
                    {
                        minIndex = iter;
                        minDelta = delta;
                    }
                    ++iter;
                }

                if (minIndex != -1)
                    return fonts[minIndex];
            }

            return null;
        }

        private static FontDescriptor RegisteredFont(string fontName, int fontSize = -1)
        {
            if (s_RegisteredFonts != null && fontName != null && s_RegisteredFonts.ContainsKey(fontName))
            {
                List<FontDescriptor> fonts = s_RegisteredFonts[fontName];

                foreach (FontDescriptor font in fonts)
                {
                    if (fontSize == -1 || font.size == fontSize)
                        return font;
                }
            }

            return null;
        }

        public static bool ContainsRegisteredFont(string fontName, int fontSize = -1)
        {
            return RegisteredFont(fontName, fontSize) != null;
        }

        public static void RegisterFont(string fontName, int fontSize, SpriteFont font)
        {
            if (s_RegisteredFonts == null)
                s_RegisteredFonts = new Dictionary<string, List<FontDescriptor>>();
            if (fontName == null)
                throw new ArgumentNullException("Font name cannot be null.");

            List<FontDescriptor> fonts = null;

            if (s_RegisteredFonts.ContainsKey(fontName))
            {
                // Font name already registered. Check to see if this size is a duplicate, and add it if not.
                if (SPTextField.RegisteredFont(fontName, fontSize) == null)
                {
                    fonts = s_RegisteredFonts[fontName];
                    fonts.Add(new FontDescriptor(fontName, fontSize, font));
                }
            }
            else
            {
                // Add the new font name with the specified fontSize
                fonts = new List<FontDescriptor>();
                fonts.Add(new FontDescriptor(fontName, fontSize, font));
                s_RegisteredFonts[fontName] = fonts;
            }
        }

        public static void DeregisterFont(string fontName, int fontSize = -1)
        {
            if (s_RegisteredFonts == null)
                return;

            if (fontSize == -1)
                s_RegisteredFonts.Remove(fontName);
            else
            {
                if (s_RegisteredFonts.ContainsKey(fontName))
                {
                    List<FontDescriptor> fonts = s_RegisteredFonts[fontName];
                    if (fonts != null && fonts.Count > 0)
                    {
                        int i = 0;
                        foreach (FontDescriptor font in fonts)
                        {
                            if (font.size == fontSize)
                                break;
                            ++i;
                        }

                        if (i >= 0 && i < fonts.Count)
                        {
                            fonts.RemoveAt(i);
                            if (fonts.Count == 0)
                                s_RegisteredFonts.Remove(fontName);
                        }
                    }
                }
            }
        }

        public override Matrix TransformationMatrix
        {
            get
            {
                if (mDirtyTransform)
                {
                    mTransformCache = Matrix.Identity;

                    Vector2 origin = Origin, scale = Scale, pivot = Pivot, fontScale = FontPtScale;
#if true
                    Matrix temp;
                    if (origin.X != 0f || origin.Y != 0f)
                    {
                        Matrix.CreateTranslation(origin.X, origin.Y, 0f, out temp);
                        Matrix.Multiply(ref temp, ref mTransformCache, out mTransformCache);
                    }

                    if (Rotation != 0f)
                    {
                        Matrix.CreateRotationZ(Rotation, out temp);
                        Matrix.Multiply(ref temp, ref mTransformCache, out mTransformCache);
                    }

                    if (scale.X != 1f || scale.Y != 1f)
                    {
                        Matrix.CreateScale(scale.X, scale.Y, 1f, out temp);
                        Matrix.Multiply(ref temp, ref mTransformCache, out mTransformCache);
                    }

                    if (fontScale.X != 1f || fontScale.Y != 1f)
                    {
                        Matrix.CreateScale(fontScale.X, fontScale.Y, 1f, out temp);
                        Matrix.Multiply(ref temp, ref mTransformCache, out mTransformCache);
                    }

                    if (pivot.X != 0f || pivot.Y != 0)
                    {
                        Matrix.CreateTranslation(-pivot.X, -pivot.Y, 0f, out temp);
                        Matrix.Multiply(ref temp, ref mTransformCache, out mTransformCache);
                    }
#else
                    if (origin.X != 0f || origin.Y != 0f) mTransformCache = Matrix.CreateTranslation(origin.X, origin.Y, 0f) * mTransformCache;
                    if (Rotation != 0f) mTransformCache = Matrix.CreateRotationZ(Rotation) * mTransformCache;
                    if (scale.X != 1f || scale.Y != 1f) mTransformCache = Matrix.CreateScale(scale.X, scale.Y, 1f) * mTransformCache;
                    if (fontScale.X != 1f || fontScale.Y != 1f) mTransformCache = Matrix.CreateScale(fontScale.X, fontScale.Y, 1f) * mTransformCache;
                    if (pivot.X != 0f || pivot.Y != 0) mTransformCache = Matrix.CreateTranslation(-pivot.X, -pivot.Y, 0f) * mTransformCache;
#endif
                    mDirtyTransform = false;
                }

                return mTransformCache;
            }
        }

        public override void Draw(GameTime gameTime, SPRenderSupport support, Matrix parentTransform)
        {
            Compile();

            PreDraw(support);
            SPEffecter effecter = support.IsUsingDefaultEffect ? null : support.CurrentEffecter;

            if (effecter == null)
            {
#if true
                Matrix globalTransform, localTransform = TransformationMatrix;
                Matrix.Multiply(ref localTransform, ref parentTransform, out globalTransform);
#else
                Matrix globalTransform = TransformationMatrix * parentTransform;
#endif
                support.AddText(this, globalTransform);
            }
            else
            {
                effecter.CustomDraw(this, gameTime, support, parentTransform);
            }

            PostDraw(support);
        }

        internal void ClearTextLines(List<TextLine> textLines)
        {
            if (textLines == null)
                return;

            if (!mCached)
            {
                textLines.Clear();
                return;
            }

            foreach (TextLine textLine in textLines)
            {
                textLine.RemoveFromParent();

                if (textLine.PoolIndex != -1)
                    TextLine.CheckinTextLineBufferIndex(textLine.PoolIndex);
                else
                    textLine.Dispose();
            }

            textLines.Clear();
        }

        protected override void Dispose(bool disposing)
        {
            if (!mIsDisposed)
            {
                try
                {
                    if (disposing)
                    {
                        ClearTextLines(mTextLines);
                        
                        if (mLinesHelper != null)
                        {
                            mLinesHelper.Clear();
                            mLinesHelper = null;
                        }

                        if (mWordsHelper != null)
                        {
                            mWordsHelper.Clear();
                            mWordsHelper = null;
                        }

                        if (mLineBuilderHelper != null)
                        {
                            mLineBuilderHelper.Length = 0;
                            mLineBuilderHelper = null;
                        }
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

        private class FontDescriptor
        {
            public FontDescriptor(string fontName, int fontSize, SpriteFont spriteFont)
            {
                name = fontName;
                size = fontSize;
                font = spriteFont;
            }

            public string name;
            public int size;
            public SpriteFont font;
        }

        internal class TextLine : SPDisplayObject
        {
            private static SPPoolIndexer s_TextLineIndexer = null;
            private static TextLine[] s_TextLineCache = null;

            public static void PrimeTextLineCacheWithCapacity(int capacity)
            {
                if (capacity <= 0 || s_TextLineCache != null)
                    return;

                s_TextLineIndexer = new SPPoolIndexer(capacity, "SPTextLine");
                s_TextLineIndexer.InitIndexes(0, 1);
                s_TextLineCache = new TextLine[capacity];

                for (int i = 0; i < capacity; ++i)
                    s_TextLineCache[i] = new TextLine(null, 0);
            }

            public static int CheckoutNextTextLineBufferIndex()
            {
                if (s_TextLineIndexer != null)
                    return s_TextLineIndexer.CheckoutNextIndex();
                else
                    return -1;
            }

            public static void CheckinTextLineBufferIndex(int index)
            {
                if (s_TextLineIndexer != null)
                    s_TextLineIndexer.CheckinIndex(index);
            }

            public static TextLine GetTextLine(string text, float textHeight)
            {
                TextLine textLine = null;
                int index = CheckoutNextTextLineBufferIndex();

                if (index != -1)
                {
                    textLine = s_TextLineCache[index];
                    textLine.Text = text;
                    textLine.TextHeight = textHeight;
                }
                else
                    textLine = new TextLine(text, textHeight);

                textLine.PoolIndex = index;
                return textLine;
            }

            public TextLine(string text, float textHeight)
            {
                PoolIndex = -1;
                Text = text;
                TextHeight = textHeight;
            }

            public int PoolIndex { get; set; }
            public string Text { get; set; }
            public float TextHeight { get; set; }

            public override void Draw(GameTime gameTime, SPRenderSupport support, Matrix parentTransform)
            {
                // Do nothing
            }

            public override SPRectangle BoundsInSpace(SPDisplayObject targetCoordinateSpace)
            {
                return Parent.BoundsInSpace(targetCoordinateSpace);
            }
        }
    }
}
