$(document).ready(function(){

    // Delete will remove all the information entirely, so make sure they mean it!
    $("#btnDelete").on("click", function () {
        $.confirm({
            title: 'Delete participant',
            content: "This will remove all the participant's data from BC-PREDICT. Are you sure you want to do that? This action cannot be undone.",
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
