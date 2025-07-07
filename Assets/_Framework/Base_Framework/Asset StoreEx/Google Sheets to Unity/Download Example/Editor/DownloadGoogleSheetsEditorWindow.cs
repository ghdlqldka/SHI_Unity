using GoogleSheetsToUnity;
using GoogleSheetsToUnity.ThirdPary;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace _Base_Framework
{
    public class DownloadGoogleSheetsEditorWindow : EditorWindow
    {
        private static string LOG_FORMAT = "<color=#37AFB6><b>[DownloadGoogleSheetsEditorWindow]</b></color> {0}";

        protected static bool cancelable = false;
        protected static int tableLength = 0;

        protected static BaseGoogleSheet googleSheet = null;
        protected static bool isDownloading = false;

        [MenuItem("GoogleSheetsToUnity/==> Download Sheets(Base_Framework)")]
        private static void DownloadSheets()
        {
            googleSheet = null;
            EditorCoroutineRunner.StartCoroutine(DownloadAllSheets());
        }

        private static IEnumerator DownloadAllSheets()
        {
            Debug.LogFormat(LOG_FORMAT, "DownloadAllSheets()");

            tableLength = Enum.GetValues(typeof(SheetType)).Length - 1;

            int count = 0;

            for (int i = 0; i < tableLength; i++)
            {
                SheetType sheetType = (SheetType)Enum.GetValues(typeof(SheetType)).GetValue(i);
                switch (sheetType)
                {
                    case SheetType.Animal:
                        googleSheet = new AnimalGoogleSheet();
                        string sheetId = googleSheet.GetSheetId();
                        _GSTU_Search search = new _GSTU_Search(sheetId, googleSheet.worksheetName, "A1", "E9");

                        isDownloading = true;
                        _SpreadSheetManager.Read(search, OnReadGoogleSheetCallback);
                        break;
                    
                    default:
                        Debug.Assert(false);
                        break;
                }

                while (isDownloading == true)
                {
                    yield return null;
                }

                count += 1;
                DisplayProgressBar(count);

                if (cancelable == true)
                {
                    EditorUtility.ClearProgressBar();
                    yield break;
                }
            } // end-of-for
        }

        private static void OnReadGoogleSheetCallback(GstuSpreadSheet spreadSheet)
        {
            Debug.LogFormat(LOG_FORMAT, "OnReadGoogleSheet<b><color=red>Callback</color></b>()");

            _GstuSpreadSheet _spreadSheet = spreadSheet as _GstuSpreadSheet;
            googleSheet.SaveAsJson(_spreadSheet);

            googleSheet = null;

            isDownloading = false;
        }

        private static void DisplayProgressBar(int count)
        {
            if (count < tableLength)
            {
                cancelable = EditorUtility.DisplayCancelableProgressBar("SpreadSheet Progress Bar", "Shows a progress bar for the given seconds", count / tableLength);
            }
            else
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}