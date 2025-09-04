﻿$(document).ready(function() {
    $("time.timeago").timeago();
    $('*[data-autocomplete-url]').each(function () {
        $(this).autocomplete({
            source: $(this).data("autocomplete-url"),
            minLength: 3
        });
    });
    if (SnitzVars.pending > 0 && SnitzVars.hidePendingNotice == '0') {
        appendAlert("There are <a href=\"Admin/Members\" class=\"alert-link\">pending</a> member registrations.", 'warning');
        setTimeout(function () {
            $("#alertmessage").slideUp(500);
        }, 3000);
    }
});
window._link_was_clicked = false;
String.prototype.replaceAt=function(index, character) {
    return this.substr(0, index) + character + this.substr(index+character.length);
}
new bootstrap.Tooltip(document.body, {
    selector: "[data-toggle='tooltip']"
});
if ($.cookie("HideAnnounce")) {
    $("#alert-announce").hide();
}
$(document).on("click", "", function () {
    var date = new Date();
    date.setTime(date.getTime() + (10 * 60 * 1000));
    $.cookie('HideAnnounce', 'yes', {
        expires: date
    });
});
const alertPlaceholder = document.getElementById('liveAlertPlaceholder');
    

const appendAlert = (message, type) => {
    console.log("alert called");
    const wrapper = document.createElement('div')
    wrapper.innerHTML = [
        `<div id="alertmessage" class="alert alert-${type} alert-dismissible fade show" role="alert">`,
        `   <div style="padding: 5px;">`,
        `       <div>${message}!!</div>`,
        '       <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>',
        '   </div>',
        '</div>'
    ].join('');
    alertPlaceholder.append(wrapper);
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
    (async () => {
        const result = await b_confirm(Snitzres.cnfMergeTopic)
        if (result) {
            $.post(href,
                {
                    selected: selected
                },
                function (data) {
                    if (!data) {
                        appendAlert(data.error, 'error');;
                    } else {
                        location.reload();
                    }
                }
            );
        }
    })();

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
    (async () => {
        const result = await b_confirm('You are about to Restart the Application')
        if (result) {
            appendAlert('Application is restarting, please wait ... <i class=\"fa fa-spinner fa-pulse fa-2x fa-fw\"></i>', "danger");
            $.post(href, '',
                function (data, status) {
                    if (!data) {
                        appendAlert("There was a problem!", 'error');
                    } else {
                        setTimeout(function () {
                            $("#alertmessage .btn-close").click();
                            location.reload(true);
                        }, 25000);
                    }
                }
            );
        }
    })();
});

$(document).on('click', '.confirm-clearcache', function (e) {
    e.preventDefault();
    var href = $(this).attr('href');
    (async () => {
        const result = await b_confirm('Clear the cache')
        if (result) {
                
            appendAlert('Clearing the cache ... <i class=\"fa fa-spinner fa-pulse fa-2x fa-fw\"></i>', "warning");
            $.post(href, '',
                function (data, status) {
                    if (!data) {
                        appendAlert("There was a problem!", 'error');
                    } else {
                        setTimeout(function () {
                            $("#alertmessage .btn-close").click();
                            location.reload(true);
                        }, 20000);
                    }
                }
            );
        }
    })();
});

/* Busy Indicator */
$(document).on('submit', 'form', function () {
    console.log('form submit');
    displayBusyIndicator();
});

window.onbeforeunload = function (event) {

    if (window._link_was_clicked) {
        return; // abort beforeunload
    }
    displayBusyIndicator();
};

jQuery(document).on('click', 'a', function (event) {
    if ($(this).attr("rel")) {
        window._link_was_clicked = true;
    }
});

// Handle page reloads and back/forward navigation
$(document).ajaxComplete(function (event, xhr, settings) {
    console.log('ajax complete');
    $('.loading').hide();
});
/* * Display a busy indicator when the page is loading */
function displayBusyIndicator() {
    $('.loading').show();
}

window.addEventListener("pageshow", function (event) {
    var historyTraversal = event.persisted ||
        (typeof window.performance != "undefined" &&
            window.performance.navigation.type === 2);
    if (historyTraversal) {
        // Handle page restore.
        window.location.reload();
    }
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

async function b_confirm(msg) {
    const modalElem = document.createElement('div')
    modalElem.id = "modal-confirm"
    modalElem.className = "modal"
    modalElem.innerHTML = `
        <div class="modal-dialog modal-dialog-centered modal-dialog-scrollable">
            <div class="modal-content bg-confirm text-bg-confirm">
            <div class="modal-body fs-6">
                <p>${msg}</p>
                <p>${Snitzres.Confirm}</p>
            </div>
            <div class="modal-footer" style="border-top:0px">
            <button id="modal-btn-cancel" type="button" class="btn btn-success">${Snitzres.btnCancel}</button>
            <button id="modal-btn-accept" type="button" class="btn btn-danger">${Snitzres.btnAccept}</button>
            </div>
        </div>
        </div>
          `
    const myModal = new bootstrap.Modal(modalElem, {
        keyboard: false,
        //backdrop: 'static'
    })
    myModal.show()

    return new Promise((resolve, reject) => {
        document.body.addEventListener('click', response)

        function response(e) {
            let bool = false
            if (e.target.id == 'modal-btn-cancel') bool = false
            else if (e.target.id == 'modal-btn-accept') bool = true
            else return

            document.body.removeEventListener('click', response)
            myModal.hide()
            //document.body.querySelector('.modal-backdrop').remove()
            modalElem.remove()
            resolve(bool)
        }
    })
}