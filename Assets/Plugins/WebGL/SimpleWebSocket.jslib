// WebSocketPlugin.jslib
// Unity Assets/Plugins/WebGL/ 폴더에 저장하세요

mergeInto(LibraryManager.library, {
  $WebSocketManager: {
    instances: {},
    nextId: 1,
    openCallback: null,
    messageCallback: null,
    errorCallback: null,
    closeCallback: null
  },

  WebSocketSetCallbacks: function(openCallback, messageCallback, errorCallback, closeCallback) {
    WebSocketManager.openCallback = openCallback;
    WebSocketManager.messageCallback = messageCallback;
    WebSocketManager.errorCallback = errorCallback;
    WebSocketManager.closeCallback = closeCallback;
  },

  WebSocketSetCallbacks__deps: ['$WebSocketManager'],

  // 동적 URL 생성 함수 추가 (HTTPS/WSS 자동 감지)
  GetDynamicWebSocketURL: function() {
    var protocol = location.protocol === 'https:' ? 'wss://' : 'ws://';
    var ws_url = protocol + location.hostname + ":5001/unity/motion_xml/digital_twin";
    var bufferSize = lengthBytesUTF8(ws_url) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(ws_url, buffer, bufferSize);
    return buffer;
  },

  GetDynamicWebSocketURL__deps: ['$WebSocketManager'],

  WebSocketCreate: function(url) {
    var urlStr = UTF8ToString(url);
    var id = WebSocketManager.nextId++;
    
    try {
      var ws = new WebSocket(urlStr);
      WebSocketManager.instances[id] = ws;
      
      ws.onopen = function() {
        if (WebSocketManager.openCallback) {
          {{{ makeDynCall('vi', 'WebSocketManager.openCallback') }}}(id);
        }
      };
      
      ws.onmessage = function(e) {
        if (WebSocketManager.messageCallback) {
          var len = lengthBytesUTF8(e.data) + 1;
          var ptr = _malloc(len);
          stringToUTF8(e.data, ptr, len);
          {{{ makeDynCall('vii', 'WebSocketManager.messageCallback') }}}(id, ptr);
          _free(ptr);
        }
      };
      
      ws.onerror = function() {
        if (WebSocketManager.errorCallback) {
          var msg = "WebSocket Error";
          var len = lengthBytesUTF8(msg) + 1;
          var ptr = _malloc(len);
          stringToUTF8(msg, ptr, len);
          {{{ makeDynCall('vii', 'WebSocketManager.errorCallback') }}}(id, ptr);
          _free(ptr);
        }
      };
      
      ws.onclose = function(e) {
        if (WebSocketManager.closeCallback) {
          {{{ makeDynCall('vii', 'WebSocketManager.closeCallback') }}}(id, e.code);
        }
        delete WebSocketManager.instances[id];
      };
      
      return id;
    } catch (e) {
      console.error("WebSocket creation failed:", e);
      return -1;
    }
  },

  WebSocketCreate__deps: ['$WebSocketManager'],

  WebSocketSend: function(id, ptr) {
    var ws = WebSocketManager.instances[id];
    if (ws && ws.readyState === 1) {
      ws.send(UTF8ToString(ptr));
      return 1;
    }
    return 0;
  },

  WebSocketSend__deps: ['$WebSocketManager'],

  WebSocketClose: function(id) {
    var ws = WebSocketManager.instances[id];
    if (ws) {
      ws.close();
      delete WebSocketManager.instances[id];
    }
  },

  WebSocketClose__deps: ['$WebSocketManager'],

  WebSocketGetState: function(id) {
    var ws = WebSocketManager.instances[id];
    return ws ? ws.readyState : -1;
  },

  WebSocketGetState__deps: ['$WebSocketManager']
});