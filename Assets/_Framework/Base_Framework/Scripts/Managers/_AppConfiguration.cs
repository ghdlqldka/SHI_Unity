using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace _Base_Framework
{
    public class _AppConfiguration : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#918C35><b>[_AppConfiguration]</b></color> {0}";

        public static string AppConfigFile = "Base_Framework_AppConfig.ini";

        [ReadOnly]
        [SerializeField]
        protected _AppConfigData _appConfigData;
        public _AppConfigData Data
        {
            get
            {
                return _appConfigData;
            }
            protected set
            {
                _appConfigData = value;
            }
        }

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject.name : <b>" + this.gameObject.name + "</b>");

            string postProcessBuild_DataPath;
#if UNITY_EDITOR
            // postProcessBuild_DataPath : D:/Works/Base_Framework/Assets
            postProcessBuild_DataPath = Application.dataPath + "/_Framework/Base_Framework/_PostProcessBuild/PostProcessBuild_Data";
#else
            postProcessBuild_DataPath = Application.dataPath + "/PostProcessBuild_Data";
#endif
            // Debug.LogFormat(LOG_FORMAT, "postProcessBuild_DataPath : " + postProcessBuild_DataPath);
            string iniFilePath = postProcessBuild_DataPath + "/" + AppConfigFile;
            if (File.Exists(iniFilePath) == true)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "iniFile(<b><color=yellow>" + iniFilePath + "</color></b>) Exist");
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "Does NOT Exist!!!!!!, @@@@@@@@@@@@@@@@, " + iniFilePath);
                return;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(_AppConfigData));
            StreamReader reader = new StreamReader(iniFilePath);
            _appConfigData = (_AppConfigData)serializer.Deserialize(reader.BaseStream);
#if DEBUG
            Debug.LogFormat(LOG_FORMAT, "<color=yellow>UseLogViewer : <b>" + Data.UseLogViewer + "</b></color>");
            Debug.LogFormat(LOG_FORMAT, "<color=yellow>OpenPlayerLogFolder : <b>" + Data.OpenPlayerLogFolder + "</b></color>");
#endif

            reader.Close();
        }
    }
}