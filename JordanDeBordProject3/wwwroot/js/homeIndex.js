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
            let rowInfo = $(`#index-row-${incoming.data}`);

            if (!(rowInfo.length > 0)) {
                _updateTablePermission(incoming.data, incoming.data2);
            }

        }
        else if (incoming.type === "ACCESS-REVOKED") {
            _removeRow(incoming.data);
        }
        else if (incoming.type === "LIST-DELETED") {
            _removeRowOnDelete(incoming.data);
        }
        else if (incoming.type === "ITEM-REMOVED" || incoming.type === "ITEM-ADDED") {
            _updateRow(incoming.data2);
        }
        else if (incoming.type === "LIST-UPDATED")
        {
            _updateRow(incoming.data);
        }
    });

    // Start connection and catch errors.
    connection.start().catch((err) => {
        return console.error(err.toString());
    });

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

    // Function to remove the row after Deletion or Access Revocation.
    function _removeRow(accessId) {

        let rowToDelete = document.querySelector(`#index-row-${accessId}`);

        if (rowToDelete != null) {
            $(`#index-row-${accessId}`).hide(400, () => {
                rowToDelete.replaceWith("");
            })
        }
    }

    function _removeRowOnDelete(listId) {
        $(`.index-row-list-${listId}`).first().hide(400, () => {
            $(`.index-row-list-${listId}`).replaceWith("");
        })
    }

    // Update the table if the row doesn't already exist.
    // This prevents repeat updates if multiple people are granted
    //      Access to the same list.
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

    // Update the row if the name changes or item count changes.
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

    // Function to update the home index after name change or item count change.
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
                _sendDeleteAjax(url, id);
            });
        });
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