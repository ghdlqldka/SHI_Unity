using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GreenerGames;
using UnityEngine;
using UnityEngine.Events;

namespace GoogleSheetsToUnity
{
    [Serializable]
    public class _GSTU_Cell : GSTU_Cell
    {
#if false
        string column = string.Empty;
        public string columnId = string.Empty;

        int row = -1;
        public string rowId = string.Empty;

        public string value = string.Empty;

        internal List<string> titleConnectedCells = new List<string>();
#endif

        public _GSTU_Cell(string value, string column, int row) : base(value, column, row)
        {
#if false
            this.value = value;
            this.column = column;
            this.row = row;
#endif
        }

        public _GSTU_Cell(string value) : base(value)
        {
#if false
            this.value = value;
#endif
        }

#if false
        public string Column()
        {
            return column;
        }
        public int Row()
        {
            return row;
        }
        public string CellRef()
        {
            return column + row;
        }
#endif

        //TODO: store the sheetId and worksheet in the spreadsheet so dont have to pass these through
        public override void UpdateCellValue(string sheetId, string worksheet, string value, UnityAction callback = null)
        {
            this.value = value;
            List<string> list = new List<string>();
            list.Add(value);
            _SpreadSheetManager.Write(new _GSTU_Search(sheetId, worksheet, CellRef()), new ValueRange(list), callback);
        }

#if false
        //TODO: store the sheetId and worksheet in the spreadsheet so dont have to pass these through
        internal ValueRange AddCellToBatchUpdate(string sheetID, string worksheet, string value)
        {
            this.value = value;
            List<string> list = new List<string>();
            list.Add(value);
            ValueRange data = new ValueRange(list);
            data.range = CellRef();
            return data;
        }
#endif
    }
}