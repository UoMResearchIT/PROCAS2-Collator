$(document).ready(function () {

    var urlpath = "";

    if (window.location.host.indexOf("vm-humdevnett01") > -1) {
        urlpath = urlpath + "/PROCAS2";
    }

    // Consent Panel
    //  Fetch data to show in the consent panel
    $.ajax({
        url: urlpath + "/Home/ConsentPanel",
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
        url: urlpath + "/Home/SitePanel",
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
        url: urlpath + "/Home/RiskPanel",
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
        url: urlpath + "/Home/AppNewsPanel",
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
        url: urlpath + "/Home/TxErrorsPanel",
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

    // Volpara info panel
    // Fetch data to show Volpara info
    $.ajax({
        url: urlpath + "/Home/VolparaPanel",
        beforeSend: function () {

            $("#pnlVolpara").html("<h4>Volpara</h4><p>Loading...</p>");
        }
    })
        .done(function (data) {

            $("#pnlVolpara").html(data);

        })
        .fail(function (error) {
            $("#pnlVolpara").html("<p>Error: Volpara information cannot be found.</p>");
        });

});