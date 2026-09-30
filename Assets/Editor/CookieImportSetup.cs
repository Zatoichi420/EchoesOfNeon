using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace EchoesOfNeon.EditorTools
{
    /// <summary>Imports Assets/Models/Cookie/Cookie.fbx with sensible settings, then builds
    /// Assets/Prefabs/Cookie.prefab with an Animator (Idle default, Walking) so the model is
    /// usable and verifiable without any GUI. Run via -executeMethod
    /// EchoesOfNeon.EditorTools.CookieImportSetup.Run</summary>
    public static class CookieImportSetup
    {
        private const string FbxPath = "Assets/Models/Cookie/Cookie.fbx";
        private const string ControllerPath = "Assets/Models/Cookie/Cookie.controller";
        private const string PrefabPath = "Assets/Prefabs/Cookie.prefab";

        public static void Run()
        {
            AssetDatabase.ImportAsset(FbxPath, ImportAssetOptions.ForceUpdate);
            var importer = (ModelImporter)AssetImporter.GetAtPath(FbxPath);
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.importAnimation = true;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.SaveAndReimport();

            var clips = AssetDatabase.LoadAllAssetsAtPath(FbxPath)
                .OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__")).ToList();
            Debug.Log("[CookieImport] clips: " + string.Join(", ", clips.Select(c => $"{c.name} ({c.length:0.00}s)")));

            // Loop the clips in the importer so Idle/Walking cycle.
            var defaults = importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations;
            foreach (var c in defaults) c.loopTime = true;
            importer.clipAnimations = defaults;
            importer.SaveAndReimport();
            clips = AssetDatabase.LoadAllAssetsAtPath(FbxPath).OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__preview__")).ToList();

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            var sm = controller.layers[0].stateMachine;
            AnimatorState idle = null;
            foreach (var clip in clips)
            {
                var st = sm.AddState(clip.name);
                st.motion = clip;
                if (clip.name.ToLower().Contains("idle")) idle = st;
            }
            if (idle != null) sm.defaultState = idle;
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            var walkState = sm.states.Select(x => x.state).FirstOrDefault(x => x.name.ToLower().Contains("walk"));
            if (idle != null && walkState != null)
            {
                var t1 = idle.AddTransition(walkState); t1.hasExitTime = false; t1.duration = 0.15f; t1.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
                var t2 = walkState.AddTransition(idle); t2.hasExitTime = false; t2.duration = 0.15f; t2.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            }

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
            // Root "Cookie" faces +Z (Unity forward). The Blender export leaves the dog facing -X,
            // so the model lives on a child rotated 90 degrees about Y; the Animator is on that child.
            var instance = new GameObject("Cookie");
            var modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            modelInstance.name = "Model";
            modelInstance.transform.SetParent(instance.transform, false);
            modelInstance.transform.localRotation = Quaternion.Euler(0, 90, 0);
            var animator = modelInstance.GetComponent<Animator>();
            if (animator == null) animator = modelInstance.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            var renderers = instance.GetComponentsInChildren<Renderer>();
            Debug.Log($"[CookieImport] renderers={renderers.Length} materials=" +
                string.Join(", ", renderers.SelectMany(r => r.sharedMaterials).Where(m => m != null).Select(m => m.name).Distinct()));
            var b = renderers.Length > 0 ? renderers.Select(r => r.bounds).Aggregate((x, y) => { x.Encapsulate(y); return x; }) : new Bounds();
            Debug.Log($"[CookieImport] bounds size={b.size} (meters)");

            System.IO.Directory.CreateDirectory("Assets/Prefabs");
            PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath);
            Object.DestroyImmediate(instance);
            AssetDatabase.SaveAssets();
            Debug.Log("[CookieImport] DONE prefab=" + PrefabPath + " states=" + sm.states.Length);
        }
    }
}
