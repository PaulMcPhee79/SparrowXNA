using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SparrowXNA
{
    public class SPDisplayObjectContainer : SPDisplayObject
    {
        public SPDisplayObjectContainer()
        {
            mChildren = new List<SPDisplayObject>();
        }

        #region Fields
        private List<SPDisplayObject> mChildren;
        #endregion

        #region Properties
        public int NumChildren { get { return mChildren.Count; } }
        private List<SPDisplayObject> Children { get { return mChildren; } }
        #endregion

        #region Methods
        public void AddChild(SPDisplayObject child)
        {
            AddChildAtIndex(child, mChildren.Count);
        }

        public virtual void AddChildAtIndex(SPDisplayObject child, int index)
        {
            if (child == null)
                throw new ArgumentException("Attempt to add a null child.");
            if (child == this)
                throw new ArgumentException("Attempt to add an object as a child of itself.");

            if (index >= 0 && index <= mChildren.Count)
            {
                child.RemoveFromParent();
                mChildren.Insert((int)MathHelper.Min(mChildren.Count, index), child);
                child.Parent = this;
                //child.DispatchEvent(new SPEvent(SPEvent.SP_EVENT_TYPE_ADDED));

                //if (Stage != null)
                //    child.DispatchEvent(new SPEvent(SPEvent.SP_EVENT_TYPE_ADDED_TO_STAGE));
            }
            else
                throw new IndexOutOfRangeException("AddChildAtIndex: Index out of bounds.");
        }

        public bool ContainsChild(SPDisplayObject child)
        {
            if (this.Equals(child)) return true;

            foreach (SPDisplayObject currentChild in mChildren)
            {
                if (currentChild is SPDisplayObjectContainer)
                {
                    if (((SPDisplayObjectContainer)currentChild).ContainsChild(child))
                        return true;
                }
                else
                {
                    if (currentChild == child)
                        return true;
                }
            }

            return false;
        }

        public int ChildIndex(SPDisplayObject child)
        {
            return mChildren.IndexOf(child);
        }

        public SPDisplayObject ChildAtIndex(int index)
        {
            if (index >= 0 && index < mChildren.Count)
                return mChildren[index];
            else
                throw new IndexOutOfRangeException("ChildAtIndex: Index out of bounds.");
        }

        public SPDisplayObject ChildForTag(uint tag)
        {
            SPDisplayObject child = null;

            foreach (SPDisplayObject currentChild in mChildren)
            {
                if (currentChild.Tag == tag)
                {
                    child = currentChild;
                    break;
                }
            }

            return child;
        }

        public void RemoveChild(SPDisplayObject child)
        {
            int index = ChildIndex(child);

            if (index != -1)
                RemoveChildAtIndex(index);
        }

        public virtual void RemoveChildAtIndex(int index)
        {
            if (index >= 0 && index < mChildren.Count)
            {
                SPDisplayObject child = ChildAtIndex(index);

                //child.DispatchEvent(new SPEvent(SPEvent.SP_EVENT_TYPE_REMOVED));

                //if (Stage != null)
                //    child.DispatchEvent(new SPEvent(SPEvent.SP_EVENT_TYPE_REMOVED_FROM_STAGE));

                if (child.Parent != null)
                    child.Parent = null;

                mChildren.RemoveAt(index);
            } else
                throw new IndexOutOfRangeException("RemoveChildAtIndex: Index out of bounds.");
        }

        public void SwapChildren(SPDisplayObject child1, SPDisplayObject child2)
        {
            int index1 = ChildIndex(child1);
            int index2 = ChildIndex(child2);
            SwapChildrenAtIndices(index1, index2);
        }

        public void SwapChildrenAtIndices(int index1, int index2)
        {
            int numChildren = mChildren.Count;
            if (index1 < 0 || index1 >= numChildren || index2 < 0 || index2 >= numChildren)
                throw new InvalidOperationException("Invalid child indices");
            SPDisplayObject temp = mChildren[index1];
            mChildren[index1] = mChildren[index2];
            mChildren[index2] = temp;
        }

        public void RemoveAllChildren()
        {
            for (int i = mChildren.Count-1; i >= 0; --i)
                RemoveChildAtIndex(i);
        }

        public override SPRectangle BoundsInSpace(SPDisplayObject targetCoordinateSpace)
        {
            int numChildren = mChildren.Count;

            if (numChildren == 0)
            {
                Matrix transform = TransformationMatrixToSpace(targetCoordinateSpace);
                Vector2 point = Origin;
                Vector2 transformedPoint = Vector2.Transform(point, transform);
                return new SPRectangle(transformedPoint.X, transformedPoint.Y, 0f, 0f);
            }
            else if (numChildren == 1)
            {
                //SPRectangle rect = mChildren[0].BoundsInSpace(targetCoordinateSpace);
                //rect.Width -= X;
                //rect.Height -= Y;
                //return rect;

                // Note: Is this not taking our offset into account?
                return mChildren[0].BoundsInSpace(targetCoordinateSpace);
            }
            else
            {
                float minX = float.PositiveInfinity, maxX = float.NegativeInfinity, minY = float.PositiveInfinity, maxY = float.NegativeInfinity;
                foreach (SPDisplayObject child in mChildren)
                {
                    SPRectangle childBounds = child.BoundsInSpace(targetCoordinateSpace);
                    minX = MathHelper.Min(minX, childBounds.X);
                    maxX = MathHelper.Max(maxX, childBounds.X + childBounds.Width);
                    minY = MathHelper.Min(minY, childBounds.Y);
                    maxY = MathHelper.Max(maxY, childBounds.Y + childBounds.Height);
                }
                return new SPRectangle(minX, minY, maxX - minX, maxY - minY);
            }
        }

        public override SPDisplayObject HitTestPoint(Vector2 localPoint, bool isTouch)
        {
            if (mChildren == null || (isTouch && (!Visible || !Touchable))) return null;

            for (int i = mChildren.Count - 1; i >= 0; --i) // front to back
            {
                SPDisplayObject child = mChildren[i];
                Matrix transform = TransformationMatrixToSpace(child);
                Vector2 transformedPoint = Vector2.Transform(localPoint, transform);
                SPDisplayObject target = child.HitTestPoint(transformedPoint, isTouch);
                if (target != null) return target;
            }

            return null;
        }

        public static void GetChildEventListeners(SPDisplayObject displayObject, string eventType, List<SPEventDispatcher> listeners)
        {
            if (displayObject.HasEventListenerForType(eventType))
                listeners.Add(displayObject);
            if (displayObject is SPDisplayObjectContainer)
            {
                foreach (SPDisplayObject child in ((SPDisplayObjectContainer)displayObject).Children)
                    SPDisplayObjectContainer.GetChildEventListeners(child, eventType, listeners);
            }
        }

        public void DispatchEventOnChildren(SPEvent ev)
        {
            // The event listeners might modify the display tree, which could make the loop crash.
            // This, we collect them in a list and iterate over that list instead.
            List<SPEventDispatcher> listeners = new List<SPEventDispatcher>();
            SPDisplayObjectContainer.GetChildEventListeners(this, ev.EventType, listeners);

            foreach (SPEventDispatcher listener in listeners)
                listener.DispatchEvent(ev);
        }

        public override void Draw(GameTime gameTime, SPRenderSupport support, Matrix parentTransform)
        {
            if (mChildren == null)
                return;

            PreDraw(support);

            float alpha = Alpha;
#if true
            Matrix globalTransform, localTransform = TransformationMatrix;
            Matrix.Multiply(ref localTransform, ref parentTransform, out globalTransform);
#else
            Matrix globalTransform = TransformationMatrix * parentTransform;
#endif
            SPEffecter effecter = support.IsUsingDefaultEffect ? null : support.CurrentEffecter;

            if (effecter == null)
            {
                foreach (SPDisplayObject child in mChildren)
                {
                    if (child != null && child.Visible && !SPMacros.SP_IS_FLOAT_EQUAL(child.Alpha, 0f))
                    {
                        float childAlpha = child.Alpha;
                        child.Alpha *= alpha;
                        child.Draw(gameTime, support, globalTransform);
                        child.Alpha = childAlpha;
                    }
                }
            }
            else
            {
                foreach (SPDisplayObject child in mChildren)
                {
                    if (child != null && child.Visible && !SPMacros.SP_IS_FLOAT_EQUAL(child.Alpha, 0f))
                    {
                        float childAlpha = child.Alpha;
                        child.Alpha *= alpha;
                        child.Draw(gameTime, support, globalTransform);
                        child.Alpha = childAlpha;
                    }

                    // Commented because: Allow children to manage batch interruptions. Our preference is to draw as many children in one call as possible.
                    //support.EndBatch();
                    //support.BeginBatch();
                }
            }

            PostDraw(support);
        }

        protected override void Dispose(bool disposing)
        {
            if (!mIsDisposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (mChildren != null)
                        {
                            for (int i = mChildren.Count - 1; i >= 0; --i)
                                mChildren[i].Dispose();
                        }

                        mChildren = null;
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
