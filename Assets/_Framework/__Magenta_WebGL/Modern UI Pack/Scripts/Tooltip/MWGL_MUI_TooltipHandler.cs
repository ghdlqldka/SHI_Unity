using System.Collections;
using TMPro;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace _Magenta_WebGL
{
    public class MWGL_MUI_TooltipHandler : _Magenta_Framework.MUI_TooltipHandlerEx
    {
        // private static string LOG_FORMAT = "<color=#EBC6FB><b>[MWGL_MUI_TooltipHandler]</b></color> {0}";

#if false //
        public static new MUI_TooltipHandlerEx Instance
        {
            get
            {
                return _instance as MUI_TooltipHandlerEx;
            }
            protected set
            {
                _instance = value;
            }
        }

        protected override void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                Debug.Assert(_camera != null);

                Debug.Assert(descriptionText != null);
                Debug.Assert(tooltipAnimator != null);
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this);
                return;
            }
        }

        protected override void Update()
        {
            if (allowUpdating == true)
            {
#if ENABLE_LEGACY_INPUT_MANAGER
                cursorPos = Input.mousePosition;
#elif ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
                cursorPos = Input.mousePosition;
#elif ENABLE_INPUT_SYSTEM
                cursorPos = Mouse.current.position.ReadValue();
#endif
                cursorPos.z = tooltipZHelper.position.z;
                uiPos = tooltipRect.anchoredPosition;
                CheckForBounds();

                if (mainCanvas.renderMode == RenderMode.ScreenSpaceCamera || mainCanvas.renderMode == RenderMode.WorldSpace)
                {
                    // tooltipRect.position = Camera.main.ScreenToWorldPoint(cursorPos);
                    tooltipRect.position = _camera.ScreenToWorldPoint(cursorPos);
                    tooltipContent.transform.localPosition = Vector3.SmoothDamp(tooltipContent.transform.localPosition, contentPos, ref tooltipVelocity, tooltipSmoothness);
                }

                else if (mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    tooltipRect.position = cursorPos;
                    tooltipContent.transform.position = Vector3.SmoothDamp(tooltipContent.transform.position, cursorPos + contentPos, ref tooltipVelocity, tooltipSmoothness);
                }
            }
        }

        // public void CheckForBounds()
        protected override void CheckForBounds()
        {
            if (uiPos.x <= -400)
            {
                contentPos = new Vector3(hBorderLeft, contentPos.y, 0);
                tooltipContent.GetComponent<RectTransform>().pivot = new Vector2(0f, tooltipContent.GetComponent<RectTransform>().pivot.y);
            }

            if (uiPos.x >= 400)
            {
                contentPos = new Vector3(hBorderRight, contentPos.y, 0);
                tooltipContent.GetComponent<RectTransform>().pivot = new Vector2(1f, tooltipContent.GetComponent<RectTransform>().pivot.y);
            }

            if (uiPos.y <= -325)
            {
                contentPos = new Vector3(contentPos.x, vBorderBottom, 0);
                tooltipContent.GetComponent<RectTransform>().pivot = new Vector2(tooltipContent.GetComponent<RectTransform>().pivot.x, 0f);
            }

            if (uiPos.y >= 325)
            {
                contentPos = new Vector3(contentPos.x, vBorderTop, 0);
                tooltipContent.GetComponent<RectTransform>().pivot = new Vector2(tooltipContent.GetComponent<RectTransform>().pivot.x, 1f);
            }
        }

        public override void ShowTooltip(bool show, float delay = 0.0f, string description = "")
        {
            if (show == true)
            {
                Description = description;
                allowUpdating = true;
                tooltipAnimator.gameObject.SetActive(false);
                tooltipAnimator.gameObject.SetActive(true);

                if (delay == 0)
                    tooltipAnimator.Play("In");
                else
                    StartCoroutine(PostShowTooltip(delay));
            }
            else
            {
                if (delay != 0)
                {
                    StopAllCoroutines();

                    if (tooltipAnimator.GetCurrentAnimatorStateInfo(0).IsName("In"))
                        tooltipAnimator.Play("Out");
                }
                else
                    tooltipAnimator.Play("Out");

                allowUpdating = false;
            }
        }

        protected override IEnumerator PostShowTooltip(float delay)
        {
            yield return new WaitForSeconds(delay);
            tooltipAnimator.Play("In");
        }
#endif
    }
}