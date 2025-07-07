using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GoogleSheetsToUnity
{
    [Serializable]
    public class _GstuSpreadSheet : GstuSpreadSheet
    {
        private static string LOG_FORMAT = "<color=#94C7FB><b>[_GstuSpreadSheet]</b></color> {0}";

        // public Dictionary<string, GSTU_Cell> Cells = new Dictionary<string, GSTU_Cell>();
        public Dictionary<string, GSTU_Cell> CellDic
        {
            get
            {
                return Cells;
            }
        }
#if false
        /// <summary>
        ///     All the cells that the spreadsheet loaded
        ///     Index is Cell ID IE "A2"
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, GSTU_Cell> Cells = new Dictionary<string, GSTU_Cell>();

        public SecondaryKeyDictionary<string, List<GSTU_Cell>> columns =
            new SecondaryKeyDictionary<string, List<GSTU_Cell>>();

        public SecondaryKeyDictionary<int, string, List<GSTU_Cell>> rows =
            new SecondaryKeyDictionary<int, string, List<GSTU_Cell>>();
#endif

        /*     public GstuSpreadSheet(GSTU_SpreadsheetResponce data)
             {
                 string startColumn = Regex.Replace(data.StartCell(), "[^a-zA-Z]", "");
                 int startRow = int.Parse(Regex.Replace(data.StartCell(), "[^0-9]", ""));

                 int startColumnAsInt = GoogleSheetsToUnityUtilities.NumberFromExcelColumn(startColumn);
                 int currentRow = startRow;

                 foreach (List<string> dataValue in data.valueRange.values)
                 {
                     int currentColumn = startColumnAsInt;

                     foreach (string entry in dataValue)
                     {
                         string realColumn = GoogleSheetsToUnityUtilities.ExcelColumnFromNumber(currentColumn);
                         GSTU_Cell cell = new GSTU_Cell(entry, realColumn, currentRow);

                         CellDic.Add(realColumn + currentRow, cell);

                         if (!rows.ContainsKey(currentRow))
                         {
                             rows.Add(currentRow, new List<GSTU_Cell>());
                         }

                         rows[currentRow].Add(cell);

                         if (!columns.ContainsPrimaryKey(realColumn))
                         {
                             columns.Add(realColumn, new List<GSTU_Cell>());
                         }

                         columns[realColumn].Add(cell);

                         currentColumn++;
                     }

                     currentRow++;
                 }

                 if(data.sheetInfo != null)
                 {
                     foreach(var merge in data.sheetInfo.merges)
                     {
                         Debug.Log("Merge starts at : " + merge.startRowIndex + " " + GoogleSheetsToUnityUtilities.ExcelColumnFromNumber(merge.startColumnIndex));
                     }
                 }
             }*/

        public _GstuSpreadSheet() : base()
        {
            //
        }

        public _GstuSpreadSheet(GSTU_SpreadsheetResponce data, string titleColumn, int titleRow) : base()
        {
            // Debug.Log("data : " + data);
            string startColumn = Regex.Replace(data.StartCell(), "[^a-zA-Z]", "");
            int startRow = int.Parse(Regex.Replace(data.StartCell(), "[^0-9]", ""));

            // Debug.LogFormat(LOG_FORMAT, "startColum : " + startColumn + ", startRow : " + startRow);

            int startColumnAsInt = Utils.GoogleSheetsToUnityUtilities.NumberFromExcelColumn(startColumn);
            int currentRow = startRow;

            Dictionary<string, string> mergeCellRedirect = new Dictionary<string, string>();
            if (data.sheetInfo != null)
            {
                foreach (var merge in data.sheetInfo.merges)
                {
                    string cell = Utils.GoogleSheetsToUnityUtilities.ExcelColumnFromNumber(merge.startColumnIndex + 1) + (merge.startRowIndex + 1);

                    for (int r = merge.startRowIndex; r < merge.endRowIndex; r++)
                    {
                        for (int c = merge.startColumnIndex; c < merge.endColumnIndex; c++)
                        {
                            string mergeCell = Utils.GoogleSheetsToUnityUtilities.ExcelColumnFromNumber(c + 1) + (r + 1);
                            mergeCellRedirect.Add(mergeCell, cell);
                        }
                    }
                }
            }

            foreach (List<string> dataValue in data.valueRange.values)
            {
                int currentColumn = startColumnAsInt;

                foreach (string entry in dataValue)
                {
                    string realColumn = Utils.GoogleSheetsToUnityUtilities.ExcelColumnFromNumber(currentColumn);
                    string cellID = realColumn + currentRow;

                    _GSTU_Cell cell = null;
                    if (mergeCellRedirect.ContainsKey(cellID) && CellDic.ContainsKey(mergeCellRedirect[cellID]))
                    {
                        cell = CellDic[mergeCellRedirect[cellID]] as _GSTU_Cell;
                    }
                    else
                    {
                        cell = new _GSTU_Cell(entry, realColumn, currentRow);

                        //check the title row and column exist, if not create them
                        if (rows.ContainsKey(currentRow) == false)
                        {
                            rows.Add(currentRow, new List<GSTU_Cell>());
                        }

                        if (columns.ContainsPrimaryKey(realColumn) == false)
                        {
                            columns.Add(realColumn, new List<GSTU_Cell>());
                        }

                        rows[currentRow].Add(cell);
                        columns[realColumn].Add(cell);


                        //build a series of seconard keys for the rows and columns
                        if (realColumn == titleColumn)
                        {
                            Debug.LogFormat(LOG_FORMAT, "currentRow : <b>" + currentRow + "</b>, cell.value : <color=yellow><b>" + cell.value + "</b></color>");
                            rows.LinkSecondaryKey(currentRow, cell.value);
                        }

                        if (currentRow == titleRow)
                        {
                            columns.LinkSecondaryKey(realColumn, cell.value);
                        }
                    }

                    CellDic.Add(cellID, cell);

                    currentColumn++;
                }

                currentRow++;
            }

            //build the column and row string Id's from titles
            foreach (_GSTU_Cell cell in CellDic.Values)
            {
                cell.columnId = CellDic[cell.Column() + titleRow].value;
                cell.rowId = CellDic[titleColumn + cell.Row()].value;
            }

            //build all links to row and columns for cells that are handled by merged title fields.
            foreach (_GSTU_Cell cell in CellDic.Values)
            {
                foreach (KeyValuePair<string, GSTU_Cell> keyValuePair in CellDic)
                {
                    if (cell.columnId == keyValuePair.Value.columnId && cell.rowId == keyValuePair.Value.rowId)
                    {
                        if (!cell.titleConnectedCells.Contains(keyValuePair.Key))
                        {
                            cell.titleConnectedCells.Add(keyValuePair.Key);
                        }
                    }
                }
            }
        }

#if false
        public GSTU_Cell this[string cellRef]
        {
            get
            {
                return CellDic[cellRef];
            }
        }

        public GSTU_Cell this[string rowId, string columnId]
        {
            get
            {
                    string columnIndex = columns.secondaryKeyLink[columnId];
                    int rowIndex = rows.secondaryKeyLink[rowId];

                    return CellDic[columnIndex + rowIndex];
            }
        }

        public List<GSTU_Cell> this [string rowID, string columnID, bool mergedCells]
        {
            get
            {
                string columnIndex = columns.secondaryKeyLink[columnID];
                int rowIndex = rows.secondaryKeyLink[rowID];
                List < string > actualCells = CellDic[columnIndex + rowIndex].titleConnectedCells;

                List<GSTU_Cell> returnCells = new List<GSTU_Cell>();
                foreach(string s in actualCells)
                {
                    returnCells.Add(CellDic[s]);
                }

                return returnCells;
            }
        }
#endif
    }
}