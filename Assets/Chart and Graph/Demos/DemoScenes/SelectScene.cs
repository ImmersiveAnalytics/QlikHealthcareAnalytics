using UnityEngine;
using System.Collections;


namespace ChartAndGraph
{
    #pragma warning disable 0618
    public class SelectScene : MonoBehaviour
    {
        public GameObject EventSystem;
        public GameObject MainCamera;
        public Canvas MainCanvas;
        public Canvas BackCanvas;

        void ChangeCanvas()
        { 
            EventSystem.SetActive(false);
            MainCamera.SetActive(false);
            MainCanvas.gameObject.SetActive(false);
            BackCanvas.gameObject.SetActive(true);
        }
        public void SelectSimpleGraph()
        {
            Application.LoadLevelAdditive("Chart And Graph/Demos/DemoScenes/simpleGraphDemo");
            ChangeCanvas();
        }
        public void SelectMultipleGraph()
        {
            Application.LoadLevelAdditive("Chart And Graph/Demos/DemoScenes/multipleGraphChart-3");
            ChangeCanvas();
        }
        public void Select2DRadar()
        {
            Application.LoadLevelAdditive("Chart And Graph/Demos/DemoScenes/2DRadar");
            ChangeCanvas();
        }
        public void Select3DRadar()
        {
            Application.LoadLevelAdditive("Chart And Graph/Demos/DemoScenes/3DRadar");
            ChangeCanvas();
        }
        public void Select2DPie()
        {
            Application.LoadLevelAdditive("Chart And Graph/Demos/DemoScenes/2d pie-8");
            ChangeCanvas();
        }
        public void Select3DPie()
        {
            Application.LoadLevelAdditive("Chart And Graph/Demos/DemoScenes/3d pie-2");
            ChangeCanvas();
        }
        public void Select3DPie2()
        {
            Application.LoadLevelAdditive("Chart And Graph/Demos/DemoScenes/3d pie-3");
            ChangeCanvas();
        }
        public void Select2DBar()
        {
            Application.LoadLevelAdditive("Chart And Graph/Demos/DemoScenes/2d bar");
            ChangeCanvas();
        }
        public void Select3DCircleBar()
        {
            Application.LoadLevelAdditive("Chart And Graph/Demos/DemoScenes/3D circle bar");
            ChangeCanvas();
        }
        public void Select3DBar()
        {
            Application.LoadLevelAdditive("Chart And Graph/Demos/DemoScenes/3D bar");
            ChangeCanvas();

        }
        public void Select3DBar2()
        {
            Application.LoadLevelAdditive("Chart And Graph/Demos/DemoScenes/3D bar-2");
            ChangeCanvas();

        }
        public void SelectMain()
        {
            Application.LoadLevel("Chart And Graph/Demos/DemoMain");
            ChangeCanvas();
        }
    }
}
