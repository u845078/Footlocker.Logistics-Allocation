// fobPackOverride.js
// This script facilitates the creation and presentation of the 'Add Override' and 'Edit Override' dialogs (via Telerik Windows)

// HACK: Want to do this by specifying callback, but telerik grid column ClientTemplate making this impossible with literal usage...
function refreshOverridesGrid(selectedOverrideID) {
    // Refresh the overrides grid
    var overridesGrid = $("#OverridesGrid").data("tGrid");
    overridesGrid.ajaxRequest();

    // HACK: Not re-selecting the row of a concept that was edited, due to needing to run the below logic after
    //            the grid.ajaxRequest() request is completed, callback style...unfortunately we have paging, and for an add if the sort is set
    //            the concept just added could end up on a different page, so (due to being unable to specify page in ajaxRequest()) we would need to get the
    //            new, correct page number from the server on add (along with the newly generated identity), set the grid to that page number, trigger the ajaxRequest()
    //            then on client run the below logic for the returned identity (or ID)
    // CHOOSING TO SAVE THIS MADNESS FOR LATER......(not implementing for the Edit, as think it would only lead to why not on the Add too...)

    //    // Re-select the concept
    //    $.each(overridesGrid.$rows(), function (index, value) {
    //        var override = overridesGrid.dataItem(value);

    //        if (override.ID == selectedOverrideID) {
    //            $(value).addClass('t-state-selected');
    //        }
    //    });

    // Disable Edit/Delete buttons until selection occurs
    setDisplayEnabled($('button.modify-button'), false);
}

function showAddOverrideWindow($contentContainer, fobPackId) {
    var name = 'overrideWindow';
    var title = 'Add Override';
    var contentAction = '/FOBPackOverride/Create?fobPackID=' + fobPackId;

    // Create 'Add Override' window
    var focusElementSelectorString = "." + name + ".t-window .form-fieldset .editor-input.t-numerictextbox";
    showWindow($contentContainer, name, title, contentAction, focusElementSelectorString);
}

function showEditOverrideWindow($contentContainer, id) {
    var name = 'overrideWindow';
    var title = 'Edit Override';
    var contentAction = '/FOBPackOverride/Edit?ID=' + id;

    // Create 'Edit Override' window
    var focusElementSelectorString = "." + name + ".t-window .form-fieldset .editor-input.t-numerictextbox";
    showWindow($contentContainer, name, title, contentAction, focusElementSelectorString);
}

function deleteOverride(id) {
    // Prompt for confirmation
    if (confirm("Are you sure you want to delete this Override?")) {
        // Post to the controller via ajax
        $.ajax({
            type: "POST",
            url: "/FOBPackOverride/Delete",
            data: JSON.stringify({ id: id }),
            contentType: "application/json; charset=utf-8",
            success: function () {
                // Refresh the overrides and underlying packs grid
                refreshOverridesGrid();
                refreshPacksGrid();
            },
            error: function (e, textState, errorThrown) {
                // NOTE: There are still issues with IE and aggresive caching here (exception response from Global.asax Application_Error() )...request gets 
                //           cached without content, so e.responseText is empty string in JS ajax error callback....issue stops if running fiddler and works fine in chome :)
                var resultObject = (e.responseText != "") ? JSON.parse(e.responseText) : null;

                // Show error message dialog (For DELETE validation exceptions and unhandled exceptions can be shown here as no other actions to perform)
                alert(resultObject ? resultObject.Message : "A system error has occurred.  Please contact your administrator. ");
            }
        });
    }
}