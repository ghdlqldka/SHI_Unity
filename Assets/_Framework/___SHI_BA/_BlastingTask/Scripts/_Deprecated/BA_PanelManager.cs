using UnityEngine;
using UnityEngine.UI;

public class BA_PanelManager : MonoBehaviour
{
    [Header("�г� ����")]
    [SerializeField] private GameObject panel01;
    [SerializeField] private GameObject panel02;
    [SerializeField] private GameObject panel03;
    [SerializeField] private GameObject panel04;
    [SerializeField] private GameObject panel05; // ���� �߰�

    [Header("��ư ����")]
    [SerializeField] private Button button01;
    [SerializeField] private Button button02;
    [SerializeField] private Button button03;
    [SerializeField] private Button button04;
    [SerializeField] private Button button05; // ���� �߰�

    [Header("����")]
    [SerializeField] private int defaultPanelIndex = 1; // �⺻���� ������ �г� (1~5)

    private GameObject[] panels;
    private Button[] buttons;

    void Start()
    {
        InitializePanels();
        SetupButtons();

        // �⺻ �г� ǥ��
        ShowPanel(defaultPanelIndex);
    }

    void InitializePanels()
    {
        // �г� �迭 �ʱ�ȭ (5���� Ȯ��)
        panels = new GameObject[5];
        panels[0] = panel01;
        panels[1] = panel02;
        panels[2] = panel03;
        panels[3] = panel04;
        panels[4] = panel05; // ���� �߰�

        // ��ư �迭 �ʱ�ȭ (5���� Ȯ��)
        buttons = new Button[5];
        buttons[0] = button01;
        buttons[1] = button02;
        buttons[2] = button03;
        buttons[3] = button04;
        buttons[4] = button05; // ���� �߰�

        // �ڵ� ã�� (Inspector���� �������� ���� ���)
        AutoFindPanelsAndButtons();

        // �г� ã�� ��� �α�
        for (int i = 0; i < 5; i++) // 5���� ����
        {
            if (panels[i] != null)
            {
                Debug.Log($"Panel_{i + 1:00}�� ã�ҽ��ϴ�: {panels[i].name}");
            }
            else
            {
                Debug.LogWarning($"Panel_{i + 1:00}�� ã�� ���߽��ϴ�!");
            }

            if (buttons[i] != null)
            {
                Debug.Log($"Button_{i + 1}�� ã�ҽ��ϴ�: {buttons[i].name}");
            }
            else
            {
                Debug.LogWarning($"Button_{i + 1}�� ã�� ���߽��ϴ�!");
            }
        }
    }

    void AutoFindPanelsAndButtons()
    {
        // �г� �ڵ� ã�� (5���� Ȯ��)
        for (int i = 0; i < 5; i++)
        {
            if (panels[i] == null)
            {
                string panelName = $"Panel_{i + 1:00}"; // Panel_01, Panel_02, Panel_03, Panel_04, Panel_05
                GameObject foundPanel = GameObject.Find(panelName);
                if (foundPanel != null)
                {
                    panels[i] = foundPanel;
                }
            }
        }

        // ��ư �ڵ� ã�� (���� ������ �̸����� �õ�)
        string[] buttonNamePatterns = {
            "Button_{0}", "Button{0}", "Btn_{0}", "Btn{0}",
            "PanelButton_{0}", "Panel_{0}_Button", "Panel{0}Button"
        };

        for (int i = 0; i < 5; i++) // 5���� ����
        {
            if (buttons[i] == null)
            {
                foreach (string pattern in buttonNamePatterns)
                {
                    string buttonName = string.Format(pattern, i + 1);
                    GameObject foundButton = GameObject.Find(buttonName);
                    if (foundButton != null)
                    {
                        Button buttonComponent = foundButton.GetComponent<Button>();
                        if (buttonComponent != null)
                        {
                            buttons[i] = buttonComponent;
                            break;
                        }
                    }
                }
            }
        }
    }

    void SetupButtons()
    {
        // ��ư �̺�Ʈ ��� (5���� Ȯ��)
        for (int i = 0; i < 5; i++)
        {
            if (buttons[i] != null)
            {
                int panelIndex = i + 1; // 1���� ����
                buttons[i].onClick.AddListener(() => ShowPanel(panelIndex));
            }
        }
    }

    // Ư�� �гθ� ���̰� �ϱ� (1~5 �ε��� ���)
    public void ShowPanel(int panelIndex)
    {
        if (panelIndex < 1 || panelIndex > 5) // ������ 5�� ����
        {
            Debug.LogError($"�߸��� �г� �ε���: {panelIndex} (1~5 ���̿��� ��)");
            return;
        }

        Debug.Log($"Panel_{panelIndex:00} ǥ�� ��...");

        // ��� �г� ����� (5���� Ȯ��)
        for (int i = 0; i < 5; i++)
        {
            if (panels[i] != null)
            {
                panels[i].SetActive(false);
            }
        }

        // ���õ� �гθ� ���̱�
        int arrayIndex = panelIndex - 1; // �迭 �ε����� ��ȯ
        if (panels[arrayIndex] != null)
        {
            panels[arrayIndex].SetActive(true);
            Debug.Log($"Panel_{panelIndex:00} Ȱ��ȭ��");
        }
        else
        {
            Debug.LogError($"Panel_{panelIndex:00}�� �Ҵ���� �ʾҽ��ϴ�!");
        }
    }

    // UI ��ư�� �޼���� (5�� �߰�)
    public void ShowPanel01() { ShowPanel(1); }
    public void ShowPanel02() { ShowPanel(2); }
    public void ShowPanel03() { ShowPanel(3); }
    public void ShowPanel04() { ShowPanel(4); }
    public void ShowPanel05() { ShowPanel(5); } // ���� �߰�

    // ���� Ȱ��ȭ�� �г� �ε��� ��ȯ (1~5, ������ 0)
    public int GetActivePanelIndex()
    {
        for (int i = 0; i < 5; i++) // 5���� ����
        {
            if (panels[i] != null && panels[i].activeInHierarchy)
            {
                return i + 1;
            }
        }
        return 0; // Ȱ��ȭ�� �г��� ����
    }

    // �г�/��ư ���� ���� �޼���� (5������ ����)
    public void SetPanel(int index, GameObject panel)
    {
        if (index >= 1 && index <= 5) // ������ 5�� ����
        {
            panels[index - 1] = panel;
            Debug.Log($"Panel_{index:00} ���� ����: {(panel != null ? panel.name : "null")}");
        }
    }

    public void SetButton(int index, Button button)
    {
        if (index >= 1 && index <= 5) // ������ 5�� ����
        {
            buttons[index - 1] = button;
            Debug.Log($"Button_{index} ���� ����: {(button != null ? button.name : "null")}");
        }
    }

    // ������ - ���� �г� ���� ���
    [ContextMenu("Debug Panel States")]
    public void DebugPanelStates()
    {
        Debug.Log("=== �г� ���� ===");
        for (int i = 0; i < 5; i++) // 5���� ����
        {
            if (panels[i] != null)
            {
                string status = panels[i].activeInHierarchy ? "Ȱ��" : "��Ȱ��";
                Debug.Log($"Panel_{i + 1:00}: {status} ({panels[i].name})");
            }
            else
            {
                Debug.Log($"Panel_{i + 1:00}: �Ҵ���� ����");
            }
        }
        Debug.Log($"���� Ȱ�� �г�: Panel_{GetActivePanelIndex():00}");
    }

    void OnDestroy()
    {
        // ��ư �̺�Ʈ ���� (5���� Ȯ��)
        for (int i = 0; i < 5; i++)
        {
            if (buttons[i] != null)
            {
                buttons[i].onClick.RemoveAllListeners();
            }
        }
    }
}