using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
using System.Text.RegularExpressions;
#endif

// A UI element to show information about a debug entry
namespace IngameDebugConsole
{
	public class _UI_DebugLogItem : DebugLogItem
	{
        public override void Initialize(DebugLogRecycledListView listView)
        {
            this.listView = listView;

            logTextOriginalPosition = logText.rectTransform.anchoredPosition;
            logTextOriginalSize = logText.rectTransform.sizeDelta;
            copyLogButtonHeight = (copyLogButton.transform as RectTransform).anchoredPosition.y + (copyLogButton.transform as RectTransform).sizeDelta.y + 2f; // 2f: space between text and button

            if (listView.manager.logItemFontOverride != null)
                logText.font = listView.manager.logItemFontOverride;

            copyLogButton.onClick.AddListener(CopyLog);
#if !UNITY_EDITOR && UNITY_WEBGL
			copyLogButton.gameObject.AddComponent<DebugLogItemCopyWebGL>().Initialize( this );
#endif
        }
    }
}