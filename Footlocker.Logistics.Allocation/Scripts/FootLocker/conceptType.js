// conceptType.js
// This script facilitates the creation and presentation of the 'Add Concept' and 'Edit Concept' dialogs (via Telerik Windows)

// HACK: Want to do this by specifying callback, but telerik grid column ClientTemplate making this impossible with literal usage...
function refreshConceptsGrid(selectedConceptID) {
    // Refresh the concepts grid
    var conceptsGrid = $("#ConceptTypesGrid").data("tGrid");
    conceptsGrid.ajaxRequest();

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

function showAddConceptWindow($contentContainer) {
    var name = 'conceptWindow';
    var title = 'Add Concept';
    var contentAction = '/ConceptType/Create'

    // Create 'Add Concept' window
    var focusElementSelectorString = "." + name + ".t-window .form-fieldset .editor-input[type=text], " + "." + name + ".t-window .form-fieldset textarea.editor-input";
    showWindow($contentContainer, name, title, contentAction, focusElementSelectorString);
}

function showEditConceptWindow($contentContainer, id) {
    var name = 'conceptWindow';
    var title = 'Manage Concept';
    var contentAction = '/ConceptType/Edit?ID=' + id;

    // Create 'Edit Concept' window
    var focusElementSelectorString = "." + name + ".t-window .form-fieldset .editor-input[type=text], " + "." + name + ".t-window .form-fieldset textarea.editor-input";
    showWindow($contentContainer, name, title, contentAction, focusElementSelectorString);
}

function deleteConcept(id) {
    // Prompt for confirmation
    if (confirm("Are you sure you want to delete this Concept?")) {
        // Post to the controller via ajax
        $.ajax({
            type: "POST",
            url: "/ConceptType/Delete",
            data: JSON.stringify({ id: id }),
            contentType: "application/json; charset=utf-8",
            success: refreshConceptsGrid,
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