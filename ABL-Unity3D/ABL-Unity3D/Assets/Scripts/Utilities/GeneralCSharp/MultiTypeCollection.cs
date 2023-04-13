using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Collections.Extensions;
using UnityEngine;

#nullable enable

namespace Utilities.GeneralCSharp
{
    public class MultiTypeCollection
    {
        private readonly MultiValueDictionary<Type, object> _dictionary = new MultiValueDictionary<Type, object>();

        public void Add<T>(T t)
        {
            if (null == t)
            {
                Debug.LogError("null not allowed in collection");
                return;
            }

            this._dictionary.Add(typeof(T), t);
        }

        public void Add(Type t, object o)
        {
            if (!t.IsInstanceOfType(o))
            {
                Debug.LogError(
                    $"Trying to add object with incorrect type!  Adding {o.GetType().Name} but expected {t.Name}");
                return;
            }

            this._dictionary.Add(t, o);
        }

        public void RemoveAllOfType(Type t)
        {
            this._dictionary.Remove(t);
        }

        public void AddDynamic(object o)
        {
            this.Add(o.GetType(), o);
        }

        public IEnumerable<T> Get<T>()
        {
            if (this._dictionary.TryGetValue(typeof(T), out var collection)) return collection.Cast<T>();

            return Enumerable.Empty<T>();
        }
    }
}