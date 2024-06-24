$(document).on('click','.confirm-delete', function(e) {
    e.preventDefault();
    debugger;
    var id = $(this).data('id');
    var href = $(this).attr('href');
    $('#confirmModal').data('id', id).data('url', href.replace("~", SnitzVars.baseUrl)).modal('show');
});
$(document).on('click','#btnYes',function(e) {
    // handle deletion here
    e.preventDefault();
    var id = $('#confirmModal').data('id');
    var url = $('#confirmModal').data('url');
    debugger;
    $.get(url, function(result){
        $('[data-id='+id+']').remove();
        $('#confirmModal').modal('hide');
        window.location.replace(result.redirectToUrl);
    });

});