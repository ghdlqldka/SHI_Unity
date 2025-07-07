using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _Magenta_Framework
{
    [RequireComponent(typeof(ToggleGroup))]
    public class MUI_RadioGroupEx : _Base_Framework._MUI_RadioGroup
    {
        private static string LOG_FORMAT = "<color=#B1C5EF><b>[MUI_RadioGroupEx]</b></color> {0}";

        protected override void Awake()
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
    }
}