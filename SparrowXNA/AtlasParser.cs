using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace SparrowXNA
{
    public class AtlasParser
    {
        private AtlasParser()
        {

        }

        public static List<Dictionary<string, object>> AtlasSubtextures(string path)
        {
            List<Dictionary<string, object>> subtextures = new List<Dictionary<string, object>>();

            using (Stream stream = TitleContainer.OpenStream(path))
            {
                if (stream == null)
                    throw new ArgumentException("Bad atlas path provied to SparrowXNA.AtlasParser: " + path);

                XDocument doc = XDocument.Load(stream);
                XElement container = doc.Element("TextureAtlas");
                

               if (container != null && container.HasAttributes)
               {
                   XAttribute imagePath = container.Attribute("imagePath");
                   if (imagePath == null)
                       throw new FormatException("Bad texture atlas format detected.");

                   subtextures.Add(new Dictionary<string, object>() { { imagePath.Name.LocalName, imagePath.Value } });
                   subtextures.AddRange(ParseSubTextures(container));
               }
            }

            return subtextures;
        }

        private static List<Dictionary<string, object>> ParseSubTextures(XElement element)
        {
            List<Dictionary<string, object>> subtextures = new List<Dictionary<string, object>>();

            foreach (XElement child in element.Elements())
            {
                if (!child.HasAttributes)
                    continue;

                Dictionary<string, object> subtexture = new Dictionary<string, object>();

                foreach (XAttribute attribute in child.Attributes())
                {
                    switch (attribute.Name.LocalName)
                    {
                        case "name":
                            subtexture["name"] = attribute.Value;
                            break;
                        case "x":
                            subtexture["x"] = float.Parse(attribute.Value);
                            break;
                        case "y":
                            subtexture["y"] = float.Parse(attribute.Value);
                            break;
                        case "width":
                            subtexture["width"] = float.Parse(attribute.Value);
                            break;
                        case "height":
                            subtexture["height"] = float.Parse(attribute.Value);
                            break;
                        case "frameX":
                            subtexture["frameX"] = float.Parse(attribute.Value);
                            break;
                        case "frameY":
                            subtexture["frameY"] = float.Parse(attribute.Value);
                            break;
                        case "frameWidth":
                            subtexture["frameWidth"] = float.Parse(attribute.Value);
                            break;
                        case "frameHeight":
                            subtexture["frameHeight"] = float.Parse(attribute.Value);
                            break;
                    }
                }

                if (subtexture.Count > 0)
                    subtextures.Add(subtexture);
            }

            return subtextures;
        }
    }
}
