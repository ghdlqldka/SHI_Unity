using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Unity.Cinemachine.Samples
{
    /// <summary>
    /// Displays a button in the game view that will bring up a window with scrollable text.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class _UI_SamplesHelp : SamplesHelpUI
    {
        protected override void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            m_HelpButton = uiDocument.rootVisualElement.Q("HelpButton") as Button;
            if (m_HelpButton == null)
            {
                Debug.LogError("Cannot find HelpToggleBox.  Is the source asset set in the UIDocument?");
                return;
            }
            m_HelpButton.RegisterCallback(new EventCallback<ClickEvent>(OpenHelpBox));

            m_HelpBox = uiDocument.rootVisualElement.Q("HelpTextBox");
            if (uiDocument.rootVisualElement.Q("HelpTextBox__Title") is Label helpTitle)
                helpTitle.text = string.IsNullOrEmpty(HelpTitle) ? SceneManager.GetActiveScene().name : HelpTitle;

            if (uiDocument.rootVisualElement.Q("HelpTextBox__ScrollView__Label") is Label helpLabel)
                helpLabel.text = HelpText;

            m_CloseButton = uiDocument.rootVisualElement.Q("HelpTextBox__CloseButton") as Button;
            if (m_CloseButton == null)
            {
                Debug.LogError("Cannot find HelpTextBox__CloseButton.  Is the source asset set in the UIDocument?");
                return;
            }
            m_CloseButton.RegisterCallback<ClickEvent>(CloseHelpBox);

            m_HelpBox.visible = VisibleAtStart;
            m_HelpButton.visible = !VisibleAtStart;
        }
    }
}
