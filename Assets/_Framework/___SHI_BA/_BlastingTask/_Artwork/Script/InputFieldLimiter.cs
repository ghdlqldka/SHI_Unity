using UnityEngine;
using TMPro;

public class InputFieldLimiter : MonoBehaviour
{
    [Header("설정")]
    public float maxValue = 200f; // 최대값 설정
    public float minValue = 0f;   // 최소값 설정 (옵션)
    public bool allowNegative = false; // 음수 허용 여부
    public bool allowDecimal = true;   // 소수점 허용 여부

    [Header("자동 연결")]
    public TMP_InputField inputField; // 인풋필드 참조

    void Start()
    {
        // 인풋필드가 설정되지 않았다면 같은 GameObject에서 찾기
        if (inputField == null)
        {
            inputField = GetComponent<TMP_InputField>();
        }

        if (inputField != null)
        {
            // 숫자만 입력 가능하도록 설정
            if (allowDecimal)
            {
                inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
            }
            else
            {
                inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            }

            // 이벤트 연결
            inputField.onValueChanged.AddListener(OnValueChanged);
            inputField.onEndEdit.AddListener(OnEndEdit);
        }
        else
        {
            Debug.LogError("[InputFieldLimiter] TMP_InputField를 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// 값이 변경될 때마다 호출 (실시간 검증)
    /// </summary>
    private void OnValueChanged(string value)
    {
        // 빈 문자열이면 무시
        if (string.IsNullOrEmpty(value)) return;

        // 숫자로 파싱 시도
        if (float.TryParse(value, out float numValue))
        {
            // 음수 체크
            if (!allowNegative && numValue < 0)
            {
                inputField.text = "0";
                return;
            }

            // 최대값 체크 (실시간으로 제한하고 싶다면 주석 해제)
            // if (numValue > maxValue)
            // {
            //     inputField.text = maxValue.ToString();
            // }
        }
    }

    /// <summary>
    /// 입력이 끝났을 때 호출 (최종 검증)
    /// </summary>
    private void OnEndEdit(string value)
    {
        // 빈 문자열이면 최소값으로 설정
        if (string.IsNullOrEmpty(value))
        {
            inputField.text = minValue.ToString();
            return;
        }

        // 숫자로 파싱 시도
        if (float.TryParse(value, out float numValue))
        {
            // 범위 체크 및 제한
            float clampedValue = Mathf.Clamp(numValue, minValue, maxValue);

            // 값이 변경되었다면 업데이트
            if (clampedValue != numValue)
            {
                if (allowDecimal)
                {
                    inputField.text = clampedValue.ToString("F2"); // 소수점 2자리
                }
                else
                {
                    inputField.text = ((int)clampedValue).ToString();
                }

                Debug.Log($"[InputFieldLimiter] 값 제한: {numValue} → {clampedValue}");
            }
        }
        else
        {
            // 숫자가 아니라면 최소값으로 설정
            inputField.text = minValue.ToString();
            Debug.LogWarning("[InputFieldLimiter] 잘못된 숫자 형식입니다. 최소값으로 설정합니다.");
        }
    }

    /// <summary>
    /// 현재 값을 float으로 가져오기
    /// </summary>
    public float GetValue()
    {
        if (inputField != null && float.TryParse(inputField.text, out float value))
        {
            return value;
        }
        return minValue;
    }

    /// <summary>
    /// 값을 프로그래밍적으로 설정
    /// </summary>
    public void SetValue(float value)
    {
        if (inputField != null)
        {
            float clampedValue = Mathf.Clamp(value, minValue, maxValue);

            if (allowDecimal)
            {
                inputField.text = clampedValue.ToString("F2");
            }
            else
            {
                inputField.text = ((int)clampedValue).ToString();
            }
        }
    }

    /// <summary>
    /// 최대값을 런타임에 변경
    /// </summary>
    public void SetMaxValue(float newMaxValue)
    {
        maxValue = newMaxValue;

        // 현재 값이 새로운 최대값을 초과하면 조정
        float currentValue = GetValue();
        if (currentValue > maxValue)
        {
            SetValue(maxValue);
        }

        Debug.Log($"[InputFieldLimiter] 최대값 변경: {maxValue}");
    }

    /// <summary>
    /// 최소값을 런타임에 변경
    /// </summary>
    public void SetMinValue(float newMinValue)
    {
        minValue = newMinValue;

        // 현재 값이 새로운 최소값보다 작으면 조정
        float currentValue = GetValue();
        if (currentValue < minValue)
        {
            SetValue(minValue);
        }

        Debug.Log($"[InputFieldLimiter] 최소값 변경: {minValue}");
    }

    /// <summary>
    /// 범위를 런타임에 변경
    /// </summary>
    public void SetRange(float newMinValue, float newMaxValue)
    {
        minValue = newMinValue;
        maxValue = newMaxValue;

        // 현재 값이 새로운 범위를 벗어나면 조정
        float currentValue = GetValue();
        if (currentValue < minValue || currentValue > maxValue)
        {
            SetValue(Mathf.Clamp(currentValue, minValue, maxValue));
        }

        Debug.Log($"[InputFieldLimiter] 범위 변경: {minValue} ~ {maxValue}");
    }

    void OnDestroy()
    {
        // 이벤트 해제
        if (inputField != null)
        {
            inputField.onValueChanged.RemoveListener(OnValueChanged);
            inputField.onEndEdit.RemoveListener(OnEndEdit);
        }
    }
}