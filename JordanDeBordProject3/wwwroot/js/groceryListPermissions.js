'use strict';
// Some portions of this was provided by (and modified from) Dr. Roach's Labs

// Self calling function when the page loads. Sets up our event listensers and
//  connection. 
(function _groceryListPermissions() {
    console.log("Permissions")
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/groceryListHub")
        .build();

    // Notification event listener. If the list is deleted while on the page for it
    //  we force a reload, which will kick the user home as the Id is no longer valid.
    connection.on("Notification", (message) => {
        var incoming = JSON.parse(message);
        console.log(incoming);

        if (incoming.type === "LIST-DELETED") {
            let access = $(`#perm-table-${incoming.data}`);
            if (access.length > 0) {
                location.reload();
            }
        }
    });

    // Start connection and catch errors.
    connection.start().catch((err) => {
        return console.error(err.toString());
    });

    // Setup the popovers for the delete button for each granted permission.
    _setupPopovers();

    // EVENT LISTENERS FROM PAGE.
    const grantPermissionForm = document.querySelector("#grantPermissionForm");

    // Listener to close out of the alert area.
    $('#alertCloseBtn').on('click', function _hideAlert() {
        $('#alertArea').hide(400);
    });

    // Event listener for our form, which (when submitted) will call the Ajax function to 
    //  add the permission.
    grantPermissionForm.addEventListener('submit', (e) => {
        e.preventDefault();
        _clearErrorMessages();
        _submitPermissionWithAjax();
    });

    // Event listener to prevent the default behavior for the delete permission button.
    $(document).on('click', '.revokeAjax', (e) => {
        e.preventDefault();
    });


    // AJAX ACTIONS AND OTHER FUNCTIONS

    // Function to submit our new permission to be added with ajax, and take action based upon the response.
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
                    _notifyConnectedClients("PERMISSION-GRANTED", result.id, result.listId );
                    $('#messageArea').html("A user was granted access!");
                    $('#alertArea').show(400);
                    location.reload();
                }
                else if (result?.message === "previous-access") {
                    $('#createGroceryListModal').modal('hide');
                    $('#messageArea').html("User already has access!");
                    $('#alertArea').show(400);
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

    // Function to send the ajax request to remove the permission from the database.
    //  We then take appropriate action based upon the response.
    function _sendRevokeAccessAjax(url, accessId) {
        fetch(url, {
            method: "post",
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `id=${accessId}`
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('There was a network error!');
                }
                return response.json();
            })
            .then(result => {
                if (result?.message === "access-revoked") {
                    console.log('Success: the user access was revoked');
                    _notifyConnectedClients("ACCESS-REVOKED", result.id, null);
                    location.reload();
                }
                else if (result?.message === "no-access") {
                    $('#messageArea').html("That user does not have access!");
                    $('#alertArea').show(400);
                }
                else if (result?.message === "revoke-declined") {
                    $('#messageArea').html("Can not remove the owner!");
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
           

    // Function to clear error messages.
    function _clearErrorMessages() {
        $.each($('span[data-valmsg-for]'), function _clearSpan() {
            $(this).html("");
        });
    }

    // Function to report errors to the user.
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

    // Function to setup the popovers on the delete buttons. This adds the second
    //  button to confirm the deletion.
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
                _sendRevokeAccessAjax(url, id);
            });
        });

    }


    // Function to send notification to clients. In this case, data is the Id of the
    //  permission (the GrocerListUser), while data2 is used to report the list that the
    //  new permission is for.
    function _notifyConnectedClients(type, data, data2) {
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