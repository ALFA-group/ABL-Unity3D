using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Linefy {
    public class DrawableComponent : MonoBehaviour {

        [HideInInspector]
        public Matrix4x4 cachedWorldMatrix;

        public Matrix4x4 worldMatrix {
            get {
                if (transform.GetType() == typeof(RectTransform)) {
                    return ((RectTransform)transform).GetCenteredWorldMatrix();
                } else {
                    return transform.localToWorldMatrix;
                }
            }
        }

        public virtual Drawable drawable {
            get;
        }

        protected virtual void PreDraw() { }

        private void LateUpdate() {
            PreDraw();
            cachedWorldMatrix = worldMatrix;
            drawable.Draw(cachedWorldMatrix, null, gameObject.layer);
        }

        private void OnDestroy() {
            if (drawable != null) {
                drawable.Dispose();
            }
        }
    }
}
