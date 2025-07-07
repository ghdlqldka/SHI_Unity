using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace _Magenta_Framework
{
    public class AppConfigurationEx : _Base_Framework._AppConfiguration
    {
        private static string LOG_FORMAT = "<color=#08FF00><b>[AppConfigurationEx]</b></color> {0}";

        public new AppConfigDataEx Data
        {
            get
            {
                return _appConfigData as AppConfigDataEx;
            }
            protected set
            {
                _appConfigData = value;
            }
        }

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject.name : " + this.gameObject.name);

            string postProcessBuild_DataPath;
#if UNITY_EDITOR
            // postProcessBuild_DataPath : D:/Works/Base_Framework/Assets
            postProcessBuild_DataPath = Application.dataPath + "/_Framework/_Magenta_Framework/_PostProcessBuild/PostProcessBuild_Data";
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

            XmlSerializer serializer = new XmlSerializer(typeof(AppConfigDataEx));
            StreamReader reader = new StreamReader(iniFilePath);
            _appConfigData = (AppConfigDataEx)serializer.Deserialize(reader.BaseStream);
#if DEBUG
            Debug.LogFormat(LOG_FORMAT, "<color=yellow>UseLogViewer : <b>" + Data.UseLogViewer + "</b></color>");
            Debug.LogFormat(LOG_FORMAT, "<color=yellow>OpenPlayerLogFolder : <b>" + Data.OpenPlayerLogFolder + "</b></color>");
#endif

            reader.Close();
        }
    }
}