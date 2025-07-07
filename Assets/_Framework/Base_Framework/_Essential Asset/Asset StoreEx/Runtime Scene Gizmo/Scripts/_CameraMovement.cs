using System.Collections;
using _InputTouches;
using UnityEngine;

namespace RuntimeSceneGizmo
{
	public class _CameraMovement : CameraMovement
    {
        private static string LOG_FORMAT = "<color=#48ADF3><b>[_CameraMovement]</b></color> {0}";

        // protected Transform mainCamParent;
        protected Transform camParentTransform
        {
            get
            {
                return _RuntimeGizmoManager.Instance.CamParentTransform;
            }
        }

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            // mainCamParent = Camera.main.transform.parent;
            mainCamParent = null; // Not used!!!!!

            StartCoroutine(PostAwake());
        }

        protected virtual IEnumerator PostAwake()
        {
            while (_RuntimeGizmoManager.Instance == null)
            {
                Debug.LogFormat(LOG_FORMAT, "");
                yield return null;
            }
        }

        protected override void Update()
        {
            // if (Input.GetMouseButtonDown(0))
            if (Input.GetMouseButtonDown(_IT_Gesture.Mouse_Left_Button))
            {
                prevMousePos = Input.mousePosition;
            }
            // else if (Input.GetMouseButton(0))
            else if (Input.GetMouseButton(_IT_Gesture.Mouse_Left_Button))
            {
                Vector3 mousePos = Input.mousePosition;
                Vector2 deltaPos = (mousePos - prevMousePos) * sensitivity;

                Vector3 rot = camParentTransform.localEulerAngles;
                while (rot.x > 180f)
                {
                    rot.x -= 360f;
                }

                while (rot.x < -180f)
                {
                    rot.x += 360f;
                }

                rot.x = Mathf.Clamp(rot.x - deltaPos.y, -89.8f, 89.8f);
                rot.y += deltaPos.x;
                rot.z = 0f;

                camParentTransform.localEulerAngles = rot;
                prevMousePos = mousePos;
            }
        }
    }
}