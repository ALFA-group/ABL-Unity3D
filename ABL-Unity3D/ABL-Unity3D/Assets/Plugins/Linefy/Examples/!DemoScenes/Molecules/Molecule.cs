
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;


namespace LinefyExamples
{
    [ExecuteInEditMode]
    //[DefaultExecutionOrder(73)]
    public class Molecule : MonoBehaviour
    {
        public Texture2D LineTexture;
        public Texture2D OutlineTexture;
        public string Name;
        public float thicknessMultiplier = 1;
       
        [System.Serializable]
        public struct Type
        {
            public string Name;
            public int AtlasID;
            public Color Col;
        }

        [System.Serializable]
        public struct Connection
        {
            public int A;
            public int B;
            [Range(0, 1)] public int Type;
        }

        [System.Serializable]
        public struct Atom
        {
            public string Name;
            public Vector3 Pos;
            [Range(0, 3)]
            public int AtomType;
        }

        public DotsAtlas atlas;
        public Type[] AtomTypes;
        public Connection[] Connections;
        public Atom[] Atoms;

        public Dots dots;
        public Lines linesConnections;

        public Dots dotsOutline;
        public Lines linesConnectionsOutline;

        public float connectionsViewOffset;
        public float connectionsOutlineViewOffset;
        public float atomsViewOffset;
        public float atomsOutlineViewOffset;



        public void DrawConnections(Matrix4x4 tm)
        {
            if (linesConnections == null)
            {
                linesConnections = new Lines("Line", 0, false, 0);
                linesConnections.widthMode = WidthMode.PercentOfScreenHeight;
            }

            if (linesConnectionsOutline == null) {
                linesConnectionsOutline = new Lines("Line", 0, false, 0);
                linesConnectionsOutline.widthMode = WidthMode.PercentOfScreenHeight;
            }
        
            linesConnections.count = Connections.Length * 2;
            linesConnectionsOutline.count = Connections.Length *2 ;
            linesConnections.texture = LineTexture;
            linesConnectionsOutline.texture = OutlineTexture;

            for (int i = 0; i < Connections.Length; i++)
            {
                int lineIndexFirst = i * 2;
                int lineIndexSecond = i * 2 + 1;
                Connection CurrentConnection = Connections[i];
                Vector3 posA = Atoms[CurrentConnection.A].Pos;
                Vector3 posB = Atoms[CurrentConnection.B].Pos;
                Vector3 center = Vector3.Lerp(posA, posB, .5f);
                Color colA = AtomTypes[Atoms[CurrentConnection.A].AtomType].Col;
                Color colB = AtomTypes[Atoms[CurrentConnection.B].AtomType].Col;

                float w = 50 * thicknessMultiplier;
                if (CurrentConnection.Type == 0)
                {
                    Line l0 = new Line(posA, center, colA, colA, w, w, 0.05f, 0.45f);
                    Line l1 = new Line(posB, center, colB, colB, w, w, 0.05f, 0.45f);

                    linesConnectionsOutline[lineIndexFirst] = l0;
                    linesConnectionsOutline[lineIndexSecond] = l1;
                    linesConnections[lineIndexFirst] = l0;
                    linesConnections[lineIndexSecond] = l1 ;


                } else {
                    Line l0 = new Line(posA, center, colA, colA, w, w, 0.55f, 0.95f);
                    Line l1 = new Line(posB, center, colB, colB, w, w, 0.55f, 0.95f);
                    linesConnectionsOutline[lineIndexFirst] = l0;
                    linesConnectionsOutline[lineIndexSecond] = l1;
                    linesConnections[lineIndexFirst] = l0;
                    linesConnections[lineIndexSecond] = l1;
                }
                //linesConnectionsOutline[i] = new Line(posA, posB, Color.black, w );


            }

            linesConnections.viewOffset = connectionsViewOffset;
            linesConnections.Draw(tm);


            linesConnectionsOutline.viewOffset = connectionsOutlineViewOffset;
            linesConnectionsOutline.Draw(tm);
        }

        public void DrawAtoms(Matrix4x4 tm)
        {
            if (dots == null){
                dots = new Dots( 0, atlas, false);
                dots.widthMode = WidthMode.PercentOfScreenHeight;

            }
            if (dotsOutline == null) {
                dotsOutline = new Dots("Atoms", 0, atlas);
                dotsOutline.transparent = false;
                dotsOutline.colorMultiplier = Color.black;
                dotsOutline.widthMode = WidthMode.PercentOfScreenHeight;
            }
            dots.count = Atoms.Length;
            dotsOutline.count = Atoms.Length;
            for (int i = 0; i < dots.count; i++)
            {
                int TypeIndex = Atoms[i].AtomType;
                Type CurrentType = AtomTypes[TypeIndex];
                Vector3 Pos = Atoms[i].Pos;
                float size = 75 * thicknessMultiplier;
                
                Atoms[i].Name = CurrentType.Name;
                dots[i] = new Dot(Pos, size, CurrentType.AtlasID, Color.white);
                dotsOutline[i] = new Dot(Pos, size+8*thicknessMultiplier, CurrentType.AtlasID, Color.black);

            }

            dots.viewOffset = atomsViewOffset;
            dots.Draw(tm);

            dotsOutline.viewOffset = atomsOutlineViewOffset;
            dotsOutline.Draw(tm);
        }
     

        private void Update()
        {
            

            Matrix4x4 tm = transform.localToWorldMatrix;
            DrawAtoms(tm);
            DrawConnections(tm);
        }
    }
}