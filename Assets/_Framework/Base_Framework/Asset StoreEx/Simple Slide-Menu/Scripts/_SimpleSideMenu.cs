// Simple Side-Menu - https://assetstore.unity.com/packages/tools/gui/simple-side-menu-143623
// Copyright (c) Daniel Lochner

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DanielLochner.Assets.SimpleSideMenu
{
    [AddComponentMenu("UI/_Simple Side-Menu")]
    public class _SimpleSideMenu : SimpleSideMenu
    {
        private static string LOG_FORMAT = "<color=#B1C5EF><b>[_SimpleSideMenu]</b></color> {0}";

        [SerializeField]
        protected _Placement placementEx = _Placement.Left;

        [SerializeField]
        protected Sprite openHandleSprite;
        [SerializeField]
        protected Sprite closeHandleSprite;

        public override float StateProgress
        {
            get
            {
                bool isLeftOrRight = (placementEx == _Placement.Left) || (placementEx == _Placement.Right) || (placementEx == _Placement.RightBottom);
                return ((rectTransform.anchoredPosition - closedPosition).magnitude / (isLeftOrRight ? rectTransform.rect.width : rectTransform.rect.height));
            }
        }

        [SerializeField]
        protected State currentState;
        public override State CurrentState
        {
            get
            {
                return currentState;
            }
            protected set
            {
                currentState = value;
            }
        }

        [SerializeField]
        protected State targetState;
        public override State TargetState
        {
            get
            {
                return targetState;
            }
            protected set
            {
                if (targetState != value)
                {
                    Debug.LogFormat(LOG_FORMAT, "TargetState : " + value);
                    targetState = value;
                    if (value == State.Closed)
                    {
                        handle.GetComponent<Image>().sprite = openHandleSprite;
                    }
                    else if (value == State.Open)
                    {
                        handle.GetComponent<Image>().sprite = closeHandleSprite;
                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                    onStateSelected.Invoke(TargetState);
                }
                else
                {
                    Debug.LogWarningFormat(LOG_FORMAT, "<color=yellow><b>IGNORE for DUPLICATION</b></color>, already targetState : <b><color=yellow>" + value + "</color></b>");
                }
            }
        }

        public Vector2 ClosedPosition
        {
            get
            {
                return closedPosition;
            }
        }

        public Vector2 OpenPosition
        {
            get
            {
                return openPosition;
            }
        }

        protected override bool IsValidConfig
        {
            get
            {
                bool valid = true;

                if (transitionSpeed <= 0)
                {
                    Debug.LogErrorFormat(LOG_FORMAT, "Transition speed cannot be less than or equal to zero.", gameObject);
                    valid = false;
                }
                if (handle != null && isHandleDraggable && handle.transform.parent != rectTransform)
                {
                    Debug.LogErrorFormat(LOG_FORMAT, "The drag handle must be a child of the side menu in order for it to be draggable.", gameObject);
                    valid = false;
                }
                if (handleToggleStateOnPressed && handle.GetComponent<Button>() == null)
                {
                    Debug.LogErrorFormat(LOG_FORMAT, "The handle must have a \"Button\" component attached to it in order for it to be able to toggle the state of the side menu when pressed.", gameObject);
                    valid = false;
                }

                return valid;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            // Initialize();
        }
#endif

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            // base.Awake();
            Initialize();
        }

        protected override void Update()
        {
            HandleState();
            HandleOverlay();
        }

        protected override void Initialize()
        {
            Debug.LogFormat(LOG_FORMAT, "Initialize()");

            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();

            if (canvas != null)
            {
                // canvasScaler = canvas.GetComponent<CanvasScaler>(); // Canvas Scaler is NOT used!!!!!
                canvasRectTransform = canvas.GetComponent<RectTransform>();
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "Canvas does NOT exist!!!!!");
            }
        }

        protected override void Setup()
        {
            Debug.LogFormat(LOG_FORMAT, "Setup()");

            // Canvas and Camera
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                canvas.planeDistance = (canvasRectTransform.rect.height / 2f) / Mathf.Tan((canvas.worldCamera.fieldOfView / 2f) * Mathf.Deg2Rad);
                if (canvas.worldCamera.farClipPlane < canvas.planeDistance)
                {
                    canvas.worldCamera.farClipPlane = Mathf.Ceil(canvas.planeDistance);
                }
            }

            // Placement
            Vector2 anchorMin = Vector2.zero;
            Vector2 anchorMax = Vector2.zero;
            Vector2 pivot = Vector2.zero;
            switch (placementEx)
            {
                case _Placement.Left:
                    anchorMin = new Vector2(0, 0.5f);
                    anchorMax = new Vector2(0, 0.5f);
                    pivot = new Vector2(1, 0.5f);
                    closedPosition = new Vector2(0, rectTransform.localPosition.y);
                    openPosition = new Vector2(rectTransform.rect.width, rectTransform.localPosition.y);
                    break;

                case _Placement.Right:
                    anchorMin = new Vector2(1, 0.5f);
                    anchorMax = new Vector2(1, 0.5f);
                    pivot = new Vector2(0, 0.5f);
                    closedPosition = new Vector2(0, rectTransform.localPosition.y);
                    openPosition = new Vector2(-1 * rectTransform.rect.width, rectTransform.localPosition.y);
                    break;

                case _Placement.RightBottom:
                    anchorMin = new Vector2(1, 0f);
                    anchorMax = new Vector2(1, 0f);
                    pivot = new Vector2(0, 0f);
                    closedPosition = new Vector2(0, 0);
                    openPosition = new Vector2(-1 * rectTransform.rect.width, 0);
                    break;

                case _Placement.Top:
                    anchorMin = new Vector2(0.5f, 1);
                    anchorMax = new Vector2(0.5f, 1);
                    pivot = new Vector2(0.5f, 0);
                    closedPosition = new Vector2(rectTransform.localPosition.x, 0);
                    openPosition = new Vector2(rectTransform.localPosition.x, -1 * rectTransform.rect.height);
                    break;

                case _Placement.Bottom:
                    anchorMin = new Vector2(0.5f, 0);
                    anchorMax = new Vector2(0.5f, 0);
                    pivot = new Vector2(0.5f, 1);
                    closedPosition = new Vector2(rectTransform.localPosition.x, 0);
                    openPosition = new Vector2(rectTransform.localPosition.x, rectTransform.rect.height);
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }

            rectTransform.sizeDelta = rectTransform.rect.size;
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;

            // Default State
            SetState(CurrentState = defaultState);
            if (CurrentState == State.Closed)
            {
                handle.GetComponent<Image>().sprite = openHandleSprite;
            }
            else if (CurrentState == State.Open)
            {
                handle.GetComponent<Image>().sprite = closeHandleSprite;
            }
            else
            {
                Debug.Assert(false);
            }

            rectTransform.anchoredPosition = (defaultState == State.Closed) ? closedPosition : openPosition;

            Debug.LogFormat(LOG_FORMAT, "rectTransform.anchorMin : " + rectTransform.anchorMin + ", anchorMax : " + rectTransform.anchorMax + ", pivot : " + rectTransform.pivot + ", anchoredPosition : " + rectTransform.anchoredPosition);

            // Drag Handle
            if (handle != null)
            {
                if (handleToggleStateOnPressed)
                {
                    handle.GetComponent<Button>().onClick.AddListener(OnToggleState);
                }

                foreach (Text text in handle.GetComponentsInChildren<Text>())
                {
                    if (text.gameObject != handle)
                        text.raycastTarget = false;
                }
            }

            // Overlay
            if (useOverlay == true)
            {
                overlay = new GameObject(gameObject.name + " (Overlay)");
                overlay.transform.parent = transform.parent;
                overlay.transform.localScale = Vector3.one;
                overlay.transform.SetSiblingIndex(transform.GetSiblingIndex());
                overlay.layer = gameObject.layer;

                if (useBlur == true)
                {
                    blur = new GameObject(gameObject.name + " (Blur)");
                    blur.transform.parent = transform.parent;
                    blur.transform.SetSiblingIndex(transform.GetSiblingIndex());

                    RectTransform blurRectTransform = blur.AddComponent<RectTransform>();
                    blurRectTransform.anchorMin = Vector2.zero;
                    blurRectTransform.anchorMax = Vector2.one;
                    blurRectTransform.offsetMin = Vector2.zero;
                    blurRectTransform.offsetMax = Vector2.zero;
                    blurImage = blur.AddComponent<Image>();
                    blurImage.raycastTarget = false;
                    blurImage.material = new Material(blurMaterial);
                    blurImage.material.SetInt("_Radius", 0);
                }

                RectTransform overlayRectTransform = overlay.AddComponent<RectTransform>();
                overlayRectTransform.anchorMin = Vector2.zero;
                overlayRectTransform.anchorMax = Vector2.one;
                overlayRectTransform.offsetMin = Vector2.zero;
                overlayRectTransform.offsetMax = Vector2.zero;
                overlayImage = overlay.AddComponent<Image>();
                overlayImage.color = (defaultState == State.Open) ? overlayColour : Color.clear;
                // overlayImage.raycastTarget = overlayCloseOnPressed;
                overlayImage.raycastTarget = true;
                Button overlayButton = overlay.AddComponent<Button>();
                overlayButton.transition = Selectable.Transition.None;
                overlayButton.onClick.AddListener(delegate { OnClickOverlay(); });
                overlayButton.enabled = overlayCloseOnPressed; // 
            }
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (isDragging && RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, eventData.position, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out Vector2 mouseLocalPosition))
            {
                Vector2 displacement = ((TargetState == State.Closed) ? closedPosition : openPosition) + (mouseLocalPosition - startPosition);
                float x = (placementEx == _Placement.Left || placementEx == _Placement.Right || placementEx == _Placement.RightBottom) ? displacement.x : rectTransform.anchoredPosition.x;
                float y = (placementEx == _Placement.Top || placementEx == _Placement.Bottom /*|| placementEx == PlacementEx.RightBottom*/) ? displacement.y : rectTransform.anchoredPosition.y;
                Vector2 min = new Vector2(Math.Min(closedPosition.x, openPosition.x), Math.Min(closedPosition.y, openPosition.y));
                Vector2 max = new Vector2(Math.Max(closedPosition.x, openPosition.x), Math.Max(closedPosition.y, openPosition.y));
                rectTransform.anchoredPosition = new Vector2(Mathf.Clamp(x, min.x, max.x), Mathf.Clamp(y, min.y, max.y));

                onStateSelecting.Invoke(CurrentState);
            }

            dragVelocity = (rectTransform.position - previousPosition) / (Time.time - previousTime);
            previousPosition = rectTransform.position;
            previousTime = Time.time;
        }
        public override void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
            releaseVelocity = dragVelocity;

            if (releaseVelocity.magnitude > thresholdDragSpeed)
            {
                switch (placementEx)
                {
                    case _Placement.Left:
                        if (releaseVelocity.x > 0)
                        {
                            Open();
                        }
                        else
                        {
                            Close();
                        }
                        break;
                    case _Placement.Right:
                        if (releaseVelocity.x < 0)
                        {
                            Open();
                        }
                        else
                        {
                            Close();
                        }
                        break;
                    case _Placement.RightBottom:
                        if (releaseVelocity.x < 0)
                        {
                            Open();
                        }
                        else
                        {
                            Close();
                        }
                        break;
                    case _Placement.Top:
                        if (releaseVelocity.y < 0)
                        {
                            Open();
                        }
                        else
                        {
                            Close();
                        }
                        break;
                    case _Placement.Bottom:
                        if (releaseVelocity.y > 0)
                        {
                            Open();
                        }
                        else
                        {
                            Close();
                        }
                        break;
                }
            }
            else
            {
                float nextStateProgress = (TargetState == State.Open) ? 1 - StateProgress : StateProgress;
                if (nextStateProgress > thresholdDraggedFraction)
                {
                    ToggleState();
                }
                else
                {
                    SetState(CurrentState);
                }
            }
        }

        public virtual void OnToggleState()
        {
            // base.TargetState();

            SetState((State)(((int)TargetState + 1) % 2));
        }

        public override void SetState(State state)
        {
            TargetState = state;
        }

        protected override void HandleState()
        {
            // base.HandleState();

            if (isDragging == false)
            {
                Vector2 targetPosition = openPosition;
                if (TargetState == State.Closed)
                {
                    targetPosition = closedPosition;
                }

                rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosition, Time.unscaledDeltaTime * transitionSpeed);

                if (CurrentState != TargetState)
                {
                    if ((rectTransform.anchoredPosition - targetPosition).magnitude <= rectTransform.rect.width / 10f)
                    {
                        CurrentState = TargetState;
                        onStateChanged.Invoke(CurrentState, TargetState);
                    }
                    else
                    {
                        onStateChanging.Invoke(CurrentState, TargetState);
                    }
                }
            }
        }

        protected override void HandleOverlay()
        {
            // base.HandleOverlay();

            if (useOverlay == true)
            {
                // overlayImage.raycastTarget = overlayCloseOnPressed && (TargetState == State.Open);
                overlayImage.raycastTarget = (TargetState == State.Open);
                overlayImage.color = new Color(overlayColour.r, overlayColour.g, overlayColour.b, overlayColour.a * StateProgress);

                if (useBlur == true)
                {
                    blurImage.material.SetInt("_Radius", (int)(blurRadius * StateProgress));
                }
            }
        }

        public override void Open()
        {
            Debug.LogFormat(LOG_FORMAT, "Open()");
            SetState(State.Open);
        }
        public override void Close()
        {
            Debug.LogFormat(LOG_FORMAT, "Close()");
            SetState(State.Closed);
        }

        public virtual void OnClickOverlay()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickOverlay()");
            SetState(State.Closed);
        }
    }
}