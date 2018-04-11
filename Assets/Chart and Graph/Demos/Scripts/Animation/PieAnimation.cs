using UnityEngine;
namespace ChartAndGraph
{
    public class PieAnimation : MonoBehaviour
    {
        public bool AnimateOnStart = true;
        [Range(0f,360f)]
        public float AnimateSpan = 360f;
        public float AnimationTime =2f;
        public AnimationCurve Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        PieChart pieChart;
        float StartTime = -1f;

        void Start()
        {
            pieChart = GetComponent<PieChart>();
            if(AnimateOnStart)
                Animate();
        }

        public void Animate()
        {
            if (pieChart != null)
            {
                AnimateSpan = pieChart.AngleSpan;
                StartTime = Time.time;
            }
        }
        void Update()
        {
            if(StartTime >= 0f && pieChart != null)
            {
                float curveTime = 0f;
                if (Curve.length > 0)
                    curveTime = Curve.keys[Curve.length - 1].time;
                float elasped = Time.time - StartTime;
                elasped /= AnimationTime;
                elasped *= curveTime;
                elasped = Curve.Evaluate(elasped);
                if (elasped > 1f)
                {
                    pieChart.AngleSpan = AnimateSpan;
                    StartTime = -1f;
                }
                else
                    pieChart.AngleSpan = Mathf.Lerp(0f, AnimateSpan, elasped);
            }
        }
    }
}