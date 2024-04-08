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
        location.href = SnitzVars.baseUrl + "/Forum";
});

$(document).on("change", "#theme-change",
    function() {
        //Account/SetTheme/?theme=
        $.get( SnitzVars.baseUrl + "/Account/SetTheme/?theme=" + $(this).val(), function( data ) {
            location.reload(true);
        });
    });

function ValidateForms() {
    // Get the forms we want to add validation styles to

    var forms = document.getElementsByClassName('needs-validation');
    // Loop over them and prevent submission
    var validation = Array.prototype.filter.call(forms,
        function(form) {
            form.addEventListener('submit',
                function(event) {
                    if (form.checkValidity() === false) {
                        event.preventDefault();
                        event.stopPropagation();
                    }

                    form.classList.add('was-validated');
                },
                false);
        });


}
/*update the session topiclist if checkbox selected*/
$(document).on('mouseup','.topic-select', function () {
    $.ajax({
        type: "POST",
        url: SnitzVars.baseUrl + "/Topic/UpdateTopicList/?id=" + $(this).val(),
        data: { topicid: $(this).val() },
        cache: false
    });
});
/*hide the Merge button if < 2 checkboxes selected*/
$(document).on('change','.topic-select', function () {
    var checkedNum = $('input[name="topicselected"]').filter(":checked").length;
    if (checkedNum <2) {
        $('.fa-object-group').hide();
    } else if (checkedNum>1){
        $('.fa-object-group').show();
    }

});
$(document).on('click','.fa-object-group',function(e) {
    e.preventDefault();
    var selected = [];
    $('input[name="topicselected"]').filter(":checked").each(function() {
        selected.push($(this).val());
    });
    //remove default click event
    $(document).off('click', '#btnYes');
    var href = SnitzVars.baseUrl + '/Topic/Merge';
    //
    $('#confirmModal .text-bg-warning').html('Merge Topics');
    $('#confirmModal #confirm-body').html('<p>You are about to Merge the select Topics.</p><p>Do you wish to proceed?</p>');
    $('#confirmModal').data('id', 0).data('url', href).modal('show');
    $('#confirmModal').one('click','#btnYes',function(e) {
        // handle deletion here
        e.preventDefault();
        $.post(SnitzVars.baseUrl + '/Topic/Merge',
            {
                selected: selected
            },
            function(data) {
                console.log(data);
                if (!data) {
                    alert(data.error);
                } else {
                    location.reload();
                }
            });
    });
});

/*update the session replylist if checkbox selected*/
$(document).on('mouseup','.reply-select', function () {

    $.ajax({
        type: "POST",
        url: SnitzVars.baseUrl + "/Topic/UpdateReplyList",
        data: { replyid: $(this).val() },
        cache: false
    });
});
$(document).on('change','.reply-select', function () {
    var checkedNum = $('input[name="replyselected"]').filter(":checked").length;
    if (checkedNum <1) {
        $('.fa-object-ungroup').hide();
    } else if (checkedNum>0){
        $('.fa-object-ungroup').show();
    }

});