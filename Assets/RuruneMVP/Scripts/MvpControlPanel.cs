using UnityEngine;

namespace RuruneMVP
{
    // Built-in IMGUI keeps the MVP independent of uGUI/TMP packages.
    public sealed class MvpControlPanel : MonoBehaviour
    {
        [SerializeField] private OrbitCamera orbitCamera;
        [SerializeField] private CharacterMotionController motionController;
        [SerializeField] private CharacterExpressionController expressionController;
        private Vector2 scroll;
        private Rect panel;
        private Rect PanelRect => new Rect(12f, 12f, Mathf.Min(310f, Screen.width - 24f),
            Mathf.Max(80f, Mathf.Min(600f, Screen.height - 24f)));
        private void Update()
        {
            panel = PanelRect;
            Vector2 pointer = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            if (orbitCamera != null) orbitCamera.PointerOverUI = panel.Contains(pointer);
        }
        private void OnDisable() { if (orbitCamera != null) orbitCamera.PointerOverUI = false; }
        private void OnGUI()
        {
            GUILayout.BeginArea(PanelRect, GUI.skin.box);
            scroll = GUILayout.BeginScrollView(scroll);
            GUILayout.Label("Character MVP");
            GUILayout.Label("Right drag: orbit / Wheel: zoom / Middle drag: pan");
            if (orbitCamera != null)
            {
                if (GUILayout.Button("Reset camera")) orbitCamera.ResetView();
                if (GUILayout.Button("Fit character") && !orbitCamera.FitCharacter())
                    Debug.LogWarning("No active character mesh found. Run Quick Setup first.", this);
                GUILayout.Label("View height (lower value moves character up)");
                orbitCamera.ViewHeight = GUILayout.HorizontalSlider(orbitCamera.ViewHeight, -1f, 1f);
            }
            GUILayout.Space(10f);
            GUILayout.Label("Motions");
            if (motionController != null)
            {
                var motions = motionController.Motions;
                if (motions == null || motions.Length == 0) GUILayout.Label("Configure Motions in Inspector.");
                else for (int i = 0; i < motions.Length; i++)
                    if (motions[i] != null && GUILayout.Button(motions[i].label ?? "Motion")) motionController.Play(i);
                GUILayout.Label(motionController.Status);
            }
            GUILayout.Space(10f);
            GUILayout.Label("Expressions");
            if (expressionController != null)
            {
                var expressions = expressionController.Expressions;
                if (expressions == null || expressions.Length == 0) GUILayout.Label("Configure Expressions in Inspector.");
                else for (int i = 0; i < expressions.Length; i++)
                    if (expressions[i] != null && GUILayout.Button(expressions[i].label ?? "Expression")) expressionController.Apply(i);
                if (GUILayout.Button("Reset expression")) expressionController.ResetExpression();
                GUILayout.Label(expressionController.Status);
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
