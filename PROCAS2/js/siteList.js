// JS functions for the Particpant List pages

$(document).ready(function () {

    $('#tblSites').DataTable({
        stateSave: true,
        columns: [
                null,
                null,
                null,
                null

        ]
    });


    // Delete will remove all the information entirely, so make sure they mean it!
    $(".site-delete").on("click", function () {
        var form = $(this).parent();
        alert(form.prop("id"));

        $.confirm({
            title: 'Delete site',
            content: "Are you sure you want to do that? This action cannot be undone.",
            buttons: {
                confirm: function () {

                    $("#" + form.prop("id")).submit();
                },
                cancel: function () {

                }
            }

        });
    });

});


// Handle the returned delete message (true or false)
function onDeleteSuccess(result) {

    if (result.deleted === true) {
        // Redirect to list
        window.location.href = "/Site/Index";
    }
    else {
        $.alert({
            title: 'Not Deleted',
            content: 'There was a problem and the site has not been deleted.',
        });
    }

}