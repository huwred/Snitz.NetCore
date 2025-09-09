
    $(document).on("click", "#STRGROUPCATEGORIES", function (e) {
        $("#groups-enable").submit();
    });
    $(document).on('click', '.confirm-archive', function (e) {
        e.preventDefault();
        var postid = $(this).data('id');
        (async() => {
            const result = await b_confirm("Archive this Forum")
            if (result) {
                $('#archive-content').html('');
                $('#archive-content').load(SnitzVars.baseUrl + "/Archive/ArchiveForm/" + postid,function()  {
                    $('#archiveModal').modal('show');
                });
            }
        })()
    });

    $('.getModerators').on('change', function () {
        $('#forum-moderators').load(SnitzVars.baseUrl + '/Admin/GetForumModerators/' + $(this).val(), function () {
            $(".multiselect").twosidedmultiselect();
        });
        $("#forum-moderators").on("click", "#select_all", function () {
            $('#ForumModeratorstsms option').prop('selected', this.checked);
        });
    });
    $(document).on('click','#button_reset_dynamic_form', () => {
        $("#moderators-form")[0].reset();
        location.reload();
    });
$('button[data-bs-toggle="pill"]').on('shown.bs.tab', function (e) {
    console.log("set active tab " + e.target.id);
    localStorage.setItem('activeForumTab', e.target.id);
    console.log(localStorage);
});
$(window).on('load', function () {
    var activeForumTab = localStorage.getItem('activeForumTab');
    console.log("activeForumTab " + activeForumTab);
    if (activeForumTab) {
        $("#v-pills-tab .nav-link.active").removeClass("active");
        $("#v-pills-tabContent .tab-pane.active").removeClass("active").removeClass("show");
        $("#v-pills-tab #" + activeForumTab).addClass("active");
        $("#v-pills-tabContent #" + activeForumTab.replace("-tab", "")).addClass("active").addClass("show");

        switch (activeForumTab) {
            case "v-pills-archive-tab":
                $('#forum-archives').load(SnitzVars.baseUrl + '/Archive/Index/', function () { });
                break;
            case "v-pills-group-tab":
                $('#forum-groups').load(SnitzVars.baseUrl + '/Groups/ManageGroups/' + $(this).val(), function () { });
                break;
        }
    }
});


    $(document).on('shown.bs.tab', 'button[data-bs-toggle="pill"]', function(e) {
        $('[id^=result]').html('');
        localStorage.setItem('activeForumTab', e.target.id);
        if (e.target.id === "v-pills-group-tab") {
            $('#forum-groups').load(SnitzVars.baseUrl + '/Groups/ManageGroups/' + $(this).val(), function () { });
        }
        if (e.target.id === "v-pills-archive-tab") {
            $('#forum-archives').load(SnitzVars.baseUrl + '/Archive/Index/' + $(this).val(), function () { });
        }
    });
    $(function() {
        $('#forum-groups').load(SnitzVars.baseUrl + '/Groups/ManageGroups/' + $(this).val(), function () {});
    });
    $(document).on('click', '.manage-group', function (e) {
        e.preventDefault();
        $('#forum-groups').load(SnitzVars.baseUrl + '/Groups/ManageGroups/' + $(this).data('id'), function () {});
    });
    $(document).on('click','.confirm-delete', function(e) {
        e.preventDefault();
        var postid = $(this).data('id');
        var href = $(this).attr('href');
        var title = $(this).attr('title');
        (async() => {
            const result = await b_confirm(title)
            if (result) {

                $.post(href,
                    {
                        id: postid
                    },
                    function (data, status) {
                        console.log(data);
                        if (!data.result) {
                            appendAlert(data, 'error');
                        } else {
                            location.reload();
                        }
                    });
                }
            })();
        });

    $("#admin-nav li a.nav-link.active").removeClass("active");
    $("#forum-options").addClass("active");

    $('.del-badword').on('click',function(e)
    {
        if ($("#Badwords_" + $(this).data("id") + "__IsDeleted").val() == "True") {
            $(this).attr("title", "Flag for deletion");
            $("#Badwords_" + $(this).data("id") + "__IsDeleted").val("False");
            $('#badword_' + $(this).data('id') + " *").prop('readonly', false);
            $('#badword_' + $(this).data('id') + " input").css('border-color', '');
        } else {
            $(this).attr("title", "Remove delete flag");
            $("#Badwords_" + $(this).data("id") + "__IsDeleted").val("True");
            $('#badword_' + $(this).data('id') + " *").prop('readonly', true);
            $('#badword_' + $(this).data('id') + " input").css('border-color', 'red');
        }
    });
    function ArchiveStarted(xhr){
        $('#archiveModal').modal('hide');
        location.reload();
    }

