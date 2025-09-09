$('.lazyloadtr').lazyload({
    // Sets the pixels to load earlier. Setting threshold to 200 causes image to load 200 pixels
    // before it appears on viewport. It should be greater or equal zero.
    threshold: 100,

    // Sets the callback function when the load event is firing.
    // element: The content in lazyload tag will be returned as a jQuery object.
    load: function (element) { },

    // Sets events to trigger lazyload. Default is customized event `appear`, it will trigger when
    // element appear in screen. You could set other events including each one separated by a space.
    trigger: "appear"
});
$(document).on('click', '#submitUpload',
    function (e) {
        e.preventDefault();
        var form = $("#upload-form");
        var formData = new FormData(form[0]);
        $.ajax({
            url: SnitzVars.baseUrl + $("#upload-form").attr("action"),
            type: "POST",
            data: formData,
            contentType: false,
            processData: false,
            success: function (data) {
                if (data.result) {
                    $('#uploadModal').modal('hide');
                    location.reload();
                }
            },
            error: function (data) {
                alert("error");
                console.log(data);
                $('#upload-content').html(data);
            },
            complete: function (data) {
                $('#uploadModal').modal('hide');
            }
        });
        return false;
    });
$(document).on("click", "#photo-upload",
    function (e) {
        e.preventDefault();
        $('#uploadModal .modal-title').html("Edit Image Details");
        $('#upload-content').load(SnitzVars.baseUrl + "/PhotoAlbum/UploadForm/?showall=true",
            function () {
                $('#uploadModal').modal('show');
            });
    });

$(document).on('click', '.confirm-privacy', function (e) {
    e.preventDefault();
    var href = $(this).attr('href');
    (async () => {
        const result = await b_confirm(Snitzres.cnfImagePrivacy)
        if (result) {
            $.post(href,
                function (data) {
                    if (!data) {
                        //appendAlert(data.error, 'error');;
                    } else {

                        $('#ImageContainer').html(data);
                    }
                }
            );
        }
    })();

});
$(document).on('click', '.confirm-feature', function (e) {
    e.preventDefault();
    var href = $(this).attr('href');
    (async () => {
        const result = await b_confirm(Snitzres.cnfImageFeature)
        if (result) {
            $.post(href,
                function (data) {
                    if (!data) {
                        //appendAlert(data.error, 'error');;
                    } else {

                        $('#ImageContainer').html(data);
                    }
                }
            );
        }
    })();

});

$(document).on('click', '.edit-image', function (e) {
    e.preventDefault();
    var dataid = $(this).data('id');
    var display = $(this).data('display');
    $('#editimage-content').html('');
    $('#editimage-content').load(SnitzVars.baseUrl + "/PhotoAlbum/Edit/" + dataid + "/?display=" + display, function () {
        $('#editModal').modal('show');
    });
});

$(document).on('click', '.confirm-delimage', function (e) {
    e.preventDefault();
    var href = $(this).attr('href');
    (async () => {
        const result = await b_confirm(Snitzres.cnfDeleteImage)
        if (result) {
            $.post(href.replace("~", SnitzVars.baseUrl),
                function (data) {
                    if (!data) {
                        //appendAlert(data.error, 'error');;
                    } else {

                        $('#ImageContainer').html(data);
                    }
                }
            );
        }
    })();

});

function EditSuccess() {
    $('#editModal').modal('hide');
}
