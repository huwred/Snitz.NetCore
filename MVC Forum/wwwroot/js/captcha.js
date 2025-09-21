
$('#Captcha').on("keyup",function (event) {
            if (event.which == 13 || event.which == 13) {
                $("#btn-captcha").trigger("click");
                event.preventDefault();
                return false;
            }
        });
$("#btn-captcha").on("click", function (e) {
            e.preventDefault();
            SnitzVars.captchaCheck($("#Captcha").val(), function(data) {
                if (data) {
                    $("#captcha-check").hide();
                    $("#captcha-refresh").hide();
                    $("#login-form").show();
                    setTimeout(function () {
                        $("#Username").focus();
                    }, 100);                    
                } else {
                    $("#Captcha").val("");
                    $("#Captcha").attr("placeholder", "Incorrect, please try again.");
                }
            });
        });
$("#captcha-refresh").on("click", function(e) {
            e.preventDefault();
            $.ajax({
                url: SnitzVars.baseUrl + "/refreshcaptcha",
                success: function (data) {
                    $("#captchaimage-div").html(data);
                }
            });
        });
$("#Captcha").focus();
