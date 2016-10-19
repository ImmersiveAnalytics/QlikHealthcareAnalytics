using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using uObject = UnityEngine.Object;

[CustomEditor(typeof(SplineFormer))]
public class SplineFormerEditor : Editor
{
    static public class Exporter
    {
        const string AssetsDirectory = "Assets/";

        public static void MeshToAsset(Mesh mesh, string filename, SplineFormer.ExportOptionsContainer options)
        {
            try
            {
                filename = Path.ChangeExtension(filename, ".asset");
                string path = null;

                if (options.ShowSaveAsDialog)
                {
                    path = EditorUtility.SaveFilePanel("Export to asset", AssetsDirectory, filename, "asset");
                    if (String.IsNullOrEmpty(path)) return;
                }
                else
                {
                    path = AssetsDirectory + options.DefaultFolder + "/" + filename;
                }

                int index = path.LastIndexOf(AssetsDirectory, StringComparison.Ordinal);
                if (index < 0)
                {
                    throw new ArgumentException(
                        String.Format("Asset can't be saved outside the folder \"{0}\"", AssetsDirectory)
                        );
                }

                PrepareDirectory(path);

                mesh = (Mesh)Instantiate(mesh);
                path = path.Remove(0, index);
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.SaveAssets();
                AssetDatabase.CreateAsset(mesh, path);
                AssetDatabase.SaveAssets();

                filename = Path.GetFileName(path);
                string msg = String.Format("Mesh exported as {0}.", filename);
                if (options.ShowExportResultDialog)
                {
                    EditorUtility.DisplayDialog("Exported successfully", msg, "OK");
                }
                else
                {
                    Debug.Log(msg);
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (options.ShowExportResultDialog)
                {
                    EditorUtility.DisplayDialog("Export failed", msg, "OK");
                }
                else
                {
                    Debug.LogError("Export failed:" + msg);
                }
            }
        }

        private static void PrepareDirectory(string path)
        {
            string directory = Path.GetDirectoryName(path);
            Directory.CreateDirectory(directory);
        }

        public static string MeshToObjString(Mesh m)
        {
            StringBuilder sb = new StringBuilder();

            foreach (Vector3 v in m.vertices)
            {
                sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
            }
            sb.Append("\n");
            foreach (Vector3 v in m.normals)
            {
                sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
            }
            sb.Append("\n");
            foreach (Vector3 v in m.uv)
            {
                sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
            }
            for (int material = 0; material < m.subMeshCount; material++)
            {
                sb.Append("\n");
                int[] triangles = m.GetTriangles(material);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                                            triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
                }
            }
            return sb.ToString();
        }

        public static void MeshToObjFile(Mesh mesh, string filename, SplineFormer.ExportOptionsContainer options)
        {
            try
            {
                filename = Path.ChangeExtension(filename, ".obj");
                string path = null;
                if (options.ShowSaveAsDialog)
                {
                    path = EditorUtility.SaveFilePanel("Export to OBJ", "", filename, "obj");
                    if (String.IsNullOrEmpty(path)) return;
                }
                else
                {
                    path = AssetsDirectory + options.DefaultFolder + "/" + filename;
                }

                PrepareDirectory(path);

                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.Write(MeshToObjString(mesh));
                }

                filename = Path.GetFileName(path);
                string msg = String.Format("Mesh exported as {0}.", filename);

                if (options.ShowExportResultDialog)
                {
                    EditorUtility.DisplayDialog("Exported successfully", msg, "OK");
                }
                else
                {
                    Debug.Log(msg);
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (options.ShowExportResultDialog)
                {
                    EditorUtility.DisplayDialog("Export failed", msg, "OK");
                }
                else
                {
                    Debug.LogError("Export failed:" + msg);
                }
            }
        }
    }
	
	private const string thread = "http://edentec.org/unity/spline-power/thread";
    private const string docs = "http://edentec.org//unity/spline-power/docs";


    public static void DrawHeaderTexture()
    {
        Texture tex = (Texture2D)Resources.Load("SP_header");
        Rect rect = GUILayoutUtility.GetRect(0f, 0f);
        rect.width = tex.width;
        rect.height = tex.height;
        rect.y += 2;
        GUILayout.Space(rect.height + 2);
        GUI.DrawTexture(rect, tex);

        Rect rectButtons = rect;
        float buttWidth = rect.height;
        rectButtons.width = buttWidth;
        Texture threadTex = (Texture2D)Resources.Load("unity_icon_27");
        rectButtons.x = 200;
        if (GUI.Button(rectButtons, new GUIContent("", "Unity forums thread")))
        {
            OpenThread();
        }
        GUI.DrawTexture(ShrinkRect(rectButtons, 2), threadTex);

        Texture docsTex = (Texture2D)Resources.Load("docs_icon_27");
        rectButtons.x += buttWidth;
        if (GUI.Button(rectButtons, new GUIContent("", "Docs")))
        {
            OpenDocs();
        }
        GUI.DrawTexture(ShrinkRect(rectButtons, 2), docsTex);
    }
	
	[MenuItem("SplinePower!/Unity forums thread")]
    static void OpenThread()
    {
        Help.BrowseURL(thread);
    }

    [MenuItem("SplinePower!/Docs")]
    static void OpenDocs()
    {
        Help.BrowseURL(docs);
    }	

    static Rect ShrinkRect(Rect rect, int pad)
    {
        rect.x += pad;
        rect.y += pad;
        rect.width -= 2 * pad;
        rect.height -= 2 * pad;
        return rect;
    }

    private SplineFormer _splineFormer;

    private readonly Color _separatorsColor = new Color(0, 0, 0, 0.5f);
    private readonly Color _invalid = Color.gray;
    private readonly Color _valid = new Color(0.5f, 0.95f, 1f);
    private readonly int _fieldWidth = 50;
    private readonly int _buttonWidth = 95;

    private Dictionary<string, bool> _foldouts = new Dictionary<string, bool>();
    private bool _showVisualOptions;
    private bool _showExportOptions;

    private void GetSplineFormer()
    {
        if (_splineFormer == null)
            _splineFormer = (SplineFormer)target;
    }

    // Use this for initialization
    private void Start()
    {
        GetSplineFormer();
    }

    private void RecordUndo()
    {
        Undo.RecordObject(target, "SplineFormer Changed");
    }

    private void RecordDeepUndo()
    {
        Undo.RegisterFullObjectHierarchyUndo(target, "SplineFormer Changed");
    }

    public override void OnInspectorGUI()
    {
        GetSplineFormer();
        DrawHeaderTexture();

        bool needRebuildMesh = false;
        bool needAddNode = false;

        Color defaultBackColor = GUI.backgroundColor;
        Color defaultColor = GUI.color;

        EditorGUILayout.LabelField("Lofting Groups");
        var groups = _splineFormer.LoftingGroups.ToList();

        EditorGUILayout.BeginVertical();

        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(GUILayout.Height(2)), _separatorsColor);
        EditorGUI.indentLevel++;
        foreach (var group in groups)
        {
            GUI.backgroundColor = group.IsValid ? _valid : _invalid;
            int groupIndex = groups.IndexOf(group);
            if (groupIndex != 0)
            {
                EditorGUI.DrawRect(EditorGUILayout.GetControlRect(GUILayout.Height(1)), _separatorsColor);
            }

            EditorGUILayout.LabelField(
                String.Format("#{0} - {1}",
                groupIndex, group.IsValid ? "Ready" : "Not ready")
                );

            EditorGUI.BeginChangeCheck();
            //======================
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Segment Mesh", GUILayout.MinWidth(_fieldWidth));
            EditorGUILayout.LabelField("MeshFilter", GUILayout.MinWidth(_fieldWidth));
            EditorGUILayout.LabelField("MeshCollider", GUILayout.MinWidth(_fieldWidth));
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            group.SegmentMesh =
                (Mesh)EditorGUILayout.ObjectField(group.SegmentMesh, typeof(Mesh), false, GUILayout.MinWidth(_fieldWidth));
            group.MeshFilter =
                EditorGUILayout.ObjectField(group.MeshFilter, typeof(MeshFilter), true, GUILayout.MinWidth(_fieldWidth)) as MeshFilter;
            group.MeshCollider =
                EditorGUILayout.ObjectField(group.MeshCollider, typeof(MeshCollider), true, GUILayout.MinWidth(_fieldWidth)) as MeshCollider;
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                if (group.MeshFilter != null) group.MeshFilter.sharedMesh = null;
                if (group.MeshCollider != null) group.MeshCollider.sharedMesh = null;
            }

            EditorGUILayout.LabelField("Interval");
            group.StartPosition = EditorGUILayout.Slider("Start Position", group.StartPosition, 0f, 1f);
            group.EndPosition = EditorGUILayout.Slider("End Position", group.EndPosition, 0f, 1f);
            //===========================
            if (FoldoutOption("Caps", group))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Start Piece");
                group.StartPiece =
                    (Mesh)EditorGUILayout.ObjectField(group.StartPiece, typeof(Mesh), false, GUILayout.MinWidth(_fieldWidth * 2));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("End Piece");
                group.EndPiece =
                    (Mesh)EditorGUILayout.ObjectField(group.EndPiece, typeof(Mesh), true, GUILayout.MinWidth(_fieldWidth * 2));
                EditorGUILayout.EndHorizontal();
            }
            //====================
            if (FoldoutOption("Processing", group))
            {

                group.ProcessOriginNormals = EditorGUILayout.ToggleLeft("Process Origin Normals", group.ProcessOriginNormals);
                if (group.ProcessOriginNormals)
                {
                    group.RecalculateNormals = false;
                    group.SmoothNormals = false;
                }

                group.ProcessOriginTangents = EditorGUILayout.ToggleLeft("Process Origin Tangents", group.ProcessOriginTangents);

                group.Weld = EditorGUILayout.ToggleLeft("Weld Close Vertices", group.Weld);

                float value = Mathf.Sqrt(group.WeldingDistance * 10f);
                value = EditorGUILayout.Slider("Welding Distance", value, 0.01f, 2f);
                group.WeldingDistance = (value * value) * 0.1f;

                group.RecalculateNormals = EditorGUILayout.ToggleLeft("Recalculate Normals", group.RecalculateNormals);
                if (!group.RecalculateNormals) group.SmoothNormals = false;
                if (group.RecalculateNormals) group.ProcessOriginNormals = false;

                group.SmoothNormals = EditorGUILayout.ToggleLeft("Smooth Normals", group.SmoothNormals);
                if (group.SmoothNormals)
                {
                    group.RecalculateNormals = true;
                    group.ProcessOriginNormals = false;
                }

            }
            //====================

            if (EditorGUI.EndChangeCheck())
            {
                needRebuildMesh = true;
            }

            //================
            GUI.backgroundColor = defaultBackColor;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Remove", GUILayout.Width(_buttonWidth)))
            {
                RecordUndo();
                _splineFormer.LoftingGroups.Remove(group);
            }

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(group.ResultMesh == null);
            string filename = "-";
            if (group.ResultMesh != null)
            {
                filename = group.ResultMesh.name;
            }
            if (GUILayout.Button("Save to OBJ", GUILayout.Width(_buttonWidth)))
            {
                Exporter.MeshToObjFile(group.ResultMesh, filename, _splineFormer.ExportOptions);
            }

            if (GUILayout.Button("Save to Asset", GUILayout.Width(_buttonWidth)))
            {
                Exporter.MeshToAsset(group.ResultMesh, filename, _splineFormer.ExportOptions);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();
        }
        EditorGUI.indentLevel--;
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(GUILayout.Height(2)), _separatorsColor);

        EditorGUILayout.EndVertical();

        GUI.backgroundColor = defaultBackColor;
        GUI.color = defaultColor;

        if (GUILayout.Button("Add Group", GUILayout.Width(_buttonWidth)))
        {
            RecordUndo();
            _splineFormer.LoftingGroups.Add(new SplineFormer.LoftingGroup());
        }

        EditorGUI.BeginChangeCheck();
        _splineFormer.SegmentsNumber = Mathf.Max(1, EditorGUILayout.IntField("Segments Count", _splineFormer.SegmentsNumber));
        _splineFormer.Coefficient = Mathf.Max(0.01f, EditorGUILayout.FloatField("Coefficient", _splineFormer.Coefficient));
        _splineFormer.LoftAngle = EditorGUILayout.Slider("Loft Angle", _splineFormer.LoftAngle, 0f, 360f);
        _splineFormer.LoftDirection = EditorGUILayout.Vector3Field("Loft Direction", _splineFormer.LoftDirection);
        if (_splineFormer.LoftDirection.sqrMagnitude < 0.1f) _splineFormer.LoftDirection = Vector3.forward;

        _splineFormer.SegmentScale = EditorGUILayout.Vector3Field("Segment Scale", _splineFormer.SegmentScale);

        _splineFormer.QuadraticSmooth = EditorGUILayout.Toggle("Quadratic Smooth", _splineFormer.QuadraticSmooth);
        var loop = EditorGUILayout.Toggle("Loop", _splineFormer.Loop);
        _splineFormer.RollerCoasterFix = EditorGUILayout.Toggle("Roller Coaster Fix", _splineFormer.RollerCoasterFix);
        EditorGUILayout.Separator();

        if (EditorGUI.EndChangeCheck())
        {
            needRebuildMesh = true;
            RecordDeepUndo();
            if (_splineFormer.Loop != loop)
            {
                Undo.RecordObject(_splineFormer.StartNode.transform, "StartNode move");
                Undo.RecordObject(_splineFormer.EndNode.transform, "EndNode move");
                _splineFormer.Loop = loop;
            }
        }

        EditorGUILayout.Separator();


        _showVisualOptions = EditorGUILayout.Foldout(_showVisualOptions, "Visual Options");
        if (_showVisualOptions)
        {
            EditorGUI.BeginChangeCheck();
            _splineFormer.VisualOptions.NodeSize = Mathf.Max(0.001f,
                EditorGUILayout.FloatField("Node Size", _splineFormer.VisualOptions.NodeSize));

            _splineFormer.VisualOptions.ShowNodeLinks = EditorGUILayout.Toggle("Show Node Links",
                _splineFormer.VisualOptions.ShowNodeLinks);

            _splineFormer.VisualOptions.ShowSegmentsPath = EditorGUILayout.Toggle("Show Segments Path",
                _splineFormer.VisualOptions.ShowSegmentsPath);

            _splineFormer.VisualOptions.ShowTangentNodes = EditorGUILayout.Toggle("Show Tangent Nodes",
                _splineFormer.VisualOptions.ShowTangentNodes);

            _splineFormer.VisualOptions.ShowTangents = EditorGUILayout.Toggle("Show Tangents",
                _splineFormer.VisualOptions.ShowTangents);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
            }
        }

        _showExportOptions = EditorGUILayout.Foldout(_showExportOptions, "Export Options");
        if (_showExportOptions)
        {
            EditorGUI.BeginChangeCheck();
            _splineFormer.ExportOptions.ShowSaveAsDialog = EditorGUILayout.Toggle("Show \"Save As\" Dialog",
            _splineFormer.ExportOptions.ShowSaveAsDialog);

            _splineFormer.ExportOptions.ShowExportResultDialog = EditorGUILayout.Toggle("Show Export Result Dialog",
            _splineFormer.ExportOptions.ShowExportResultDialog);

            EditorGUILayout.BeginHorizontal();

            var assetsFolder = Path.GetFullPath("Assets/").Replace('\\', '/');

            EditorGUILayout.LabelField("Default Export Folder", _splineFormer.ExportOptions.DefaultFolder);

            if (GUILayout.Button("Select"))
            {
                var defaultFolder = _splineFormer.ExportOptions.DefaultFolder;

                if (!Directory.Exists(defaultFolder))
                {
                    defaultFolder = assetsFolder;
                }
                var selectedFolder = EditorUtility.SaveFolderPanel("Default Export Folder",
                        defaultFolder, "Splines");

                if (!selectedFolder.Contains(assetsFolder))
                {
                    EditorUtility.DisplayDialog(
                        "Path is outside the assets folder",
                        String.Format("Asset can't be saved outside of the folder \"{0}\"",
                            assetsFolder),
                        "OK");
                }
                else
                {
                    _splineFormer.ExportOptions.DefaultFolder = selectedFolder.Replace(assetsFolder, "");
                }
            }
            EditorGUILayout.EndHorizontal();

            _splineFormer.ExportOptions.ExtendedNaming = EditorGUILayout.Toggle("Extended Naming",
            _splineFormer.ExportOptions.ExtendedNaming);

            if (_splineFormer.ExportOptions.ExtendedNaming)
            {
                EditorGUI.indentLevel++;

                _splineFormer.ExportOptions.AddObjectName = EditorGUILayout.Toggle("Add Object Name",
                _splineFormer.ExportOptions.AddObjectName);

                _splineFormer.ExportOptions.AddLoftingGroupIndex = EditorGUILayout.Toggle("Add Lofting Group Index",
                _splineFormer.ExportOptions.AddLoftingGroupIndex);

                _splineFormer.ExportOptions.CustomName = EditorGUILayout.TextField("Custom Name",
                _splineFormer.ExportOptions.CustomName);

                _splineFormer.ExportOptions.AddDateTime = EditorGUILayout.Toggle("Add Date/Time Stamp",
                _splineFormer.ExportOptions.AddDateTime);

                _splineFormer.ExportOptions.AddAutoIncrementNumber = EditorGUILayout.Toggle("Add Auto Increment Number",
                _splineFormer.ExportOptions.AddAutoIncrementNumber);

                EditorGUI.indentLevel--;
            }


            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
            }
        }

        EditorGUILayout.BeginHorizontal();
        needAddNode |= GUILayout.Button("Add Node", GUILayout.Width(_buttonWidth));
        needRebuildMesh |= GUILayout.Button("Refresh", GUILayout.Width(_buttonWidth));
        EditorGUILayout.EndHorizontal();

        if (needAddNode)
        {
            _splineFormer.AddNewNode();
        }

        if (needRebuildMesh || needAddNode)
        {
            _splineFormer.InvalidateMesh();
            EditorUtility.SetDirty(target);
        }
    }

    private void OnSceneGui()
    {
        GetSplineFormer();
    }

    private bool FoldoutOption(string label, SplineFormer.LoftingGroup group)
    {
        int index = _splineFormer.LoftingGroups.IndexOf(group);
        string key = label + index;
        if (!_foldouts.ContainsKey(key))
            _foldouts[key] = false;

        _foldouts[key] = EditorGUILayout.Foldout(_foldouts[key], label);
        return _foldouts[key];
    }

    [PostProcessScene]
    public static void OnPostprocessScene()
    {
        var formers = uObject.FindObjectsOfType<SplineFormer>();
        foreach (var former in formers)
        {
            former.Clear();
            EditorUtility.SetDirty(former);
            EditorUtility.SetDirty(former.gameObject);

            foreach (var group in former.LoftingGroups)
            {
                if (group.MeshCollider != null)
                {
                    EditorUtility.SetDirty(group.MeshCollider);
                }
                if (group.MeshFilter != null)
                {
                    EditorUtility.SetDirty(group.MeshFilter);
                }
            }
        }


        AssetDatabase.SaveAssets();
    }
}