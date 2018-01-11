$(function () {
    var $menu = $("#qyMenu>ul");
    $menu.click(function (event) {
        var $target = $(event.target);
        $.each($menu, function (index, content) {
            $(this).find("li").each(function () {
                $(this).removeClass("active");
            });
        });
        $target.parent("li").addClass("active");
    });

})