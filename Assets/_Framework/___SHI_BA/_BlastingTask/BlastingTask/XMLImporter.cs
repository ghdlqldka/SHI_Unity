using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using TMPro;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using AOT; // MonoPInvokeCallback을 위해 필요
#endif
#if !UNITY_WEBGL || UNITY_EDITOR
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
#endif

// 서버에서 받아오는 XML 데이터 응답 클래스
[System.Serializable]
public class XMLWebSocketData
{
    public string XMLdata;
    public string timestamp;
    public string type;
}

public class XMLImporter : MonoBehaviour
{
    [Header("WebSocket 서버 설정")]
    public string webSocketServerUrl = "ws://192.168.0.83:5001/unity/motion_xml/digital_twin";

    [Header("WebGL 동적 URL 사용")]
    public bool useCurrentHostForWebGL = true;

    [Header("연결 설정")]
    public bool autoConnectOnStart = false; // 시작 시 자동 연결 비활성화
    public bool showConnectionButton = true; // UI 버튼 표시 여부

    [Header("인증/초기화 데이터")]
    [TextArea(3, 5)]
    public string initJsonData = "{}";

    [Header("UI 연결")]
    public TMP_Text Text_Twindata;
    public TMP_Text Text_ConnectionStatus;

    [Header("Make_XML 연결")]
    public Make_XML makeXml;

    [Header("재연결 설정")]
    public bool autoReconnect = false; // 자동 재연결 비활성화
    public float reconnectInterval = 5.0f;
    public int maxReconnectAttempts = 5;
    public float maxReconnectInterval = 30.0f;
    public bool exponentialBackoff = true;

    [Header("디버그")]
    public bool debugMode = true;

#if UNITY_WEBGL && !UNITY_EDITOR
    // WebGL용 JavaScript 함수 선언
   [DllImport("__Internal")]
    private static extern System.IntPtr GetDynamicWebSocketURL();
    

    [DllImport("__Internal")]
    private static extern void WebSocketSetCallbacks(
        Action<int> openCallback,
        Action<int, System.IntPtr> messageCallback,
        Action<int, System.IntPtr> errorCallback,
        Action<int, int> closeCallback
    );

    [DllImport("__Internal")]
    private static extern int WebSocketCreate(string url);

    [DllImport("__Internal")]
    private static extern int WebSocketSend(int id, string message);

    [DllImport("__Internal")]
    private static extern void WebSocketClose(int id);

    [DllImport("__Internal")]
    private static extern int WebSocketGetState(int id);

    // WebGL WebSocket 상태 (JavaScript WebSocket.readyState와 동일)
    private const int WS_CONNECTING = 0;
    private const int WS_OPEN = 1;
    private const int WS_CLOSING = 2;
    private const int WS_CLOSED = 3;

    private int webSocketId = -1;
#else
    // PC/모바일용 WebSocket 변수
    private ClientWebSocket webSocket;
    private CancellationTokenSource cancellationTokenSource;
    private bool isReceiving = false;
#endif

    // 공통 변수
    private string lastXmlData = "";
    private bool isConnecting = false;
    private int reconnectAttempts = 0;
    private Coroutine reconnectCoroutine;

    // 연결 상태
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Reconnecting,
        Error
    }
    private ConnectionState currentState = ConnectionState.Disconnected;

    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL에서 JavaScript 콜백 설정
        WebSocketSetCallbacks(OnWebSocketOpen, OnWebSocketMessage, OnWebSocketError, OnWebSocketClose);
#endif

        // 자동 연결을 선택적으로만 실행
        if (autoConnectOnStart)
        {
            ConnectToWebSocket();
        }
        else
        {
            if (debugMode) Debug.Log("[XMLImporter] 자동 연결이 비활성화되어 있습니다. 수동으로 연결하세요.");
        }

        UpdateConnectionStatusUI();
    }
    // JS에서 메시지 받을 때
    public void OnWebSocketMessageFromJS(string json)
    {
        Debug.Log(" JS에서 받은 메시지: " + json);
        // 이후 처리 (ex: JSON 파싱 → XML 변환 등)
    }
    // 메시지 보낼 때
 
    void OnDestroy()
    {
        DisconnectFromWebSocket();
    }

    void OnDisable()
    {
        DisconnectFromWebSocket();
    }

#if !UNITY_WEBGL || UNITY_EDITOR
    void Update()
    {
        // PC/모바일에서만 필요 (WebGL은 JavaScript 이벤트 기반)
    }
#endif

    #region WebSocket 연결 관리

    public async void ConnectToWebSocket()
    {
        if (isConnecting || IsWebSocketConnected())
        {
            if (debugMode) Debug.Log("[XMLImporter] 이미 연결되어 있거나 연결 중입니다.");
            return;
        }

        isConnecting = true;
        currentState = ConnectionState.Connecting;
        UpdateConnectionStatusUI();

        try
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL에서 동적 URL 생성 (location.hostname 사용)
            string finalUrl;
            if (useCurrentHostForWebGL)
            {
                System.IntPtr urlPtr = GetDynamicWebSocketURL();
                finalUrl = Marshal.PtrToStringUTF8(urlPtr);
                // JavaScript에서 할당한 메모리 해제
                Marshal.FreeHGlobal(urlPtr);
            }
            else
            {
                finalUrl = webSocketServerUrl;
            }
            
            if (debugMode) Debug.Log($"[XMLImporter] WebGL WebSocket 연결 시도: {finalUrl}");
            webSocketId = WebSocketCreate(finalUrl);
            if (webSocketId == -1)
            {
                throw new Exception("WebSocket 생성 실패");
            }
            // WebGL에서는 연결이 비동기적으로 처리되므로 OnWebSocketOpen 콜백을 기다림
#else
            // PC/모바일 WebSocket 연결
            if (webSocket != null)
            {
                Debug.Log("webSocket 존재중");
                webSocket.Dispose();
            }

            webSocket = new ClientWebSocket();
            cancellationTokenSource = new CancellationTokenSource();

            if (debugMode) Debug.Log($"[XMLImporter] WebSocket 연결 시도: {webSocketServerUrl}");

            await webSocket.ConnectAsync(new Uri(webSocketServerUrl), cancellationTokenSource.Token);

            if (webSocket.State == WebSocketState.Open)
            {
                OnWebSocketOpenHandler();
                StartReceiving();
            }
#endif
        }
        catch (Exception e)
        {
            Debug.LogError($"[XMLImporter] WebSocket 연결 실패: {e.Message}");
            currentState = ConnectionState.Error;
            UpdateConnectionStatusUI();
            isConnecting = false;

            if (autoReconnect)
            {
                StartReconnectCoroutine();
            }
        }
    }

    public async void DisconnectFromWebSocket()
    {
        if (reconnectCoroutine != null)
        {
            StopCoroutine(reconnectCoroutine);
            reconnectCoroutine = null;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL WebSocket 종료
        if (webSocketId != -1)
        {
            WebSocketClose(webSocketId);
            webSocketId = -1;
        }
#else
        // PC/모바일 WebSocket 종료
        isReceiving = false;

        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }

        if (webSocket != null)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnecting", CancellationToken.None);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[XMLImporter] WebSocket 종료 중 오류: {e.Message}");
                }
            }

            webSocket.Dispose();
            webSocket = null;
        }
#endif

        currentState = ConnectionState.Disconnected;
        UpdateConnectionStatusUI();
        isConnecting = false;
        reconnectAttempts = 0;

        if (debugMode) Debug.Log("[XMLImporter] WebSocket 연결 해제");
    }

    #endregion

    #region WebSocket 이벤트 처리

#if UNITY_WEBGL && !UNITY_EDITOR
    // WebGL 콜백 함수들 (JavaScript에서 호출됨)
    [MonoPInvokeCallback(typeof(Action<int>))]
    private static void OnWebSocketOpen(int id)
    {
        var instance = FindFirstObjectByType<XMLImporter>();
        if (instance != null)
        {
            instance.HandleWebSocketOpen();
        }
    }

    [MonoPInvokeCallback(typeof(Action<int, System.IntPtr>))]
    private static void OnWebSocketMessage(int id, System.IntPtr messagePtr)
    {
        var instance = FindFirstObjectByType<XMLImporter>();
        if (instance != null)
        {
            string message = Marshal.PtrToStringUTF8(messagePtr);
            instance.HandleWebSocketMessage(message);
        }
    }

    [MonoPInvokeCallback(typeof(Action<int, System.IntPtr>))]
    private static void OnWebSocketError(int id, System.IntPtr errorPtr)
    {
        var instance = FindFirstObjectByType<XMLImporter>();
        if (instance != null)
        {
            string error = Marshal.PtrToStringUTF8(errorPtr);
            instance.HandleWebSocketError(error);
        }
    }

    [MonoPInvokeCallback(typeof(Action<int, int>))]
    private static void OnWebSocketClose(int id, int closeCode)
    {
        var instance = FindFirstObjectByType<XMLImporter>();
        if (instance != null)
        {
            instance.HandleWebSocketClose(closeCode);
        }
    }

    private void HandleWebSocketOpen()
    {
        OnWebSocketOpenHandler();
    }

    private void HandleWebSocketMessage(string message)
    {
        OnWebSocketMessageHandler(message);
    }

    private void HandleWebSocketError(string error)
    {
        OnWebSocketErrorHandler(error);
    }

    private void HandleWebSocketClose(int closeCode)
    {
        OnWebSocketCloseHandler(closeCode);
    }
#endif

    private void OnWebSocketOpenHandler()
    {
        if (debugMode) Debug.Log("✅ [XMLImporter] WebSocket 연결 성공");

        currentState = ConnectionState.Connected;
        UpdateConnectionStatusUI();
        isConnecting = false;

        // 연결 성공 시 재연결 관련 변수 완전 리셋
        reconnectAttempts = 0;
        if (!autoReconnect) autoReconnect = true; // 자동 재연결 다시 활성화

        // 연결 후 초기화 데이터 전송
        if (!string.IsNullOrEmpty(initJsonData))
        {
            SendWebSocketMessage(initJsonData);
        }
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    private void OnWebSocketMessageHandler(string message)
#else
    private async void StartReceiving()
    {
        if (isReceiving) return;

        isReceiving = true;
        byte[] buffer = new byte[1024 * 4];

        try
        {
            while (webSocket != null && webSocket.State == WebSocketState.Open && !cancellationTokenSource.Token.IsCancellationRequested)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    OnWebSocketMessageHandler(message);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    OnWebSocketCloseHandler(result.CloseStatus);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            if (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                OnWebSocketErrorHandler(e.Message);
            }
        }
        finally
        {
            isReceiving = false;
        }
    }

    private void OnWebSocketMessageHandler(string message)
#endif
    {
        try
        {
            if (debugMode && !string.IsNullOrEmpty(message))
            {
                Debug.Log($"[XMLImporter] WebSocket 메시지 수신: {message.Substring(0, Mathf.Min(100, message.Length))}...");
            }

            ProcessReceivedMessage(message);
        }
        catch (Exception e)
        {
            Debug.LogError($"[XMLImporter] 메시지 처리 오류: {e.Message}");
        }
    }

    private void OnWebSocketErrorHandler(string error)
    {
        Debug.LogError($"❌ [XMLImporter] WebSocket 오류: {error}");

        currentState = ConnectionState.Error;
        UpdateConnectionStatusUI();
        isConnecting = false;

        if (autoReconnect)
        {
            StartReconnectCoroutine();
        }
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    private void OnWebSocketCloseHandler(int closeCode)
    {
        if (debugMode) Debug.Log($"[XMLImporter] WebSocket 연결 종료: {closeCode}");
        bool normalClose = closeCode == 1000; // WebSocket.CLOSE_NORMAL
#else
    private void OnWebSocketCloseHandler(WebSocketCloseStatus? closeStatus)
    {
        if (debugMode) Debug.Log($"[XMLImporter] WebSocket 연결 종료: {closeStatus}");
        bool normalClose = closeStatus == WebSocketCloseStatus.NormalClosure;
#endif

        currentState = ConnectionState.Disconnected;
        UpdateConnectionStatusUI();
        isConnecting = false;

        if (autoReconnect && !normalClose)
        {
            StartReconnectCoroutine();
        }
    }

    #endregion

    #region 메시지 처리

    private void ProcessReceivedMessage(string message)
    {
        bool parseSuccess = TryParseResponse(message, out string xmlData);

        if (parseSuccess && !string.IsNullOrEmpty(xmlData))
        {
            ProcessReceivedXML(xmlData);
            UpdateTMPText(xmlData);
        }
        else if (!parseSuccess)
        {
            Debug.LogError("[XMLImporter] 메시지 파싱 실패");
            Debug.LogError($"받은 메시지: {message}");
        }
    }

    private bool TryParseResponse(string response, out string xmlData)
    {
        xmlData = "";

        try
        {
            if (response.StartsWith("{"))
            {
                XMLWebSocketData responseData = JsonUtility.FromJson<XMLWebSocketData>(response);
                if (!string.IsNullOrEmpty(responseData.XMLdata))
                {
                    xmlData = responseData.XMLdata;
                    return true;
                }
            }
            else if (response.StartsWith("<"))
            {
                xmlData = response;
                return true;
            }
            else if (!string.IsNullOrEmpty(response))
            {
                xmlData = response;
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[XMLImporter] 응답 파싱 오류: {e.Message}");
            return false;
        }

        return false;
    }

    public void SendWebSocketMessage(string message)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (webSocketId != -1 && WebSocketGetState(webSocketId) == WS_OPEN)
        {
            int result = WebSocketSend(webSocketId, message);
            if (result == 1)
            {
                if (debugMode) Debug.Log($"[XMLImporter] 메시지 전송: {message}");
            }
            else
            {
                Debug.LogError("[XMLImporter] 메시지 전송 실패");
            }
        }   
        else
        {
            Debug.LogWarning("[XMLImporter] WebSocket이 연결되지 않았습니다.");
        }
#else
        if (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            Task.Run(async () =>
            {
                try
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    await webSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, CancellationToken.None);
                    if (debugMode) Debug.Log($"[XMLImporter] 메시지 전송: {message}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[XMLImporter] 메시지 전송 실패: {e.Message}");
                }
            });
        }
        else
        {
            Debug.LogWarning("[XMLImporter] WebSocket이 연결되지 않았습니다.");
        }
#endif
    }

    private void UpdateTMPText(string xmlData)
    {
        if (Text_Twindata != null)
        {
            if (lastXmlData != xmlData)
            {
                Text_Twindata.text = xmlData;
                lastXmlData = xmlData;

                if (debugMode)
                {
                    Debug.Log("[XMLImporter] TMP Text 업데이트 완료");
                }
            }
        }
        else
        {
            Debug.LogWarning("[XMLImporter] Text_Twindata가 연결되지 않았습니다!");
        }
    }

    private void ProcessReceivedXML(string xmlData)
    {
        if (debugMode)
        {
            Debug.Log("[XMLImporter] 받은 XML 데이터 처리 시작");
        }

        if (makeXml != null)
        {
            // makeXml.LoadXMLData(xmlData);
            if (debugMode)
            {
                Debug.Log("[XMLImporter] Make_XML에 데이터 전달 준비됨");
            }
        }

        if (debugMode)
        {
            Debug.Log("✅ [XMLImporter] XML 데이터 처리 완료");
        }
    }

    #endregion

    #region 재연결 관리

    private void StartReconnectCoroutine()
    {
        if (reconnectCoroutine != null)
        {
            StopCoroutine(reconnectCoroutine);
        }

        if (reconnectAttempts < maxReconnectAttempts)
        {
            reconnectCoroutine = StartCoroutine(ReconnectCoroutine());
        }
        else
        {
            Debug.LogError($"[XMLImporter] 최대 재연결 시도 횟수({maxReconnectAttempts})에 도달했습니다. 자동 재연결을 중단합니다.");
            currentState = ConnectionState.Error;
            UpdateConnectionStatusUI();

            // 자동 재연결 완전 비활성화
            autoReconnect = false;

            // 사용자에게 수동 재연결 안내
            if (debugMode) Debug.Log("[XMLImporter] 수동으로 재연결하려면 ForceReconnect()를 호출하세요.");
        }
    }

    private IEnumerator ReconnectCoroutine()
    {
        reconnectAttempts++;
        currentState = ConnectionState.Reconnecting;
        UpdateConnectionStatusUI();

        // 지수적 백오프: 시도할 때마다 대기 시간 증가
        float currentInterval = reconnectInterval;
        if (exponentialBackoff)
        {
            currentInterval = Mathf.Min(reconnectInterval * Mathf.Pow(2, reconnectAttempts - 1), maxReconnectInterval);
        }

        if (debugMode) Debug.Log($"[XMLImporter] 재연결 시도 {reconnectAttempts}/{maxReconnectAttempts} (대기: {currentInterval:F1}초)");

        yield return new WaitForSeconds(currentInterval);

        // 재연결 시도 전에 한 번 더 체크
        if (autoReconnect && reconnectAttempts <= maxReconnectAttempts)
        {
            ConnectToWebSocket();
        }
        else
        {
            if (debugMode) Debug.Log("[XMLImporter] 자동 재연결이 비활성화되었거나 최대 시도 횟수에 도달했습니다.");
            currentState = ConnectionState.Error;
            UpdateConnectionStatusUI();
        }

        reconnectCoroutine = null;
    }

    #endregion

    #region UI 업데이트

    private void UpdateConnectionStatusUI()
    {
        if (Text_ConnectionStatus != null)
        {
            string statusText = "";
            Color statusColor = Color.white;

            switch (currentState)
            {
                case ConnectionState.Disconnected:
                    statusText = autoConnectOnStart ? "연결 끊김" : "연결 대기 중 (수동 연결 필요)";
                    statusColor = Color.gray;
                    break;
                case ConnectionState.Connecting:
                    statusText = "연결 중...";
                    statusColor = Color.yellow;
                    break;
                case ConnectionState.Connected:
                    statusText = "실시간 연결됨";
                    statusColor = Color.green;
                    break;
                case ConnectionState.Reconnecting:
                    statusText = $"재연결 중... ({reconnectAttempts}/{maxReconnectAttempts})";
                    statusColor = new Color(1f, 0.5f, 0f); // 오렌지색
                    break;
                case ConnectionState.Error:
                    statusText = "연결 오류";
                    statusColor = Color.red;
                    break;
            }

            Text_ConnectionStatus.text = statusText;
            Text_ConnectionStatus.color = statusColor;
        }
    }

    #endregion

    #region 공개 메서드

    public ConnectionState GetConnectionState()
    {
        return currentState;
    }

    public void SetServerUrl(string newUrl)
    {
        webSocketServerUrl = newUrl;
        if (debugMode) Debug.Log($"[XMLImporter] WebSocket 서버 URL 변경: {newUrl}");
    }

    public string GetCurrentXMLData()
    {
        return lastXmlData;
    }

    public void ClearTextDisplay()
    {
        if (Text_Twindata != null)
        {
            Text_Twindata.text = "";
            lastXmlData = "";
            if (debugMode) Debug.Log("[XMLImporter] TMP Text 초기화");
        }
    }

    public void SetAutoReconnect(bool enable)
    {
        autoReconnect = enable;
        if (debugMode) Debug.Log($"[XMLImporter] 자동 재연결: {(enable ? "활성화" : "비활성화")}");
    }

    public void SetReconnectInterval(float interval)
    {
        reconnectInterval = Mathf.Max(1.0f, interval);
        if (debugMode) Debug.Log($"[XMLImporter] 재연결 간격 변경: {reconnectInterval}초");
    }

    public bool IsWebSocketConnected()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return webSocketId != -1 && WebSocketGetState(webSocketId) == WS_OPEN;
#else
        return webSocket != null && webSocket.State == WebSocketState.Open;
#endif
    }

    public void ForceReconnect()
    {
        if (debugMode) Debug.Log("[XMLImporter] 수동 재연결 시도");

        // 재연결 카운터 리셋하고 재연결 활성화
        reconnectAttempts = 0;
        autoReconnect = true;

        // 기존 연결 종료 후 새로 연결
        DisconnectFromWebSocket();
        StartCoroutine(DelayedReconnect());
    }

    private IEnumerator DelayedReconnect()
    {
        yield return new WaitForSeconds(1.0f); // 1초 대기 후 재연결
        ConnectToWebSocket();
    }

    // UI 버튼용 함수들
    public void OnConnectButtonClick()
    {
        if (IsWebSocketConnected())
        {
            DisconnectFromWebSocket();
        }
        else
        {
            ConnectToWebSocket();
        }
    }

    #endregion

    #region 컨텍스트 메뉴

    [ContextMenu("WebSocket 연결")]
    public void MenuConnect()
    {
        ConnectToWebSocket();
    }

    [ContextMenu("WebSocket 연결 해제")]
    public void MenuDisconnect()
    {
        DisconnectFromWebSocket();
    }

    [ContextMenu("강제 재연결 (카운터 리셋)")]
    public void MenuForceReconnect()
    {
        ForceReconnect();
    }

    [ContextMenu("테스트 메시지 전송")]
    public void MenuSendTestMessage()
    {
        SendWebSocketMessage("{\"type\":\"test\",\"message\":\"Hello from Unity\"}");
    }

    [ContextMenu("재연결 토글")]
    public void MenuToggleAutoReconnect()
    {
        SetAutoReconnect(!autoReconnect);
    }

    [ContextMenu("연결 상태 확인")]
    public void MenuCheckConnectionState()
    {
        Debug.Log($"[XMLImporter] 현재 상태: {currentState}");
#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log($"WebGL WebSocket ID: {webSocketId}");
        Debug.Log($"WebSocket 상태: {(webSocketId != -1 ? WebSocketGetState(webSocketId).ToString() : "없음")}");
#else
        Debug.Log($"WebSocket 상태: {(webSocket != null ? webSocket.State.ToString() : "null")}");
#endif
        Debug.Log($"자동 연결: {autoConnectOnStart}");
        Debug.Log($"자동 재연결: {autoReconnect}");
        Debug.Log($"재연결 시도: {reconnectAttempts}/{maxReconnectAttempts}");
    }

    #endregion
}