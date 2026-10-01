// Builds Assets/Scenes/SideScrollProto.unity: the 2.5D side-scroller prototype graybox (ground, a pit, a platform,
// three enemies, Marcus with cane/edge sensor, Cookie). Same batch-mode convention as TestSceneSetup.
//   Unity -batchmode -nographics -projectPath <path> -executeMethod SideScrollSceneSetup.Build -quit -logFile <log>
using System.IO;
using System.Linq;
using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using EchoesOfNeon.Player;
using EchoesOfNeon.SideScroll;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SideScrollSceneSetup
{
    public const string ScenePath = "Assets/Scenes/SideScrollProto.unity";

    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var sun = new GameObject("Directional Light").AddComponent<Light>();
        sun.type = LightType.Directional; sun.intensity = 1.1f; sun.transform.rotation = Quaternion.Euler(40, -25, 0);

        // --- Level graybox (top surface at y = 0). Pit between x=30 and x=36; raised platform over the second stretch. ---
        Block("Ground_A", -12f, 30f, 0f, new Color(0.15f, 0.2f, 0.25f));
        Block("Ground_B", 36f, 90f, 0f, new Color(0.15f, 0.2f, 0.25f));
        Block("Platform_1", 44f, 49f, 2.5f, new Color(0.0f, 0.5f, 0.55f), thickness: 0.4f);
        Block("Wall_Left", -13f, -12f, 6f, Color.black, thickness: 8f);

        // --- Managers (same singletons as the first-person scene) ---
        new GameObject("InputManager").AddComponent<InputManager>();
        var acc = new GameObject("AccessibilityManager"); acc.AddComponent<AccessibilityManager>(); acc.AddComponent<TestAnnouncer>();
        new GameObject("AcousticEventSystem").AddComponent<AcousticEventSystem>();

        // --- Marcus ---
        var marcus = new GameObject("Marcus");
        marcus.transform.position = new Vector3(0f, 0.05f, 0f);
        var cc = marcus.AddComponent<CharacterController>(); cc.center = new Vector3(0, 0.9f, 0); cc.height = 1.8f; cc.radius = 0.35f;
        var player = marcus.AddComponent<SideScrollPlayer>();
        marcus.AddComponent<SideScrollEdgeSensor>();
        marcus.AddComponent<SideScrollAbilities>();
        new GameObject("Progression").AddComponent<SideScrollProgression>();
        var marcusPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Marcus.prefab");
        if (marcusPrefab != null)
        {
            var body = (GameObject)PrefabUtility.InstantiatePrefab(marcusPrefab, marcus.transform);
            body.name = "MarcusBody"; body.transform.localPosition = Vector3.zero; body.transform.localRotation = Quaternion.identity;
            marcus.AddComponent<PlayerAnimationBridge>().animator = body.GetComponentInChildren<Animator>();
        }
        else Debug.LogWarning("[SideScrollSetup] Marcus.prefab missing - graybox capsule used.");

        // --- Camera + audio listener (listener on the plane so stereo pan == world X) ---
        var camGO = new GameObject("Main Camera"); camGO.tag = "MainCamera";
        camGO.AddComponent<Camera>(); camGO.AddComponent<AudioListener>();
        var follow = camGO.AddComponent<SideScrollCamera>(); follow.target = marcus.transform;
        camGO.transform.position = new Vector3(0, 2.5f, -14f);

        // --- Cookie ---
        var cookiePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Cookie.prefab");
        if (cookiePrefab != null)
        {
            var cookie = (GameObject)PrefabUtility.InstantiatePrefab(cookiePrefab);
            cookie.name = "Cookie"; cookie.transform.position = new Vector3(-1.6f, 0f, 0f);
            var sc = cookie.AddComponent<SideScrollCookie>();
            sc.leader = marcus.transform; sc.animator = cookie.GetComponentInChildren<Animator>();
            sc.pawsteps = Load("Assets/Audio/Footsteps", "cookie_pawstep");
            sc.collar = Load("Assets/Audio/Companion", "cookie_collar");
            player.cookie = sc;
        }
        else Debug.LogWarning("[SideScrollSetup] Cookie.prefab missing.");

        // --- Enemies ---
        Enemy("Enforcer_A", 12f, player, 4f, EnemyKind.PipeThug);
        Enemy("Enforcer_B", 22f, player, 2f, EnemyKind.PipeThug);
        Enemy("KnifeRunner_1", 40f, player, 2f, EnemyKind.KnifeRunner);
        Enemy("BottleThrower_1", 52f, player, 2f, EnemyKind.BottleThrower);
        Enemy("ShieldGuard_1", 60f, player, 2f, EnemyKind.ShieldGuard);
        Enemy("ZipGunPunk_1", 66f, player, 2f, EnemyKind.ZipGunPunk);
        Enemy("AlleyRat_1", 72f, player, 1f, EnemyKind.AlleyRat);
        Enemy("AlleyRat_2", 73.5f, player, 1f, EnemyKind.AlleyRat);
        Enemy("AlleyRat_3", 75f, player, 1f, EnemyKind.AlleyRat);
        Enemy("DroneHawk_1", 80f, player, 2f, EnemyKind.DroneHawk);
        Enemy("BruteBouncer_1", 86f, player, 1f, EnemyKind.BruteBouncer);

        // --- Recovery + exploration: found by ear (locator beeps / hum) ---
        Locator<HealthPickup>("MedCell_1", 8f, 0.6f, player);
        Locator<HealthPickup>("MedCell_2", 38f, 0.6f, player);
        Locator<RestPoint>("RestPoint_Workshop", 2.5f, 0.1f, player);
        var rest2 = Locator<RestPoint>("RestPoint_Alley", 37.5f, 0.1f, player); rest2.label = "Alley nook";
        var b1 = Locator<ExplorationBeacon>("Beacon_Rooftop", 46.5f, 3.0f, player); b1.description = "Rooftop vantage found";
        var b2 = Locator<ExplorationBeacon>("Beacon_Alley", 15f, 0.6f, player); b2.description = "Hidden alley found";

        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log("[SideScrollSetup] BUILT " + ScenePath);
    }

    private static AudioClip[] Load(string folder, string prefix) =>
        AssetDatabase.FindAssets("t:AudioClip " + prefix, new[] { folder }).Select(g => AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(g))).Where(c => c != null).ToArray();

    private static void Block(string name, float x0, float x1, float topY, Color color, float thickness = 1f)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube); g.name = name;
        g.transform.position = new Vector3((x0 + x1) / 2f, topY - thickness / 2f, 0f);
        g.transform.localScale = new Vector3(x1 - x0, thickness, 6f);
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")); mat.color = color;
        g.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private static T Locator<T>(string name, float x, float y, SideScrollPlayer player) where T : LocatorObject
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Sphere); g.name = name;
        Object.DestroyImmediate(g.GetComponent<Collider>());
        g.transform.position = new Vector3(x, y, 0f); g.transform.localScale = Vector3.one * 0.4f;
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        mat.color = typeof(T) == typeof(HealthPickup) ? new Color(0.2f, 0.9f, 0.3f) : typeof(T) == typeof(RestPoint) ? new Color(0.3f, 0.5f, 1f) : new Color(1f, 0.8f, 0.2f);
        g.GetComponent<Renderer>().sharedMaterial = mat;
        var c = g.AddComponent<T>(); c.player = player;
        return c;
    }

    private static void Enemy(string name, float x, SideScrollPlayer player, float patrolHalf, EnemyKind kind = EnemyKind.PipeThug)
    {
        var root = new GameObject(name); root.transform.position = new Vector3(x, 0.05f, 0f);
        var cc = root.AddComponent<CharacterController>(); cc.center = new Vector3(0, 0.9f, 0); cc.height = 1.8f; cc.radius = 0.4f;
        var vis = GameObject.CreatePrimitive(PrimitiveType.Capsule); vis.name = "Body";
        Object.DestroyImmediate(vis.GetComponent<Collider>());
        vis.transform.SetParent(root.transform, false); vis.transform.localPosition = new Vector3(0, 0.9f, 0); vis.transform.localScale = new Vector3(0.8f, 0.9f, 0.8f);
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")); mat.color = new Color(0.8f, 0.2f, 0.1f);
        vis.GetComponent<Renderer>().sharedMaterial = mat;
        var e = root.AddComponent<SideScrollEnemy>(); e.target = player; e.patrolHalfWidth = patrolHalf; e.kind = kind;
    }
}
