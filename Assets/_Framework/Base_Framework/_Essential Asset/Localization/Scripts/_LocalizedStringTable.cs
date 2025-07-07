using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

public class _LocalizedStringTable : MonoBehaviour
{
    private static string LOG_FORMAT = "<color=#8DFFC0><b>[LocalizeString]</b></color> {0}";

    protected static _LocalizedStringTable _instance;
    public static _LocalizedStringTable Instance
    {
        get
        {
            return _instance;
        }
        protected set
        {
            _instance = value;
        }
    }

    [ReadOnly]
    [SerializeField]
    protected bool _isReady = false;
    public bool IsReady
    {
        get
        {
            return _isReady;
        }
    }

    [Space(10)]
    [SerializeField]
    protected TableReference tableReference;
    protected static StringTable _stringTable;

#if DEBUG
    [Header("========== For DEBUGGING ==========")]
    [ReadOnly]
    [SerializeField]
    protected StringTable DEBUG_stringTable = null;
#endif
    protected virtual void Awake()
    {
        Debug.LogFormat(LOG_FORMAT, "Awake(), Table Collection Name : <b><color=red>" + tableReference + "</color></b>");

        if (Instance == null)
        {
            Instance = this;

            _isReady = false;
        }
        else
        {
            Debug.LogErrorFormat(LOG_FORMAT, "");
            Destroy(this);
            return;
        }
    }

    protected virtual void OnDestroy()
    {
        if (Instance != this)
        {
            return;
        }

        Instance = null;
    }

    protected virtual IEnumerator Start()
    {
        // Wait for the localization system to initialize, loading Locales, preloading etc.
        yield return LocalizationSettings.InitializationOperation;

        LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
        OnSelectedLocaleChanged(LocalizationSettings.SelectedLocale);
    }

    protected virtual void OnSelectedLocaleChanged(Locale locale)
    {
        Debug.LogFormat(LOG_FORMAT, "<b><color=yellow>OnSelectedLocaleChanged</color></b>(), <b><color=red>" + locale.Identifier + "</color></b>");

        AsyncOperationHandle<StringTable> op = LocalizationSettings.StringDatabase.GetTableAsync(tableReference, locale);
        op.WaitForCompletion();
        _stringTable = op.Result;
#if DEBUG
        Debug.LogFormat(LOG_FORMAT, "_stringTable.Count : <color=yellow>" + _stringTable.Count + "</color>");
        DEBUG_stringTable = _stringTable;
#endif
        // Get("aaaa");

        _isReady = true;
    }

    public static string Get(string key)
    {
        StringTableEntry entry = _stringTable.GetEntry(key);
        if (entry == null)
        {
            Debug.LogErrorFormat(LOG_FORMAT, "There is No key(<color=red>" + key + "</color>)!!!!!!!!");
            return key;
        }

        return entry.Value;
    }
}