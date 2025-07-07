using System;
using System.Collections;
using System.Net;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Networking;

namespace GoogleSheetsToUnity
{
    public class _GoogleAuthrisationHelper : GoogleAuthrisationHelper
    {
        private static string LOG_FORMAT = "<color=#FFCD00><b>[_GoogleAuthrisationHelper]</b></color> {0}";

#if UNITY_EDITOR
        public static new void BuildHttpListener()
        {
            Debug.LogFormat(LOG_FORMAT, "BuildHttpListener()");

            if (_httpListener != null)
            {
                _httpListener.Abort();
                _httpListener = null;
            }
            _onComplete = null;

            string serverUrl = string.Format("http://127.0.0.1:{0}", _SpreadSheetManager.Config.PORT);

            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(serverUrl + "/");
            _httpListener.Start();
            _httpListener.BeginGetContext(new AsyncCallback(ListenerCallback), _httpListener);

            // _onComplete += GetAuthComplete;
            _onComplete += OnGetAuthComplete;

            string url = "https://accounts.google.com/o/oauth2/v2/auth?";
            url += "client_id=" + Uri.EscapeDataString(_SpreadSheetManager.Config.CLIENT_ID) + "&";
            url += "redirect_uri=" + Uri.EscapeDataString(serverUrl) + "&";
            url += "response_type=" + "code" + "&";
            url += "scope=" + Uri.EscapeDataString("https://www.googleapis.com/auth/spreadsheets") + "&";
            url += "access_type=" + "offline" + "&";
            url += "prompt=" + "consent" + "&";

            Debug.LogFormat(LOG_FORMAT, "Application.OpenURL(), url : <b>" + url + "</b>");

            Application.OpenURL(url);
        }

        // protected static void GetAuthComplete(string authToken)
        protected static void OnGetAuthComplete(string authToken)
        {
            Debug.LogFormat(LOG_FORMAT, "OnGetAuthComplete(), authToken : <b><color=yellow>" + authToken + "</color></b>");

            string serverUrl = string.Format("http://127.0.0.1:{0}", _SpreadSheetManager.Config.PORT);

            // Debug.LogFormat(LOG_FORMAT, authToken);
            Debug.LogFormat(LOG_FORMAT, "Auth Token = " + authToken);

            WWWForm f = new WWWForm();
            f.AddField("code", authToken);
            f.AddField("client_id", _SpreadSheetManager.Config.CLIENT_ID);
            f.AddField("client_secret", _SpreadSheetManager.Config.CLIENT_SECRET);
            f.AddField("redirect_uri", serverUrl);
            f.AddField("grant_type", "authorization_code");
            f.AddField("scope", "");

            GoogleSheetsToUnity.ThirdPary.EditorCoroutineRunner.StartCoroutine(GetToken(f));
        }

        protected static new IEnumerator GetToken(WWWForm f)
        {
            using (UnityWebRequest uwr = UnityWebRequest.Post("https://accounts.google.com/o/oauth2/token", f))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogErrorFormat(LOG_FORMAT, "uwr.result : " + uwr.result + ", Error While Sending: " + uwr.error);
                    if (uwr.error == "Malformed URL")
                    {
                        // Debug.LogFormat(LOG_FORMAT, "URL:" + uri);
                    }
                    // listener(uwr.error, null);
                }
                else
                {
                    // listener(uwr.error, uwr.downloadHandler);
                    _SpreadSheetManager.Config.gdr = JsonUtility.FromJson<GoogleDataResponse>(uwr.downloadHandler.text);
                    _SpreadSheetManager.Config.gdr.nextRefreshTime = DateTime.Now.AddSeconds(_SpreadSheetManager.Config.gdr.expires_in);

                    Debug.LogFormat(LOG_FORMAT, "--------------- END-Build Connection ------------------");
                    EditorUtility.SetDirty(_SpreadSheetManager.Config);
                    AssetDatabase.SaveAssets();
                }
            }
        }
#endif

        public static new IEnumerator CheckForRefreshOfToken()
        {
            if (DateTime.Now > _SpreadSheetManager.Config.gdr.nextRefreshTime)
            {
                Debug.LogFormat(LOG_FORMAT, "Refreshing Token");

                WWWForm f = new WWWForm();
                f.AddField("client_id", _SpreadSheetManager.Config.CLIENT_ID);
                f.AddField("client_secret", _SpreadSheetManager.Config.CLIENT_SECRET);
                f.AddField("refresh_token", _SpreadSheetManager.Config.gdr.refresh_token);
                f.AddField("grant_type", "refresh_token");
                f.AddField("scope", "");

                using (UnityWebRequest request = UnityWebRequest.Post("https://www.googleapis.com/oauth2/v4/token", f))
                {
                    yield return request.SendWebRequest();

                    GoogleDataResponse newGdr = JsonUtility.FromJson<GoogleDataResponse>(request.downloadHandler.text);
                    _SpreadSheetManager.Config.gdr.access_token = newGdr.access_token;
                    _SpreadSheetManager.Config.gdr.nextRefreshTime = DateTime.Now.AddSeconds(newGdr.expires_in);

#if UNITY_EDITOR
                    EditorUtility.SetDirty(_SpreadSheetManager.Config);
                    AssetDatabase.SaveAssets();
#endif
                }
            }
        }
    }
}
