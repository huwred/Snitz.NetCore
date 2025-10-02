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
    $('#rating-id').on("rating:change", function (event, value, caption) {
        var form = $('#save-rating');
        $.ajax({
            url: SnitzVars.baseUrl + '/Topic/SaveRating',
            type: "POST",
            data: form.serialize(),
            success: function (data) {
                $('#rating-id').rating("refresh", { disabled: true, showClear: false });
            },
            error: function (err) {
                console.log(err);
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

$("#QuickReply").submit(function (e) {
    e.preventDefault();
    $("#btn-submit").prop("disabled", true);
    window.displayBusyIndicator();
    tinyMCE.get("msg-text").save();
    var form = $("#QuickReply");
    var formData = new FormData(form[0]);
    $.ajax({
        url: $(this).attr("action"),
        type: "POST",
        data: formData,
        contentType: false,
        processData: false,
        success: function (data) {
            window.displayBusyIndicator();
            location.href = data.url;// + '?page=-1';
        },
        error: function (err) {
            qrappendAlert(SnitzVars.floodErr, 'danger');
            $('.loading').hide();
        }
    });
    return false;
});

$('#SendTopic').on('click', function (e) {
    e.preventDefault();
    var id = $(this).data('id');
    var archive = $.getUrlParam('archived');
    $.get(SnitzVars.baseUrl + '/Topic/SendTo/' + id + '?archived=' + archive, function (data) {
        $('#sendToContainer').html(data);
        $.validator.unobtrusive.parse($("#sendToForm"));
        $('#modal-sendto').modal('show');
    });
});

$('.reply-select').on('change', function () {
    var checkedNum = $('input[name="replyselected"]').filter(":checked").length;
    if (checkedNum < 1) {
        $('.fa-clone').hide();
    } else if (checkedNum > 0) {
        $('.fa-clone').show();
    }

});

$('#submitUpload').on('click', function(e) {
    e.preventDefault();
    var form = $("#upload-form");
    var formData = new FormData(form[0]);
    $.ajax({
        url: $("#upload-form").attr("action"),
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

$("#theme-change").on("change",
    function () {
        $.get(SnitzVars.baseUrl + "/Account/SetTheme/?theme=" + $(this).val(), function (data) {
            location.reload(true);
        });
    });

$('body').off().on("click", ".email-link", function(e) {
        e.preventDefault();
        var memberid = $(this).data("id");
        var href = $(this).data("url");
        $('#memberModal .modal-title').html('Send Member an Email');
        $('#member-modal').load(href + "/" + memberid, function() {});
        $('#memberModal .modal-footer').show();
        $(":reset").on("click", function () {
            $('#send-email').prop('disabled', false);
        });
        $('body').on("click",
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

$('body').on("click", ".sendpm-link", function(e) {
        e.preventDefault();
        var memberid = $(this).data("id");
        var href = $(this).data("url");
        $('#pmModal .modal-title').html('Send private message');
        $('#pm-modal').load(href + "/" + memberid, function () {
            var forms = document.getElementsByClassName('needs-validation');
            // Loop over them and prevent submission
            var validation = Array.prototype.filter.call(forms,
                function (form) {
                    form.addEventListener('submit',
                        function (event) {
                            if (form.checkValidity() === false) {
                                event.preventDefault();
                                event.stopPropagation();
                            }

                            form.classList.add('was-validated');
                        },
                        false);
                });
            $(":reset").on("click", function () {
                $('#send-pm').prop('disabled', false);
            });
            $('body').on("click", "#send-pm",
                function (e) {
                    e.preventDefault();
                    $(this).prop('disabled', true);
                    var post_url = $('#sendpm-form').attr("action");
                    var form_data = $('#sendpm-form').serialize();
                    $.ajax({
                        url: post_url,
                        type: "POST",
                        data: form_data
                    }).done(function (response) {
                        if (response.result) {
                            $('#pmModal').modal('hide');
                        } else {
                            //Dont close the modal, display the error
                            $('#pm-modal').html(response);
                        }
                    });

                });

        });



        $('#pmModal').data('id', memberid).modal('show');

});

$('body').on("click", ".show-ip", function(e) {
        e.preventDefault();
        var memberid = $(this).data("id");
        var href = $(this).data("url");
        $('#memberModal .modal-title').html('Member\'s IP Address');
        $('#member-modal').load(href + "/" + memberid, function() {});
        $('#memberModal').data('id', memberid).modal('show');
        $('#memberModal .modal-footer').hide();
    });

$('body').on("change", "#aFile_upload", function (e) {
        var filesize = ((this.files[0].size / 1024) / 1024).toFixed(4); // MB
        var maxsize = parseInt('@SnitzConfig.GetValue("INTMAXFILESIZE", "5")');
        if (filesize > maxsize) {
            alert("File is too big!");
            this.value = "";
        }
});

$('body').on("change", "input[type=radio][name=pagesize]", function () {
        $("#defaultdays-form").submit();
});

$(document).on("click", "#PrintTopic", function (e) {
    e.preventDefault();
    var id = $(this).data('id');
    var archive = $.getUrlParam('archived');
    location.href = SnitzVars.baseUrl + '/Topic/Print/' + id + '?archived=' + archive;

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
function TopicRated() {
    $('.topic_rating').hide();
    $('#input-id-@Model.Id').rating("refresh", { disabled: true, showClear: false });
}

document.querySelectorAll('.star-rating:not(.readonly) label').forEach(star => {
    star.addEventListener('click', function () {
        this.style.transform = 'scale(1.2)';
        setTimeout(() => {
            this.style.transform = 'scale(1)';
        }, 200);
    });
});

