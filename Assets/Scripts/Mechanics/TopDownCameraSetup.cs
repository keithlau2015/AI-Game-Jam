using Platformer.Mechanics;
using UnityEngine;

namespace Platformer.Mechanics
{
    public class TopDownCameraSetup : MonoBehaviour
    {
        public Vector3 position = new Vector3(0f, 0f, -10f);
        public float orthographicSize = 7f;
        public Color backgroundColor = new Color(0.12f, 0.14f, 0.18f, 1f);

        void Awake()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera", typeof(Camera));
                cameraObject.tag = "MainCamera";
                camera = cameraObject.GetComponent<Camera>();
            }

            camera.gameObject.SetActive(true);
            camera.orthographic = true;
            camera.orthographicSize = orthographicSize;
            camera.transform.position = position;
            camera.backgroundColor = backgroundColor;
        }
    }
}
