'use strict';
// Some portions of this was provided by (and modified from) Dr. Roach's Labs

// Self calling function when the page loads. Sets up our event listensers and
//  connection. 
(function _groceryListGoShopping() {
    console.log("Shopping")
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/groceryListHub")
        .build();

    // Notification event listener.
    connection.on("Notification", (message) => {
        var incoming = JSON.parse(message);
        console.log(incoming)

        // If an item is checked or unchecked, update the row.
        if (incoming.type === "ITEM-CHECKED") {
            _updateCheckedRow(incoming.data);
        }
        else if (incoming.type === "ITEM-UNCHECKED") {
            _updateCheckedRow(incoming.data);
        }
            // If the access for a user is revoked, force a reload.
        else if (incoming.type === "ACCESS-REVOKED") {
            let access = $(`#permission-${incoming.data}`);
            if (access.length > 0) {
                location.reload();
            }
        }
            // If the list is updated, replace the name.
        else if (incoming.type === "LIST-UPDATED") {
            _updateListName(incoming.data, incoming.data2);
        }
            // If an item is added or removed, update the table.
        else if (incoming.type === "ITEM-ADDED") {
            _updateShoppingTable(incoming.data, incoming.data2);
        }
        else if (incoming.type === "ITEM-REMOVED") {
            _removeShoppingRow(incoming.data)
        }
            // If the list is deleted, force a reload.
        else if (incoming.type === "LIST-DELETED")
        {
            let access = $(`#shop-table-${incoming.data}`);
            if (access.length > 0) {
                location.reload();
            }
        }

    });

    // Start connection and catch errors.
    connection.start().catch((err) => {
        return console.error(err.toString());
    });

    // EVENT LISTENERS FROM PAGE.

    // Set up listeners on each of the checkboxes. Anytime a box is checked or unchecked
    //  we send an ajax notification with ajax to either set the item to shopped (checked) or not.
    $("input[type=checkbox]").change(function () {
        let $box = $(this); // Get the checkbox
        let boxId = $box.attr('id');
        let idx = boxId.lastIndexOf('-');
        let id = boxId.substring(idx + 1);
        if (this.checked) {
            // If checked, send check
            _sendCheckWithAjax("/grocerylist/checkAjax", id);
        }
        else {
            // if unchecked, send uncheck
            _sendUnCheckWithAjax("/grocerylist/uncheckAjax", id);
        }
    })


    // AJAX ACTIONS AND OTHER FUNCTIONS

    // Function to send ajax request to update item in database to show it was shopped, and to
    //  send a notification to all other shoppers for that list, so it automatically updates to checked.
    function _sendCheckWithAjax(url, id){
        let formData = new FormData();
        formData.append("Id", id);
        formData.append("Shopped", true);

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
                if (result?.message === "checked") {
                    console.log('Success: the item was checked.');
                    _notifyConnectedClients("ITEM-CHECKED", result.id);
                }
                else if (result?.message === "no-item") {
                    console.log('Item no longer exists.')
                }
                else {
                    _reportErrors(result);
                }
            })
            .catch(error => {
                console.error('Error:', error);
            });
    }

    // Function to send ajax request to update item in database to show it was not shopped, and to
    //  send a notification to all other shoppers for that list, so it automatically updates to unchecked.
    function _sendUnCheckWithAjax(url, id) {
        let formData = new FormData();
        formData.append("Id", id);
        formData.append("Shopped", false);

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
                if (result?.message === "unchecked") {
                    console.log('Success: Item unchecked');
                    _notifyConnectedClients("ITEM-UNCHECKED", result.id);
                }
                else if (result?.message === "no-item") {
                    console.log('Item no longer exists.')
                }
                else {
                    _reportErrors(result);
                }
            })
            .catch(error => {
                console.error('Error:', error);
            });
    }


    // If we get a notification that the list name has changed, update it.
    // This will only update the name if we are on the Go Shopping page for 
    // the list that has changed. 
    function _updateListName(listId, newName) {
        $(`#shop-title-${listId}`).html(newName);
    }

    // If we get a notification that an item is checked or unchecked, we send a fetch to
    //  get the row with the appropriate checked/unchecked box and slashed out name or not.
    //  We then reset the event listeners.
    function _updateCheckedRow(id) {
        fetch(`/grocerylist/shoprow/${id}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('There was a network error!');
                }
                return response.text();
            })
            .then(result => {
                let row = $(`#shop-row-${id}`); // If the row with the id exists, replace it.
                if (row != null)
                {
                    $(`#shop-row-${id}`).html(result);
                    // Reset event listener
                    $(`input[type=checkbox]`).off();
                    $(`input[type=checkbox]`).change(function () {
                        let $box = $(this); // Get the checkbox
                        let boxId = $box.attr('id');
                        let idx = boxId.lastIndexOf('-');
                        let id = boxId.substring(idx + 1);
                        if (this.checked) {
                            // If checked, send check
                            _sendCheckWithAjax("/grocerylist/checkAjax", id);

                        }
                        else {
                            // if unchecked, send uncheck
                            _sendUnCheckWithAjax("/grocerylist/uncheckAjax", id);
                        }
                    })  
                }
            })
            .catch(error => {
                console.error('Error:', error);
            });
    }

    // If we get a response that a new item was created, we get the row.
    //  If the row isn't for this table, we don't append, but if so we do. 
    //  We then reset event listeners.
    function _updateShoppingTable(rowId, listId) {
        fetch(`/grocerylist/addshoprow/${rowId}/${listId}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('There was a network error!');
                }
                return response.text();
            })
            .then(result => {
                if (result != "")
                {
                    $(`#shop-table-${listId}`).append(result);
                    // Reset event listener
                    $(`input[type=checkbox]`).off();
                    $(`input[type=checkbox]`).change(function () {
                        let $box = $(this); // Get the checkbox
                        let boxId = $box.attr('id');
                        let idx = boxId.lastIndexOf('-');
                        let id = boxId.substring(idx + 1);
                        if (this.checked) {
                            // If checked, send check
                            _sendCheckWithAjax("/grocerylist/checkAjax", id);

                        }
                        else {
                            // if unchecked, send uncheck
                            _sendUnCheckWithAjax("/grocerylist/uncheckAjax", id);
                        }
                    })
                }
                else {
                    console.log('Invalid item or list.');
                }
            })
            .catch(error => {
                console.error('Error:', error);
            });
    }

    // If an item was deleted, and it was for this list, we hide it then remove it.
    function _removeShoppingRow(itemId) {
        let rowToDelete = document.querySelector(`#shop-row-${itemId}`);

        if (rowToDelete !== null) {
            $(`#shop-row-${itemId}`).hide(400, () => {
                rowToDelete.replaceWith("");
            })
        }
    }

    // Function to report errors to users.
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

    // Function to send notification to clients. We send the message of if the item was
    //  checked or unchecked, as well as the item Id.
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