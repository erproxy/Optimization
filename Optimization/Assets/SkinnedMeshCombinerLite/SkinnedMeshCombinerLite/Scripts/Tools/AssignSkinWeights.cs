using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Attached to character's armature
//This script will assign proper bone weights to meshes (such as armors)
namespace LylekGames.Tools.SkinnedMeshCombiner
{
    [System.Serializable]
    public class AssignSkinWeights : MonoBehaviour
    {
        [SerializeField]
        public GameObject armatureRoot; //character's armature root (top/first of skeleton hierarchy (before root/master/pelvis)) | if not assigns, assume it is this object
        [SerializeField]
        public SkinnedMeshRenderer defaultSkinWeight; //the skinned mesh renderer of an object containing the weight data we require | we use the character's torso, which came rigg to this character, on import
        public List<SkinnedMeshRenderer> skinMeshes = new List<SkinnedMeshRenderer>(); //our skinned meshes to be weighted | assign in the inspector

        public void Initialize()
        {
            GetArmatureRoot();
            GetDefaultSkinWeightReference();
        }
        public void GetArmatureRoot()
        {
            Transform[] children;
            children = transform.GetComponentsInChildren<Transform>();

            foreach (Transform child in children)
            {
                if (child.name == "Armature")
                {
                    armatureRoot = child.gameObject;
                    break;
                }
            }
            if (!armatureRoot)
            {
                Debug.LogWarning("Unable to locate 'Armature'. Please assign Armature Root manually.");
            }
            else
            {
                Debug.Log("Armature found!");
            }
        }
        public void GetDefaultSkinWeightReference()
        {
            SkinnedMeshRenderer[] allSkinnedMeshes;

            allSkinnedMeshes = transform.GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (SkinnedMeshRenderer skinnedMesh in allSkinnedMeshes)
            {
                if (skinnedMesh.rootBone)
                {
                    defaultSkinWeight = skinnedMesh;
                    break;
                }
            }
            if (!defaultSkinWeight)
            {
                Debug.LogWarning("Unable to locate any SkinnedMeshRenderers with proper bone weights. Please assign Default Skin Weight manually.");
            }
            else
            {
                Debug.Log("Default Skin Weight found!");
            }
        }
        public void GetNewSkinnedMeshes()
        {
            skinMeshes.Clear();
            SkinnedMeshRenderer[] allSkinnedMeshes;
            allSkinnedMeshes = transform.GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (SkinnedMeshRenderer skinnedMesh in allSkinnedMeshes)
            {
                if (skinnedMesh.rootBone == null)
                {
                    skinMeshes.Add(skinnedMesh);
                }
            }

            if (skinMeshes.Count == 0)
            {
                Debug.Log("No new skin meshes have been found. All SkinnedMeshRenderers already have bone weights!");
            }
            else
            {
                Debug.Log("New skin meshes have been added!");
            }
        }
        public void AssignSkinMeshes()
        {
            if (!armatureRoot)
            {
                Debug.Log("ArmatureRoot was not assigned. AssignSkinWeight will try to use THIS gameObject, until otherwise specified.");
                armatureRoot = gameObject;
            }

            foreach (SkinnedMeshRenderer mesh in skinMeshes)
            {
                mesh.bones = defaultSkinWeight.bones;
                mesh.rootBone = defaultSkinWeight.rootBone;
            }
            if (skinMeshes.Count > 0)
            {
                Debug.Log("Skin Weights have been assigned!");
            }
            else
            {
                Debug.Log("No skin meshes to assign.");
            }
            skinMeshes.Clear();
        }
    }
}

