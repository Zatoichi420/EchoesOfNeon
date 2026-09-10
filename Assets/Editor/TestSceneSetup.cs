// One-time scene-authoring script for the Phase 1-3 verification test -
// builds Assets/Scenes/TestRange_Proto.unity entirely in code, the same
// batch-mode approach used for everything else in this project so far.
// Run via:
//   Unity.exe -batchmode -nographics -projectPath <path>
//     -executeMethod TestSceneSetup.BuildTestScene -quit -logFile <log>
using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using EchoesOfNeon.Player;
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
}
