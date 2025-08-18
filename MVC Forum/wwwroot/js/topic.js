$.extend({
    getUrlParams: function () {
        var vars = [], hash;
        var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
        for (var i = 0; i < hashes.length; i++) {
            hash = hashes[i].split('=');
            vars.push(hash[0]);
            vars[hash[0]] = hash[1];
        }
        return vars;
    },
    getUrlParam: function (name) {
        return $.getUrlParams()[name];
    }
});        
$(document).ready(function() {
    var replyid = $.getUrlParam('replyid');
    if (replyid) {
        $([document.documentElement, document.body]).animate({
            scrollTop: $("#reply-" + replyid).offset().top
        }, 300);
    } else {
        $(window).scroll(lazyload);

    }
    $(".fig-caption").each(function() {
        var test = $(this);
        $.ajax({
            url: SnitzVars.baseUrl + "/PhotoAlbum/GetCaption/" + $(this).data("id"),
            type: "GET",
            success: function(data) {
                //alert(data); // the View
                test.html(data);
            }
        });
    });
});

$('.modal-link').on('click', function (e) {
    e.preventDefault();
    $.get($(this).attr("href"),function(data) {
        $("#posModeration").html(data);
        $("#moderateModal").modal('show');
    });
});
$('.send-email').on('click', function (e) {
    e.preventDefault();
    var userid = $(this).data('id');
    $.get(SnitzVars.baseUrl +  + '/Account/EmailMember/' + userid, function (data) {
        $('#emailContainer').html(data);
        $.validator.unobtrusive.parse($("#emailMemberForm"));
        $('#emailModal').modal('show');

    });
});
//$('.sendpm-link').on('click', function () {
//    var username = $(this).data('id');
//    $.get(SnitzVars.baseUrl +  + '/PrivateMessage/SendMemberPm/' + username, function (data) {
//        $('#pmContainer').html(data);
//        $.validator.unobtrusive.parse($("#sendPMForm"));
//        $('#modal-sendpm').modal('show');
//    });
//});
$(document).on("click","#PrintTopic",function(e) {
    e.preventDefault();
    var id = $(this).data('id');
    var archive = $.getUrlParam('archived');
    location.href = SnitzVars.baseUrl + '/Topic/Print/' + id + '?archived=' + archive;

});
$(document).on('click','.sendto-link', function (e) {
        e.preventDefault();
    var id = $(this).data('id');
    var archive = $.getUrlParam('archived');
    $.get(SnitzVars.baseUrl + '/Topic/SendTo/' + id + '?archived=' + archive, function (data) {
        $('#sendToContainer').html(data);
        $.validator.unobtrusive.parse($("#sendToForm"));
        $('#modal-sendto').modal('show');
    });
});
$(document).on('click', '#submitUpload', function(e) {
    e.preventDefault();
    var form = $("#upload-form");
    var formData = new FormData(form[0]);
    $.ajax({
        url: SnitzVars.baseUrl + $("#upload-form").attr("action"),
        type: "POST",
        data: formData,
        contentType: false,
        processData: false,
        success: function(data) {
            if (data.result) {
                if (!data.type) {
                    var img = "[image=" + data.id + "]";
                    if (data.caption) {
                        img = "[cimage=" + data.id + "]";
                    }
                    tinymce.activeEditor.execCommand('mceInsertContent', false, img);
                } else {
                    //alert(data.type);
                    var file = "[file " + data.filesize + "]" + data.data + "[/file]";
                    if (data.type === ".pdf") {
                        file = "[pdf]" + data.data + "[/pdf]";
                    } else if (data.type === ".jpg" || data.type === ".png" || data.type === ".jpeg" || data.type === ".webp") {
                        file = "[img]" + data.data + "[/img]";
                    }

                    tinymce.activeEditor.execCommand('mceInsertContent', false, file);
                }

                $('#uploadModal').modal('hide');
            }

        },
        error: function(data) {
            alert("error");
            $('#upload-content').html(data);
        },
        complete: function(data) {
            $('#uploadModal').modal('hide');
        }

    });
    return false;
});
$(document).on("click", ".email-link", function(e) {
        e.preventDefault();
        var memberid = $(this).data("id");
        var href = $(this).data("url");
        $('#memberModal .modal-title').html('Send Member an Email');
        $('#member-modal').load(href + "/" + memberid, function() {});
        $('#memberModal .modal-footer').show();

        $(document).on("click",
            "#send-email",
            function(e) {
                e.preventDefault();
                $(this).prop('disabled', true);
                var post_url = $('#sendemail-form').attr("action");
                var form_data = $('#sendemail-form').serialize();
                $.ajax({
                    url: post_url,
                    type: "POST",
                    data: form_data
                }).done(function(response) {
                    if (response.result) {
                        $('#memberModal').modal('hide');
                    } else {
                        //Dont close the modal, display the error
                        $('#member-modal').html(response);
                    }
                });

            });
        $('#memberModal').data('id', memberid).modal('show');
    });
$(document).on("click", ".sendpm-link", function(e) {
        e.preventDefault();
        var memberid = $(this).data("id");
        var href = $(this).data("url");
        $('#pmModal .modal-title').html('Send private message');
        $('#pm-modal').load(href + "/" + memberid, function() {});
        //$('#memberModal #btnOk').hide();

        $(document).on("click",
            "#send-pm",
            function(e) {
                e.preventDefault();
                $(this).prop('disabled', true);
                var post_url = $('#sendpm-form').attr("action");
                var form_data = $('#sendpm-form').serialize();
                $.ajax({
                    url: post_url,
                    type: "POST",
                    data: form_data
                }).done(function(response) {
                    if (response.result) {
                        $('#pmModal').modal('hide');
                    } else {
                        //Dont close the modal, display the error
                        $('#pm-modal').html(response);
                    }
                });

            });
        $('#pmModal').data('id', memberid).modal('show');

    });
$(document).on("click", ".show-ip", function(e) {
        e.preventDefault();
        var memberid = $(this).data("id");
        var href = $(this).data("url");
        $('#memberModal .modal-title').html('Member\'s IP Address');
        $('#member-modal').load(href + "/" + memberid, function() {});
        $('#memberModal').data('id', memberid).modal('show');
        $('#memberModal .modal-footer').hide();
    });


$(document).on("change", "#aFile_upload", function (e) {
        var filesize = ((this.files[0].size / 1024) / 1024).toFixed(4); // MB
        var maxsize = parseInt('@SnitzConfig.GetValue("INTMAXFILESIZE", "5")');
        if (filesize > maxsize) {
            alert("File is too big!");
            this.value = "";
        }
    });
$(document).on("change", "input[type=radio][name=pagesize]", function () {
        $("#defaultdays-form").submit();
    });

Prism.hooks.add("before-highlight",
    function(env) {
        env.code = env.element.innerText;
    });

code = document.getElementsByTagName('code');
Array.from(code).forEach(el => { Prism.highlightElement(el) });

function lazyload(){
    var wt = $(window).scrollTop();    //* top of the window
    var wb = wt + $(window).height();  //* bottom of the window
    $(".reply-card").each(function () {
        var ot = $(this).offset().top;  //* top of object (i.e. advertising div)
        var ob = ot + $(this).height(); //* bottom of object
        if(!$(this).attr("loaded") && wt<=ob && wb >= ot && ot !=0) {
            $(this).children().show();
            $(this).attr("loaded",true);
        }
    });
}