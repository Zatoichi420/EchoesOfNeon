// One-time scene-authoring script for the Phase 1-3 verification test -
// builds Assets/Scenes/TestRange_Proto.unity entirely in code, the same
// batch-mode approach used for everything else in this project so far.
// Run via:
//   Unity.exe -batchmode -nographics -projectPath <path>
//     -executeMethod TestSceneSetup.BuildTestScene -quit -logFile <log>
using EchoesOfNeon.Accessibility;
using EchoesOfNeon.AI;
using EchoesOfNeon.Core;
using EchoesOfNeon.Optics;
using EchoesOfNeon.Player;
using EchoesOfNeon.UI;
using EchoesOfNeon.Weapons;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TestSceneSetup
{
    public static void BuildTestScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // --- Lighting, so the scene isn't pitch black for anyone looking at a screenshot ---
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // --- Ground + a couple of landmark cubes ---
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(5f, 1f, 5f); // 50x50 units

        CreateLandmarkCube("Landmark_North", new Vector3(0f, 1f, 10f));
        CreateLandmarkCube("Landmark_East", new Vector3(10f, 1f, 0f));

        // --- Manager singletons ---
        var inputGO = new GameObject("InputManager");
        inputGO.AddComponent<InputManager>();

        var accessibilityGO = new GameObject("AccessibilityManager");
        accessibilityGO.AddComponent<AccessibilityManager>();
        accessibilityGO.AddComponent<TestAnnouncer>();

        // --- Player ---
        var playerGO = new GameObject("Player");
        playerGO.transform.position = new Vector3(0f, 1.1f, 0f);
        var characterController = playerGO.AddComponent<CharacterController>();
        characterController.center = new Vector3(0f, 0.9f, 0f);
        characterController.height = 1.8f;
        characterController.radius = 0.35f;
        var playerController = playerGO.AddComponent<TacticalPlayerController>();

        var cameraPivotGO = new GameObject("CameraPivot");
        cameraPivotGO.transform.SetParent(playerGO.transform);
        cameraPivotGO.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        cameraPivotGO.AddComponent<Camera>();
        cameraPivotGO.AddComponent<AudioListener>();
        cameraPivotGO.tag = "MainCamera";

        // TacticalPlayerController.cameraPivot is a private [SerializeField] -
        // set it via SerializedObject, the correct way to assign a private
        // serialized field from editor tooling rather than making it public
        // just for this one assignment.
        var so = new SerializedObject(playerController);
        so.FindProperty("cameraPivot").objectReferenceValue = cameraPivotGO.transform;
        so.ApplyModifiedProperties();

        // --- Oculus Sensory Suite (Phase 4) - wired in for the first time,
        // predates this test scene. originTransform is the same private-
        // [SerializeField] pattern as cameraPivot above. ---
        var oculusSuite = playerGO.AddComponent<OculusSensorySuite>();
        var oculusSo = new SerializedObject(oculusSuite);
        oculusSo.FindProperty("originTransform").objectReferenceValue = cameraPivotGO.transform;
        oculusSo.ApplyModifiedProperties();

        // --- Vanguard 9 Suppressed Pistol (Phase 6) ---
        var weaponEmitter = playerGO.AddComponent<AcousticEmitter>();
        var weaponEmitterSo = new SerializedObject(weaponEmitter);
        weaponEmitterSo.FindProperty("eventType").enumValueIndex = (int)AcousticEventType.Gunfire;
        weaponEmitterSo.ApplyModifiedProperties();

        var weapon = playerGO.AddComponent<BallisticWeapon>();
        var weaponSo = new SerializedObject(weapon);
        weaponSo.FindProperty("muzzle").objectReferenceValue = cameraPivotGO.transform;
        weaponSo.ApplyModifiedProperties();

        // --- Sound Visualizer Compass (Phase 5) - subscribes to
        // AcousticEventSystem, needs a Canvas to draw into. ---
        var canvasGO = new GameObject("HUDCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        var compassGO = new GameObject("SoundVisualizerCompass");
        compassGO.transform.SetParent(canvasGO.transform, false);
        var compass = compassGO.AddComponent<SoundVisualizerCompass>();
        var compassSo = new SerializedObject(compass);
        compassSo.FindProperty("listenerTransform").objectReferenceValue = cameraPivotGO.transform;
        compassSo.ApplyModifiedProperties();

        // --- Acoustic Event System (Phase 5) - singleton, no scene wiring
        // needed beyond existing (creates itself if absent, but explicit here
        // for parity with the other manager singletons). ---
        var acousticGO = new GameObject("AcousticEventSystem");
        acousticGO.AddComponent<AcousticEventSystem>();

        // --- A couple of test emitters so the compass/sonar have something
        // to react to without needing enemy AI (Phase 6) yet. ---
        CreateTestEmitter("TestEmitter_Gunfire", new Vector3(10f, 1f, 0f), AcousticEventType.Gunfire, 20f);
        CreateTestEmitter("TestEmitter_Mechanical", new Vector3(0f, 1f, 10f), AcousticEventType.Mechanical, 15f);

        // --- Test enemy (Phase 6) - patrols between two points, reacts to
        // sonar pings and acoustic events, damageable by the pistol. ---
        CreateTestEnemy(playerGO.transform, new Vector3(-8f, 1f, -8f), new Vector3(-8f, 1f, 8f));

        // --- Save scene + register it in Build Settings ---
        const string scenePath = "Assets/Scenes/TestRange_Proto.unity";
        EditorSceneManager.SaveScene(scene, scenePath);

        var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        buildScenes.RemoveAll(s => s.path == scenePath);
        buildScenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = buildScenes.ToArray();

        Debug.Log("[TestSceneSetup] Test scene built and saved to " + scenePath);
    }

    private static void CreateLandmarkCube(string name, Vector3 position)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = Vector3.one * 2f;
    }

    private static void CreateTestEmitter(string name, Vector3 position, AcousticEventType type, float loudness)
    {
        var go = new GameObject(name);
        go.transform.position = position;
        var emitter = go.AddComponent<AcousticEmitter>();
        var so = new SerializedObject(emitter);
        so.FindProperty("eventType").enumValueIndex = (int)type;
        so.FindProperty("loudness").floatValue = loudness;
        so.ApplyModifiedProperties();
        go.AddComponent<TestEmitterLoop>();
    }

    private static void CreateTestEnemy(Transform playerTransform, Vector3 patrolA, Vector3 patrolB)
    {
        var enemyGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemyGO.name = "TestEnemy";
        enemyGO.transform.position = patrolA;

        var eyeGO = new GameObject("Eye");
        eyeGO.transform.SetParent(enemyGO.transform, false);
        eyeGO.transform.localPosition = new Vector3(0f, 0.6f, 0f);

        var pointAGO = new GameObject("PatrolPoint_A");
        pointAGO.transform.position = patrolA;
        var pointBGO = new GameObject("PatrolPoint_B");
        pointBGO.transform.position = patrolB;

        var emitter = enemyGO.AddComponent<AcousticEmitter>();
        var emitterSo = new SerializedObject(emitter);
        emitterSo.FindProperty("eventType").enumValueIndex = (int)AcousticEventType.Mechanical;
        emitterSo.FindProperty("loudness").floatValue = 12f;
        emitterSo.ApplyModifiedProperties();

        var ai = enemyGO.AddComponent<TacticalEnemyAI>();
        var aiSo = new SerializedObject(ai);
        var patrolPointsProp = aiSo.FindProperty("patrolPoints");
        patrolPointsProp.arraySize = 2;
        patrolPointsProp.GetArrayElementAtIndex(0).objectReferenceValue = pointAGO.transform;
        patrolPointsProp.GetArrayElementAtIndex(1).objectReferenceValue = pointBGO.transform;
        aiSo.FindProperty("eye").objectReferenceValue = eyeGO.transform;
        aiSo.FindProperty("playerTransform").objectReferenceValue = playerTransform;
        aiSo.ApplyModifiedProperties();
    }
}
