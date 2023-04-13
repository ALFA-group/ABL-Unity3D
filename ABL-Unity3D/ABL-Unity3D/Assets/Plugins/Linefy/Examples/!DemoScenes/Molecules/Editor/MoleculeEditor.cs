using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Linefy;

namespace LinefyExamples
{
    [CustomEditor(typeof(Molecule))]
    public class MoleculeEditor : Editor
    {
        Vector3Handle[] handles;

        private void OnEnable()
        {
            CreateHandles();
        }

        void CreateHandles()
        {
            Molecule t = target as Molecule;
            handles = new Vector3Handle[t.Atoms.Length];
            for (int i = 0; i < handles.Length; i++)
            {
                handles[i] = new Vector3Handle(i);
            }
        }

        private void OnSceneGUI()
        {
            Molecule t = target as Molecule;
            Handles.matrix = t.transform.localToWorldMatrix;
            if (t.Atoms.Length != handles.Length)
            {
                CreateHandles();
            }
            for (int i = 0; i < handles.Length; i++)
            {
                t.Atoms[i].Pos = handles[i].DrawOnSceneGUI(t.Atoms[i].Pos, true, false);
            }
        }
    }
}
