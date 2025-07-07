using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace _Base_Framework
{
    [RequireComponent(typeof(TMP_Text))]
    public class _LocalizeTMP_Text : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#0465FF><b>[_LocalizeTMP_Text]</b></color> {0}";

        [SerializeField]
        protected string key;

        [ReadOnly]
        [SerializeField]
        protected TMP_Text _TMP_text;

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            _TMP_text = this.GetComponent<TMP_Text>();
            Debug.Assert(_TMP_text != null);

            _LocalizationManager.OnLocaleChanged += OnLocaleChanged;
        }

        protected virtual IEnumerator Start()
        {
            // Wait for the localization system to initialize, loading Locales, preloading etc.
            yield return LocalizationSettings.InitializationOperation;

            Debug.LogFormat(LOG_FORMAT, "Start()");

            /*
            while (_LocalizationManager.Instance == null)
            {
                yield return null;
            }
            yield return null; // for-timing

            OnLocaleChanged(_LocalizationManager.Instance.SelectedLocale);
            */
        }

        protected virtual void OnLocaleChanged(Locale locale)
        {
            // Debug.LogFormat(LOG_FORMAT, "OnLocaleChanged(), locale : " + locale);

            Localize();
        }

        protected virtual void Localize()
        {
            string text = _LocalizedStringTable.Get(key);
#if DEBUG
            Debug.LogFormat(LOG_FORMAT, "Localize(), <color=red>" + _LocalizationManager.Instance.SelectedLocale + "</color> => key : <color=yellow>" + key + "</color>, text : <color=cyan>" + text + "</color>");
#endif

            _TMP_text.text = text;
        }
    }
}