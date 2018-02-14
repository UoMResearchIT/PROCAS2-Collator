$(document).ready(function () {

    $('#tblTxErrors').DataTable({
        stateSave: true,
        columns: [
                null,
                null,
                null,
                null

        ]
    });

    // Make sure they mean it!
    $(".error-review").on("click", function () {
        var form = $(this).parent();
        //alert(form.prop("id"));

        $.confirm({
            title: 'Mark as Reviewed',
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
function onReviewSuccess(result) {

    if (result.reviewed === true) {
        // Redirect to list
        window.location.href = "/TxErrors/Index";
    }
    else {
        $.alert({
            title: 'Not marked',
            content: 'There was a problem and the error has not been marked as reviewed.',
        });
    }

}