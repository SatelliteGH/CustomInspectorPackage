using System;
using UnityEngine;

namespace CustomInspector
{
    public class PolyFieldAttribute : PropertyAttribute
    {
        public readonly Type BaseType;
        public readonly bool CollectionItem;


        public PolyFieldAttribute(Type baseType, bool collectionItem)
        {
            BaseType = baseType;
            CollectionItem = collectionItem;
        }
    }
}