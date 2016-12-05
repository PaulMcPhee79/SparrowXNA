
//#define SP_VERTEX_TYPE_NORMALS

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace SparrowXNA
{
    public class SPQuad : SPPrimitive
    {
        //  Client     Actual
        //   0 - 1     1 - 3
        //   | / |     | / |
        //   2 - 3     0 - 2
        public SPQuad(float width, float height)
        {
            mTexture = null;
            mVerticesOffset = SPVertexPositionNormalTextureColor.CheckoutNextVertexBufferIndex();

            if (mVerticesOffset != -1)
                mVertices = SPVertexPositionNormalTextureColor.VertexBuffer;
            else
                mVertices = new SPVertexPositionNormalTextureColor[NumVertices];

            FillVertices(width, height, Color.White);
        }

        public SPQuad(SPTexture texture)
        {
            mTexture = texture;
            mVerticesOffset = SPVertexPositionNormalTextureColor.CheckoutNextVertexBufferIndex();

            if (mVerticesOffset != -1)
                mVertices = SPVertexPositionNormalTextureColor.VertexBuffer;
            else
                mVertices = new SPVertexPositionNormalTextureColor[NumVertices];

            if (mTexture != null)
                FillVertices(MathHelper.Max(texture.Width, texture.Frame.Width), MathHelper.Max(texture.Height, texture.Frame.Height), Color.White);
            else
                FillVertices(0, 0, Color.White);
        }

        #region Fields
        private const int NumVertices = 6;
        private bool mIsVerticesCentered = false;
        private SPTexture mTexture;
        private int mVerticesOffset;
        private SPVertexPositionNormalTextureColor[] mVertices;
        #endregion

        #region Properties
        public bool IsVerticesCentered
        {
            get { return mIsVerticesCentered; }
            set
            {
                if (mIsVerticesCentered != value)
                {
                    mIsVerticesCentered = value;
                    if (mTexture != null)
                        FillVertices(MathHelper.Max(mTexture.Width, mTexture.Frame.Width), MathHelper.Max(mTexture.Height, mTexture.Frame.Height), Color);
                }
            }
        }
        public override SPTexture Texture
        {
            get { return mTexture; }
            set
            {
                mTexture = value;

                if (mTexture != null)
                    FillVertices(MathHelper.Max(mTexture.Width, mTexture.Frame.Width), MathHelper.Max(mTexture.Height, mTexture.Frame.Height), Color);
            }
        }
        public override SPVertexPositionNormalTextureColor[] Vertices { get { return mVertices; } protected set { mVertices = value; } }
        public override int VerticesOffset { get { return (mVerticesOffset == -1) ? 0 : mVerticesOffset; } }
        public override int VertexCount { get { return NumVertices; } }
        public override Color Color
        {
            get
            {
                return ColorAtVertex(0); // Only valid for non-gradient quads
            }

            set
            {
                for (int i = 0; i < 4; i++)
                    SetColor(value, i);
            }
        }
        public override bool RequiresRenderTransform
        {
            get { return (mTexture != null) ? !mTexture.Frame.IsEmpty : false; }
        }
        public override Matrix PreRenderTransformationMatrix
        {
            get
            {
                if (RequiresRenderTransform)
                {
                    SPRectangle frame = mTexture.Frame;
                    Matrix transform = Matrix.CreateTranslation(-frame.X, -frame.Y, 0f);
                    transform = Matrix.CreateScale(mTexture.Width / frame.Width, mTexture.Height / frame.Height, 1f) * transform;
                    return transform;
                }
                else
                {
                    return Matrix.Identity;
                }
            }
        }
        #endregion

        #region Methods
        private static readonly Vector2[] kShaderCoords = new Vector2[]
        {
            new Vector2(0, 1),
            new Vector2(0, 0),
            new Vector2(1, 1),
            new Vector2(1, 1),
            new Vector2(0, 0),
            new Vector2(1, 0)
        };
        public override Vector2 ShaderCoordAtVertex(int vertex)
        {
            if (vertex >= 0 && vertex < kShaderCoords.Length)
                return kShaderCoords[vertex];
            else
                return Vector2.Zero;
        }

        //  Client     Actual
        //   0 - 1     1 - 3
        //   | / |     | / |
        //   2 - 3     0 - 2
        public override Color ColorAtVertex(int vertex)
        {
            if (vertex < 0 || vertex > 3)
                throw new ArgumentOutOfRangeException("SPQuad color vertex must be between 0 and 3.");

            Color color;
            int offset = VerticesOffset;

            switch (vertex)
            {
                case 0:
                    color = mVertices[offset + 1].Color;
                    break;
                case 1:
                    color = mVertices[offset + 5].Color;
                    break;
                case 2:
                    color = mVertices[offset + 0].Color;
                    break;
                case 3:
                    color = mVertices[offset + 2].Color;
                    break;
                default:
                    color = mVertices[offset + vertex].Color;
                    break;
            }

            return color;
        }

        //  Client     Actual
        //   0 - 1     1 - 3
        //   | / |     | / |
        //   2 - 3     0 - 2
        public override void SetColor(Color color, int vertex)
        {
            if (vertex < 0 || vertex > 3)
                throw new ArgumentOutOfRangeException("SPQuad color vertex must be between 0 and 3.");

            int offset = VerticesOffset;

            switch (vertex)
            {
                case 0:
                    mVertices[offset + 1].Color = color;
                    mVertices[offset + 4].Color = color;
                    break;
                case 1:
                    mVertices[offset + 5].Color = color;
                    break;
                case 2:
                    mVertices[offset + 0].Color = color;
                    break;
                case 3:
                    mVertices[offset + 2].Color = color;
                    mVertices[offset + 3].Color = color;
                    break;
                default:
                    mVertices[offset + vertex].Color = color;
                    break;
            }
        }

        public void SetDimensions(float width, float height)
        {
            FillVertices(width, height, Color);
        }

        private void FillVertices(float width, float height, Color color)
        {
            float z = 0f;
            int offset = VerticesOffset;

            for (int i = 0; i < NumVertices; i++)
            {
                int index = offset + i;

                switch (i)
                {
                    case 0:
                        mVertices[index].Position = IsVerticesCentered ? new Vector3(-width / 2, height / 2, z) : new Vector3(0, height, z);
                        mVertices[index].TextureCoordinate = new Vector2(0.0f, 1.0f);
                        break;
                    case 1:
                        mVertices[index].Position = IsVerticesCentered ? new Vector3(-width / 2, -height / 2, z) : new Vector3(0, 0, z);
                        mVertices[index].TextureCoordinate = new Vector2(0.0f, 0.0f);
                        break;
                    case 2:
                        mVertices[index].Position = IsVerticesCentered ? new Vector3(width / 2, height / 2, z) : new Vector3(width, height, z);
                        mVertices[index].TextureCoordinate = new Vector2(1.0f, 1.0f);
                        break;
                    case 3:
                        mVertices[index].Position = IsVerticesCentered ? new Vector3(width / 2, height / 2, z) : new Vector3(width, height, z);
                        mVertices[index].TextureCoordinate = new Vector2(1.0f, 1.0f);
                        break;
                    case 4:
                        mVertices[index].Position = IsVerticesCentered ? new Vector3(-width / 2, -height / 2, z) : new Vector3(0, 0, z);
                        mVertices[index].TextureCoordinate = new Vector2(0.0f, 0.0f);
                        break;
                    case 5:
                        mVertices[index].Position = IsVerticesCentered ? new Vector3(width / 2, -height / 2, z) : new Vector3(width, 0, z);
                        mVertices[index].TextureCoordinate = new Vector2(1.0f, 0.0f);
                        break;
                }

#if SP_VERTEX_TYPE_NORMALS
                mVertices[index].Normal = Vector3.Backward;
#endif
                mVertices[index].Color = color;
            }
        }

        public Vector2 TexCoordAtVertex(int vertex)
        {
            if (vertex < 0 || vertex > 3)
                throw new ArgumentOutOfRangeException("SPQuad texture vertex must be between 0 and 3.");

            Vector2 texCoord;
            int offset = VerticesOffset;

            switch (vertex)
            {
                case 0:
                    texCoord = mVertices[offset + 1].TextureCoordinate;
                    break;
                case 1:
                    texCoord = mVertices[offset + 5].TextureCoordinate;
                    break;
                case 2:
                    texCoord = mVertices[offset + 0].TextureCoordinate;
                    break;
                case 3:
                    texCoord = mVertices[offset + 2].TextureCoordinate;
                    break;
                default:
                    texCoord = mVertices[offset + vertex].TextureCoordinate;
                    break;
            }

            return texCoord;
        }

        public void SetTexCoord(Vector2 coord, int vertex)
        {
            if (vertex < 0 || vertex > 3)
                throw new ArgumentOutOfRangeException("SPQuad texture vertex must be between 0 and 3.");

            int offset = VerticesOffset;

            switch (vertex)
            {
                case 0:
                    mVertices[offset + 1].TextureCoordinate = coord;
                    mVertices[offset + 4].TextureCoordinate = coord;
                    break;
                case 1:
                    mVertices[offset + 5].TextureCoordinate = coord;
                    break;
                case 2:
                    mVertices[offset + 0].TextureCoordinate = coord;
                    break;
                case 3:
                    mVertices[offset + 2].TextureCoordinate = coord;
                    mVertices[offset + 3].TextureCoordinate = coord;
                    break;
                default:
                    mVertices[offset + vertex].TextureCoordinate = coord;
                    break;
            }
        }

        public override SPRectangle BoundsInSpace(SPDisplayObject targetCoordinateSpace)
        {
            if (mVertices == null)
                return new SPRectangle();

            float minX = float.PositiveInfinity, maxX = float.NegativeInfinity, minY = float.PositiveInfinity, maxY = float.NegativeInfinity;
            int offset = VerticesOffset;

            if (targetCoordinateSpace == this) // Optimization
            {
                for (int i = 0; i < NumVertices; ++i)
                {
                    int index = offset + i;
                    float x = mVertices[index].Position.X;
                    float y = mVertices[index].Position.Y;
                    minX = MathHelper.Min(minX, x);
                    maxX = MathHelper.Max(maxX, x);
                    minY = MathHelper.Min(minY, y);
                    maxY = MathHelper.Max(maxY, y);
                }
            }
            /*
            else if (targetCoordinateSpace == Parent && Rotation == 0f) // Optimization
            {
                float scaleX = ScaleX, scaleY = ScaleY;
                SPRectangle rect = new SPRectangle(
                    X - PivotX * scaleX,
                    Y - PivotY * scaleY,
                    mVertices[2].Position.X * scaleX,
                    mVertices[2].Position.Y * scaleY);
                if (scaleX < 0f) { rect.Width *= -1; rect.X -= rect.Width; }
                if (scaleY < 0f) { rect.Height *= -1; rect.Y -= rect.Height; }
                return rect;
            }
            */
            else
            {
                Matrix transform = TransformationMatrixToSpace(targetCoordinateSpace);
                Vector2 point = Vector2.Zero;

                for (int i = 0; i < NumVertices; ++i)
                {
                    Vector3 pos3 = mVertices[offset + i].Position;
                    point = new Vector2(pos3.X, pos3.Y);
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

        public override void PreDraw(SPRenderSupport support)
        {
            base.PreDraw(support);

            if (Texture != null && Texture.Repeat)
                support.SamplerState = SamplerState.LinearWrap;
        }

        public override void Draw(GameTime gameTime, SPRenderSupport support, Matrix parentTransform)
        {
            PreDraw(support);
            SPEffecter effecter = support.IsUsingDefaultEffect ? null : support.CurrentEffecter;

            if (effecter == null || effecter.CustomDraw == null)
            {
#if true
                Matrix globalTransform, localTransform = TransformationMatrix;
                Matrix.Multiply(ref localTransform, ref parentTransform, out globalTransform);
#else
                Matrix globalTransform = TransformationMatrix * parentTransform;
#endif
                support.DefaultTexture = Texture != null ? Texture.Texture : null;
                support.AddPrimitive(this, globalTransform);
            }
            else
            {
                effecter.CustomDraw(this, gameTime, support, parentTransform);
            }

            PostDraw(support);
        }

        protected override void Dispose(bool disposing)
        {
            if (!mIsDisposed)
            {
                try
                {
                    if (mVerticesOffset != -1)
                    {
                        SPVertexPositionNormalTextureColor.CheckinVertexBufferIndex(mVerticesOffset);
                        mVerticesOffset = -1;
                    }

                    mVertices = null;
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
    }
}
