using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace _Base_Framework
{
    public class _LocalizationManager : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#00FBFF><b>[_LocalizationManager]</b></color> {0}";

        protected static _LocalizationManager _instance;
        public static _LocalizationManager Instance
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
        protected Locale _selectedLocale;
        public Locale SelectedLocale
        {
            get
            {
#if DEBUG
                Debug.Assert(LocalizationSettings.SelectedLocale == _selectedLocale);
#endif
                return _selectedLocale;
            }
        }

        public static event Action<Locale> OnLocaleChanged;

#if DEBUG
        [Header("=====> For DEBUGGING <=====")]
        [ReadOnly]
        [SerializeField]
        protected List<Locale> DEBUG_availableLocaleList;
#endif

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            if (Instance == null)
            {
                Instance = this;

#if DEBUG
                DEBUG_availableLocaleList = LocalizationSettings.AvailableLocales.Locales;
#endif
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

            Debug.LogFormat(LOG_FORMAT, "Start()");

            LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
            OnSelectedLocaleChanged(LocalizationSettings.SelectedLocale);
        }

#if DEBUG
        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                int index = 0;
                for (int i = 0; i < DEBUG_availableLocaleList.Count; i++)
                {
                    if (LocalizationSettings.SelectedLocale == DEBUG_availableLocaleList[i])
                    {
                        index = i + 1;
                        break;
                    }
                }

                if (index >= DEBUG_availableLocaleList.Count)
                {
                    index = 0;
                }

                SetLocale(DEBUG_availableLocaleList[index]);
            }
        }
#endif

        protected virtual void OnSelectedLocaleChanged(Locale locale)
        {
            Debug.LogFormat(LOG_FORMAT, "<b><color=yellow>OnSelectedLocaleChanged</color></b>(), <b><color=red>" + locale.Identifier + "</color></b>");
            // throw new System.NotImplementedException();

            _selectedLocale = locale;

            StartCoroutine(PostOnSelectedLocaleChanged());
        }

        protected IEnumerator PostOnSelectedLocaleChanged()
        {
            yield return null; // for-timing

            if (OnLocaleChanged != null)
            {
                OnLocaleChanged(SelectedLocale);
            }
        }

        public virtual void SetLocale(Locale locale)
        {
            Debug.LogFormat(LOG_FORMAT, "SetLocale(), locale : <b><color=red>" + locale + "</color></b>");

            LocalizationSettings.SelectedLocale = locale;
        }
    }
}