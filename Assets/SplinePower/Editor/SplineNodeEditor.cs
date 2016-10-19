using System;
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(SplineNode))]
[CanEditMultipleObjects]
public class SplineNodeEditor : Editor
{
    private SerializedProperty _SplineFormerProp;
    private SerializedProperty _AdditionalLoftAngleProp;
    private SerializedProperty _LeftPCoefficientProp;
    private SerializedProperty _TangentGuidesModeProp;
    private SerializedProperty _RightPCoefficientProp;

    public void OnEnable()
    {
        // Setup the SerializedProperties
        _SplineFormerProp = serializedObject.FindProperty("SplineFormer");
        _AdditionalLoftAngleProp = serializedObject.FindProperty("AdditionalLoftAngle");
        _TangentGuidesModeProp = serializedObject.FindProperty("TangentGuidesMode");
        _LeftPCoefficientProp = serializedObject.FindProperty("LeftPCoefficient");
        _RightPCoefficientProp = serializedObject.FindProperty("RightPCoefficient");
    }

    public override void OnInspectorGUI()
    {
        bool needRebuildMesh = false;
        serializedObject.Update();

        SplineFormerEditor.DrawHeaderTexture();
        var splineNodes = targets.OfType<SplineNode>().ToList();

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField(
            "Spline Former Host",
            _SplineFormerProp.objectReferenceValue,
            typeof(SplineFormer),
            true);
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginChangeCheck();

        _AdditionalLoftAngleProp.floatValue =
            EditorGUILayout.Slider(
            "Additional Loft Angle",
            _AdditionalLoftAngleProp.floatValue,
            -180f, 180f);

        EditorGUILayout.PropertyField(
            _TangentGuidesModeProp);

        if (splineNodes.TrueForAll(p => p.TangentGuidesMode == SplineNode.SplineTangentGuidesMode.Manual))
        {
            EditorGUILayout.PropertyField(
            _LeftPCoefficientProp,
            new GUIContent("Left Tangent Coefficient"));

            EditorGUILayout.PropertyField(
            _RightPCoefficientProp,
            new GUIContent("Right Tangent Coefficient"));
        }

        if (EditorGUI.EndChangeCheck())
        {
            needRebuildMesh = true;
        }

        if (splineNodes.Count == 1)
        {
            if (GUILayout.Button("Split", GUILayout.Width(100)))
            {
                splineNodes[0].Split();
                needRebuildMesh = true;
            }
        }

        if (needRebuildMesh)
        {
            needRebuildMesh = false;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            foreach (var splineNode in splineNodes)
            {
                if (splineNode.SplineFormer != null)
                {
                    splineNode.SplineFormer.InvalidateMesh();
                    EditorUtility.SetDirty(splineNode.SplineFormer);
                }
            }
        }
    }

}
