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
//post button controls
$(document).on("click",".reply-del", function() {
    let postid;
    if (confirm("Are you sure!") === true) {
        postid = $(this).data("id");
        $.post(SnitzVars.baseUrl + "/Topic/DeleteReply",
            {
                id: postid
            },
            function(data, status){
                if (!data.result) {
                    alert(data.error);
                } else {
                    location.reload(true);
                }
            });
        
    } 
});
$(document).on("click",".post-del", function(e) {
    e.preventDefault();
    var postid = $(this).data("id");
    var href = $(this).attr('href');
    $('#confirmModal #confirm-body').html('<p>You are about to Delete this Topic.</p><p>Do you wish to proceed?</p>');
    $('#confirmModal').data('id', postid).data('url', href).modal('show');

    $('#confirmModal').on('click','#btnYes',function(e) {
        // handle deletion here
        e.preventDefault();
        $.post(SnitzVars.baseUrl + "/Topic/DeleteTopic",
            {
                id: postid
            },
            function(data, status){
                
                if (!data.result) {
                    alert(data.error);
                } else {
                    location.href = data.url;
                }
            });
    });
});
        $(document).on("click",
            ".lock-member",
            function(e) {
                e.preventDefault();
                var memberid = $(this).data("id");
                var href = $(this).data("url");
                $('#modal-title').html('Lock Member');
                $('#member-modal').html('Toggle Member Status');
                $('#memberModal').data('id', memberid).modal('show');
                $('#memberModal #btnOk').show();
                $('#memberModal').one('click',
                    '#btnOk',
                    function(e) {
                        // handle deletion here
                        e.preventDefault();
                        $.post(href,
                            {
                                id: memberid
                            },
                            function(data, status) {

                                if (!data.result) {
                                    alert(data.error);
                                } else {
                                    location.reload();
                                }
                            });
                    });
            });
$(document).on('click',
    '.bookmark-add',
    function(e) {
        e.preventDefault();
        var href = $(this).attr('href');
        var postid = $(this).data("id");
        //confirm-body
        $('#confirmModal #confirm-body').html('<p>You are about to Bookmark this Topic.</p><p>Do you wish to proceed?</p>');
        $('#confirmModal').data('id', postid).data('url', href).modal('show');
        $('#confirmModal').on('click','#btnYes',function(e) {
            // handle deletion here
            e.preventDefault();
            e.stopPropagation();
            $.post(SnitzVars.baseUrl + "/Bookmark/Create",
                {
                    id: postid
                },
                function(data, status){
                    $('#confirmModal').modal('hide');
                    if (!data.result) {
                        alert(data.error);
                    } else {

                        location.reload(true);
                    }
                });
                    
        }); 
    });
    $(document).on('click',
        '.bookmark-del',
        function(e) {
            e.preventDefault();
            var href = $(this).attr('href');
            var postid = $(this).data("id");
            //confirm-body
            $('#confirmModal #confirm-body').html('<p>You are about to delete this Bookmark.</p><p>Do you wish to proceed?</p>');
            $('#confirmModal').data('id', postid).data('url', href).modal('show');
            $('#confirmModal').on('click','#btnYes',function(e) {
                // handle deletion here
                e.preventDefault();
                $.post(SnitzVars.baseUrl + "/Bookmark/Delete",
                    {
                        id: postid
                    },
                    function(data, status){
                        $('#confirmModal').modal('hide');
                        if (!data.result) {
                            alert(data.error);
                        } else {

                            location.reload(true);
                        }
                    });
                    
            }); 
        });
$(document).on('click', '.post-lock', function (e) {
    e.preventDefault();

    var href = $(this).attr('href');
    var postid = $(this).data("id");
    var poststatus = $(this).data("status");
    //confirm-body
    $('#confirmModal #confirm-body').html('<p>You are about to Lock this Topic.</p><p>Do you wish to proceed?</p>');
    $('#confirmModal').data('id', postid).data('url', href).modal('show');
    $('#confirmModal').on('click','#btnYes',function(e) {
        // handle deletion here
        e.preventDefault();
        e.stopPropagation();
        $.post(SnitzVars.baseUrl + "/Topic/LockTopic",
            {
                id: postid,
                status: poststatus
            },
            function(data, status){
                $('#confirmModal').modal('hide');
                if (!data.result) {
                    alert(data.error);
                } else {

                    location.reload(true);
                }
            });
                    
    });        
});

$(document).on("click",".reply-quote", function() {
    var postid = $(this).data("id");
    location.href = SnitzVars.baseUrl + "/Topic/QuoteReply/" + postid;
});
$(document).on("click",".post-quote", function() {
    var postid = $(this).data("id");
    location.href = SnitzVars.baseUrl + "/Topic/Quote/" + postid;
});
$(document).on("click",".post-reply", function() {
    var postid = $(this).data("id");
    location.href = SnitzVars.baseUrl + "/Topic/Reply/" + postid;
});
$(document).on("click",".reply-edit", function() {

    var postid = $(this).data("id");
    location.href = SnitzVars.baseUrl + "/Topic/EditReply/" + postid;
});
$(document).on('click', '.reply-answer', function (e) {
    e.preventDefault();

    var href = $(this).attr('href');
    var postid = $(this).data("id");
    var poststatus = $(this).data("status");
    //confirm-body
    $('#confirmModal #confirm-body').html('<p>Mark as Answer?</p><p>Do you wish to proceed?</p>');
    $('#confirmModal').data('id', postid).data('url', href).modal('show');
    $('#confirmModal').on('click','#btnYes',function(e) {
        // handle deletion here
        e.preventDefault();
        e.stopPropagation();
        $.post(SnitzVars.baseUrl + "/Topic/Answered",
            {
                id: postid,
                status: poststatus
            },
            function(data, status){
                $('#confirmModal').modal('hide');
                if (!data.result) {
                    alert(data.error);
                } else {

                    location.reload(true);
                }
            });
                    
    });        
});
$(document).on("click",".post-edit", function() {

    var postid = $(this).data("id");
    location.href = SnitzVars.baseUrl + "/Topic/Edit/" + postid;

});

    // functions to insert/select text in textarea
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