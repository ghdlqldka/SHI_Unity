using _Magenta_WebGL;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace _SHI_BA
{
    public static class _SHI_BA_Config
    {
        private static string LOG_FORMAT = "<color=red><b>[_SHI_BA_Config]</b></color> {0}";

        public static string RootPath
        {
            get
            {
                return "Assets/_Framework/___SHI_BA";
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
                return "Samsung Heavy Industries";
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
                return _Magenta_WebGL_Config.bundleVersion;
            }
            set
            {
                _Magenta_WebGL_Config.bundleVersion = value;
            }
        }

        public static int bundleVersionCode
        {
            get
            {
                return _Magenta_WebGL_Config.bundleVersionCode;
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
                return _Magenta_WebGL_Config.Product;
            }
            set
            {
                _Magenta_WebGL_Config.Product = value;
            }
        }

        public static bool DefaultIsNativeResolution
        {
            get
            {
                return _Magenta_WebGL_Config.DefaultIsNativeResolution;
            }
            set
            {
                _Magenta_WebGL_Config.DefaultIsNativeResolution = value;
            }
        }

        public static int DefaultScreenWidth
        {
            get
            {
                return _Magenta_WebGL_Config.DefaultScreenWidth;
            }
            set
            {
                _Magenta_WebGL_Config.DefaultScreenWidth = value;
            }
        }

        public static int DefaultScreenHeight
        {
            get
            {
                return _Magenta_WebGL_Config.DefaultScreenHeight;
            }
            set
            {
                _Magenta_WebGL_Config.DefaultScreenHeight = value;
            }
        }

        static _SHI_BA_Config()
        {
            Product = _Base_Framework._Base_Framework_Config._Product.SHI_BA;

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

            if (Product == _Base_Framework._Base_Framework_Config._Product.SHI_BA)
            {
                Product = _Base_Framework._Base_Framework_Config._Product.SHI_BA;
                bundleVersion = "1.0.0";
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
            }
        }
    } // end-of-class
}