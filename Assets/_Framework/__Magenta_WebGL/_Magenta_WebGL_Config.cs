using _Magenta_Framework;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace _Magenta_WebGL
{
    public static class _Magenta_WebGL_Config
    {
        private static string LOG_FORMAT = "<color=red><b>[_Magenta_WebGL_Config]</b></color> {0}";

        public static string RootPath
        {
            get
            {
                return "Assets/_Framework/__Magenta_WebGL";
            }
        }

        public static string IconFilePath
        {
            get
            {
                return RootPath + "/Textures/Icons/" + Product + "_Icon.png";
            }
        }

        public static string companyName
        {
            get
            {
                // return productName;
                return "Magenta Robotics";
            }
        }

        public static string productName
        {
            get
            {
                return ("" + Product);
            }
        }

        // public static Color ambientLight = new Color(0.8f, 0.8f, 0.8f, 1);

        public static string applicationIdentifier
        {
            get
            {
                return "com." + companyName + "." + productName;
            }
        }

        public static string bundleVersion
        {
            get
            {
                return _Magenta_Framework_Config.bundleVersion;
            }
            set
            {
                _Magenta_Framework_Config.bundleVersion = value;
            }
        }

        public static int bundleVersionCode
        {
            get
            {
                return _Magenta_Framework_Config.bundleVersionCode;
            }
        }

        public static string AssetBundleURL
        {
            get;
            set;
        }

        public static string Advertisement_gameId
        {
            get;
            set;
        }

        public static _Base_Framework._Base_Framework_Config._Product Product
        {
            get
            {
                return _Magenta_Framework_Config.Product;
            }
            set
            {
                _Magenta_Framework_Config.Product = value;
            }
        }

        public static bool DefaultIsNativeResolution
        {
            get
            {
                return _Magenta_Framework_Config.DefaultIsNativeResolution;
            }
            set
            {
                _Magenta_Framework_Config.DefaultIsNativeResolution = value;
            }
        }

        public static int DefaultScreenWidth
        {
            get
            {
                return _Magenta_Framework_Config.DefaultScreenWidth;
            }
            set
            {
                _Magenta_Framework_Config.DefaultScreenWidth = value;
            }
        }

        public static int DefaultScreenHeight
        {
            get
            {
                return _Magenta_Framework_Config.DefaultScreenHeight;
            }
            set
            {
                _Magenta_Framework_Config.DefaultScreenHeight = value;
            }
        }

#if UNITY_IOS
        public static string buildNumber
        {
            get;
            set;
        }

        // PlayerSettings.iOS.appleDeveloperTeamID = "3EN4J47AP6";
        public static string appleDeveloperTeamID
        {
            get;
            set;
        }

        public static bool appleEnableAutomaticSigning
        {
            get;
            set;
        }

        public static bool hideHomeButton
        {
            get;
            set;
        }
#endif

        static _Magenta_WebGL_Config()
        {
            Product = _Base_Framework._Base_Framework_Config._Product.Magenta_WebGL;

            Debug.LogFormat(LOG_FORMAT, "STATIC constructor!!!!!");

            DefaultIsNativeResolution = true;
            if (DefaultIsNativeResolution == true)
            {
                DefaultScreenWidth = Screen.width;
                DefaultScreenHeight = Screen.height;
            }
            else
            {
                DefaultScreenWidth = 1920;
                DefaultScreenHeight = 1080;
            }

            if (Product == _Base_Framework._Base_Framework_Config._Product.Magenta_WebGL)
            {
                Product = _Base_Framework._Base_Framework_Config._Product.Magenta_WebGL;
                bundleVersion = "1.0.0";
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
            }

#if UNITY_IOS
            buildNumber = "1";
            appleDeveloperTeamID = "3EN4J47AP6";
            appleEnableAutomaticSigning = true;
            hideHomeButton = true;
#endif
        }
    } // end-of-class
}