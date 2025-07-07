using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Base_Framework
{
    public enum SheetType
    {
        None = -1,

        Animal,
    }

    public class GoogleSheetManager : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#94B530><b>[GoogleSheetManager]</b></color> {0}";

        protected static GoogleSheetManager _instance = null;
        public static GoogleSheetManager Instance
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

        public static string JsonFolderPath = "Assets/_Framework/Base_Framework/Asset StoreEx/Google Sheets to Unity/Download Example/LocalRes";

        [ReadOnly]
        [SerializeField]
        protected int _maxGoogleSheetLength = Enum.GetValues(typeof(SheetType)).Length - 1;
        public int MaxGoogleSheetLength
        {
            get
            {
                return _maxGoogleSheetLength;
            }
        }

        [ReadOnly]
        [SerializeField]
        protected int _loadedCount = 0;
        protected int LoadedCount
        {
            get
            {
                return _loadedCount;
            }
            set
            {
                _loadedCount = value;
                Debug.Assert(value <= MaxGoogleSheetLength);
                if (value == MaxGoogleSheetLength)
                {
                    Invoke_OnLoadDone();
                }
            }
        }

        protected Dictionary<SheetType, BaseGoogleSheet> googleSheetDictionary = new Dictionary<SheetType, BaseGoogleSheet>(); // table type, table

        public delegate void LoadDone();
        public event LoadDone OnLoadDone;

        protected void Invoke_OnLoadDone()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "<b><color=yellow>Invoke_OnLoadDone()</color></b>");

            if (OnLoadDone != null)
            {
                OnLoadDone();
            }
        }

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                int count = Enum.GetValues(typeof(SheetType)).Length - 1;
                _maxGoogleSheetLength = count;
                LoadedCount = 0;
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

        protected virtual void Update()
        {
#if DEBUG
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                LoadData();

                List<AnimalStatsRowData> rowDataList = ((AnimalGoogleSheet)googleSheetDictionary[SheetType.Animal]).RowDataList;
                for (int i = 0; i < rowDataList.Count; i++)
                {
                    Debug.LogFormat(LOG_FORMAT, "" + rowDataList[i]._Name);
                }
            }
#endif
        }

#if DEBUG
        [ReadOnly]
        [SerializeField]
        protected AnimalGoogleSheet DEBUG_animalGoogleSheet;
#endif
        public virtual void LoadData()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "==========> LoadData() <==========, MaxGoogleSheetLength : " + MaxGoogleSheetLength);

            Debug.Assert(LoadedCount == 0);

            if (LoadedCount >= MaxGoogleSheetLength)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "Is it possible??????");
                return;
            }

            for (int i = 0; i < MaxGoogleSheetLength; i++)
            {
                BaseGoogleSheet googleSheet = null;
                switch (Enum.GetValues(typeof(SheetType)).GetValue(i))
                {
                    case SheetType.Animal:
                        googleSheet = new AnimalGoogleSheet();
                        googleSheet.LoadJsonData();
#if DEBUG
                        DEBUG_animalGoogleSheet = googleSheet as AnimalGoogleSheet;
#endif
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }

                googleSheetDictionary.Add(googleSheet._SheetType, googleSheet);
                LoadedCount++;
            }
        }

        public virtual void _Reset()
        {
            _loadedCount = 0;
            googleSheetDictionary.Clear();
        }
    }
}