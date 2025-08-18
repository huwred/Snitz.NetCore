// Disable form submissions if there are invalid fields
(function () {
    'use strict';
    window.addEventListener('load', function () {
        ValidateForms();
        revalidate();
    }, false);
})();
function revalidate() {
    var container = document.getElementById("reply-area");
    if (container) {
        var forms = container.getElementsByTagName("form");
        var newForm = forms[forms.length - 1];
        $.validator.unobtrusive.parse(newForm);
    }
}
$(document).on("change",
    "#sortdir",
    function () {
        $("#defaultdays-form").submit();
    });

/* post button events */

// Delete Reply
$(document).on("click",".reply-del", function() {
    var postid = $(this).data("id");
    var archived = $(this).data("archived");
    (async () => {
        const result = await b_confirm(Snitzres.cnfDeleteReply)
        if (result) {
            $.post(SnitzVars.baseUrl + "/Reply/Delete",
                {
                    id: postid,
                    archived: archived
                },
                function (data, status) {
                    if (!data.result) {
                        appendAlert(data.error, 'error');
                    } else {
                        location.reload(true);
                    }
                }
            );
        }
    })();

});
// Delete Post
$(document).on("click",".post-del", function(e) {
    e.preventDefault();
    var postid = $(this).data("id");
    var href = $(this).attr('href');
    var archived = $(this).data("archived");
    (async () => {
        const result = await b_confirm(Snitzres.cnfDeleteTopic)
        if (result) {
            $.post(SnitzVars.baseUrl + "/Topic/Delete",
                {
                    id: postid,
                    archived: archived
                },
                function (data, status) {
                    if (!data.result) {
                        appendAlert(data.error, 'error');
                    } else {
                        location.href = data.url;
                    }
                }
            );
        }
    })();
});
// Lock Member
$(document).on("click",".lock-member", function(e) {
    e.preventDefault();
    var memberid = $(this).data("id");
    var href = $(this).data("url");
    (async () => {
        const result = await b_confirm(Snitzres.cnfLockMember)
        if (result) {
            $.post(href,
                {
                    id: memberid
                },
                function (data, status) {

                    if (!data.result) {
                        appendAlert(data.error, 'error');
                    } else {
                        location.reload();
                    }
                }
            );
        }
    })();

});
// Bookmark Post
$(document).on('click',
    '.bookmark-add',
    function(e) {
        e.preventDefault();
        var href = $(this).attr('href');
        var postid = $(this).data("id");
        (async () => {
            const result = await b_confirm(Snitzres.cnfBookmarkPost)
            if (result) {
                $.post(SnitzVars.baseUrl + "/Bookmark/Create",
                    {
                        id: postid
                    },
                    function (data, status) {
                        if (!data.result) {
                            appendAlert(data.error, 'error');
                        } else {

                            location.reload(true);
                        }
                    }
                );
            }
        })();

});
// Delete Bookmark
$(document).on('click',
    '.bookmark-del',
    function(e) {
        e.preventDefault();
        var href = $(this).attr('href');
        var postid = $(this).data("id");
        (async () => {
            const result = await b_confirm(Snitzres.cnfDeleteBookmark)
            if (result) {
                $.post(SnitzVars.baseUrl + "/Bookmark/Delete",
                    {
                        id: postid
                    },
                    function (data, status) {
                        if (!data.result) {
                            appendAlert(data.error, 'error');
                        } else {

                            location.reload(true);
                        }
                    }
                );
            }
        })();
 
    });
// Lock Post
$(document).on('click', '.post-lock', function (e) {
    e.preventDefault();
    var postid = $(this).data("id");
    var poststatus = $(this).data("status");
    (async () => {
        const result = await b_confirm(Snitzres.cnfToggleStatus)
        if (result) {
            $.post(SnitzVars.baseUrl + "/Topic/LockTopic",
                {
                    id: postid,
                    status: poststatus
                },
                function (data, status) {
                    if (!data.result) {
                        appendAlert(data.error, 'error');
                    } else {

                        location.reload(true);
                    }
                }
            );
        }
    })();
        
});
// Stick Post
$(document).on('click', '.post-stick', function (e) {
    e.preventDefault();

    var href = $(this).attr('href');
    var postid = $(this).data("id");
    var poststatus = $(this).data("status");
    (async () => {
        const result = await b_confirm(Snitzres.cnfToggleSticky)
        if (result) {
            $.post(SnitzVars.baseUrl + "/Topic/MakeSticky",
                {
                    id: postid,
                    state: poststatus
                },
                function (data, status) {
                    if (!data.result) {
                        appendAlert(data.error, 'error');
                    } else {

                        location.reload(true);
                    }
                }
            );
        }
    })();
       
});
// Quote Reply
$(document).on("click",".reply-quote", function() {
    var postid = $(this).data("id");
    location.href = SnitzVars.baseUrl + "/Reply/Quote/" + postid;
});
// Quote Post
$(document).on("click",".post-quote", function() {
    var postid = $(this).data("id");
    location.href = SnitzVars.baseUrl + "/Topic/Quote/" + postid;
});
// Reply to Post
$(document).on("click",".post-reply", function() {
    var postid = $(this).data("id");
    location.href = SnitzVars.baseUrl + "/Reply/Create/" + postid;
});
// Edit Reply
$(document).on("click",".reply-edit", function() {

    var postid = $(this).data("id");
    archived = $(this).data("archived");
    location.href = SnitzVars.baseUrl + "/Reply/Edit/" + postid + "?archived=" + archived;
});
// Mark as Answer
$(document).on('click', '.reply-answer', function (e) {
    e.preventDefault();

    var href = $(this).attr('href');
    var postid = $(this).data("id");
    var poststatus = $(this).data("status");
    (async () => {
        const result = await b_confirm(Snitzres.cnfMarkAnswer)
        if (result) {
            $.post(SnitzVars.baseUrl + "/Topic/Answered",
                {
                    id: postid,
                    status: poststatus
                },
                function (data, status) {
                    if (!data.result) {
                        appendAlert(data.error, 'error');
                    } else {

                        location.reload(true);
                    }
                }
            );
        }
    })();
        
});
// Edit Post
$(document).on("click",".post-edit", function() {

    var postid = $(this).data("id");
    archived = $(this).data("archived");
    location.href = SnitzVars.baseUrl + "/Topic/Edit/" + postid + "?archived=" + archived;

});
// Thumbs Up
$(document).on('click',
    '.thumbs-up',
    function(e) {
        e.preventDefault();
        var id = $(this).data('id');
        var replyid = $(this).data('replyid');
            $.post(SnitzVars.baseUrl + "/PostThanks/Thank",
                {
                    id: id,
                    replyid:replyid
                },
                function(data, status){
                    if (!data.result) {
                        appendAlert(data.error, 'error');
                    } else {

                        location.reload(true);
                    }
                });
        });
// Thumbs Down
$(document).on('click',
    '.thumbs-down',
    function(e) {
        e.preventDefault();
        var id = $(this).data('id');
        var replyid = $(this).data('replyid');
            $.post(SnitzVars.baseUrl + "/PostThanks/UnThank",
                {
                    id: id,
                    replyid:replyid
                },
                function(data, status){
                    if (!data.result) {
                        appendAlert(data.error, 'error');
                    } else {

                        location.reload(true);
                    }
                });
        });
// Show/Hide Thanks List
$( ".thanks-list" )
  .on( "mouseenter", function() {
    var e=$(this);
    var id = e.data('id');
    var replyid = e.data('replyid');

    $.ajax(
        {
            url: SnitzVars.baseUrl + '/PostThanks/Members/' + id + '?replyid=' + replyid,
            async: false,
            success: function (response) {
            e.popover({html: true,content: response}).popover('show');
            },
            error: function (jqXHR, textStatus, errorThrown) {
                appendAlert(jqXHR.responseText, 'warning');
            }
            });

  } )
  .on( "mouseleave", function() {
        var e=$(this);
        e.popover('hide');
  } );

/* functions to insert/select text in textarea */
$.fn.extend({
    insertAtCaret: function (myValue) {
        var textComponent = $(this)[0];
        if (document.selection !== undefined) {
            this.focus();
            sel = document.selection.createRange();
            sel.text = myValue;
            this.focus();
        } else if (textComponent.selectionStart != undefined) {
            var startPos = textComponent.selectionStart;
            var endPos = textComponent.selectionEnd;
            var scrollTop = textComponent.scrollTop;
            textComponent.value = textComponent.value.substring(0, startPos) + myValue + textComponent.value.substring(endPos, textComponent.value.length);
            this.focus();
            this.selectionStart = startPos + myValue.length;
            this.selectionEnd = startPos + myValue.length;
            this.scrollTop = scrollTop;
        } else {
            this.val(this.val() + myValue);
            this.focus();
        }
    },
    surroundSelection: function (val1, val2) {
        var selectedText = '';
        var textComponent = $(this)[0]; 
        if (document.selection != undefined) {
            
            this.focus();
            var sel = document.selection.createRange();
            sel.text = val1 + sel.text + val2;
            this.focus();
        }
            // Mozilla version
        else if (textComponent.selectionStart != undefined) {
            //textComponent.focus();
            var startPos = textComponent.selectionStart;
            var endPos = textComponent.selectionEnd;
            var scrollTop = textComponent.scrollTop;
            var myValue = val1 + val2;

            if (startPos !== endPos) {
                selectedText = textComponent.value.substring(startPos, endPos);
            
                textComponent.value = textComponent.value.substring(0, startPos) + val1 + selectedText + val2 + textComponent.value.substring(endPos, textComponent.value.length);

            } else {
                textComponent.value = textComponent.value.substring(0, startPos) + myValue + textComponent.value.substring(endPos, textComponent.value.length);
                //textComponent.value += val1 + val2;
            }
            textComponent.selectionStart = startPos + myValue.length;
            textComponent.selectionEnd = startPos + myValue.length;
            textComponent.scrollTop = scrollTop;

        }
        

    },
    replaceSelection: function (val) {
        var selectedText = '';
        var textComponent = $(this)[0];
        //
        if (document.selection != undefined) {
            this.focus();
            var sel = document.selection.createRange();
            sel.text = val;
            this.focus();
        }
            // Mozilla version
        else if (textComponent.selectionStart != undefined) {
            textComponent.focus();
            var startPos = textComponent.selectionStart;
            var endPos = textComponent.selectionEnd;
            var scrollTop = textComponent.scrollTop;
            var myValue = val;

            if (startPos != endPos) {
                selectedText = textComponent.value.substring(startPos, endPos);

                textComponent.value = textComponent.value.substring(0, startPos) + val + textComponent.value.substring(endPos, textComponent.value.length);

            } else {
                textComponent.value = textComponent.value.substring(0, startPos) + myValue + textComponent.value.substring(endPos, textComponent.value.length);
                //textComponent.value += val1 + val2;
            }
            textComponent.selectionStart = startPos;
            textComponent.selectionEnd = startPos + myValue.length;
            textComponent.scrollTop = scrollTop;

        }


    },
    getSelection: function () {
        var selectedText = '';
        var textComponent = $(this)[0];

        if (document.selection != undefined) {
            this.focus();
            var sel = document.selection.createRange();
            selectedText = sel.text;
            sel.text = "";
            this.focus();
        }
            // Mozilla version
        else if (textComponent.selectionStart != undefined) {
            textComponent.focus();
            var startPos = textComponent.selectionStart;
            var endPos = textComponent.selectionEnd;
            var scrollTop = textComponent.scrollTop;
            var myValue = "";

            if (startPos != endPos) {
                selectedText = textComponent.value.substring(startPos, endPos);

                textComponent.value = textComponent.value.substring(0, startPos) + textComponent.value.substring(endPos, textComponent.value.length);

            } else {
                textComponent.value = textComponent.value.substring(0, startPos) + myValue + textComponent.value.substring(endPos, textComponent.value.length);
                //textComponent.value += val1 + val2;
            }
            textComponent.selectionStart = startPos + myValue.length;
            textComponent.selectionEnd = startPos + myValue.length;
            textComponent.scrollTop = scrollTop;

        }
        return selectedText;

    },
    selectRange:function (start, end) {
    if (!end) end = start;
    return this.each(function () {
        if (this.setSelectionRange) {
            this.focus();
            this.setSelectionRange(start, end);
        } else if (this.createTextRange) {
            var range = this.createTextRange();
            range.collapse(true);
            range.moveEnd('character', end);
            range.moveStart('character', start);
            range.select();
        }
    });
}
});