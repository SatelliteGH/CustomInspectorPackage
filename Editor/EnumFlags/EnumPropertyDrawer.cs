using System;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(Enum), true)]
    public sealed class EnumPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                if (HasEnumFlagsAttribute())
                {
                    int intValue = EditorGUI.MaskField(position, label, property.intValue, property.enumDisplayNames);

                    if (property.intValue != intValue)
                    {
                        property.intValue = intValue;
                    }
                }
                else
                {
                    EditorGUI.PropertyField(position, property, label);
                }
            }

            return;

            bool HasEnumFlagsAttribute()
            {
                Type fieldType = fieldInfo.FieldType;

                if (!fieldType.IsArray) return fieldType.IsDefined(typeof(FlagsAttribute), false);

                Type elementType = fieldType.GetElementType();

                return elementType != null && elementType.IsDefined(typeof(FlagsAttribute), false);
            }
        }
    }
}