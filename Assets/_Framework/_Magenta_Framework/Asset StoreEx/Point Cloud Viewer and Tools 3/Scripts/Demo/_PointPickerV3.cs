// point picking example for v3 tiles viewer
// this script would change in future, so for now i'd suggest duplicate this script and make your own logic to manage first and 2nd pick

using mgear.tools;
using PointCloudHelpers;
using pointcloudviewer.binaryviewer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PointCloudExtras
{

    public class _PointPickerV3 : PointPickerV3
    {
        [SerializeField]
        protected Camera _camera;

        protected virtual void OnDestroy()
        {
            viewer.PointWasSelected -= PointSelected;
        }

        protected override void Start()
        {
            // cam = Camera.main;
            cam = _camera;

            // viewer.PointWasSelected -= PointSelected;
            viewer.PointWasSelected += PointSelected;
        }
    }
}