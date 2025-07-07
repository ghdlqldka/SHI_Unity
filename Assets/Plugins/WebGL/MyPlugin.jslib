var socket = null;
var shouldReceive = false; // 기본적으로 수신 안 함

mergeInto(LibraryManager.library, {
  SocketIO_Init: function () {
    if (typeof io === 'undefined') {
      console.error("Socket.IO가 로드되지 않았습니다.");
      return;
    }

    socket = io();

    socket.on('/unity/motion_xml/digital_twin', function (data) {
      if (!shouldReceive) return;
      console.log("받은 데이터:", data);
      SendMessage('XML_Importer', 'OnSocketMessage', JSON.stringify(data));
    });

    socket.on('connect', function () {
      SendMessage('XML_Importer', 'OnSocketConnected', 'connected');
    });
  },

  SocketIO_SendMessage: function (msgPtr) {
    var msg = UTF8ToString(msgPtr);
    console.log("Unity에서 보낸 메시지:", msg);

    if (socket) {
      socket.emit('from_unity', msg);
    } else {
      console.warn("Socket이 아직 초기화되지 않았습니다.");
    }
  },

  SocketIO_SetReceive: function (val) {
    shouldReceive = !!val; // 0이면 false, 1이면 true
    console.log("수신 상태:", shouldReceive);
  }
});