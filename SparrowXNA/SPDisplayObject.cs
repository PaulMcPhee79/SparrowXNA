using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
//using System.Runtime.CompilerServices;

namespace SparrowXNA
{
    public abstract class SPDisplayObject : SPEventDispatcher, IDisposable
    {
        public SPDisplayObject()
        {
            mParentRef = new WeakReference(null);
            mBlendState = null; // Uses default blend state when null
            mEffecter = null; // Uses default effect when null
            mTag = 0;
            Rotation = 0f;
            Visible = true;
            Touchable = true;
            Alpha = 1f;
            Origin = Vector2.Zero;
            Pivot = Vector2.Zero;
            Scale = Vector2.One;
        }

        #region Fields
        protected bool mIsDisposed = false;
        protected bool mDirtyTransform = true;
        private uint mTag;
        protected Matrix mTransformCache = Matrix.Identity;
        private Vector2 mOrigin;
        private Vector2 mPivot;
        private Vector2 mScale;
        private float mRotation;
        private float mAlpha;
        private WeakReference mParentRef; // Don't want a circular reference
        private BlendState mBlendState;
        private SPEffecter mEffecter;
        #endregion

        #region Properties
        public virtual Vector2 Origin { get { return mOrigin; } set { mOrigin = value; mDirtyTransform = true; } }
        public virtual Vector2 Pivot { get { return mPivot; } set { mPivot = value; mDirtyTransform = true; } }
        public virtual Vector2 Scale { get { return mScale; } set { mScale = value; mDirtyTransform = true; } }
        public virtual float Rotation {
            get { return mRotation; }
            set
            {
                while (value < -SPMacros.PI) value += SPMacros.TWO_PI;
                while (value > SPMacros.PI) value -= SPMacros.TWO_PI;
                mRotation = value;
                mDirtyTransform = true;
            }
        }
        public virtual float Alpha { get { return mAlpha; } set { mAlpha = MathHelper.Max(0f, MathHelper.Min(1f, value)); } }
        public virtual bool Visible { get; set; }
        public virtual bool Touchable { get; set; }
        public uint Tag { get { return mTag; } set { mTag = value; } }
        public SPDisplayObjectContainer Parent {
            get { return (SPDisplayObjectContainer)mParentRef.Target; }
            set { mParentRef.Target = value; }
        }
        public virtual float X { get { return mOrigin.X; } set { mOrigin.X = value; mDirtyTransform = true; } }
        public virtual float Y { get { return mOrigin.Y; } set { mOrigin.Y = value; mDirtyTransform = true; } }
        public virtual float PivotX { get { return mPivot.X; } set { mPivot.X = value; mDirtyTransform = true; } }
        public virtual float PivotY { get { return mPivot.Y; } set { mPivot.Y = value; mDirtyTransform = true; } }
        public virtual float ScaleX { get { return mScale.X; } set { mScale.X = value; mDirtyTransform = true; } }
        public virtual float ScaleY { get { return mScale.Y; } set { mScale.Y = value; mDirtyTransform = true; } }
        public virtual BlendState BlendState { get { return mBlendState; } set { mBlendState = value; } }
        public virtual SPEffecter Effecter { get { return mEffecter; } set { mEffecter = value; } }
        public virtual SPRectangle Bounds { get { return BoundsInSpace(Parent); } }
        public virtual float Width {
            get
            {
                return BoundsInSpace(Parent).Width;
            }
            set
            {
                ScaleX = 1.0f;
                float actualWidth = Width;
                if (actualWidth != 0.0f) ScaleX = value / actualWidth;
                else ScaleX = 1.0f;
            }
        }
        public virtual float Height
        {
            get
            {
                return BoundsInSpace(Parent).Height;
            }
            set
            {
                ScaleY = 1.0f;
                float actualHeight = Height;
                if (actualHeight != 0.0f) ScaleY = value / actualHeight;
                else ScaleY = 1.0f;
            }
        }
        public SPDisplayObject Root
        {
            get
            {
                SPDisplayObject currentObject = this;
                while (currentObject.Parent != null)
                    currentObject = currentObject.Parent;
                return currentObject;
            }
        }
        public SPStage Stage
        {
            get
            {
                SPDisplayObject root = Root;
                if (root is SPStage) return (SPStage)root;
                else return null;
            }
        }
        public virtual bool RequiresRenderTransform
        {
            get { return false; }
        }
        public virtual Matrix PreRenderTransformationMatrix
        {
            get
            {
                throw new MissingMethodException("Method needs to be implemented in subclass.");
            }
        }
        public virtual Matrix TransformationMatrix
        {
            get
            {
                if (mDirtyTransform)
                {
                    mTransformCache = Matrix.Identity;
#if true
                    Matrix temp;
                    if (mOrigin.X != 0f || mOrigin.Y != 0f)
                    {
                        Matrix.CreateTranslation(mOrigin.X, mOrigin.Y, 0f, out temp);
                        Matrix.Multiply(ref temp, ref mTransformCache, out mTransformCache);
                    }

                    if (mRotation != 0f)
                    {
                        Matrix.CreateRotationZ(mRotation, out temp);
                        Matrix.Multiply(ref temp, ref mTransformCache, out mTransformCache);
                    }

                    if (mScale.X != 1f || mScale.Y != 1f)
                    {
                        Matrix.CreateScale(mScale.X, mScale.Y, 1f, out temp);
                        Matrix.Multiply(ref temp, ref mTransformCache, out mTransformCache);
                    }

                    if (mPivot.X != 0f || mPivot.Y != 0)
                    {
                        Matrix.CreateTranslation(-mPivot.X, -mPivot.Y, 0f, out temp);
                        Matrix.Multiply(ref temp, ref mTransformCache, out mTransformCache);
                    }
#else
                    if (mOrigin.X != 0f || mOrigin.Y != 0f) mTransformCache = Matrix.CreateTranslation(mOrigin.X, mOrigin.Y, 0f) * mTransformCache;
                    if (mRotation != 0f) mTransformCache = Matrix.CreateRotationZ(mRotation) * mTransformCache;
                    if (mScale.X != 1f || mScale.Y != 1f) mTransformCache = Matrix.CreateScale(mScale.X, mScale.Y, 1f) * mTransformCache;
                    if (mPivot.X != 0f || mPivot.Y != 0) mTransformCache = Matrix.CreateTranslation(-mPivot.X, -mPivot.Y, 0f) * mTransformCache;
#endif
                    mDirtyTransform = false;
                }

                return mTransformCache;
            }
        }
        #endregion

        #region Methods
        // This method is used very often during touch testing, so we optimized the code.
        // We use a static array to save the ancestors because drawing will not be threaded.
        private static SPDisplayObject[] s_ancestors = new SPDisplayObject[SPMacros.SP_MAX_DISPLAY_TREE_DEPTH];
        public Matrix TransformationMatrixToSpace(SPDisplayObject targetCoordinateSpace)
        {
            SPDisplayObject currentObject = null;

            if (targetCoordinateSpace == this)
                return Matrix.Identity;
            else if (targetCoordinateSpace == Parent || (targetCoordinateSpace == null && Parent == null))
                return TransformationMatrix;
            else if (targetCoordinateSpace == null || targetCoordinateSpace == Root)
            {
                // targetCoordinateSpace == null represents that target coordinate of the root object.
                // -> move up from this to root
                Matrix matrix = Matrix.Identity;
                currentObject = this;

                while (currentObject != targetCoordinateSpace)
                {
                    matrix *= currentObject.TransformationMatrix;
                    currentObject = currentObject.Parent;
                }
                return matrix;
            }
            else if (targetCoordinateSpace.Parent == this) // Optimization
                return Matrix.Invert(targetCoordinateSpace.TransformationMatrix);

            // 1. Find a common parent of this and the target coordinate space.
            int count = 0;
            SPDisplayObject commonParent = null;
            currentObject = this;

            while (currentObject != null && count < SPMacros.SP_MAX_DISPLAY_TREE_DEPTH)
            {
                s_ancestors[count++] = currentObject;
                currentObject = currentObject.Parent;
            }

            currentObject = targetCoordinateSpace;

            while (currentObject != null && commonParent == null)
            {
                for (int i = 0; i < count; ++i)
                {
                    if (currentObject == s_ancestors[i])
                    {
                        commonParent = s_ancestors[i];
                        break;
                    }
                }
                currentObject = currentObject.Parent;
            }

            if (commonParent == null)
                throw new InvalidOperationException("Display object not connected to target.");

            // 2. Move up from this to common parent
            Matrix selfMatrix = Matrix.Identity;
            currentObject = this;

            while (currentObject != commonParent)
            {
                selfMatrix *= currentObject.TransformationMatrix;
                currentObject = currentObject.Parent;
            }

            if (commonParent == targetCoordinateSpace)
                return selfMatrix;

            // 3. Now move up from target until we reach the common parent
            Matrix targetMatrix = Matrix.Identity;
            currentObject = targetCoordinateSpace;

            while (currentObject != null && currentObject != commonParent)
            {
                targetMatrix *= currentObject.TransformationMatrix;
                currentObject = currentObject.Parent;
            }

            // 4. Combine the two matrices
            return selfMatrix * Matrix.Invert(targetMatrix);
        }

        public virtual SPRectangle BoundsInSpace(SPDisplayObject targetCoordinateSpace)
        {
            throw new MissingMethodException("Method needs to be implemented in subclass.");
        }

        public virtual SPDisplayObject HitTestPoint(Vector2 localPoint, bool isTouch)
        {
            // On a touch test, invisible or untouchable objects cause the test to fail
            if (isTouch && (!Visible || !Touchable)) return null;

            // Otherwise, check bounding box
            if (BoundsInSpace(this).Contains(localPoint)) return this;
            else return null;
        }

        public Vector2 LocalToGlobal(Vector2 localPoint)
        {
            // Move up until parent is null
            Matrix transform = Matrix.Identity;
            SPDisplayObject currentObject = this;

            while (currentObject != null)
            {
                Matrix matrix1 = transform, matrix2 = currentObject.TransformationMatrix;
                Matrix.Multiply(ref matrix1, ref matrix2, out transform);
                currentObject = currentObject.Parent;

                //transform *= currentObject.TransformationMatrix;
                //currentObject = currentObject.Parent;
            }

            return Vector2.Transform(localPoint, transform);
        }

        public Vector2 GlobalToLocal(Vector2 globalPoint)
        {
            // Move up until parent is null, then invert matrix
            Matrix transform = Matrix.Identity;
            SPDisplayObject currentObject = this;

            while (currentObject != null)
            {
                Matrix matrix1 = transform, matrix2 = currentObject.TransformationMatrix;
                Matrix.Multiply(ref matrix1, ref matrix2, out transform);
                currentObject = currentObject.Parent;

                //transform *= currentObject.TransformationMatrix;
                //currentObject = currentObject.Parent;
            }

            transform = Matrix.Invert(transform);
            return Vector2.Transform(globalPoint, transform);
        }

        public void RemoveFromParent()
        {
            if (Parent != null)
                Parent.RemoveChild(this);
        }

        //[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public virtual void PreDraw(SPRenderSupport support)
        {
            if (Effecter != null)
                support.PushEffect(Effecter);

            if (BlendState != null)
                support.PushBlendState(BlendState);
        }

        public virtual void Draw(GameTime gameTime, SPRenderSupport support)
        {
            Matrix identity = Matrix.Identity;
            Draw(gameTime, support, identity);
        }

        public abstract void Draw(GameTime gameTime, SPRenderSupport support, Matrix parentTransform);

        //[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public virtual void PostDraw(SPRenderSupport support)
        {
            // Undo local state changes
            if (Effecter != null && support.CurrentEffecter == Effecter)
                support.PopEffect();

            if (BlendState != null && support.CurrentBlendState == BlendState)
                support.PopBlendState();

            support.SamplerState = support.DefaultSamplerState;
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
                try
                {
                    if (disposing)
                    {
                        if (BlendState != null)
                        {
                            BlendState.Dispose();
                            BlendState = null;
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore
                }
                finally
                {
                    mIsDisposed = true;
                }
            }
        }

        ~SPDisplayObject()
        {
            Dispose(false);
        }
        #endregion
    }
}
