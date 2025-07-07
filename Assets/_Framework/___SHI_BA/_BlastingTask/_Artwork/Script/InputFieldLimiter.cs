using UnityEngine;
using TMPro;

public class InputFieldLimiter : MonoBehaviour
{
    [Header("����")]
    public float maxValue = 200f; // �ִ밪 ����
    public float minValue = 0f;   // �ּҰ� ���� (�ɼ�)
    public bool allowNegative = false; // ���� ��� ����
    public bool allowDecimal = true;   // �Ҽ��� ��� ����

    [Header("�ڵ� ����")]
    public TMP_InputField inputField; // ��ǲ�ʵ� ����

    void Start()
    {
        // ��ǲ�ʵ尡 �������� �ʾҴٸ� ���� GameObject���� ã��
        if (inputField == null)
        {
            inputField = GetComponent<TMP_InputField>();
        }

        if (inputField != null)
        {
            // ���ڸ� �Է� �����ϵ��� ����
            if (allowDecimal)
            {
                inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
            }
            else
            {
                inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            }

            // �̺�Ʈ ����
            inputField.onValueChanged.AddListener(OnValueChanged);
            inputField.onEndEdit.AddListener(OnEndEdit);
        }
        else
        {
            Debug.LogError("[InputFieldLimiter] TMP_InputField�� ã�� �� �����ϴ�!");
        }
    }

    /// <summary>
    /// ���� ����� ������ ȣ�� (�ǽð� ����)
    /// </summary>
    private void OnValueChanged(string value)
    {
        // �� ���ڿ��̸� ����
        if (string.IsNullOrEmpty(value)) return;

        // ���ڷ� �Ľ� �õ�
        if (float.TryParse(value, out float numValue))
        {
            // ���� üũ
            if (!allowNegative && numValue < 0)
            {
                inputField.text = "0";
                return;
            }

            // �ִ밪 üũ (�ǽð����� �����ϰ� �ʹٸ� �ּ� ����)
            // if (numValue > maxValue)
            // {
            //     inputField.text = maxValue.ToString();
            // }
        }
    }

    /// <summary>
    /// �Է��� ������ �� ȣ�� (���� ����)
    /// </summary>
    private void OnEndEdit(string value)
    {
        // �� ���ڿ��̸� �ּҰ����� ����
        if (string.IsNullOrEmpty(value))
        {
            inputField.text = minValue.ToString();
            return;
        }

        // ���ڷ� �Ľ� �õ�
        if (float.TryParse(value, out float numValue))
        {
            // ���� üũ �� ����
            float clampedValue = Mathf.Clamp(numValue, minValue, maxValue);

            // ���� ����Ǿ��ٸ� ������Ʈ
            if (clampedValue != numValue)
            {
                if (allowDecimal)
                {
                    inputField.text = clampedValue.ToString("F2"); // �Ҽ��� 2�ڸ�
                }
                else
                {
                    inputField.text = ((int)clampedValue).ToString();
                }

                Debug.Log($"[InputFieldLimiter] �� ����: {numValue} �� {clampedValue}");
            }
        }
        else
        {
            // ���ڰ� �ƴ϶�� �ּҰ����� ����
            inputField.text = minValue.ToString();
            Debug.LogWarning("[InputFieldLimiter] �߸��� ���� �����Դϴ�. �ּҰ����� �����մϴ�.");
        }
    }

    /// <summary>
    /// ���� ���� float���� ��������
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
    /// ���� ���α׷��������� ����
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
    /// �ִ밪�� ��Ÿ�ӿ� ����
    /// </summary>
    public void SetMaxValue(float newMaxValue)
    {
        maxValue = newMaxValue;

        // ���� ���� ���ο� �ִ밪�� �ʰ��ϸ� ����
        float currentValue = GetValue();
        if (currentValue > maxValue)
        {
            SetValue(maxValue);
        }

        Debug.Log($"[InputFieldLimiter] �ִ밪 ����: {maxValue}");
    }

    /// <summary>
    /// �ּҰ��� ��Ÿ�ӿ� ����
    /// </summary>
    public void SetMinValue(float newMinValue)
    {
        minValue = newMinValue;

        // ���� ���� ���ο� �ּҰ����� ������ ����
        float currentValue = GetValue();
        if (currentValue < minValue)
        {
            SetValue(minValue);
        }

        Debug.Log($"[InputFieldLimiter] �ּҰ� ����: {minValue}");
    }

    /// <summary>
    /// ������ ��Ÿ�ӿ� ����
    /// </summary>
    public void SetRange(float newMinValue, float newMaxValue)
    {
        minValue = newMinValue;
        maxValue = newMaxValue;

        // ���� ���� ���ο� ������ ����� ����
        float currentValue = GetValue();
        if (currentValue < minValue || currentValue > maxValue)
        {
            SetValue(Mathf.Clamp(currentValue, minValue, maxValue));
        }

        Debug.Log($"[InputFieldLimiter] ���� ����: {minValue} ~ {maxValue}");
    }

    void OnDestroy()
    {
        // �̺�Ʈ ����
        if (inputField != null)
        {
            inputField.onValueChanged.RemoveListener(OnValueChanged);
            inputField.onEndEdit.RemoveListener(OnEndEdit);
        }
    }
}