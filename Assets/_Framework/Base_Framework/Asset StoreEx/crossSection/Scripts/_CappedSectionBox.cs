//The purpose of this script is to manipulate the scale and position of the capped section box gizmo object 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldSpaceTransitions
{
    public class _CappedSectionBox : CappedSectionBox
    {
        private static string LOG_FORMAT = "<color=#AB3DA5><b>[_CappedSectionBox]</b></color> {0}";

        [Space(10)]
        [SerializeField]
        protected Camera _cam;
        public Camera _Camera
        {
            get
            {
                return _cam;
            }
            set
            {
                _cam = value;
            }
        }

        protected virtual void Awake()
        {
            //
        }

        protected override void Update()
        {
            // base.Update();

            if (Input.GetMouseButtonDown(0))
            {
                ray = _Camera.ScreenPointToRay(Input.mousePosition);
                dragplane = new Plane();

                RaycastHit hit;
                if (xAxis.Raycast(ray, out hit, 1000f))
                {
                    selectedAxis = GizmoAxis.X;
                    dragplane.SetNormalAndPosition(transform.up, transform.position);
                }
                else if (xAxisNeg.Raycast(ray, out hit, 1000f))
                {
                    selectedAxis = GizmoAxis.Xneg;
                    dragplane.SetNormalAndPosition(-transform.up, transform.position);
                }
                else if (yAxis.Raycast(ray, out hit, 1000f))
                {
                    selectedAxis = GizmoAxis.Y;
                    dragplane.SetNormalAndPosition(transform.forward, transform.position);
                }
                else if (yAxisNeg.Raycast(ray, out hit, 1000f))
                {
                    selectedAxis = GizmoAxis.Yneg;
                    dragplane.SetNormalAndPosition(-transform.forward, transform.position);
                }
                else if (zAxis.Raycast(ray, out hit, 1000f))
                {
                    selectedAxis = GizmoAxis.Z;
                    dragplane.SetNormalAndPosition(transform.up, transform.position);
                }
                else if (zAxisNeg.Raycast(ray, out hit, 1000f))
                {
                    selectedAxis = GizmoAxis.Zneg;
                    dragplane.SetNormalAndPosition(-transform.up, transform.position);
                }
                else
                {
                    //Debug.Log(hit.collider.name);
                    return;
                }
                distance = hit.distance;
                startDrag = _Camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance));
                startPos = transform.position;
                startScale = transform.localScale;
                dragging = true;
            }

            if (dragging == true)
            {
                ray = _Camera.ScreenPointToRay(Input.mousePosition);

                Vector3 onDrag = _Camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance));
                Vector3 translation = onDrag - startDrag;
                Vector3 projectedTranslation = Vector3.zero;

                // if (dragging == true)
                {
                    float lsx = startScale.x;
                    float lsy = startScale.y;
                    float lsz = startScale.z;

                    switch (selectedAxis)
                    {
                        case GizmoAxis.X:
                            {
                                projectedTranslation = Vector3.Project(translation, transform.right);
                                transform.position = startPos + 0.5f * (projectedTranslation.normalized * translation.magnitude);
                                lsx += translation.magnitude * Mathf.Sign(Vector3.Dot(projectedTranslation, transform.right));
                                break;
                            }
                        case GizmoAxis.Xneg:
                            {
                                projectedTranslation = Vector3.Project(translation, -transform.right);
                                transform.position = startPos + 0.5f * (projectedTranslation.normalized * translation.magnitude);
                                lsx += translation.magnitude * Mathf.Sign(Vector3.Dot(projectedTranslation, -transform.right));
                                break;
                            }
                        case GizmoAxis.Y:
                            {
                                projectedTranslation = Vector3.Project(translation, transform.up);
                                transform.position = startPos + 0.5f * (projectedTranslation.normalized * translation.magnitude);
                                lsy += translation.magnitude * Mathf.Sign(Vector3.Dot(projectedTranslation, transform.up));
                                break;
                            }
                        case GizmoAxis.Yneg:
                            {
                                projectedTranslation = Vector3.Project(translation, -transform.up);
                                transform.position = startPos + 0.5f * (projectedTranslation.normalized * translation.magnitude);
                                lsy += translation.magnitude * Mathf.Sign(Vector3.Dot(projectedTranslation, -transform.up));
                                break;
                            }
                        case GizmoAxis.Z:
                            {
                                projectedTranslation = Vector3.Project(translation, transform.forward);
                                transform.position = startPos + 0.5f * (projectedTranslation.normalized * translation.magnitude);
                                lsz += translation.magnitude * Mathf.Sign(Vector3.Dot(projectedTranslation, transform.forward));
                                break;
                            }
                        case GizmoAxis.Zneg:
                            {
                                projectedTranslation = Vector3.Project(translation, -transform.forward);
                                transform.position = startPos + 0.5f * (projectedTranslation.normalized * translation.magnitude);
                                lsz += translation.magnitude * Mathf.Sign(Vector3.Dot(projectedTranslation, -transform.forward));
                                break;
                            }

                    }

                    transform.localScale = new Vector3(Mathf.Clamp(lsx, 0.01f, Mathf.Infinity), Mathf.Clamp(lsy, 0.01f, Mathf.Infinity), Mathf.Clamp(lsz, 0.01f, Mathf.Infinity));

                    //foreach (UVScaler uvs in gameObject.GetComponentsInChildren<UVScaler>()) uvs.SetUV();
                }

                if (Input.GetMouseButtonUp(0))
                {
                    dragging = false;
                }
            }
        }

        public override void Size(Bounds bounds, GameObject g, BoundsOrientation orientation)
        {
            Debug.LogFormat(LOG_FORMAT, "Size(), bounds : " + bounds);

            float scale = 1f;

            Vector3 clearance = 0.01f * Vector3.one;

            // this.transform.localScale = Vector3.one;
            this.transform.localScale = scale * bounds.size + clearance;
            this.transform.position = bounds.center;
        }
    }
}