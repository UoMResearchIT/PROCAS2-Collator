// JS functions for the Volpara results view page

$(document).ready(function () {


    $('#tblVolpara').DataTable({
        stateSave: true,
        scrollX:true,
        fixedColumns: true,
        paging: false,
        info:false,

        columnDefs: [
        {
            targets: 'volparaCol',
            render: function (data, type, full, meta) {
                return "<div class='text-wrap width-200'>" + data + "</div>";
            }
        }
        ]
    });

});