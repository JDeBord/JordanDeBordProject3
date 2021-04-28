'use strict';
// Some portions of this was provided by (and modified from) Dr. Roach's Labs

// Self calling function when the page loads. Sets up our event listensers and
//  connection. 
(function _groceryListEdit() {
    console.log("Edit")
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/groceryListHub")
        .build();

    // Notification event listener.
    connection.on("Notification", (message) => {
        var incoming = JSON.parse(message);
        console.log(incoming)

            // If the user lost access to the list, force reload the page, kicking them
            //  to the no access page.
        if (incoming.type === "ACCESS-REVOKED") {
            let access = $(`#permission-${incoming.data}`);
            if (access.length > 0) {
                location.reload();
            }
        }
            // If an item was added, update the list of items if it is for this list.
            // Data is the item Id and Data2 is the list id.
        else if (incoming.type === "ITEM-ADDED") {
            _updateGroceryListTable(incoming.data, incoming.data2);
        }
            // If the list name was changed, update the name. Data is the list id and data2 is the list name.
        else if (incoming.type === "LIST-UPDATED") {
            _updateListName(incoming.data, incoming.data2);
        }
            // If an item was added, remove it from the list if it is for this list.
            // Data is the item id and data2 is the list id.
        else if (incoming.type === "ITEM-REMOVED") {
            _removeItemRow(incoming.data, incoming.data2);
        }
            // If the list was deleted, force reload the page if it was this list. This
            //  will kick the user back home.
        else if (incoming.type === "LIST-DELETED") {
            let access = $(`#edit-list-${incoming.data}`);
            if (access.length > 0) {
                location.reload();
            }
        }
    });

    // Start connection and catch errors.
    connection.start().catch((err) => {
        return console.error(err.toString());
    });

    // Set up the popovers for the delete item buttons
    _setupPopovers();

    // EVENT LISTENERS FROM PAGE.
    const createGroceryItemForm = document.querySelector("#createGroceryItemForm");


    // If the user clicks the close button on the alert area, hide it.
    $('#alertCloseBtn').on('click', function _hideAlert() {
        $('#alertArea').hide(400);
    });

    // If the user submits the new item, submit it with Ajax.
    createGroceryItemForm.addEventListener('submit', (e) => {
        e.preventDefault();
        _clearErrorMessages();
        _submitItemWithAjax();
    });

    // If the owner sends name change update, prevent the default and send it with Ajax.
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

    // Prevent default action on the delete buttons.
    $(document).on('click', '.removeAjax', (e) => {
        e.preventDefault();
    });


    // AJAX ACTIONS

    // Function to update the name of the list using Ajax. We then take appropriate action
    //  based upon the response. 
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
                    _notifyConnectedClientsTwoParts("LIST-UPDATED", result.id, result.name);
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

    // Function to send an Ajax request to create an item. We then take appropriate action based
    //  upon the response. 
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

    // Function to send a request to delete an Item with Ajax. We then take appropriate action
    //  based upon the response. 
    function _sendRemoveItemAjax(url, id) {
        fetch(url, {
            method: "post",
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `id=${id}`
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('There was a network error!');
                }
                return response.json();
            })
            .then(result => {
                if (result?.message === "item-removed") {
                    console.log('Success: the item was removed');
                    _notifyConnectedClientsTwoParts("ITEM-REMOVED", result.id, result.listId);
                }
                else if (result?.message === "not-valid") {
                    $('#messageArea').html("The Item or List no longer exists!");
                    $('#alertArea').show(400);
                }
                else {
                    _reportErrors(result);
                }
            })
            .catch(error => {
                console.error('Error:', error);
            });
    }

    // OTHER METHODS/FUNCTIONS

    // If we get a notification that the name has changed, update the name on the page.
    function _updateListName(listId, newName) {
        let data = $(`#ListName-${listId}`);
        if (data.length > 0) {
            $(`#ListName-${listId}`).val(newName);
        }
    }

    // If we get a notification that an item has been added, we get the row for that item.
    //  If this item is not for this result, our append will not fire, if it is we add it to the list.
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
                _setupPopovers();
            })
            .catch(error => {
                console.error('Error:', error);
            });
    }

    // If an item has been deleted, see if it exists in the table. If so, hide it
    //  then replace it with an empty string to remove it.
    function _removeItemRow(rowId, listId) {
        let rowToDelete = document.querySelector(`#edit-row-${rowId}`);

        if (rowToDelete != null)
        {
            $(`#edit-row-${rowId}`).hide(400, () => {
                rowToDelete.replaceWith("");
            })
        }
    }


    // Function to clear error messages.
    function _clearErrorMessages() {
        $.each($('span[data-valmsg-for]'), function _clearSpan() {
            $(this).html("");
        });
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

    // Function to set up popovers
    function _setupPopovers() {
        $('[data-toggle="popover"]').popover();
        $('.popover-dismiss').popover({
            trigger: 'focus'
        });

        $('[data-toggle="popover"]').on('inserted.bs.popover', function _onPopoverInserted() {
            let $a = $(this);
            let url = $a.attr('href');
            let idx = url.lastIndexOf('/');
            let id = url.substring(idx + 1);
            url = url.substring(0, idx);
            let btnYesId = "btnYes-" + id;
            $('.popover-body').html(`<button id="${btnYesId}">Yes</button><button>No</button>`);
            $(`#${btnYesId}`).on('click', function _onYes() {
                console.log(url);
                console.log(id);
                _sendRemoveItemAjax(url, id);
            });
        });
    }

    // Function to send notification to clients. 
    //  for name changes, data is the listId and data2 is the name, for items data is itemId and 
    //  data2 is listId.
    function _notifyConnectedClientsTwoParts(type, data, data2) {
        let message = {
            type, data, data2
        };
        console.log(JSON.stringify(message));
        connection.invoke("SendMessageToAllAsync", JSON.stringify(message))
            .catch(function (err) {
                return console.error(err.toString());
            });
    }
})();