namespace WC {
    public class OrbitControlsConfig {
        // Rotation
        public float rotationSpeedX = 180f;
        public float rotationSpeedY = 180f;
        public float rotationEasing = 0.25f;
        public float polarAngleMin = -90;
        public float polarAngleMax = 90;
        public float thetaAngleMin = -90;
        public float thetaAngleMax = 90;
        // Zoom
        public float zoomSpeed = 2f;
        public float zoomEasing = 0.15f;
        public float zoomDistanceStart = 0.15f;
        public float zoomDistanceMin = 1f;
        public float zoomDistanceMax = 50f;
    }
}
