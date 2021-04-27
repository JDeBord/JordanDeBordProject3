'use strict';
(function _groceryListEdit() {
    console.log("Edit")
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
        else if (incoming.type === "ITEM-ADDED") {
            _updateGroceryListTable(incoming.data, incoming.otherId);
        }
        else if (incoming.type === "LIST-UPDATED") {

        }
    });

    // Start connection and catch errors.
    connection.start().catch((err) => {
        return console.error(err.toString());
    });

    // EVENT LISTENERS FROM PAGE.
    const createGroceryItemForm = document.querySelector("#createGroceryItemForm");
    const editGroceryListForm = document.querySelector("#editGroceryListForm");

    // If the user clicks the close button on the alert area, hide it.
    $('#alertCloseBtn').on('click', function _hideAlert() {
        $('#alertArea').hide(400);
    });

    // If the user submits the new item, verify and submit it with Ajax.
    createGroceryItemForm.addEventListener('submit', (e) => {
        e.preventDefault();
        _clearErrorMessages();
        _submitItemWithAjax();
    });

    // If the owner sends name change update.
    let btnSave = document.querySelector('#btnSave');
    if (btnSave != null)
    {
        btnSave.addEventListener('click', (e) => {
            e.preventDefault();
            let id = $(`#ListIdField`).val();
            let name = $(`#ListNameField`).val();
            let list = { id, name };

            _updateListNameWithAjax("/grocerylist/updateAjax", list);
        })
    }


    // AJAX ACTIONS
    function _updateListNameWithAjax(url, list) {
        let formData = new FormData();
        formData.append("Id", list.id);
        formData.append("ListName", list.name);

        fetch(url, {
            method: "post",
            body: formData
        })
            .then(response => {
            if (!response.ok) {
                throw new Error('There was a network error!');
            }
            return response.json();
        })
            .then(result => {
                if (result?.message === "updated-list") {
                    _notifyConnectedClients("LIST-UPDATED", result.id);
                    $('#messageArea').html("Grocery List name updated!");
                    $('#alertArea').show(400);
                }
                else if (result?.message === "invalid-list") {
                    $('#messageArea').html("List no longer exists!");
                    $('#alertArea').show(400);
                }
                else if (result?.message === "invalid-name")
                {
                    $('#messageArea').html("Error! The List must have a Name between 1 and 50 characters long!");
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

    function _submitItemWithAjax() {
        const url = createGroceryItemForm.getAttribute('action') + "ajax";
        const method = createGroceryItemForm.getAttribute('method');
        const formData = new FormData(createGroceryItemForm);

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
                if (result?.message === "added-item") {
                    $('#createGroceryItemModal').modal('hide');
                    _notifyConnectedClientsTwoParts("ITEM-ADDED", result.id, result.listId);
                    $('#messageArea').html("A new grocery item was added!");
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

    function _updateGroceryListTable(itemId, listId) {
        fetch(`/grocerylist/listitemrow/${itemId}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('There was a network error!');
                }
                return response.text();
            })
            .then(result => {
                $(`#edit-list-${listId}`).append(result);
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