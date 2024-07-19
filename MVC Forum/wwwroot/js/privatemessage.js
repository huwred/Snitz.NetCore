        $(document).on("click", '[class^="pmread_"]',
            function(e) {
                e.preventDefault();
                $('#pm-message').load(SnitzVars.baseUrl + "/PrivateMessage/Read/" + $(this).data("id"), function () {
                    $("#msg-list").hide();
                });
            });
        $(".pm-settings").on("click",
            function(e) {
                e.preventDefault();
                $('#pm-message').load(SnitzVars.baseUrl + "/PrivateMessage/Settings/",function() {
                        $('.btn-success').focus();
                        $("#msg-list").hide();
                });

                    
            });
        $(".pm-message").on("click",
            function (e) {
                e.preventDefault();
                $('#pm-message').load(SnitzVars.baseUrl + "/PrivateMessage/Create/",function() {
                    ValidateForms();
                    revalidate();
                    $('#To').focus();
                    $("#msg-list").hide();
                });
            });
        $('#select_all').on("change",function() {
            var checkboxes = $(this).closest('form').find(':checkbox');
            checkboxes.prop('checked', $(this).is(':checked'));
        });
        $(".pm-delete").on("click",
            function() {
                if ($(this).data("id")) {
                    if(confirm("delete this Message? ")) {

                        $.post(SnitzVars.baseUrl + "/PrivateMessage/Delete", { pmid: $(this).data("id"), userid: $(this).data("user") }, function (data) {
                            if (data.success) {
                                //alert(data.responseText);
                                location.reload();
                            } else {
                                alert(data.responseText);
                            }
                        });
                    }
                } else {
                        if (confirm("delete selected messages")) {
                        $.post(SnitzVars.baseUrl + "/PrivateMessage/DeleteMany", $("#pm-form").closest('form').serialize(), function (data) {
                            if (data.success) {
                                //alert(data.responseText);
                                location.reload();
                            } else {
                                alert(data.responseText);
                            }
                        });
                    }
                }
            });
        $(".fa-trash").on("click",
            function() {
                if ($(this).data("id")) {
                    if(confirm("delete this Message? ")) {

                        $.post(SnitzVars.baseUrl + "/PrivateMessage/Delete", { pmid: $(this).data("id"), userid: $(this).data("user") }, function (data) {
                            if (data.success) {
                                alert(data.responseText);
                                location.reload();
                            } else {
                                alert(data.responseText);
                            }
                        });
                    }
                } else {
                        if (confirm("delete selected messages")) {
                        $.post(SnitzVars.baseUrl + "/PrivateMessage/DeleteMany", $("#pm-form").closest('form').serialize(), function (data) {
                            if (data.success) {
                                alert(data.responseText);
                                location.reload();
                            } else {
                                alert(data.responseText);
                            }
                        });
                    }
                }
            });
        $(document).on('click', '#pm-find', function () {
            $(".pm-button").removeClass("active");
                $.post(SnitzVars.baseUrl + "/PrivateMessage/Search", $(this).closest('form').serialize(), function (data) {
                if (data) {
                    $("#msg-list").show();
                    $('#msg-list').html(data);
                    $('#pm-message').hide();
                } else {
                    alert(data);
                }
            });
        });
        $(document).on("click", ".fa-reply", function (e) {
            $('#pm-message').load(SnitzVars.baseUrl + "/PrivateMessage/Reply/" + $(this).data("id"),function() {
                ValidateForms();
                revalidate();
                $("#msg-list").hide();
            });
            e.stopPropagation();
            return false;
        });
        $(document).on("click", ".fa-share", function (e) {
            $('#pm-message').load(SnitzVars.baseUrl + "/PrivateMessage/Forward/" + $(this).data("id"), function () {
                ValidateForms();
                revalidate();
                $("#msg-list").hide();
            });
            e.stopPropagation();
            return false;
        });
        $(document).on("click", ".fa-envelope", function (e) {
            $.get(SnitzVars.baseUrl + "/PrivateMessage/MarkRead/?id=" + $(this).data("id") + "&val=1", function () {
                location.reload();
                });
            e.stopPropagation();
            return false;
        });
        $(document).on("click", ".fa-envelope-open", function (e) {
            $.get(SnitzVars.baseUrl + "/PrivateMessage/MarkRead/?id=" + $(this).data("id") + "&val=0", function () {
                location.reload();
                });
            e.stopPropagation();
            return false;
        });
        $(document).on("click", ".fa-search", function (e) {
                $('#pm-message').load(SnitzVars.baseUrl + "/PrivateMessage/SearchMessages/" + $(this).data("id"), function () {
                ValidateForms();
                revalidate();
                $("#msg-list").hide();
                $('#Term').focus();

            });
            e.stopPropagation();
            return false;
        });
        $(document).on("click", ".pm-search", function (e) {
                $('#pm-message').load(SnitzVars.baseUrl + "/PrivateMessage/SearchMessages/" + $(this).data("id"), function () {
                ValidateForms();
                revalidate();
                $("#msg-list").hide();
                $('#Term').focus();

            });
            e.stopPropagation();
            return false;
        });
        function revalidate() {
            var container = document.getElementById("pm-message");
            var forms = container.getElementsByTagName("form");
            var newForm = forms[forms.length - 1];
            $.validator.unobtrusive.parse(newForm);
        }