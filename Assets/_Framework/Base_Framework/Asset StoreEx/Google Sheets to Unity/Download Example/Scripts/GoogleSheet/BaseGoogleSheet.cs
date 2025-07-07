using GoogleSheetsToUnity;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace _Base_Framework
{
    public abstract class BaseGoogleSheet
    {
        private static string LOG_FORMAT = "<color=#FFBB00><b>[BaseGoogleSheet]</b></color> {0}";

        protected SheetType _sheetType = SheetType.None;
        public SheetType _SheetType
        {
            get
            {
                return _sheetType;
            }
        }
        protected string _worksheetName = "";
        public string worksheetName
        {
            get
            {
                return _worksheetName;
            }
        }

        protected Dictionary<SheetType, string> sheetIdDic = new Dictionary<SheetType, string>() // table, sheetId
        {
            { SheetType.Animal, "19bBi51z-6VwLQ4BCBXf9xx1VRX9zEagpfHrHb-udTP0" },
        };

        public BaseGoogleSheet()
        {
            //
        }

        public string GetSheetId()
        {
            return sheetIdDic[_SheetType];
        }

        public void LoadJsonData()
        {
            Debug.LogFormat(LOG_FORMAT, "LoadJsonData(), worksheetName : <b><color=yellow>" + worksheetName + "</color></b>");

            string path = string.Format("{0}/{1}.json", GoogleSheetManager.JsonFolderPath, worksheetName);
            Debug.LogFormat(LOG_FORMAT, "path : <b><color=yellow>" + path + "</color></b>");
            string jsonData = File.ReadAllText(path);

            ParseData(jsonData);
        }

#if UNITY_EDITOR
        public abstract void SaveAsJson(GstuSpreadSheet spreadSheet);

        protected virtual void CreateJsonFile(string worksheetName, string jsonData)
        {
            string path = string.Format("{0}/{1}.json", GoogleSheetManager.JsonFolderPath, worksheetName);

            Debug.LogFormat(LOG_FORMAT, "CreateJsonFile(), path : <b><color=yellow>" + path + "</color></b>");

            FileStream fileStream = new FileStream(path, FileMode.Create);

            string temp = JValue.Parse(jsonData).ToString(Newtonsoft.Json.Formatting.Indented);
            byte[] data = Encoding.UTF8.GetBytes(temp);
            fileStream.Write(data, 0, data.Length);

            fileStream.Close();
        }
#endif

        protected abstract void ParseData(string jsonData);

        /*
        public virtual List<T> GetDataList<T>()
        {
            return null;
        }
        */

        protected string FindData(List<GSTU_Cell> cellList, string columnId)
        {
            for (int i = 0; i < cellList.Count; i++)
            {
                if (cellList[i].columnId == columnId)
                {
                    return cellList[i].value; // <=====
                }
            }

            return "";
        }

        protected virtual bool ParseBool(string value)
        {
            if (value.Equals(""))
            {
                //Debug.LogError("Sheet bool value empty");
                return default;
            }

            return bool.Parse(value);
        }

        protected virtual int ParseInt(string value)
        {
            if (value.Equals(""))
            {
                //Debug.LogError("Sheet Int value empty");
                return default;
            }
            // Debug.Log(value);

            return int.Parse(value);
        }

        protected virtual float ParseFloat(string value)
        {
            if (value.Equals(""))
            {
                //Debug.LogError("Sheet float value empty");
                return default;
            }

            return float.Parse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
        }

        protected virtual double ParseDouble(string value)
        {
            if (value.Equals(""))
            {
                //Debug.LogError("Sheet double value empty");
                return default;
            }

            return double.Parse(value);
        }
    }
}