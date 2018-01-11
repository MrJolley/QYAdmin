function transferTableChecked() {
    var address = $("#txtEmail").val();
    var password = $("#txtPassword").val();
    var reg = /^[-a-z0-9~!$%^&*_=+}{\'?]+(\.[-a-z0-9~!$%^&*_=+}{\'?]+)*@([a-z0-9_][-a-z0-9_]*(\.[-a-z0-9_]+)*\.(aero|arpa|biz|com|coop|edu|gov|info|int|mil|museum|name|net|org|pro|travel|mobi|[a-z][a-z])|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(:[0-9]{1,5})?$/i;
    if (!address.match(reg)) {
        alert("a wrong format of sender mail address founded.");
        return false;
    }
    if (password.length == 0) {
        alert("enter a pwd of the special email");
        return false;
    }
    var checked_value = [];
    var checked_name = document.getElementsByName("selectOption");
    for (var i = 0; i < checked_name.length; i++) {
        if (checked_name[i].checked == true) {
            checked_value.push(checked_name[i].value);
        }
    }
    if (checked_value.length == 0) {
        alert("pls select at least one receiver.");
        return false;
    }
    var date = $("#txtDate").val();
    var $description = $("#description").val();

    var head_content = [];
    var body_content = [];
    var $head = $("#table_head tr td");
    $.each($head, function (idx, content) {
        if (idx > 1)
        {
            var $index = $(content).html().indexOf("<");
            if ($index > 0)
                head_content.push($(content).html().substring(0, $index));
            else
                head_content.push($(content).html());
        }
            
    });
    var $body = $("#table_body tr input:checkbox[name=selectOption]:checked");
    $.each($body, function (idx, content) {
        body_content[idx] = [];
        var $tr = $(content).parent("td").parent("tr").children("td:gt(1)");
        $.each($tr, function (subidx, content) {
            body_content[idx].push($(content).html());
        });
    });

    $.ajax({
        type: "post",
        url: "/Home/SendMail",
        data: { "head": head_content, "body": body_content, "sender": address, "password": password, "date": date, "description": $description },
        dataType: "json",
        success: function (data) {
            var m = [];
            var success = data["success"];
            for (var key in success) {
                m.push(success[key]);
            }
            for (var i = 0; i < m.length; i++) {
                $("." + m[i]).html("√");
                $("." + m[i]).addClass("correctStyle");
            }
            alert("邮件发送成功：" + m + "\r\n 一共" + m.length + "人");
        }
    });
}

$("#table_head").click(function (event) {
    var $target = $(event.target);
    if ($target.hasClass("delete-col")) {
        var $td = $target.parent("td");
        var $idx = $td.index();
        $td.remove();
        var $body = $("#table_body tr");
        $.each($body, function (index, content) {
            $(content).find("td:eq(" + $idx + ")").remove();
        });
    }
    var titleHeight = $(".tablehead").height();
    var titltWidth = $(".tablehead").width();
    $(".tablebody").css("top", titleHeight);
    $(".tablebody").css("width", titltWidth + 17);
});

$(function () {
    var description = "Dear colleague,\r\n\r\n以下是本月发放的工资明细，\r\n \r\n如有任何问题，请及时联系我。";
    $("#description").val(description);
});