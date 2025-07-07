// Assets/Plugins/WebGL/WebGLHostname.jslib
// WebGL에서 JavaScript의 location 정보를 Unity로 전달하는 플러그인

mergeInto(LibraryManager.library, {
    
    GetHostname: function () {
        var hostname = window.location.hostname;
        var bufferSize = lengthBytesUTF8(hostname) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(hostname, buffer, bufferSize);
        return buffer;
    },
    
    GetProtocol: function () {
        var protocol = window.location.protocol;
        var bufferSize = lengthBytesUTF8(protocol) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(protocol, buffer, bufferSize);
        return buffer;
    },
    
    GetPort: function () {
        var port = window.location.port;
        var bufferSize = lengthBytesUTF8(port) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(port, buffer, bufferSize);
        return buffer;
    },
    
    GetFullURL: function () {
        var fullURL = window.location.href;
        var bufferSize = lengthBytesUTF8(fullURL) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(fullURL, buffer, bufferSize);
        return buffer;
    },
    
    GetOrigin: function () {
        var origin = window.location.origin;
        var bufferSize = lengthBytesUTF8(origin) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(origin, buffer, bufferSize);
        return buffer;
    }
    
});