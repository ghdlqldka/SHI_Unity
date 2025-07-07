using UnityEngine;
using mgear.tools;

namespace pointcloudviewer.examples
{
    public class _MeshPointPickExample : MeshPointPickExample
    {
        private static string LOG_FORMAT = "<color=#00FF90><b>[_MeshPointPickExample]</b></color> {0}";

        // public MeshPointPicker meshPointPicker;
        protected _MeshPointPicker _meshPointPicker
        {
            get
            {
                return meshPointPicker as _MeshPointPicker;
            }
        }

        [SerializeField]
        protected Camera _camera;

        protected virtual void Awake()
        {
            Debug.Assert(_meshPointPicker != null);
            Debug.Assert(_camera != null);
        }

        protected override void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start()");

            // cam = Camera.main;
            cam = _camera;
        }

        protected override void Update()
        {
            // if click mouse
            if (Input.GetMouseButtonDown(0) /* || isTesting*/)
            {
                // create ray at mouse pos, if you want to pick using VR hand or other devices, create ray from that position
                var ray = cam.ScreenPointToRay(Input.mousePosition);

                /*
                if (isTesting)
                {
                    ray = cam.ScreenPointToRay(new Vector3(522.9f, 260.7f, 0.0f));
                }
                */

                var stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                // call point picker
                _meshPointPicker.Pick(ray);

                stopwatch.Stop();
                Debug.LogFormat("PickTimer: {0} ms", stopwatch.ElapsedMilliseconds);
                stopwatch.Reset();
            } // mouse down
        } // Update()

        public override void OnPointPicked(PointData data)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "OnPointPicked(), position : " + data.position + ", Color: " + data.color + " , VertexIndex: " + data.vertexIndex + ", Gameobject: " + data.meshGameObject.name);
            if (measureInfoText != null)
            {
                measureInfoText.text = $"OnPointPicked: {data.position}, Color: {data.color}, VertexIndex: {data.vertexIndex}, Gameobject: {data.meshGameObject.name}";
            }

            // draw point position
            _GLDebug.Instance.DrawLine(data.position + Vector3.left * 0.1f, data.position + Vector3.right * 0.1f, Color.red, 10, false);
            _GLDebug.Instance.DrawLine(data.position + Vector3.up * 0.1f, data.position + Vector3.down * 0.1f, Color.green, 10, false);
            _GLDebug.Instance.DrawLine(data.position + Vector3.forward * 0.1f, data.position + Vector3.back * 0.1f, Color.blue, 10, false);
        }

        // link this into point picker OnPointNotFound event
        public override void OnPointNotFound()
        {
            Debug.LogFormat(LOG_FORMAT, "No points found!");
        }

    } // class
} // namespace