using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ChartAndGraph
{
    /// <summary>
    /// base class for all chart item effects
    /// </summary>
    abstract class ChartItemEffect : MonoBehaviour
    {
        CharItemEffectController mController;
        protected CharItemEffectController Controller
        {
            get
            {
                if (mController == null)
                {
                    mController = GetComponent<CharItemEffectController>();
                    if (mController == null)
                        mController = gameObject.AddComponent<CharItemEffectController>();
                }
                return mController;
            }
        }
        private void Register()
        {
            CharItemEffectController control = Controller;
            if (control != null)
                control.Register(this);
        }
        private void Unregister()
        {
            CharItemEffectController control = Controller;
            if (control != null)
                control.Unregister(this);
        }

        protected virtual void OnDisable()
        {
            Unregister();
        }

        protected virtual void OnEnable()
        {
            Register();
        }

        protected virtual void Start()
        {
            Register();
        }

        protected virtual void Destroy()
        {
            Unregister();
        }

        /// <summary>
        /// applies a scaling to the object
        /// </summary>
        internal abstract Vector3 ScaleMultiplier {get;}
        /// <summary>
        /// applies rotation to the object
        /// </summary>
        internal abstract Quaternion Rotation { get; }
        /// <summary>
        /// applies translation to the object
        /// </summary>
        internal abstract Vector3 Translation { get; }
    }
}
