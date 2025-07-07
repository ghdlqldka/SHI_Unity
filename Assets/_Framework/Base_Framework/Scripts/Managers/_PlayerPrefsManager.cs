using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Base_Framework
{
    public class _PlayerPrefsManager : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#5A934B><b>[_PlayerPrefsManager]</b></color> {0}";

        protected static _PlayerPrefsManager _instance;
        public static _PlayerPrefsManager Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Debug.LogFormat(LOG_FORMAT, "Awake()");

                Debug.Assert(_Base_Framework_Config.Product == _Base_Framework_Config._Product.Base_Framework); // Must be override this function!!!!!

                Instance = this;

                Init();
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this);
                return;
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        protected virtual void Update()
        {
            /*
#if DEBUG
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _Reset();
            }
#endif
            */
        }

        protected virtual void Init()
        {
            //
        }

        public void Save()
        {
            // By default Unity writes preferences to disk during OnApplicationQuit().
            // In cases when the game crashes or otherwise prematuraly exits,
            // you might want to write the PlayerPrefs at sensible 'checkpoints' in your game.
            // This function will write to disk potentially causing a small hiccup,
            // therefore it is not recommended to call during actual gameplay.
            PlayerPrefs.Save();
        }

        public delegate void ResetCallback();
        public event ResetCallback OnReset;

        protected void Invoke_OnReset()
        {
            if (OnReset != null)
            {
                OnReset();
            }
        }

        public virtual void _Reset()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "<b>_Reset()</b>");

            PlayerPrefs.DeleteAll();

            Init();

            Invoke_OnReset();
        }

        // Removes all keys and values from the preferences. Use with caution.
        protected void DeleteAll()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "<b><color=red>DeleteAll()</color></b>");

            PlayerPrefs.DeleteAll();
        }

        public static void SetBool(string key, bool value)
        {
            if (value == true)
            {
                PlayerPrefs.SetInt(key, 1);
            }
            else
            {
                PlayerPrefs.SetInt(key, 0);
            }
        }

        public static bool GetBool(string key)
        {
            Debug.AssertFormat(PlayerPrefs.HasKey(key) == true, "key : " + key);
            int value = PlayerPrefs.GetInt(key);
            Debug.Assert(value == 0 || value == 1);
            if (value == 0)
            {
                return false;
            }
            return true;
        }

        public static void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public static int GetInt(string key)
        {
            Debug.AssertFormat(PlayerPrefs.HasKey(key) == true, "key : " + key);
            return PlayerPrefs.GetInt(key);
        }

        public static void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }

        public static float GetFloat(string key)
        {
            Debug.AssertFormat(PlayerPrefs.HasKey(key) == true, "key : " + key);
            return PlayerPrefs.GetFloat(key);
        }

        public static void SetDouble(string key, double value)
        {
            //this saves the double you passed into the key you provided. 
            //but first it runs the function called DoubleToString to turn it into a string with the format R
            PlayerPrefs.SetString(key, DoubleToString(value));
        }

        //this retrieves a double stored under the key you provided and takes a default value.
        public static double GetDouble(string key, double defaultValue)
        {
            //store the default value incase its not stored.
            string defaultVal = DoubleToString(defaultValue);
            return StringToDouble(PlayerPrefs.GetString(key, defaultVal));
        }

        public static void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        public static string GetString(string key)
        {
            Debug.AssertFormat(PlayerPrefs.HasKey(key) == true, "key : " + key);
            return PlayerPrefs.GetString(key);
        }

        //this turned the double into a string with the format R
        //which microsoft states will ensure it converts back the same.
        /* link - https://msdn.microsoft.com/en-us/library/dwhawy9k.aspx#RFormatString */

        protected static string DoubleToString(double target)
        {
            return target.ToString("R");
        }

        protected static double StringToDouble(string target)
        {
            if (string.IsNullOrEmpty(target))
                return 0d;

            return double.Parse(target);
        }
    }
}