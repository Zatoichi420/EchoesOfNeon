// Captures imported character prefabs inside Unity itself (orientation, scale, materials, pose) using a
// temporary scene that is never saved. Run WITHOUT -nographics (rendering needs a GPU):
//   Unity -batchmode -projectPath <path> -executeMethod CookieScreenshotCapture.CaptureAll     (Cookie)
//   Unity -batchmode -projectPath <path> -executeMethod CookieScreenshotCapture.CaptureMarcus (Marcus)
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CookieScreenshotCapture
{
    private const string OutDir = "/Users/orlandojohnson/Desktop/EchoesOfNeon_ConceptIdeas/Characters_3D";

    public static void CaptureAll() =>
        Capture("Assets/Prefabs/Cookie.prefab", "Assets/Models/Cookie/Cookie.fbx", "Walking", "Unity_Cookie", 0.4f);

    public static void CaptureMarcusBind() =>
        Capture("Assets/Prefabs/Marcus.prefab", "Assets/Models/Marcus/Marcus.fbx", "__none__", "Unity_MarcusBind", 0f);

    public static void CaptureMarcusTint()
    {
        // Diagnostic: force the body material bright red and read back the texture's import settings.
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Models/Marcus/Marcus.mat");
        var tex = mat.mainTexture as Texture2D;
        var path = AssetDatabase.GetAssetPath(tex);
        var ti = (TextureImporter)AssetImporter.GetAtPath(path);
        Debug.Log($"[CharShot] TEX {path} size={tex.width}x{tex.height} type={ti.textureType} readable={ti.isReadable} sRGB={ti.sRGBTexture} compression={ti.textureCompression} filter={tex.filterMode}");
        Debug.Log($"[CharShot] MAT color={mat.GetColor("_Color")} shaderKeywords={string.Join(",", mat.shaderKeywords)} mode={(mat.HasProperty("_Mode") ? mat.GetFloat("_Mode") : -1)}");
        mat.SetColor("_Color", new Color(1f, 0.1f, 0.1f));
        Capture("Assets/Prefabs/Marcus.prefab", "Assets/Models/Marcus/Marcus.fbx", "Walk", "Unity_MarcusTint", 0.3f);
        mat.SetColor("_Color", Color.white);
    }

    public static void CaptureMarcusUnlit() =>
        Capture("Assets/Prefabs/Marcus.prefab", "Assets/Models/Marcus/Marcus.fbx", "Walk", "Unity_MarcusUnlit", 0.3f, go =>
        {
            var atlas = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Models/Marcus/Marcus_Atlas.png");
            var m = new Material(Shader.Find("Unlit/Texture")) { mainTexture = atlas };
            foreach (var r in go.GetComponentsInChildren<SkinnedMeshRenderer>()) r.sharedMaterial = m;
        });

    public static void CaptureMarcus() =>
        Capture("Assets/Prefabs/Marcus.prefab", "Assets/Models/Marcus/Marcus.fbx", "Walk", "Unity_Marcus", 0.3f);

    private static void Capture(string prefabPath, string fbxPath, string clipKey, string prefix, float sampleTime, System.Action<GameObject> post = null)
    {
        Directory.CreateDirectory(OutDir);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Dark, neutral environment so flat surfaces do not mirror the default blue sky.
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.12f, 0.13f, 0.16f);
        RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        var character = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        character.transform.position = Vector3.zero;

        var clips = AssetDatabase.LoadAllAssetsAtPath(fbxPath)
            .OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__")).ToList();
        Debug.Log("[CharShot] clips: " + string.Join(", ", clips.Select(c => c.name)));
        var clip = clips.FirstOrDefault(c => c.name.Contains(clipKey));
        // Pose through the Animator (works for Humanoid retargeting); SampleAnimation collapses Humanoid rigs.
        var animator = character.GetComponentInChildren<Animator>();
        Debug.Log($"[CharShot] animator isHuman={animator.isHuman} avatarValid={(animator.avatar != null && animator.avatar.isValid)} clipFound={(clip != null)}");
        if (clip != null)
        {
            animator.Rebind();
            animator.Play(clip.name, 0, sampleTime / Mathf.Max(clip.length, 0.01f));
            animator.Update(0f);
        }

        post?.Invoke(character);
        var rends = character.GetComponentsInChildren<Renderer>();
        foreach (var r in rends)
            Debug.Log($"[CharShot] renderer {r.name} type={r.GetType().Name} enabled={r.enabled} boundsSize={r.bounds.size} shader={(r.sharedMaterial != null ? r.sharedMaterial.shader.name : "NULL")} mat={(r.sharedMaterial != null ? r.sharedMaterial.name : "NULL")} tex={(r.sharedMaterial != null && r.sharedMaterial.mainTexture != null ? r.sharedMaterial.mainTexture.name + ":" + AssetDatabase.GetAssetPath(r.sharedMaterial.mainTexture) : "none")} mats={r.sharedMaterials.Length}");
        var body = rends.Where(r => r is SkinnedMeshRenderer).ToArray(); if (body.Length == 0) body = rends;
        var b = body.Select(r => r.bounds).Aggregate((x, y) => { x.Encapsulate(y); return x; });
        Debug.Log($"[CharShot] world bounds center={b.center} size={b.size} min.y={b.min.y}");
        character.transform.position += new Vector3(0, -b.min.y, 0);
        b = body.Select(r => r.bounds).Aggregate((x, y) => { x.Encapsulate(y); return x; });

        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.transform.localScale = new Vector3(2, 1, 2);
        var rp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
        var gm = new Material(rp != null && rp.defaultMaterial != null ? rp.defaultMaterial : ground.GetComponent<Renderer>().sharedMaterial) { color = new Color(0.03f, 0.035f, 0.045f) };
        ground.GetComponent<Renderer>().sharedMaterial = gm;

        MakeLight("Key", new Vector3(-3, 4, -3), Quaternion.Euler(45, 35, 0), new Color(0.85f, 0.9f, 1f), 1.6f);
        MakeLight("Rim", new Vector3(3, 3, 3), Quaternion.Euler(30, 215, 0), new Color(0.2f, 0.6f, 1f), 1.2f);

        var camGO = new GameObject("Cam");
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.01f, 0.015f, 0.03f);
        cam.fieldOfView = 35;
        var c = b.center; float d = Mathf.Max(b.size.x, b.size.y, b.size.z) * 2.6f;

        // The character faces +Z: front 3/4 is from (-x,+z), rear 3/4 from (+x,-z).
        Shot(cam, prefix + "_1_side.png", c + new Vector3(d, 0.1f, 0), c);
        Shot(cam, prefix + "_2_front34.png", c + new Vector3(-d * 0.6f, 0.2f * d, d * 0.8f), c);
        Shot(cam, prefix + "_3_rear34.png", c + new Vector3(d * 0.6f, 0.2f * d, -d * 0.8f), c);
        Shot(cam, prefix + "_4_front.png", c + new Vector3(0, 0.05f * d, d), c);
        Debug.Log("[CharShot] DONE bounds size=" + b.size);
    }

    private static void MakeLight(string n, Vector3 p, Quaternion r, Color col, float intensity)
    {
        var go = new GameObject(n); go.transform.SetPositionAndRotation(p, r);
        var l = go.AddComponent<Light>(); l.type = LightType.Directional; l.color = col; l.intensity = intensity;
    }

    private static void Shot(Camera cam, string file, Vector3 pos, Vector3 target)
    {
        cam.transform.position = pos; cam.transform.LookAt(target);
        var rt = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt; cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); tex.Apply();
        File.WriteAllBytes(Path.Combine(OutDir, file), tex.EncodeToPNG());
        RenderTexture.active = null; cam.targetTexture = null; rt.Release();
        Debug.Log("[CharShot] wrote " + file);
    }
}
