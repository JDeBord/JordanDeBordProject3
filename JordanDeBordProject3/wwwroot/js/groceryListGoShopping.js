﻿'use strict';
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

        if (incoming.type === "ITEM-CHECKED") {
            _updateCheckedRow(incoming.data);
        }
        else if (incoming.type === "ITEM-UNCHECKED")
        {
            _updateCheckedRow(incoming.data);
        }
    });

    // Start connection and catch errors.
    connection.start().catch((err) => {
        return console.error(err.toString());
    });

    // EVENT LISTENERS FROM PAGE.
    $("input[type=checkbox]").change(function () {
        let $box = $(this); // Get the checkbox
        let boxId = $box.attr('id');
        let idx = boxId.lastIndexOf('-');
        let id = boxId.substring(idx + 1);
        if (this.checked) {
            // If checked, send check
            _sendCheckWithAjax("/grocerylist/checkAjax", id);
            console.log(id);
        }
        else {
            // if unchecked, send uncheck
            _sendUnCheckWithAjax("/grocerylist/uncheckAjax", id);
        }
    })


    // AJAX ACTIONS
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
            })
            .catch(error => {
                console.error('Error:', error);
            });
    }

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
            })
            .catch(error => {
                console.error('Error:', error);
            });
    }

    // OTHER METHODS/FUNCTIONS
    function _updateCheckedRow(id) {
        fetch(`/grocerylist/shoprow/${id}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('There was a network error!');
                }
                return response.text();
            })
            .then(result => {
                let row = $(`#shop-row-${id}`);
                if (row != null)
                {
                    $(`#shop-row-${id}`).hide(400, () => {
                        $(`#shop-row-${id}`).html(result);
                        $(`#shop-row-${id}`).show(400);
                    })
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