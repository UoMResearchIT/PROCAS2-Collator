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


    // Risk letter info panel
    // Fetch data to show risk letter information
    $.ajax({
        url: "/Home/RiskPanel",
        beforeSend: function () {

            $("#pnlRisk").html("<h4>Risk Letters</h4><p>Loading...</p>");
        }
    })
        .done(function (data) {

            $("#pnlRisk").html(data);

        })
        .fail(function (error) {
            $("#pnlRisk").html("<p>Error: Risk Letter information cannot be found.</p>");
        });


    // App News items info panel
    // Fetch data to show applicaiton news information
    $.ajax({
        url: "/Home/AppNewsPanel",
        beforeSend: function () {

            $("#pnlAppNews").html("<h4>Application News</h4><p>Loading...</p>");
        }
    })
        .done(function (data) {

            $("#pnlAppNews").html(data);

        })
        .fail(function (error) {
            $("#pnlAppNews").html("<p>Error: Application news cannot be found.</p>");
        });


    // Tx errors info panel
    // Fetch data to show tranmission errors
    $.ajax({
        url: "/Home/TxErrorsPanel",
        beforeSend: function () {

            $("#pnlTxErrors").html("<h4>Transmission Errors</h4><p>Loading...</p>");
        }
    })
        .done(function (data) {

            $("#pnlTxErrors").html(data);

        })
        .fail(function (error) {
            $("#pnlTxErrors").html("<p>Error: Transmission Errors cannot be found.</p>");
        });

});