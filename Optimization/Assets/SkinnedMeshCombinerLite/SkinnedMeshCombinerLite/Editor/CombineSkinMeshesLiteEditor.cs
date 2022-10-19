using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.IO;
using LylekGames.Tools.SkinnedMeshCombiner;

namespace LylekGames.Tools.SkinnedMeshCombiner
{
    [CustomEditor(typeof(CombineSkinMeshesLite))]
    public class CombineSkinMeshesLiteEditor : Editor
    {
        CombineSkinMeshesLite myCombine;

        public delegate void GUIDelegate();
        public GUIDelegate onGUI;

        private GameObject model;
        private ModelImporter importer;
        private ModelImporterAnimationType animationType = ModelImporterAnimationType.None;

        private bool properOrientation = false;
        private bool properScale = false;

        private bool fixScale = false;
        private bool fixOrientation = false;
        private bool forcePose = false;

        private bool showFixOrientationInfo;
        private bool showFixScaleInfo;

        private bool showAnimationInfo = true;
        private bool showAtlasingInfo = true;
        private bool showAtlases = true;
        private bool showMaterialInfo = true;
        private bool showSkinnedMeshes = true;
        private bool showRecalculateBones = false;
        private bool showMyMeshes = false;
        private bool showBones = false;

        private bool showDisassembleInfo = true;

        public void OnEnable()
        {
            myCombine = (CombineSkinMeshesLite)target;

            if (!myCombine.armature || !myCombine.initiallized)
                onGUI = InitializeGUI;
            else
                onGUI = DefaultGUI;

            myCombine.onCombine += ForceStandardMaterial;
            myCombine.onCombine += ForceReadWriteEnabled;
            myCombine.onCombine += FillMissingTextures;
        }
        public void OnDisable()
        {
            myCombine.onCombine -= ForceReadWriteEnabled;
            myCombine.onCombine -= ForceStandardMaterial;
            myCombine.onCombine -= FillMissingTextures;
        }
        public void InitializeGUI()
        {
            GUI.enabled = false;
            MonoScript script = MonoScript.FromMonoBehaviour((CombineSkinMeshesLite)target);
            script = EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false) as MonoScript;
            GUI.enabled = true;

            if (!myCombine.armature)
            {
                EditorGUILayout.HelpBox("Please assign an armature.", MessageType.None);
                myCombine.armature = EditorGUILayout.ObjectField("Armature", myCombine.armature, typeof(GameObject), true) as GameObject;
                EditorGUILayout.Space();
            }
            else
            {
                if (myCombine.armature.transform.localRotation.x != 0 || myCombine.armature.transform.localRotation.y != 0 || myCombine.armature.transform.localRotation.z != 0)
                { properOrientation = false; }
                else
                { properOrientation = true; }
                if (myCombine.armature.transform.localScale.x != 1 || myCombine.armature.transform.localScale.y != 1 || myCombine.armature.transform.localScale.z != 1)
                { properScale = false; }
                else
                { properScale = true; }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.HelpBox("Your armature.", MessageType.None);
                myCombine.armature = EditorGUILayout.ObjectField("Armature", myCombine.armature, typeof(GameObject), true) as GameObject;
                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();

                if (!properOrientation || !properScale || !myCombine.initiallized)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    if (!properOrientation || !properScale)
                    {
                        EditorGUILayout.HelpBox("Something is wrong.", MessageType.Warning);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Optional", MessageType.None);
                    }

                    if (!properScale)
                    {
                        fixScale = true;
                        EditorGUILayout.BeginHorizontal();
                        GUI.enabled = false;
                        fixScale = EditorGUILayout.Toggle(fixScale, GUILayout.Width(40));
                        GUI.enabled = true;
                        showFixScaleInfo = EditorGUILayout.Foldout(showFixScaleInfo, "Your armature is not to proper scale.");
                        EditorGUILayout.EndHorizontal();
                        if (showFixScaleInfo)
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.HelpBox("Your armature AND all child Skinned Mesh Renderers must be at a scale of (1, 1, 1). You may a resize your character either by adjusting the Scale Factor in the Import Settings, or the Transform scale of the root game object.", MessageType.None);
                            EditorGUILayout.EndVertical();
                        }
                    }
                    if (!properOrientation)
                    {
                        fixOrientation = true;
                        EditorGUILayout.BeginHorizontal();
                        GUI.enabled = false;
                        fixOrientation = EditorGUILayout.Toggle(fixOrientation, GUILayout.Width(40));
                        GUI.enabled = true;
                        showFixOrientationInfo = EditorGUILayout.Foldout(showFixOrientationInfo, "Your armature does not have proper orientation.");
                        EditorGUILayout.EndHorizontal();
                        if (showFixOrientationInfo)
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.HelpBox("Your armature AND alld child Skinned Mesh Renderers must be at a rotation of (0, 0, 0). It is likely that the character was not imported properly. Try reimporting the character with proper orientation, or press Fix. This fix may not leave your character standing up-right.", MessageType.None);
                            EditorGUILayout.EndVertical();
                        }
                    }
                    if (!myCombine.initiallized)
                    {
                        EditorGUILayout.BeginHorizontal();
                        forcePose = EditorGUILayout.Toggle(forcePose, GUILayout.Width(40));
                        forcePose = EditorGUILayout.Foldout(forcePose, "Your armature must be in Default Pose.");
                        EditorGUILayout.EndHorizontal();
                        if (forcePose)
                        {
                            if (!model)
                            {
                                Animator anim = null;
                                anim = myCombine.GetComponent<Animator>();
                                if (anim != null)
                                {
                                    if (anim.avatar != null)
                                    {
                                        string avatarPath = "";
                                        avatarPath = AssetDatabase.GetAssetPath(anim.avatar);
                                        avatarPath = avatarPath.Replace("Avatar", "");
                                        avatarPath = avatarPath.Replace("avatar", "");
                                        Debug.Log(avatarPath);
                                        model = (GameObject)AssetDatabase.LoadAssetAtPath(avatarPath, typeof(GameObject));
                                    }
                                }
                            }
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                            if (!model)
                            {
                                EditorGUILayout.HelpBox("Model could not be found.\nPlease assign the imported model file.", MessageType.Warning);
                            }
                            model = EditorGUILayout.ObjectField("Model Importer", model, typeof(GameObject), true) as GameObject;
                            EditorGUILayout.LabelField("Force a default pose onto the character to ensure its bones are oriented properly. Keep unchecked if character is already in its default pose.", EditorStyles.wordWrappedLabel);
                            EditorGUILayout.HelpBox("This pose will be forced by changing the the model's Animation Type to None and back again, and then reimporting the file. Do not continue if you are not sure what this means or how it may affect your project.", MessageType.Warning);
                            EditorGUILayout.EndVertical();
                        }
                    }
                    if (fixScale || fixOrientation || forcePose)
                    {
                        EditorGUILayout.Space();
                        if (GUILayout.Button("Fix"))
                        {
                            if (forcePose)
                            {
                                if (model != null)
                                {
                                    AssetImporter asset = (ModelImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(model));
                                    importer = asset as ModelImporter;
                                    if (importer != null)
                                    {
                                        animationType = importer.animationType;
                                        importer.animationType = ModelImporterAnimationType.None;
                                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(model));
                                        myCombine.Initialize();
                                        importer.animationType = animationType;
                                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(model));
                                        onGUI = DefaultGUI;
                                    }
                                    else
                                    {
                                        Debug.Log("Failed to force default pose. " + AssetDatabase.GetAssetPath(model));
                                    }
                                }
                                else
                                {
                                    Debug.Log("Failed to force default pose, model not assigned.");
                                }
                                forcePose = false;
                            }
                            SkinnedMeshRenderer[] skinnedMeshes = myCombine.transform.GetComponentsInChildren<SkinnedMeshRenderer>();
                            if (fixOrientation)
                            {
                                Quaternion properRot = Quaternion.identity;
                                myCombine.armature.transform.localRotation = properRot;
                                foreach (SkinnedMeshRenderer skinnedMesh in skinnedMeshes)
                                    skinnedMesh.transform.localRotation = properRot;
                                fixOrientation = false;
                            }
                            if (fixScale)
                            {
                                Vector3 properScale = new Vector3(1, 1, 1);
                                myCombine.armature.transform.localScale = properScale;
                                foreach (SkinnedMeshRenderer skinnedMesh in skinnedMeshes)
                                    skinnedMesh.transform.localScale = properScale;
                                fixScale = false;
                            }


                            if (!myCombine.initiallized)
                            {
                                myCombine.Initialize();
                                onGUI = DefaultGUI;
                            }
                        }
                    }
                    else
                    {
                        EditorGUILayout.Space();
                        if (GUILayout.Button("INITIALIZE"))
                        {
                            if (myCombine.armature)
                            {
                                myCombine.Initialize();
                                onGUI = DefaultGUI;
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("View the Readme file for further information, or contact support@lylekgames.com for assistance.", MessageType.None);
                }
            }
        }
        public void DefaultGUI()
        {
            GUI.enabled = false;
            MonoScript script = MonoScript.FromMonoBehaviour((CombineSkinMeshesLite)target);
            script = EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false) as MonoScript;
            GUI.enabled = true;

            EditorGUILayout.Space();

            if (!myCombine.myCombinedMesh)
            {
                showSkinnedMeshes = EditorGUILayout.Foldout(showSkinnedMeshes, new GUIContent("Combine Meshes", ""));
                if (showSkinnedMeshes)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    if (GUILayout.Button(new GUIContent("Combine Meshes", "In code call the BeginCombineMeshes(); method.")))
                    {
                        myCombine.BeginCombineMeshes();
                    }
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    showMyMeshes = EditorGUILayout.Foldout(showMyMeshes, new GUIContent("Skinned Meshes", "The meshes to combine."));
                    EditorGUILayout.EndHorizontal();
                    if (showMyMeshes)
                    {
                        if (myCombine.mySkinnedMeshes.Count == 0)
                        {
                            if (myCombine.combineImmediateChildrenOnly)
                                EditorGUILayout.HelpBox("If left empty all IMMEDIATE child Skinned Mesh Renderers will be acquired.", MessageType.Info);
                            else
                                EditorGUILayout.HelpBox("If left empty ALL child Skinned Mesh Renderers will be acquired.", MessageType.Info);
                        }
                        for (int i = 0; i < myCombine.mySkinnedMeshes.Count; i++)
                        {
                            SkinnedMeshRenderer smr = myCombine.mySkinnedMeshes[i];
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button("-", GUILayout.Width(20)))
                            {
                                myCombine.mySkinnedMeshes.Remove(smr);
                            }
                            if (smr != null)
                                smr = EditorGUILayout.ObjectField(smr, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;
                            EditorGUILayout.EndHorizontal();
                        }
                        if (myCombine.mySkinnedMeshes.Count > 0)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.Space();
                            if (GUILayout.Button("Clear", GUILayout.Width(60)))
                            {
                                myCombine.mySkinnedMeshes.Clear();
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.Space();
                        }
                        DropAreaSkinnedMeshesGUI();
                    }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    myCombine.autoForceReadWrite = EditorGUILayout.Toggle(new GUIContent("Auto-Force Read/Write Enabled", "Automatically enable Read/Write for all textures before combining meshes. This may cause a delay when combing meshes for the first time, for textures that do not already have Read/Wite enabled, but it beats setting it manually.\n\nRead/Write must be enabled for all textures in order for atlasing to work. This must be done before build."), myCombine.autoForceReadWrite);
                    if (!myCombine.autoForceReadWrite)
                        EditorGUILayout.HelpBox("Please make sure your textures have Read/Write enabled in the Import Settings, otherwise atlasing will break!", MessageType.Warning);

                    myCombine.autoForceStandardMaterial = EditorGUILayout.Toggle(new GUIContent("Auto-Force Standard Material", "Automatically force all materials to use the Standard shader before combining meshes. If left unchecked any mesh containing a material that does not use the Standard shader will be excluded from combine."), myCombine.autoForceStandardMaterial);

                    myCombine.autoFillMissingTextures = EditorGUILayout.Toggle(new GUIContent("Auto-Fill Missing Textures", "If a texture type is enable for atlasing, but a material is missing this texture type, we will automatically fill this property with a 'Default' texture map before combining meshes. If left unchecked any mesh containing a material that is missing a texture for atlasing will be excluded from combine."), myCombine.autoFillMissingTextures);

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    myCombine.combineImmediateChildrenOnly = EditorGUILayout.Toggle(new GUIContent("Combine Immediate Children Only", "Only IMMEDIATE children will be combined. Children of children will be excluded."), myCombine.combineImmediateChildrenOnly);
                    myCombine.excludeClothMeshes = EditorGUILayout.Toggle(new GUIContent("Exclude Cloth Meshes", "Skinned meshes containing a Cloth component will not be combined, as to retain their cloth physics."), myCombine.excludeClothMeshes);
                    EditorGUILayout.EndVertical();

                    if (myCombine.autoForceReadWrite || myCombine.autoForceStandardMaterial || myCombine.autoFillMissingTextures)
                        EditorGUILayout.Space();

                    if (myCombine.autoForceReadWrite)
                    {
                        if (GUILayout.Button("Force Read/Write  -> Now!"))
                        {
                            ForceReadWriteEnabled();
                        }
                    }
                    if (myCombine.autoForceStandardMaterial)
                    {
                        if (GUILayout.Button("Force Standard Materials -> Now!"))
                        {
                            ForceStandardMaterial();
                        }
                    }
                    if (myCombine.autoFillMissingTextures)
                    {
                        if (GUILayout.Button("Fill Missing Textures  -> Now!"))
                        {
                            FillMissingTextures();
                        }
                    }
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.Space();
                
                showAtlasingInfo = EditorGUILayout.Foldout(showAtlasingInfo, "Atlasing Properties");
                if (showAtlasingInfo)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    myCombine.textureAtlasSize = (CombineSkinMeshesLite.TextureAtlasSize)EditorGUILayout.EnumPopup("Texture Size", myCombine.textureAtlasSize);
                    myCombine.isMipMap = EditorGUILayout.Toggle("Mip Map", myCombine.isMipMap);

                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    showAtlases = EditorGUILayout.Foldout(showAtlases, "Atlases");
                    EditorGUILayout.EndHorizontal();
                    if (showAtlases)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        myCombine.atlasDiffuse = EditorGUILayout.Toggle(new GUIContent("Albedo", "If unchecked, albedo (diffuse) maps will be excluded from atlasing."), myCombine.atlasDiffuse);
                        GUI.enabled = false;
                        myCombine.atlasNormals = EditorGUILayout.Toggle(new GUIContent("Normals", "If unchecked, normal maps will be excluded from atlasing."), false);
                        myCombine.atlasSpecular = EditorGUILayout.Toggle(new GUIContent("Specular", "If unchecked, specular (metallic) maps will be excluded from atlasing."), false);
                        myCombine.atlasAO = EditorGUILayout.Toggle(new GUIContent("Ambient Occlusion", "If unchecked, ambient occlusion maps will be excluded from atlasing."), false);
                        GUI.enabled = true;
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.Space();
                showAnimationInfo = EditorGUILayout.Foldout(showAnimationInfo, "Animation Properties");
                if (showAnimationInfo)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    myCombine.armature = EditorGUILayout.ObjectField("Armature", myCombine.armature, typeof(GameObject), true) as GameObject;
                    myCombine.anim = EditorGUILayout.ObjectField("Animator", myCombine.anim, typeof(Animator), true) as Animator;
                    myCombine.animCullingMode = (AnimatorCullingMode)EditorGUILayout.EnumPopup(new GUIContent("Anim Culling Mode", "Animator culling mode."), myCombine.animCullingMode);
                    myCombine.updateWhenOffScreen = EditorGUILayout.Toggle(new GUIContent("Update When Off Screen", "Update when off screen."), myCombine.updateWhenOffScreen);
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.Space();
                showMaterialInfo = EditorGUILayout.Foldout(showMaterialInfo, "Material Properties");
                if (showMaterialInfo)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    myCombine.blendMode = (StandardShaderUtils.BlendMode)EditorGUILayout.EnumPopup("Blend Mode", myCombine.blendMode);
                    if (myCombine.blendMode != StandardShaderUtils.BlendMode.Fade && myCombine.blendMode != StandardShaderUtils.BlendMode.Transparent)
                    {
                        myCombine.separateTransparentMaterials = EditorGUILayout.Toggle(new GUIContent("Separate Transparent Materials", "Materials with a color alpha value of less than 1.0 will be treated like transparent materials and not included in the combined " + myCombine.blendMode + " mesh."), myCombine.separateTransparentMaterials);
                        EditorGUILayout.Space();
                    }
                    myCombine.metalness = EditorGUILayout.Slider("Metalness", myCombine.metalness, 0.0f, 1.0f);
                    myCombine.smoothness = EditorGUILayout.Slider("Smoothness", myCombine.smoothness, 0.0f, 1.0f);
                    myCombine.useSpecularHighlights = EditorGUILayout.Toggle("Specular Highlights", myCombine.useSpecularHighlights);
                    myCombine.useReflections = EditorGUILayout.Toggle("Reflections", myCombine.useReflections);
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.Space();

                if (!Application.isPlaying)
                {
                    showRecalculateBones = EditorGUILayout.Foldout(showRecalculateBones, new GUIContent("Recalculate Bones", ""));
                    if (showRecalculateBones)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(10);
                        showBones = EditorGUILayout.Foldout(showBones, new GUIContent("Bones", ""));
                        EditorGUILayout.EndHorizontal();
                        if (showBones)
                        {
                            for (int i = 0; i < myCombine.myBones.Count; i++)
                            {
                                Transform bone = myCombine.myBones[i];
                                EditorGUILayout.BeginHorizontal();
                                if (GUILayout.Button("-", GUILayout.Width(20)))
                                {
                                    myCombine.myBones.Remove(bone);
                                }
                                if (bone != null)
                                    bone = EditorGUILayout.ObjectField(bone, typeof(Transform), true) as Transform;
                                EditorGUILayout.EndHorizontal();
                            }
                            if (myCombine.myBones.Count > 0)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.Space();
                                if (GUILayout.Button("Clear", GUILayout.Width(60)))
                                {
                                    myCombine.myBones.Clear();
                                }
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.Space();
                            }
                            DropAreaTransformGUI();
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.BeginHorizontal();
                        forcePose = EditorGUILayout.Toggle(forcePose, GUILayout.Width(40));
                        forcePose = EditorGUILayout.Foldout(forcePose, "Force default pose");
                        EditorGUILayout.EndHorizontal();
                        if (forcePose)
                        {
                            if (!model)
                            {
                                Animator anim = null;
                                anim = myCombine.GetComponent<Animator>();
                                if (anim != null)
                                {
                                    if (anim.avatar != null)
                                    {
                                        string avatarPath = "";
                                        avatarPath = AssetDatabase.GetAssetPath(anim.avatar);
                                        avatarPath = avatarPath.Replace("Avatar", "");
                                        avatarPath = avatarPath.Replace("avatar", "");
                                        Debug.Log(avatarPath);
                                        model = (GameObject)AssetDatabase.LoadAssetAtPath(avatarPath, typeof(GameObject));
                                    }
                                }
                            }
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            if (!model)
                            {
                                EditorGUILayout.HelpBox("Model could not be found.\nPlease assign the imported model file.", MessageType.Warning);
                            }
                            model = EditorGUILayout.ObjectField("Model Importer", model, typeof(GameObject), true) as GameObject;
                            EditorGUILayout.LabelField("Force a default pose onto the character to ensure its bones are oriented properly. Keep unchecked if character is already in its default pose.", EditorStyles.wordWrappedLabel);
                            EditorGUILayout.HelpBox("This pose will be forced by changing the the model's Animation Type to None and back again, and then reimporting the file. Do not continue if you are not sure what this means or how it may affect your project.", MessageType.Warning);
                            EditorGUILayout.EndVertical();
                        }
                        EditorGUILayout.EndVertical();
                        if (GUILayout.Button("Recalculate"))
                        {
                            if (forcePose)
                            {
                                if (model != null)
                                {
                                    AssetImporter asset = (ModelImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(model));
                                    importer = asset as ModelImporter;
                                    if (importer != null)
                                    {
                                        animationType = importer.animationType;
                                        importer.animationType = ModelImporterAnimationType.None;
                                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(model));
                                        myCombine.RecalculateBones();
                                        importer.animationType = animationType;
                                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(model));
                                        onGUI = DefaultGUI;
                                    }
                                    else
                                    {
                                        Debug.Log("Failed to force default pose. " + AssetDatabase.GetAssetPath(model));
                                    }
                                }
                                else
                                {
                                    Debug.Log("Failed to force default pose, model not assigned.");
                                }
                                forcePose = false;
                            }
                            else
                            {
                                myCombine.RecalculateBones();
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }
                }
            }
            else
            {
                showDisassembleInfo = EditorGUILayout.Foldout(showDisassembleInfo, "Dissasemble Meshes");
                if (showDisassembleInfo)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    if (GUILayout.Button(new GUIContent("Disassemble Mesh", "In code call the DisassembleMesh(); method")))
                    {
                        onGUI = DefaultGUI;
                        myCombine.DisassembleMesh();
                    }
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("WARNING: Saving is not available in Skinned Mesh Combiner Lite. Do not combine multiple characters in one scene without the use of the save feature. Especilly when working with characters that utilize large sizes of data. This may overload your scene and may cause it to crash. The data combined is intended to be saved to your project.", MessageType.Warning);
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Need help?\n\nView the Readme file for further information, or contact support@lylekgames.com for assistance.", MessageType.None);
            if (GUILayout.Button(new GUIContent("Link>> Skinned Mesh Combiner PRO"), EditorStyles.miniLabel))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/modeling/multi-material-atlasing-skinned-mesh-combiner-with-data-save-139574?aid=1101l3JmD");
            }
        }
        public void DropAreaSkinnedMeshesGUI()
        {
            Event evt = Event.current;
            Rect drop_area = GUILayoutUtility.GetRect(0.0f, 20.0f, GUILayout.ExpandWidth(true));
            GUI.Box(drop_area, "+ Drag n' Drop");

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop_area.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        bool alreadyContained = false;
                        foreach (Object dragndrop in DragAndDrop.objectReferences)
                        {
                            if (dragndrop is GameObject)
                            {
                                GameObject newObj = (GameObject)dragndrop as GameObject;
                                SkinnedMeshRenderer newsmr = newObj.GetComponent<SkinnedMeshRenderer>();
                                if (newsmr != null)
                                {
                                    foreach (SkinnedMeshRenderer smr in myCombine.mySkinnedMeshes)
                                    {
                                        if (newsmr == smr)
                                            alreadyContained = true;
                                    }
                                    if (!alreadyContained)
                                    {
                                        myCombine.mySkinnedMeshes.Add(newsmr);
                                    }
                                    else Debug.Log("That Skinned Mesh Renderer has already been added.");
                                }
                            }
                        }
                    }
                    break;
            }
        }
        public void DropAreaTransformGUI()
        {
            Event evt = Event.current;
            Rect drop_area = GUILayoutUtility.GetRect(0.0f, 20.0f, GUILayout.ExpandWidth(true));
            GUI.Box(drop_area, "+ Drag n' Drop");

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop_area.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        bool alreadyContained = false;
                        foreach (Object dragndrop in DragAndDrop.objectReferences)
                        {
                            if (dragndrop is GameObject)
                            {
                                GameObject newObj = (GameObject)dragndrop as GameObject;
                                Transform newTransform = newObj.GetComponent<Transform>();
                                if (newTransform != null)
                                {
                                    foreach (Transform bone in myCombine.myBones)
                                    {
                                        if (newTransform == bone)
                                            alreadyContained = true;
                                    }
                                    if (!alreadyContained)
                                    {
                                        myCombine.myBones.Add(newTransform);
                                    }
                                    else Debug.Log("That Transform has already been added.");
                                }
                            }
                        }
                    }
                    break;
            }
        }
        public override void OnInspectorGUI()
        {
            if (onGUI != null)
                onGUI();
            else
                DrawDefaultInspector();
        }
        public void FillMissingTextures()
        {
            if (myCombine.autoFillMissingTextures)
            {
                GetAndSetActualTextureSizes();

                SkinnedMeshRenderer[] myMeshes = myCombine.GetComponentsInChildren<SkinnedMeshRenderer>();
                bool forceRequired = false;
                foreach (SkinnedMeshRenderer smr in myMeshes)
                {
                    for (int j = 0; j < smr.sharedMesh.subMeshCount; j++)
                    {
                        int textureSize = 64;
                        Material mat = smr.sharedMaterials[j];
                        if (mat.shader.name != "Standard")
                        {
                            forceRequired = true;
                            Debug.Log("Material: " + mat.name + " of " + smr.name + " does not use the Standard shader. Filling of missing textures may not be possible.");
                        }
                        //FIND THE APPROPRIATE TEXTURE SIZE FOR OUR FILL TEXTURE(S)
                        if (mat.GetTexture("_MainTex"))
                        {
                            textureSize = mat.GetTexture("_MainTex").width;
                        }
                        else if (mat.GetTexture("_BumpMap"))
                        {
                            textureSize = mat.GetTexture("_BumpMap").width;
                        }
                        else if (mat.GetTexture("_MetallicGlossMap"))
                        {
                            textureSize = mat.GetTexture("_MetallicGlossMap").width;
                        }
                        else if (mat.GetTexture("_OcclusionMap"))
                        {
                            textureSize = mat.GetTexture("_OcclusionMap").width;
                        }
                        //FILL MISSING TEXTURES
                        if (myCombine.atlasDiffuse)
                        {
                            if (mat.GetTexture("_MainTex") == null)
                            {
                                mat.SetTexture("_MainTex", (Texture2D)Resources.Load("DefaultTextures/Default_DiffuseMap_" + textureSize.ToString()));
                                forceRequired = true;
                                Debug.Log("✔ Filled missing Diffuse texture for " + mat.name + " of " + smr.name + " mesh.");
                            }
                        }
                        if (myCombine.atlasNormals)
                        {
                            if (mat.GetTexture("_BumpMap") == null)
                            {
                                mat.SetTexture("_BumpMap", (Texture2D)Resources.Load("DefaultTextures/Default_NormalMap_" + textureSize.ToString()));
                                forceRequired = true;
                                Debug.Log("✔ Filled missing Normalmap texture for " + mat.name + " of " + smr.name + " mesh.");
                            }
                        }
                        if (myCombine.atlasSpecular)
                        {
                            if (mat.GetTexture("_MetallicGlossMap") == null)
                            {
                                float smoothness = mat.GetFloat("_Glossiness");
                                float metalness = mat.GetFloat("_Metallic");
                                Color newColor = new Color(metalness, metalness, metalness, smoothness);
                                Texture2D newTexture = Instantiate(myCombine.CreateColorTexture(newColor, textureSize));
                                mat.SetTexture("_MetallicGlossMap", newTexture);
                                //mat.SetTexture("_MetallicGlossMap", (Texture2D)Resources.Load("DefaultTextures/Default_SpecularMap_" + textureSize.ToString()));
                                forceRequired = true;
                                Debug.Log("✔ Filled missing Metallic texture for " + mat.name + " of " + smr.name + " mesh.");
                            }
                        }
                        if (myCombine.atlasAO)
                        {
                            if (mat.GetTexture("_OcclusionMap") == null)
                            {
                                mat.SetTexture("_OcclusionMap", (Texture2D)Resources.Load("DefaultTextures/Default_DiffuseMap_" + textureSize.ToString()));
                                forceRequired = true;
                                Debug.Log("✔ Filled missing Ambient Occlusion texture for " + mat.name + " of " + smr.name + " mesh.");
                            }
                        }
                    }
                }
                if (!forceRequired)
                {
                    Debug.Log("✔ All textures maps are already assigned.");
                }
            }
        }
        public void GetAndSetActualTextureSizes()
        {
            SkinnedMeshRenderer[] myMeshes = myCombine.GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (SkinnedMeshRenderer smr in myMeshes)
            {
                for (int j = 0; j < smr.sharedMesh.subMeshCount; j++)
                {
                    int textureSize = 64;
                    Material mat = smr.sharedMaterials[j];
                    //FIND THE TEXTURE SIZE OF ANY EXISTING TEXTURES
                    if (mat.GetTexture("_MainTex"))
                    {
                        Texture2D myTexture = (Texture2D)mat.mainTexture;
                        if (myTexture != null)
                        {
                            if (ReturnActualTextureSize(myTexture) != 0)
                            {
                                textureSize = ReturnActualTextureSize(myTexture);
                                SetActualTextureMaxSize(myTexture, textureSize);
                            }
                        }
                    }
                    if (mat.GetTexture("_BumpMap"))
                    {
                        Texture2D myTexture = (Texture2D)mat.GetTexture("_BumpMap");
                        if (myTexture != null)
                        {
                            if (ReturnActualTextureSize(myTexture) != 0)
                            {
                                textureSize = ReturnActualTextureSize(myTexture);
                                SetActualTextureMaxSize(myTexture, textureSize);
                            }
                        }
                    }
                    if (mat.GetTexture("_MetallicGlossMap"))
                    {
                        Texture2D myTexture = (Texture2D)mat.GetTexture("_MetallicGlossMap");
                        if (myTexture != null)
                        {
                            if (ReturnActualTextureSize(myTexture) != 0)
                            {
                                textureSize = ReturnActualTextureSize(myTexture);
                                SetActualTextureMaxSize(myTexture, textureSize);
                            }
                        }
                    }
                    if (mat.GetTexture("_OcclusionMap"))
                    {
                        Texture2D myTexture = (Texture2D)mat.GetTexture("_OcclusionMap");
                        if (myTexture != null)
                        {
                            if (ReturnActualTextureSize(myTexture) != 0)
                            {
                                textureSize = ReturnActualTextureSize(myTexture);
                                SetActualTextureMaxSize(myTexture, textureSize);
                            }
                        }
                    }
                }
            }
        }
        public int ReturnActualTextureSize(Texture2D myTexture)
        {
            int textureSize = 0;

            string textureAssetPath = AssetDatabase.GetAssetPath(myTexture);
            TextureImporter textureImport = (TextureImporter)TextureImporter.GetAtPath(textureAssetPath);

            System.Type type = typeof(TextureImporter);
            MethodInfo method = type.GetMethod("GetWidthAndHeight", BindingFlags.Instance | BindingFlags.NonPublic);

            if (textureImport != null)
            {
                var args = new object[2];
                method.Invoke(textureImport, args);
                textureSize = (int)args[0];
            }

            return textureSize;
        }
        public void SetActualTextureMaxSize(Texture2D myTexture, int maxSize)
        {
            string textureAssetPath = AssetDatabase.GetAssetPath(myTexture);
            TextureImporter textureImport = (TextureImporter)TextureImporter.GetAtPath(textureAssetPath);

            if (textureImport.maxTextureSize != maxSize)
            {
                textureImport.maxTextureSize = maxSize;
                Debug.Log("✔ Corrected maxSize value of Texture:" + myTexture.name + ".");
                textureImport.SaveAndReimport();
            }
        }
        public void ForceStandardMaterial()
        {
            if (myCombine.autoForceStandardMaterial)
            {
                SkinnedMeshRenderer[] myMeshes = myCombine.GetComponentsInChildren<SkinnedMeshRenderer>();
                bool forceRequired = false;
                foreach (SkinnedMeshRenderer smr in myMeshes)
                {
                    for (int j = 0; j < smr.sharedMesh.subMeshCount; j++)
                    {
                        Material mat = smr.sharedMaterials[j];
                        if (mat.shader.name != "Standard")
                        {
                            mat.shader = Shader.Find("Standard");
                            forceRequired = true;
                            Debug.Log("✔ Forced Standard shader for Material: " + mat.name + " of " + smr.name + " mesh.");
                        }
                    }
                }
                if (!forceRequired)
                {
                    Debug.Log("✔ All materials are already using the Standard shader.");
                }
            }
        }
        public void ForceReadWriteEnabled()
        {
            if (myCombine.autoForceReadWrite)
            {
                GetAndSetActualTextureSizes();

                SkinnedMeshRenderer[] myMeshes = myCombine.GetComponentsInChildren<SkinnedMeshRenderer>();
                bool forceRequired = false;
                foreach (SkinnedMeshRenderer smr in myMeshes)
                {
                    for (int j = 0; j < smr.sharedMesh.subMeshCount; j++)
                    {
                        Material mat = smr.sharedMaterials[j];
                        if (mat.GetTexture("_MainTex"))
                        {
                            Texture2D myTexture = (Texture2D)mat.mainTexture;
                            if (myTexture != null)
                            {
                                string textureAssetPath = AssetDatabase.GetAssetPath(myTexture);
                                TextureImporter textureImport = (TextureImporter)TextureImporter.GetAtPath(textureAssetPath);
                                if (textureImport != null)
                                {
                                    if (!textureImport.isReadable)
                                    {
                                        textureImport.isReadable = true;
                                        textureImport.SaveAndReimport();
                                        forceRequired = true;
                                        Debug.Log("✔ Forced Read/Write Enabled for Texture: " + myTexture.name + "\nof Material: " + mat.name + " of " + smr.name + " mesh.");
                                    }
                                }
                            }
                        }
                        if (mat.GetTexture("_BumpMap"))
                        {
                            Texture2D myTexture = (Texture2D)mat.GetTexture("_BumpMap");
                            if (myTexture != null)
                            {
                                string textureAssetPath = AssetDatabase.GetAssetPath(myTexture);
                                TextureImporter textureImport = (TextureImporter)TextureImporter.GetAtPath(textureAssetPath);
                                if (textureImport != null)
                                {
                                    if (!textureImport.isReadable)
                                    {
                                        textureImport.isReadable = true;
                                        textureImport.SaveAndReimport();
                                        forceRequired = true;
                                        Debug.Log("✔ Forced Read/Write Enabled for Texture: " + myTexture.name + "\nof Material: " + mat.name + " of " + smr.name + " mesh.");
                                    }
                                }
                            }
                        }
                        if (mat.GetTexture("_MetallicGlossMap"))
                        {
                            Texture2D myTexture = (Texture2D)mat.GetTexture("_MetallicGlossMap");
                            if (myTexture != null)
                            {
                                string textureAssetPath = AssetDatabase.GetAssetPath(myTexture);
                                TextureImporter textureImport = (TextureImporter)TextureImporter.GetAtPath(textureAssetPath);
                                if (textureImport != null)
                                {
                                    if (!textureImport.isReadable)
                                    {
                                        textureImport.isReadable = true;
                                        textureImport.SaveAndReimport();
                                        forceRequired = true;
                                        Debug.Log("✔ Forced Read/Write Enabled for Texture: " + myTexture.name + "\nof Material: " + mat.name + " of " + smr.name + " mesh.");
                                    }
                                }
                            }
                        }
                        if (mat.GetTexture("_OcclusionMap"))
                        {
                            Texture2D myTexture = (Texture2D)mat.GetTexture("_OcclusionMap");
                            if (myTexture != null)
                            {
                                string textureAssetPath = AssetDatabase.GetAssetPath(myTexture);
                                TextureImporter textureImport = (TextureImporter)TextureImporter.GetAtPath(textureAssetPath);
                                if (textureImport != null)
                                {
                                    if (!textureImport.isReadable)
                                    {
                                        textureImport.isReadable = true;
                                        textureImport.SaveAndReimport();
                                        forceRequired = true;
                                        Debug.Log("✔ Forced Read/Write Enabled for Texture: " + myTexture.name + "\nof Material: " + mat.name + " of " + smr.name + " mesh.");
                                    }
                                }
                            }
                        }
                    }
                }
                if (!forceRequired)
                {
                    Debug.Log("✔ All textures already have Read/Write Enabled.");
                }
            }
        }
    }
}