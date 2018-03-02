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

});