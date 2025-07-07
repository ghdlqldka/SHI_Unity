using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.MUIP
{
    [ExecuteInEditMode]
    public class UIManagerTooltip : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] protected UIManager UIManagerAsset;

        [Header("Resources")]
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI text;

        protected virtual void Awake()
        {
            if (UIManagerAsset == null) { UIManagerAsset = Resources.Load<UIManager>("MUIP Manager"); }

            this.enabled = true;

            if (UIManagerAsset.enableDynamicUpdate == false)
            {
                UpdateTooltip();
                this.enabled = false;
            }
        }

        void Update()
        {
            if (UIManagerAsset == null) { return; }
            if (UIManagerAsset.enableDynamicUpdate == true) { UpdateTooltip(); }
        }

        protected virtual void UpdateTooltip()
        {
            background.color = UIManagerAsset.tooltipBackgroundColor;
            text.color = UIManagerAsset.tooltipTextColor;
            text.font = UIManagerAsset.tooltipFont;
            text.fontSize = UIManagerAsset.tooltipFontSize;
        }
    }
}