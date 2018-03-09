$(document).ready(function () {

   

    // Delete will remove all the information entirely, so make sure they mean it!
    $("#btnDelete").on("click", function () {
        $.confirm({
            title: 'Delete histology focus',
            content: "This will remove the focus data from BC-PREDICT. Are you sure you want to do that? This action cannot be undone.",
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
        window.location.href = "/Histology/Edit/" + $("#NHSNumber").val() + "/" + $("#PrimaryNumber").val();
    }
    else {
        $.alert({
            title: 'Not Deleted',
            content: "There was a problem and the focus record has not been deleted.",
        });
    }

}