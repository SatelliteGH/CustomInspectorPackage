using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CustomInspector.Editor
{
    public class UniversalPickerWindow<T> : PopupWindowContent
    {
        private bool _showNamesOnly = false;
        private UniversalTreeView<T> _tree;
        private TreeViewState<int> _state;

        private List<T> _items;
        private Action<T> _onSelect;

        private Func<T, string> _getName;
        private Func<T, string> _getGroup;
        private Func<T, string> _getSearch;

        private string _search = "";


        public UniversalPickerWindow(List<T> items,
                                     Action<T> onSelect,
                                     Func<T, string> getName,
                                     Func<T, string> getGroup,
                                     Func<T, string> getSearch,
                                     bool showNamesOnly = false)
        {
            _showNamesOnly = showNamesOnly;
            _items = items;
            _onSelect = onSelect;
            _getName = getName;
            _getGroup = getGroup;
            _getSearch = getSearch;
            _state = new TreeViewState<int>();

            _tree = new UniversalTreeView<T>(_state,
                                             _getName,
                                             _getGroup,
                                             _getSearch
                                            );

            _tree.ShowNamesOnly = _showNamesOnly;
            _tree.OnEnter += SelectAndClose;
            _tree.OnDoubleClick += SelectAndClose;
            _tree.SetData(_items, "");
            _tree.Reload();
        }


        public static void Show(Rect buttonRect,
                                List<T> items,
                                Action<T> onSelect,
                                Func<T, string> getName,
                                Func<T, string> getGroup,
                                Func<T, string> getSearch,
                                bool showNamesOnly = false)
        {
            PopupWindow.Show(buttonRect, new UniversalPickerWindow<T>(items, onSelect, getName, getGroup, getSearch, showNamesOnly));
        }


        public override Vector2 GetWindowSize() => new Vector2(500, 400);


        public override void OnGUI(Rect rect)
        {
            DrawToolbar(rect);
            Rect treeRect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandHeight(true));

            _tree.OnGUI(treeRect);
        }


        private void DrawToolbar(Rect rect)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                string newSearch = EditorGUILayout.TextField(_search, GUILayout.Width(rect.width - 128));
                if (newSearch != _search)
                {
                    _search = newSearch;
                    _tree.SetData(_items, _search);
                    _tree.Reload();
                }

                bool newToggle = EditorGUILayout.ToggleLeft("Names Only", _showNamesOnly, GUILayout.Width(128));
                if (newToggle != _showNamesOnly)
                {
                    _showNamesOnly = newToggle;
                    _tree.ShowNamesOnly = _showNamesOnly;
                    _tree.Reload();
                }
            }
        }


        private void SelectAndClose(T selected)
        {
            _onSelect?.Invoke(selected);
            _tree.OnEnter -= SelectAndClose;
            _tree.OnDoubleClick -= SelectAndClose;
            editorWindow.Close();
        }
    }
}