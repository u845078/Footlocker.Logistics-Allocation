// window.js
// Helper functions for the creation/loading/showing of a telerik window

function createWindow(name, title, contentHTML) {
    // Create window
    var window = $.telerik.window.create({
        name: name,
        title: title,
        modal: true,
        draggable: true,
        resizable: false,
        onClose: function () {
            window.data('tWindow').destroy();
        },
        onOpen: function () {

        }
    });

    window.addClass('form-window');
    window.addClass(name);

    // Load window content
    var containerHTML = '<div class="window-content-container"><div class="dialog-content"></div></div>';
    $(window).data('tWindow').content(containerHTML);
    $(window).find('.dialog-content').html(contentHTML);

    // Open window
    $(window).data('tWindow').center().open();
}

function giveInputElementFocus(inputElementSelectorString) {
    // Default input
    if (!inputElementSelectorString) {
        inputElementSelectorString = ".t-window .form-fieldset .editor-input[type=text]:not(.textbox-readonly), .t-window .form-fieldset textarea.editor-input:not(.textbox-readonly)";
    }

    // Get input elements which could be focussed (priority based list)
    var $focusInputs = $(inputElementSelectorString);
    if ($focusInputs.length < 1) {
        $focusInputs = $(".t-window .form-fieldset .editor-input:not(.textbox-readonly)");
    }

    // Give focus to input element
    $focusInputs.first().focus();
}

function showWindow($obj, name, title, contentAction, focusElementSelectorString) {
    // Make AJAX call for window content
    //$obj.ajax({
    $.ajax({
        type: 'GET',
        url: contentAction,
        success: function (data, textState, e) {
            // Show the window
            createWindow(name, title, data);

            // Give focus
            giveInputElementFocus(focusElementSelectorString);
        },
        error: function (e, textState, errorThrown) {
            // Show universal, unhandled exception error message dialog
            var resultObject = (e.responseText != "") ? JSON.parse(e.responseText) : null;
            alert(resultObject ? resultObject.Message : "A system error has occurred.  Please contact your administrator. ");
        }
    });
}