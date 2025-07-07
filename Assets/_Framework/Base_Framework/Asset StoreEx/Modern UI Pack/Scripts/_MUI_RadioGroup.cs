using System.Collections.Generic;
using Michsky.MUIP;
using UnityEngine;
using UnityEngine.UI;

namespace _Base_Framework
{
    // [RequireComponent(typeof(ToggleGroup))]
    public class _MUI_RadioGroup : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[_MUI_RadioGroup]</b></color> {0}";

        public delegate void ValueChangedCallback(int index);
        public event ValueChangedCallback OnValueChanged;

        [SerializeField]
        protected ToggleGroup toggleGroup;

        [SerializeField]
        protected List<_MUI_Toggle> toggleList = new List<_MUI_Toggle>();
        [SerializeField]
        protected int startIndex = 0;
        [SerializeField]
        protected int index = -1;
        public virtual int Index
        {
            get
            {
                return index;
            }
            protected set
            {
                if (index != value)
                {
                    index = value;
                    Debug.LogWarningFormat(LOG_FORMAT, "<color=magenta>Index : <b>" + value + "</b></color>");
                    Invoke_OnValueChanged(value);
                }
            }
        }

        protected void Invoke_OnValueChanged(int index)
        {
            if (OnValueChanged != null)
            {
                OnValueChanged(index);
            }
        }

        protected virtual void Awake()
        {
#if DEBUG
            Debug.LogWarningFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");

            Debug.Assert(toggleGroup != null);
#endif

            // m_Toggles.AddRange(this.transform.GetComponentsInChildren<Toggle>());
            Debug.Assert(toggleList != null);
            for (int i = 0; i < toggleList.Count; i++)
            {
                toggleList[i].onValueChanged.AddListener(OnToggleValueChanged);
                Debug.AssertFormat(toggleList[i]._toggle.isOn == false, "i(" + i + ") is NOT Off"); // All toggles are Off !!!!
                Debug.AssertFormat(toggleList[i]._toggle.group == toggleGroup, "");
            }

            Debug.Assert(toggleGroup.allowSwitchOff == false);
            toggleGroup.allowSwitchOff = false;
        }

        protected virtual void OnEnable()
        {
            //
        }

        protected virtual void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start(), startIndex : <color=yellow><b>" + startIndex + "</b></color>");
            toggleList[startIndex]._toggle.isOn = true;
            // m_Toggles[startIndex].SetIsOnWithoutNotify(true); // New value for isOn.
        }

        protected virtual void OnToggleValueChanged(bool isOn)
        {
            // Debug.LogFormat(LOG_FORMAT, "OnToggleValueChanged(), isOn : " + isOn);

            if (isOn == true)
            {
                Toggle toggle = toggleGroup.GetFirstActiveToggle();
                for (int i = 0; i < toggleList.Count; i++)
                {
                    if (toggle.GetComponent<_MUI_Toggle>() == toggleList[i])
                    {
                        Index = i;
                        break;
                    }
                }
            }
        }
    }
}