function requestInterceptorCallback(request) {
    console.log(request);
    if (request.url.indexOf('/token') > -1) {
        request.responseInterceptor = function(res) {
            if (res.ok && res.obj.token) {
               // swaggerUi.authActions.authorize({ Bearer: { name: 'Bearer', value: 'Bearer ' + res.obj.token }});
            }
        }
    }
}

function appendItem(message) {
    console.log(message)
}

var wsconnection;
function authSwaggerCustom(token) {

    if (wsconnection) {
        wsconnection.onDisconnected = undefined;
        wsconnection.socket.close();
    }

    wsconnection = new WebSocketManager((location.protocol == "https:" ? "wss://" : "ws://") + location.host + "/game?token=" + token);

    // optional.
    // called when the connection has been established together with your id.
    wsconnection.onConnected = (id) => {
        appendItem("-> You are now connected! Connection ID: " + wsconnection.id);
    }
    // optional.
    // called when the connection to the server has been lost.
    wsconnection.onDisconnected = () => {
        appendItem("-> Disconnected!");
    };

    // here we register a method with two arguments that can be called by the server.
    wsconnection.methods.matchRequest = function (request) {
        console.log('matchRequest:');
        console.log(request);
    };

    // here we register a method with two arguments that can be called by the server.
    wsconnection.methods.matchConfirmed = function (request) {
        console.log('matchConfirmed:');
        console.log(request);
    };
    
    wsconnection.connect();
}
