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
$('#cat-select').on("change", function (e) {
    $(this).closest('form').trigger('submit');
});
$('#submitUpload').on('click', 
    function (e) {
        e.preventDefault();
        var form = $("#upload-form");
        if (form[0].checkValidity() === false) {
            e.preventDefault();
            e.stopPropagation();
            form[0].classList.add('was-validated');
            return false;
        }

        form[0].classList.add('was-validated');
        var formData = new FormData(form[0]);
        $.ajax({
            url: $("#upload-form").attr("action"),
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
$("#photo-upload").on("click", 
    function (e) {
        e.preventDefault();
        //$('#uploadModal .modal-title').html("Edit Image Details");
        $('#upload-content').load(SnitzVars.baseUrl + "/PhotoAlbum/UploadForm/?showall=true",
            function () {
                $('#uploadModal').modal('show');
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
            });
    });

$('body').on('click', '.confirm-privacy', function (e) {
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
$('body').on('click', '#category-add', function (e) {
    e.preventDefault();
    $('#addcategory-content').html('');
    $('#addcategory-content').load(SnitzVars.baseUrl + "/PhotoAlbum/AddCategory/", function () {
        $('#addImageCat').modal('show');
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
    });

});
$('body').on('click', '.confirm-feature', function (e) {
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

$('body').on('click', '.edit-image', function (e) {
    e.preventDefault();
    var dataid = $(this).data('id');
    var display = $(this).data('display');
    $('#editimage-content').html('');
    $('#editimage-content').load(SnitzVars.baseUrl + "/PhotoAlbum/Edit/" + dataid + "/?display=" + display, function () {
        $('#editModal').modal('show');
    });
});

$('body').on('click', '.confirm-delimage', function (e) {
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

function refreshlist() {
    
}
function EditSuccess() {
    $('#editModal').modal('hide');
    location.reload(true);
}
