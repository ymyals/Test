using UnityEngine;

namespace RuruneMVP
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class OrbitCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1f, 0f);
        [SerializeField, Min(0.05f)] private float minimumDistance = 0.5f;
        [SerializeField, Min(0.05f)] private float maximumDistance = 8f;
        [SerializeField] private float initialDistance = 3f;
        [SerializeField] private float initialYaw = 180f;
        [SerializeField, Range(-80f, 80f)] private float initialPitch = 10f;
        [SerializeField, Min(0f)] private float rotationSpeed = 4f;
        [SerializeField, Min(0f)] private float zoomSpeed = 2f;
        private float yaw, pitch, distance;
        private bool dragging;
        public bool PointerOverUI { get; set; }

        private void OnEnable() { ResetView(); }
        private void OnDisable() { dragging = false; }
        private void OnApplicationFocus(bool focused) { if (!focused) dragging = false; }
        private void OnValidate()
        {
            minimumDistance = Mathf.Max(0.05f, minimumDistance);
            maximumDistance = Mathf.Max(minimumDistance, maximumDistance);
            initialDistance = Mathf.Clamp(initialDistance, minimumDistance, maximumDistance);
        }
        public void ResetView()
        {
            yaw = initialYaw;
            pitch = initialPitch;
            distance = Mathf.Clamp(initialDistance, minimumDistance, maximumDistance);
            dragging = false;
        }
        private void LateUpdate()
        {
            if (target == null) return;
            if (Input.GetMouseButtonDown(1)) dragging = !PointerOverUI;
            if (!Input.GetMouseButton(1)) dragging = false;
            if (dragging && !PointerOverUI)
            {
                yaw += Input.GetAxis("Mouse X") * rotationSpeed;
                pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * rotationSpeed, -80f, 80f);
            }
            if (!PointerOverUI)
                distance = Mathf.Clamp(distance - Input.mouseScrollDelta.y * zoomSpeed * 0.1f,
                    minimumDistance, maximumDistance);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = target.position + targetOffset;
            transform.SetPositionAndRotation(pivot - rotation * Vector3.forward * distance, rotation);
        }
    }
}
