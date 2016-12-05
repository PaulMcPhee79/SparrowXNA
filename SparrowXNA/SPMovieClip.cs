using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SparrowXNA
{
    public class SPMovieClip : SPQuad, ISPAnimatable
    {
        public const string SP_EVENT_TYPE_MOVIE_COMPLETED = "movieCompleted";

        public SPMovieClip(SPTexture texture, float fps)
            : base(texture)
        {
            mDefaultFrameDuration = (fps == 0.0f) ? int.MaxValue : 1.0 / fps;
            Initialize();
            AddFrame(texture);
        }

        public SPMovieClip(List<SPTexture> textures, float fps)
            : base((textures == null || textures.Count == 0) ? null : textures[0])
        {
            if (textures == null || textures.Count == 0)
                throw new ArgumentException("Empty texture array.");

            Fps = fps;
            Initialize();

            foreach (SPTexture texture in textures)
                AddFrame(texture);
        }

        #region Fields
        private List<double> mFrameDurations;
        private double mDefaultFrameDuration;
        private double mTotalDuration;
        private double mCurrentTime;
        private bool mLoop;
        private bool mPlaying;
        private int mCurrentFrame;
        private uint mAnimKey;
        private List<SPTexture> mFrames;
        //private List<SPSoundChannel> mSounds;
        #endregion

        #region Properties
        public bool IsComplete { get { return false; } }
        public uint AnimKey { get { return mAnimKey; } }
        public object Target { get { return null; } }
        public double Duration { get { return mTotalDuration; } }
        public bool Loop { get { return mLoop; } set { mLoop = value; } }
        public bool IsPlaying { get { return mPlaying; } }
        public int NumFrames { get { return (mFrames == null) ? 0 : mFrames.Count; } }
        public int CurrentFrame
        {
            get { return mCurrentFrame; }
            set
            {
                CheckIndex(value);
                mCurrentFrame = value;
                mCurrentTime = 0.0;

                for (int i = 0; i < value; ++i)
                    mCurrentTime += mFrameDurations[i];

                UpdateCurrentFrame();
            }
        }
        
        public float Fps
        {
            get
            {
                return (float)(1.0 / mDefaultFrameDuration);
            }

            set
            {
                float newFrameDuration = (float)(value == 0.0f ? int.MaxValue : 1.0 / value);
                float acceleration = (float)(newFrameDuration / mDefaultFrameDuration);
                mCurrentTime *= acceleration;
                mDefaultFrameDuration = newFrameDuration;

                for (int i = 0; i < NumFrames; ++i)
                    SetDurationAtIndex(DurationAtIndex(i) * acceleration, i);
            }
        }
        #endregion

        #region Methods
        private void Initialize()
        {
            mLoop = true;
            mPlaying = true;
            mAnimKey = SPJuggler.NextAnimKey();
            mTotalDuration = 0.0;
            mCurrentTime = 0.0;
            mCurrentFrame = 0;
            mFrames = new List<SPTexture>();
            //mSounds = new List<SPSoundChannel>();
            mFrameDurations = new List<double>();
        }

        public void AddFrame(SPTexture texture)
        {
            AddFrame(texture, mDefaultFrameDuration);
        }

        public int AddFrame(SPTexture texture, double duration)
        {
            mTotalDuration += duration;
            mFrames.Add(texture);
            mFrameDurations.Add(duration);
            return mFrames.Count - 1;
        }

        public void InsertFrameAtIndex(SPTexture texture, int index)
        {
            if (index < 0 || index > mFrames.Count)
                throw new IndexOutOfRangeException("Invalid frame index");
            mFrames.Insert(index, texture);
            mFrameDurations.Insert(index, mDefaultFrameDuration);
            mTotalDuration += mDefaultFrameDuration;
        }

        public void RemoveFrameAtIndex(int index)
        {
            CheckIndex(index);
            mTotalDuration -= DurationAtIndex(index);
            mFrames.RemoveAt(index);
            mFrameDurations.RemoveAt(index);
        }

        public void SetFrameAtIndex(SPTexture texture, int index)
        {
            CheckIndex(index);
            mFrames[index] = texture;
        }

        //public void SetSound(SPSoundChannel sound, int index);

        public void SetDurationAtIndex(double duration, int index)
        {
            CheckIndex(index);
            mTotalDuration -= DurationAtIndex(index);
            mFrameDurations[index] = duration;
            mTotalDuration += duration;
        }

        public SPTexture FrameAtIndex(int index)
        {
            CheckIndex(index);
            return mFrames[index];
        }

        //public SPSoundChannel SoundAtIndex(int index);

        public double DurationAtIndex(int index)
        {
            CheckIndex(index);
            return mFrameDurations[index];
        }

        public void Play()
        {
            mPlaying = true;
        }

        public void Pause()
        {
            mPlaying = false;
        }

        private void UpdateCurrentFrame()
        {
            if (mFrames != null)
                Texture = mFrames[mCurrentFrame];
        }

        //private void PlayCurrentSound;

        private void CheckIndex(int index)
        {
            if (mFrames == null || index < 0 || index >= mFrames.Count)
                throw new IndexOutOfRangeException("Invalid frame index");
        }

        public void AdvanceTime(double seconds)
        {
            if (mLoop && mCurrentTime == mTotalDuration) mCurrentTime = 0.0;
            if (!mPlaying || seconds == 0.0 || mCurrentTime == mTotalDuration) return;

            int i = 0;
            double durationSum = 0.0;
            double previousTime = mCurrentTime;
            double restTime = mTotalDuration - mCurrentTime;
            double carryOverTime = seconds > restTime ? seconds - restTime : 0.0;
            mCurrentTime = Math.Min(mTotalDuration, mCurrentTime + seconds);

            foreach (double frameDuration in mFrameDurations)
            {
                if (durationSum + frameDuration >= mCurrentTime)
                {
                    if (mCurrentFrame != i)
                    {
                        mCurrentFrame = i;
                        UpdateCurrentFrame();
                        //PlayCurrentSound();
                    }
                    break;
                }

                ++i;
                durationSum += frameDuration;
            }

            if (previousTime < mTotalDuration && mCurrentTime == mTotalDuration && HasEventListenerForType(SP_EVENT_TYPE_MOVIE_COMPLETED))
                DispatchEvent(SPEvent.SPEventWithType(SP_EVENT_TYPE_MOVIE_COMPLETED));

            AdvanceTime(carryOverTime);
        }

        protected override void Dispose(bool disposing)
        {
            if (!mIsDisposed)
            {
                try
                {
                    if (disposing)
                    {
                        mFrames = null;
                        //mSounds = null;
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
