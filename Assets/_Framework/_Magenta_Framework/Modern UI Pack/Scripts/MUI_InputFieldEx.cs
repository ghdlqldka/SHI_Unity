using UnityEngine;
using TMPro;

namespace _Magenta_Framework
{
    [RequireComponent(typeof(TMP_InputField))]
    [RequireComponent(typeof(Animator))]
    public class MUI_InputFieldEx : Michsky.MUIP._MUI_InputField
    {
#if false //
        protected override void OnEnable()
        {
            base.OnEnable();
        }
#endif
    }
}