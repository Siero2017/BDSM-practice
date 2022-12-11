using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LevelBuilder : EditorWindow
{
    private const string _builds = "Assets/Editor Resources/Buildings";
    private const string _decor = "Assets/Editor Resources/Decor";
    private const string _vehicles = "Assets/Editor Resources/Vehicles";
    private int _selectedTabNumber = 0;
    private string[] _tabNames = { "Buildings", "Decor", "Vehicles" };

    private Vector2 _scrollPosition;
    private int _selectedElement;
    private int _currentElement = 0;
    private List<GameObject> _catalog = new List<GameObject>();
    private bool _building;

    private GameObject _createdObject;
    private GameObject _parent;
    private GameObject _tempParent;
    private GameObject _tempPrefab;

    [MenuItem("Level/MilitaryBaseBuild")]
    private static void ShowWindow()
    {
        GetWindow(typeof(LevelBuilder));
    }

    private void OnFocus()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        if (_tempParent != null && _tempParent.transform.childCount != 0)
        {
            DestroyImmediate(_tempParent.transform.GetChild(0).gameObject);
        }
    }

    private void OnGUI()
    {
        _parent = (GameObject)EditorGUILayout.ObjectField("Parent", _parent, typeof(GameObject), true);
        _tempParent = (GameObject)EditorGUILayout.ObjectField("Temp parent", _tempParent, typeof(GameObject), true);
        ChooseTabs();
        if (_parent != null && _tempParent != null)
        {
            if (_selectedElement != _currentElement)
            {
                if (_tempParent.transform.childCount != 0)
                {
                    DestroyImmediate(_tempParent.transform.GetChild(0).gameObject);
                }
                _tempPrefab = Instantiate(_catalog[_selectedElement]);                
                _tempPrefab.transform.parent = _tempParent.transform;
                _currentElement = _selectedElement;
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            if (_createdObject != null)
            {
                EditorGUILayout.LabelField("Created Object Settings");                
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
        if (_building)
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

        if (Physics.Raycast(guiRay, out RaycastHit raycastHit))
        {
            contactPoint = raycastHit.point;
            return true;            
        }
        return false;
    }

    private void DrawBuild(Vector3 position)
    {
        if (_tempPrefab != null)
        {
            _tempPrefab.transform.position = position;
        }
    }

    private void ChooseTabs()
    {
        _selectedTabNumber = GUILayout.Toolbar(_selectedTabNumber, _tabNames);

        switch (_selectedTabNumber)
        {
            case 0:
                RefreshCatalog(_builds, _catalog);
                break;
            case 1:
                RefreshCatalog(_decor, _catalog);
                break;
            case 2:
                RefreshCatalog(_vehicles, _catalog);
                break;
        }
    }

    private bool CheckInput()
    {
        HandleUtility.AddDefaultControl(0);

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.J)
        {
            _tempPrefab.transform.Rotate(new Vector3(0, 0, 5));
        }
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.K)
        {
            _tempPrefab.transform.Rotate(new Vector3(0, 0, -5));
        }

        if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.N)
        {
            _tempPrefab.transform.localScale += Vector3.one / 2;
        }
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.M)
        {
            _tempPrefab.transform.localScale += -Vector3.one / 2;
        }

        return Event.current.type == EventType.MouseDown && Event.current.button == 0;
    }

    private void CreateObject(Vector3 position)
    {
        if (_selectedElement < _catalog.Count)
        {
            GameObject prefab = _catalog[_selectedElement];           
            LayerMask layer = LayerMask.GetMask("Build");
            Collider[] colliders = Physics.OverlapBox(position, _tempPrefab.GetComponent<BoxCollider>().size / 2, Quaternion.identity, layer);
            if (colliders.Length == 0)
            {
                _createdObject = Instantiate(prefab);
                _createdObject.transform.position = position;
                _createdObject.transform.parent = _parent.transform;
                _createdObject.transform.rotation = _tempPrefab.transform.rotation;
                _createdObject.transform.localScale = _tempPrefab.transform.localScale;
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

    private void RefreshCatalog(string path, List<GameObject> gameObjects)
    {
        gameObjects.Clear();

        System.IO.Directory.CreateDirectory(path);
        string[] prefabFiles = System.IO.Directory.GetFiles(path, "*.prefab");
        foreach (var prefabFile in prefabFiles)
        {
            gameObjects.Add(AssetDatabase.LoadAssetAtPath(prefabFile, typeof(GameObject)) as GameObject);
        }            
    }

}