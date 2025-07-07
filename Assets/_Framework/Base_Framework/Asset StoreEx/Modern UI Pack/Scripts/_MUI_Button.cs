using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
#endif

namespace Michsky.MUIP
{
    [RequireComponent(typeof(Button))]
    public class _MUI_Button : ButtonManager
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[_MUI_Button]</b></color> {0}";

#if UNITY_EDITOR
        // public int latestTabIndex = 0;
        public int _latestTabIndex
        {
            get
            {
                return latestTabIndex;
            }
            set
            {
                if (latestTabIndex != value)
                {
                    latestTabIndex = value;
                    Debug.LogWarningFormat(LOG_FORMAT, "Latest Tab Index CHANGED to : <b>" + value + "</b>");
                }
            }
        }
#endif

        protected virtual void Awake()
        {
            if (Application.isPlaying == true && enableButtonSounds == true)
            {
                if (soundSource == null)
                {
                    Canvas _canvas = this.GetComponentInParent<Canvas>();
                    Debug.Assert(_canvas != null);
                    soundSource = _canvas.GetComponent<AudioSource>();
                }
                Debug.AssertFormat(soundSource != null, "There is NO AudioSource!!!!!");
            }
        }

        protected override void OnEnable()
        {
            if (isInitialized == false) 
            { 
                Initialize();
            }

            UpdateUI();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            Debug.LogFormat(LOG_FORMAT, "<color=yellow>" + this.gameObject.name + "</color>-OnPointerClick()");
            // base.OnPointerClick(eventData);

            if (isInteractable == false || eventData.button != PointerEventData.InputButton.Left)
            { 
                return;
            }

            if (enableButtonSounds && useClickSound == true && soundSource != null)
            { 
                soundSource.PlayOneShot(clickSound);
            }

            // Invoke click actions
            onClick.Invoke();

            // Check for double click
            if (checkForDoubleClick == false || !gameObject.activeInHierarchy) 
            { 
                return;
            }

            if (waitingForDoubleClickInput == true)
            {
                onDoubleClick.Invoke();
                waitingForDoubleClickInput = false;
                return;
            }

            waitingForDoubleClickInput = true;

            StopCoroutine("CheckForDoubleClick");
            StartCoroutine("CheckForDoubleClick");
        }

        protected override void Initialize()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            { 
                return;
            }
#endif
            if (animationSolution == AnimationSolution.ScriptBased && TryGetComponent<Animator>(out var tempAnimator)) 
            { 
                Destroy(tempAnimator);
            }

            if (this.gameObject.GetComponent<Image>() == null)
            {
                Image raycastImg = this.gameObject.AddComponent<Image>();
                raycastImg.color = new Color(0, 0, 0, 0);
                raycastImg.raycastTarget = true;
            }

            if (targetCanvas == null) 
            { 
                targetCanvas = GetComponentInParent<Canvas>();
            }
            if (normalCG == null)
            { 
                normalCG = new GameObject().AddComponent<CanvasGroup>();
                normalCG.gameObject.AddComponent<RectTransform>(); 
                normalCG.transform.SetParent(transform);
                normalCG.gameObject.name = "Normal";
            }
            if (highlightCG == null)
            { 
                highlightCG = new GameObject().AddComponent<CanvasGroup>(); 
                highlightCG.gameObject.AddComponent<RectTransform>(); 
                highlightCG.transform.SetParent(transform); 
                highlightCG.gameObject.name = "Highlight"; 
            }
            if (disabledCG == null)
            { 
                disabledCG = new GameObject().AddComponent<CanvasGroup>(); 
                disabledCG.gameObject.AddComponent<RectTransform>(); 
                disabledCG.transform.SetParent(transform); 
                disabledCG.gameObject.name = "Disabled"; 
            }

            if (useRipple == true && rippleParent != null) 
            { 
                rippleParent.SetActive(false);
            }
            else if (useRipple == false && rippleParent != null) 
            { 
                Destroy(rippleParent);
            }

            if (gameObject.activeInHierarchy) 
            { 
                StartCoroutine(nameof(LayoutFix));
            }

#if true //
            targetButton = this.gameObject.GetComponent<Button>();
            if (targetButton == null)
            {
                targetButton = this.gameObject.AddComponent<Button>();

                targetButton.transition = Selectable.Transition.None;

                Navigation customNav = new Navigation();
                customNav.mode = Navigation.Mode.None;
                targetButton.navigation = customNav;
            }
#else
            if (targetButton == null)
            {
                if (gameObject.GetComponent<Button>() == null) 
                { 
                    targetButton = gameObject.AddComponent<Button>(); 
                }
                else 
                {
                    targetButton = GetComponent<Button>(); 
                }

                targetButton.transition = Selectable.Transition.None;

                Navigation customNav = new Navigation();
                customNav.mode = Navigation.Mode.None;
                targetButton.navigation = customNav;
            }
#endif

            if (useUINavigation)
            { 
                AddUINavigation();
            }

            isInitialized = true;
        }
    }
}