using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Linefy {
    /// <summary>
    /// An helper component that calculate Matrix4x4 from rect transform.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [HelpURL("https://polyflow.xyz/content/linefy/documentation-1-1/linefy-documentation.html#LinefyRectTransform")]
    public class LinefyRectTransform : MonoBehaviour {

        bool _centeredRectTransformMatrix;
        /// <summary>
        /// If true, the matrix position located at minus half of the transform size, otherwise at 0.0
        /// </summary>
        public bool centeredRectTransformMatrix;
 
        RectTransform _rt;
 
        Matrix4x4 _rtMatrix = Matrix4x4.identity;

        /// <summary>
        /// an worldspace matrix based on RectTransform bounds
        /// </summary>
        public Matrix4x4 rectTransformWorldMatrix {
            get {
                if (_rt == null) {
                    _rt = GetComponent<RectTransform>();
                }

                if (_rt.hasChanged ) {
                     _rtMatrix  = centeredRectTransformMatrix?  _rt.GetCenteredWorldMatrix() : _rt.GetWorldMatrix();
                    _rt.hasChanged = false;
                }

                if (centeredRectTransformMatrix != _centeredRectTransformMatrix) {
                    _centeredRectTransformMatrix = centeredRectTransformMatrix;
                    _rtMatrix = centeredRectTransformMatrix ? _rt.GetCenteredWorldMatrix() : _rt.GetWorldMatrix();
                }
                return _rtMatrix;
            }
        }
    }


}
