$(document).ready(function () {

    $(".datepicker").datepicker({
        format: "dd/mm/yyyy"
    });

    // Fade out the 'saved' message if necessary
    window.setTimeout(function () {
        $("#divSaveChangeAlert").fadeTo(500, 0).slideUp(500, function () {
            $(this).remove();
        });
    }, 5000);

    // Delete will remove all the information entirely, so make sure they mean it!
    $("#btnDelete").on("click", function () {
        $.confirm({
            title: 'Delete histology',
            content: "This will remove all the participant's histology data from BC-PREDICT. Are you sure you want to do that? This action cannot be undone.",
            buttons: {
                confirm: function () {
                    $("#frmDelete").submit();
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
        window.location.href = "/Histology/Index";
    }
    else {
        $.alert({
            title: 'Not Deleted',
            content: "There was a problem and the participant's histology record has not been deleted.",
        });
    }

}