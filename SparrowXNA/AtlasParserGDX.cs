using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace SparrowXNA
{
    public class AtlasParserGDX
    {
        private AtlasParserGDX()
        {

        }

        public static List<AtlasTokenGDX> AtlasSubtextures(string path)
        {
            List<AtlasTokenGDX> subtextures = new List<AtlasTokenGDX>(20);

            try
            {
                using (Stream stream = TitleContainer.OpenStream(path))
                {
                    if (stream == null)
                        throw new ArgumentException("Bad atlas path provied to SparrowXNA.AtlasParserGDX: " + path);

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        AtlasTokenGDX token = null;
                        int totalLineCounter = 0, lineCounter = 0;
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            if (line != null && line.Length > 0)
                            {

                                if (!(token == null && lineCounter == 0 || token != null && lineCounter != 0))
                                    throw new Exception("AtlasParserGDX::AtlasSubtextures - invalid internal state.");

                                do
                                {
                                    // Image name
                                    if (totalLineCounter == 0)
                                    {
                                        token = new AtlasTokenGDX();
                                        token.ImageName = line;
                                        subtextures.Add(token);
                                        token = null;
                                        break;
                                    }

                                    // Atlas size
                                    if (totalLineCounter == 1)
                                        break;

                                    // Ignore format, filter, repeat.
                                    if (line.StartsWith("format:") || line.StartsWith("filter:") || line.StartsWith("repeat:"))
                                        break;

                                    if (token == null)
                                        token = new AtlasTokenGDX();

                                    switch (lineCounter)
                                    {
                                        case 0: // name:
                                            {
                                                token.Name = line;
                                            }
                                            break;
                                        case 1: // rotate:
                                            {
                                                string str = line.Substring(line.IndexOf(": ") + 2);
                                                token.Rotate = bool.Parse(str);
                                            }
                                            break;
                                        case 2: // xy
                                            {
                                                int xStart = line.IndexOf(": ") + 2;
                                                int xLen = line.IndexOf(", ") - xStart;
                                                string strX = line.Substring(xStart, xLen);

                                                int yStart = line.IndexOf(", ") + 2;
                                                string strY = line.Substring(yStart);

                                                token.XY = new SPCoord(int.Parse(strX), int.Parse(strY));
                                            }
                                            break;
                                        case 3: // size
                                            {
                                                int wStart = line.IndexOf(": ") + 2;
                                                int wLen = line.IndexOf(", ") - wStart;
                                                string strW = line.Substring(wStart, wLen);

                                                int hStart = line.IndexOf(", ") + 2;
                                                string strH = line.Substring(hStart);

                                                token.Size = new SPSize(int.Parse(strW), int.Parse(strH));
                                            }
                                            break;
                                        case 4: // orig
                                            {
                                                int wStart = line.IndexOf(": ") + 2;
                                                int wLen = line.IndexOf(", ") - wStart;
                                                string strW = line.Substring(wStart, wLen);

                                                int hStart = line.IndexOf(", ") + 2;
                                                string strH = line.Substring(hStart);

                                                token.Orig = new SPSize(int.Parse(strW), int.Parse(strH));
                                            }
                                            break;
                                        case 5: // Offset
                                            {
                                                int xStart = line.IndexOf(": ") + 2;
                                                int xLen = line.IndexOf(", ") - xStart;
                                                string strX = line.Substring(xStart, xLen);

                                                int yStart = line.IndexOf(", ") + 2;
                                                string strY = line.Substring(yStart);

                                                token.Offset = new SPCoord(int.Parse(strX), int.Parse(strY));
                                            }
                                            break;
                                        case 6: // index
                                            {
                                                string str = line.Substring(line.IndexOf(": ") + 2);
                                                token.Index = int.Parse(str);

                                                if (token.Index != -1)
                                                {
                                                    if (token.Index < 10)
                                                        token.Name += "_0" + str;
                                                    else
                                                        token.Name += "_" + str;
                                                }
                                            }
                                            break;
                                        default:
                                            Debug.Assert(false, "AtlasParserGDX::AtlasSubtextures - invalid atlas file.");
                                            break;
                                    }

                                    ++lineCounter;

                                    if (lineCounter == 7)
                                    {
                                        subtextures.Add(token);
                                        token = null;
                                        lineCounter = 0;
                                    }
                                }
                                while (false);
                               
                                ++totalLineCounter;
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

            return subtextures;
        }
    }
}
