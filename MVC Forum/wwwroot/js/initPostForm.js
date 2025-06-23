// Disable form submissions if there are invalid fields
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

/* Toolbar/Menu default */
let toolbardef = 
'fileButton imageButton media | undo redo | styleselect | bold italic | alignleft aligncenter alignright | bullist numlist | link emoticons | code codesample';

let menudef = 'favs edit insert format table help';

/* Quick reply editor overrides */
if ($('#QuickReply').length > 0) {
    menudef = false; // turn off the menu bar
    toolbardef =
    'fileButton imageButton media | undo redo | styleselect | bullist numlist | link emoticons | code codesample';

}

// Initialize the TinyMCE editor
tinymce.remove();
tinymce.init({
    selector: 'textarea#msg-text',
    menu: {
        favs: {title: 'My Favourites', items: 'code | searchreplace | emoticons'}
    },
    menubar: menudef,
    toolbar: toolbardef,
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
    images_upload_base_path: window.ContentFolder,
    images_reuse_filename: true,
    emoticons_database: 'emojiimages',
    emoticons_images_url: SnitzVars.baseUrl + '/images/emoticon/',
    content_css: SnitzVars.baseUrl +'/css/bootstrap.min.css,'+SnitzVars.baseUrl +'/lib/font-awesome/css/font-awesome.min.css,'+SnitzVars.baseUrl +'/css/site.min.css',
    content_css_cors: true,
    content_style: `body { font-family:Helvetica,Arial,sans-serif; font-size:14px; padding:1rem;} 
    .mce-content-body img {max-width: 99% !important;height: auto;} `,
    setup: function(editor) {
        if (attach) {
            editor.ui.registry.addButton('fileButton',
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
        if (photoalbum) {
            editor.ui.registry.addButton('imageButton',
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


