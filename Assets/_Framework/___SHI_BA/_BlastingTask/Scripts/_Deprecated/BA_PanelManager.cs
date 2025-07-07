using UnityEngine;
using UnityEngine.UI;

public class BA_PanelManager : MonoBehaviour
{
    [Header("패널 참조")]
    [SerializeField] private GameObject panel01;
    [SerializeField] private GameObject panel02;
    [SerializeField] private GameObject panel03;
    [SerializeField] private GameObject panel04;
    [SerializeField] private GameObject panel05; // 새로 추가

    [Header("버튼 참조")]
    [SerializeField] private Button button01;
    [SerializeField] private Button button02;
    [SerializeField] private Button button03;
    [SerializeField] private Button button04;
    [SerializeField] private Button button05; // 새로 추가

    [Header("설정")]
    [SerializeField] private int defaultPanelIndex = 1; // 기본으로 보여줄 패널 (1~5)

    private GameObject[] panels;
    private Button[] buttons;

    void Start()
    {
        InitializePanels();
        SetupButtons();

        // 기본 패널 표시
        ShowPanel(defaultPanelIndex);
    }

    void InitializePanels()
    {
        // 패널 배열 초기화 (5개로 확장)
        panels = new GameObject[5];
        panels[0] = panel01;
        panels[1] = panel02;
        panels[2] = panel03;
        panels[3] = panel04;
        panels[4] = panel05; // 새로 추가

        // 버튼 배열 초기화 (5개로 확장)
        buttons = new Button[5];
        buttons[0] = button01;
        buttons[1] = button02;
        buttons[2] = button03;
        buttons[3] = button04;
        buttons[4] = button05; // 새로 추가

        // 자동 찾기 (Inspector에서 연결하지 않은 경우)
        AutoFindPanelsAndButtons();

        // 패널 찾기 결과 로그
        for (int i = 0; i < 5; i++) // 5개로 변경
        {
            if (panels[i] != null)
            {
                Debug.Log($"Panel_{i + 1:00}을 찾았습니다: {panels[i].name}");
            }
            else
            {
                Debug.LogWarning($"Panel_{i + 1:00}을 찾지 못했습니다!");
            }

            if (buttons[i] != null)
            {
                Debug.Log($"Button_{i + 1}을 찾았습니다: {buttons[i].name}");
            }
            else
            {
                Debug.LogWarning($"Button_{i + 1}을 찾지 못했습니다!");
            }
        }
    }

    void AutoFindPanelsAndButtons()
    {
        // 패널 자동 찾기 (5개로 확장)
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

        // 버튼 자동 찾기 (여러 가능한 이름으로 시도)
        string[] buttonNamePatterns = {
            "Button_{0}", "Button{0}", "Btn_{0}", "Btn{0}",
            "PanelButton_{0}", "Panel_{0}_Button", "Panel{0}Button"
        };

        for (int i = 0; i < 5; i++) // 5개로 변경
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
        // 버튼 이벤트 등록 (5개로 확장)
        for (int i = 0; i < 5; i++)
        {
            if (buttons[i] != null)
            {
                int panelIndex = i + 1; // 1부터 시작
                buttons[i].onClick.AddListener(() => ShowPanel(panelIndex));
            }
        }
    }

    // 특정 패널만 보이게 하기 (1~5 인덱스 사용)
    public void ShowPanel(int panelIndex)
    {
        if (panelIndex < 1 || panelIndex > 5) // 범위를 5로 변경
        {
            Debug.LogError($"잘못된 패널 인덱스: {panelIndex} (1~5 사이여야 함)");
            return;
        }

        Debug.Log($"Panel_{panelIndex:00} 표시 중...");

        // 모든 패널 숨기기 (5개로 확장)
        for (int i = 0; i < 5; i++)
        {
            if (panels[i] != null)
            {
                panels[i].SetActive(false);
            }
        }

        // 선택된 패널만 보이기
        int arrayIndex = panelIndex - 1; // 배열 인덱스로 변환
        if (panels[arrayIndex] != null)
        {
            panels[arrayIndex].SetActive(true);
            Debug.Log($"Panel_{panelIndex:00} 활성화됨");
        }
        else
        {
            Debug.LogError($"Panel_{panelIndex:00}이 할당되지 않았습니다!");
        }
    }

    // UI 버튼용 메서드들 (5번 추가)
    public void ShowPanel01() { ShowPanel(1); }
    public void ShowPanel02() { ShowPanel(2); }
    public void ShowPanel03() { ShowPanel(3); }
    public void ShowPanel04() { ShowPanel(4); }
    public void ShowPanel05() { ShowPanel(5); } // 새로 추가

    // 현재 활성화된 패널 인덱스 반환 (1~5, 없으면 0)
    public int GetActivePanelIndex()
    {
        for (int i = 0; i < 5; i++) // 5개로 변경
        {
            if (panels[i] != null && panels[i].activeInHierarchy)
            {
                return i + 1;
            }
        }
        return 0; // 활성화된 패널이 없음
    }

    // 패널/버튼 수동 설정 메서드들 (5번까지 지원)
    public void SetPanel(int index, GameObject panel)
    {
        if (index >= 1 && index <= 5) // 범위를 5로 변경
        {
            panels[index - 1] = panel;
            Debug.Log($"Panel_{index:00} 수동 설정: {(panel != null ? panel.name : "null")}");
        }
    }

    public void SetButton(int index, Button button)
    {
        if (index >= 1 && index <= 5) // 범위를 5로 변경
        {
            buttons[index - 1] = button;
            Debug.Log($"Button_{index} 수동 설정: {(button != null ? button.name : "null")}");
        }
    }

    // 디버깅용 - 현재 패널 상태 출력
    [ContextMenu("Debug Panel States")]
    public void DebugPanelStates()
    {
        Debug.Log("=== 패널 상태 ===");
        for (int i = 0; i < 5; i++) // 5개로 변경
        {
            if (panels[i] != null)
            {
                string status = panels[i].activeInHierarchy ? "활성" : "비활성";
                Debug.Log($"Panel_{i + 1:00}: {status} ({panels[i].name})");
            }
            else
            {
                Debug.Log($"Panel_{i + 1:00}: 할당되지 않음");
            }
        }
        Debug.Log($"현재 활성 패널: Panel_{GetActivePanelIndex():00}");
    }

    void OnDestroy()
    {
        // 버튼 이벤트 정리 (5개로 확장)
        for (int i = 0; i < 5; i++)
        {
            if (buttons[i] != null)
            {
                buttons[i].onClick.RemoveAllListeners();
            }
        }
    }
}