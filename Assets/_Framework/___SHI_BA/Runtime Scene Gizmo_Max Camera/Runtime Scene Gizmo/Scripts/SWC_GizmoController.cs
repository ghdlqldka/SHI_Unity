using RuntimeSceneGizmo;
using UnityEngine;

namespace _SHI_BA
{

	public class SWC_GizmoController : _Magenta_WebGL.MWGL_GizmoController
    {
        private static string LOG_FORMAT = "<color=#F36D48><b>[MWGL_GizmoController]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), <color=red>GIZMOS_LAYER : <b>" + GIZMOS_LAYER + "</b></color>");

            gizmoCamParent = gizmoCamera.transform.parent;
            labelsTR = new Transform[labels.Length];

            int gizmoResolution = Mathf.Min(Mathf.NextPowerOfTwo(Mathf.Max(Screen.width, Screen.height) / 6), 512);
            TargetTexture = new RenderTexture(gizmoResolution, gizmoResolution, 16);
            gizmoCamera.aspect = 1f;
            gizmoCamera.targetTexture = TargetTexture;
            gizmoCamera.cullingMask = 1 << GIZMOS_LAYER;
            gizmoCamera.eventMask = 0;
            gizmoCamera.enabled = false;

            gizmoNormalMaterial = gizmoComponents[0].sharedMaterial;
            gizmoFadeMaterial = new Material(gizmoNormalMaterial);
            gizmoMaterialFadeProperty = Shader.PropertyToID("_AlphaVal");

            gizmoHighlightMaterial = new Material(gizmoNormalMaterial);
            gizmoHighlightMaterial.EnableKeyword("HIGHLIGHT_ON");
            gizmoHighlightMaterial.color = Color.yellow;

            for (int i = 0; i < gizmoComponents.Length; i++)
            {
                gizmoComponents[i].gameObject.layer = GIZMOS_LAYER;
            }

            for (int i = 0; i < labelsTR.Length; i++)
            {
                labels[i].gameObject.layer = GIZMOS_LAYER;
                labelsTR[i] = labels[i].transform;
            }
        }

        public override void OnPointerHover(Vector3 normalizedPosition)
        {
            // Set highlighted component
            GizmoComponent hitComponent = Raycast(normalizedPosition);
            if (hitComponent != GizmoComponent.None)
            {
                if (hitComponent != highlightedComponent)
                {
                    if (highlightedComponent != GizmoComponent.None)
                        gizmoComponents[(int)highlightedComponent].sharedMaterial = gizmoNormalMaterial;

                    if (hitComponent != fadingComponent)
                    {
                        highlightedComponent = hitComponent;
                        gizmoComponents[(int)highlightedComponent].sharedMaterial = gizmoHighlightMaterial;
                    }
                    else
                        highlightedComponent = GizmoComponent.None;

                    updateTargetTexture = true;
                }
            }
            else if (highlightedComponent != GizmoComponent.None)
            {
                gizmoComponents[(int)highlightedComponent].sharedMaterial = gizmoNormalMaterial;
                highlightedComponent = GizmoComponent.None;

                updateTargetTexture = true;
            }
        }

        protected override void LateUpdate()
        {
            if (m_referenceTransform == null)
            {
                // ReferenceTransform = Camera.main.transform;
                ReferenceTransform = _RuntimeGizmoManager.Instance._Camera.transform;

                if (m_referenceTransform == null)
                {
                    Debug.LogError("ReferenceTransform mustn't be null!");
                    return;
                }
            }

            Vector3 forward = m_referenceTransform.forward;
            if (prevForward != forward)
            {
                float xAbs = forward.x < 0 ? -forward.x : forward.x;
                float yAbs = forward.y < 0 ? -forward.y : forward.y;
                float zAbs = forward.z < 0 ? -forward.z : forward.z;

                GizmoComponent facingDirection;
                float facingDirectionCosine;
                if (xAbs > yAbs)
                {
                    if (xAbs > zAbs)
                    {
                        facingDirection = forward.x > 0 ? GizmoComponent.XPositive : GizmoComponent.XNegative;
                        facingDirectionCosine = Vector3.Dot(forward, new Vector3(1f, 0f, 0f));
                    }
                    else
                    {
                        facingDirection = forward.z > 0 ? GizmoComponent.ZPositive : GizmoComponent.ZNegative;
                        facingDirectionCosine = Vector3.Dot(forward, new Vector3(0f, 0f, 1f));
                    }
                }
                else if (yAbs > zAbs)
                {
                    facingDirection = forward.y > 0 ? GizmoComponent.YPositive : GizmoComponent.YNegative;
                    facingDirectionCosine = Vector3.Dot(forward, new Vector3(0f, 1f, 0f));
                }
                else
                {
                    facingDirection = forward.z > 0 ? GizmoComponent.ZPositive : GizmoComponent.ZNegative;
                    facingDirectionCosine = Vector3.Dot(forward, new Vector3(0f, 0f, 1f));
                }

                if (facingDirectionCosine < 0)
                    facingDirectionCosine = -facingDirectionCosine;

                if (facingDirectionCosine >= 0.92f) // cos(20)
                    SetHiddenComponent(GetOppositeComponent(facingDirection));
                else
                    SetHiddenComponent(GizmoComponent.None);

                Quaternion rotation = m_referenceTransform.rotation;
                gizmoCamParent.localRotation = rotation;

                // Adjust the labels
                float xLabelPos = (xAbs - 0.15f) * 0.65f;
                float yLabelPos = (yAbs - 0.15f) * 0.65f;
                float zLabelPos = (zAbs - 0.15f) * 0.65f;

                if (xLabelPos < 0f)
                    xLabelPos = 0f;
                if (yLabelPos < 0f)
                    yLabelPos = 0f;
                if (zLabelPos < 0f)
                    zLabelPos = 0f;

                labelsTR[0].localPosition = new Vector3(0f, 0f, xLabelPos);
                labelsTR[1].localPosition = new Vector3(0f, 0f, yLabelPos);
                labelsTR[2].localPosition = new Vector3(0f, 0f, zLabelPos);

                labelsTR[0].rotation = rotation;
                labelsTR[1].rotation = rotation;
                labelsTR[2].rotation = rotation;

                updateTargetTexture = true;
                prevForward = forward;
            }

            if (fadeT < 1f)
            {
                fadeT += Time.unscaledDeltaTime * 4f;
                if (fadeT >= 1f)
                    fadeT = 1f;

                SetAlphaOf(fadingComponent, isFadingToZero ? 1f - fadeT : fadeT);
                if (fadeT >= 1f)
                {
                    if (!isFadingToZero)
                    {
                        SetMaterialOf(fadingComponent, gizmoNormalMaterial);
                        fadingComponent = GizmoComponent.None;
                    }
                    else
                    {
                        gizmoComponents[(int)fadingComponent].gameObject.SetActive(false);
                        gizmoComponents[(int)GetOppositeComponent(fadingComponent)].gameObject.SetActive(false);
                    }
                }

                updateTargetTexture = true;
            }

            if (updateTargetTexture)
            {
                gizmoCamera.Render();
                updateTargetTexture = false;
            }
        }
    }
}