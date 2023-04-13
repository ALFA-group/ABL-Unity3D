using ABLUnitySimulation.Actions.Helpers;
using UnityEngine;
using Utilities.Unity;

namespace UI.ABLUnitySimulation
{
    public static class Extensions
    {
        public static Circle ToCircle(this CapsuleCollider cylinder)
        {
            var t = cylinder.transform;

            var circle = new Circle(t.TransformPoint(cylinder.center).ToSimVector2(), t.lossyScale.x * cylinder.radius, cylinder.name);

            return circle;
        }
    }
}