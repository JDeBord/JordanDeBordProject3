'use strict';
(function _groceryListGoShopping() {
    console.log("Shopping")
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/groceryListHub")
        .build();

    // Notification event listener.
    connection.on("Notification", (message) => {
        var incoming = JSON.parse(message);

        // Log message
        console.log(incoming);

        if (incoming.type === "LIST-CREATED") {
            
        }
    });

    // Start connection and catch errors.
    connection.start().catch((err) => {
        return console.error(err.toString());
    });

    // EVENT LISTENERS FROM PAGE.

    // AJAX ACTIONS


    // OTHER METHODS/FUNCTIONS

    // Function to clear error messages.
    function _clearErrorMessages() {
        $.each($('span[data-valmsg-for]'), function _clearSpan() {
            $(this).html("");
        });
    }

    // Function to report errors.
    function _reportErrors(response) {
        for (let key in response) {
            if (response[key].errors.length > 0) {
                for (let error of response[key].errors) {
                    console.log(key + " : " + error.errorMessage);
                    const selector = `span[data-valmsg-for="${key}"]`
                    const errMessageSpan = document.querySelector(selector);
                    if (errMessageSpan !== null) {
                        errMessageSpan.textContent = error.errorMessage;
                    }
                }
            }
        }
    }

    // Function to send notification to clients. 
    function _notifyConnectedClients(type, data) {
        let message = {
            type, data
        };
        console.log(JSON.stringify(message));
        connection.invoke("SendMessageToAllAsync", JSON.stringify(message))
            .catch(function (err) {
                return console.error(err.toString());
            });
    }
})();