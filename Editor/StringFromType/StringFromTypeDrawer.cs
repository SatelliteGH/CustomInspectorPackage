using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(StringFromTypeAttribute))]
    public class StringFromTypeDrawer : PropertyDrawer
    {
        private static readonly Dictionary<Type, List<Type>> Cache = new();


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            StringFromTypeAttribute attr = (StringFromTypeAttribute)attribute;
            Type baseType = attr.TargetType;

            if (!Cache.TryGetValue(baseType, out var typeList))
            {
                typeList = BuildTypeMap(baseType);
                Cache[baseType] = typeList;
            }


            EditorGUI.BeginProperty(position, label, property);

            string currentTypeName = property.stringValue;

            Rect buttonRect;
            if (string.IsNullOrEmpty(currentTypeName))
            {
                buttonRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            }
            else
            {
                Rect rect = new Rect(position.x, position.y, position.width * 0.8f, EditorGUIUtility.singleLineHeight);
                bool isHovered = rect.Contains(Event.current.mousePosition);


                EditorGUI.SelectableLabel(rect, isHovered ? currentTypeName : currentTypeName[(currentTypeName.LastIndexOf('.') + 1)..]);
                buttonRect = new Rect(position.x + position.width * 0.8f, position.y, position.width * 0.2f, EditorGUIUtility.singleLineHeight);
            }

            GUIContent typeContent = new GUIContent("Select");

            if (EditorGUI.DropdownButton(buttonRect, typeContent, FocusType.Keyboard))
            {
                UniversalPickerWindow<Type>.Show(
                                                 buttonRect,
                                                 typeList,
                                                 type =>
                                                 {
                                                     property.stringValue = type.FullName;
                                                     property.serializedObject.ApplyModifiedProperties();
                                                 },
                                                 t => t.Name,
                                                 t => t.Namespace,
                                                 t => t.FullName
                                                );
            }

            EditorGUI.EndProperty();
        }


        private List<Type> BuildTypeMap(Type baseType)
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
                            .Where(t => !t.IsAbstract && !t.IsGenericType && baseType.IsAssignableFrom(t))
                            .OrderBy(t => t.FullName)
                            .ToList();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}