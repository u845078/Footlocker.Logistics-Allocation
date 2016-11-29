
// --------------------------------------------------------------------------------------------
// telerikShims_ChildGrid.js
// NOTE: This script provides functions useful for overcoming some of the shortcomings of the Telerik MVC grid, specifically when utilizing child grids
// --------------------------------------------------------------------------------------------



function hack_removeTelerikExpandIconsForChildlessNodes($indicatingInputElements, indicatorFunction) {
    // Get all master rows with size value less than 4 -- Non-CaseLot Ring Fence Detail rows...
    $indicatingInputElements.each(function (idx) {
        if (indicatorFunction($(this))) {
            // Hide expand icons
            var $masterRow = $(this).closest('tr.t-master-row');
            $masterRow.find('.t-hierarchy-cell .t-icon').css('visibility', 'hidden');
        }
    });
}

function hack_setChildGridColumnWidth(childGridColumnWidth) {
    // Set child grid col span width...
    $('.t-detail-cell').attr('colspan', childGridColumnWidth);
}





