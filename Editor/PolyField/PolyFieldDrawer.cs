using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CustomInspector
{
    [CustomPropertyDrawer(typeof(PolyFieldAttribute))]
    public class PolyFieldDrawer : PropertyDrawer
    {
        private static readonly Dictionary<Type, Dictionary<string, Type>> Cache = new();
        private bool _isEditorWindow = false;
        private static float _menuPadding = 18f;
        private static float _buttonPadding = 15f;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PolyFieldAttribute attr = (PolyFieldAttribute)attribute;
            Type baseType = attr.BaseType;

            if (!Cache.TryGetValue(baseType, out var typeMap))
            {
                typeMap = BuildTypeMap(baseType);
                Cache[baseType] = typeMap;
            }

            if (EditorWindow.mouseOverWindow != null && EditorWindow.mouseOverWindow.GetType().Name == "InspectorWindow")
            {
                _isEditorWindow = false;
            }
            else if (EditorWindow.mouseOverWindow != null && typeof(EditorWindow).IsAssignableFrom(EditorWindow.mouseOverWindow.GetType()))
            {
                _isEditorWindow = true;
            }

            Rect main = position;

            EditorGUI.BeginProperty(position, label, property);


            if (!attr.CollectionItem)
            {
                Rect labelRect = new Rect(main.x, main.y, EditorGUIUtility.labelWidth, main.height);
                main = new Rect(labelRect.x + EditorGUIUtility.labelWidth, main.y,
                                main.width - EditorGUIUtility.labelWidth, main.height);

                EditorGUI.LabelField(labelRect, label);
            }


            string currentTypeName = property.managedReferenceFullTypename;
            string displayName = GetShortTypeName(currentTypeName) ?? "Select Type";

            Rect buttonRect = new Rect(main.x + _buttonPadding, main.y, main.width - _menuPadding - _buttonPadding, EditorGUIUtility.singleLineHeight);


            GUIContent typeContent = new($"{displayName}");

            if (EditorGUI.DropdownButton(buttonRect, typeContent, FocusType.Keyboard))
            {
                GenericMenu menu = new GenericMenu();

                foreach ((string name, Type type) in typeMap)
                {
                    menu.AddItem(new GUIContent(name),
                                 type.FullName == currentTypeName,
                                 () =>
                                 {
                                     property.managedReferenceValue = Activator.CreateInstance(type);
                                     property.serializedObject.ApplyModifiedProperties();
                                 });
                }

                menu.ShowAsContext();
            }

            float inspectorPadding = _isEditorWindow ? 0 : 15f;
            Rect foldoutRect = new Rect(main.x + inspectorPadding, main.y, 14, EditorGUIUtility.singleLineHeight);

            if (property.managedReferenceValue != null)
            {
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none, true);
            }

            if (property.isExpanded && property.managedReferenceValue != null)
            {
                EditorGUI.indentLevel++;

                SerializedProperty child = property.Copy();
                SerializedProperty end = child.GetEndProperty();

                float y = main.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                bool enterChildren = true;

                while (child.NextVisible(enterChildren) && !SerializedProperty.EqualContents(child, end))
                {
                    float height = EditorGUI.GetPropertyHeight(child, true);
                    Rect childRect = new Rect(main.x, y, main.width, height);

                    EditorGUI.PropertyField(childRect, child, true);            

                    y += height + EditorGUIUtility.standardVerticalSpacing;
                    enterChildren = false;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0;
            if (property.managedReferenceValue != null)
            {
                height += EditorGUI.GetPropertyHeight(property, label, true);
            }
            else
            {
                height = EditorGUIUtility.singleLineHeight;
            }

            return height;
        }


        static Dictionary<string, Type> BuildTypeMap(Type baseType)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(a =>
                             {
                                 try
                                 {
                                     return a.GetTypes();
                                 }
                                 catch
                                 {
                                     return Type.EmptyTypes;
                                 }
                             })
                            .Where(t =>
                                       !t.IsAbstract &&
                                       !t.IsGenericType &&
                                       baseType.IsAssignableFrom(t))
                            .ToDictionary(
                                          t => ObjectNames.NicifyVariableName(t.Name),
                                          t => t
                                         );
        }


        private static string GetShortTypeName(string fullTypeName)
        {
            if (string.IsNullOrEmpty(fullTypeName))
            {
                return null;
            }

            string[] parts = fullTypeName.Split(' ');
            return parts.Length > 1
                ? parts[1].Split('.').Last()
                : fullTypeName;
        }
    }
}