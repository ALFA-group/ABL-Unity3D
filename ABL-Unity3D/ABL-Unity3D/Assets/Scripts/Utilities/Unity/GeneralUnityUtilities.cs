using UnityEngine;

namespace Utilities.Unity
{
    /// <summary>
    /// General utility functions for Unity.
    /// </summary>
    /// <remarks>
    /// If you have objects placed higher than 50 or lower than -50, you will need to modify
    /// <see cref="HEIGHT_OF_HIGHEST_OBJECT_IN_KM"/> and <see cref="HEIGHT_OF_LOWEST_OBJECT_IN_KM"/>.
    /// </remarks>
    public static class GeneralUnityUtilities
    {
        /// <summary>
        /// The height of the highest object in the scene. Units are kilometers.
        /// </summary>
        /// <remarks>
        /// This doesn't usually need to be touched. It's only necessary to be touched if you have objects
        /// placed high up.
        /// </remarks>
        private const float HEIGHT_OF_HIGHEST_OBJECT_IN_KM = 50;

        /// <summary>
        /// The height of the lowest object in the scene. Units are kilometers.
        /// </summary>
        /// <remarks>
        /// This doesn't usually need to be touched. It's only necessary to be touched if you have objects
        /// placed low down.
        /// </remarks>
        private const float HEIGHT_OF_LOWEST_OBJECT_IN_KM = -50;
        
        public static Vector3 SimToUnityCoords(
            Vector2 v, float extraHeightAboveTerrain, int layerMask)
        {
            var drawingPosition = v.ToUnityVector3();
            // Looking down from `HEIGHT_OF_HIGHEST_OBJECT_IN_KM` km up.
            var ray = new Ray(drawingPosition.WithY(HEIGHT_OF_HIGHEST_OBJECT_IN_KM), Vector3.down);

            if (Physics.Raycast(ray, out var hitInfo, HEIGHT_OF_HIGHEST_OBJECT_IN_KM - HEIGHT_OF_LOWEST_OBJECT_IN_KM, layerMask))
                drawingPosition.y = hitInfo.point.y + extraHeightAboveTerrain;
            else
                drawingPosition.y = extraHeightAboveTerrain;

            return drawingPosition;
        }
    }
}