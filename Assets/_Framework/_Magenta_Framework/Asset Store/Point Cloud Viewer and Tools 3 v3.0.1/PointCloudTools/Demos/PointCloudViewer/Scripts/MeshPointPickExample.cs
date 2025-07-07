using UnityEngine;
using UnityEngine.UI;
using mgear.tools;

namespace pointcloudviewer.examples
{
    public class MeshPointPickExample : MonoBehaviour
    {
        public MeshPointPicker meshPointPicker;
        public Text measureInfoText;

        protected Camera cam;

        // for automated testing
        bool isTesting = false;

        protected virtual void Start()
        {
            cam = Camera.main;
        }

        protected virtual void Update()
        {
            // if click mouse
            if (Input.GetMouseButtonDown(0) || isTesting)
            {
                // create ray at mouse pos, if you want to pick using VR hand or other devices, create ray from that position
                var ray = cam.ScreenPointToRay(Input.mousePosition);

                if (isTesting)
                {
                    ray = cam.ScreenPointToRay(new Vector3(522.9f, 260.7f, 0.0f));
                }

                var stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                // call point picker
                meshPointPicker.Pick(ray);

                stopwatch.Stop();
                Debug.LogFormat("PickTimer: {0} ms", stopwatch.ElapsedMilliseconds);
                stopwatch.Reset();
            } // mouse down
        } // Update()

        // link this into point picker OnPointPicked event
        public virtual void OnPointPicked(PointData p)
        {
            Debug.Log($"OnPointPicked: {p.position}, Color: {p.color}, VertexIndex: {p.vertexIndex}, Gameobject: {p.meshGameObject.name}");
            if (measureInfoText!=null) measureInfoText.text = $"OnPointPicked: {p.position}, Color: {p.color}, VertexIndex: {p.vertexIndex}, Gameobject: {p.meshGameObject.name}";

            // draw point position
            GLDebug.DrawLine(p.position + Vector3.left * 0.1f, p.position + Vector3.right * 0.1f, Color.red, 10);
            GLDebug.DrawLine(p.position + Vector3.up * 0.1f, p.position + Vector3.down * 0.1f, Color.green, 10);
            GLDebug.DrawLine(p.position + Vector3.forward * 0.1f, p.position + Vector3.back * 0.1f, Color.blue, 10);
        }

        // link this into point picker OnPointNotFound event
        public virtual void OnPointNotFound()
        {
            Debug.Log("No points found!");
        }

    } // class
} // namespace