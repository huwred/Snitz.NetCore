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
                console.log(data.result);
                if (!data.result) {
                    alert(data.error);
                } else {
                    location.reload(true);
                }
            });
        
    } 
});
$(document).on("click",".post-del", function() {

    if (confirm("Are you sure!") === true) {
        var postid = $(this).data("id");
        $.post("/Topic/DeleteTopic",
            {
                id: postid
            },
            function(data, status){
                console.log(data.result);
                if (!data.result) {
                    alert(data.error);
                } else {
                    location.reload(true);
                }
            });
        
    }
});

$(document).on("click",".post-lock", function() {

    if (confirm("Are you sure!") === true) {
        var postid = $(this).data("id");
        var poststatus = $(this).data("status");
        $.post("/Topic/LockTopic",
            {
                id: postid,
                status: poststatus
            },
            function(data, status){
                console.log(data.result);
                if (!data.result) {
                    alert(data.error);
                } else {
                    location.reload(true);
                }
            });
        
    }
});

$(document).on("click",".reply-quote", function() {
    var postid = $(this).data("id");
    location.href = "/Topic/QuoteReply/" + postid;
});
$(document).on("click",".post-quote", function() {
    var postid = $(this).data("id");
    location.href = "/Topic/Quote/" + postid;
});

$(document).on("click",".reply-edit", function() {

    var postid = $(this).data("id");
    location.href = "/Topic/EditReply/" + postid;
});
$(document).on("click",".post-edit", function() {

    var postid = $(this).data("id");
    location.href = "/Topic/Edit/" + postid;

});