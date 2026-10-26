using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuruneMVP.Editor
{
    public sealed class MvpQuickSetupWindow : EditorWindow
    {
        [SerializeField] private GameObject character;
        [SerializeField] private Animator animator;
        [SerializeField] private OrbitCamera orbit;
        [SerializeField] private CharacterMotionController motion;
        [SerializeField] private CharacterExpressionController expression;
        [SerializeField] private MvpControlPanel panel;
        [SerializeField] private GameObject ground;
        [SerializeField] private SkinnedMeshRenderer face;
        [SerializeField] private AnimationClip[] clips = new AnimationClip[0];
        private string search = "", shape = "", expressionLabel = "", message = "";
        private float weight;
        private Vector2 scroll;

        [MenuItem("Tools/Rurune MVP/Quick Setup")]
        public static void Open() { GetWindow<MvpQuickSetupWindow>("Rurune MVP Setup"); }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.HelpBox("Edit Mode only. Select the character root in Hierarchy, then use selection. " +
                "Setup saves a separate local scene copy. Purchased assets and existing expression presets are preserved.", MessageType.Info);
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorGUILayout.HelpBox("Stop Play Mode before configuring.", MessageType.Warning);
                EditorGUILayout.EndScrollView();
                return;
            }
            character = (GameObject)EditorGUILayout.ObjectField("Character root", character, typeof(GameObject), true);
            if (GUILayout.Button("Use Hierarchy selection and discover references")) Run(Discover);
            animator = (Animator)EditorGUILayout.ObjectField("Animator", animator, typeof(Animator), true);
            orbit = (OrbitCamera)EditorGUILayout.ObjectField("Orbit camera", orbit, typeof(OrbitCamera), true);
            motion = (CharacterMotionController)EditorGUILayout.ObjectField("Motion controller", motion, typeof(CharacterMotionController), true);
            expression = (CharacterExpressionController)EditorGUILayout.ObjectField("Expression controller", expression, typeof(CharacterExpressionController), true);
            panel = (MvpControlPanel)EditorGUILayout.ObjectField("Control panel", panel, typeof(MvpControlPanel), true);
            ground = (GameObject)EditorGUILayout.ObjectField("Ground (optional)", ground, typeof(GameObject), true);
            EditorGUILayout.HelpBox("Ground is not guessed. Drag the scene's Ground object here to place its top below the feet. " +
                "It will keep its current enabled/disabled state. Multiple candidates require manual selection.", MessageType.None);
            if (GUILayout.Button("1. Save local copy + connect + fit camera / ground")) Run(Setup);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Expression presets", EditorStyles.boldLabel);
            face = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Renderer", face, typeof(SkinnedMeshRenderer), true);
            search = EditorGUILayout.TextField("Name filter", search);
            if (face != null && face.sharedMesh != null)
            {
                var names = new List<string>();
                for (int i = 0; i < face.sharedMesh.blendShapeCount; i++)
                {
                    string name = face.sharedMesh.GetBlendShapeName(i);
                    if (name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) names.Add(name);
                }
                if (names.Count > 0)
                {
                    string[] options = new[] { "Select an existing BlendShape" }.Concat(names).ToArray();
                    int current = names.IndexOf(shape) + 1;
                    int next = EditorGUILayout.Popup("BlendShape", current, options);
                    if (next != current) shape = next == 0 ? "" : names[next - 1];
                }
                else EditorGUILayout.LabelField("No matching BlendShapes.");
            }
            EditorGUILayout.LabelField("Selected", shape);
            expressionLabel = EditorGUILayout.TextField("Button label", expressionLabel);
            weight = EditorGUILayout.FloatField("Weight (confirm on model)", weight);
            if (GUILayout.Button("2. Append expression preset")) Run(AddExpression);
            EditorGUILayout.HelpBox("No expression meaning or weight is inferred. Existing presets are retained. " +
                "For the already confirmed eye closure, select b_blink and enter 100. Multi-shape presets remain editable in Inspector.", MessageType.None);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Motion setup", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Create controller replaces the selected Animator's assignment in the local scene. " +
                "Use the original scene to recover the previous setup. Clip assets and their import settings are never edited.", MessageType.None);
            var serialized = new SerializedObject(this);
            EditorGUILayout.PropertyField(serialized.FindProperty("clips"), new GUIContent("Clips (select deliberately)"), true);
            serialized.ApplyModifiedProperties();
            EditorGUILayout.HelpBox("Assign only compatible, licensed motion clips. Creates a NEW local controller and assigns it to " +
                "the chosen Animator; it does not edit the old controller asset. Existing motion-button entries are replaced. " +
                "First clip becomes the default. VRChat FX/gestures are not automatically converted into body motions.", MessageType.Warning);
            if (GUILayout.Button("3. Create and assign app controller from clips")) Run(CreateController);
            if (GUILayout.Button("Alternative: list states from current controller")) Run(RegisterExistingStates);
            if (GUILayout.Button("Write local diagnostic report")) Run(WriteReport);
            if (!string.IsNullOrEmpty(message)) EditorGUILayout.HelpBox(message, MessageType.Info);
            EditorGUILayout.EndScrollView();
        }

        private void Run(Action action)
        {
            try { action(); }
            catch (Exception exception)
            {
                message = exception.Message;
                Debug.LogException(exception);
            }
        }
        private static T Only<T>(Scene scene) where T : Component
        {
            T[] matches = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true)).ToArray();
            return matches.Length == 1 ? matches[0] : null;
        }
        private void Discover()
        {
            if (Selection.activeGameObject != null) character = Selection.activeGameObject;
            RequireCharacter();
            Scene scene = character.scene;
            Animator[] animators = character.GetComponentsInChildren<Animator>(true);
            animator = animators.Length == 1 ? animators[0] : null;
            orbit = Only<OrbitCamera>(scene);
            motion = Only<CharacterMotionController>(scene);
            expression = Only<CharacterExpressionController>(scene);
            panel = Only<MvpControlPanel>(scene);
            var renderers = character.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(renderer => renderer.sharedMesh != null && renderer.sharedMesh.blendShapeCount > 0).ToArray();
            face = renderers.Length == 1 ? renderers[0] : null;
            shape = "";
            message = "Unique references discovered. Assign empty fields explicitly; Renderer and Ground may be optional.";
        }
        private void RequireCharacter()
        {
            if (character == null || EditorUtility.IsPersistent(character) || !character.scene.IsValid() ||
                !character.scene.isLoaded || EditorSceneManager.IsPreviewScene(character.scene) ||
                PrefabStageUtility.GetPrefabStage(character) != null)
                throw new InvalidOperationException("Choose a scene character root in Hierarchy, not a Project asset or Prefab Mode object.");
        }
        private void RequireReferences()
        {
            RequireCharacter();
            if (animator == null || orbit == null || motion == null || expression == null || panel == null)
                throw new InvalidOperationException("Assign Animator, Orbit camera, Motion controller, Expression controller and Control panel.");
            if (new Component[] { animator, orbit, motion, expression, panel }.Any(item => item.gameObject.scene != character.scene))
                throw new InvalidOperationException("All references must belong to the character's scene.");
            if (!animator.transform.IsChildOf(character.transform))
                throw new InvalidOperationException("Animator must belong to the chosen character root.");
        }
        private void SaveLocalCopy()
        {
            System.IO.Directory.CreateDirectory("Assets/LocalOnly/Scenes");
            AssetDatabase.Refresh();
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/LocalOnly/Scenes/RuruneApp_Configured.unity");
            // Save as a copy first, then change the active scene path. Model assets are not copied or modified.
            if (!EditorSceneManager.SaveScene(character.scene, path))
                throw new InvalidOperationException("Could not save the local scene. No setup was applied.");
        }
        private static void SetReference(UnityEngine.Object owner, string name, UnityEngine.Object value)
        {
            var data = new SerializedObject(owner);
            data.FindProperty(name).objectReferenceValue = value;
            data.ApplyModifiedProperties();
        }
        private void SaveChanges()
        {
            // Direct field changes on prefab instances need explicit override recording.
            var touched = new List<UnityEngine.Object> { animator, orbit, motion, expression, panel };
            if (orbit != null) touched.Add(orbit.GetComponent<Camera>());
            if (ground != null) touched.Add(ground.transform);
            foreach (UnityEngine.Object item in touched)
            {
                if (item == null) continue;
                EditorUtility.SetDirty(item);
                if (PrefabUtility.IsPartOfPrefabInstance(item))
                    PrefabUtility.RecordPrefabInstancePropertyModifications(item);
            }
            EditorSceneManager.MarkSceneDirty(character.scene);
            if (!EditorSceneManager.SaveScene(character.scene))
                throw new InvalidOperationException("Changes are in memory but scene saving failed. Save manually before closing.");
        }
        private void Setup()
        {
            RequireReferences();
            if (orbit.transform.IsChildOf(character.transform))
                throw new InvalidOperationException("Camera must be outside the character hierarchy.");
            if (ground != null && (ground.scene != character.scene || ground.transform.IsChildOf(character.transform) ||
                character.transform.IsChildOf(ground.transform)))
                throw new InvalidOperationException("Ground must be a separate object in the same scene.");
            if (!CharacterBounds.TryGet(character.transform, out Bounds bounds))
                throw new InvalidOperationException("No enabled, active character mesh was found.");
            Renderer groundRenderer = ground == null ? null : ground.GetComponent<Renderer>();
            if (ground != null && groundRenderer == null)
                throw new InvalidOperationException("Assign the Ground object with its Renderer, or leave Ground empty.");
            SaveLocalCopy();
            Undo.RecordObjects(new UnityEngine.Object[] { orbit, orbit.GetComponent<Camera>(), animator, motion, panel }, "Configure character MVP");
            orbit.ConfigureFraming(character.transform, bounds);
            animator.applyRootMotion = false;
            SetReference(motion, "animator", animator);
            SetReference(panel, "orbitCamera", orbit);
            SetReference(panel, "motionController", motion);
            SetReference(panel, "expressionController", expression);
            if (groundRenderer != null)
            {
                Undo.RecordObject(ground.transform, "Align ground below character");
                float top = CharacterBounds.TransformBounds(ground.transform, groundRenderer.localBounds).max.y;
                ground.transform.position += Vector3.up * (bounds.min.y - Mathf.Max(0.001f, bounds.size.y * 0.002f) - top);
            }
            SaveChanges();
            message = "Local scene saved: " + character.scene.path + ". Camera fitted, references connected; existing expressions retained.";
        }
        private void AddExpression()
        {
            RequireReferences();
            if (face == null || !face.transform.IsChildOf(character.transform) || face.sharedMesh == null ||
                string.IsNullOrEmpty(shape) || face.sharedMesh.GetBlendShapeIndex(shape) < 0 ||
                string.IsNullOrWhiteSpace(expressionLabel) || float.IsNaN(weight) || float.IsInfinity(weight))
                throw new InvalidOperationException("Choose an actual character BlendShape, a label, and a finite weight.");
            if (expression.Expressions != null && expression.Expressions.Any(item => item != null && item.label == expressionLabel))
                throw new InvalidOperationException("That button label already exists. Choose another label or edit its preset in Inspector.");
            SaveLocalCopy();
            Undo.RecordObject(expression, "Append expression");
            var data = new SerializedObject(expression);
            SerializedProperty list = data.FindProperty("expressions");
            int index = list.arraySize;
            list.arraySize++;
            SerializedProperty entry = list.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("label").stringValue = expressionLabel;
            SerializedProperty shapes = entry.FindPropertyRelative("shapes");
            shapes.arraySize = 1;
            SerializedProperty itemProperty = shapes.GetArrayElementAtIndex(0);
            itemProperty.FindPropertyRelative("renderer").objectReferenceValue = face;
            itemProperty.FindPropertyRelative("shapeName").stringValue = shape;
            itemProperty.FindPropertyRelative("weight").floatValue = weight;
            data.ApplyModifiedProperties();
            SaveChanges();
            message = "Expression appended. Verify appearance in Play Mode; existing presets retained.";
        }
        private void CreateController()
        {
            RequireReferences();
            if (clips == null || clips.Length == 0 || clips.Any(clip => clip == null || clip.legacy))
                throw new InvalidOperationException("Assign at least one non-legacy AnimationClip; remove empty entries.");
            if (animator.avatar == null || !animator.avatar.isValid)
                throw new InvalidOperationException("Animator needs a valid Avatar. Check the model Rig before assigning clips.");
            if (clips.Any(clip => clip.humanMotion) && !animator.isHuman)
                throw new InvalidOperationException("Humanoid motion requires a Humanoid Animator.");
            if (clips.Distinct().Count() != clips.Length)
                throw new InvalidOperationException("Remove duplicate clips before creating the controller.");
            SaveLocalCopy();
            System.IO.Directory.CreateDirectory("Assets/LocalOnly/Animations");
            AssetDatabase.Refresh();
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/LocalOnly/Animations/AppMotions.controller");
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            var entries = new List<CharacterMotionController.Motion>();
            for (int i = 0; i < clips.Length; i++)
            {
                string stateName = "Motion_" + i.ToString("D3");
                AnimatorState state = controller.layers[0].stateMachine.AddState(stateName);
                state.motion = clips[i];
                state.writeDefaultValues = false;
                EditorUtility.SetDirty(state);
                if (i == 0) controller.layers[0].stateMachine.defaultState = state;
                entries.Add(new CharacterMotionController.Motion { label = clips[i].name,
                    statePath = controller.layers[0].name + "." + stateName, layer = 0 });
            }
            EditorUtility.SetDirty(controller.layers[0].stateMachine);
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            Undo.RecordObject(animator, "Assign app controller");
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            SetReference(motion, "animator", animator);
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            SaveMotions(entries);
            message = "New local controller assigned; old asset unchanged. Verify each clip's pose and compatibility in Play Mode.";
        }
        private void RegisterExistingStates()
        {
            RequireReferences();
            RuntimeAnimatorController source = animator.runtimeAnimatorController;
            while (source is AnimatorOverrideController overrides) source = overrides.runtimeAnimatorController;
            var controller = source as AnimatorController;
            if (controller == null) throw new InvalidOperationException("The selected Animator has no readable AnimatorController.");
            var entries = new List<CharacterMotionController.Motion>();
            for (int i = 0; i < controller.layers.Length; i++)
            {
                AnimatorControllerLayer layer = controller.layers[i];
                if (layer.syncedLayerIndex >= 0) continue;
                Collect(layer.stateMachine, layer.name, i, entries);
            }
            if (entries.Count == 0) throw new InvalidOperationException("No non-synced motion states were found.");
            SaveLocalCopy();
            SetReference(motion, "animator", animator);
            SaveMotions(entries);
            message = "Actual state paths registered. Existing transitions can override a button request; FX states may not animate the body.";
        }
        private static void Collect(AnimatorStateMachine machine, string prefix, int layer,
            List<CharacterMotionController.Motion> entries)
        {
            foreach (ChildAnimatorState child in machine.states)
                if (child.state.motion != null)
                    entries.Add(new CharacterMotionController.Motion { label = prefix + "." + child.state.name,
                        statePath = prefix + "." + child.state.name, layer = layer });
            foreach (ChildAnimatorStateMachine child in machine.stateMachines)
                Collect(child.stateMachine, prefix + "." + child.stateMachine.name, layer, entries);
        }
        private void SaveMotions(List<CharacterMotionController.Motion> entries)
        {
            Undo.RecordObject(motion, "Register app motions");
            var data = new SerializedObject(motion);
            SerializedProperty list = data.FindProperty("motions");
            list.arraySize = entries.Count;
            for (int i = 0; i < entries.Count; i++)
            {
                SerializedProperty entry = list.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("label").stringValue = entries[i].label;
                entry.FindPropertyRelative("statePath").stringValue = entries[i].statePath;
                entry.FindPropertyRelative("layer").intValue = entries[i].layer;
                entry.FindPropertyRelative("transitionSeconds").floatValue = 0.15f;
            }
            data.ApplyModifiedProperties();
            SaveChanges();
        }
        private void WriteReport()
        {
            RequireCharacter();
            var lines = new List<string> { "Rurune MVP local diagnostic", "Unity: " + Application.unityVersion,
                "Platform: " + Application.platform, "Character: " + character.name };
            foreach (Animator item in character.GetComponentsInChildren<Animator>(true))
                lines.Add("Animator: " + item.name + "; controller=" +
                    (item.runtimeAnimatorController == null ? "none" : item.runtimeAnimatorController.name) + "; humanoid=" + item.isHuman);
            foreach (SkinnedMeshRenderer item in character.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                lines.Add("Renderer: " + item.name + "; blendShapes=" + (item.sharedMesh == null ? 0 : item.sharedMesh.blendShapeCount));
            if (CharacterBounds.TryGet(character.transform, out Bounds bounds))
                lines.Add("World mesh bounds: " + bounds.ToString());
            lines.Add("Motion presets: " + (motion == null ? 0 : motion.Motions.Length));
            lines.Add("Expression presets: " + (expression == null ? 0 : expression.Expressions.Length));
            lines.Add("This report does not verify rendering, clip compatibility, or Windows build behavior.");
            System.IO.Directory.CreateDirectory("Assets/LocalOnly/Diagnostics");
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/LocalOnly/Diagnostics/MvpReport.txt");
            System.IO.File.WriteAllLines(path, lines);
            AssetDatabase.Refresh();
            message = "Report saved locally: " + path + ". No upload was performed. Console errors are not included.";
        }
    }
}
