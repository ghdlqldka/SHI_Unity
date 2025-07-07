using UnityEngine;
using TMPro;

namespace Michsky.MUIP
{
    // [CreateAssetMenu(fileName = "New UI Manager", menuName = "Modern UI Pack/New UI Manager")]
    public class _MUI_Manager : UIManager
    {
        // private static string LOG_FORMAT = "<color=#EBC6FB><b>[_MUI_Manager]</b></color> {0}";

#if DEBUG
        public static bool ShowDebugLog = false;
#endif
    }
}