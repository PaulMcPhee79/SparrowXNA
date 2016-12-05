using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;

namespace SparrowXNA
{
    class FontParser
    {
        private FontParser()
        {

        }

        public static void ParseFont(SPBitmapFont font)
        {
            if (font == null || font.Path == null) return;

            Dictionary<int, SPBitmapChar> bmChars = font.Chars;

            try
            {
                using (Stream stream = TitleContainer.OpenStream(font.Path))
                {
                    if (stream == null)
                        throw new ArgumentException("Bad font path provied to SparrowXNA.FontParser: " + font.Path);

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        FontToken token = null;
                        char[] separator = new char[] { ' ', '\t' };
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            if (line != null && line.Length > 0)
                            {
                                do
                                {
                                    if (line.StartsWith("char id=")) // By far the most common, so place first.
                                    {
                                        string[] elements = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                                        token = new FontToken();

                                        foreach (string element in elements)
                                        {
                                            if (element.StartsWith("id="))
                                            {
                                                token.CharID = int.Parse(element.Substring("id=".Length));
                                            }
                                            else if (element.StartsWith("x="))
                                            {
                                                token.X = int.Parse(element.Substring("x=".Length));
                                            }
                                            else if (element.StartsWith("y="))
                                            {
                                                token.Y = int.Parse(element.Substring("y=".Length));
                                            }
                                            else if (element.StartsWith("width="))
                                            {
                                                token.Width = int.Parse(element.Substring("width=".Length));
                                            }
                                            else if (element.StartsWith("height="))
                                            {
                                                token.Height = int.Parse(element.Substring("height=".Length));
                                            }
                                            else if (element.StartsWith("xoffset="))
                                            {
                                                token.XOffset = int.Parse(element.Substring("xoffset=".Length));
                                            }
                                            else if (element.StartsWith("yoffset="))
                                            {
                                                token.YOffset = int.Parse(element.Substring("yoffset=".Length));
                                            }
                                            else if (element.StartsWith("xadvance="))
                                            {
                                                token.XAdvance = int.Parse(element.Substring("xadvance=".Length));
                                            }
                                        }

                                        SPBitmapChar bitmapChar = new SPBitmapChar(
                                            token.CharID,
                                            new SPSubTexture(new SPRectangle(token.X, token.Y, token.Width, token.Height), font.Texture),
                                            new Vector2(token.XOffset, token.YOffset),
                                            token.XAdvance);
                                        bmChars[token.CharID] = bitmapChar;
                                    }
                                    else if (line.StartsWith("info face="))
                                    {
                                        string[] elements = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                                        foreach (string element in elements)
                                        {
                                            if (element.StartsWith("size="))
                                            {
                                                font.Size = int.Parse(element.Substring("size=".Length));
                                            }
                                        }
                                    }
                                    else if (line.StartsWith("common lineHeight="))
                                    {
                                        string[] elements = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                                        foreach (string element in elements)
                                        {
                                            if (element.StartsWith("face="))
                                            {
                                                if (font.Name == null)
                                                {
                                                    string str = element.Substring("face=".Length);

                                                    // Remove (possible) quotes
                                                    if (str.StartsWith("\""))
                                                        str = str.Substring(1);
                                                    if (str.EndsWith("\""))
                                                        str = str.Substring(0, str.Length - 1);
                                                    font.Name = str; 
                                                }
                                            }
                                            else if (element.StartsWith(SPBitmapFont.FntLineHeightToken + "="))
                                            {
                                                font.LineHeight = int.Parse(element.Substring((SPBitmapFont.FntLineHeightToken + "=").Length));
                                            }
                                        }
                                    }
                                }
                                while (false);
                            }
                        }
                    }
                }
            }
            catch (ArgumentException ae)
            {
                throw ae;
            }
            catch (Exception e)
            {
                // Perhaps do soemthing more meaningful here...
                throw e;
            }
        }
    }
}
