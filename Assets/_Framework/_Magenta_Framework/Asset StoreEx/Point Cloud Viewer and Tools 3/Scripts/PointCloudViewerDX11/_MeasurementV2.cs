using UnityEngine;
using PointCloudViewer;
using UnityEngine.EventSystems;
using PointCloudHelpers;
using mgear.tools;

namespace PointCloudExtras
{
    public class _MeasurementV2 : MeasurementManagerV2
    {
        private static string LOG_FORMAT = "<color=#AFFF04><b>[_MeasurementV2]</b></color> {0}";

        protected override void OnDestroy()
        {
            // unsubscribe
            _PointCloudManager.PointWasSelected -= OnPointSelected;
        }

        protected override void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start()");

            // cam = Camera.main;
            cam = null; // Do not use!!!!!

            closestDistance = 0;

            // subscribe to event listener
            // _PointCloudManager.PointWasSelected -= PointSelected; // unsubscribe just in case
            _PointCloudManager.PointWasSelected += OnPointSelected;
        }

        public override void Update()
        {
            // dont do pick if over UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            // left button click to select
            if (Input.GetMouseButtonDown(0) && Input.GetKey(altKey) == false)
            {
                // Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                Ray ray = _PointCloudManager.Instance._Camera.ScreenPointToRay(Input.mousePosition);

                Vector3 rayEnd = ray.origin + ray.direction * 100f;
                Ray invertedRay = new Ray(rayEnd, -ray.direction * 100f);

                // call threaded point picker, you can call this manually from your own scripts, by passing your own ray
                _PointCloudManager.Instance.RunPointPickingThread(ray);
            }

            if (haveFirstPoint == true && haveSecondPoint == false)
            {
                //
            }
            else if (haveFirstPoint == true && haveSecondPoint == true)
            {
                DrawLine();
            }
        }

        protected override void PointSelected(Vector3 pos)
        {
            throw new System.NotSupportedException("");
        }

        protected virtual void OnPointSelected(Vector3 pos)
        {
            // debug lines only
            PointCloudMath.DebugHighLightPointGreen(pos);

            // was this the first selection
            if (isFirstPoint == true)
            {
                startPos = pos;
                if (distanceUIText != null)
                {
                    distanceUIText.text = "Measure: Select 2nd point";
                }
                haveFirstPoint = true;
                haveSecondPoint = false;
            }
            else
            { // it was 2nd click
                endPos = pos;
                haveSecondPoint = true;

                var distance = Vector3.Distance(previousPoint, pos);
                if (distanceUIText != null)
                {
                    distanceUIText.text = "Distance:" + distance.ToString();
                }
                Debug.LogFormat(LOG_FORMAT, "Distance:" + distance);
            }

            previousPoint = pos;
            isFirstPoint = !isFirstPoint; // flip boolean
        }

        public override void DrawLine()
        {
            Debug.LogFormat(LOG_FORMAT, "DrawLine()");

            _GLDebug.Instance.DrawLine(startPos, endPos, lineColor, 0, true);
        }
    }
}