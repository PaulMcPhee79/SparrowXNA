using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;


namespace SparrowXNA
{
    public class SPTextureAtlas : IDisposable
    {
        /*
        public SPTextureAtlas(AtlasData atlasData, SPTexture texture)
        {
            mAtlasTexture = texture;
            mTextureRegions = new Dictionary<string, SPRectangle>();
            mTextureFrames = new Dictionary<string, SPRectangle>();
            ParseAtlasData(atlasData);
        }
        */

        // libGDX format
        public SPTextureAtlas(List<AtlasTokenGDX> atlasData, SPTexture texture)
        {
            mAtlasTexture = texture;
            mTextureRegions = new Dictionary<string, SPRectangle>();
            mTextureFrames = new Dictionary<string, SPRectangle>();
            ParseAtlasDataGDX(atlasData);
        }

        // Sparrow format
        public SPTextureAtlas(List<Dictionary<string, object>> atlasData, SPTexture texture)
        {
            mAtlasTexture = texture;
            mTextureRegions = new Dictionary<string, SPRectangle>();
            mTextureFrames = new Dictionary<string, SPRectangle>();
            ParseAtlasData(atlasData);
        }

        #region Fields
        private bool mIsDisposed = false;
        private SPTexture mAtlasTexture;
        private Dictionary<string, SPRectangle> mTextureRegions;
        private Dictionary<string, SPRectangle> mTextureFrames;
        #endregion

        #region Properties
        public int Count { get { return mTextureRegions.Count; } }
        #endregion

        #region Methods
        public void ParseAtlasDataGDX(List<AtlasTokenGDX> atlasData)
        {
            foreach (AtlasTokenGDX subtexture in atlasData)
            {
                string name = subtexture.Name;
                SPRectangle region = new SPRectangle(
                    subtexture.XY.X,
                    subtexture.XY.Y,
                    subtexture.Size.W,
                    subtexture.Size.H);
                SPRectangle frame = new SPRectangle(
                    -subtexture.Offset.X,
                    -subtexture.Offset.Y,
                    subtexture.Orig.W,
                    subtexture.Orig.H);

                if (name != null)
                {
                    mTextureRegions.Add(name, region);
                    mTextureFrames.Add(name, frame);
                }
            }
        }

        // Sparrow format
        public void ParseAtlasData(List<Dictionary<string, object>> atlasData)
        {
            foreach (Dictionary<string, object> subtexture in atlasData)
            {
                string name = null;
                SPRectangle region = SPRectangle.Empty, frame = SPRectangle.Empty;

                foreach (KeyValuePair<string, object> kvp in subtexture)
                {
                    switch (kvp.Key)
                    {
                        case "name":
                            name = kvp.Value as string;
                            break;
                        case "x":
                            region.X = Convert.ToSingle(kvp.Value);
                            break;
                        case "y":
                            region.Y = Convert.ToSingle(kvp.Value);
                            break;
                        case "width":
                            region.Width = Convert.ToSingle(kvp.Value);
                            break;
                        case "height":
                            region.Height = Convert.ToSingle(kvp.Value);
                            break;
                        case "frameX":
                            frame.X = Convert.ToSingle(kvp.Value);
                            break;
                        case "frameY":
                            frame.Y = Convert.ToSingle(kvp.Value);
                            break;
                        case "frameWidth":
                            frame.Width = Convert.ToSingle(kvp.Value);
                            break;
                        case "frameHeight":
                            frame.Height = Convert.ToSingle(kvp.Value);
                            break;
                    }
                }

                if (name != null)
                {
                    mTextureRegions.Add(name, region);
                    mTextureFrames.Add(name, frame);
                }
            }
        }
        /*
        public void ParseAtlasData(AtlasData atlasData)
        {
            foreach (TextureRegionData regionData in atlasData.TextureRegions)
            {
                SPRectangle region = new SPRectangle(regionData.X, regionData.Y, regionData.Width, regionData.Height);
                mTextureRegions.Add(regionData.Name, region);

                SPRectangle frame = new SPRectangle(regionData.FrameX, regionData.FrameY, regionData.FrameWidth, regionData.FrameHeight);
                mTextureFrames.Add(regionData.Name, frame);
            }
        }
        */
        public SPTexture TextureByName(string name)
        {
            if (name != null && mTextureRegions.ContainsKey(name))
            {
                SPTexture texture = new SPSubTexture(mTextureRegions[name], mAtlasTexture);
                texture.Frame = mTextureFrames[name];
                return texture;
            }
            else
                return null;
        }

        public List<SPTexture> TexturesStartingWith(string name)
        {
            List<string> textureNames = new List<string>();

            foreach (string textureName in mTextureRegions.Keys)
            {
                if (textureName.StartsWith(name))
                    textureNames.Add(textureName);
            }

            textureNames.Sort();

            List<SPTexture> textures = new List<SPTexture>();
            foreach (string textureName in textureNames)
                textures.Add(TextureByName(textureName));
            return textures;
        }

        public void AddRegion(SPRectangle region, string name)
        {
            AddRegion(region, name, SPRectangle.Empty);
        }

        public void AddRegion(SPRectangle region, string name, SPRectangle frame)
        {
            mTextureRegions.Add(name, region);
            mTextureFrames.Add(name, frame);
        }

        public void RemoveRegion(string name)
        {
            mTextureRegions.Remove(name);
            mTextureFrames.Remove(name);
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!mIsDisposed)
            {
                if (disposing)
                {
                    if (mAtlasTexture != null)
                    {
                        //mAtlasTexture.Texture.Dispose(); // Clients must manage disposal as this is most likely owned by a ContentManager
                        mAtlasTexture.Dispose();
                        mAtlasTexture = null;
                    }

                    mTextureRegions = null;
                    mTextureFrames = null;
                }

                mIsDisposed = true;
            }
        }

        ~SPTextureAtlas()
        {
            Dispose(false);
        }
        #endregion
    }
}
