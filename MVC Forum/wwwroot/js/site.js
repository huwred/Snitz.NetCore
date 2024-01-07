$(document).ready(function() {
    $("time.timeago").timeago();
    $('*[data-autocomplete-url]').each(function () {
        $(this).autocomplete({
            source: $(this).data("autocomplete-url"),
            minLength: 3
        });
    });
});

String.prototype.replaceAt=function(index, character) {
    return this.substr(0, index) + character + this.substr(index+character.length);
}

function bbcodeinsert(starttag, endtag, textareaid) {
    var $txt = $("#" + textareaid);
            
    var textAreaTxt = $txt.val();
    var startPos = $txt[0].selectionStart;
    var endPos = $txt[0].selectionEnd;
    var sel = $txt.val().substring(startPos, endPos);

    $txt.val(textAreaTxt.replaceAt(startPos, starttag + sel + endtag) + textAreaTxt.substring(endPos));
}

$(document).on("click",".insert-emote", function () {
    var $txt = $("#msg-text");
    var caretPos = $txt[0].selectionStart;
    var textAreaTxt = $txt.val();
    var txtToAdd = $(this).data("code").trim();
    $txt.val(textAreaTxt.substring(0, caretPos) + txtToAdd + textAreaTxt.substring(caretPos) );
});

$(document).on("click",".btn-postform", function () {
    bbcodeinsert($(this).data("first"), $(this).data("last"), "msg-text");
});
$(document).on("click", ".cat-change",
    function() {
        location.href = "/Forum";
});

$(document).on("change", "#theme-change",
    function() {
        //Account/SetTheme/?theme=
        $.get( "/Account/SetTheme/?theme=" + $(this).val(), function( data ) {
            location.reload(true);
        });
    });

function ValidateForms() {
    // Get the forms we want to add validation styles to
    console.log("validation");
    var forms = document.getElementsByClassName('needs-validation');
    // Loop over them and prevent submission
    var validation = Array.prototype.filter.call(forms,
        function(form) {
            form.addEventListener('submit',
                function(event) {
                    //console.log("submit me");
                    //debugger;
                    if (form.checkValidity() === false) {
                        event.preventDefault();
                        event.stopPropagation();
                    }

                    form.classList.add('was-validated');
                },
                false);
        });


}
