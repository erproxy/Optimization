using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.IO;
using LylekGames.Tools.SkinnedMeshCombiner;

namespace LylekGames.Tools.SkinnedMeshCombiner
{
    [CustomEditor(typeof(AssignSkinWeights))]
	public class AssignSkinWeightsEditor : Editor
    {
		public void OnEnable()
        {
			AssignSkinWeights myASW = (AssignSkinWeights)target;
			myASW.Initialize ();
		}

		public override void OnInspectorGUI()
        {
			AssignSkinWeights myASW = (AssignSkinWeights)target;
			DrawDefaultInspector();
			if (myASW.armatureRoot)
            {
				if (GUILayout.Button ("Assign Skin Meshes"))
                {
                    myASW.GetNewSkinnedMeshes();
                    myASW.AssignSkinMeshes ();
				}
			}
            else
            {
				if (GUILayout.Button ("Initialize"))
                {
					myASW.Initialize ();
				}
			}
		}
	}
}
