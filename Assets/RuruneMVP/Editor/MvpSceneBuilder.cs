using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuruneMVP.Editor
{
    public static class MvpSceneBuilder
    {
        [MenuItem("Tools/Rurune MVP/Create Starter Scene")]
        public static void CreateStarterScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var characterRoot = new GameObject("CharacterRoot");
            var systems = new GameObject("MVP Systems");
            var motion = systems.AddComponent<CharacterMotionController>();
            var expression = systems.AddComponent<CharacterExpressionController>();
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.nearClipPlane = 0.05f;
            var orbit = cameraObject.AddComponent<OrbitCamera>();
            var cameraSettings = new SerializedObject(orbit);
            cameraSettings.FindProperty("target").objectReferenceValue = characterRoot.transform;
            cameraSettings.ApplyModifiedPropertiesWithoutUndo();
            orbit.ResetView();
            var lightObject = new GameObject("Directional Light", typeof(Light));
            Light light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            lightObject.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(2f, 1f, 2f);
            var panelObject = new GameObject("Control Panel");
            var panel = new SerializedObject(panelObject.AddComponent<MvpControlPanel>());
            panel.FindProperty("orbitCamera").objectReferenceValue = orbit;
            panel.FindProperty("motionController").objectReferenceValue = motion;
            panel.FindProperty("expressionController").objectReferenceValue = expression;
            panel.ApplyModifiedPropertiesWithoutUndo();
            if (!AssetDatabase.IsValidFolder("Assets/RuruneMVP/Scenes"))
                System.IO.Directory.CreateDirectory("Assets/RuruneMVP/Scenes");
            AssetDatabase.Refresh();
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/RuruneMVP/Scenes/MvpStarter.unity");
            EditorSceneManager.SaveScene(scene, path);
            Selection.activeGameObject = systems;
            Debug.Log("Starter scene created. Import your licensed model locally, then configure MVP Systems.");
        }
    }
}
