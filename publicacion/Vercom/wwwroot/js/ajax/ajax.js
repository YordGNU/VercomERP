$(document).ajaxStart(function () {
    $('#global-loader').css('display', 'flex');
});

$(document).ajaxStop(function () {
    $('#global-loader').css('display', 'none');
});
