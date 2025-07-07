using GoogleSheetsToUnity;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace _Base_Framework
{
    [System.Serializable]
    public class AnimalGoogleSheet : BaseGoogleSheet
    {
        private static string LOG_FORMAT = "<color=#00FF03><b>[AnimalGoogleSheet]</b></color> {0}";

        [ReadOnly]
        [SerializeField]
        private List<AnimalStatsRowData> rowDataList = new List<AnimalStatsRowData>();
        public List<AnimalStatsRowData> RowDataList
        {
            get
            {
                return rowDataList;
            }
        }

        public AnimalGoogleSheet() : base()
        {
            _sheetType = SheetType.Animal;
            _worksheetName = "Stats";
        }

#if UNITY_EDITOR
        public override void SaveAsJson(GstuSpreadSheet spreadSheet)
        {
            Debug.LogFormat(LOG_FORMAT, "SaveAsJson() - EditorMode");

            Dictionary<int, List<GSTU_Cell>> rowDic = spreadSheet.rows.primaryDictionary; // int(row index), ...

            foreach (KeyValuePair<int, List<GSTU_Cell>> keyValuePair in rowDic)
            {
                if (keyValuePair.Key == 1)
                {
                    continue;
                }

                string _name = FindData(keyValuePair.Value, "Name");
                int health = ParseInt(FindData(keyValuePair.Value, "Health"));
                int attack = ParseInt(FindData(keyValuePair.Value, "Attack"));
                int defence = ParseInt(FindData(keyValuePair.Value, "Defence"));
                string items = FindData(keyValuePair.Value, "Items");

                AnimalStatsRowData temp = new AnimalStatsRowData(_name, health, attack, defence, items);

                rowDataList.Add(temp);
            }

            CreateJsonFile(worksheetName, JsonConvert.SerializeObject(rowDataList));
        }
#endif

        protected override void ParseData(string jsonData)
        {
            Debug.LogFormat(LOG_FORMAT, "ParseData() - PlayMode");

            this.rowDataList = JsonConvert.DeserializeObject<List<AnimalStatsRowData>>(jsonData);

            Debug.LogWarningFormat(LOG_FORMAT, "<b><color=magenta>Finish</color></b>LoadJsonData(), worksheetName : <b><color=yellow>" + worksheetName + "</color></b>");
        }

        /*
        public override List<T> GetDataList<T>()
        {
            Debug.LogFormat(LOG_FORMAT, "GetDataList()");

            List<T> temp = new List<T>();
            for (int i = 0; i < rowDataList.Count; i++)
            {
                temp.Add((T)Convert.ChangeType(rowDataList[i], typeof(T)));
            }
            return temp;
        }
        */
    }
}