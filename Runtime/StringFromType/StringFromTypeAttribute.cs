using System;
using UnityEngine;

namespace CustomInspector
{
    public class StringFromTypeAttribute : PropertyAttribute
    {
        public readonly Type TargetType;
        public readonly bool CollectionItem;


        public StringFromTypeAttribute(Type targetType, bool collectionItem)
        {
            TargetType = targetType;
            CollectionItem = collectionItem;
        }
    }
}