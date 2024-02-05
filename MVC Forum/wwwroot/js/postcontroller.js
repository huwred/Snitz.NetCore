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
    var forms = container.getElementsByTagName("form");
    var newForm = forms[forms.length - 1];
    $.validator.unobtrusive.parse(newForm);
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
        $.post("/Topic/DeleteReply",
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
        $.post("/Topic/DeleteTopic",
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
            $.post("/Bookmark/Create",
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
                $.post("/Bookmark/Delete",
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
        $.post("/Topic/LockTopic",
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
    location.href = "/Topic/QuoteReply/" + postid;
});
$(document).on("click",".post-quote", function() {
    var postid = $(this).data("id");
    location.href = "/Topic/Quote/" + postid;
});
$(document).on("click",".post-reply", function() {
    var postid = $(this).data("id");
    location.href = "/Topic/Reply/" + postid;
});
$(document).on("click",".reply-edit", function() {

    var postid = $(this).data("id");
    location.href = "/Topic/EditReply/" + postid;
});
$(document).on("click",".post-edit", function() {

    var postid = $(this).data("id");
    location.href = "/Topic/Edit/" + postid;

});