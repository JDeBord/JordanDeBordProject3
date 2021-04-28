'use strict';
(function _homeIndexMain() {
    console.log("Home")
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/groceryListHub")
        .build();

    // Notification event listener.
    connection.on("Notification", (message) => {
        var incoming = JSON.parse(message);
        console.log(incoming);

        if (incoming.type === "LIST-CREATED") {
            _updateListTable(incoming.data);
        }
        else if (incoming.type === "PERMISSION-GRANTED") {
            _updateTablePermission(incoming.data, incoming.data2);
        }
        else if (incoming.type === "ACCESS-REVOKED") {
            _removeRow(incoming.data);
        }
        else if (incoming.type === "LIST-UPDATED") {

        }
        else if (incoming.type === "ITEM-REMOVED" || incoming.type === "ITEM-ADDED")
        {

        }
    });

    // Start connection and catch errors.
    connection.start().catch((err) => {
        return console.error(err.toString());
    });


    // EVENT LISTENERS FROM PAGE.
    const createGroceryListForm = document.querySelector("#createGroceryListForm");

    // If the user clicks the close button on the alert area, hide it.
    $('#alertCloseBtn').on('click', function _hideAlert() {
        $('#alertArea').hide(400);
    });

    // If the user clicks the cancel button in the create modal, clear the data and hide it.
    $('#createCancelBtn').on('click', (e) => {
        e.preventDefault();
        $('input').val("");
        $('#createGroceryListModal').modal('hide');
    })

    // If the user submits the name, verify and submit it with Ajax.
    createGroceryListForm.addEventListener('submit', (e) => {
        e.preventDefault();
        _clearErrorMessages();
        _submitWithAjax();
    });



    // AJAX ACTIONS
    function _submitWithAjax() {
        const url = createGroceryListForm.getAttribute('action') + "ajax";
        const method = createGroceryListForm.getAttribute('method');
        const formData = new FormData(createGroceryListForm);

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
                if (result?.message === "created") {
                    $('#createGroceryListModal').modal('hide');
                    _notifyConnectedClients("LIST-CREATED", result.id);
                    $('#messageArea').html("A new grocery list was created!");
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

    // Function to remove the row after Deletion or Access Revocation.
    function _removeRow(accessId) {
        let rowToDelete = document.querySelector(`#index-row-${accessId}`);

        if (rowToDelete != null) {
            $(`#index-row-${accessId}`).hide(400, () => {
                rowToDelete.replaceWith("");
            })
        }
    }

    // Update the table if the row doesn't already exist.
    // This prevents repeat updates if multiple people are granted
    //      Access to the same list.
    function _updateTablePermission(accessId, listId) {
        let len = $(`#index-row-${accessId}`).length;
        if (len > 0) {
            // element exists so no action.
        }
        else {
            _updateListTable(listId);
        }

    }

    // Function to add grocery list to index page.
    function _updateListTable(listId) {
        fetch(`/grocerylist/listrow/${listId}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('There was a network error!');
                }
                return response.text();
            })
            .then(result => {
                $('#table-home-index').append(result);
            })
            .catch(error => {
                console.error('Error:', error);
            });
    }


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