// Disable form submissions if there are invalid fields
(function () {
    'use strict';
    window.addEventListener('load', function () {
        ValidateForms();
    }, false);
})();
tinymce.remove();
tinymce.init({
    selector: 'textarea#msg-text',
    height: 200,
    browser_spellcheck: true,
    contextmenu: false,
    plugins: 'bbcode code emoticons codesample image link lists anchor',
    branding: false,
    relative_urls : false,
    remove_script_host : true,
    document_base_url : window.location.protocol + "//" + window.location.host + "/",
    convert_urls: true,
    toolbar: 'code undo redo | styleselect | bullist numlist | indent outdent | link codesample emoticons',
    file_picker_types: "image",
    images_upload_url: "/forumupload",
    images_reuse_filename: true,
    emoticons_database: 'emojiimages',
    emoticons_images_url: '/images/emoticon/',
    content_style: `body { font-family:Helvetica,Arial,sans-serif; font-size:14px; } 
    .mce-content-body img {max-width: 99% !important;height: auto;} `
});