using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CustomInspector.Editor
{
    public class UniversalTreeView<T> : TreeView<int>
    {
        public bool ShowNamesOnly = false;
        private List<T> _all;
        private List<T> _filtered;
        private Dictionary<int, T> _idToItem;
        private T _selected;

        private Func<T, string> _getName;
        private Func<T, string> _getGroup;
        private Func<T, string> _getSearchString;
        public Action<T> OnDoubleClick;
        public Action<T> OnEnter;


        public UniversalTreeView(TreeViewState<int> state,
                                 Func<T, string> getName,
                                 Func<T, string> getGroup,
                                 Func<T, string> getSearchString) : base(state)
        {
            _getName = getName;
            _getGroup = getGroup;
            _getSearchString = getSearchString;

            showBorder = true;
            showAlternatingRowBackgrounds = true;
        }


        public void SetData(List<T> data, string filter)
        {
            _all = data;

            _filtered = string.IsNullOrWhiteSpace(filter)
                ? new List<T>(_all)
                : _all.Where(x =>
                                 _getSearchString(x).IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                      .ToList();
        }


        public T GetSelected() => _selected;


        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                var selected = GetSelected();
                if (selected != null)
                {
                    OnEnter?.Invoke(selected);
                }
            }
        }


        protected override TreeViewItem<int> BuildRoot()
        {
            TreeViewItem<int> root = new TreeViewItem<int>(0, -1);
            _idToItem = new Dictionary<int, T>();
            int idCounter = 1;
            root.children = new List<TreeViewItem<int>>();
            if (ShowNamesOnly)
            {
                foreach (var item in _filtered.OrderBy(_getName))
                {
                    int id = idCounter++;
                    root.children.Add(new TreeViewItem<int>(id, 0, _getName(item)));
                    _idToItem[id] = item;
                }
            }
            else
            {
                Dictionary<string, List<T>> map = new Dictionary<string, List<T>>();

                foreach (T item in _filtered)
                {
                    string group = _getGroup(item) ?? "Default";

                    if (!map.TryGetValue(group, out var list))
                    {
                        list = new List<T>();
                        map[group] = list;
                    }

                    list.Add(item);
                }


                foreach (var group in map.OrderBy(x => x.Key))
                {
                    var groupItem = new TreeViewItem<int>(idCounter++, 0, group.Key)
                    {
                        children = new List<TreeViewItem<int>>()
                    };

                    foreach (var item in group.Value.OrderBy(_getName))
                    {
                        int id = idCounter++;
                        groupItem.children.Add(new TreeViewItem<int>(id, 1, _getName(item)));
                        _idToItem[id] = item;
                    }

                    root.children.Add(groupItem);
                }
            }

            return root;
        }


        protected override void DoubleClickedItem(int id)
        {
            var selected = GetSelected();
            if (selected == null) return;

            OnDoubleClick?.Invoke(selected);
        }


        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds == null || selectedIds.Count == 0)
            {
                _selected = default;
                return;
            }

            _idToItem.TryGetValue(selectedIds[0], out _selected);
        }


        protected override bool CanMultiSelect(TreeViewItem<int> item) => false;
    }
}