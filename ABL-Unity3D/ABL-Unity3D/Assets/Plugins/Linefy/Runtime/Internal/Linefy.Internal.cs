using System.Collections.Generic;
using System;
using UnityEngine;


namespace Linefy {
    internal class HashedCollection<T> {
        List<T> items = new List<T>();
        Dictionary<T, int> dict = new Dictionary<T, int>();
        System.Action<T> collisionCallback;

        public HashedCollection(System.Action<T> collisionCallback) {
            this.collisionCallback = collisionCallback;

        }

        public T FindOrAdd(T item) {
            int idx = -1;
            if (dict.TryGetValue(item, out idx)) {
                T finded = items[idx];
                if (collisionCallback != null) {
                    collisionCallback(finded);
                }
                return items[idx];
            } else {
                dict.Add(item, items.Count);
                items.Add(item);
                return item;
            }
        }

        public int FindOrAddIdx(T item) {
            int idx = -1;
            if (dict.TryGetValue(item, out idx)) {
                T finded = items[idx];
                if (collisionCallback != null) {
                    collisionCallback(finded);
                }
                return idx;
            } else {
                dict.Add(item, items.Count);
                items.Add(item);
                return items.Count - 1;
            }

        }

        public int Count {
            get {
                return items.Count;
            }
        }

        public T this[int idx] {
            get {
                return items[idx];
            }

            set {
                items[idx] = value;
            }
        }

        public T[] ToArray() {
            return items.ToArray();
        }
    }


}


namespace Linefy.Internal {


    public class DFlag {
        public string name;
        bool _value;

        public DFlag(string name, bool initialValue) {
            this.name = name;
            _value = initialValue;
        }

        public void Set( ) {
            _value = true;
 
        }

        public void Reset() {
            _value = false;
        }

        public static implicit operator bool(DFlag df) {
            return df._value;
        }

        public override string ToString() {
            return name + (_value ? "[dirty]" : "[NOT dirty]");
        }
    }

    public class DValue<T> where T : struct, IEquatable<T> {
        public string name;
        protected T _value;
        protected DFlag[] dFlags;
 
        public void ForceSetDirty() {
            foreach (DFlag df in dFlags) {
                df.Set();
            }
        }

        public DValue(T initialValue, params DFlag[] dirtyFlags) {
            _value = initialValue;
            this.dFlags = dirtyFlags;
        }

        public void AssignDirtyFlags(DFlag[] dirtyFlags) {
            this.dFlags = dirtyFlags;
        }

        public void SetValue(T value) {
            if (!value.Equals(_value)) {
                foreach (DFlag df in dFlags) {
                    df.Set();
                }
                _value = value;
            }
        }

        public static implicit operator T(DValue<T> v) {
            return v._value;
        }
    }

    public class DIntValue : DValue<int> {

        public DIntValue(int initialValue, params DFlag[] dirtyFlags) : base(initialValue, dirtyFlags) { 
            
        }

        public static implicit operator int(DIntValue v) {
            return v._value;
        }
    }

    public class DFloatValue : DValue<float> {

        public DFloatValue(float initialValue, params DFlag[] dirtyFlags) : base(initialValue, dirtyFlags) {

        }

        public static implicit operator float(DFloatValue v) {
            return v._value;
        }
    }

    public class DBoolValue : DValue<bool> {

        public DBoolValue(bool initialValue, params DFlag[] dirtyFlags) : base(initialValue, dirtyFlags) {

        }

        public static implicit operator bool(DBoolValue v) {
            return v._value;
        }
    }

	# if UNITY_2019_1_OR_NEWER

    public class DVector3Value : DValue<Vector3> {

        public DVector3Value(Vector3 initialValue, params DFlag[] dirtyFlags) : base(initialValue, dirtyFlags) {

        }

        public static implicit operator Vector3(DVector3Value v) {
            return v._value;
        }
    }
	#else  
	public class DVector3Value   {
		Vector3 _value;
        protected DFlag[] dFlags;

        public DVector3Value(Vector3 initialValue, params DFlag[] dirtyFlags)   {
            _value = initialValue;
            this.dFlags = dirtyFlags;

        }

		public void SetValue(Vector3 v){
            if (v != _value) {
                foreach (DFlag df in dFlags) {
                    df.Set();
                }
                _value = v;
            }
		}

		public static implicit operator Vector3(DVector3Value v) {
			return v._value;
		}
	}
	#endif
    
  [System.Serializable]
    public struct DebugInfoString {
        [InfoString]
        public string text;

        public static implicit operator DebugInfoString( string s ) {
            DebugInfoString r;
            r.text = s;
            return r;
        }
    }
}

