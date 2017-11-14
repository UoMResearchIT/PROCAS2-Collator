$(document).ready(function () {
    $("#btnUpload").on("click", function (e) {
        $('#divExportAlert').hide();
        showLoading();

        var token = new Date().getTime();

        $.cookie("fileDownloadToken", token, { expires: 1, path: '/' });

        downloadCheck = window.setInterval(function () {
            var cookieValue = $.cookie('fileDownloadToken');
            if (cookieValue == token * (-1))
                downloadStarted();
        }, 1000);

        function downloadStarted() {
            window.clearInterval(downloadCheck);

            $.cookie('fileDownloadToken', null, { expires: 1, path: '/' }); //clear the cookie value
            $("#exportElement").spin(false);
            $("#exportElement").hide();
            location.href = location.href;;
        }
    });

    


})

function showLoading() {

    $("#exportElement").show();
    var opts = {
        lines: 13, // The number of lines to draw
        length: 20, // The length of each line
        width: 10, // The line thickness
        radius: 30, // The radius of the inner circle
        corners: 1, // Corner roundness (0..1)
        rotate: 0, // The rotation offset
        direction: 1, // 1: clockwise, -1: counterclockwise
        color: '#000', // #rgb or #rrggbb or array of colors
        speed: 1, // Rounds per second
        trail: 60, // Afterglow percentage
        shadow: false, // Whether to render a shadow
        hwaccel: false, // Whether to use hardware acceleration
        className: 'spinner', // The CSS class to assign to the spinner
        zIndex: 2e9, // The z-index (defaults to 2000000000)
        top: '50%', // Top position relative to parent in px
        left: '50%' // Left position relative to parent in px
    };

    $("#exportElement").spin(opts);
}