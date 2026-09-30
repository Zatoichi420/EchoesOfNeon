using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace EchoesOfNeon.EditorTools
{
    /// <summary>Builds Assets/Prefabs/Marcus.prefab from Assets/Models/Marcus/Marcus.fbx (CC0 Quaternius
    /// "Animated Human"): Humanoid rig, recolored atlas material (black tactical, bald), a glowing
    /// mobility cane in the right hand, and dark smart glasses. Marcus is cane-only, never a phone
    /// (standing rule, 2026-09-22). Run: -executeMethod EchoesOfNeon.EditorTools.MarcusImportSetup.Run</summary>
    public static class MarcusImportSetup
    {
        private const string FbxPath = "Assets/Models/Marcus/Marcus.fbx";
        private const string AtlasPath = "Assets/Models/Marcus/Marcus_Atlas.png";
        private const string MatPath = "Assets/Models/Marcus/Marcus.mat";
        private const string ControllerPath = "Assets/Models/Marcus/Marcus.controller";
        private const string PrefabPath = "Assets/Prefabs/Marcus.prefab";
        private const float TargetHeight = 1.88f; // 6'2"
        private static readonly Vector3 CaneTilt = new Vector3(0, 0, 155f);

        public static void Run()
        {
            // Point-filtered so the 32x32 palette stays crisp.
            var ti = (TextureImporter)AssetImporter.GetAtPath(AtlasPath);
            ti.filterMode = FilterMode.Point; ti.mipmapEnabled = false; ti.SaveAndReimport();

            AssetDatabase.ImportAsset(FbxPath, ImportAssetOptions.ForceUpdate);
            var importer = (ModelImporter)AssetImporter.GetAtPath(FbxPath);
            importer.animationType = ModelImporterAnimationType.Human;
            importer.importAnimation = true;
            importer.SaveAndReimport();
            var defaults = importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations;
            foreach (var c in defaults) c.loopTime = true;
            importer.clipAnimations = defaults;
            importer.SaveAndReimport();

            var clips = AssetDatabase.LoadAllAssetsAtPath(FbxPath).OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__preview__")).ToList();
            Debug.Log("[MarcusImport] clips: " + string.Join(", ", clips.Select(c => $"{c.name} ({c.length:0.00}s)")));

            var atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(AtlasPath);
            var rp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            var shader = rp != null && rp.defaultMaterial != null ? rp.defaultMaterial.shader : Shader.Find("Standard");
            var mat = new Material(shader) { name = "Marcus" };
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", atlas);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", atlas);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
            // Matte: flat low-poly palette should not mirror the sky (which turned black clothes vivid blue).
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0f);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0f);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
            if (mat.HasProperty("_SpecularHighlights")) mat.SetFloat("_SpecularHighlights", 0f);
            if (mat.HasProperty("_GlossyReflections")) mat.SetFloat("_GlossyReflections", 0f);
            mat.EnableKeyword("_SPECULARHIGHLIGHTS_OFF"); mat.EnableKeyword("_GLOSSYREFLECTIONS_OFF");
            AssetDatabase.CreateAsset(mat, MatPath);
            // Make the FBX itself use this material (renderer-level overrides on the prefab were not winning).
            foreach (var embedded in AssetDatabase.LoadAllAssetsAtPath(FbxPath).OfType<Material>().ToList())
            {
                Debug.Log("[MarcusImport] remapping embedded material: " + embedded.name);
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(embedded), mat);
            }
            importer.SaveAndReimport();

            // Animator: Idle default, Idle<->Walk<->Run driven by a float "Speed".
            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            var sm = controller.layers[0].stateMachine;
            AnimatorState Find(string key) {
                var clip = clips.FirstOrDefault(c => c.name.ToLower().Contains(key));
                if (clip == null) return null;
                var st = sm.AddState(clip.name); st.motion = clip; return st;
            }
            var idle = Find("idle"); var walk = Find("walk"); var run = Find("run");
            foreach (var c in clips) if (sm.states.All(s => s.state.motion != c)) sm.AddState(c.name).motion = c;
            if (idle != null) sm.defaultState = idle;
            void Link(AnimatorState a, AnimatorState b, AnimatorConditionMode m, float th) {
                if (a == null || b == null) return;
                var t = a.AddTransition(b); t.hasExitTime = false; t.duration = 0.15f; t.AddCondition(m, th, "Speed");
            }
            Link(idle, walk, AnimatorConditionMode.Greater, 0.1f);
            Link(walk, idle, AnimatorConditionMode.Less, 0.1f);
            Link(walk, run, AnimatorConditionMode.Greater, 3.0f);
            Link(run, walk, AnimatorConditionMode.Less, 3.0f);

            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
            var root = new GameObject("Marcus");
            var model = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
            model.name = "Model"; model.transform.SetParent(root.transform, false);
            foreach (var r in model.GetComponentsInChildren<Renderer>()) r.sharedMaterials = r.sharedMaterials.Select(_ => mat).ToArray();
            var animator = model.GetComponent<Animator>();
            if (animator == null) animator = model.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            // Scale to 6'2"; slightly wider/deeper for the heavyset 250 lb build.
            var b0 = Bounds(model);
            float k = TargetHeight / b0.size.y;
            model.transform.localScale = new Vector3(k * 1.18f, k, k * 1.22f);
            var b1 = Bounds(model);
            model.transform.position += new Vector3(0, -b1.min.y, 0);
            Debug.Log($"[MarcusImport] native height={b0.size.y:0.00} scale k={k:0.000} final size={Bounds(model).size}");

            AddProps(model);
            var stab = root.AddComponent<EchoesOfNeon.Player.HeldCaneStabilizer>();
            stab.cane = _cane; stab.facing = root.transform;
            System.IO.Directory.CreateDirectory("Assets/Prefabs");
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            Debug.Log("[MarcusImport] DONE prefab=" + PrefabPath);
        }

        private static Bounds Bounds(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>();
            var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds); return b;
        }

        private static void SetColor(Material m, Color c)
        { if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c); if (m.HasProperty("_Color")) m.SetColor("_Color", c); }
        private static void SetEmission(Material m, Color c)
        { m.EnableKeyword("_EMISSION"); if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", c);
          m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None; }

        private static Transform MeterAnchor(string name, Transform parent)
        {
            var t = new GameObject(name).transform; t.SetParent(parent, false);
            var ls = parent.lossyScale;
            t.localScale = new Vector3(1f / ls.x, 1f / ls.y, 1f / ls.z);
            return t;
        }
        private static Transform Prim(PrimitiveType type, string name, Transform parent, Material m)
        {
            var g = GameObject.CreatePrimitive(type); g.name = name;
            Object.DestroyImmediate(g.GetComponent<Collider>());
            g.transform.SetParent(parent, false); g.GetComponent<Renderer>().sharedMaterial = m;
            return g.transform;
        }

        private static Transform FindBone(Transform root, string name) =>
            root.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name.EndsWith(name));

        private static Transform _cane;
        private static void AddProps(GameObject model)
        {
            // Use the project's own default render-pipeline shader (URP shader names aren't reliable here).
            var rp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            var sh = rp != null && rp.defaultMaterial != null ? rp.defaultMaterial.shader : Shader.Find("Standard");
            var glow = new Material(sh) { name = "MarcusCaneGlow" };
            SetColor(glow, new Color(0.1f, 1f, 0.7f)); SetEmission(glow, new Color(0.1f, 1f, 0.7f) * 2.5f);
            var dark = new Material(sh) { name = "MarcusProp" };
            SetColor(dark, new Color(0.03f, 0.03f, 0.035f));
            AssetDatabase.CreateAsset(glow, "Assets/Models/Marcus/MarcusCaneGlow.mat");
            AssetDatabase.CreateAsset(dark, "Assets/Models/Marcus/MarcusProp.mat");

            var hand = FindBone(model.transform, "RightHand");
            var head = FindBone(model.transform, "Head");
            Debug.Log($"[MarcusImport] hand={(hand ? hand.name : "MISSING")} head={(head ? head.name : "MISSING")} handLossy={(hand ? hand.lossyScale.ToString() : "-")}");
            if (hand != null)
            {
                // Anchor cancels the bone's (large, FBX-imposed) scale so children are authored in real meters.
                var cane = MeterAnchor("MobilityCane", hand); _cane = cane;
                cane.localRotation = Quaternion.Euler(CaneTilt);
                var shaft = Prim(PrimitiveType.Cylinder, "Shaft", cane, dark);
                shaft.localScale = new Vector3(0.024f, 0.62f, 0.024f);             // 1.24 m long, 2.4 cm thick
                shaft.localPosition = new Vector3(0, -0.45f, 0);                    // grip near the top
                var band = Prim(PrimitiveType.Cylinder, "GlowBand", cane, glow);
                band.localScale = new Vector3(0.03f, 0.14f, 0.03f);
                band.localPosition = new Vector3(0, -0.95f, 0);                     // glowing band near the ground tip
            }
            if (head != null)
            {
                // World-placed (bind pose, model faces +Z): eye level ~9 cm above the head bone, 9 cm forward.
                var gl = MeterAnchor("SmartGlassesAnchor", head);
                gl.position = head.position + model.transform.up * 0.06f + model.transform.forward * 0.14f;
                gl.rotation = model.transform.rotation;
                var glasses = Prim(PrimitiveType.Cube, "SmartGlasses", gl, dark);
                glasses.localScale = new Vector3(0.17f, 0.04f, 0.05f);
            }
        }
    }
}
