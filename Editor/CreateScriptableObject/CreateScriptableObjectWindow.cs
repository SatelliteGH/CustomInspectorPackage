using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomInspector.Editor
{
    public class CreateScriptableObjectWindow : EditorWindow
    {
        private const string MENU_PATH = "Assets/Create Scriptable Object";
        private const int MENU_PRIORITY = -9999;

        private static List<Type> _cachedTypes;
        private bool _filterProjectOnly = true;
        private static bool _showLogs = false;

        private string _targetFolderPath;
        private List<Type> _types;
        private Type _selectedType;
        private string _searchFilter = "";

        private TreeViewState<int> _treeViewState;
        private ScriptableObjectTypeTreeView _treeView;

        private string _lastSearch;
        private int _lastCount;


        [MenuItem(MENU_PATH, false, MENU_PRIORITY)]
        private static void OpenFromMenu()
        {
            Open(GetSelectedFolder());
        }


        private static void Open(string folder)
        {
            CreateScriptableObjectWindow window = GetWindow<CreateScriptableObjectWindow>(true, "Create Scriptable Object", true);
            window._targetFolderPath = folder ?? "Assets";
            window.minSize = new Vector2(320, 400);
            window.RefreshTypes();
            window.EnsureTree();
        }


        private static string GetSelectedFolder()
        {
            Object[] selected = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
            if (selected == null || selected.Length == 0)
                return "Assets";

            foreach (Object obj in selected)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path)) continue;

                if (AssetDatabase.IsValidFolder(path))
                    return path;

                string dir = Path.GetDirectoryName(path)?.Replace("\\", "/");
                if (!string.IsNullOrEmpty(dir))
                    return dir;
            }

            return "Assets";
        }


        private void EnsureTree()
        {
            if (_treeViewState == null)
            {
                _treeViewState = new TreeViewState<int>();
            }

            if (_treeView == null)
            {
                _treeView = new ScriptableObjectTypeTreeView(_treeViewState);
            }
        }


        private void RefreshTypes()
        {
            if (_cachedTypes == null)
                BuildCache();

            _types = _filterProjectOnly ? CollectProjectScriptableObjects() : new List<Type>(_cachedTypes);

            if (_types.Count > 0 && _selectedType == null)
                _selectedType = _types[0];
        }


        private static void BuildCache()
        {
            _cachedTypes = TypeCache.GetTypesDerivedFrom<ScriptableObject>()
                                    .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition)
                                    .OrderBy(t => t.FullName)
                                    .ToList();
        }


        private static List<Type> CollectProjectScriptableObjects()
        {
            string projectPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> result = new List<Type>();

            foreach (Assembly asm in assemblies)
            {
                if (asm.IsDynamic) continue;

                string loc;
                try
                {
                    loc = asm.Location;
                }
                catch (NotSupportedException)
                {
                    continue;
                }

                if (!loc.StartsWith(projectPath, StringComparison.OrdinalIgnoreCase)) continue;
                string name = asm.GetName().Name;
                if (name.StartsWith("Unity") || name.StartsWith("System")) continue;

                try
                {
                    foreach (Type t in asm.GetExportedTypes())
                    {
                        if (t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition
                         && typeof(ScriptableObject).IsAssignableFrom(t))
                        {
                            result.Add(t);
                            if (_showLogs) Debug.Log($"{t.Name} -> {asm.FullName}");
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                }
            }

            return result.OrderBy(t => t.FullName).ToList();
        }


        private void OnGUI()
        {
            EditorGUILayout.LabelField("Folder: " + _targetFolderPath, EditorStyles.miniLabel);

            string newSearch = EditorGUILayout.TextField("Search", _searchFilter);
            if (newSearch != _searchFilter)
            {
                _searchFilter = newSearch;
            }

            bool newFilter = EditorGUILayout.Toggle("Project only", _filterProjectOnly);
            if (newFilter != _filterProjectOnly)
            {
                _filterProjectOnly = newFilter;
                RefreshTypes();
            }

            _showLogs = EditorGUILayout.Toggle("Logs", _showLogs);

            EnsureTree();

            bool changed = _searchFilter != _lastSearch || _types.Count != _lastCount;
            if (changed)
            {
                _lastSearch = _searchFilter;
                _lastCount = _types.Count;
                _treeView.SetData(_types, _searchFilter);
                _treeView.Reload();
            }

            Rect rect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandHeight(true));
            _treeView.OnGUI(rect);

            _selectedType = _treeView.GetSelected();

            GUI.enabled = _selectedType != null;
            if (GUILayout.Button("Create", GUILayout.Height(28)))
                Create();
            GUI.enabled = true;

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                if (_selectedType != null)
                    Create();
            }
        }


        private void Create()
        {
            try
            {
                ScriptableObject instance = CreateInstance(_selectedType);
                if (instance == null) return;

                string path = AssetDatabase.GenerateUniqueAssetPath($"{_targetFolderPath}/{_selectedType.Name}.asset");

                AssetDatabase.CreateAsset(instance, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Selection.activeObject = instance;
                EditorGUIUtility.PingObject(instance);

                Close();
            }
            catch
            {
                EditorUtility.DisplayDialog("Error", "Failed to create instance", "OK");
            }
        }
    }
}