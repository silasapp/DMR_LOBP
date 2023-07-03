$(document).ready(function () {
    initialize();
    loadModal();
    loadSelect2();

    function initialize() {
        $("#lnk-Dashboard > a").addClass("selected").find(".act").addClass("selected");
        //$("li.more ul").hide();
        $(".sidebar > ul > li.more > a").click(function (e) {
            e.preventDefault();
            if ($(this).hasClass("open")) {
                $(this).removeClass("open");
                $(this).parent("li").find("ul").slideToggle(200);
            }
            else {
                $(this).addClass("open")
                //$(this).find(".more").css({ "transform": "rotate('90deg')" })
                $(this).parent("li").find("ul").slideToggle(200);
            }
        });
    }
    function loadModal() {
        $('#modalPopup').on('show.bs.modal', function (e) {
            var target = $(e.relatedTarget);
            var loadurl = target.data('url');
            $(this).find('.modal-content').empty();
            $(this).find('.modal-content').load(loadurl, function () { loadSelect2(); });
        });
    }
    function loadSelect2() {
        $(".use-select2").select2();
    }
});