using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;

namespace CustomInspector.Editor
{
    internal class ScriptableObjectTypeTreeView : TreeView<int>
    {
        private List<Type> _all;
        private List<Type> _filtered;
        private Type _selected;
        private Dictionary<int, Type> _idToType;


        public ScriptableObjectTypeTreeView(TreeViewState<int> state) : base(state)
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;
        }


        public void SetData(List<Type> all, string filter)
        {
            _all = all;
            _filtered = string.IsNullOrWhiteSpace(filter)
                ? new List<Type>(all)
                : all.Where(t => t.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                 (t.FullName != null && t.FullName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0))
                     .ToList();
        }


        public Type GetSelected() => _selected;


        protected override TreeViewItem<int> BuildRoot()
        {
            TreeViewItem<int> root = new TreeViewItem<int>(0, -1);
            Dictionary<string, List<Type>> map = new Dictionary<string, List<Type>>();
            _idToType = new Dictionary<int, Type>();
            int idCounter = 1;

            foreach (Type t in _filtered)
            {
                string ns = t.Namespace ?? "Global";
                if (!map.TryGetValue(ns, out List<Type> list)) map[ns] = list = new List<Type>();
                list.Add(t);
            }

            root.children = new List<TreeViewItem<int>>();

            foreach (KeyValuePair<string, List<Type>> kv in map.OrderBy(x => x.Key))
            {
                TreeViewItem<int> nsItem = new TreeViewItem<int>(idCounter++, 0, kv.Key) { children = new List<TreeViewItem<int>>() };

                foreach (Type t in kv.Value.OrderBy(x => x.Name))
                {
                    int typeId = idCounter++;
                    nsItem.children.Add(new TreeViewItem<int>(typeId, 1, t.Name));
                    _idToType[typeId] = t;
                }

                root.children.Add(nsItem);
            }

            return root;
        }


        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds == null || selectedIds.Count == 0)
            {
                _selected = null;
                return;
            }

            int id = selectedIds[0];
            _idToType.TryGetValue(id, out _selected);
        }


        protected override bool CanMultiSelect(TreeViewItem<int> item) => false;
    }
}