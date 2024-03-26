// Disable form submissions if there are invalid fields
(function () {
    'use strict';
    window.addEventListener('load', function () {
        ValidateForms();
    }, false);
})();
let curLang = "en";
let langDirection = "ltr";
if ($.cookie(".AspNetCore.Culture")) {
    var storedCulture = $.cookie(".AspNetCore.Culture");
    // c=no|uic=no
    if (storedCulture !== '') {
        var cultureArray = storedCulture.split('|');
        var culture = cultureArray[0].split('=');
        curLang = culture[1];
    }
    console.log(curLang);
    if (curLang === "fa") {
        langDirection = "rtl";
    }

}

tinymce.remove();
tinymce.init({
    selector: 'textarea#msg-text',
    menu: {
        favs: {title: 'My Favorites', items: 'code | searchreplace | emoticons'}
    },
    menubar: 'favs edit insert format table help',
    language: curLang,
    directionality: langDirection,
/*    height: 200,*/
    browser_spellcheck: true,
    contextmenu: false,
    plugins:  [
        'advlist autolink link lists charmap hr',
        'searchreplace wordcount code insertdatetime media',
        'table emoticons paste help'
    ],
    branding: false,
    relative_urls : false,
    remove_script_host : true,
    document_base_url : window.location.protocol + "//" + window.location.host + "/",
    convert_urls: true,
        toolbar: 'undo redo | styleselect | bold italic | alignleft aligncenter alignright | ' +
            'bullist numlist | link emoticons | fileButton imageButton media code',
    //toolbar: 'code undo redo | removeformat | styleselect | bullist numlist table | blockquote  | link codesample emoticons | fileButton imageButton media',
    images_upload_url: "/forumupload",
    images_upload_base_path: "/Content/PhotoAlbum/",
    images_reuse_filename: true,
    emoticons_database: 'emojiimages',
    emoticons_images_url: '/images/emoticon/',
    content_style: `body { font-family:Helvetica,Arial,sans-serif; font-size:14px; } 
    .mce-content-body img {max-width: 99% !important;height: auto;} `,
    setup: function(editor) {
        if (attach) {
            editor.ui.registry.addButton('fileButton',
                {
                    icon: 'new-document',
                    tooltip: 'Attach file to post',
                    onAction: function(_) {
                        $('#upload-content').html('');
                        $('#upload-content').load("/Topic/UploadForm/",function() {
                            $('#uploadModal').modal('show');
                        });
                    }
                });
        }
        if (photoalbum) {
            editor.ui.registry.addButton('imageButton',
                {
                    icon: 'gallery',
                    tooltip: 'Upload image to your album',
                    onAction: function(_) {
                        $('#upload-content').html('');
                        $('#upload-content').load("/PhotoAlbum/UploadForm/?showall=false",function() {
                            $('#uploadModal').modal('show');
                        });
                    }
                });
        }
    }
});
