using UnityEngine;

namespace GoogleSheetsToUnity
{

    /// <summary>
    /// example script to show realtime updates of multiple items
    /// </summary>
    public class _AnimalManager : AnimalManager
    {
        private static string LOG_FORMAT = "<color=#3E9AF6><b>[_AnimalManager]</b></color> {0}";

        protected static _AnimalManager _instance;
        public static _AnimalManager Instance
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

        public _AnimalContainer _container
        {
            get
            {
                return container as _AnimalContainer;
            }
        }

        protected override void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                Debug.LogFormat(LOG_FORMAT, "Awake(), updateOnPlay : <b>" + updateOnPlay + "</b>" + ", _container.name : <b>" + _container.name + "</b>");

                // https://docs.google.com/spreadsheets/d/19bBi51z-6VwLQ4BCBXf9xx1VRX9zEagpfHrHb-udTP0/edit#gid=0
                associatedSheet = "19bBi51z-6VwLQ4BCBXf9xx1VRX9zEagpfHrHb-udTP0";
                associatedWorksheet = "Stats";

                Debug.LogFormat(LOG_FORMAT, "associatedSheet : <b><color=magenta>" + associatedSheet + "</color></b>, associatedWorksheet : <b><color=yellow>" + associatedWorksheet + "</color></b>");
                if (updateOnPlay == true)
                {
                    UpdateStats();
                }
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

        protected override void UpdateStats()
        {
            Debug.LogFormat(LOG_FORMAT, "UpdateStats(), sheetStatus : <b><color=yellow>" + sheetStatus + "</color></b>");

            if (sheetStatus == SheetStatus.PRIVATE)
            {
                _SpreadSheetManager.Read(new _GSTU_Search(associatedSheet, associatedWorksheet), OnSpreadSheetLoadedCallback);
            }
            else if (sheetStatus == SheetStatus.PUBLIC)
            {
                // _SpreadsheetManager.ReadPublicSpreadsheet(new _GSTU_Search(associatedSheet, associatedWorksheet), UpdateAllAnimals);
                _SpreadSheetManager.ReadPublicSpreadSheet(new _GSTU_Search(associatedSheet, associatedWorksheet, "A1", "E9"), OnSpreadSheetLoadedCallback);
            }
            else
            {
                Debug.Assert(false);
            }
        }

        protected virtual void OnSpreadSheetLoadedCallback(GstuSpreadSheet sheet)
        {
            Debug.LogFormat(LOG_FORMAT, "<b><color=yellow>OnSpreadSheetLoadedCallback()</color></b>");

            _GstuSpreadSheet ss = sheet as _GstuSpreadSheet;
            // UpdateAllAnimals(ss);
            foreach (_Animal animal in _container.allAnimals)
            {
                animal.UpdateStats(ss);
            }

            foreach (_AnimalObject animalObject in animalObjects)
            {
                animalObject.BuildAnimalInfo();
            }
        }
    }
}