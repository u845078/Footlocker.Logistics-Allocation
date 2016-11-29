// fobPack.js
// This script facilitates the creation and presentation of the 'Edit FOB Pack' dialog (via Telerik Windows)

// HACK: Want to do this by specifying callback, but telerik grid column ClientTemplate making this impossible with literal usage...
function refreshPacksGrid() {
    // Refresh the packs grid
    var packsGrid = $("#PacksGrid").data("tGrid");
    packsGrid.ajaxRequest();

    // HACK: Not re-selecting the row of a concept that was edited, due to needing to run the below logic after
    //            the grid.ajaxRequest() request is completed, callback style...unfortunately we have paging, and for an add if the sort is set
    //            the concept just added could end up on a different page, so (due to being unable to specify page in ajaxRequest()) we would need to get the
    //            new, correct page number from the server on add (along with the newly generated identity), set the grid to that page number, trigger the ajaxRequest()
    //            then on client run the below logic for the returned identity (or ID)
    // CHOOSING TO SAVE THIS MADNESS FOR LATER......(not implementing for the Edit, as think it would only lead to why not on the Add too...)

    //    // Re-select the concept
    //    $.each(conceptsGrid.$rows(), function (index, value) {
    //        var concept = conceptsGrid.dataItem(value);

    //        if (concept.ID == selectedConceptID) {
    //            $(value).addClass('t-state-selected');
    //        }
    //    });

    // Disable Edit/Delete buttons until selection occurs
    setDisplayEnabled($('button.modify-button'), false);
}

function showEditPackWindow($contentContainer, id) {
    var name = 'packWindow';
    var title = 'Manage FOB Pack';
    var contentAction = '/FOBPack/Edit?ID=' + id;

    // Create 'Edit FOB Pack' window
    var focusElementSelectorString = "." + name + ".t-window .form-fieldset .editor-input[type=text]:not(.textbox-readonly), " + "." + name + ".t-window .form-fieldset textarea.editor-input:not(.textbox-readonly)";
    showWindow($contentContainer, name, title, contentAction, focusElementSelectorString);
}
