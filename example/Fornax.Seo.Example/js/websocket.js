(function () {
    const wsUri = "ws://localhost:8080/websocket";
    function init() {
        websocket = new WebSocket(wsUri);
        websocket.onclose = function (evt) { onClose(evt) };
    }
    function onClose(evt) {
        console.log('closing');
        websocket.close();
        document.location.reload();
    }
    window.addEventListener("load", init, false);
})();
