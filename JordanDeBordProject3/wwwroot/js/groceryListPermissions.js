'use strict';
(function _groceryListPermissions() {
    console.log("Permissions")
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
        else if (incoming.type === "PERMISSION-GRANTED") {
            
        }
    });

    // Start connection and catch errors.
    connection.start().catch((err) => {
        return console.error(err.toString());
    });

    // EVENT LISTENERS FROM PAGE.
    const grantPermissionForm = document.querySelector("#grantPermissionForm");

    $('#alertCloseBtn').on('click', function _hideAlert() {
        $('#alertArea').hide(400);
    });

    // If the user submits the name, verify and submit it with Ajax.
    grantPermissionForm.addEventListener('submit', (e) => {
        e.preventDefault();
        _clearErrorMessages();
        _submitPermissionWithAjax();
    });


    // AJAX ACTIONS
    function _submitPermissionWithAjax() {
        const url = grantPermissionForm.getAttribute('action') + "ajax";
        const method = grantPermissionForm.getAttribute('method');
        const formData = new FormData(grantPermissionForm);

        fetch(url, {
            method: method,
            body: formData
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('There was a network error!');
                }
                return response.json();
            })
            .then(result => {
                if (result?.message === "granted-permission") {
                    $('#createGroceryListModal').modal('hide');
                    _notifyConnectedClientsTwoParts("PERMISSION-GRANTED", result.id, result.listId );
                    $('#messageArea').html("A user was granted access!");
                    $('#alertArea').show(400);
                    location.reload();
                }
                else if (result?.message === "invalid-permission") {
                    $('#createGroceryListModal').modal('hide');
                    $('#messageArea').html("Invalid Email!");
                    $('#alertArea').show(400);
                } 
                else {
                    _reportErrors(result);
                }
            })
            .catch(error => {
                console.error('Error:', error);
            })
    }

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
    function _notifyConnectedClientsTwoParts(type, data, otherId) {
        let message = {
            type, data, otherId
        };
        console.log(JSON.stringify(message));
        connection.invoke("SendMessageToAllAsync", JSON.stringify(message))
            .catch(function (err) {
                return console.error(err.toString());
            });
    }
})();