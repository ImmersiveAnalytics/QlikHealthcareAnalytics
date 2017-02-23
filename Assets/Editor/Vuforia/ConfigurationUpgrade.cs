/*===============================================================================
Copyright (c) 2016 PTC Inc. All Rights Reserved.
Confidential and Proprietary - Protected under copyright and other laws.
Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Vuforia.EditorClasses
{
    public class ConfigurationUpgrade
    {

        #region STATIC_METHODS

        [MenuItem("Vuforia/Export Configuration")]
        public static void CreateConfigurationFromCurrentScene()
        {
            var outputDir = "Assets/Resources";
            if (!Directory.Exists(outputDir))
                outputDir = "Assets";
            string path = EditorUtility.SaveFilePanel("Save configuration", outputDir, "VuforiaConfiguration.asset", "asset");
            if (path.Length != 0)
            {

                CreateConfigurationFromCurrentScene(path);
            }
        }

        public static void CreateConfigurationForFirstScene()
        {
            var outputFile = "Assets/Resources/VuforiaConfiguration.asset";
            if (!Directory.Exists("Assets/Resources"))
            {
                Directory.CreateDirectory("Assets/Resources");
            }

            foreach (var bs in EditorBuildSettings.scenes)
            {
                if (bs.enabled)
                {
                    EditorApplication.OpenScene(bs.path);
                    if (GameObject.FindObjectOfType<VuforiaAbstractBehaviour>())
                    {
                        Debug.Log("Create configuration for scene " + bs.path);
                        CreateConfigurationFromCurrentScene(outputFile);
                        break;
                    }
                }
            }
        }

        [MenuItem("Vuforia/Export Configurations of all scenes")]
        public static void CreateConfigurationForAllScenes()
        {
            var outputDir = "Assets/Resources";
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            foreach (var bs in EditorBuildSettings.scenes)
            {
                if (bs.enabled)
                {
                    EditorApplication.OpenScene(bs.path);
                    if (GameObject.FindObjectOfType<VuforiaAbstractBehaviour>())
                    {
                        var idx = bs.path.LastIndexOfAny(new[] { '\\', '/' });
                        var name = idx < 0 ? bs.path : bs.path.Substring(idx + 1);
                        idx = name.LastIndexOf('.');
                        name = name.Substring(0, idx);
                        name = "Configuration_" + name + ".asset";

                        Debug.Log("Create configuration for scene " + name);
                        var fileName = Path.Combine(outputDir, name);
                        CreateConfigurationFromCurrentScene(fileName);
                    }
                }
            }
        }

        public static void CreateConfigurationFromCurrentScene(string outputFile)
        {
            var upgrade = new ConfigurationUpgrade();
            upgrade.Setup();
            upgrade.Run(outputFile);
        }


        #endregion // STATIC_METHODS


        #region NESTED

        struct Section
        {
            public string ConfigurationName;
            public string BehaviourName;
            public Property[] Properties;

            public Section(string configName, string behaviourName, Property[] properties)
            {
                ConfigurationName = configName;
                BehaviourName = behaviourName;
                Properties = properties;
            }
        }

        struct Property
        {
            public string ConfigurationName;
            public string BehaviourName;
            public PropertyType Type;

            public Property(string configName, string behaviourName, PropertyType type)
            {
                ConfigurationName = configName;
                BehaviourName = behaviourName;
                Type = type;
            }
        }

        enum PropertyType
        {
            STRING,
            INT,
            FLOAT,
            BOOL,
            ENUM,
            VECTOR2,
            VECTOR3,
            VECTOR4,
            OBJECT,
            STRING_ARRAY,
            CLASS,
        }

        #endregion


        #region PRIVATE_MEMBERS

        private GameObject mARCamera;
        private StreamWriter mWriter;
        private List<Section> mSections;
        private string mIndent;

        #endregion // PRIVATE_MEMBERS


        #region PRIVATE_METHODS

        private void Setup()
        {
            mSections = new List<Section>()
            {
                new Section("vuforia", "VuforiaAbstractBehaviour", new[]
                {
                    new Property("vuforiaLicenseKey", "VuforiaLicenseKey", PropertyType.STRING),
                    new Property("cameraDeviceModeSetting", "CameraDeviceModeSetting", PropertyType.ENUM),
                    new Property("maxSimultaneousImageTargets", "MaxSimultaneousImageTargets", PropertyType.INT),
                    new Property("maxSimultaneousObjectTargets", "MaxSimultaneousObjectTargets", PropertyType.INT),
                    new Property("useDelayedLoadingObjectTargets", "UseDelayedLoadingObjectTargets", PropertyType.BOOL),
                    new Property("cameraDirection", "CameraDirection", PropertyType.ENUM),
                    new Property("mirrorVideoBackground", "MirrorVideoBackground", PropertyType.ENUM),
                }),
                new Section("digitalEyewear", "DigitalEyewearBehaviour", new[]
                {
                    new Property("cameraOffset", "mCameraOffset", PropertyType.FLOAT),
                    new Property("distortionRenderingMode", "mDistortionRenderingMode", PropertyType.ENUM),
                    new Property("distortionRenderingLayer", "mDistortionRenderingLayer", PropertyType.INT),
                    new Property("eyewearType", "mEyewearType", PropertyType.ENUM),
                    new Property("stereoFramework", "mStereoFramework", PropertyType.ENUM),
                    new Property("viewerName", "mViewerName", PropertyType.STRING),
                    new Property("viewerManufacturer", "mViewerManufacturer", PropertyType.STRING),
                    new Property("useCustomViewer", "mUseCustomViewer", PropertyType.BOOL),
                    new Property("customViewer", "mCustomViewer", PropertyType.CLASS),
                    new Property("Version", "mCustomViewer.Version", PropertyType.FLOAT),
                    new Property("Name", "mCustomViewer.Name", PropertyType.STRING),
                    new Property("Manufacturer", "mCustomViewer.Manufacturer", PropertyType.STRING),
                    new Property("ButtonType", "mCustomViewer.ButtonType", PropertyType.ENUM),
                    new Property("ScreenToLensDistance", "mCustomViewer.ScreenToLensDistance", PropertyType.FLOAT),
                    new Property("InterLensDistance", "mCustomViewer.InterLensDistance", PropertyType.FLOAT),
                    new Property("TrayAlignment", "mCustomViewer.TrayAlignment", PropertyType.ENUM),
                    new Property("LensCenterToTrayDistance", "mCustomViewer.LensCenterToTrayDistance",
                        PropertyType.FLOAT),
                    new Property("DistortionCoefficients", "mCustomViewer.DistortionCoefficients", PropertyType.VECTOR2),
                    new Property("FieldOfView", "mCustomViewer.FieldOfView", PropertyType.VECTOR4),
                    new Property("ContainsMagnet", "mCustomViewer.ContainsMagnet", PropertyType.BOOL),
                }),
                new Section("databaseLoad", "DatabaseLoadAbstractBehaviour", new[]
                {
                    new Property("dataSetsToLoad", "mDataSetsToLoad", PropertyType.STRING_ARRAY),
                    new Property("dataSetsToActivate", "mDataSetsToActivate", PropertyType.STRING_ARRAY),
                }),
                new Section("videoBackground", "VideoBackgroundManagerAbstractBehaviour", new[]
                {
                    new Property("clippingMode", "mClippingMode", PropertyType.ENUM),
                    new Property("matteShader", "mMatteShader", PropertyType.OBJECT),
                    new Property("videoBackgroundEnabled", "mVideoBackgroundEnabled", PropertyType.BOOL),
                }),
                new Section("smartTerrainTracker", "SmartTerrainTrackerAbstractBehaviour", new[]
                {
                    new Property("autoInitTracker", "mAutoInitTracker", PropertyType.BOOL),
                    new Property("autoStartTracker", "mAutoStartTracker", PropertyType.BOOL),
                    new Property("autoInitBuilder", "mAutoInitBuilder", PropertyType.BOOL),
                    new Property("sceneUnitsToMillimeter", "mSceneUnitsToMillimeter", PropertyType.FLOAT),
                }),
                new Section("deviceTracker", "DeviceTrackerAbstractBehaviour", new[]
                {
                    new Property("autoInitTracker", "mAutoInitTracker", PropertyType.BOOL),
                    new Property("autoStartTracker", "mAutoStartTracker", PropertyType.BOOL),
                    new Property("posePrediction", "mPosePrediction", PropertyType.BOOL),
                    new Property("modelCorrectionMode", "mModelCorrectionMode", PropertyType.ENUM),
                    new Property("modelTransformEnabled", "mModelTransformEnabled", PropertyType.BOOL),
                    new Property("modelTransform", "mModelTransform", PropertyType.VECTOR3),
                }),
                new Section("webcam", "WebCamAbstractBehaviour", new[]
                {
                    new Property("deviceNameSetInEditor", "mDeviceNameSetInEditor", PropertyType.STRING),
                    new Property("flipHorizontally", "mFlipHorizontally", PropertyType.BOOL),
                    new Property("turnOffWebCam", "mTurnOffWebCam", PropertyType.BOOL),
                    new Property("renderTextureLayer", "RenderTextureLayer", PropertyType.INT),
                }),

            };
        }

        private void Run(string outputFile)
        {
            var bhvr = GameObject.FindObjectOfType<VuforiaBehaviour>();
            mARCamera = bhvr.gameObject;

            using (mWriter = new StreamWriter(outputFile))
            {
                ExportHeader();

                foreach (var section in mSections)
                {
                    ExportSection(section);
                }

            }

            mWriter = null;
        }

        private void ExportHeader()
        {
            mWriter.WriteLine("%YAML 1.1");
            mWriter.WriteLine("%TAG !u! tag:unity3d.com,2011:");
            mWriter.WriteLine("--- !u!114 &11400000");
            mWriter.WriteLine("MonoBehaviour:");
            mWriter.WriteLine("  m_ObjectHideFlags: 0");
            mWriter.WriteLine("  m_PrefabParentObject: {fileID: 0}");
            mWriter.WriteLine("  m_PrefabInternal: {fileID: 0}");
            mWriter.WriteLine("  m_GameObject: {fileID: 0}");
            mWriter.WriteLine("  m_Enabled: 1");
            mWriter.WriteLine("  m_EditorHideFlags: 0");
            mWriter.WriteLine("  m_Script: {fileID: 11500000, guid: d76b89ff4afc5a04195eab54bc63d331, type: 3}");
            mWriter.WriteLine("  m_Name: VuforiaConfiguration");
            mWriter.WriteLine("  m_EditorClassIdentifier: ");
        }


        private void ExportSection(Section section)
        {

            var behaviour = mARCamera.GetComponent(section.BehaviourName);
            if (behaviour == null)
            {
                Debug.LogWarning("Could not find " + section.BehaviourName);
                return;
            }

            var serializedObject = new SerializedObject(behaviour);
            mWriter.WriteLine("  " + section.ConfigurationName + ":");


            foreach (var property in section.Properties)
            {
                mIndent = "    ";
                var indent = property.BehaviourName.Length - property.BehaviourName.Replace(".", "").Length;
                for (int i = 0; i < indent; i++)
                    mIndent += "  ";

                switch (property.Type)
                {
                    case PropertyType.STRING:
                        WriteProperty(property.ConfigurationName, serializedObject.FindProperty(property.BehaviourName).stringValue);
                        break;
                    case PropertyType.BOOL:
                        WriteProperty(property.ConfigurationName, serializedObject.FindProperty(property.BehaviourName).boolValue);
                        break;
                    case PropertyType.INT:
                        WriteProperty(property.ConfigurationName, serializedObject.FindProperty(property.BehaviourName).intValue);
                        break;
                    case PropertyType.FLOAT:
                        WriteProperty(property.ConfigurationName, serializedObject.FindProperty(property.BehaviourName).floatValue);
                        break;
                    case PropertyType.ENUM:
                        // enumValueIndex does not work here, because it uses indices from 0,1,2,... independent from the enum specification
                        WriteProperty(property.ConfigurationName, serializedObject.FindProperty(property.BehaviourName).intValue);
                        break;
                    case PropertyType.VECTOR2:
                        WriteProperty(property.ConfigurationName, serializedObject.FindProperty(property.BehaviourName).vector2Value);
                        break;
                    case PropertyType.VECTOR3:
                        WriteProperty(property.ConfigurationName, serializedObject.FindProperty(property.BehaviourName).vector3Value);
                        break;
                    case PropertyType.VECTOR4:
                        WriteProperty(property.ConfigurationName, serializedObject.FindProperty(property.BehaviourName).vector4Value);
                        break;
                    case PropertyType.OBJECT:
                        WriteProperty(property.ConfigurationName, serializedObject.FindProperty(property.BehaviourName).objectReferenceValue);
                        break;
                    case PropertyType.CLASS:
                        WriteProperty(property.ConfigurationName, "");
                        break;
                    case PropertyType.STRING_ARRAY:
                        string[] data;
                        serializedObject.FindProperty(property.BehaviourName).GetArrayItems(out data);
                        WriteProperty(property.ConfigurationName, data);
                        break;
                }
            }
        }


        private void WriteProperty(string property, string value)
        {
            mWriter.WriteLine(mIndent + property + ": " + value);
        }

        private void WriteProperty(string property, float value)
        {
            WriteProperty(property, value.ToString());
        }

        private void WriteProperty(string property, int value)
        {
            WriteProperty(property, value.ToString());
        }

        private void WriteProperty(string property, bool value)
        {
            WriteProperty(property, value ? 1 : 0);
        }

        private void WriteProperty(string property, Vector2 value)
        {
            var str = "{x: " + value.x + ", y: " + value.y + "}";
            WriteProperty(property, str);
        }

        private void WriteProperty(string property, Vector3 value)
        {
            var str = "{x: " + value.x + ", y: " + value.y + ", z: " + value.z + "}";
            WriteProperty(property, str);
        }

        private void WriteProperty(string property, Vector4 value)
        {
            var str = "{x: " + value.x + ", y: " + value.y + ", z: " + value.z + ", w: " + value.w + "}";
            WriteProperty(property, str);
        }

        private void WriteProperty(string property, string[] value)
        {
            WriteProperty(property, "");
            foreach (var item in value)
                mWriter.WriteLine(mIndent + "- " + item);
        }


        private void WriteProperty(string property, UnityEngine.Object obj)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            var guid = AssetDatabase.AssetPathToGUID(path);
            var fileId = "4800000"; // Shaders have id 48
            var type = "3";
            var str = "{fileID: " + fileId + ", guid: " + guid + ", type: " + type + "}";
            WriteProperty(property, str);
        }

        #endregion // PRIVATE_METHODS
    }
}