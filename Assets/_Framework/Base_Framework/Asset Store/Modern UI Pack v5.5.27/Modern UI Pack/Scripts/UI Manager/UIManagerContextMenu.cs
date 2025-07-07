using UnityEngine;
using UnityEngine.UI;

namespace Michsky.MUIP
{
    [ExecuteInEditMode]
    public class UIManagerContextMenu : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] protected UIManager UIManagerAsset;

        [Header("Resources")]
        [SerializeField] private Image backgroundImage;

        protected virtual void Awake()
        {
            if (UIManagerAsset == null) { UIManagerAsset = Resources.Load<UIManager>("MUIP Manager"); }

            this.enabled = true;

            if (UIManagerAsset.enableDynamicUpdate == false)
            {
                UpdateContextMenu();
                this.enabled = false;
            }
        }

        protected virtual void Update()
        {
            if (UIManagerAsset == null) { return; }
            if (UIManagerAsset.enableDynamicUpdate == true) { UpdateContextMenu(); }
        }

        protected void UpdateContextMenu()
        {
            if (backgroundImage != null) { backgroundImage.color = UIManagerAsset.contextBackgroundColor; }
        }
    }
}