using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;

namespace WorldSpaceTransitions.Examples
{
    public class PieSectionExample : MonoBehaviour
    {

        //private Material sMat;
        public GameObject model;
        private Vector3 normal1;
        private Vector3 normal2;
        public float startAngle = 0f;
        public float maxAngle = 360f;
        [Range(0.1f, 5)]
        public float angleIncrement = 0.5f;
        public Slider pieSlider;
        //public Transform PieSectionPrefab;

        Transform quadPlane1;
        Transform quadPlane2;
        Quaternion startPieRotation;
        float angle = 0f;


        // Use this for initialization
        void Start()
        {
            Shader.DisableKeyword("CLIP_NONE");
            Shader.EnableKeyword("CLIP_PIE");
            //we have declared: "material.EnableKeyword("CLIP_PLANE");" on all the crossSectionStandard derived materials - in the CrossSectionStandardShaderGUI editor script - so we have to switch it off
            if (model)
            {
                Renderer[] allrenderers = model.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in allrenderers)
                {
                    Material[] mats = r.sharedMaterials;
                    foreach (Material m in mats) m.DisableKeyword("CLIP_PLANE");
                }
            }
            quadPlane1 = transform.Find("quad1");
            quadPlane2 = transform.Find("quad2");
            //quadPlane1.localRotation = Quaternion.Euler(0, -quadPlane2.localEulerAngles.y,0);
            Shader.SetGlobalMatrix("_WorldToObjectMatrix", transform.worldToLocalMatrix);
            Shader.SetGlobalFloat("_pieAngle", 2 * Mathf.Deg2Rad * quadPlane2.localEulerAngles.y);
            startPieRotation = transform.rotation;
            transform.RotateAround(transform.position, transform.up, startAngle / 2);
            Application.targetFrameRate = 60;
            if (pieSlider)
            {
                //clippingSphere.localScale = radiusSlider.value * 2 * Vector3.one;//get initial values from UI
                //radiusValueText.text = radiusSlider.value.ToString("0.00");
                pieSlider.value = maxAngle / 360;
                pieSlider.onValueChanged.AddListener(delegate
                {
                    maxAngle = 360 * pieSlider.value;
                });
            }
        }

        // Update is called once per frame
        void LateUpdate()
        {
            //return;
            if (angle > 360)
            {
                angle = 0;
                transform.rotation = startPieRotation;
                transform.RotateAround(transform.position, transform.up, startAngle / 2);
                quadPlane1.localRotation = Quaternion.Euler(0, -startAngle / 2, 0);
                quadPlane2.localRotation = Quaternion.Euler(0, startAngle / 2, 0);
                Shader.SetGlobalMatrix("_WorldToObjectMatrix", transform.worldToLocalMatrix);
                Shader.SetGlobalFloat("_pieAngle", Mathf.Deg2Rad * startAngle);
            }
            angle += angleIncrement;
            if (angle < startAngle) return;
            if (angle > maxAngle) return;

            transform.RotateAround(transform.position, transform.up, angleIncrement / 2);
            quadPlane1.localRotation = Quaternion.Euler(0, -angle / 2, 0);
            quadPlane2.localRotation = Quaternion.Euler(0, angle / 2, 0);
            Shader.SetGlobalMatrix("_WorldToObjectMatrix", transform.worldToLocalMatrix);
            Shader.SetGlobalFloat("_pieAngle", Mathf.Deg2Rad * angle);
        }

        void OnEnable()
        {
            Shader.DisableKeyword("CLIP_NONE");
            Shader.EnableKeyword("CLIP_PIE");
            //Shader.EnableKeyword("CLIP_PLANE");
        }

        void OnDisable()
        {
            Shader.DisableKeyword("CLIP_PIE");
            Shader.EnableKeyword("CLIP_NONE");
        }

        void OnApplicationQuit()
        {
            //disable clipping so we could see the materials and objects in editor properly
            Shader.DisableKeyword("CLIP_PIE");
            Shader.EnableKeyword("CLIP_NONE");
        }


        IEnumerator drag()
        {
            float cameraDistance = Vector3.Distance(transform.position, Camera.main.transform.position);
            Vector3 startPoint = Camera.main.ScreenToWorldPoint(new Vector3(
#if ENABLE_INPUT_SYSTEM
            Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y
#else
                //#endif
                //#if ENABLE_LEGACY_INPUT_MANAGER
                Input.mousePosition.x, Input.mousePosition.y
#endif
            , cameraDistance));
            Vector3 startNormal = normal1;
            Vector3 translation = Vector3.zero;
            Camera.main.GetComponent<MaxCamera>().enabled = false;
            while (
#if ENABLE_INPUT_SYSTEM
            Mouse.current.leftButton.isPressed
#else
                //#endif
                //#if ENABLE_LEGACY_INPUT_MANAGER
                Input.GetMouseButton(0)
#endif
            )
            {
                translation = Camera.main.ScreenToWorldPoint(new Vector3(
#if ENABLE_INPUT_SYSTEM
                Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y
#else
                    //#endif
                    //#if ENABLE_LEGACY_INPUT_MANAGER
                    Input.mousePosition.x, Input.mousePosition.y
#endif
                , cameraDistance)) - startPoint;
                float val = Vector3.Dot(translation, transform.up);
                if (val > 0 && maxAngle < 360)
                {
                    if (maxAngle < 360)
                    {
                        maxAngle += angleIncrement;
                        angle += angleIncrement;
                        transform.RotateAround(transform.position, transform.up, +angleIncrement / 2);
                    }
                }
                else
                {
                    if (maxAngle > startAngle)
                    {
                        maxAngle -= angleIncrement;
                        angle -= angleIncrement;
                        transform.RotateAround(transform.position, transform.up, -angleIncrement / 2);
                    }
                }
                if (angle < 0) yield return null;
                yield return null;
            }
            Camera.main.GetComponent<MaxCamera>().enabled = true;

        }

    }
}