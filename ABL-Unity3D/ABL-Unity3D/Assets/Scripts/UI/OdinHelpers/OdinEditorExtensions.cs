using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;

namespace UI.OdinHelpers
{
    public static class OdinEditorExtensions
    {
        public static bool TryGetValueInParents<T>(this InspectorProperty property, out T value)
        {
            while (true)
            {
                if (null == property.Parent)
                {
                    value = default;
                    return false;
                }

                if (property.Parent.TryGetValue(out value)) return true;

                property = property.Parent;
            }
        }

        public static bool TryGetValue<T>(this InspectorProperty property, out T value)
        {
            if (property.ValueEntry?.TypeOfValue == typeof(T))
            {
                value = (T)property.ValueEntry.WeakSmartValue;
                return true;
            }

            value = default;
            return false;
        }

        public static void AddValue<TOwner, TValue>(this IList<InspectorPropertyInfo> infos, string name,
            ValueGetter<TOwner, TValue> getter, params Attribute[] attributes)
        {
            infos.AddValue(name, getter, (ref TOwner instance, TValue value) => { }, attributes);
        }
    }
}