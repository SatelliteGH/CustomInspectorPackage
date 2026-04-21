using System;
using UnityEngine;

namespace CustomInspector
{
    public class PolyFieldAttribute : PropertyAttribute
    {
        public readonly Type BaseType;
        public readonly bool CollectionItem;
        public readonly bool Readonly;

        public PolyFieldAttribute(Type baseType, bool collectionItem, bool makeRadonly = false)
        {
            BaseType = baseType;
            CollectionItem = collectionItem;
            Readonly = makeRadonly;
        }
    }
}