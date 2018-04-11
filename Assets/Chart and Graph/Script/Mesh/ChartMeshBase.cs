using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ChartAndGraph
{
    /// <summary>
    /// base class for all chart mesh generators
    /// </summary>
    abstract class ChartMeshBase : IChartMesh
    {
        List<BillboardText> mText;

        public ChartOrientation Orientation { get; set; }
        public float Tile { get; set; }
        public float Offset { get; set; }
        public float Length { get; set; }

        public ChartMeshBase()
        {
            Orientation = ChartOrientation.Vertical;
        }

        public virtual List<BillboardText> TextObjects
        {
            get
            {
                return mText;
            }
        }

        Vector2[][] mUvs = new Vector2[2][]
        {
            new Vector2[]{ new Vector2(0f,0f),new Vector2(1f,0f),new Vector2(0f,1f), new Vector2(1f,1f)},
            new Vector2[]{ new Vector2(0f,1f),new Vector2(0f,0f),new Vector2(1f,1f), new Vector2(1f,0f)}
        };

        Vector2[] mTmpUv = new Vector2[4];
        protected Vector2[] GetUvs(Rect rect)
        {
            return GetUvs(rect, Orientation);
        }

        protected Vector2[] GetUvs(Rect rect, ChartOrientation orientaion)
        {
            Vector2[] arr;
            if (Orientation == ChartOrientation.Vertical)
                arr = mUvs[0];
            else
                arr= mUvs[1];
            if (Tile <=0f)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 uv = arr[i];
                    mTmpUv[i] = new Vector2(Offset + uv.x*Length, uv.y);
                }
                return mTmpUv;
            }
            float length = rect.width;
            if (orientaion == ChartOrientation.Horizontal)
                length = rect.height;
            length /= Math.Abs(Length); 
            for (int i=0; i<4; i++)
            {
                Vector2 uv = arr[i];
                mTmpUv[i] = new Vector2((Offset + uv.x * Length) * length / Tile, uv.y);
            }
            return mTmpUv;
        }

        public virtual void AddText(AnyChart chart, Text prefab, Transform parentTransform, int fontSize,float fontSharpness, string text, float x, float y, float z,float angle, object userData)
        {
            BillboardText billboardText = ChartCommon.CreateBillboardText(prefab, parentTransform, text, x, y, z,angle,null, ((IInternalUse) chart).HideHierarchy,fontSize, fontSharpness);
            billboardText.UserData = userData;
            if (mText == null)
                mText = new List<BillboardText>();
            mText.Add(billboardText);
        }

        public abstract void AddQuad(UIVertex vLeftTop, UIVertex vRightTop, UIVertex vLeftBottom, UIVertex vRightBottom);
        public abstract void AddXYRect(Rect rect, int subMeshGroup, float depth);
        public abstract void AddXZRect(Rect rect, int subMeshGroup, float yPosition);
        public abstract void AddYZRect(Rect rect, int subMeshGroup, float xPosition);

    }
}
