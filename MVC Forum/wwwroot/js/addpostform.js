﻿        //$('.date').datepicker({
        //    autoclose: true,
        //    format: {
        //        /*
        //         * Say our UI should display a week ahead,
        //         * but textbox should store the actual date.
        //         * This is useful if we need UI to select local dates,
        //         * but store in UTC
        //         */
                    
        //        toDisplay: function (date, format, language) {
        //            var d = new Date(date);
        //            d.setDate(d.getDate());
        //            return d.toLocaleDateString();
        //        },
        //        toValue: function (date, format, language) {
        //            let re = /([0-9]{4})([0-9]{2})([0-9]{2})/;
        //            let lastFirst = date.replace(re, '$1-$2-$3');
        //            var d = new Date(lastFirst);
        //            d.setDate(d.getDate());
        //            return new Date(d);
        //        }
        //    }
        //});
        $(document).on("submit", "#addPostForm", function (e) {
            e.preventDefault();
            tinyMCE.get("msg-text").save();
            var form = $("#addPostForm");
            var formData = new FormData(form[0]);
            $.ajax({
                url: $(this).attr("action"),
                type: "POST",
                data: formData,
                contentType: false,
                processData: false,
                success: function (data) {
                    if ($('#event-form').length > 0) {
                        var evtform = $('#event-form').find('form');

                        $('#event-topic').val(data.id);
                        var eventData = new FormData(evtform[0]);
                        $.ajax({
                            url: evtform.attr("action"),
                            type: "POST",
                            data: eventData,
                            contentType: false,
                            processData: false,
                            success: function (data) {
                                location.href = data.url;
                            },
                            error:function(err){
                                console.log(err);
                            }
                        }); 
                        //save event data for Topic
                    }
                    else if ($('#poll-form').length > 0) {
                        //save poll data for Topic
                        var evtform = $('#poll-form').find('form');

                        $('#poll-topic').val(data.id);
                        var eventData = new FormData(evtform[0]);
                        $.ajax({
                            url: evtform.attr("action"),
                            type: "POST",
                            data: eventData,
                            contentType: false,
                            processData: false,
                            success: function (data) {
                                location.href = data.url;
                            },
                            error:function(err){
                                console.log(err);
                            }
                        }); 
                    } else {
                        location.href = data.url;
                    };

                },
                error:function(err){
                    console.log(err);
                }
            }); 
            return false;
        });
        $(document).ready(function () {
            $(".fig-caption").each(function () {
                var test = $(this);
                $.ajax({
                    url: SnitzVars.baseUrl + "/PhotoAlbum/GetCaption/" + $(this).data("id"),
                    type: "GET",
                    success: function (data) {
                        //alert(data); // the View
                        test.html(data);
                    }
                });
            });
        });
        $(document).on('click',
            '#submitUpload',
            function (e) {
                e.preventDefault();
                var form = $("#upload-form");
                var formData = new FormData(form[0]);
                $.ajax({
                    url: SnitzVars.baseUrl + $("#upload-form").attr("action"),
                    type: "POST",
                    data: formData,
                    contentType: false,
                    processData: false,
                    success: function (data) {
                        if (data.result) {
                            if (!data.type) {
                                var img = "[image=" + data.id + "]";
                                if (data.caption) {
                                    img = "[cimage=" + data.id + "]";
                                }
                                tinymce.activeEditor.execCommand('mceInsertContent', false, img);
                            } else {
                                //alert(data.type);
                                var file = "[file " + data.filesize + "]" + data.data + "[/file]";
                                if (data.type === ".pdf") {
                                    file = "[pdf]" + data.data + "[/pdf]";
                                } else if (data.type === ".jpg" || data.type === ".png" || data.type === ".jpeg" || data.type === ".webp") {
                                    file = "[img]" + data.data + "[/img]";
                                }

                                tinymce.activeEditor.execCommand('mceInsertContent', false, file);
                            }

                            $('#uploadModal').modal('hide');
                        }

                    },
                    error: function (data) {
                        appendAlert(data, 'error');
                        $('#upload-content').html(data);
                    },
                    complete: function (data) {
                        $('#uploadModal').modal('hide');
                    }

                });
                return false;
            });


        $("#aFile_upload").on("change",
            function(e) {
                var filesize = ((this.files[0].size / 1024) / 1024).toFixed(4); // MB
                var maxsize = SnitzVars.MaxFileSize;
                if (filesize > maxsize) {
                    appendAlert("File is too big!", 'warning');
                    this.value = "";
                }

            });