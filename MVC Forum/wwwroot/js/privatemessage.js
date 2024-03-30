            $(document).on("click", '[class^="pmread_"]',
            function(e) {
                e.preventDefault();
                $('#pm-message').load("/PrivateMessage/Read/" + $(this).data("id"));
            });
        $(".pm-settings").on("click",
            function(e) {
                e.preventDefault();
                $('#pm-message').load("/PrivateMessage/Settings/",function() {
                        $('.btn-success').focus();
                });

                    
            });
        $(".pm-message").on("click",
            function (e) {
                e.preventDefault();
                $('#pm-message').load("/PrivateMessage/Create/",function() {
                    ValidateForms();
                    revalidate();
                    $('#To').focus();
                });
            });
        $('#select_all').on("change",function() {
            var checkboxes = $(this).closest('form').find(':checkbox');
            checkboxes.prop('checked', $(this).is(':checked'));
        });
        $(".fa-trash").on("click",
            function() {
                if ($(this).data("id")) {
                    if(confirm("delete this Message? ")) {

                        $.post("/PrivateMessage/Delete", { pmid: $(this).data("id"), userid: $(this).data("user") }, function (data) {
                            if (data.success) {
                                alert(data.responseText);
                                location.reload();
                            } else {
                                alert(data.responseText);
                            }
                        });
                    }
                } else {
                        if (confirm("@Localizer["PMDelConfirm"]")) {
                        $.post("/PrivateMessage/DeleteMany", $("#pm-form").closest('form').serialize(), function (data) {
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
            var $form = $(this).closest('form');
                $.post("/PrivateMessage/Search", $(this).closest('form').serialize(), function (data) {
                if (data) {
                    $('#msg-list').html(data);
                } else {
                    alert(data);
                }
            });
        });
        $(document).on("click", ".fa-reply", function (e) {
            $('#pm-message').load("/PrivateMessage/Reply/" + $(this).data("id"),function() {
                ValidateForms();
                revalidate();
            });
            e.stopPropagation();
            return false;
        });
        $(document).on("click", ".fa-share", function (e) {
            $('#pm-message').load("/PrivateMessage/Forward/" + $(this).data("id"), function () {
                ValidateForms();
                revalidate();
                
            });
            e.stopPropagation();
            return false;
        });
        $(document).on("click", ".fa-search", function (e) {
                $('#pm-message').load("/PrivateMessage/SearchMessages/" + $(this).data("id"), function () {
                ValidateForms();
                revalidate();
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