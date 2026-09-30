// Builds Assets/Scenes/CompanionTest.unity (Marcus walks/runs a loop, Cookie follows) and simulates it
// deterministically (no Play Mode) to verify movement, spacing and animation states.
//   Build:    -executeMethod CompanionTestScene.Build      (can use -nographics)
//   Simulate: -executeMethod CompanionTestScene.Simulate   (needs rendering: no -nographics)
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EchoesOfNeon.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CompanionTestScene
{
    private const string ScenePath = "Assets/Scenes/CompanionTest.unity";

    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane); ground.name = "Ground"; ground.transform.localScale = new Vector3(3, 1, 3);
        var l = new GameObject("Sun").AddComponent<Light>(); l.type = LightType.Directional; l.intensity = 1.2f; l.transform.rotation = Quaternion.Euler(50, -30, 0);
        var cam = new GameObject("Main Camera"); cam.tag = "MainCamera"; var c = cam.AddComponent<Camera>();
        cam.transform.position = new Vector3(0, 9, -12); cam.transform.rotation = Quaternion.Euler(35, 0, 0);

        var pts = new[] { new Vector3(-4, 0, -4), new Vector3(4, 0, -4), new Vector3(4, 0, 4), new Vector3(-4, 0, 4) };
        var wp = new List<Transform>(); var path = new GameObject("Waypoints");
        for (int i = 0; i < pts.Length; i++)
        {
            var m = GameObject.CreatePrimitive(PrimitiveType.Cube); m.name = "WP" + i; m.transform.SetParent(path.transform);
            m.transform.position = pts[i] + Vector3.up * 0.05f; m.transform.localScale = new Vector3(0.4f, 0.1f, 0.4f);
            Object.DestroyImmediate(m.GetComponent<Collider>()); wp.Add(m.transform);
        }

        var marcus = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Marcus.prefab"));
        marcus.transform.SetPositionAndRotation(pts[0], Quaternion.LookRotation(pts[1] - pts[0]));
        var w = marcus.AddComponent<WaypointWalker>();
        w.waypoints = new[] { wp[1], wp[2], wp[3], wp[0] };
        w.runToWaypoint = new[] { false, true, false, false };   // the second leg is run
        w.animator = marcus.GetComponentInChildren<Animator>();

        var cookie = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Cookie.prefab"));
        cookie.transform.SetPositionAndRotation(pts[0] + new Vector3(-1.2f, 0, -1.2f), Quaternion.LookRotation(pts[1] - pts[0]));
        var f = cookie.AddComponent<CompanionFollower>(); f.leader = marcus.transform; f.animator = cookie.GetComponentInChildren<Animator>();

        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log("[CompanionTest] BUILT " + ScenePath);
    }

    public static void Simulate()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        RenderSettings.skybox = null; RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat; RenderSettings.ambientLight = new Color(0.4f, 0.4f, 0.45f);
        var w = Object.FindFirstObjectByType<WaypointWalker>(); var f = Object.FindFirstObjectByType<CompanionFollower>();
        var mA = w.animator; var cA = f.animator; mA.Rebind(); cA.Rebind();

        var cam = new GameObject("TopCam").AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = 8; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.05f, 0.06f, 0.08f);
        cam.transform.position = new Vector3(0, 20, 0); cam.transform.rotation = Quaternion.Euler(90, 0, 0);

        const float dt = 1f / 30f; const int steps = 1500; // 50 s
        float minSep = 1e9f, maxSep = 0, sumSep = 0, maxCookieSpeed = 0, maxMarcusSpeed = 0;
        var mStates = new HashSet<string>(); var cStates = new HashSet<string>();
        var lastWp = -1; int frame = 0;
        for (int i = 0; i <= steps; i++)
        {
            w.Tick(dt); f.Tick(dt); mA.Update(dt); cA.Update(dt);
            float sep = Vector3.Distance(w.transform.position, f.transform.position);
            if (i > 60) { minSep = Mathf.Min(minSep, sep); maxSep = Mathf.Max(maxSep, sep); }
            sumSep += sep; maxCookieSpeed = Mathf.Max(maxCookieSpeed, f.CurrentSpeed); maxMarcusSpeed = Mathf.Max(maxMarcusSpeed, w.CurrentSpeed);
            var mi = mA.GetCurrentAnimatorClipInfo(0); if (mi.Length > 0) mStates.Add(mi[0].clip.name.Split('|').Last());
            var ci = cA.GetCurrentAnimatorClipInfo(0); if (ci.Length > 0) cStates.Add(ci[0].clip.name.Split('|').Last());
            if (i % 150 == 0) { Shot(cam, $"/tmp/companion_{frame++:00}.png");
                Debug.Log($"[CompanionTest] t={i * dt:0.0}s marcus={w.transform.position:F1} speed={w.CurrentSpeed:0.0} cookie={f.transform.position:F1} speed={f.CurrentSpeed:0.0} sep={sep:0.00} wp={w.TargetIndex}"); }
            lastWp = w.TargetIndex;
        }
        Debug.Log($"[CompanionTest] separation min={minSep:0.00} max={maxSep:0.00} avg={sumSep / (steps + 1):0.00} | Marcus maxSpeed={maxMarcusSpeed:0.0} Cookie maxSpeed={maxCookieSpeed:0.0}");
        Debug.Log("[CompanionTest] Marcus animation clips seen: " + string.Join(", ", mStates));
        Debug.Log("[CompanionTest] Cookie animation clips seen: " + string.Join(", ", cStates));
        bool pass = minSep > 0.9f && maxSep < 4.5f && mStates.Any(s => s == "Idle") && mStates.Any(s => s == "Walk") && mStates.Any(s => s == "Run")
                    && cStates.Any(s => s == "Idle") && cStates.Any(s => s == "Walking");
        Debug.Log("[CompanionTest] RESULT " + (pass ? "PASS" : "FAIL"));
    }

    private static void Shot(Camera cam, string file)
    {
        var rt = new RenderTexture(640, 640, 24); cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
        var t = new Texture2D(640, 640, TextureFormat.RGB24, false); t.ReadPixels(new Rect(0, 0, 640, 640), 0, 0); t.Apply();
        File.WriteAllBytes(file, t.EncodeToPNG()); RenderTexture.active = null; cam.targetTexture = null; rt.Release();
    }
}
