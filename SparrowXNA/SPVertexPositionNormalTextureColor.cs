
//#define SP_VERTEX_TYPE_NORMALS

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace SparrowXNA
{
    public struct SPVertexPositionNormalTextureColor : IVertexType
    {
        private Vector3 mPosition;
#if SP_VERTEX_TYPE_NORMALS
        private Vector3 mNormal;
#endif
        private Vector2 mTextureCoordinate;
        private Vector2 mShaderCoordinate;
        private Color mColor;

        public Vector3 Position { get { return mPosition; } set { mPosition = value; } }
#if SP_VERTEX_TYPE_NORMALS
        public Vector3 Normal { get { return mNormal; } set { mNormal = value; } }
#endif
        public Vector2 TextureCoordinate { get { return mTextureCoordinate; } set { mTextureCoordinate = value; } }
        public Vector2 ShaderCoordinate { get { return mShaderCoordinate; } set { mShaderCoordinate = value; } }
        public Color Color { get { return mColor; } set { mColor = value; } }

        public SPVertexPositionNormalTextureColor(Vector3 position, Vector3 normal, Vector2 texcoord, Color color)
        {
            mPosition = position;
#if SP_VERTEX_TYPE_NORMALS
            mNormal = normal;
#endif
            mTextureCoordinate = texcoord;
            mShaderCoordinate = Vector2.Zero;
            mColor = color;
        }

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
#if SP_VERTEX_TYPE_NORMALS
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(3 * sizeof(float), VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(6 * sizeof(float), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(8 * sizeof(float), VertexElementFormat.Color, VertexElementUsage.Color, 0)
#else
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(3 * sizeof(float), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(5 * sizeof(float), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(7 * sizeof(float), VertexElementFormat.Color, VertexElementUsage.Color, 0)
#endif
        );

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

#if SP_VERTEX_TYPE_NORMALS
        public const int SizeInBytes = 8 * sizeof(float) + sizeof(uint);
#else
        public const int SizeInBytes = 5 * sizeof(float) + sizeof(uint);
#endif

        // Vertex buffer optimization
        private static SPPoolIndexer s_VertexBufferIndexer = null;
        private static SPVertexPositionNormalTextureColor[] s_VertexBuffer = null;

        internal static void PrimeVertexBufferWithSize(int size, int segmentSize)
        {
            if (size <= 0 || segmentSize <= 0 || size % segmentSize != 0)
                return;

            PurgeVertexBuffer();

            int vertexCheckouts = size / segmentSize;
            s_VertexBuffer = new SPVertexPositionNormalTextureColor[size];
            s_VertexBufferIndexer = new SPPoolIndexer(vertexCheckouts, "SPVertexPositionNormalTextureColor");

            for (int i = 0; i < s_VertexBufferIndexer.Capacity; ++i)
                s_VertexBufferIndexer.InsertPoolIndex(i, i * segmentSize);
        }

        internal static void PurgeVertexBuffer()
        {
            s_VertexBufferIndexer = null;
            s_VertexBuffer = null;
        }

        internal static SPVertexPositionNormalTextureColor[] VertexBuffer { get { return s_VertexBuffer; } }

        internal static int CheckoutNextVertexBufferIndex()
        {
#if DEBUG
            if (s_VertexBufferIndexer.IndicesIndex > s_VertexBufferIndexer.Capacity)
                Debug.WriteLine("~*~*~*~* SPVertexPositionNormalTextureColor PoolIndex Empty: {0} *~*~*~*~", s_VertexBufferIndexer.IndicesIndex);
#endif
            if (s_VertexBufferIndexer != null)
                return s_VertexBufferIndexer.CheckoutNextIndex();
            else
                return -1;
        }

        internal static void CheckinVertexBufferIndex(int index)
        {
            if (s_VertexBufferIndexer != null)
                s_VertexBufferIndexer.CheckinIndex(index);
        }
    }
}
