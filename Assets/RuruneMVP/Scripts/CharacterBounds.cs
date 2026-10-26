using UnityEngine;

namespace RuruneMVP
{
    public static class CharacterBounds
    {
        // On-demand measurement only; no model hierarchy or BlendShape names are assumed.
        public static bool TryGet(Transform root, out Bounds result)
        {
            result = new Bounds();
            bool found = false;
            if (root == null) return false;
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(false))
            {
                if (!renderer.enabled) continue;
                Bounds world;
                if (renderer is SkinnedMeshRenderer skin)
                {
                    if (skin.sharedMesh == null || skin.sharedMesh.vertexCount == 0) continue;
                    var baked = new Mesh();
                    try
                    {
                        skin.BakeMesh(baked);
                        baked.RecalculateBounds();
                        world = TransformBounds(skin.transform, baked.bounds);
                    }
                    finally
                    {
                        if (Application.isPlaying) Object.Destroy(baked);
                        else Object.DestroyImmediate(baked);
                    }
                }
                else if (renderer is MeshRenderer meshRenderer)
                {
                    MeshFilter filter = meshRenderer.GetComponent<MeshFilter>();
                    if (filter == null || filter.sharedMesh == null) continue;
                    world = TransformBounds(filter.transform, filter.sharedMesh.bounds);
                }
                else continue;
                if (!Finite(world.center) || !Finite(world.size) || world.size.sqrMagnitude <= 0f) continue;
                if (found) result.Encapsulate(world);
                else { result = world; found = true; }
            }
            return found;
        }
        private static bool Finite(Vector3 value)
        {
            return !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
                !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
                !float.IsNaN(value.z) && !float.IsInfinity(value.z);
        }
        public static Bounds TransformBounds(Transform transform, Bounds local)
        {
            var bounds = new Bounds(transform.TransformPoint(local.center), Vector3.zero);
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = local.center + Vector3.Scale(local.extents,
                    new Vector3((i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f));
                bounds.Encapsulate(transform.TransformPoint(corner));
            }
            return bounds;
        }
    }
}
