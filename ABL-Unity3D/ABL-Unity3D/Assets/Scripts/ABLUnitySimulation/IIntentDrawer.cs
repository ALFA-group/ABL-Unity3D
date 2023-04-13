using System.Collections.Generic;
using ABLUnitySimulation.Actions.Helpers;
using UnityEngine;

namespace ABLUnitySimulation
{
    public interface IIntentDrawer
    {
        public void DrawPath(Team team, IEnumerable<Vector2> pathPoints);

        public void DrawCircle(Team team, Circle circle);

        public void DrawText(Team team, Vector2 position, string text);
        void DrawRect(Rect rect, Team team);
    }
}