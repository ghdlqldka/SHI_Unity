using UnityEngine;

namespace GoogleSheetsToUnity
{
    /// <summary>
    /// Search class for accessing a database
    /// </summary>
    public class _GSTU_Search : GSTU_Search
    {
        private static string LOG_FORMAT = "<color=#007FFF><b>[_GSTU_Search]</b></color> {0}";

#if false
        public readonly string sheetId = "";
        public readonly string worksheetName = "Sheet1";

        public readonly string startCell = "A1";
        public readonly string endCell = "Z100";

        public readonly string titleColumn = "A";
        public readonly int titleRow = 1;
#endif

        public _GSTU_Search(string sheetId, string worksheetName) : base(sheetId, worksheetName)
        {
            Debug.LogFormat(LOG_FORMAT, "constructor!!!! - worksheetName : <b><color=yellow>" + worksheetName + "</color></b>");

#if false
            this.sheetId = sheetId;
            this.worksheetName = worksheetName;
#endif
        }

        public _GSTU_Search(string sheetId, string worksheetName, string startCell) : base(sheetId, worksheetName, startCell)
        {
            Debug.LogFormat(LOG_FORMAT, "constructor!!!! - worksheetName : <b><color=yellow>" + worksheetName + "</color></b>");

#if false
            this.sheetId = sheetId;
            this.worksheetName = worksheetName;
            this.startCell = startCell;
#endif
        }

        public _GSTU_Search(string sheetId, string worksheetName, string startCell, string endCell) : base(sheetId, worksheetName, startCell, endCell)
        {
            Debug.LogFormat(LOG_FORMAT, "constructor!!!! - worksheetName : <b><color=yellow>" + worksheetName + "</color></b>");

#if false
            this.sheetId = sheetId;
            this.worksheetName = worksheetName;
            this.startCell = startCell;
            this.endCell = endCell;
#endif
        }

        public _GSTU_Search(string sheetId, string worksheetName, string startCell, string endCell, string titleColumn, int titleRow) : base(sheetId, worksheetName, startCell, endCell, titleColumn, titleRow)
        {
            Debug.LogFormat(LOG_FORMAT, "constructor!!!! - worksheetName : <b><color=yellow>" + worksheetName + "</color></b>");

#if false
            this.sheetId = sheetId;
            this.worksheetName = worksheetName;
            this.startCell = startCell;
            this.endCell = endCell;
            this.titleColumn = titleColumn;
            this.titleRow = titleRow;
#endif
        }
    }

}