using UnityEditor;
using UnityEngine;

namespace CustomInspector
{
    [CustomPropertyDrawer(typeof(ScriptableObject), true)]
    public class FoldoutSODrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float vspace = EditorGUIUtility.standardVerticalSpacing;

            Rect headerRect = new Rect(position.x, position.y, position.width, line);
            Rect labelRect = EditorGUI.PrefixLabel(headerRect, label);


            Rect foldoutRect = new Rect(labelRect.x, labelRect.y, 0, line);
            Rect objectRect = new Rect(labelRect.x + 0, labelRect.y, labelRect.width, line);

            if (property.objectReferenceValue != null)
            {
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none);
            }

            Type objType;

            if (fieldInfo.FieldType.IsArray)
            {
                objType = fieldInfo.FieldType.GetElementType();
            }
            else if (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                objType = fieldInfo.FieldType.GetGenericArguments()[0];
            }
            else
            {
                objType = fieldInfo.FieldType;
            }

            property.objectReferenceValue = EditorGUI.ObjectField(objectRect, property.objectReferenceValue, objType, false);

            if (property.objectReferenceValue == null || !property.isExpanded)
                return;

            float indent = (EditorGUI.indentLevel) * 15f;
            float boxY = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            float boxH = GetPropertyHeight(property, label) - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing ;

            Rect boxRect = new Rect(position.x + indent, boxY, position.width - indent, boxH);

            EditorGUI.DrawRect(boxRect, new Color(0.13f, 0.1f, 0.15f, 0.25f));

            // Draw SO content section
            SerializedObject so = new SerializedObject(property.objectReferenceValue);
            so.UpdateIfRequiredOrScript();

            float y = headerRect.y + line + vspace;

            int prevIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = prevIndent + 1;

            // Draw each visible property
            SerializedProperty iterator = so.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                if (iterator.name == "m_Script")
                {
                    enterChildren = false;
                    continue;
                }

                float propertyHeight = EditorGUI.GetPropertyHeight(iterator, true);
                Rect rect = new Rect(position.x, y, position.width, propertyHeight);

                EditorGUI.PropertyField(rect, iterator, true);

                y += propertyHeight + vspace;
                enterChildren = false;
            }

            so.ApplyModifiedProperties();
            EditorGUI.indentLevel = prevIndent;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float vspace = EditorGUIUtility.standardVerticalSpacing;

            float height = line;

            if (property.objectReferenceValue != null && property.isExpanded)
            {
                SerializedObject so = new SerializedObject(property.objectReferenceValue);
                so.UpdateIfRequiredOrScript();

                float inner = 0f;
                SerializedProperty it = so.GetIterator();
                bool enterChildren = true;

                while (it.NextVisible(enterChildren))
                {
                    if (it.name == "m_Script")
                    {
                        enterChildren = false;
                        continue;
                    }

                    inner += EditorGUI.GetPropertyHeight(it, true) + vspace;
                    enterChildren = false;
                }

                if (inner > 0f)
                    inner -= vspace;

                height += vspace + inner;
            }

            float extraPadding = 2f;

            return height + extraPadding;
        }
    }
}