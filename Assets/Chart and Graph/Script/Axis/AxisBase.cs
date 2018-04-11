using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ChartAndGraph
{
    /// <summary>
    /// base class for all axis monobehaviours. This class contains paramets for defining chart axis
    /// </summary>
    public abstract class AxisBase : ChartSettingItemBase, ISerializationCallbackReceiver
    {

#pragma warning disable 0414
        /// <summary>
        /// used internally by the axis inspector
        /// </summary>
        [SerializeField]
        private bool SimpleView = true;
#pragma warning restore 0414
        /// <summary>
        /// the format of the axis labels. This can be either a number, time or date time. If the selected value is either DateTime or Time , user ChartDateUtillity to convert dates to double values that can be set to the chart.
        /// </summary>
        [SerializeField]
        [Tooltip("the format of the axis labels. This can be either a number, time or date time. If the selected value is either DateTime or Time , user ChartDateUtillity to convert dates to double values that can be set to the graph")]
        private AxisFormat format;

        /// <summary>
        /// the format of the axis labels. This can be either a number, time or date time. If the selected value is either DateTime or Time , user ChartDateUtillity to convert dates to double values that can be set to the chart.
        /// </summary>
        public AxisFormat Format
        {
            get { return format; }
            set
            {
                format = value;
                RaiseOnChanged();
            }
        }

        /// <summary>
        /// the depth of the axis reltive to the chart position
        /// </summary>
        [SerializeField]
        [Tooltip("the depth of the axis reltive to the chart position")]
        private AutoFloat depth;
        public AutoFloat Depth
        {
            get { return depth; }
            set { depth = value;
                RaiseOnChanged();
            }
        }

        [SerializeField]
        [Tooltip("The main divisions of the chart axis")]
        [FormerlySerializedAs("MainDivisions")]
        private ChartDivisionInfo mainDivisions = new ChartDivisionInfo();
        [SerializeField]
        [Tooltip("The sub divisions of each main division")]
        [FormerlySerializedAs("SubDivisions")]
        private ChartDivisionInfo subDivisions = new ChartDivisionInfo();

        /// <summary>
        /// the main division properies for this axis
        /// </summary>
        public ChartDivisionInfo MainDivisions { get { return mainDivisions; } }
        /// <summary>
        /// the sub division properies for this axis
        /// </summary>
        public ChartDivisionInfo SubDivisions { get { return subDivisions; } }

        public AxisBase()
        {
            AddInnerItems();
        }
        private void AddInnerItems()
        {
            AddInnerItem(MainDivisions);
            AddInnerItem(SubDivisions);
        }
        /// <summary>
        /// used internally to hold data required for updating the axis lables
        /// </summary>
        internal class TextData
        {
            public ChartDivisionInfo info;
            public float interp;
            public int fractionDigits;
        }

        /// <summary>
        /// checks that all properies of this instance have valid values. This method is used internally and should not be called
        /// </summary>
        public void ValidateProperties()
        {
            mainDivisions.ValidateProperites();
            subDivisions.ValidateProperites();
        }

        /// <summary>
        /// retrieves the begining and end division of the chart
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="orientation"></param>
        /// <param name="total"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void GetStartEnd(AnyChart parent, ChartOrientation orientation, int total, out int start, out int end)
        {
            start = 0;
            end = total;
        //    if ((orientation == ChartOrientation.Horizontal) ? parent.Frame.Left.Visible : parent.Frame.Bottom.Visible)
        //        ++start;
        //    if ((orientation == ChartOrientation.Horizontal) ? parent.Frame.Right.Visible : parent.Frame.Top.Visible)
        //        --end;
        }

        /// <summary>
        /// sets the uv of a chart mesh based on the current length and offset
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="length"></param>
        /// <param name="offset"></param>
        private void SetMeshUv(IChartMesh mesh, float length,float offset)
        {
            if (length < 0)
                offset -= length;
            mesh.Length = length;
            mesh.Offset = offset;
        }
        /// <summary>
        /// used internally to draw the division of the axis into a chart mesh
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="parentTransform"></param>
        /// <param name="info"></param>
        /// <param name="mesh"></param>
        /// <param name="group"></param>
        /// <param name="orientation"></param>
        /// <param name="totalDivisions"></param>
        /// <param name="oppositeSide"></param>
        /// <param name="skip"></param>
        private void DrawDivisions(AnyChart parent, Transform parentTransform, ChartDivisionInfo info, IChartMesh mesh, int group, ChartOrientation orientation, int totalDivisions, bool oppositeSide, int skip)
        {
            float parentSize = (orientation == ChartOrientation.Vertical) ? ((IInternalUse)parent).InternalTotalHeight : ((IInternalUse)parent).InternalTotalWidth;
            Vector2 startPosition, lengthDirection, advanceDirection;
            GetDirectionVectors(parent, info, orientation, oppositeSide, out startPosition, out lengthDirection, out advanceDirection);
            float markDepth = ChartCommon.GetAutoDepth(parent, orientation, info);
            float length = ChartCommon.GetAutoLength(parent, orientation, info);
            float backLength = (orientation == ChartOrientation.Vertical) ? ((IInternalUse)parent).InternalTotalWidth : ((IInternalUse)parent).InternalTotalHeight;
            if (info.MarkBackLength.Automatic == false)
                backLength = info.MarkBackLength.Value;
            float totaluv = Math.Abs(length);
            if (backLength != 0 && markDepth > 0)
                totaluv += Math.Abs(backLength) + Math.Abs(markDepth);
            Vector2 halfThickness = advanceDirection * (info.MarkThickness * 0.5f);
            int first, last;
            GetStartEnd(parent, orientation, totalDivisions, out first, out last);

            bool hasValues = ((IInternalUse)parent).InternalHasValues(this);
            double maxValue = ((IInternalUse)parent).InternalMaxValue(this);
            double minValue = ((IInternalUse)parent).InternalMinValue(this);

            float AutoAxisDepth = Depth.Value;
            if(Depth.Automatic)
            {
                AutoAxisDepth = (((IInternalUse)parent).InternalTotalDepth) - markDepth;

            }
            for (int i = first; i < last; i++)
            {
                if (skip != -1 && i % skip == 0)
                    continue;
                float factor = ((float)i) / (float)(totalDivisions - 1);

                float offset = factor * parentSize;
                Vector2 start = startPosition + advanceDirection * offset;
                Vector2 size = halfThickness + length * lengthDirection;
                start -= halfThickness;
                //size += halfThickness;
                float uvoffset = 0f;

                Rect r = ChartCommon.FixRect(new Rect(start.x, start.y, size.x, size.y));
    
                SetMeshUv(mesh, -length / totaluv, uvoffset);
                uvoffset += Math.Abs(mesh.Length);
                        
                mesh.AddXYRect(r, group, AutoAxisDepth);

                if (hasValues)
                {
                    double val = minValue * (1- (double)factor) +  maxValue * (double)factor;
                    string toSet = "";
                    if (format == AxisFormat.Number)
                        toSet = ChartAdancedSettings.Instance.FormatFractionDigits(info.FractionDigits, (float)val);
                    else
                    {
                        DateTime date = ChartDateUtility.ValueToDate(val);
                        if (format == AxisFormat.DateTime)
                            toSet = ChartDateUtility.DateToDateTimeString(date);
                        else
                        {
                            if (format == AxisFormat.Date)
                                toSet = ChartDateUtility.DateToDateString(date);
                            else
                                toSet = ChartDateUtility.DateToTimeString(date);
                        }
                    }
                    toSet = info.TextPrefix + toSet + info.TextSuffix;
                    Vector2 textPos = new Vector2(start.x, start.y);
                    textPos += lengthDirection * info.TextSeperation;
                    TextData userData = new TextData();
                    userData.interp = factor;
                    userData.info = info;
                    userData.fractionDigits = info.FractionDigits;
                    mesh.AddText(parent, info.TextPrefab, parentTransform, info.FontSize, info.FontSharpness, toSet, textPos.x, textPos.y, AutoAxisDepth + info.TextDepth,0f, userData);
                }

                if (markDepth > 0)
                {
                    
                    
                    if (orientation == ChartOrientation.Horizontal)
                    {
                        SetMeshUv(mesh, markDepth / totaluv, uvoffset);
                        r = ChartCommon.FixRect(new Rect(start.x, AutoAxisDepth, size.x, markDepth));
                        mesh.AddXZRect(r, group, start.y);
                    }
                    else
                    {
                        SetMeshUv(mesh, -markDepth / totaluv, uvoffset);
                        r = ChartCommon.FixRect(new Rect(start.y, AutoAxisDepth, size.y, markDepth));
                        mesh.AddYZRect(r, group, start.x);
                    }
                    uvoffset += Math.Abs(mesh.Length);

                    if (backLength != 0)
                    {

                        SetMeshUv(mesh, backLength / totaluv, uvoffset);
                        uvoffset += Math.Abs(mesh.Length);

                        Vector2 backSize = halfThickness + backLength * lengthDirection;
                        Rect backR = ChartCommon.FixRect(new Rect(start.x, start.y, backSize.x, backSize.y));
                        mesh.AddXYRect(backR, group, AutoAxisDepth + markDepth);
                    }
                }
            }
        }
        
        /// <summary>
        /// used internally to determine drawing direction of the each chart division
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="info"></param>
        /// <param name="orientation"></param>
        /// <param name="oppositeSide"></param>
        /// <param name="startPosition"></param>
        /// <param name="lengthDirection"></param>
        /// <param name="advanceDirection"></param>
        private void GetDirectionVectors(AnyChart parent, ChartDivisionInfo info, ChartOrientation orientation, bool oppositeSide, out Vector2 startPosition, out Vector2 lengthDirection, out Vector2 advanceDirection)
        {
            if (orientation == ChartOrientation.Horizontal)
            {
                advanceDirection = new Vector2(1f, 0f);
                if (oppositeSide)
                {
                    startPosition = new Vector2(0f, ((IInternalUse)parent).InternalTotalHeight);
                    lengthDirection = new Vector2(0f, -1f);
                    return;
                }
                startPosition = new Vector2(0f, 0f);
                lengthDirection = new Vector2(0f, 1f);
                return;
            }
            advanceDirection = new Vector2(0f, 1f);
            if (oppositeSide)
            {
                startPosition = new Vector2(0f, 0f);
                lengthDirection = new Vector2(1f, 0f);
                return;
            }
            startPosition = new Vector2(((IInternalUse)parent).InternalTotalWidth, 0f);
            lengthDirection = new Vector2(-1f, 0f);
        }
        /// <summary>
        /// used internally , adds the axis sub divisions to the chart mesh
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="parentTransform"></param>
        /// <param name="mesh"></param>
        /// <param name="orientation"></param>
        internal void AddSubdivisionToChartMesh(AnyChart parent, Transform parentTransform, IChartMesh mesh, ChartOrientation orientation)
        {
            if (SubDivisions.Total <= 1) // no need for more
                return;
            mesh.Tile = ChartCommon.GetTiling(SubDivisions.MaterialTiling);
            if ((SubDivisions.Alignment & ChartDivisionAligment.Opposite) == ChartDivisionAligment.Opposite)
                DrawDivisions(parent, parentTransform, SubDivisions, mesh, 0, orientation, MainDivisions.Total * SubDivisions.Total + 1, false, SubDivisions.Total);
            if ((SubDivisions.Alignment & ChartDivisionAligment.Standard) == ChartDivisionAligment.Standard)
                DrawDivisions(parent, parentTransform, SubDivisions, mesh, 0, orientation, MainDivisions.Total * SubDivisions.Total + 1, true, SubDivisions.Total);
        }
        /// <summary>
        /// used internally , adds the axis main divisions to the chart mesh
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="parentTransform"></param>
        /// <param name="mesh"></param>
        /// <param name="orientation"></param>

        internal void AddMainDivisionToChartMesh(AnyChart parent, Transform parentTransform, IChartMesh mesh, ChartOrientation orientation)
        {
            if (MainDivisions.Total <= 0)
                return;
            mesh.Tile = ChartCommon.GetTiling(MainDivisions.MaterialTiling);
            if ((MainDivisions.Alignment & ChartDivisionAligment.Opposite) == ChartDivisionAligment.Opposite)
                DrawDivisions(parent, parentTransform, MainDivisions, mesh, 0, orientation, MainDivisions.Total + 1, false, -1);
            if ((MainDivisions.Alignment & ChartDivisionAligment.Standard) == ChartDivisionAligment.Standard)
                DrawDivisions(parent, parentTransform, MainDivisions, mesh, 0, orientation, MainDivisions.Total + 1, true, -1);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {

        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            AddInnerItems();
        }
    }
}
