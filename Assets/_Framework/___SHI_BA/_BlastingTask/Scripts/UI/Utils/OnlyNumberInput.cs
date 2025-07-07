using UnityEngine;
using TMPro;

public class OnlyNumberInput : MonoBehaviour
{
    [Header("Input Field Settings")]
    public TMP_InputField inputField;
    
    [Header("Options")]
    [Tooltip("소수점 허용 여부")]
    public bool allowDecimal = true;
    
    [Tooltip("음수 허용 여부")]
    public bool allowNegative = true;
    
    private void Start()
    {
        // InputField가 할당되지 않았다면 현재 오브젝트에서 찾기
        if (inputField == null)
        {
            inputField = GetComponent<TMP_InputField>();
        }
        
        if (inputField != null)
        {
            // InputField의 텍스트가 변경될 때마다 호출되는 이벤트 등록
            inputField.onValueChanged.AddListener(ValidateInput);
            
            // 문자 입력 시 실시간으로 검증하는 함수 등록
            inputField.onValidateInput += ValidateChar;
        }
        else
        {
            Debug.LogError("TMP_InputField가 할당되지 않았습니다!");
        }
    }
    
    // 개별 문자 입력 시 검증 (실시간)
    private char ValidateChar(string text, int charIndex, char addedChar)
    {
        // 숫자인지 확인
        if (char.IsDigit(addedChar))
        {
            return addedChar;
        }
        
        // 소수점 허용하고 입력된 문자가 '.'인 경우
        if (allowDecimal && addedChar == '.')
        {
            // 이미 소수점이 있는지 확인
            if (!text.Contains("."))
            {
                return addedChar;
            }
        }
        
        // 음수 허용하고 입력된 문자가 '-'이며 맨 앞에 위치하는 경우
        if (allowNegative && addedChar == '-' && charIndex == 0)
        {
            // 이미 '-'가 있는지 확인
            if (!text.Contains("-"))
            {
                return addedChar;
            }
        }
        
        // 허용되지 않는 문자는 빈 문자로 반환 (입력 차단)
        return '\0';
    }
    
    // 전체 텍스트 검증 (추가 보안)
    private void ValidateInput(string input)
    {
        string validatedText = "";
        bool hasDecimal = false;
        bool hasNegative = false;
        
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            
            if (char.IsDigit(c))
            {
                validatedText += c;
            }
            else if (allowDecimal && c == '.' && !hasDecimal)
            {
                validatedText += c;
                hasDecimal = true;
            }
            else if (allowNegative && c == '-' && i == 0 && !hasNegative)
            {
                validatedText += c;
                hasNegative = true;
            }
        }
        
        // 검증된 텍스트가 원본과 다르면 업데이트
        if (input != validatedText)
        {
            inputField.text = validatedText;
            // 커서 위치를 맨 끝으로 이동
            inputField.caretPosition = validatedText.Length;
        }
    }
    
    // 현재 입력된 값을 float로 반환하는 유틸리티 함수
    public float GetFloatValue()
    {
        if (float.TryParse(inputField.text, out float result))
        {
            return result;
        }
        return 0f;
    }
    
    // 현재 입력된 값을 int로 반환하는 유틸리티 함수
    public int GetIntValue()
    {
        if (int.TryParse(inputField.text, out int result))
        {
            return result;
        }
        return 0;
    }
    
    // 값 설정 함수
    public void SetValue(float value)
    {
        inputField.text = value.ToString();
    }
    
    public void SetValue(int value)
    {
        inputField.text = value.ToString();
    }
}