using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Linefy {
    /// <summary>
    /// Base class for all Linefy renderable classes - Lines, Dots, Polyline, PolygonalMesh, LabelsRenderer, primitives.  
    /// </summary>
    public abstract class Drawable {
        protected Matrix4x4 identityMatrix = Matrix4x4.identity;

        /// <summary>
        /// For internal use
        /// </summary>
        public Matrix4x4 onSceneGUIMatrix = Matrix4x4.identity;

        /// <summary>
        /// For internal use
        /// </summary>
        public Matrix4x4 _editorGUIMatrix = Matrix4x4.identity;

        /// <summary>
        /// Actually sends objects to render with Identity transformation matrix. Use inside Update() and LateUpdate() only. 
        /// </summary>
        public void Draw() {
            Draw(identityMatrix, null, 0);
        }

        /// <summary>
        /// Actually sends objects to render. Use inside Update() and LateUpdate() only.
        /// </summary>
        /// <param name="matrix">transformation matrix</param>
        public void Draw(Matrix4x4 matrix) {
            Draw(matrix, null, 0);
        }

        /// <summary>
        /// Actually sends objects to render. Use inside Update() and LateUpdate() only.
        /// </summary>
        /// <param name="layer">Layer to use.</param>
        public void Draw(int layer) {
            Draw(identityMatrix, null, layer);
        }

        /// <summary>
        /// Actually sends objects to render. Use inside Update() and LateUpdate() only.
        /// </summary>
        /// <param name="matrix">transformation matrix</param>
        /// <param name="layer">Layer to use.</param>
        public void Draw(Matrix4x4 matrix, int layer) {
            Draw(matrix, null, layer);
        }

        /// <summary>
        /// Actually sends objects to render. Use inside Update() and LateUpdate() only.
        /// </summary>
        /// <param name="matrix">transformation matrix</param>
        /// <param name="camera"> If null (default), the mesh will be drawn in all cameras. Otherwise it will be rendered in the given camera only. </param>
        public void Draw(Matrix4x4 matrix, Camera camera) {
            Draw(matrix, camera, 0);
        }

        /// <summary>
        /// Actually sends objects to render. Use inside Update() and LateUpdate() only.
        /// </summary>
        /// <param name="matrix">transformation matrix</param>
        /// <param name="camera"> If null (default), the mesh will be drawn in all cameras. Otherwise it will be rendered in the given camera only. </param>
        /// <param name="layer">Layer to use.</param>
        public abstract void Draw(Matrix4x4 matrix, Camera camera, int layer);

        public abstract void DrawNow(Matrix4x4 matrix);

        protected bool _disposed;

        /// <summary>
        /// Returns true when the LinefyEntity is disposed. You should not call methods on a disposed LinefyEntity.
        /// </summary>
        public bool disposed {
            get {
                return _disposed;
            }
        }

        /// <summary>
        /// Destroys all internal unmanaged Objects. 
        /// </summary>
        public abstract void Dispose();

        public abstract void GetStatistic(ref int linesCount, ref int totallinesCount, ref int dotsCount, ref int totalDotsCount, ref int polylinesCount, ref int totalPolylineVerticesCount);

    }
}
