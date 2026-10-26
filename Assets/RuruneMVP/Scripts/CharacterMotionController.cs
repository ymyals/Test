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
        [SerializeField] private Motion[] motions = new Motion[0];
        public Motion[] Motions => motions;
        public string Status { get; private set; } = "Select a configured motion.";

        public bool Play(int index)
        {
            if (motions == null || index < 0 || index >= motions.Length)
                return Fail("Motion index is invalid.");
            Motion motion = motions[index];
            if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null)
                return Fail("Assign an active Animator and Runtime Animator Controller.");
            if (motion == null || string.IsNullOrWhiteSpace(motion.statePath) ||
                motion.layer < 0 || motion.layer >= animator.layerCount)
                return Fail("Check the state path and layer in the Inspector.");
            int hash = Animator.StringToHash(motion.statePath);
            if (!animator.HasState(motion.layer, hash))
                return Fail("Animator state not found: " + motion.statePath);
            animator.CrossFadeInFixedTime(hash, Mathf.Max(0f, motion.transitionSeconds), motion.layer, 0f);
            Status = "Motion: " + motion.label;
            return true;
        }
        private bool Fail(string message)
        {
            Status = message;
            Debug.LogWarning(message, this);
            return false;
        }
    }
}
