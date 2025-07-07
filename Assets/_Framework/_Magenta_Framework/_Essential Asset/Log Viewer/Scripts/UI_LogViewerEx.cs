#if UNITY_CHANGE1 || UNITY_CHANGE2 || UNITY_CHANGE3 || UNITY_CHANGE4
#warning UNITY_CHANGE has been set manually
#elif UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
#define UNITY_CHANGE1
#elif UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#define UNITY_CHANGE2
#else
#define UNITY_CHANGE3
#endif

#if UNITY_2018_3_OR_NEWER
#define UNITY_CHANGE4
#endif
//use UNITY_CHANGE1 for unity older than "unity 5"
//use UNITY_CHANGE2 for unity 5.0 -> 5.3 
//use UNITY_CHANGE3 for unity 5.3 (fix for new SceneManger system)
//use UNITY_CHANGE4 for unity 2018.3 (Networking system)

using UnityEngine;

namespace _Magenta_Framework
{
	public class UI_LogViewerEx : _Base_Framework._UI_LogViewer
    {
        private static string LOG_FORMAT = "<color=white><b>[UI_LogViewerEx]</b></color> {0}";

        protected new LogViewerEx logViewer
        {
            get
            {
                return _logViewer as LogViewerEx;
            }
        }

        public override void OnValueChangedShowMemory(bool value)
        {
            Debug.LogFormat(LOG_FORMAT, "OnValueChangedShowMemory(), value : " + value);

            logViewer.ShowMemory = value;
        }
    }


}