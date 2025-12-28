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


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PolyFieldAttribute attr = (PolyFieldAttribute)attribute;
            Type baseType = attr.BaseType;

            if (!Cache.TryGetValue(baseType, out var typeMap))
            {
                typeMap = BuildTypeMap(baseType);
                Cache[baseType] = typeMap;
            }

            EditorGUI.BeginProperty(position, label, property);

            Rect labelRect;
            if (attr.CollectionItem)
            {
                labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            }
            else
            {
                Rect lineRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                labelRect = new Rect(lineRect.x + EditorGUIUtility.labelWidth, lineRect.y,
                                     lineRect.width - EditorGUIUtility.labelWidth, lineRect.height);

                EditorGUI.LabelField(lineRect, label);
            }


            string currentTypeName = property.managedReferenceFullTypename;
            string displayName = GetShortTypeName(currentTypeName) ?? "Select Type";


            GUIContent typeContent = new($"{displayName}");

            if (EditorGUI.DropdownButton(labelRect, typeContent, FocusType.Keyboard))
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

            if (property.managedReferenceValue != null)
            {
                Rect bodyRect = new(position.x, position.y, position.width, position.height);


                EditorGUI.PropertyField(bodyRect, property, GUIContent.none, true);
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