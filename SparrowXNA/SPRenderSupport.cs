using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace SparrowXNA
{
    public class SPRenderSupport : IDisposable
    {
        private static uint s_Tag = 1;
        public SPRenderSupport(GraphicsDevice device, Effect defaultEffect, Effect nonTexturedEffect, int width, int height, int vertexBufferCapacity)
        {
            mTag = s_Tag++;
            mGraphicsDevice = device;
            mDefaultEffect = defaultEffect;
            mWidth = width;
            mHeight = height;
            mViewMatrix = Matrix.Identity;
            mProjectionMatrix = Matrix.Identity;
            mSamplerState = DefaultSamplerState;
            mPrimitiveBatch = new SPPrimitiveBatch(this);
            mSpriteBatch = new SPSpriteBatch(this);
            mCurrentRenderBatch = mPrimitiveBatch;

            mDefaultEffecter = new SPBasicEffecter(defaultEffect, DefaultEffectDraw);
            mDefaultEffecter.NonTexturedEffecter = new SPBasicEffecter(nonTexturedEffect, DefaultEffectNonTexturedDraw);
            mEffecterStack = new List<SPEffecter>();
            PushEffect(mDefaultEffecter);

            mDefaultBlendState = new BlendState(); // Dummy value
            mBlendStack = new List<BlendState>();
            PushBlendState(mDefaultBlendState);

            mDefaultRasterState = new RasterizerState(); // Dummy value
            mRasterStack = new List<RasterizerState>();
            PushRasterState(mDefaultRasterState);
            
            ResetDefaultEffect();
            ResetDefaultBlendState();
            ResetDefaultRasterizerState();
            ResetDefaultDepthStencilState();

            if (vertexBufferCapacity > 0 && SPVertexPositionNormalTextureColor.VertexBuffer == null)
                SPVertexPositionNormalTextureColor.PrimeVertexBufferWithSize(vertexBufferCapacity * 6, 6);
        }

        #region Fields
        private bool mIsDisposed = false;
        private bool mSuspended = false;
        private uint mTag;
        private int mWidth;
        private int mHeight;
        private GraphicsDevice mGraphicsDevice;
        private SPPrimitiveBatch mPrimitiveBatch;
        private SPSpriteBatch mSpriteBatch;
        private List<SPEffecter> mEffecterStack;
        private Effect mDefaultEffect;
        private SPBasicEffecter mDefaultEffecter;
        private SamplerState mSamplerState;
        private List<BlendState> mBlendStack;
        private BlendState mDefaultBlendState;
        private RasterizerState mDefaultRasterState;
        private List<RasterizerState> mRasterStack;
        private Matrix mViewMatrix;
        private Matrix mProjectionMatrix;
        private SPRenderBatch mCurrentRenderBatch;
        #endregion

        #region Properties
        public static RasterizerState NewDefaultRasterizerState
        {
            get
            {
                RasterizerState rasterizerState = new RasterizerState();
                rasterizerState.CullMode = CullMode.None;
                return rasterizerState;
            }
        }
        public static BlendState NewDefaultBlendState
        {
            get
            {
                BlendState blendState = new BlendState();
                blendState.AlphaSourceBlend = blendState.ColorSourceBlend = Blend.One;
                blendState.AlphaDestinationBlend = blendState.ColorDestinationBlend = Blend.InverseSourceAlpha;
                blendState.AlphaBlendFunction = blendState.ColorBlendFunction = BlendFunction.Add;
                return blendState;
            }
        }
        public int RenderWidth { get { return mWidth; } }
        public int RenderHeight { get { return mHeight; } }
        public GraphicsDevice GraphicsDevice { get { return mGraphicsDevice; } }
        private SPPrimitiveBatch PrimitiveBatch { get { return mPrimitiveBatch; } }
        private SPSpriteBatch SpriteBatch { get { return mSpriteBatch; } }
        public SPBasicEffecter DefaultEffecter
        {
            get
            {
                return mDefaultEffecter;
            }
        }
        public SPEffecter CurrentEffecter
        {
            get
            {
                return mEffecterStack[mEffecterStack.Count - 1]; // Stack always has at least one element
            }
        }
        public Texture2D DefaultTexture
        {
            get { return DefaultEffecter.Texture; }
            set
            {
                if (value != DefaultTexture)
                {
                    bool wasBatching = CurrentRenderBatch.IsBatching;
                    if (wasBatching) EndBatch();

                    DefaultEffecter.Texture = value;
                    DefaultEffecter.TextureEnabled = (value != null);

                    if (wasBatching) CurrentRenderBatch.Begin();
                }
            }
        }
        public SamplerState SamplerState
        {
            get { return mSamplerState; }
            set
            {
                if (value != mSamplerState)
                {
                    bool wasBatching = CurrentRenderBatch.IsBatching;
                    if (wasBatching) EndBatch();

                    mSamplerState = value;

                    if (wasBatching) CurrentRenderBatch.Begin();
                }
            }
        }
        public SamplerState DefaultSamplerState{ get { return SamplerState.LinearClamp; } }
        public bool IsUsingDefaultEffect { get { return CurrentEffecter == mDefaultEffecter; } }
        public RasterizerState DefaultRasterState
        {
            get
            {
                return mDefaultRasterState;
            }
        }
        public RasterizerState CurrentRasterState
        {
            get
            {
                return mRasterStack[mRasterStack.Count - 1]; // Stack always has at least one element
            }
        }
        public bool IsUsingDefaultRasterState { get { return CurrentRasterState == DefaultRasterState; } }
        public BlendState DefaultBlendState
        {
            get
            {
                return mDefaultBlendState;
            }
        }
        public BlendState CurrentBlendState
        {
            get
            {
                return mBlendStack[mBlendStack.Count - 1]; // Stack always has at least one element
            }
        }
        public bool IsUsingDefaultBlendState { get { return CurrentBlendState == DefaultBlendState; } }
        public Matrix ViewMatrix
        {
            get { return mViewMatrix; }
            set
            {
                mViewMatrix = value;
                DefaultEffecter.View = value;

                if (CurrentEffecter.Effect is IEffectMatrices)
                    ((IEffectMatrices)CurrentEffecter.Effect).View = value;
            }
        }
        public Matrix ProjectionMatrix
        {
            get { return mProjectionMatrix; }
            set
            {
                mProjectionMatrix = value;
                DefaultEffecter.Projection = value;

                if (CurrentEffecter.Effect is IEffectMatrices)
                    ((IEffectMatrices)CurrentEffecter.Effect).Projection = value;
            }
        }
        private SPRenderBatch CurrentRenderBatch
        {
            get
            {
                return mCurrentRenderBatch;
            }
            set
            {
                if (value == null)
                    throw new InvalidOperationException("Cannot set CurrentRenderBatch to null. There must be a valid SPRenderBatch at all times.");
                if (value != mCurrentRenderBatch)
                {
                    EndBatch();
                    mCurrentRenderBatch = value;
                }
            }
        }
        #endregion

        #region Methods
        public void SetDimensions(int width, int height)
        {
            mWidth = width;
            mHeight = height;
            ResetDefaultEffect();
        }

        private void DefaultEffectDraw(SPDisplayObject displayObject, GameTime gameTime, SPRenderSupport support, Matrix parentTransform)
        {
            if (displayObject is SPPrimitive == false)
                return;

            SPEffecter effecter = support.CurrentEffecter;
            effecter.EffectParameterForKey("tex").SetValue(DefaultTexture);
            support.AddPrimitive(displayObject as SPPrimitive, displayObject.TransformationMatrix * parentTransform);
        }

        private void DefaultEffectNonTexturedDraw(SPDisplayObject displayObject, GameTime gameTime, SPRenderSupport support, Matrix parentTransform)
        {
            if (displayObject is SPPrimitive == false)
                return;

            support.AddPrimitive(displayObject as SPPrimitive, displayObject.TransformationMatrix * parentTransform);
        }

        protected virtual void ApplyState()
        {
            bool wasBatching = CurrentRenderBatch.IsBatching;
            if (wasBatching) EndBatch();

            GraphicsDevice.RasterizerState = CurrentRasterState;
            GraphicsDevice.BlendState = CurrentBlendState;

            if (wasBatching) BeginBatch();
        }

        public void SuspendRendering(bool suspend)
        {
            if (suspend && !mSuspended)
            {
                if (CurrentRenderBatch.IsBatching)
                {
                    mSuspended = true;
                    EndBatch();
                }
            }
            else if (!suspend && mSuspended)
            {
                mSuspended = false;
                ApplyState();
                BeginBatch();
            }
        }

        public void PushEffect(SPEffecter effecter)
        {
            if (effecter == null)
                throw new ArgumentNullException("Cannot push a null Effecter.");

            bool wasBatching = CurrentRenderBatch.IsBatching;
            if (wasBatching) EndBatch();

            mEffecterStack.Add(effecter);

            if (effecter.Effect is IEffectMatrices)
                ((IEffectMatrices)effecter.Effect).View = mViewMatrix;

            if (wasBatching) CurrentRenderBatch.Begin();
        }

        public void PopEffect()
        {
            // Don't pop lowest element (default effect)
            if (mEffecterStack.Count > 1)
            {
                bool wasBatching = CurrentRenderBatch.IsBatching;
                if (wasBatching) EndBatch();

                mEffecterStack.RemoveAt(mEffecterStack.Count - 1);

                if (CurrentEffecter.Effect is IEffectMatrices)
                    ((IEffectMatrices)CurrentEffecter.Effect).View = mViewMatrix;
                //ResetDefaultBlendState();

                if (wasBatching) CurrentRenderBatch.Begin();
            }
        }

        public void PushBlendState(BlendState state)
        {
            if (state == null)
                throw new ArgumentNullException("Cannot push a null BlendState.");

            bool wasBatching = CurrentRenderBatch.IsBatching;
            if (wasBatching) EndBatch();

            mBlendStack.Add(state);
            GraphicsDevice.BlendState = state;

            if (wasBatching) CurrentRenderBatch.Begin();
        }

        public void PopBlendState()
        {
            // Don't pop lowest element (default blend state)
            if (mBlendStack.Count > 1)
            {
                bool wasBatching = CurrentRenderBatch.IsBatching;
                if (wasBatching) EndBatch();

                mBlendStack.RemoveAt(mBlendStack.Count - 1);
                GraphicsDevice.BlendState = CurrentBlendState;

                if (wasBatching) CurrentRenderBatch.Begin();
            }
        }

        public void PushRasterState(RasterizerState state)
        {
            if (state == null)
                throw new ArgumentNullException("Cannot push a null RasterizerState.");

            bool wasBatching = CurrentRenderBatch.IsBatching;
            if (wasBatching) EndBatch();

            mRasterStack.Add(state);
            GraphicsDevice.RasterizerState = state;

            if (wasBatching) CurrentRenderBatch.Begin();
        }

        public void PopRasterState()
        {
            // Don't pop lowest element (default raster state)
            if (mRasterStack.Count > 1)
            {
                bool wasBatching = CurrentRenderBatch.IsBatching;
                if (wasBatching) EndBatch();

                mRasterStack.RemoveAt(mRasterStack.Count - 1);
                GraphicsDevice.RasterizerState = CurrentRasterState;

                if (wasBatching) CurrentRenderBatch.Begin();
            }
        }

        public void ResetDefaultRasterizerState()
        {
            bool isBatching = CurrentRenderBatch.IsBatching;
            if (isBatching) EndBatch();

            // Remove old one from bottom of stack
            if (mRasterStack.Count > 0)
                mRasterStack.RemoveAt(0);

            mDefaultRasterState = NewDefaultRasterizerState;
            GraphicsDevice.RasterizerState = mDefaultRasterState;
            mRasterStack.Insert(0, mDefaultRasterState);

            if (isBatching) BeginBatch();
        }

        public void ResetDefaultDepthStencilState()
        {
            DepthStencilState state = new DepthStencilState();
            state.DepthBufferEnable = false;
            GraphicsDevice.DepthStencilState = state;
        }

        public void ResetDefaultEffect()
        {
            bool isBatching = CurrentRenderBatch.IsBatching;
            if (isBatching) EndBatch();

            // Remove old one from bottom of stack
            if (mEffecterStack.Count > 0)
                mEffecterStack.RemoveAt(0);

            //mDefaultEffecter = new SPBasicEffecter(defaultEffect, DefaultEffectDraw);
            mDefaultEffecter.World = Matrix.Identity;
            mDefaultEffecter.View = mViewMatrix;

            // See: http://blogs.msdn.com/b/shawnhar/archive/2010/04/05/spritebatch-and-custom-shaders-in-xna-game-studio-4-0.aspx
            Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);
            mProjectionMatrix = halfPixelOffset * Matrix.CreateOrthographicOffCenter(0, mWidth, mHeight, 0, 0, 5);

            //effect.Projection = Matrix.CreatePerspective(mWidth, mHeight, 1f, 100);

            mDefaultEffecter.Projection = mProjectionMatrix;
            mDefaultEffecter.TextureEnabled = true;
            mDefaultEffecter.Texture = null;

            // Add new one to bottom of stack
            mEffecterStack.Insert(0, mDefaultEffecter);

            // Let the Effects set these in HLSL
            //try
            //{
            //    SamplerStateCollection samplerStates = mGraphicsDevice.SamplerStates;
            //    for (int i = 0; i < 8; ++i)
            //    {
            //        SamplerState ss = new Microsoft.Xna.Framework.Graphics.SamplerState();
            //        ss.AddressU = samplerStates[i].AddressU;
            //        ss.AddressV = samplerStates[i].AddressV;
            //        ss.AddressW = samplerStates[i].AddressW;
            //        ss.Filter = samplerStates[i].Filter;
            //        ss.MaxAnisotropy = samplerStates[i].MaxAnisotropy;
            //        ss.MaxMipLevel = samplerStates[i].MaxMipLevel;
            //        ss.MipMapLevelOfDetailBias = samplerStates[i].MipMapLevelOfDetailBias;
            //        samplerStates[i] = ss;
            //    }
            //}
            //catch (Exception)
            //{
            //    /* ignore */
            //}

            // TODO
            // Print these out to check the default values
            //GraphicsDevice.SamplerStates[0].AddressU
            //GraphicsDevice.SamplerStates[0].AddressV
            //GraphicsDevice.SamplerStates[0].Filter
            //GraphicsDevice.SamplerStates[0].MaxMipLevel

            if (isBatching) BeginBatch();
        }

        public void ResetDefaultBlendState()
        {
            bool isBatching = CurrentRenderBatch.IsBatching;
            if (isBatching) EndBatch();

            // Remove old one from bottom of stack
            if (mBlendStack.Count > 0)
                mBlendStack.RemoveAt(0);

            mDefaultBlendState = NewDefaultBlendState;
            GraphicsDevice.BlendState = mDefaultBlendState;
            mBlendStack.Insert(0, mDefaultBlendState);

            if (isBatching) BeginBatch();
        }

        private void FlushBatch(SPRenderBatch batch)
        {
            if (batch != null && batch.IsBatching)
            {
                batch.End();
                batch.Begin();
            }
        }

        public void SetRenderTarget(RenderTarget2D target)
        {
            bool wasBatching = CurrentRenderBatch.IsBatching;

            if (wasBatching)
                CurrentRenderBatch.End();
            mGraphicsDevice.SetRenderTarget(target);

            if (wasBatching)
                CurrentRenderBatch.Begin();
        }

        public void BeginBatch()
        {
            if (mSuspended)
                throw new InvalidOperationException("BeginBatch called while rendering was suspended.");

            if (!CurrentRenderBatch.IsBatching)
                CurrentRenderBatch.Begin();
        }

        public void EndBatch()
        {
            if (CurrentRenderBatch.IsBatching)
                CurrentRenderBatch.End();
        }

        public void AddPrimitive(SPPrimitive primitive, Matrix transform)
        {
            if (CurrentRenderBatch != mPrimitiveBatch)
            {
                CurrentRenderBatch = mPrimitiveBatch;
                BeginBatch();
            }

            mPrimitiveBatch.AddPrimitive(primitive, transform);
        }

        public void AddImage(SPImage image, Matrix transform)
        {
            if (CurrentRenderBatch != mSpriteBatch)
            {
                CurrentRenderBatch = mSpriteBatch;
                BeginBatch();
            }

            mSpriteBatch.AddImage(image, transform);
        }

        public void AddText(SPTextField textField, Matrix transform)
        {
            if (CurrentRenderBatch != mSpriteBatch)
            {
                CurrentRenderBatch = mSpriteBatch;
                BeginBatch();
            }

            mSpriteBatch.AddText(textField, transform);
        }
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
                    // Don't dispose this in case it's still set on the GraphicsDevice
                    mDefaultBlendState = null;

                    if (mPrimitiveBatch != null)
                    {
                        mPrimitiveBatch.Dispose();
                        mPrimitiveBatch = null;
                    }

                    // Don't dispose this because it can crash after SPRenderTextures (don't know why!)
                    //if (mSpriteBatch != null)
                    //{
                    //    mSpriteBatch.Dispose();
                    //    mSpriteBatch = null;
                    //}
                }
                
                mIsDisposed = true;
            }
        }

        ~SPRenderSupport()
        {
            Dispose(false);
        }
        #endregion
    }
}
