using System;
using System.Collections.Generic;
using UnityEngine;

namespace RuruneMVP
{
    [DisallowMultipleComponent]
    public sealed class CharacterExpressionController : MonoBehaviour
    {
        [Serializable]
        public sealed class ShapeWeight
        {
            public SkinnedMeshRenderer renderer;
            [Tooltip("Select an existing BlendShape using the Inspector dropdown.")]
            public string shapeName;
            [Tooltip("Weight range depends on the actual mesh. Check its author documentation.")]
            public float weight;
        }
        [Serializable]
        public sealed class Expression
        {
            public string label;
            public ShapeWeight[] shapes = new ShapeWeight[0];
        }
        [SerializeField] private Expression[] expressions = new Expression[0];
        private readonly List<Binding> bindings = new List<Binding>();
        private bool selected;
        private sealed class Binding
        {
            public SkinnedMeshRenderer renderer;
            public Mesh mesh;
            public int index;
            public float original, desired;
        }
        public Expression[] Expressions => expressions;
        public string Status { get; private set; } = "Select a configured expression.";

        public bool Apply(int index)
        {
            if (expressions == null || index < 0 || index >= expressions.Length || expressions[index] == null)
                return Fail("Expression index is invalid.");
            ShapeWeight[] shapes = expressions[index].shapes;
            if (shapes == null || shapes.Length == 0) return Fail("Expression has no BlendShapes.");
            // Validate the entire preset before changing any weight.
            var seen = new HashSet<string>();
            foreach (ShapeWeight shape in shapes)
            {
                if (shape == null || shape.renderer == null || shape.renderer.sharedMesh == null ||
                    string.IsNullOrEmpty(shape.shapeName) ||
                    shape.renderer.sharedMesh.GetBlendShapeIndex(shape.shapeName) < 0 ||
                    float.IsNaN(shape.weight) || float.IsInfinity(shape.weight))
                    return Fail("Assign an existing renderer/BlendShape and a finite weight.");
                string key = shape.renderer.GetInstanceID() + ":" + shape.shapeName;
                if (!seen.Add(key)) return Fail("Duplicate BlendShape in this expression: " + shape.shapeName);
            }
            foreach (Binding binding in bindings) binding.desired = binding.original;
            foreach (ShapeWeight shape in shapes)
            {
                Mesh mesh = shape.renderer.sharedMesh;
                int shapeIndex = mesh.GetBlendShapeIndex(shape.shapeName);
                Binding binding = bindings.Find(b => b.renderer == shape.renderer && b.mesh == mesh && b.index == shapeIndex);
                if (binding == null)
                {
                    binding = new Binding { renderer = shape.renderer, mesh = mesh, index = shapeIndex,
                        original = shape.renderer.GetBlendShapeWeight(shapeIndex) };
                    bindings.Add(binding);
                }
                binding.desired = shape.weight;
            }
            selected = true;
            WriteWeights(false);
            Status = "Expression: " + expressions[index].label;
            return true;
        }
        // Apply after Animator evaluation. Configured channels belong to this component while selected.
        private void LateUpdate() { if (selected) WriteWeights(false); }
        public void ResetExpression()
        {
            WriteWeights(true);
            bindings.Clear();
            selected = false;
            Status = "Expression weights restored.";
        }
        private void OnDisable() { ResetExpression(); }
        private void WriteWeights(bool restore)
        {
            foreach (Binding binding in bindings)
                if (binding.renderer != null && binding.renderer.sharedMesh == binding.mesh)
                    binding.renderer.SetBlendShapeWeight(binding.index, restore ? binding.original : binding.desired);
        }
        private bool Fail(string message)
        {
            Status = message;
            Debug.LogWarning(message, this);
            return false;
        }
    }
}
