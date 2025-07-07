using System.IO;
using UnityEngine;

namespace _Base_Framework
{
    public static class _Base_Framework_Config
    {
        private static string LOG_FORMAT = "<color=red><b>[_Base_Framework_Config]</b></color> {0}";

        public static string RootPath
        {
            get
            {
                return "Assets/_Framework/Base_Framework";
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
                return "Base_Framework_Company";
            }
        }

        public static string productName
        {
            get
            {
                return ("" + Product);
            }
        }

        public static string applicationIdentifier
        {
            get
            {
                return "com." + companyName + "." + productName;
            }
        }

        // Must has format like "4.10.3"
        public static string bundleVersion
        {
            get;
            set;
        }

        // you can use a major*10000 + minor*100 + build formula - that's what Google does with their apps, so that 4.10.3 would have code 41003
        public static int bundleVersionCode
        {
            get
            {
                string[] number = bundleVersion.Split('.');
                Debug.Assert(number.Length == 3); // major.minor.build
                int code = int.Parse(number[0]) * 10000 + int.Parse(number[1]) * 100 + int.Parse(number[2]);
                return code;
            }
        }

        public static bool ChineseLanguageSupport
        {
            get;
            private set;
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

        public enum _Product
        {
            Base_Framework,

            PC_Framework,
            Mobile_Framework,
            VR_Framework,
            Magenta_Framework,

            // PC_Framework

            // Mobile
            AR_Framework,
            Blockchain_Framework,
            AD_Sonic,

            // Magenta_Framework
            Magenta_WebGL,
            SHI_BA, // Blasting automation at Samsung Heavy Industries

            GT3,
            GT4,

            // AR_Framework

            // LG_Framework

            // VR_Framework
        };

        public static _Product Product;

        public static bool DefaultIsNativeResolution
        {
            get;
            set;
        }

        public static int DefaultScreenWidth
        {
            get;
            set;
        }

        public static int DefaultScreenHeight
        {
            get;
            set;
        }

        static _Base_Framework_Config()
        {
            Debug.LogFormat(LOG_FORMAT, "STATIC constructor!!!!!");

            Product = _Product.Base_Framework;

            ChineseLanguageSupport = false;

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

            bundleVersion = "1.0.0"; // major.minor.build
        }
    } // end-of-class
}