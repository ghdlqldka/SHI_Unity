using UnityEngine;
using UnityEngine.UI;

namespace Michsky.MUIP
{
    [ExecuteInEditMode]
    public class UIManagerScrollbar : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] protected UIManager UIManagerAsset;

        [Header("Resources")]
        [SerializeField] private Image background;
        [SerializeField] private Image bar;

        protected virtual void Awake()
        {
            if (UIManagerAsset == null) { UIManagerAsset = Resources.Load<UIManager>("MUIP Manager"); }

            this.enabled = true;

            if (UIManagerAsset.enableDynamicUpdate == false)
            {
                UpdateScrollbar();
                this.enabled = false;
            }
        }

        protected virtual void Update()
        {
            if (UIManagerAsset == null) { return; }
            if (UIManagerAsset.enableDynamicUpdate == true) { UpdateScrollbar(); }
        }

        protected void UpdateScrollbar()
        {
            background.color = UIManagerAsset.scrollbarBackgroundColor;
            bar.color = UIManagerAsset.scrollbarColor;
        }
    }
}