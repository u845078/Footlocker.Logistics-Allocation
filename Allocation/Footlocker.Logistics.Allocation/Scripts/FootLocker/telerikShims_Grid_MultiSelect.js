// --------------------------------------------------------------------------------------------
// telerikShims_Grid_MultiSelect.js
// NOTE: This script provides functions useful for overcoming some of the shortcomings of the Telerik MVC grid, specifically when utilizing multi-select grids
// --------------------------------------------------------------------------------------------




// HACK: Manual Multi-select Grid for Telerik...
function hack_multiSelectGrid_onRowDataBound(e) {
    // Wire-up hover and selection row events
    $(e.row).hover(function (e) {
        $(this).addClass('t-state-hover');

    }, function (e) {
        $(this).removeClass('t-state-hover');
    });
    $(e.row).click(function (e) {
        $(this).toggleClass('t-state-selected');
    });
}