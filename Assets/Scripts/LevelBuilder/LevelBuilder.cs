using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LevelBuilder : EditorWindow
{
    private const string _path1 = "Assets/Editor Resources/Buildings";
    private const string _path2 = "Assets/Editor Resources/Buildings";
    private const string _path3 = "Assets/Editor Resources/Buildings";

    private Vector2 _scrollPosition;
    private int _selectedElement;
    private int _currentElement = 0;
    private List<GameObject> _catalog = new List<GameObject>();
    private bool _building;

    private GameObject _createdObject;
    private GameObject _parent;
    private GameObject _tempParent;
    private GameObject _prefab;

    [MenuItem("Level/Builder")]
    private static void ShowWindow()
    {
        GetWindow(typeof(LevelBuilder));
    }

    private void OnFocus()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI; //каждый кадр вызывается Ons
        RefreshCatalog();
    }

    private void OnGUI()
    {
        _parent = (GameObject)EditorGUILayout.ObjectField("Parent", _parent, typeof(GameObject), true);
        _tempParent = (GameObject)EditorGUILayout.ObjectField("Temp parent", _tempParent, typeof(GameObject), true);
        if (_parent != null && _tempParent != null)
        {
            if (_selectedElement != _currentElement)
            {
                if (_tempParent.transform.childCount != 0)
                {
                    DestroyImmediate(_tempParent.transform.GetChild(0).gameObject);
                }
                _prefab = Instantiate(_catalog[_selectedElement]);                
                _prefab.transform.parent = _tempParent.transform;
                _currentElement = _selectedElement;
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            if (_createdObject != null)
            {
                EditorGUILayout.LabelField("Created Object Settings");
                Transform createdTransform = _createdObject.transform;

                createdTransform.position = EditorGUILayout.Vector3Field("Position", createdTransform.position);
                createdTransform.rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Position", createdTransform.rotation.eulerAngles));
                createdTransform.localScale = EditorGUILayout.Vector3Field("Position", createdTransform.localScale);
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            _building = GUILayout.Toggle(_building, "Start building", "Button", GUILayout.Height(60));

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.BeginVertical(GUI.skin.window);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            DrawCatalog(GetCatalogIcons());
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (_building) //решжим постройки
        {
            if (Raycast(out Vector3 contactPoint))
            {
                DrawBuild(contactPoint);

                if (CheckInput())
                {
                    CreateObject(contactPoint);
                }

                sceneView.Repaint();
            }
        }
    }

    private bool Raycast(out Vector3 contactPoint)
    {
        Ray guiRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        contactPoint = Vector3.zero;

        LayerMask layer = LayerMask.GetMask("Build");
        if (Physics.Raycast(guiRay, out RaycastHit raycastHit))
        {
            if (raycastHit.collider.gameObject.layer != layer)
            {
                contactPoint = raycastHit.point;
                return true;
            }                                
        }
        return false;
    }

    private void DrawBuild(Vector3 position)
    {
        _prefab.transform.position = position;
    }

    private bool CheckInput()
    {
        HandleUtility.AddDefaultControl(0);

        return Event.current.type == EventType.MouseDown && Event.current.button == 0;
    }

    private void CreateObject(Vector3 position)
    {
        if (_selectedElement < _catalog.Count)
        {
            GameObject prefab = _catalog[_selectedElement];
            //GameObject gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            LayerMask layer = LayerMask.GetMask("Ground");
            if (Physics.BoxCast(position + Vector3.up * 10, prefab.GetComponent<Collider>().bounds.size, -Vector3.up, Quaternion.identity, 10f, layer))
            {
                _createdObject = Instantiate(prefab);
                _createdObject.layer = LayerMask.NameToLayer("Build");
                _createdObject.transform.position = position;
                _createdObject.transform.parent = _parent.transform;
                _createdObject.gameObject.GetComponent<BoxCollider>().enabled = true;

                Undo.RegisterCreatedObjectUndo(_createdObject, "Create Building");
            }       
        }
    }

    private void DrawCatalog(List<GUIContent> catalogIcons)
    {
        GUILayout.Label("Buildings");
        _selectedElement = GUILayout.SelectionGrid(_selectedElement, catalogIcons.ToArray(), 4, GUILayout.Width(400), GUILayout.Height(1000));
    }

    private List<GUIContent> GetCatalogIcons()
    {
        List<GUIContent> catalogIcons = new List<GUIContent>();

        foreach (var element in _catalog)
        {
            Texture2D texture = AssetPreview.GetAssetPreview(element);
            catalogIcons.Add(new GUIContent(texture));
        }

        return catalogIcons;
    }

    private void RefreshCatalog()
    {
        _catalog.Clear();

        System.IO.Directory.CreateDirectory(_path1);
        string[] prefabFiles = System.IO.Directory.GetFiles(_path1, "*.prefab");
        foreach (var prefabFile in prefabFiles)
            _catalog.Add(AssetDatabase.LoadAssetAtPath(prefabFile, typeof(GameObject)) as GameObject);
    }
}