using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(Camera))]
    public class BackgroundPlane : MonoBehaviour
    {
        [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.3f);
        [SerializeField] private float distance = 10f;

        private void Start()
        {
            var camera = GetComponent<Camera>();
            if (camera == null) return;

            var plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            plane.name = "Background";
            plane.transform.SetParent(camera.transform);
            plane.transform.localPosition = new Vector3(0, 0, distance);
            plane.transform.localRotation = Quaternion.identity;
            plane.transform.localScale = new Vector3(camera.aspect * 20, 20, 1);

            var renderer = plane.GetComponent<Renderer>();
            renderer.sharedMaterial = new Material(Shader.Find("Unlit/Color"));
            renderer.sharedMaterial.color = backgroundColor;
        }
    }
}