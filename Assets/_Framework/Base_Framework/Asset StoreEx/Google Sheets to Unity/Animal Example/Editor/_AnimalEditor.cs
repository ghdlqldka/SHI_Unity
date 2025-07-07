using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace GoogleSheetsToUnity
{
    [CustomEditor(typeof(_Animal))]
    public class _AnimalEditor : AnimalEditor
    {
        private static string LOG_FORMAT = "<color=#B1C5EF><b>[_AnimalEditor]</b></color> {0}";

        protected _Animal _animal
        {
            get
            {
                return animal as _Animal;
            }
        }

        protected override void OnEnable()
        {
            animal = (_Animal)target;
        }

#if false
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            OnInspectorGUI_Customization();
        }
#endif

        protected override void OnInspectorGUI_Customization()
        {
            GUILayout.Label("Read Data Examples");

            if (GUILayout.Button("Pull Data Method One"))
            {
                UpdateStats(UpdateMethodOneCallback);
            }

            if (GUILayout.Button("Pull Data Method Two"))
            {
                UpdateStats(UpdateMethodTwoCallback);
            }

            if (GUILayout.Button("Pull Data With merged Cells"))
            {
                UpdateStats(UpdateMethodMergedCellsCallback, true);
            }

            GUILayout.Label("Write Data Examples");
            GUILayout.Label("Update the existing data");
            if (GUILayout.Button("Update sheet information"))
            {
                UpdateAnimalInformationOnSheet();
            }

            if (GUILayout.Button("Update Only Health"))
            {
                UpdateAnimalHealth();
            }

            GUILayout.Label("Add New Data");
            if (GUILayout.Button("Add Via Append"))
            {
                AppendToSheet();
            }

            if (GUILayout.Button("Add Via Write"))
            {
                WriteToSheet();
            }
        }

        protected override void UpdateStats(UnityAction<GstuSpreadSheet> callback, bool mergedCells = false)
        {
            Debug.LogFormat(LOG_FORMAT, "UpdateStats()");

            _SpreadSheetManager.Read(new _GSTU_Search(_animal._associatedSheet, _animal.associatedWorksheet), callback, mergedCells);
        }

        protected virtual void UpdateMethodOneCallback(GstuSpreadSheet spreadSheet)
        {
            _GstuSpreadSheet ss = spreadSheet as _GstuSpreadSheet;
            List<GSTU_Cell> list = ss.rows[_animal.Name];
            _animal.UpdateStats(list);

            EditorUtility.SetDirty(target);
        }

        protected virtual void UpdateMethodTwoCallback(GstuSpreadSheet spreadSheet)
        {
            _GstuSpreadSheet ss = spreadSheet as _GstuSpreadSheet;
            _animal.UpdateStats(ss);

            EditorUtility.SetDirty(target);
        }

        protected virtual void UpdateMethodMergedCellsCallback(GstuSpreadSheet spreadSheet)
        {
            _GstuSpreadSheet ss = spreadSheet as _GstuSpreadSheet;
            _animal.UpdateStats(ss, true);

            EditorUtility.SetDirty(target);
        }

        protected override void UpdateAnimalInformationOnSheet()
        {
            _SpreadSheetManager.Read(new _GSTU_Search(_animal._associatedSheet, _animal.associatedWorksheet), OnUpdateAnimalInformationCallback);
        }

        protected virtual void OnUpdateAnimalInformationCallback(GstuSpreadSheet spreadSheet)
        {
            _GstuSpreadSheet ss = spreadSheet as _GstuSpreadSheet;
            BatchRequestBody updateRequest = new BatchRequestBody();
            updateRequest.Add(ss[_animal.Name, "Health"].AddCellToBatchUpdate(_animal.associatedSheet, _animal.associatedWorksheet, _animal.health.ToString()));
            updateRequest.Add(ss[_animal.Name, "Defence"].AddCellToBatchUpdate(_animal.associatedSheet, _animal.associatedWorksheet, _animal.health.ToString()));
            updateRequest.Add(ss[_animal.Name, "Attack"].AddCellToBatchUpdate(_animal.associatedSheet, _animal.associatedWorksheet, _animal.health.ToString()));
            updateRequest.Send(_animal.associatedSheet, _animal.associatedWorksheet, null);
        }

        protected override void UpdateAnimalHealth()
        {
            // base.UpdateAnimalHealth();
            _SpreadSheetManager.Read(new _GSTU_Search(_animal._associatedSheet, _animal.associatedWorksheet), OnUpdateAnimalHealthCallback);
        }

        protected virtual void OnUpdateAnimalHealthCallback(GstuSpreadSheet spreadSheet)
        {
            _GstuSpreadSheet ss = spreadSheet as _GstuSpreadSheet;
            string value = _animal.health.ToString();
            Debug.LogFormat(LOG_FORMAT, "OnUpdateAnimalHealthCallback(), value : " + value);
            ss[_animal.Name, "Health"].UpdateCellValue(_animal.associatedSheet, _animal.associatedWorksheet, value);
        }

        protected override void AppendToSheet()
        {
            List<string> list = new List<string>() { _animal.Name, _animal.health.ToString(), _animal.attack.ToString(), _animal.defence.ToString() };

            _SpreadSheetManager.Append(new _GSTU_Search(_animal._associatedSheet, _animal.associatedWorksheet), new ValueRange(list), null);
        }

        protected override void WriteToSheet()
        {
            // base.WriteToSheet();

            List<string> list = new List<string>();

            list.Add(_animal.Name);
            list.Add(_animal.health.ToString());
            list.Add(_animal.attack.ToString());
            list.Add(_animal.defence.ToString());

            _SpreadSheetManager.Write(new _GSTU_Search(_animal._associatedSheet, _animal.associatedWorksheet, "G10"), new ValueRange(list), null);
        }
    }
}