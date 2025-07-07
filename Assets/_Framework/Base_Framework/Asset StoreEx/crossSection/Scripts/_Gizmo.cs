using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using _Base_Framework;

namespace AdvancedGizmo
{
    public class _Gizmo : Gizmo
    {
        private static string LOG_FORMAT = "<color=#42DFE3><b>[_Gizmo]</b></color> {0}";

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

        protected override void LateUpdate()
        {
            // base.LateUpdate();

            /*
            bool mouseOutsideGUI = true;
            if (ES) 
                mouseOutsideGUI = !EventSystem.current.IsPointerOverGameObject();
            */
            bool mouseOutsideGUI = !_EventSystem.IsPointerOverGameObject();

            if (Input.GetMouseButtonDown(1) && mouseOutsideGUI)
            {
                ray = _Camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 1000f, layer))
                {
                    if (hit.transform.parent == transform)
                        ChangeMode();
                }
            }

            if (Input.GetMouseButtonDown(0) && mouseOutsideGUI)
            {
                ray = _Camera.ScreenPointToRay(Input.mousePosition);
                dragplane = new Plane();

                if (Physics.Raycast(ray, out hit, 1000f, layer))
                {
                    hitvector = hit.point - transform.position;

                    if (hit.collider == xAxis)
                    {
                        selectedAxis = GizmoAxis.X;
                        dragplane.SetNormalAndPosition(transform.up, transform.position);
                    }
                    else if (hit.collider == yAxis)
                    {
                        selectedAxis = GizmoAxis.Y;
                        dragplane.SetNormalAndPosition(transform.forward, transform.position);
                    }
                    else if (hit.collider == zAxis)
                    {
                        selectedAxis = GizmoAxis.Z;
                        dragplane.SetNormalAndPosition(transform.up, transform.position);
                    }
                    else if (hit.collider == xyPlane)
                    {
                        selectedAxis = GizmoAxis.XY;
                        dragplane.SetNormalAndPosition(transform.forward, transform.position);
                    }
                    else if (hit.collider == xzPlane)
                    {
                        selectedAxis = GizmoAxis.XZ;
                        dragplane.SetNormalAndPosition(transform.up, transform.position);
                    }
                    else if (hit.collider == yzPlane)
                    {
                        selectedAxis = GizmoAxis.YZ;
                        dragplane.SetNormalAndPosition(transform.right, transform.position);
                    }
                    else if (hit.collider == xyRotate)
                    {
                        selectedAxis = GizmoAxis.XYRotate;
                        rotationAxis = -transform.forward;
                        lookHitPoint = hit.point - transform.position - Vector3.Project(hit.point - transform.position, transform.forward);
                        //Debug.DrawLine(transform.position, transform.position + lookHitPoint, Color.cyan, 10.0f);
                        dragplane.SetNormalAndPosition(lookHitPoint, hit.point);
                        //DrawPlane(lookHitPoint, transform.position + lookHitPoint, 10.0f);
                        rotating = true;
                    }
                    else if (hit.collider == xzRotate)
                    {
                        selectedAxis = GizmoAxis.XZRotate;
                        rotationAxis = -transform.up;
                        lookHitPoint = hit.point - transform.position - Vector3.Project(hit.point - transform.position, transform.up);
                        //Debug.DrawLine(transform.position, transform.position + lookHitPoint, Color.cyan, 10.0f);
                        dragplane.SetNormalAndPosition(lookHitPoint, hit.point);
                        //DrawPlane(lookHitPoint, transform.position + lookHitPoint, 10.0f);
                        rotating = true;
                    }
                    else if (hit.collider == yzRotate)
                    {
                        selectedAxis = GizmoAxis.YZRotate;
                        rotationAxis = -transform.right;
                        lookHitPoint = hit.point - transform.position - Vector3.Project(hit.point - transform.position, transform.right);
                        //Debug.DrawLine(transform.position, transform.position + lookHitPoint, Color.cyan, 10.0f);
                        dragplane.SetNormalAndPosition(lookHitPoint, hit.point);
                        //DrawPlane(lookHitPoint, transform.position + lookHitPoint, 10.0f);
                        rotating = true;
                    }
                    else if (hit.collider == sphereRotate)
                    {
                        selectedAxis = GizmoAxis.none;
                        lookCamera = _Camera.transform.position - transform.position;
                        startDragRot = transform.position + lookCamera.normalized * sphereRadius;
                        dragplane.SetNormalAndPosition(lookCamera.normalized, startDragRot);
                        //DrawPlane(lookCamera.normalized, startDragRot, 10.0f);
                        rotating = true;
                    }
                    else
                    {
                        Debug.LogFormat(LOG_FORMAT, hit.collider.name);
                        return;
                    }

                    if (rotating == true)
                    {
                        if (dragplane.Raycast(ray, out rayDistance))
                        {
                            startDragRot = ray.GetPoint(rayDistance);
                        }
                        rotationParent = new GameObject();
                        rotationParent.transform.position = transform.position;
                        transform.SetParent(rotationParent.transform);
                    }

                    distance = hit.distance;
                    startDrag = _Camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance));
                    startPos = transform.position;
                    dragging = true;
                }
            }

            if (dragging == true || rotating == true)
            {
                ray = _Camera.ScreenPointToRay(Input.mousePosition);

                if (dragplane.Raycast(ray, out rayDistance))
                {
                    mousePos = ray.GetPoint(rayDistance);
                }

                Vector3 onDrag = _Camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance));
                Vector3 translation = onDrag - startDrag;
                Vector3 projectedTranslation = Vector3.zero;

                if (dragging)
                {
                    switch (selectedAxis)
                    {
                        case GizmoAxis.X:
                            {
                                projectedTranslation = Vector3.Project(translation, transform.right);
                                transform.position = startPos + projectedTranslation.normalized * translation.magnitude;
                                break;
                            }
                        case GizmoAxis.Y:
                            {
                                projectedTranslation = Vector3.Project(translation, transform.up);
                                transform.position = startPos + projectedTranslation.normalized * translation.magnitude;
                                break;
                            }
                        case GizmoAxis.Z:
                            {
                                projectedTranslation = Vector3.Project(translation, transform.forward);
                                transform.position = startPos + projectedTranslation.normalized * translation.magnitude;
                                break;
                            }
                        case GizmoAxis.XY:
                        case GizmoAxis.XZ:
                        case GizmoAxis.YZ:
                            {
                                transform.position = mousePos - hitvector;
                                break;
                            }

                        default:
                            break;
                    }
                }

                if (rotating == true)
                {
                    translation = mousePos - startDragRot;
                    //Debug.DrawLine(startDragRot, mousePos, Color.white, 6.0f);

                    Vector3 rotationAxis2 = Vector3.zero;

                    switch (selectedAxis)
                    {
                        case GizmoAxis.XYRotate:
                            {
                                projectedTranslation = translation - Vector3.Project(translation, rotationAxis);
                                //Debug.DrawLine(startDragRot, startDragRot + projectedTranslation, Color.yellow, 1.0f);
                                rotationAxis2 = Vector3.Cross(projectedTranslation, lookHitPoint);
                                //Debug.DrawLine(transform.position, transform.position + 5 * rotationAxis2.normalized, Color.magenta, 1.0f);
                                //Debug.DrawLine(transform.position, transform.position + 5 * rotationAxis.normalized, Color.cyan, 1.0f);
                                break;
                            }
                        case GizmoAxis.XZRotate:
                            {
                                projectedTranslation = translation - Vector3.Project(translation, rotationAxis);
                                //Debug.DrawLine(startDragRot, startDragRot + projectedTranslation, Color.yellow, 1.0f);
                                rotationAxis2 = Vector3.Cross(projectedTranslation, lookHitPoint);
                                //Debug.DrawLine(transform.position, transform.position + 5 * rotationAxis2.normalized, Color.magenta, 1.0f);
                                //Debug.DrawLine(transform.position, transform.position + 5 * rotationAxis.normalized, Color.magenta, 1.0f);
                                break;
                            }
                        case GizmoAxis.YZRotate:
                            {
                                projectedTranslation = translation - Vector3.Project(translation, rotationAxis);
                                //Debug.DrawLine(startDragRot, startDragRot + projectedTranslation, Color.yellow, 1.0f);
                                rotationAxis2 = Vector3.Cross(projectedTranslation, lookHitPoint);

                                //Debug.DrawLine(transform.position, transform.position + 5 * rotationAxis.normalized, Color.gray, 1.0f);
                                break;
                            }
                        case GizmoAxis.none:
                            {
                                rotationAxis2 = rotationAxis;
                                projectedTranslation = translation;
                                rotationAxis = Vector3.Cross(translation, lookCamera);
                                break;
                            }

                        default:
                            break;
                    }

                    float angle;
                    Quaternion delta;

                    //Debug.DrawLine(transform.position, transform.position + 5 * rotationAxis.normalized, Color.red, 1.0f);
                    angle = -Mathf.Rad2Deg * projectedTranslation.magnitude / sphereRadius;
                    delta = Quaternion.AngleAxis(angle, rotationAxis2);

                    if (selectedAxis == GizmoAxis.none)
                    {
                        rotationParent.transform.rotation = delta;
                    }
                    else
                    {
                        rotationParent.transform.rotation = delta;
                    }
                }

                if (Input.GetMouseButtonUp(0))
                    SetFalse();

            }

        }
    }
}