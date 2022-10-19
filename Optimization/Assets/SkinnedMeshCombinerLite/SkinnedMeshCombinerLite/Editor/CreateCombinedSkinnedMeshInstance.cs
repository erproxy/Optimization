using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LylekGames.Tools.SkinnedMeshCombiner
{
	public class CreateCombinedStaticMeshInstance
    {
		[MenuItem("Tools/Combine/Skinned Meshes")]
		private static void CreateNewPlayer() {
			//MY OBJECT
			Object selectedObject = Selection.activeObject;
			GameObject myObject = (GameObject)selectedObject;

            CombineSkinMeshesLite myCombine;
            if (!myObject.GetComponent<CombineSkinMeshesLite>())
                myCombine = myObject.AddComponent<CombineSkinMeshesLite>();
            else
                myCombine = myObject.GetComponent<CombineSkinMeshesLite>();
        }
	}
}