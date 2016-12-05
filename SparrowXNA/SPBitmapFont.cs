using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SparrowXNA
{
    public class SPBitmapFont
    {
        public const string kLineHeightTokenLineHeight = "lineHeight";
        public const string kLineHeightTokenBase = "base";

        private const int kCharSpace = 32;
        private const int kCharTab = 9;
        private static int kCharNewline = 10;
        private static string kNewLineString = new string((char)kCharNewline, 0); 

        public static void SetNewLineChar(int newLine)
        {
            kCharNewline = newLine;
            kNewLineString = new string((char)kCharNewline, 1);
        }

        public static string NewLineString { get { return kNewLineString; } }

        public SPBitmapFont(string path, SPTexture texture, string name = null)
        {
            mName = name;
            mPath = path;
            mLineHeight = mSize = 32;
            mFontTexture = texture;
            mChars = new Dictionary<int, SPBitmapChar>();
            FontParser.ParseFont(this);
        }

        #region Fields
        private SPTexture mFontTexture;
        private string mName = "unknown";
        private string mPath;
        private Dictionary<int, SPBitmapChar> mChars;
        private int mSize;
        private int mLineHeight;
        private Vector2 mTextOffset = new Vector2(0);
        private static string s_FntLineHeightToken = kLineHeightTokenLineHeight;
        #endregion

        #region Properties
        public string Name { get { return mName; } internal set { mName = value; } }
        public int Size { get { return mSize; } internal set { mSize = value; } }
        public int LineHeight { get { return mLineHeight; } set { mLineHeight = value; } }
        public Vector2 TextOffset { get { return mTextOffset; } set { mTextOffset = value; } }
        internal string Path { get { return mPath; } }
        internal SPTexture Texture { get { return mFontTexture; } }
        internal Dictionary<int, SPBitmapChar> Chars { get { return mChars; } }
        public static string FntLineHeightToken
        {
            get { return s_FntLineHeightToken; }
            set
            {
                if (value == null || Array.FindAll<string>(LineHeightTokens, s => s.Equals(value)).Length == 0)
                    throw new ArgumentException("Invalid LineHeight token provided to SPBitmapFont. Valid tokens found in SPBitmapFont.LineHeightTokens array.");
                s_FntLineHeightToken = value;
            }
        }
        public static string[] LineHeightTokens { get { return new string[] { kLineHeightTokenLineHeight, kLineHeightTokenBase }; } }
        #endregion

        #region Methods
        public SPBitmapChar CharByID(int charID)
        {
            return mChars != null && mChars.ContainsKey(charID) ? mChars[charID] : null;
        }

        public SPDisplayObject CreateDisplayObject(string text, int size, Color color,
            SPTextField.SPHAlign hAlign = SPTextField.SPHAlign.Left, SPTextField.SPVAlign vAlign = SPTextField.SPVAlign.Top, int borderWidth = 0, bool kerning = false)
        {
            return CreateDisplayObject(0, 0, text, size, color, hAlign, vAlign, borderWidth, kerning);
        }

        public SPDisplayObject CreateDisplayObject(float width, float height, string text, int size, Color color,
            SPTextField.SPHAlign hAlign = SPTextField.SPHAlign.Left, SPTextField.SPVAlign vAlign = SPTextField.SPVAlign.Top, int borderWidth = 0, bool kerning = false)
        {
            SPSprite lineContainer = new SPSprite();

            if (size < 0) size *= -Size;

            float scale = size / (float)Size;
            lineContainer.ScaleX = lineContainer.ScaleY = scale;
            float containerWidth = width / scale;
            float containerHeight = height / scale;

            int lastWhiteSpace = -1;
            int lastCharID = -1;
            float currentX = 0;
            SPSprite currentLine = new SPSprite();

            for (int i = 0; i < text.Length; ++i)
            {
                bool lineFull = false;

                int charID = text[i];
                if (charID == kCharNewline)
                {
                    lineFull = true;
                }
                else
                {
                    if (charID == kCharSpace || charID == kCharTab)
                        lastWhiteSpace = i;

                    SPBitmapChar bitmapChar = CharByID(charID);
                    if (bitmapChar == null) bitmapChar = CharByID(kCharSpace);
                    SPImage charImage = bitmapChar.CreateImage();

                    if (kerning)
                        currentX += bitmapChar.KerningToChar(lastCharID);

                    charImage.X = currentX + bitmapChar.Offset.X;
                    charImage.Y = bitmapChar.Offset.Y;

                    charImage.Color = color;
                    currentLine.AddChild(charImage);

                    currentX += bitmapChar.XAdvance;
                    lastCharID = charID;

                    if (currentX > containerWidth && containerWidth > 0)
                    {
                        // remove characters and add them again to next line
                        int numCharsToRemove = lastWhiteSpace == -1 ? 1 : i - lastWhiteSpace;
                        int removeIndex = currentLine.NumChildren - numCharsToRemove;

                        for (int temp = 0; temp < numCharsToRemove; ++temp)
                            currentLine.RemoveChildAtIndex(removeIndex);

                        if (currentLine.NumChildren == 0)
                            break;

                        SPDisplayObject lastChar = currentLine.ChildAtIndex(currentLine.NumChildren - 1);
                        currentX = lastChar.X + lastChar.Width;

                        i -= numCharsToRemove;
                        lineFull = true;
                    }
                }

                if (lineFull || i == text.Length - 1)
                {
                    float nextLineY = currentLine.Y + LineHeight;
                    lineContainer.AddChild(currentLine);

                    if (containerHeight == 0 || nextLineY + LineHeight <= containerHeight)
                    {
                        currentLine = new SPSprite();
                        currentLine.Y = nextLineY;
                        currentX = 0;
                        lastWhiteSpace = -1;
                        lastCharID = -1;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // hAlign
            if (hAlign != SPTextField.SPHAlign.Left && containerWidth != 0)
            {
                for (int childIndex = 0; childIndex < lineContainer.NumChildren; ++childIndex)
                {
                    SPSprite line = lineContainer.ChildAtIndex(childIndex) as SPSprite;
                    if (line.NumChildren == 0) continue;
                    SPDisplayObject lastChar = line.ChildAtIndex(line.NumChildren - 1);
                    float lineWidth = lastChar.X + lastChar.Width;
                    float widthDiff = containerWidth - lineWidth;
                    line.X = (int)(hAlign == SPTextField.SPHAlign.Right ? widthDiff : widthDiff / 2f);
                }
            }

            SPSprite outerContainer = new SPSprite(); // Should be an SPCompiledSprite, but that class doesn't yet exist...
            outerContainer.AddChild(lineContainer);

            if (vAlign != SPTextField.SPVAlign.Top)
            {
                float contentHeight = lineContainer.NumChildren * LineHeight * scale;
                float heightDiff = height - contentHeight;
                lineContainer.Y = (int)(vAlign == SPTextField.SPVAlign.Bottom ? heightDiff : heightDiff / 2f);
            }

            if (borderWidth > 0)
            {
                SPQuad topBorder = new SPQuad(width, borderWidth);
                SPQuad bottomBorder = new SPQuad(width, borderWidth);
                SPQuad leftBorder = new SPQuad(borderWidth, height - 2 * borderWidth);
                SPQuad rightBorder = new SPQuad(borderWidth, height - 2 * borderWidth);

                topBorder.Color = bottomBorder.Color = leftBorder.Color = rightBorder.Color = color;
                bottomBorder.Y = height - borderWidth;
                leftBorder.Y = rightBorder.Y = borderWidth;
                rightBorder.X = width - borderWidth;

                outerContainer.AddChild(topBorder);
                outerContainer.AddChild(bottomBorder);
                outerContainer.AddChild(leftBorder);
                outerContainer.AddChild(rightBorder);
            }

            return outerContainer;
        }
        #endregion
    }
}
