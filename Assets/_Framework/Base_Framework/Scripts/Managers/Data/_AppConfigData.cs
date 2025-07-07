using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace _Base_Framework
{
    [System.Serializable]
    public class _AppConfigData
    {
        public bool UseLogViewer;
        public bool OpenPlayerLogFolder; // by F12 key
    }
}