﻿// Disable form submissions if there are invalid fields
(function () {
    'use strict';
    window.addEventListener('load', function () {
        ValidateForms();
    }, false);
})();

/* Set up language variables */
let curLang = "en";
let langDirection = "ltr";
if ($.cookie("CookieLang")) {
    var storedCulture = $.cookie("CookieLang");
    // c=no|uic=no
    if (storedCulture !== '') {
        curLang = storedCulture;
    }
    if (curLang === "fa") {
        langDirection = "rtl";
    }

}


// Initialize the TinyMCE editor
tinymce.remove();
tinymce.init({
    selector: 'textarea#msg-text',
    menu: SnitzVars.favmenu,
    menubar: SnitzVars.menudef,
    toolbar: SnitzVars.toolbardef,
    language: curLang,
    directionality: langDirection,
/*  DO NOT EDIT BELOW THIS LINE */
    browser_spellcheck: true,
    contextmenu: false,
    plugins:  [
        'advlist autolink link lists code media',
        'searchreplace wordcount charmap hr insertdatetime paste',
        'table emoticons codesample'
    ],
    branding: false,
    relative_urls : false,
    remove_script_host : true,
    document_base_url : SnitzVars.forumUrl,
    convert_urls: true,
    formats: {
        blockquote: { block: 'blockquote', classes: 'newquote' },
        bold: { inline: 'span', classes: [ 'fw-bold' ] }
    },
    extended_valid_elements: 'i[class],div[*],',
    images_upload_url: SnitzVars.baseUrl + "/forumupload",
    images_upload_base_path: SnitzVars.contentFolder,
    images_reuse_filename: true,
    emoticons_database: 'emojiimages',
    emoticons_images_url: SnitzVars.baseUrl + '/images/emoticon/',
    content_css: SnitzVars.baseUrl +'/css/bootstrap.min.css,'+SnitzVars.baseUrl +'/lib/font-awesome/css/font-awesome.min.css,'+SnitzVars.baseUrl +'/css/site.min.css',
    content_css_cors: true,
    content_style: `body { font-family:Helvetica,Arial,sans-serif; font-size:14px; padding:1rem;} 
    .mce-content-body img {max-width: 99% !important;height: auto;} `,
    setup: function(editor) {
        if (SnitzVars.attach) {
            editor.ui.registry.addButton('snitzAttach',
                {
                    icon: 'new-document',
                    tooltip: SnitzVars.toolTips.fileBtn + '\n(' + SnitzVars.allowedfiles + ')',
                    onAction: function(_) {
                        $('#upload-content').html('');
                        $('#upload-content').load(SnitzVars.baseUrl + "/Topic/UploadForm/",function() {
                            $('#uploadModal').modal('show');
                        });
                    }
                });
        }
        if (SnitzVars.photoalbum) {
            editor.ui.registry.addButton('snitzImage',
                {
                    icon: 'gallery',
                    tooltip: SnitzVars.toolTips.albumBtn + '\n(' + SnitzVars.allowedimagetypes + ')',
                    onAction: function(_) {
                        $('#upload-content').html('');
                        $('#upload-content').load(SnitzVars.baseUrl + "/PhotoAlbum/UploadForm/?showall=false",function() {
                            $('#uploadModal').modal('show');
                        });
                    }
                });
        }
    }
});


