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
        private bool dragging, panning;
        private Vector3 panOffset;
        [SerializeField, Min(0.05f)] private float framingHeight = 1f;
        public float ViewHeight { get; set; }

        public bool FitCharacter()
        {
            if (target == null || !CharacterBounds.TryGet(target, out Bounds bounds)) return false;
            ConfigureFraming(target, bounds);
            return true;
        }

        public void ConfigureFraming(Transform character, Bounds bounds)
        {
            target = character;
            targetOffset = bounds.center - character.position;
            framingHeight = Mathf.Max(0.05f, bounds.size.y);
            Camera camera = GetComponent<Camera>();
            float vertical = Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float horizontal = vertical * Mathf.Max(0.1f, camera.aspect * 0.65f);
            // A bounding sphere remains valid when orbiting to the side of the model.
            float radius = Mathf.Max(0.025f, bounds.extents.magnitude);
            float halfAngle = Mathf.Atan(Mathf.Min(vertical, horizontal));
            initialDistance = radius / Mathf.Sin(halfAngle) * 1.15f;
            minimumDistance = Mathf.Max(0.05f, radius * 0.15f);
            maximumDistance = Mathf.Max(initialDistance * 4f, minimumDistance);
            camera.nearClipPlane = Mathf.Max(0.001f, Mathf.Min(0.05f, minimumDistance * 0.1f));
            camera.farClipPlane = Mathf.Max(camera.farClipPlane, maximumDistance + radius * 2f);
            initialPitch = 0f;
            ResetView();
        }
        public bool PointerOverUI { get; set; }

        private void OnEnable() { ResetView(); }
        private void OnDisable() { dragging = panning = false; }
        private void OnApplicationFocus(bool focused) { if (!focused) dragging = panning = false; }
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
            dragging = panning = false;
            panOffset = Vector3.zero;
            ViewHeight = 0f;
        }
        private void LateUpdate()
        {
            if (target == null) return;
            if (Input.GetMouseButtonDown(1)) dragging = !PointerOverUI;
            if (!Input.GetMouseButton(1)) dragging = false;
            if (Input.GetMouseButtonDown(2)) panning = !PointerOverUI;
            if (!Input.GetMouseButton(2)) panning = false;
            if (panning && !PointerOverUI)
            {
                float step = distance * 0.02f;
                panOffset -= (transform.right * Input.GetAxis("Mouse X") +
                    transform.up * Input.GetAxis("Mouse Y")) * step;
            }
            if (dragging && !PointerOverUI)
            {
                yaw += Input.GetAxis("Mouse X") * rotationSpeed;
                pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * rotationSpeed, -80f, 80f);
            }
            if (!PointerOverUI)
                distance = Mathf.Clamp(distance - Input.mouseScrollDelta.y * zoomSpeed * 0.1f,
                    minimumDistance, maximumDistance);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = target.position + targetOffset + panOffset + Vector3.up * ViewHeight * framingHeight;
            transform.SetPositionAndRotation(pivot - rotation * Vector3.forward * distance, rotation);
        }
    }
}
