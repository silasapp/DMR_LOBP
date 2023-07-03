$(document).ready(function () {

    var userId = '@Session["Email"]';
    if (userId != null) {
        $.get(url, function (data) {
            $(".modal-content").html(data);
        });
    }

});