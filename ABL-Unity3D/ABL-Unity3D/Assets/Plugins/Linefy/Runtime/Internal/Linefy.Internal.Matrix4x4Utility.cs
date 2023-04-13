using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Linefy.Internal{

    public static class Matrix4x4Utility  {
        
        public static Matrix4x4 Interpolate(Matrix4x4 a, Matrix4x4 b, float t) {
            Vector3 up = Vector3.LerpUnclamped(a.GetColumn(1), b.GetColumn(1), t);
            Vector3 fwd = Vector3.LerpUnclamped(a.GetColumn(2), b.GetColumn(2), t);
            Vector3 pos = Vector3.LerpUnclamped(a.GetColumn(3), b.GetColumn(3), t);
            return Matrix4x4.TRS(pos, Quaternion.LookRotation(fwd, up), Vector3.one);
        }


        /// <summary>
        /// vs Matrix4x4.inverse
        ///Fast alternative for Matrix.4x4 inverse for orthonormalized unscaled matrices.
        ///Standalone x2 faster
        ///Webgl x6 faster
        /// </summary>
        /// <param name="tm"></param>
        /// <returns></returns>
        public static Matrix4x4 OrthonormalUnscaledInverse (this Matrix4x4 tm) {
            Matrix4x4 r = tm;
            r.m01 = tm.m10;
            r.m02 = tm.m20;
            r.m12 = tm.m21;

            r.m10 = tm.m01;
            r.m20 = tm.m02;
            r.m21 = tm.m12;

            float px = -tm.m03;
            float py = -tm.m13;
            float pz = -tm.m23;
            float rpx = r.m00 * px + r.m01 * py + r.m02 * pz;
            float rpy = r.m10 * px + r.m11 * py + r.m12 * pz;
            float rpz = r.m20 * px + r.m21 * py + r.m22 * pz;
            r.m03 = rpx;
            r.m13 = rpy;
            r.m23 = rpz;
            return r;
        }

        /// <summary>
        /// vs Matrix4x4.TRS(...).inverse
        /// Editor Win  178:78 = x2.2 
        /// Build Win   32:16 = x2
        /// WebGL 34:12 = x2.8
        /// </summary>
        /// <param name="position"></param>
        /// <param name="forward"></param>
        /// <param name="upward"></param>
        /// <returns></returns>
        public static Matrix4x4 UnscaledTRSInverse(Vector3 position, Vector3 forward, Vector3 upward) {
            Vector3 right = Vector3.Cross(upward, forward);
            Vector3.OrthoNormalize(ref forward, ref upward, ref right);
            Matrix4x4 r = new Matrix4x4();
            r.m00 = right.x;
            r.m10 = upward.x;
            r.m20 = forward.x;

            r.m01 = right.y;
            r.m11 = upward.y;
            r.m21 = forward.y;

            r.m02 = right.z;
            r.m12 = upward.z;
            r.m22 = forward.z;

            float px = -position.x;
            float py = -position.y;
            float pz = -position.z;
            float rpx = r.m00 * px + r.m01 * py + r.m02 * pz;
            float rpy = r.m10 * px + r.m11 * py + r.m12 * pz;
            float rpz = r.m20 * px + r.m21 * py + r.m22 * pz;
            r.m03 = rpx;
            r.m13 = rpy;
            r.m23 = rpz;
            r.m33 = 1;
            return r;
        }

        /// <summary>
        /// vs Matrix4x4.TRS(...)
        /// Editor Win x1
        /// Build Win x1.4  
        /// WebGL x2 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="forward"></param>
        /// <param name="upward"></param>
        /// <returns></returns>
        public static Matrix4x4 UnscaledTRS(Vector3 position, Vector3 forward, Vector3 upward) {
            Vector3 right = Vector3.Cross(  upward, forward);
            Vector3.OrthoNormalize(ref forward, ref upward, ref right);
            Matrix4x4 r = new Matrix4x4();
            r.m00 = right.x;
            r.m10 = right.y;
            r.m20 = right.z;

            r.m01 = upward.x;
            r.m11 = upward.y;
            r.m21 = upward.z;

            r.m02 = forward.x;
            r.m12 = forward.y;
            r.m22 = forward.z;

            r.m03 = position.x;
            r.m13 = position.y;
            r.m23 = position.z;
            r.m33 = 1;
            return r;
        }

        public static void Normalize(ref Matrix4x4 tm) {
            tm.SetColumn(0, ((Vector3)tm.GetColumn(0)).normalized);
            tm.SetColumn(1, ((Vector3)tm.GetColumn(1)).normalized);
            tm.SetColumn(2, ((Vector3)tm.GetColumn(2)).normalized);
        }

        public static Matrix4x4 ToUnscaled(this Matrix4x4 tm) {
            return UnscaledTRS(tm.GetPosition(), tm.GetColumn(2), tm.GetColumn(1));
        }

        public static Matrix4x4 SetRotation(this Matrix4x4 tm, Quaternion rot) {
            return Matrix4x4.TRS(tm.GetColumn(3), rot, Vector3.one);
        }

        public static Matrix4x4 SetPosition(this Matrix4x4 tm, Vector3 position) {
            return Matrix4x4.TRS(position, tm.GetRotation(), Vector3.one);
        }

        public static Quaternion GetRotation(this Matrix4x4 tm) {
            Vector3 zDir = tm.GetColumn(2);
            Vector3 yDir = tm.GetColumn(1);
            return Quaternion.LookRotation(zDir, yDir);
        }

        public static Vector3 GetPosition(this Matrix4x4 tm) {
            return tm.GetColumn(3);
        }

        /// 0,0----w,0
        ///  |      |   w = Screen.Width
        ///  |      |   h = Screen.Height 
        /// 0,h----w,h     
        /// <summary>
        /// Returns the transformation matrix located on the near clipping plane of camera, scaled to the camera viewing area in GUI space. .
        /// </summary>
        /// <param name="offset">Offset along camera`s Z axis from its near clip plane.</param>
        /// <returns></returns>
        public static Matrix4x4 NearClipPlaneGUISpaceMatrix(Camera cam, float offset) {
            Ray rBlue = cam.ViewportPointToRay(new Vector3(0, 1, 0));
            Ray rRed = cam.ViewportPointToRay(new Vector3(1, 1, 0));
            Ray rGreen = cam.ViewportPointToRay(new Vector3(0, 0, 0));
            Vector3 pC = rBlue.GetPoint(offset);
            Vector3 pRed = rRed.GetPoint(offset);
            Vector3 pGreen = rGreen.GetPoint(offset);
            Rect camPixelRect = cam.pixelRect;
            float numRed = Screen.width / camPixelRect.width;
            float numGreen = Screen.height / camPixelRect.height;
            Vector3 axisRed = (pRed - pC) / Screen.width * numRed;
            Vector3 greenAxis = (pGreen - pC) / Screen.height * numGreen;
            Vector4 column3 = pC - axisRed * camPixelRect.x - greenAxis * camPixelRect.y;
            column3.w = 1;
            return new Matrix4x4(axisRed, greenAxis, rBlue.direction, column3);
        }


        /// 0,h----w,h
        ///  |      |   w = Screen.Width 
        ///  |      |   h = Screen.Height 
        /// 0,0----w,0 
        /// <summary>
        /// Returns the transformation matrix located on the near clipping plane of camera, scaled to the camera viewing area in ScreenSpace.  
        /// </summary>
        /// <param name="offset">Offset along camera`s Z axis from its near clip plane.</param>
        /// <returns></returns>
        public static Matrix4x4 NearClipPlaneScreenSpaceMatrix(Camera camera, float offset) {
            Ray rBlue = camera.ViewportPointToRay(new Vector3(0, 0, 0));
            Ray rRed = camera.ViewportPointToRay(new Vector3(1, 0, 0));
            Ray rGreen = camera.ViewportPointToRay(new Vector3(0, 1, 0));
            Vector3 pC = rBlue.GetPoint(offset);
            Vector3 pRed = rRed.GetPoint(offset);
            Vector3 pGreen = rGreen.GetPoint(offset);
            Rect camPixelRect = camera.pixelRect;
            float numRed = Screen.width / camPixelRect.width;
            float numGreen = Screen.height / camPixelRect.height;
            Vector3 axisRed = (pRed - pC) / Screen.width * numRed;
            Vector3 greenAxis = (pGreen - pC) / Screen.height * numGreen;
            Vector4 column3 = pC - axisRed * camPixelRect.x - greenAxis * camPixelRect.y;
            column3.w = 1;
            return new Matrix4x4(axisRed, greenAxis, rBlue.direction, column3);
        }


        /// 0,1----1,1
        ///  |      |    
        ///  |      |
        /// 0,0----1,0 
        /// <summary>
        /// Returns the transformation matrix located on the far clipping plane of camera.  
        /// </summary>
         /// <returns></returns>
        public static Matrix4x4 FarClipPlaneViewportMatrix(Camera camera ) {
            Ray rBlue = camera.ViewportPointToRay(new Vector3(0, 0, 0));
            Ray rRed = camera.ViewportPointToRay(new Vector3(1, 0, 0));
            Ray rGreen = camera.ViewportPointToRay(new Vector3(0, 1, 0));
            Plane far = new Plane(camera.transform.forward, camera.transform.TransformPoint(0, 0, camera.farClipPlane));

            Vector3 pC = Vector3.zero;
            PlaneExtension.RaycastDoublesided(far, rBlue, ref pC);

            Vector3 pRed = Vector3.zero;
            PlaneExtension.RaycastDoublesided(far, rRed, ref pRed);

            Vector3 pGreen = Vector3.zero;
            PlaneExtension.RaycastDoublesided(far, rGreen, ref pGreen);
 
            Vector3 axisRed = (pRed - pC) ;
            Vector3 greenAxis = (pGreen - pC)  ;
            Vector4 column3 = pC;
            column3.w = 1;
            return new Matrix4x4(axisRed, greenAxis, rBlue.direction, column3);
        }

        public static string GetInfo(this Matrix4x4 tm) {
            Vector3 pos = tm.GetPosition();
            Vector3 euler = tm.GetRotation().eulerAngles;
            return string.Format("pos:{0} rot:{1}", pos.ToString("F3"), euler.ToString("F0"));
        }
    }
}
