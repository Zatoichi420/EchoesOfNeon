// Movement test: steps each character's animation clips through the Animator and saves evenly spaced
// side-view frames, so the motion can be inspected without Play Mode. Run WITHOUT -nographics:
//   Unity -batchmode -projectPath <path> -executeMethod MovementFilmstrip.Run -quit -logFile <log>
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MovementFilmstrip
{
    private const string OutDir = "/tmp/filmstrip";
    private const int Frames = 6, W = 480, H = 360;

    public static void Run()
    {
        Directory.CreateDirectory(OutDir);
        Strip("Assets/Prefabs/Marcus.prefab", "Assets/Models/Marcus/Marcus.fbx", "Marcus", new[] { "Idle", "Walk", "Run" });
        Strip("Assets/Prefabs/Cookie.prefab", "Assets/Models/Cookie/Cookie.fbx", "Cookie", new[] { "Idle", "Walking" });
        Debug.Log("[Filmstrip] DONE");
    }

    private static void Strip(string prefabPath, string fbxPath, string who, string[] keys)
    {
        var clips = AssetDatabase.LoadAllAssetsAtPath(fbxPath).OfType<AnimationClip>()
            .Where(c => !c.name.StartsWith("__preview__")).ToList();
        foreach (var key in keys)
        {
            var clip = clips.FirstOrDefault(c => c.name.Contains(key));
            if (clip == null) { Debug.Log($"[Filmstrip] {who}: no clip '{key}'"); continue; }

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.skybox = null; RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.35f, 0.36f, 0.4f);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
            var animator = go.GetComponentInChildren<Animator>();
            animator.applyRootMotion = false; animator.Rebind();

            var l = new GameObject("L").AddComponent<Light>(); l.type = LightType.Directional; l.intensity = 1.4f;
            l.transform.rotation = Quaternion.Euler(50, -30, 0);

            var cam = new GameObject("C").AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.10f, 0.11f, 0.14f); cam.fieldOfView = 30;

            // Frame once on the first pose so the camera stays fixed and any drift shows up.
            animator.Play(clip.name, 0, 0f); animator.Update(0f);
            var body = go.GetComponentsInChildren<SkinnedMeshRenderer>();
            var b = body.Select(r => r.bounds).Aggregate((x, y) => { x.Encapsulate(y); return x; });
            float d = Mathf.Max(b.size.x, b.size.y, b.size.z) * 2.4f;
            cam.transform.position = b.center + new Vector3(d, 0, 0); cam.transform.LookAt(b.center);

            float len = Mathf.Min(clip.length, key == "Idle" ? 3f : clip.length);
            for (int i = 0; i < Frames; i++)
            {
                float t = len * i / Frames;
                // Humanoid rigs step correctly through the Animator; Generic rigs (Cookie) do not in edit mode,
                // so those are sampled directly.
                if (animator.isHuman) { animator.Play(clip.name, 0, t / clip.length); animator.Update(0f); }
                else clip.SampleAnimation(animator.gameObject, t);
                foreach (var st in go.GetComponentsInChildren<EchoesOfNeon.Player.HeldCaneStabilizer>()) st.Apply();
                // Edit mode does not re-skin a SkinnedMeshRenderer between samples, so render a baked snapshot of the
                // deformed mesh instead (BakeMesh evaluates the real skinning, verified to move vertices up to ~21 cm).
                var ghosts = new System.Collections.Generic.List<GameObject>();
                foreach (var smr in animator.isHuman ? new SkinnedMeshRenderer[0] : go.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    var baked = new Mesh(); smr.BakeMesh(baked);
                    var g = new GameObject("ghost"); g.transform.SetParent(smr.transform, false);
                    g.AddComponent<MeshFilter>().sharedMesh = baked;
                    g.AddComponent<MeshRenderer>().sharedMaterials = smr.sharedMaterials;
                    smr.enabled = false; ghosts.Add(g);
                }
                Shot(cam, Path.Combine(OutDir, $"{who}_{key}_{i}.png"));
                foreach (var g in ghosts) { g.transform.parent.GetComponent<SkinnedMeshRenderer>().enabled = true; Object.DestroyImmediate(g); }
            }
            Debug.Log($"[Filmstrip] {who} {key}: clip {clip.name} length={clip.length:0.00}s frames={Frames}");
        }
    }

    private static void Shot(Camera cam, string file)
    {
        var rt = new RenderTexture(W, H, 24); cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
        var t = new Texture2D(W, H, TextureFormat.RGB24, false); t.ReadPixels(new Rect(0, 0, W, H), 0, 0); t.Apply();
        File.WriteAllBytes(file, t.EncodeToPNG());
        RenderTexture.active = null; cam.targetTexture = null; rt.Release();
    }
}
