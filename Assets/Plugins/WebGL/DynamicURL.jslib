mergeInto(LibraryManager.library, {
  GetWebSocketURL: function() {
    var ws_url = "ws://" + location.hostname + ":5001/unity/motion_xml/digital_twin";
    var bufferSize = lengthBytesUTF8(ws_url) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(ws_url, buffer, bufferSize);
    return buffer;
  }
});