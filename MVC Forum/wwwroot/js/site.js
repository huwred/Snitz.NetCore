$(document).ready(function() {
    $("time.timeago").timeago();
    $('*[data-autocomplete-url]').each(function () {
        $(this).autocomplete({
            source: $(this).data("autocomplete-url"),
            minLength: 3
        });
    });
    if (SnitzVars.pending > 0) {
        $('#forumAlert').modal('show');
        setTimeout(function () {
            $('#forumAlert').modal('hide');
        }, 3000);
    }
});

String.prototype.replaceAt=function(index, character) {
    return this.substr(0, index) + character + this.substr(index+character.length);
}
new bootstrap.Tooltip(document.body, {
    selector: "[data-toggle='tooltip']"
});
const alertPlaceholder = document.getElementById('liveAlertPlaceholder')
const appendAlert = (message, type) => {
    const wrapper = document.createElement('div')
    wrapper.innerHTML = [
        `<div class="alert alert-${type} alert-dismissible position-absolute top-50 start-50 translate-middle" role="alert">`,
        `   <div>${message}</div>`,
        '   <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>',
        '</div>'
    ].join('')

    alertPlaceholder.append(wrapper)
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
/* Merge Topics */
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
                if (!data) {
                    appendAlert(data.error, 'error');;
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
/* Restart confirmation */
$(document).on('click', '.confirm-restart', function (e) {
    e.preventDefault();
    var href = $(this).attr('href');
    $('#confirmRestart').data('url', href).modal('show');

    $('#confirmRestart').on('click', '#btnRestartYes', function (e) {
        e.preventDefault();
        $.post(href, '',
            function (data, status) {
                if (!data) {
                    appendAlert("There was a problem!", 'error');
                } else {
                    $('#confirmRestart .modal-body').html("<p>Application is restarting, please wait ...</p>");
                    $('#btnRestartYes').hide();
                    setTimeout(function () {
                        $('#confirmRestart').modal('hide');
                        location.reload(true);
                    }, 25000);

                }
            });
    });
});
$(document).on('click', '.confirm-clearcache', function (e) {
    e.preventDefault();
    var href = $(this).attr('href');
    $('#confirmRestart').data('url', href).modal('show');

    $('#confirmRestart').on('click', '#btnRestartYes', function (e) {
        e.preventDefault();
        $.post(href, '',
            function (data, status) {
                if (!data) {
                    appendAlert("There was a problem!", 'error');
                } else {
                    $('#confirmRestart .modal-body').html("<p>Clearing Cache, please wait ...</p>");
                    $('#btnRestartYes').hide();
                    setTimeout(function () {
                        $('#confirmRestart').modal('hide');
                        location.reload(true);
                    }, 5000);

                }
            });
    });
});

/* Busy Indicator */
$(document).on('submit', 'form', function () {
    displayBusyIndicator();
});
$(window).on('beforeunload', function () {
    displayBusyIndicator();
});
// Handle page reloads and back/forward navigation

window.addEventListener("pageshow", function (event) {
    var historyTraversal = event.persisted ||
        (typeof window.performance != "undefined" &&
            window.performance.navigation.type === 2);
    if (historyTraversal) {
        // Handle page restore.
        window.location.reload();
    }
});

$(document).ajaxComplete(function (event, xhr, settings) {
    $('.loading').hide();
});

/* * Show the page load time in the footer
 * This is only shown if the showPageTimer variable is set to 1
 */
if (SnitzVars.showPageTimer == '1') {
    window.addEventListener('load', () => {
        const [pageNav] = performance.getEntriesByType('navigation');
        const footer = document.getElementById('loadTime');

        let workerTime = 0;

        if (pageNav.responseEnd > 0) {
            workerTime = (pageNav.responseEnd - pageNav.workerStart) / 1000;

            if (footer) {
                var test = (workerTime).toLocaleString(
                    undefined, // leave undefined to use the visitor's browser 
                    // locale or a string like 'en-US' to override it.
                    { minimumFractionDigits: 2 }
                );
                footer.textContent = `Page loaded in ${test} s`;
            }

        }
    });
}

/* * Display a busy indicator when the page is loading */
function displayBusyIndicator() {
    $('.loading').show();
}
/* * Validate forms with the class 'needs-validation'
 * This function prevents form submission if the form is invalid
 */
function ValidateForms() {
    // Get the forms we want to add validation styles to

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


}

/* * Insert BBCode tags around the selected text in the textarea
 * @param {string} starttag - The opening BBCode tag
 * @param {string} endtag - The closing BBCode tag
 * @param {string} textareaid - The ID of the textarea to modify
 */
function bbcodeinsert(starttag, endtag, textareaid) {
    var $txt = $("#" + textareaid);

    var textAreaTxt = $txt.val();
    var startPos = $txt[0].selectionStart;
    var endPos = $txt[0].selectionEnd;
    var sel = $txt.val().substring(startPos, endPos);

    $txt.val(textAreaTxt.replaceAt(startPos, starttag + sel + endtag) + textAreaTxt.substring(endPos));
}
