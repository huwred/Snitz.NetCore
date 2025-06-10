
        $(document).on("click","#btn-captcha", function(e) {
            e.preventDefault();
            SnitzVars.captchaCheck($("#Captcha").val(), function(data) {
                if (data) {
                    $("#captcha-check").hide();
                    $("#captcha-refresh").hide();
                    $("#login-form").show();
                } else {
                    $("#Captcha").val("");
                    $("#Captcha").attr("placeholder", "Incorrect, please try again.");
                }
            });
        });
        $(document).on("click","#captcha-refresh", function(e) {
            e.preventDefault();
            $.ajax({
                url: SnitzVars.baseUrl + "/refreshcaptcha",
                success: function (data) {
                    $("#captchaimage-div").html(data);
                }
            });
        });
