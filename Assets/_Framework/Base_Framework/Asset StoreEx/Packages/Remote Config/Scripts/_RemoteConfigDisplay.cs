using System.Globalization;
using Sample.Deployment.RemoteConfigSample;
using TMPro;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.UI;

public class _RemoteConfigDisplay : RemoteConfigDisplay
{
    private static string LOG_FORMAT = "<color=#00FFD9><b>[_RemoteConfigDisplay]</b></color> {0}";

    // protected RemoteConfig m_RemoteConfig;
    protected _RemoteConfig _remoteConfig
    {
        get
        {
            return m_RemoteConfig as _RemoteConfig;
        }
    }

    protected override void Awake()
    {
        if (_remoteConfig == null)
        {
            Debug.LogErrorFormat(LOG_FORMAT, "Remote Config is null");
            return;
        }

        _remoteConfig.OnRemoteConfigUpdated += DisplayRemoteConfig;
    }

    protected override void DisplayRemoteConfig(RuntimeConfig config)
    {
        Debug.LogFormat(LOG_FORMAT, "DisplayRemoteConfig()");

        m_StringValue.text = config.GetString("string_key");
        m_IntValue.text = config.GetInt("int_key").ToString();
        m_BoolValue.isOn = config.GetBool("bool_key");
        m_LongValue.text = config.GetLong("long_key").ToString();
        m_FloatValue.text = config.GetFloat("float_key").ToString(CultureInfo.InvariantCulture);
        m_JsonValue.text = config.GetJson("json_key");
    }

    public override void GetConfig()
    {
        throw new System.NotSupportedException("");
    }

    public virtual void OnClickGetConfig()
    {
        Debug.LogFormat(LOG_FORMAT, "OnClickGetConfig()");

        var fetchConfigsAsync = _remoteConfig.FetchConfigsAsync();
    }
}
