using System.Globalization;
using Sample.Deployment.RemoteConfigSample;
using TMPro;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.UI;

public class RemoteConfigDisplay : MonoBehaviour
{
    [SerializeReference] protected RemoteConfig m_RemoteConfig;

    [SerializeField] protected TextMeshProUGUI m_StringValue;
    [SerializeField] protected TextMeshProUGUI m_IntValue;
    [SerializeField] protected Toggle m_BoolValue;
    [SerializeField] protected TextMeshProUGUI m_LongValue;
    [SerializeField] protected TextMeshProUGUI m_FloatValue;
    [SerializeField] protected TextMeshProUGUI m_JsonValue;

    protected virtual void Awake()
    {
        if (!m_RemoteConfig)
        {
            Debug.LogError("Remote Config is null");
            return;
        }

        m_RemoteConfig.OnRemoteConfigUpdated += DisplayRemoteConfig;
    }

    public virtual void GetConfig()
    {
        var fetchConfigsAsync = m_RemoteConfig.FetchConfigsAsync();
    }

    protected virtual void DisplayRemoteConfig(RuntimeConfig config)
    {
        m_StringValue.text = config.GetString("string_key");
        m_IntValue.text = config.GetInt("int_key").ToString();
        m_BoolValue.isOn = config.GetBool("bool_key");
        m_LongValue.text = config.GetLong("long_key").ToString();
        m_FloatValue.text = config.GetFloat("float_key").ToString(CultureInfo.InvariantCulture);
        m_JsonValue.text = config.GetJson("json_key");
    }
}
