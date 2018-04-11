﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ChartAndGraph
{
    public class CanvasBarChart : BarChart, ICanvas
    {
        [SerializeField]
        /// <summary>
        /// prefab for the bar elements of the chart. must be the size of one unit with a pivot at the middle bottom
        /// </summary>
        [Tooltip("Prefab for the bar elements of the chart. must be the size of one unit with a pivot at the middle bottom")]
        private CanvasRenderer barPrefab;

        /// <summary>
        /// prefab for the bar elements of the chart. must be the size of one unit with a pivot at the middle bottom
        /// </summary>
        public CanvasRenderer BarPrefab
        {
            get { return barPrefab; }
            set
            {
                barPrefab = value;
                OnPropertyUpdated();
            }
        }

        /// <summary>
        /// The seperation between the axis and the chart bars.
        /// </summary>
        [SerializeField]
        [Tooltip("The seperation between the axis and the chart bars")]
        private float axisSeperation = 20f;
        protected override float TotalDepthLink
        {
            get
            {
                return 0.0f;
            }
        }
        /// <summary>
        /// The seperation between the axis and the chart bars.
        /// </summary>
        public float AxisSeperation
        {
            get { return axisSeperation; }
            set
            {
                axisSeperation = value;
                OnPropertyUpdated();
            }
        }

        /// <summary>
        /// seperation between bar of the same group
        /// </summary>
        [SerializeField]
        [Tooltip("seperation between bar of the same group")]
        private float barSeperation = 45f;

        /// <summary>
        /// seperation between bars of the same group.
        /// </summary>
        public float BarSeperation
        {
            get { return barSeperation; }
            set
            {
                barSeperation = value;
                OnPropertyUpdated();
            }
        }

        /// <summary>
        /// seperation between bar groups
        /// </summary>
        [SerializeField]
        [Tooltip("seperation between bar groups")]
        private float groupSeperation = 220f;

        /// <summary>
        /// The seperation between bar groups.
        /// <summary>
        public float GroupSeperation
        {
            get { return groupSeperation; }
            set
            {
                groupSeperation = value;
                OnPropertyUpdated();
            }
        }

        /// <summary>
        /// the width of each bar in the chart
        /// </summary>
        [SerializeField]
        [Tooltip("the width of each bar in the chart")]
        private float barSize = 20f;

        /// <summary>
        /// the width of each bar in the chart
        /// </summary>
        public float BarSize
        {
            get { return barSize; }
            set
            {
                barSize = value;
                OnPropertyUpdated();
            }
        }

        private Vector2 mLastSetSize = Vector2.zero;

        protected override ChartOrientedSize AxisSeperationLink
        {
            get
            {
                return new ChartOrientedSize(AxisSeperation);
            }
        }

        protected override ChartOrientedSize BarSeperationLink
        {
            get
            {
                return new ChartOrientedSize(BarSeperation);
            }
        }

        protected override ChartOrientedSize GroupSeperationLink
        {
            get
            {
                return new ChartOrientedSize(GroupSeperation);
            }
        }

        protected override ChartOrientedSize BarSizeLink
        {
            get
            {
                return new ChartOrientedSize(BarSize);
            }
        }

        protected override void SetBarSize(GameObject bar, Vector3 size)
        {
            RectTransform rect = bar.GetComponent<RectTransform>();
            if(rect != null)
            {
                rect.pivot = new Vector2(0.5f, 0f);
                rect.sizeDelta = new Vector2(size.x, size.y);
            }
            else
                base.SetBarSize(bar, size);
        }

        protected override void Update()
        {
            base.Update();
            RectTransform trans = GetComponent<RectTransform>();
            if (trans != null && trans.hasChanged)
            {
                if (mLastSetSize != trans.rect.size)
                {
                    GenerateChart();
                }
            }
        }

        [ContextMenu("Refresh chart")]
        public override void GenerateChart()
        {
            base.GenerateChart();
            if(TextController != null && TextController.gameObject)
                TextController.gameObject.transform.SetAsLastSibling();
            RectTransform trans = GetComponent<RectTransform>();
            GameObject fixPosition = new GameObject();
            ChartCommon.HideObject(fixPosition, hideHierarchy);
            fixPosition.AddComponent<ChartItem>();
            fixPosition.transform.position = transform.position;
            while (gameObject.transform.childCount > 0)
                transform.GetChild(0).SetParent(fixPosition.transform,false);
            fixPosition.transform.SetParent(transform,false);
            fixPosition.transform.localScale = new Vector3(1f, 1f, 1f);
            float widthScale = trans.rect.size.x/TotalWidth;
            float heightScale = trans.rect.size.y / HeightRatio;
            float uniformScale = Math.Min(widthScale, heightScale);
            fixPosition.transform.localScale = new Vector3(uniformScale, uniformScale, uniformScale);
            fixPosition.transform.localPosition = new Vector3(-TotalWidth* uniformScale * 0.5f, -HeightRatio* uniformScale * 0.5f, 0f);
            mLastSetSize = trans.rect.size;            
        }

        protected override GameObject BarPrefabLink
        {
            get
            {
                if (BarPrefab == null)
                    return null;
                return BarPrefab.gameObject;
            }
        }

    }
}
