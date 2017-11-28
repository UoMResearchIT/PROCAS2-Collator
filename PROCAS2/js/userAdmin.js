
// JS functions for the User Admin pages

$(document).ready(function () {


    

    /* Create an array with the values of all the checkboxes in a column */
    $.fn.dataTable.ext.order['dom-checkbox'] = function (settings, col) {
        return this.api().column(col, { order: 'index' }).nodes().map(function (td, i) {
            return $('input', td).prop('checked') ? '1' : '0';
        });
    };

    $('#tblUsers').DataTable({
        stateSave: true,
        columns: [
                null,
                { "orderDataType": "dom-checkbox" },
                { "orderDataType": "dom-checkbox" },
                null,
                null,
                null
                
        ]
    });

    // If any of the buttons on the table are pressed, prompt the user to make sure that they
    // really want to do it!
    $('.btnAdminAction').confirm({
        title: "Are you sure?",
        content: "You clicked to do the following action: <br/> ", 
        onContentReady: function () {
            

            this.setContentAppend('<div>' + this.$target[0].text + '</div>');


            }
    });


    
});