
// --------------------------------------------------------------------------------------------
// telerikShims_ComboBox.js
// NOTE: This script provides functions useful for overcoming some of the shortcomings of the Telerik ComboBox
// --------------------------------------------------------------------------------------------



// HACK: Handling to reset selection value when entering text which dosnt correspond to an option
function comboBox_Changed(e) {
    var comboBox = $(e.target).data("tComboBox");
    processComboBoxChange(comboBox);
}

function processComboBoxChange(comboBox) {
    if (comboBox.value() && comboBox.selectedIndex === -1) {
        comboBox.select(function (dataItem) {
            return (dataItem.Value == e.target.defaultValue);
        });

        if (comboBox.selectedIndex === -1) {
            comboBox.select(0);
        }
    }
    else {
        comboBox.select(comboBox.selectedIndex);
    }

    return false;
}


// HACK: Pre-load ajax data bound combobox
function comboBox_ajax_OnLoad(e, reloadCallback) {
    var $comboBoxElement = $(e.target);
    var comboBox = $comboBoxElement.data("tComboBox");

    var callback = function () { };
    if (reloadCallback && reloadCallback != undefined) {
        callback = function () {
            // Select first element by default
            comboBox.select(0);

            // HACK: Set default value, for changing with invalid text to change back...
            $comboBoxElement[0].defaultValue = comboBox.selectedValue;

            // Call specified callback
            reloadCallback();
        };
    }
    else {
        callback = function () {
            // Select first element by default
            comboBox.select(0);

            // HACK: Set default value, for changing with invalid text to change back...
            $comboBoxElement[0].defaultValue = comboBox.selectedValue;
        }
    }

    // Pre-load if combobox using ajax databinding
    comboBox.reload(callback);
}