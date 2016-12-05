using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace SparrowXNA
{
    internal class SPPrimitiveBatch : SPRenderBatch
    {
        public SPPrimitiveBatch(SPRenderSupport support)
            : base(support)
        {
            mSupportRef = new WeakReference(support);
        }

        #region Fields
        private const int DefaultBufferSize = 2048;
        private int mBufferIndex = 0;
        private int mNumVertsPerPrimitive = 3;
        private bool mIsDisposed = false;
        private SPVertexPositionNormalTextureColor[] mVertices = new SPVertexPositionNormalTextureColor[DefaultBufferSize];
        private WeakReference mSupportRef;
        #endregion

        #region Properties
        private SPRenderSupport Support
        {
            get { return (SPRenderSupport)mSupportRef.Target; }
            set { mSupportRef.Target = value; }
        }

        public override bool IsBatching { get; protected set; }
        #endregion

        #region Methods
        public override void Begin()
        {
            if (IsBatching)
                throw new InvalidOperationException("End must be called before Begin can be called again.");
            IsBatching = true;
        }

        public void AddPrimitive(SPPrimitive primitive, Matrix transform)
        {
            if (!IsBatching)
                throw new InvalidOperationException("Begin must be called before AddPrimitive can be called.");

            if (primitive == null)
                return;

            if (mBufferIndex + primitive.VertexCount > DefaultBufferSize)
                Flush();

            if (primitive.RequiresRenderTransform)
                transform = primitive.PreRenderTransformationMatrix * transform;

            int vertexCount = primitive.VertexCount, offset = primitive.VerticesOffset;

            for (int i = 0; i < vertexCount; ++i)
            {
                int index = mBufferIndex + i;
                mVertices[index] = primitive.Vertices[offset + i];
                mVertices[index].Position = Vector3.Transform(mVertices[index].Position, transform);
                mVertices[index].Color = mVertices[index].Color * primitive.Alpha;
                mVertices[index].ShaderCoordinate = primitive.ShaderCoordAtVertex(i);
            }

            if (primitive.Texture != null)
                primitive.Texture.AdjustTextureCoordinates(mVertices, mVertices, mBufferIndex, vertexCount);

            mBufferIndex += primitive.VertexCount;
        }

        public override void End()
        {
            if (!IsBatching)
                throw new InvalidOperationException("Begin must be called before End can be called.");

            Flush();
            IsBatching = false;
        }

        private void Flush()
        {
            if (!IsBatching)
                throw new InvalidOperationException("Begin must be called before Flush can be called.");

            if (mBufferIndex == 0)
                return;

            int primitiveCount = mBufferIndex / mNumVertsPerPrimitive;

            SPRenderSupport support = Support;
            support.GraphicsDevice.SamplerStates[0] = support.SamplerState;

            foreach (EffectPass pass in support.CurrentEffecter.Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                support.GraphicsDevice.DrawUserPrimitives<SPVertexPositionNormalTextureColor>(PrimitiveType.TriangleList, mVertices, 0, primitiveCount);
            }

            mBufferIndex = 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (!mIsDisposed)
            {
                try
                {
                    if (disposing)
                    {
                        Support = null; // Support may be disposed of after a Begin but before an End. We don't want to use it in this case.
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
    }
}
