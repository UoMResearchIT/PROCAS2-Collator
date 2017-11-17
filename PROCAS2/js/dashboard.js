$(document).ready(function () {

    $.ajax({
        url: "/Home/ConsentPanel",
        beforeSend: function () {
            
            $("#pnlConsent").html("<h4>Consent</h4><p>Loading...</p>");
        }
    })
        .done(function (data) {

            $("#pnlConsent").html(data);

        })
        .fail(function(error){
            $("#pnlConsent").html("<p>Error: Consent Panel information cannot be found.</p>");
        });


});