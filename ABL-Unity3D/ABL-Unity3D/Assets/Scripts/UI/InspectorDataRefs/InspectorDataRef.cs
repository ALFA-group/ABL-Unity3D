using System;
using UnityEngine;

#nullable enable

namespace UI.InspectorDataRefs
{
    public class InspectorDataRef<T> : MonoBehaviour where T : class
    {
        public T? data;

        public static T? Fetch(InspectorDataRef<T>? refHolder)
        {
            if (null != refHolder && null != refHolder.data) return refHolder.data;

            return null;
        }

        public static void Set(InspectorDataRef<T>? refHolder, T newData)
        {
            if (null != refHolder)
                refHolder.data = newData;
            else
                throw new Exception($"No holder found for {typeof(T).Name}");
        }
    }
}