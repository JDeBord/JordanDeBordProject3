'use strict';
// Some portions of this was provided by (and modified from) Dr. Roach's Labs

// Self calling function when the page loads. Sets up our event listensers and
//  connection. 
(function _homeIndexMain() {
    console.log("Home")
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/groceryListHub")
        .build();

    // Notification event listener. Takes action based upon the incoming message
    //  and the action corresponds to what is needed on this page.
    connection.on("Notification", (message) => {
        var incoming = JSON.parse(message);
        console.log(incoming);

        // If a list was created, add it to the table if the user has access.
        if (incoming.type === "LIST-CREATED") {
            _updateListTable(incoming.data);
        }

        // If the row doesn't exist, update the Table if needed.
        else if (incoming.type === "PERMISSION-GRANTED") {
            let rowInfo = $(`#index-row-${incoming.data}`);

            if (!(rowInfo.length > 0)) {
                _updateTablePermission(incoming.data, incoming.data2);
            }

        }
        // Check if the user's access for a list was revoked.
        else if (incoming.type === "ACCESS-REVOKED") {
            _removeRow(incoming.data);
        }
            // Check if a list on this page was deleted.
        else if (incoming.type === "LIST-DELETED") {
            _removeRowOnDelete(incoming.data);
        }
            // If an item was added or removed from a list, update the list in the table.
        else if (incoming.type === "ITEM-REMOVED" || incoming.type === "ITEM-ADDED") {
            _updateRow(incoming.data2);
        }
            // If a list name was changed, update the list in the table.
        else if (incoming.type === "LIST-UPDATED")
        {
            _updateRow(incoming.data);
        }
    });

    // Start connection and catch errors.
    connection.start().catch((err) => {
        return console.error(err.toString());
    });

    // Call setupPopovers to set them up for the delete buttons.
    _setupPopovers();

    // EVENT LISTENERS FROM PAGE.
    const createGroceryListForm = document.querySelector("#createGroceryListForm");

    // If the user clicks on the delete button, prevent it from submitting.
    $(document).on('click', '.deleteAjax', (e) => {
        e.preventDefault();
    });


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

    // If the user submits the name, clear error messages and and submit it with Ajax.
    createGroceryListForm.addEventListener('submit', (e) => {
        e.preventDefault();
        _clearErrorMessages();
        _submitWithAjax();
    });



    // AJAX ACTIONS
    // This when the create list form is submitted, this function sends a fetch
    //      request using ajax. It then checks the result and takes appropriate action.
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
                    _reportErrors(result); // If the name was invalid, report errors to user.
                }
            })
            .catch(error => {
                console.error('Error:', error);
            })
    }

    // This function, when a user confirms the delete from the popover,
    //  sends the delete request using ajax. It then takes the appropriate action
    //  based on the result.
    function _sendDeleteAjax(url, id) {
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
                if (result?.message === "deleted") {
                    console.log('Success: the item was removed');
                    _notifyConnectedClients("LIST-DELETED", result.id);
                    $('#messageArea').html("The list has been deleted!");
                    $('#alertArea').show(400);
                }
                else if (result?.message === "not-owner") {
                    $('#messageArea').html("Only the owner can delete the list!");
                    $('#alertArea').show(400);
                }
                else if (result?.message === "invalid-request") {
                    $('#messageArea').html("The list no longer exists!");
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

    // Function to remove the row after access revocation using the Id for the GroceryListUser
    //  which grants access to the list. If a row exists we find and fade it out, then we
    //  overwrite it with an empty string(deleting it). 
    function _removeRow(accessId) {

        let rowToDelete = document.querySelector(`#index-row-${accessId}`);

        if (rowToDelete != null) {
            $(`#index-row-${accessId}`).hide(400, () => {
                rowToDelete.replaceWith("");
            })
        }
    }

    // Function to remove the row after a list is deleted using the Id of the list. 
    //  Because each item can only have 1 Id, we used a unique class for each row based upon
    //  The list Id. If the list exists, we hide it and then replace it with an empty string.
    function _removeRowOnDelete(listId) {
        $(`.index-row-list-${listId}`).first().hide(400, () => {
            $(`.index-row-list-${listId}`).replaceWith("");
        })
    }

    // Function used when a permission is granted. We first check if a row exists with that permission.
    //  If not, we then see if a row for that list already exists. If not, we updated the table for the
    //  list.
    function _updateTablePermission(accessId, listId) {
        let row = $(`#index-row-${accessId}`);
        if (!(row.len > 0)) {
            // If the row for that permission doesn't exist, check if its already represented.
            let rowToCheck = $(`.index-row-list-${listId}`).first()
            
            if (!(rowToCheck.length > 0)) {
                _updateListTable(listId);
            }
        }
    }

    // Function to update the row if the list's name changes or item count changes.
    //  If the list is in the table, we overwrite its content with the result,
    //  and reset the popovers.
    function _updateRow(listId) {
        fetch(`/grocerylist/updatelistrow/${listId}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('There was a network error!');
                }
                return response.text();
            })
            .then(result => {
                $(`.index-row-list-${listId}`).first().html(result);
                _setupPopovers();
            })
            .catch(error => {
                console.error('Error:', error);
            });
    }

    // Function to update the home index after adding a list or granting access.
    //      We fetch the row, which checks if the user has permission to see it.
    //      We then add it to the table. If the user didn't have access, we append
    //      an empty string, which does nothing. We then reset popovers.
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
                _setupPopovers();
            })
            .catch(error => {
                console.error('Error:', error);
            });
    }


    // Function to clear error messages from the span for users.
    function _clearErrorMessages() {
        $.each($('span[data-valmsg-for]'), function _clearSpan() {
            $(this).html("");
        });
    }

    // Function to report errors to users in the span for errors.
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

    // Function to set up popovers, provided by Dr. Roach.
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
                _sendDeleteAjax(url, id);
            });
        });
    }

    // Function to send notification to clients. This page will report if a list
    //  is created or list is deleted to all users, along with the id of that list.
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