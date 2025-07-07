using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using static UnityEngine.UI.Toggle;

namespace Michsky.MUIP
{
    [RequireComponent(typeof(Toggle))]
    [RequireComponent(typeof(Animator))]
    public class _MUI_Toggle : CustomToggle
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[_MUI_Toggle]</b></color> {0}";

        [Header("Events")]
        public ToggleEvent onValueChanged = new ToggleEvent();
        // 

        // [HideInInspector] public Toggle toggleObject;
        public Toggle _toggle
        {
            get
            {
                return toggleObject;
            }
            protected set
            {
                toggleObject = value;
            }
        }

        protected override void Awake()
        {
            // base.Awake();

            if (_toggle == null)
            {
                _toggle = gameObject.GetComponent<Toggle>();
            }
            if (toggleAnimator == null)
            { 
                toggleAnimator = _toggle.GetComponent<Animator>();
            }
            if (invokeOnAwake == true)
            {
                _toggle.onValueChanged.Invoke(_toggle.isOn);
            }

            _toggle.onValueChanged.AddListener(UpdateState);
            UpdateState();
            isInitialized = true;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        public virtual void OnToggled(bool isOn)
        {
            Debug.LogFormat(LOG_FORMAT, "OnToggled(), this.gameObject : <b>" + this.gameObject.name + "</b>, isOn : <b>" + isOn + "</b>");

            if (onValueChanged != null)
            {
                onValueChanged.Invoke(isOn);
            }
        }
    }
}