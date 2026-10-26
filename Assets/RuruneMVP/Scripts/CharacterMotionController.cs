using System;
using UnityEngine;

namespace RuruneMVP
{
    [DisallowMultipleComponent]
    public sealed class CharacterMotionController : MonoBehaviour
    {
        [Serializable]
        public sealed class Motion
        {
            public string label;
            [Tooltip("Exact full Animator state path, including layer, as verified in the Animator window.")]
            public string statePath;
            [Min(0)] public int layer;
            [Min(0f)] public float transitionSeconds = 0.15f;
        }
        [SerializeField] private Animator animator;
        [Tooltip("Explicit scene character root. Used only if the assigned Animator becomes unavailable.")]
        [SerializeField] private Transform characterRoot;
        [SerializeField] private Motion[] motions = new Motion[0];
        public Motion[] Motions => motions;
        public string Status { get; private set; } = "Select a configured motion.";

        public bool Play(int index)
        {
            if (motions == null || index < 0 || index >= motions.Length)
                return Fail("Motion index is invalid.");
            Motion motion = motions[index];
            if (motion == null || string.IsNullOrWhiteSpace(motion.statePath) || motion.layer < 0)
                return Fail("Check the motion state path and layer in MVP Systems.");
            int hash = Animator.StringToHash(motion.statePath);
            if (!Ready(animator) && characterRoot != null)
            {
                // Never search the whole scene or enable a component disabled by another system.
                // Rebind only when exactly one active Animator under the explicitly selected root
                // contains the requested state. This also handles component replacement at runtime.
                Animator match = null;
                foreach (Animator candidate in characterRoot.GetComponentsInChildren<Animator>(false))
                {
                    if (!Ready(candidate) || motion.layer >= candidate.layerCount ||
                        !candidate.HasState(motion.layer, hash)) continue;
                    if (match != null) return Fail("Multiple matching Animators under Character Root. Assign the intended Animator explicitly.");
                    match = candidate;
                }
                if (match != null) animator = match;
            }
            if (animator == null)
                return Fail("Animator reference is missing. Stop Play Mode and assign the scene model's Animator in MVP Systems.");
            if (!animator.gameObject.activeInHierarchy)
                return Fail("Animator object is inactive: " + AnimatorPath());
            if (!animator.enabled)
                return Fail("Animator component is disabled: " + AnimatorPath());
            if (animator.runtimeAnimatorController == null)
                return Fail("Animator has no Controller: " + AnimatorPath());
            if (motion.layer >= animator.layerCount)
                return Fail("Layer is out of range on " + AnimatorPath());
            if (!animator.HasState(motion.layer, hash))
                return Fail("Animator state not found: " + motion.statePath);
            animator.CrossFadeInFixedTime(hash, Mathf.Max(0f, motion.transitionSeconds), motion.layer, 0f);
            Status = "Motion requested: " + motion.label + " / Animator: " + AnimatorPath();
            return true;
        }
        private static bool Ready(Animator value)
        {
            return value != null && value.isActiveAndEnabled && value.runtimeAnimatorController != null;
        }
        private string AnimatorPath()
        {
            if (animator == null) return "none";
            string path = animator.name;
            for (Transform parent = animator.transform.parent; parent != null; parent = parent.parent)
                path = parent.name + "/" + path;
            return path;
        }
        private bool Fail(string message)
        {
            Status = message;
            Debug.LogWarning(message, this);
            return false;
        }
    }
}
