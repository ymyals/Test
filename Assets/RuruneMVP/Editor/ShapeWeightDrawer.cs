using UnityEditor;
using UnityEngine;

namespace RuruneMVP.Editor
{
    [CustomPropertyDrawer(typeof(CharacterExpressionController.ShapeWeight))]
    public sealed class ShapeWeightDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 4f;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            float step = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            Rect row = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(row, label);
            row.y += step;
            SerializedProperty rendererProperty = property.FindPropertyRelative("renderer");
            SerializedProperty nameProperty = property.FindPropertyRelative("shapeName");
            EditorGUI.PropertyField(row, rendererProperty);
            row.y += step;
            var renderer = rendererProperty.objectReferenceValue as SkinnedMeshRenderer;
            Mesh mesh = renderer == null ? null : renderer.sharedMesh;
            if (mesh != null && mesh.blendShapeCount > 0)
            {
                string[] names = new string[mesh.blendShapeCount + 1];
                names[0] = string.IsNullOrEmpty(nameProperty.stringValue)
                    ? "Select a BlendShape" : "Missing / select: " + nameProperty.stringValue;
                int selected = 0;
                for (int i = 0; i < mesh.blendShapeCount; i++)
                {
                    names[i + 1] = mesh.GetBlendShapeName(i);
                    if (names[i + 1] == nameProperty.stringValue) selected = i + 1;
                }
                EditorGUI.BeginChangeCheck();
                int next = EditorGUI.Popup(row, "BlendShape", selected, names);
                if (EditorGUI.EndChangeCheck()) nameProperty.stringValue = next == 0 ? "" : names[next];
            }
            else EditorGUI.LabelField(row, "BlendShape", "Assign a mesh with BlendShapes.");
            row.y += step;
            EditorGUI.PropertyField(row, property.FindPropertyRelative("weight"));
            EditorGUI.EndProperty();
        }
    }
}
