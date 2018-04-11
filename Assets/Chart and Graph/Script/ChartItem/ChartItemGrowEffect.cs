
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ChartAndGraph
{
    /// <summary>
    /// makes the chart item grow and shrink with easing. this can be connected to hover events for example
    /// </summary>
    class ChartItemGrowEffect : ChartItemEffect
    {
        const int NoOp = 0;
        const int GrowOp = 1;
        const int ShrinkOp = 2;
        const int GrowShrinkOp = 3;
        public float GrowMultiplier = 1.2f;

        /// <summary>
        /// scales the time used in the easing curves
        /// </summary>
        public float TimeScale = 1f;
        /// <summary>
        /// easing function for the grow effect. when the curve is at 0 there will be no change , when the curve is at 1 the change will be equal to the GrowMultiplier property
        /// </summary>
        public AnimationCurve GrowEaseFunction = AnimationCurve.EaseInOut(0f,0f,1f,1f);
        /// <summary>
        /// easing function for the shrink effect. when the curve is at 0 there will be no change , when the curve is at 1 the change will be equal to the GrowMultiplier property
        /// </summary>
        public AnimationCurve ShrinkEaseFunction = AnimationCurve.EaseInOut(1f, 1f, 0f, 0f);

        float mScaleMultiplier = 1f;
        float mStartTime = 0f;
        float mStartValue = 0f;
        int Operation = NoOp;

        internal override Vector3 ScaleMultiplier
        {
            get
            {
                return new Vector3(mScaleMultiplier, mScaleMultiplier, mScaleMultiplier);
            }
        }

        internal override Quaternion Rotation
        {
            get
            {
                return Quaternion.identity;
            }
        }

        internal override Vector3 Translation
        {
            get
            {
                return Vector3.zero;
            }
        }

        /// <summary>
        /// equivalent to calling Grow and Shrink one after the other
        /// </summary>
        public void GrowAndShrink()
        {
            mStartTime = Time.time;
            mStartValue = mScaleMultiplier;
            Operation = GrowShrinkOp;
        }
        public bool CheckAnimationEnded( float time, AnimationCurve curve)
        {
            if (curve.length == 0)
                return true;
            return  time > curve.keys[curve.length - 1].time;
        }

        private void FixEaseFunction(AnimationCurve curve)
        {
            curve.postWrapMode = WrapMode.Once;
            curve.preWrapMode = WrapMode.Once;
        }
        void Update()
        {
            float opTime = Time.time - mStartTime;
            opTime *= TimeScale;
            float val;
            switch(Operation)
            {
                case GrowOp:
                    FixEaseFunction(GrowEaseFunction);
                    val = GrowEaseFunction.Evaluate(opTime);
                    mScaleMultiplier = ChartCommon.SmoothLerp(mStartValue, GrowMultiplier, val);
                    if (CheckAnimationEnded(opTime, GrowEaseFunction))
                    {
                        Operation = NoOp;
                        mScaleMultiplier = GrowMultiplier;
                    }
                    break;
                case ShrinkOp:
                    FixEaseFunction(ShrinkEaseFunction);
                    val = ShrinkEaseFunction.Evaluate(opTime);
                    mScaleMultiplier = ChartCommon.SmoothLerp(mStartValue, 1f, val);
                    if (CheckAnimationEnded(opTime, ShrinkEaseFunction))
                    {
                        Operation = NoOp;
                        mScaleMultiplier = 1f;
                    }
                    break;
                case GrowShrinkOp:
                    FixEaseFunction(GrowEaseFunction);
                    val = GrowEaseFunction.Evaluate(opTime);
                    mScaleMultiplier = ChartCommon.SmoothLerp(mStartValue, GrowMultiplier, val);
                    if (CheckAnimationEnded(opTime, GrowEaseFunction))
                    {
                        mScaleMultiplier = GrowMultiplier;
                        Shrink();
                    }
                    break;
            }
        }

        /// <summary>
        /// Grows the size of the object
        /// </summary>
        public void Grow()
        {
            mStartTime = Time.time;
            mStartValue = mScaleMultiplier;
            Operation = GrowOp;
        }

        /// <summary>
        /// Shrinks the object back to the original size
        /// </summary>
        public void Shrink()
        {
            mStartTime = Time.time;
            mStartValue = mScaleMultiplier;
            Operation = ShrinkOp;
        }

    }
}
