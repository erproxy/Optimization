using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace LylekGames.Tools
{
    public class CombineSkinMeshesLite : MonoBehaviour
    {
        [HideInInspector]
        public bool initiallized = false;
        private bool invertMatrix = true;
        [Header("Animation Info")]
        public GameObject armature;
        public Animator anim;
        public AnimatorCullingMode animCullingMode = AnimatorCullingMode.AlwaysAnimate;
        public bool updateWhenOffScreen = false;
        [Header("Texture Atlasing")]
        public TextureAtlasSize textureAtlasSize = TextureAtlasSize.x1024;
        public bool isMipMap = true;
        [Header("Material Properties")]
        public bool separateTransparentMaterials = true;
        public StandardShaderUtils.BlendMode blendMode;
        [Range(0.0f, 1.0f)]
        public float metalness = 0.0f;
        [Range(0.0f, 1.0f)]
        public float smoothness = 1.0f;
        public bool useSpecularHighlights = true;
        public bool useReflections = true;

        [HideInInspector]
        public TextureFormat textureFormat = TextureFormat.RGBA32;
        [Header("Meshes")]
        [Tooltip("When combing meshes (if meshes are not manually assigned) only search for immediate children, ignoring meshes which may be stored as children of children and within the character's armature.")]
        public bool combineImmediateChildrenOnly = false;
        [Tooltip("When enabled, Skinned Meshes containing a Cloth Component such as robes or a cape will not be combined as to retain their Cloth physics.")]
        public bool excludeClothMeshes = false;
        private bool highVertexCount = false;
        [HideInInspector]
        [SerializeField]
        public GameObject myCombinedMesh;
        public List<GameObject> subMeshes = new List<GameObject>();
        [SerializeField]
        public List<SkinnedMeshRenderer> mySkinnedMeshes = new List<SkinnedMeshRenderer>();
        [HideInInspector]
        [SerializeField]
        private List<Vector3> defaultBonePositions = new List<Vector3>(); //*required to ensure proper mesh deform when combining meshes
        [HideInInspector]
        [SerializeField]
        private List<Quaternion> defaultBoneRotations = new List<Quaternion>(); //*required to ensure proper mesh deform when combining meshes
        [HideInInspector]
        [SerializeField]
        public List<Transform> myBones = new List<Transform>(); //*required to ensure proper mesh deform when combining meshes
        private List<Transform> bones = new List<Transform>();
        private List<BoneWeight> boneWeights = new List<BoneWeight>();
        private List<CombineInstance> combineInstances = new List<CombineInstance>();

        [HideInInspector]
        public List<Texture2D> texturesDiffuse = new List<Texture2D>();
        [Header("Textures")]
        public bool atlasDiffuse = true;
        public bool atlasNormals = false;
        public bool atlasSpecular = false;
        public bool atlasAO = false;

        public Texture2D diffuseAtlas;
        [Header("Buttons")]
        public bool displayHiddenButtons = false;

        public bool autoForceReadWrite = true;
        public bool autoForceStandardMaterial = false;
        public bool autoFillMissingTextures = false;

        public delegate void Combine();
        public Combine onCombine;

        //TEXTURE SIZES------------------
        public enum TextureAtlasSize
        {
            x64 = 64,
            x128 = 128,
            x256 = 256,
            x512 = 512,
            x1024 = 1024,
        }
        public void Initialize()
        {
            if (initiallized == false)
            {
                //GET ANIMATOR
                if (GetComponent<Animator>())
                    anim = GetComponent<Animator>();
                //CALCULATE BONES
                RecalculateBones();
                initiallized = true;
            }
        }
        //RECALCULATE BONES------------------
        public void RecalculateBones()
        {
            if (!Application.isPlaying)
            {
                //GET BONES AND DEFAULT BONE INFORMATION
                myBones.Clear();
                defaultBonePositions.Clear();
                defaultBoneRotations.Clear();

                myBones.Add(armature.transform);
                defaultBonePositions.Add(armature.transform.localPosition);
                defaultBoneRotations.Add(armature.transform.localRotation);

                SkinnedMeshRenderer[] skinMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();
                if (skinMeshes.Length > 0)
                {
                    Transform[] meshBone = skinMeshes[0].bones;
                    foreach (Transform bone in meshBone)
                    {
                        defaultBonePositions.Add(bone.localPosition);
                        defaultBoneRotations.Add(bone.localRotation);
                        myBones.Add(bone);
                    }
                }
                else
                {
                    Debug.LogWarning("No skinned meshes were found, so no bones were acquired. Acquiring all non-meshes within our armature, instead...");
                    Transform[] allBones = armature.transform.GetComponentsInChildren<Transform>();
                    foreach (Transform bone in allBones)
                    {
                        if (!bone.GetComponent<Renderer>())
                        {
                            defaultBonePositions.Add(bone.localPosition);
                            defaultBoneRotations.Add(bone.localRotation);
                            myBones.Add(bone);
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Recalculating bones should only be done in the Editor while the character is in a default T-pose!");
            }
        }
        //BEGIN COMBINE SKINNED MESHES------------------
        public void BeginCombineMeshes()
        {
            //CLEAR BONES, WEIGHTS, AND MESH INFORMATION
            bones.Clear();
            boneWeights.Clear();
            combineInstances.Clear();
            texturesDiffuse.Clear();
            //REMOVE ANY PREVIOUSLY COMBINED MESH
            if (myCombinedMesh)
                DestroyImmediate(myCombinedMesh);
            foreach (GameObject sub in subMeshes)
                DestroyImmediate(sub);
            subMeshes.Clear();
            //GET MY SKINNED MESHES / The meshes we will combine
            GetMySkinnedMeshes();
            //MAKE SURE WE ACTUALLY HAVE SOMETHING TO COMBINE
            if (mySkinnedMeshes.Count <= 0)
            {
                print("No meshes found.");
                if (combineImmediateChildrenOnly)
                    print("Combine Immediate Children Only is enabled.");
            }
            else
            {
                int vertCount = 0;
                foreach (SkinnedMeshRenderer mesh in mySkinnedMeshes)
                {
                    vertCount += mesh.sharedMesh.vertices.Length;
                }
                if (vertCount > 65535)
                    highVertexCount = true;
                else
                    highVertexCount = false;

                //COMBINE SKINNED MESHES
                StartCoroutine(CombineMeshes());
            }
        }
        public void GetMySkinnedMeshes()
        {
            //MAKE SURE OUR LIST OF SKINED MESHES ARE ACTIVE
            if (mySkinnedMeshes != null)
            {
                if (mySkinnedMeshes.Count > 0)
                {
                    foreach (SkinnedMeshRenderer sMesh in mySkinnedMeshes)
                    {
                        sMesh.gameObject.SetActive(true);
                    }
                }
            }
            //IF MY-SKINNED-MESHES HAVE NOT BEEN MANUALLY ASSIGNED, GET ALL ACTIVE SKINNED MESHES
            if (mySkinnedMeshes == null || mySkinnedMeshes.Count <= 0)
            {
                if (combineImmediateChildrenOnly)
                {
                    List<Transform> immediateChildren = new List<Transform>();
                    for (int k = 0; k < transform.childCount; k++)
                    {
                        immediateChildren.Add(transform.GetChild(k).transform);
                    }
                    foreach (Transform child in immediateChildren)
                    {
                        if (child.gameObject.activeSelf)
                        {
                            if (child.GetComponent<SkinnedMeshRenderer>())
                            {
                                if (!child.GetComponent<Cloth>() && excludeClothMeshes == true)
                                {
                                    mySkinnedMeshes.Add(child.GetComponent<SkinnedMeshRenderer>());
                                }
                                else if (excludeClothMeshes == false)
                                {
                                    mySkinnedMeshes.Add(child.GetComponent<SkinnedMeshRenderer>());
                                }
                            }
                        }
                    }
                }
                else
                {
                    SkinnedMeshRenderer[] skinnedMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();
                    foreach (SkinnedMeshRenderer mesh in skinnedMeshes)
                    {
                        if (mesh.gameObject.activeSelf)
                        {
                            if (!mesh.GetComponent<Cloth>() && excludeClothMeshes == true)
                            {
                                mySkinnedMeshes.Add(mesh);
                            }
                            else if (excludeClothMeshes == false)
                            {
                                mySkinnedMeshes.Add(mesh);
                            }
                        }
                    }
                }
            }
        }
        public GameObject CreateNewEmptyGameObject(string newName)
        {
            //CREATE A NEW GAMEOBJECT TO USE AS OUR COMBINED SKINNED MESH
            GameObject newGameObject = new GameObject(newName);
            //SET ITS POSITION AND ROTATION, AND PARENT IT TO OUR PLAYER OBJECT
            newGameObject.transform.position = armature.transform.position;
            newGameObject.transform.rotation = armature.transform.rotation;
            newGameObject.transform.parent = transform;

            return newGameObject;
        }
        //COMBINE SKINNED MESHES------------------
        public IEnumerator CombineMeshes()
        {
            if (onCombine != null)
                onCombine();

            if (mySkinnedMeshes.Count > 0)
            {
                myCombinedMesh = CreateNewEmptyGameObject(gameObject.name + "Mesh");

                //GET SUBMESH COUNT
                int numSubs = 0;
                foreach (SkinnedMeshRenderer smr in mySkinnedMeshes)
                {
                    numSubs += smr.sharedMesh.subMeshCount;
                }
                int[] meshIndex = new int[numSubs];
                int k = 0;

                //FOR EACH SKINNED MESH
                foreach (SkinnedMeshRenderer smr in mySkinnedMeshes)
                {
                    //GET BONES
                    Transform[] meshBones = smr.bones;
                    foreach (Transform bone in meshBones)
                    {
                        if (!bones.Contains(bone))
                            bones.Add(bone);
                    }
                }
                //FOR EACH SKINNED MESH
                foreach (SkinnedMeshRenderer smr in mySkinnedMeshes)
                {
                    if (Application.isPlaying)
                        yield return null;

                    //RESET OUR BONES TO DEFAULT POSITION
                    ResetBoneOrientationsToDefault();

                    //GET MESH INSTANCES
                    Mesh ciMesh = Instantiate(smr.sharedMesh);
                    smr.BakeMesh(ciMesh);

                    //FOR EACH SUBMESH
                    for (int j = 0; j < smr.sharedMesh.subMeshCount; j++)
                    {
                        CombineInstance ci = new CombineInstance();
                        Material mat = smr.sharedMaterials[j];

                        if (SubMeshIsCompatible(mat))
                        {
                            //RECONFIGURE BONE WEIGHTS
                            BoneWeight[] meshWeights = ReconfigureBoneWeights(smr);
                            ciMesh.boneWeights = meshWeights;

                            //GET SUB MESH INSTANCE
                            Mesh newMesh = ciMesh.GetSubmesh(j);
                            ci.mesh = newMesh;
                            ci.subMeshIndex = 0;

                            if (invertMatrix == true)
                                ci.transform = smr.transform.localToWorldMatrix * transform.worldToLocalMatrix.inverse;
                            else
                                ci.transform = smr.transform.localToWorldMatrix * transform.worldToLocalMatrix;

                            meshIndex[k] = newMesh.vertices.Length;

                            //GET BONE WEIGHTS
                            foreach (BoneWeight boneWeight in newMesh.boneWeights)
                                boneWeights.Add(boneWeight);

                            combineInstances.Add(ci);
                            k++;

                            //GET DIFFUSE TEXTURE
                            if (mat.GetTexture("_MainTex") && atlasDiffuse)
                            {
                                Texture2D myTexture = (Texture2D)mat.mainTexture;
                                if (myTexture != null)
                                {
                                    Texture2D newDiffuse;

                                    if (mat.color != Color.white)
                                    {
                                        newDiffuse = BakeTextureColor(myTexture, mat.color);
                                    }
                                    else
                                    {
                                        newDiffuse = new Texture2D(myTexture.width, myTexture.height, myTexture.format, isMipMap);
                                        Graphics.CopyTexture(myTexture, newDiffuse);
                                    }
                                    newDiffuse.Apply(isMipMap, false);

                                    texturesDiffuse.Add(newDiffuse);
                                }
                            }
                        }
                        else
                        {
                            //GET BONE WEIGHTS
                            ciMesh.boneWeights = smr.sharedMesh.boneWeights;

                            //GET SUB MESH INSTANCE
                            Mesh newMesh = ciMesh.GetSubmesh(j);
                            ci.mesh = newMesh;
                            ci.subMeshIndex = 0;

                            if (invertMatrix == true)
                                ci.transform = smr.transform.localToWorldMatrix * transform.worldToLocalMatrix.inverse;
                            else
                                ci.transform = smr.transform.localToWorldMatrix * transform.worldToLocalMatrix;

                            //CREATE NEW SKINNED SUB-MESH INSTANCE
                            GameObject newSubMesh = CreateNewEmptyGameObject(gameObject.name + "-" + mat.name + "-subMesh" + subMeshes.Count);

                            List<Matrix4x4> newPoses = new List<Matrix4x4>();
                            for (int b = 0; b < smr.bones.Length; b++)
                                newPoses.Add(smr.bones[b].worldToLocalMatrix * transform.worldToLocalMatrix);

                            List<BoneWeight> newWeights = new List<BoneWeight>();
                            foreach (BoneWeight boneWeight in newMesh.boneWeights)
                                newWeights.Add(boneWeight);

                            SkinnedMeshRenderer newSkin = newSubMesh.gameObject.AddComponent<SkinnedMeshRenderer>();
                            CombineInstance[] cis = new CombineInstance[1];
                            cis[0] = ci;
                            newSkin.sharedMesh = new Mesh();
                            newSkin.sharedMesh.CombineMeshes(cis, true, true);
                            newSkin.updateWhenOffscreen = true;
                            newSkin.material = mat;
                            newSkin.bones = smr.bones;
                            newSkin.rootBone = smr.rootBone;
                            newSkin.sharedMesh.boneWeights = newWeights.ToArray();
                            newSkin.sharedMesh.bindposes = newPoses.ToArray();
                            newSkin.sharedMesh.RecalculateBounds();

                            subMeshes.Add(newSubMesh);
                        }

                        //RESET OUR BONES TO DEFAULT POSITION
                        ResetBoneOrientationsToDefault();
                    }
                }

                foreach (SkinnedMeshRenderer smr in mySkinnedMeshes)
                {
                    //DISABLE OUR INDIVIDUAL SKINNED MESHES
                    smr.gameObject.SetActive(false);
                }

                //GET BINDPOSES
                List<Matrix4x4> bindposes = new List<Matrix4x4>();
                for (int b = 0; b < bones.Count; b++)
                {
                    bindposes.Add(bones[b].worldToLocalMatrix * transform.worldToLocalMatrix);
                }

                //CREATE OUR NEW SKINNED MESH RENDERER
                SkinnedMeshRenderer r = myCombinedMesh.gameObject.AddComponent<SkinnedMeshRenderer>();
                r.sharedMesh = new Mesh();
                if (highVertexCount)
                {
                    r.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                    print("High vertices count. Updating mesh index format to indexFormat.32.");
                }
                //COMBINE OUR MESH INSTANCES
                r.sharedMesh.CombineMeshes(combineInstances.ToArray(), true, true);
                r.updateWhenOffscreen = true;

                print("Vertices count: " + r.sharedMesh.vertices.Length);

                //CREATE OUR NEW TEXTURE ATLASES
                Rect[] packed = new Rect[0];
                if (atlasDiffuse)
                {
                    if (texturesDiffuse.Count > 0)
                    {
                        //DIFFUSE
                        diffuseAtlas = new Texture2D((int)textureAtlasSize, (int)textureAtlasSize, textureFormat, isMipMap);
                        packed = new Rect[diffuseAtlas.PackTextures(texturesDiffuse.ToArray(), 0, (int)textureAtlasSize).Length];
                        packed = diffuseAtlas.PackTextures(texturesDiffuse.ToArray(), 0, (int)textureAtlasSize);
                    }
                }
                //MAP OUR MESH'S UVs
                Vector2[] originalUVs = r.sharedMesh.uv;
                Vector2[] atlasUVs = new Vector2[originalUVs.Length];
                if (atlasDiffuse)
                {
                    if (packed.Length > 0)
                    {
                        int rectIndex = 0;
                        int vertTracker = 0;
                        for (int i = 0; i < atlasUVs.Length; i++)
                        {
                            if (i >= meshIndex[rectIndex] + vertTracker)
                            {
                                vertTracker += meshIndex[rectIndex];
                                rectIndex++;
                            }
                            atlasUVs[i].x = Mathf.Lerp(packed[rectIndex].x, packed[rectIndex].xMax, originalUVs[i].x);
                            atlasUVs[i].y = Mathf.Lerp(packed[rectIndex].y, packed[rectIndex].yMax, originalUVs[i].y);
                        }
                    }
                }
                //CREATE OUR NEW MATERIAL
                Material combinedMat = new Material(Shader.Find("Standard"));
                combinedMat = StandardShaderUtils.ChangeRenderMode(combinedMat, blendMode, metalness, smoothness, useSpecularHighlights, useReflections);
                //ASSIGN OUR TEXTURE ATLASES
                if (atlasDiffuse && diffuseAtlas != null)
                    combinedMat.mainTexture = diffuseAtlas;

                combinedMat.DisableKeyword("_NORMALMAP");
                combinedMat.DisableKeyword("_METALLICGLOSSMAP");
                
                //SET UVS OF OUR NEW MESH
                r.sharedMesh.uv = atlasUVs;
                //SET THE MATERIAL OF OUR NEW MESH
                r.sharedMaterial = combinedMat;
                //SET THE BONES OF OUR NEW MESH
                r.bones = bones.ToArray();
                //SET BONE WEIGHTS OF OUR NEW MESH
                r.sharedMesh.boneWeights = boneWeights.ToArray();
                //SET BINDPOSES
                r.sharedMesh.bindposes = bindposes.ToArray();
                //recalculate bounds
                r.sharedMesh.RecalculateBounds();
                if (anim)
                {
                    anim.cullingMode = animCullingMode;
                }
                combinedMat.color = Color.white;
                Debug.Log("✔ SkinedMeshes combined.");
            }
        }
        public Texture2D CreateColorTexture(Color myColor, int size)
        {
            Texture2D newTexture = new Texture2D(size, size, TextureFormat.RGBA32, isMipMap);
            Color[] pixels = newTexture.GetPixels();
            for (int p = 0; p < pixels.Length; p++)
            {
                float r = myColor.r;
                float b = myColor.g;
                float g = myColor.b;
                float a = myColor.a;

                Color newColor = new Color(r, b, g, a);

                pixels[p] = newColor;
            }
            newTexture.SetPixels(0, 0, size, size, pixels);
            return newTexture;
        }
        public Texture2D BakeTextureColor(Texture2D myTexture, Color myColor)
        {
            Texture2D newTexture = new Texture2D(myTexture.width, myTexture.height, TextureFormat.RGBA32, isMipMap);
            Color[] pixels = myTexture.GetPixels();
            for (int p = 0; p < pixels.Length; p++)
            {
                float r = (myColor.r * pixels[p].r);
                float b = (myColor.g * pixels[p].g);
                float g = (myColor.b * pixels[p].b);
                float a = pixels[p].a;

                Color newColor = new Color(r, b, g, a);

                pixels[p] = newColor;
            }
            newTexture.SetPixels(0, 0, myTexture.width, myTexture.height, pixels);
            return newTexture;
        }
        public BoneWeight[] ReconfigureBoneWeights(SkinnedMeshRenderer smr)
        {
            BoneWeight[] weights = smr.sharedMesh.boneWeights;
            BoneWeight[] meshWeights = new BoneWeight[smr.sharedMesh.boneWeights.Length];
            Transform[] meshBones = smr.bones;
            for (int i = 0; i < meshBones.Length; i++)
            {
                for (int b = 0; b < bones.Count; b++)
                {
                    if (bones[b] == meshBones[i])
                    {
                        for (int w = 0; w < weights.Length; w++)
                        {
                            if (weights[w].boneIndex0 == i)
                            {
                                meshWeights[w].boneIndex0 = b;
                                meshWeights[w].weight0 = weights[w].weight0;
                            }
                            if (weights[w].boneIndex1 == i)
                            {
                                meshWeights[w].boneIndex1 = b;
                                meshWeights[w].weight1 = weights[w].weight1;
                            }
                            if (weights[w].boneIndex2 == i)
                            {
                                meshWeights[w].boneIndex2 = b;
                                meshWeights[w].weight2 = weights[w].weight2;
                            }
                            if (weights[w].boneIndex3 == i)
                            {
                                meshWeights[w].boneIndex3 = b;
                                meshWeights[w].weight3 = weights[w].weight3;
                            }
                        }
                    }
                }
            }
            return meshWeights;
        }
        public void ResetBoneOrientationsToDefault()
        {
            //RESET OUR BONES TO DEFAULT POSITION
            for (int b = 0; b < myBones.Count; b++)
            {
                myBones[b].transform.localPosition = defaultBonePositions[b];
                myBones[b].transform.localRotation = defaultBoneRotations[b];
            }
        }
        public bool SubMeshIsCompatible(Material mat)
        {
            //MAKE SURE ALL SUBMESHES USE THE STANDARD MATERIAL / Otherwise, don't combine it
            int textureSize = 0;
            bool materialCompatible = true;
            if (materialCompatible == true)
            {
                if (mat.shader.name != "Standard")
                {
                    materialCompatible = false;
                    Debug.Log("Material: " + mat.name + "contains a material that does not use a Standard shader. This mesh will not be combined.");
                }
                else
                {
                    if (separateTransparentMaterials)
                    {
                        if (blendMode != StandardShaderUtils.BlendMode.Fade && blendMode != StandardShaderUtils.BlendMode.Transparent)
                        {
                            Color matColor = mat.color;
                            if (matColor.a < 1.0f)
                            {
                                materialCompatible = false;
                                Debug.Log("Material: " + mat.name + " has a color alpha of < 1.0. This material will be separated as a transparent material.");
                            }
                        }
                    }
                    if (!mat.GetTexture("_MainTex") && atlasDiffuse)
                    {
                        materialCompatible = false;
                        Debug.Log("Material: " + mat.name + "is missing a diffuse texture. This mesh will not be combined.");
                    }
                    else if (mat.GetTexture("_MainTex") && atlasDiffuse)
                    {
                        if (!mat.GetTexture("_MainTex").isReadable)
                        {
                            materialCompatible = false;
                            Debug.Log("The diffuse texture of Material: " + mat.name + "is not readable. This mesh will not be combined.");
                        }
                        else
                        {
                            if (textureSize == 0)
                                textureSize = mat.GetTexture("_MainTex").width;
                            else if (textureSize != mat.GetTexture("_MainTex").width)
                            {
                                materialCompatible = false;
                                Debug.Log("Material: " + mat.name + " has disproportionate texture sizes. This would cause disproportionate atlases. This mesh will not be combined.");
                            }
                        }
                    }
                }
            }
            return materialCompatible;
        }
        //DISABLE MESH------------------
        public void DisassembleMesh()
        {
            //RE-ENABLE OUR CHARACTER'S MESHES
            if (mySkinnedMeshes != null)
            {
                if (mySkinnedMeshes.Count > 0)
                {
                    foreach (SkinnedMeshRenderer mesh in mySkinnedMeshes)
                    {
                        mesh.gameObject.SetActive(true);
                    }
                }
            }
            //DESTROY OUR COMBINED MESH
            if (myCombinedMesh)
            {
                string myCombinedMeshName = myCombinedMesh.name;
                DestroyImmediate(myCombinedMesh, true);
                myCombinedMesh = null;
                Debug.Log("✔ " + myCombinedMeshName + " disassembled.");
            }
            foreach (GameObject sub in subMeshes)
                DestroyImmediate(sub);
            //CLEAR
            bones.Clear();
            boneWeights.Clear();
            combineInstances.Clear();
            texturesDiffuse.Clear();
            mySkinnedMeshes.Clear();
            subMeshes.Clear();
        }
    }
}
