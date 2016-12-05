#define PLIST_PARSER_3_0

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace SparrowXNA
{
    public class PlistParser
    {
        private PlistParser()
        {

        }

        public static List<object> ArrayFromPlist(string path)
        {
            using (Stream stream = TitleContainer.OpenStream(path))
            {
                if (stream == null)
                    throw new ArgumentException("Bad plist path provied to SparrowXNA.PlistParser: " + path);
#if PLIST_PARSER_3_0
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.DtdProcessing = DtdProcessing.Ignore;
                using (XmlReader reader = XmlReader.Create(stream, settings))
                {
                    reader.ReadToFollowing("array");
                    return ParseArrayElement(reader);
                }
            }
#else
                XDocument doc = XDocument.Load(stream);
                XElement container = doc.Element("plist");

                if (container != null)
                {
                    XElement element = container.Element("array");

                    if (element != null)
                        return ParseArrayElement(element);
                }

            }

            return null;
#endif
        }

        public static Dictionary<string, object> DictionaryFromPlist(string path)
        {
            using (Stream stream = TitleContainer.OpenStream(path))
            {
                if (stream == null)
                    throw new ArgumentException("Bad plist path provied to SparrowXNA.PlistParser: " + path);
#if PLIST_PARSER_3_0
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.DtdProcessing = DtdProcessing.Ignore;
                using (XmlReader reader = XmlReader.Create(stream, settings))
                {
                    reader.ReadToFollowing("dict");
                    return ParseDictElement(reader);
                }
            }
#else
                XDocument doc = XDocument.Load(stream);
                XElement container = doc.Element("plist");

                if (container != null)
                {
                    XElement element = container.Element("dict");

                    if (element != null)
                        return ParseDictElement(element);
                }
            }

            return null;
#endif
        }

#if PLIST_PARSER_3_0
        private static List<object> ParseArrayElement(XmlReader reader)
        {
            List<object> list = new List<object>();
            int depth = reader.Depth;

            while (reader.Read())
            {
                if (reader.Depth == depth)
                    break;

                if (!reader.IsStartElement())
                    continue;

                switch (reader.LocalName)
                {
                    case "string":
                        list.Add(reader.ReadElementContentAsString());
                        break;
                    case "integer":
                        list.Add(reader.ReadElementContentAsInt());
                        break;
                    case "real":
                        list.Add(reader.ReadElementContentAsFloat());
                        break;
                    case "true":
                        list.Add(true);
                        break;
                    case "false":
                        list.Add(false);
                        break;
                    case "dict":
                        list.Add(PlistParser.ParseDictElement(reader));
                        break;
                    case "array":
                        list.Add(PlistParser.ParseArrayElement(reader));
                        break;
                }
            }

            return list;
        }

        private static Dictionary<string, object> ParseDictElement(XmlReader reader)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            int depth = reader.Depth;

            string key = "";
            while (reader.Read())
            {
                if (reader.Depth == depth)
                    break;

                if (!reader.IsStartElement())
                    continue;

                switch (reader.LocalName)
                {
                    case "key":
                        key = reader.ReadElementContentAsString();
                        break;
                    case "string":
                        dict[key] = reader.ReadElementContentAsString();
                        break;
                    case "integer":
                        dict[key] = reader.ReadElementContentAsInt();
                        break;
                    case "real":
                        dict[key] = reader.ReadElementContentAsFloat();
                        break;
                    case "true":
                        dict[key] = true;
                        break;
                    case "false":
                        dict[key] = false;
                        break;
                    case "dict":
                        dict[key] = PlistParser.ParseDictElement(reader);
                        break;
                    case "array":
                        dict[key] = PlistParser.ParseArrayElement(reader);
                        break;
                }
            }

            return dict;
        }
#else
        private static List<object> ParseArrayElement(XElement element)
        {
            List<object> list = new List<object>();

            foreach (XElement child in element.Elements())
            {
                switch (child.Name.LocalName)
                {
                    case "string":
                        list.Add(child.Value);
                        break;
                    case "integer":
                        list.Add(int.Parse(child.Value));
                        break;
                    case "real":
                        list.Add(float.Parse(child.Value));
                        break;
                    case "true":
                        list.Add(true);
                        break;
                    case "false":
                        list.Add(false);
                        break;
                    case "dict":
                        list.Add(PlistParser.ParseDictElement(child));
                        break;
                    case "array":
                        list.Add(PlistParser.ParseArrayElement(child));
                        break;
                }
            }

            return list;
        }

        private static Dictionary<string, object> ParseDictElement(XElement element)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>(); 

            string key = "";
            foreach (XElement child in element.Elements())
            {
                switch (child.Name.LocalName)
                {
                    case "key":
                        key = child.Value;
                        break;
                    case "string":
                        dict[key] = child.Value;
                        break;
                    case "integer":
                        dict[key] = int.Parse(child.Value);
                        break;
                    case "real":
                        dict[key] = float.Parse(child.Value);
                        break;
                    case "true":
                        dict[key] = true;
                        break;
                    case "false":
                        dict[key] = false;
                        break;
                    case "dict":
                        dict[key] = PlistParser.ParseDictElement(child);
                        break;
                    case "array":
                        dict[key] = PlistParser.ParseArrayElement(child);
                        break;
                }
            }

            return dict;
        }
#endif
    }
}
