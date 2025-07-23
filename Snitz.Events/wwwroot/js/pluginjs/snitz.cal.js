$(document).ready(function () {
    if ($('.cal-dates').length >= 1) {

        $('#calendar-start-date').datetimepicker({
            dateFormat: 'dd M yy',
            timeFormat: 'HH:mm',
            timeInput: true,
            showHour: false,
            showMinute: false
            
        });

        $('#calendar-end-date').datetimepicker({
            dateFormat: 'dd M yy',
            timeFormat: 'HH:mm',
            timeInput: true,
            showHour: false,
            showMinute: false

        });
        if ($('#cal-recur').val() == 'EveryDay') {
            $('#cal-dow').show();
        }
        $('#cal-recur').on('change', function(evt) {
            if ($(this).val() == 'EveryDay') {
                $('#cal-dow').show();
            } else {
                $('#cal-dow').hide();
            }
        });
    }
    if ($('.club-dates').length >= 1) {

        $('#calendar-start-date').datetimepicker({
            dateFormat: 'dd M yy',
            timeFormat: 'HH:mm',
            timeInput: false,
            showHour: false,
            showMinute: false

        });

        $('#calendar-end-date').datetimepicker({
            dateFormat: 'dd M yy',
            timeFormat: 'HH:mm',
            timeInput: false,
            showHour: false,
            showMinute: false

        });
    }
});

UpComingCalendar = function(url, divid) {
    var d = new Date();
    var day = d.getDate();
    var month = d.getMonth() + 1;
    var year = d.getFullYear();
    var date = '' + year + '-' + (month <= 9 ? '0' + month : month) + '-' + (day <= 9 ? '0' + day : day);

    $('#' + divid)
        .fullCalendar({
            header: {
                left: 'title',
                center: '',
                right: ''
            },
            duration: { weeks: 5 },
            defaultView: 'upComing',
            lang: SnitzVars.forumlang,
            isJalaali: SnitzVars.forumlang === 'fa',
            isRTL: SnitzVars.forumlang === 'fa',
            allDayText: 'all-day',
            dateFormat: 'DD/MM/YY',
            timeFormat: 'HH:mm',
            //titleFormat: 'Upcoming Events', //TODO
            height: 560,
            eventSources: [
                { url: url },
                { url: SnitzVars.baseUrl + "/Calendar/GetHolidays" },
                { url: SnitzVars.baseUrl + "/Calendar/GetBirthDays" }
            ],
            loading: function(bool) {

                if (bool) {
                    $("#calendar-list").css({ "visibility": "hidden" });
                    $("#calendar-list").css({ "height": "0px" });
                    $('#upcoming-events').show();
                } else {
                    $("#calendar-list").css({ "visibility": "visible" });
                    $('#cal-loading').hide();
                    $("#calendar-list").css({ "height": "auto" });


                }

            },
            defaultDate: moment(date)
        });
};

FullCalendar = function(url, divid, firstday, country) {
    //console.log("SnitzVars.baseUrl");
    //console.log(SnitzVars.baseUrl);
    var view = $('#' + divid).fullCalendar('getView');
    if (country.length > 1) {
        $('#' + divid).fullCalendar('destroy');
    }
    var holidayUrl = SnitzVars.baseUrl + "/Calendar/GetHolidays/" + country;
    var eventsUrl = SnitzVars.baseUrl + "/Events/GetClubCalendarEvents/-1?old=0&calendar=1";
    var birthdayUrl = SnitzVars.baseUrl + "/Calendar/GetBirthDays";

    $('#' + divid)
        .fullCalendar({
            header: {
                left: 'basicWeek,month,year',
                center: 'title',
                right: 'prev,next,today'
            },
            defaultView: 'month',
            lang: SnitzVars.forumlang,
            isJalaali: SnitzVars.forumlang === 'fa',
            isRTL: SnitzVars.forumlang === 'fa',
            firstDay: parseInt(firstday),
            dateFormat: 'dd/MM/yy',
            timeFormat: 'HH:mm',
            eventSources: [
                { url: url },
                { url: eventsUrl },
                { url: holidayUrl },
                { url: birthdayUrl }
            ],
            eventClick: function(calEvent, jsEvent, view) {

                if (calEvent.className[0] === "event-birthday") {
                    alert(calEvent.title);
                }
            }

        });
    if (country.length > 1) {
        var newview = $('#' + divid).fullCalendar('getView');
        if (newview.name !== view.name) {
            $('#' + divid).fullCalendar('changeView', view.name);
        }

    }
};

ClubCalendar = function(url, divid, catfilter) {
    var d = new Date();
    var n = d.getDay();
    var fullweeks = 52;

    if (catfilter.length > 1) {
        fullweeks = calcWeeks(catfilter);
    }
    var view = 'upComing';
    if (catfilter.length > 1) {
        view = 'disContinued';
    }
    $('#' + divid)
        .fullCalendar({
            header: {
                left: '',
                center: 'title',
                right: ''
            },
            views: {
                upComing: {
                    type: 'callist',
                    duration: { weeks: 52 },
                    titleFormat: upComingEventsTitle,
                    rows: 26
                },
                disContinued: {
                    type: 'callist',
                    duration: { weeks: fullweeks },
                    titleFormat: pastEventsTitle,
                    rows: 26
                }
            },
            defaultView: view,
            firstDay: n,
            dateFormat: 'DD/MM/YY',
            timeFormat: 'HH:mm',
            eventSources: [url],
            navLinks: false, // can click day/week names to navigate views
            editable: false,
            eventLimit: true // allow "more" link when too many events

        });
    if (catfilter.length > 1) {
        //$('#' + divid).fullCalendar('changeView', 'disContinued');
        $('#' + divid).fullCalendar('gotoDate', catfilter);
    }
    if ($('.my-fc-list>div').is(':empty')) {
        $('.my-fc-list>div').html('No Events');

    }
};

//get number of weeks between dates
calcWeeks = function(parm1) {
    var date1 = new Date(parm1);
    var date2 = new Date();
    return (date2 - date1) / (1000 * 60 * 60 * 24 * 7).toFixed(2);
};

postEvent = function(event, arr) {

    if ($('#calendar-start-date').val() === '') {
        location.reload(arr[1]);
        return false;
    }

    $('#cal-topicid').val(arr[0]);
    var serializedForm = $("#cal-addTopicEvent").serialize();
    event.preventDefault();
    $.ajax({
        type: "POST",
        url: SnitzVars.baseUrl + '/Calendar/SaveEvent',
        data: serializedForm,
        dataType: "json",
        success: function(data) {
            if (data.success) {
                BootstrapDialog.alert(
                {
                    title: "Event Info ",
                    message: data.responseText
                });
                location.href = arr[1];
                return false;
            }

        },
        error: function(jqXHR, textStatus, errorThrown) {
            BootstrapDialog.alert(
            {
                title: "Event Error ",
                message: jqXHR.responseText
            });
            location.href = arr[1];
            return false;
        }
    });
};

setForumEventsAuth = function(event) {
    var serializedForm = $("#cal-forumAuth").serialize();
    event.preventDefault();
    $.ajax({
        type: "POST",
        url: SnitzVars.baseUrl + '/Calendar/SaveForum',
        data: serializedForm,
        dataType: "json",
        success: function(data) {
            if (data.success) {
                BootstrapDialog.alert(
                {
                    title: "Event Info ",
                    message: data.responseText
                });
                location.reload();
                return false;
            }

        },
        error: function(result) {
            BootstrapDialog.alert(
            {
                title: "Event Error ",
                message: result
            });
        }
    });
};

