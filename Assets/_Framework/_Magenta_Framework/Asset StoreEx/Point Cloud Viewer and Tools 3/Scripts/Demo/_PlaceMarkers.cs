// example script for placing markers on selected points

using UnityEngine;
using UnityEngine.UI;
using PointCloudViewer;
using UnityEngine.EventSystems;

namespace pointcloudviewer.examples
{
    public class _PlaceMarkers : PlaceMarkers
    {
        private static string LOG_FORMAT = "<color=#00FF90><b>[_PlaceMarkers]</b></color> {0}";

        protected virtual void Awake()
        {
            cam = null; // Do not use!!!!!
            pointCloudManager = null; // Do not use!!!!!
        }

        protected override void OnDestroy()
        {
            // unsubscribe
            _PointCloudManager.PointWasSelected -= OnPointSelected;
        }

        protected override void Start()
        {
            // cam = _PointCloudManager.Instance._Camera;

            // subscribe to event listener
            // PointCloudManager.PointWasSelected -= PointSelected; // unsubscribe just in case
            _PointCloudManager.PointWasSelected += OnPointSelected;
        }

        public override void Update()
        {
            // dont do pick if over UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            // left button click to select
            if (Input.GetMouseButtonDown(0))
            {
                // Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                Ray ray = _PointCloudManager.Instance._Camera.ScreenPointToRay(Input.mousePosition);
                //ray.origin = ray.GetPoint(cam.nearClipPlane + 0.05f);

                // call threaded point picker, you can call this manually from your own scripts, by passing your own ray
                _PointCloudManager.Instance.RunPointPickingThread(ray);
            }
        }

        protected override void PointSelected(Vector3 pos)
        {
            throw new System.NotSupportedException("");
        }

        protected virtual void OnPointSelected(Vector3 pos)
        {
            // we got point selected event, lets instantiate object there
            Debug.LogFormat(LOG_FORMAT, "OnPointSelected(), pos : " + pos);
            var go = Instantiate(prefab, pos + offset, Quaternion.identity) as GameObject;

            // fix for 2017.x
            go.transform.position = pos + offset;

            // find text component from our prefab
            var txt = go.GetComponentInChildren<Text>();
            if (txt != null)
            {
                // 2 decimals
                txt.text = (pos).ToString("0.0#");
            }
        }
    }
}