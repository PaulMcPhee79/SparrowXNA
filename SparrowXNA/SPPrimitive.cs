using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SparrowXNA
{
    public abstract class SPPrimitive : SPDisplayObject
    {
        public abstract Vector2 ShaderCoordAtVertex(int vertex);
        public abstract Color ColorAtVertex(int vertex);
        public abstract Color Color { get; set; }
        public abstract void SetColor(Color color, int vertex);
        public abstract SPTexture Texture { get; set; } // Don't dispose - another display object may be based off the same texture
        public abstract int VertexCount { get; }
        public abstract int VerticesOffset { get; }
        public abstract SPVertexPositionNormalTextureColor[] Vertices { get; protected set; }
    }
}
