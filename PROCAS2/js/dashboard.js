$(document).ready(function () {

    // Consent Panel
    //  Fetch data to show in the consent panel
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


    // Site listing panel
    // Fetch data to show in the site listing panel
    $.ajax({
        url: "/Home/SitePanel",
        beforeSend: function () {

            $("#pnlSite").html("<h4>Site Listing</h4><p>Loading...</p>");
        }
    })
        .done(function (data) {

            $("#pnlSite").html(data);

        })
        .fail(function (error) {
            $("#pnlSite").html("<p>Error: Site Panel information cannot be found.</p>");
        });
});