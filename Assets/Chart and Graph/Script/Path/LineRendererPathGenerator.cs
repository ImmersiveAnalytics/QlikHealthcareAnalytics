using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ChartAndGraph
{
    [RequireComponent(typeof(LineRenderer))]
    class LineRendererPathGenerator : PathGenerator
    {
        LineRenderer mRenderer;
        private void Start()
        {
            
        }
        public void EnsureRenderer()
        {
            mRenderer = GetComponent<LineRenderer>();
        }
        public override void Clear()
        {
            EnsureRenderer();
            if (mRenderer!= null)
            {
                #if (!UNITY_5_2) && (!UNITY_5_3) && (!UNITY_5_4)
                    mRenderer.numPositions = 0;
                #else
                mRenderer.SetVertexCount(0);
                #endif
            }
        }

        public override void Generator(Vector3[] path, float thickness, bool closed)
        {
            EnsureRenderer();
            if (mRenderer != null)
            {
#if !UNITY_5_2 && !UNITY_5_3 && !UNITY_5_4
                mRenderer.startWidth = thickness;
                mRenderer.endWidth = thickness;
                mRenderer.numPositions = path.Length;
#else
                mRenderer.SetWidth(thickness, thickness);
                mRenderer.SetVertexCount(path.Length);

#endif

                for (int i=0; i< path.Length; i++)
                {
                    mRenderer.SetPosition(i, path[i]);
                }
            }
        }
    }
}
