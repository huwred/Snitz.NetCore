
if ($('.cal-dates').length >= 1) {
    if ($('#cal-recur').val() == 'EveryDay') {
        $('#cal-dow').show();
    }
    $(document).on('change', '#cal-recur', function (evt) {
        if ($(this).val() == 'EveryDay') {
            $('#cal-dow').show();
        } else {
            $('#cal-dow').hide();
        }
    });
}
$(document).on("change", '#countryRegion', function () {
    var val = $('#change-holidays').val() + '|' + $(this).val();
    var url = SnitzVars.baseUrl + "/Calendar/GetHolidays/";
    FullCalendarNew(url, 'calendar', '', val);
});

$('#change-holidays')
    .on("change",function () {
        var val = $(this).val();
        populateRegions(val);
        localStorage.setItem('pubCountry', val);
        var url = SnitzVars.baseUrl + "/Calendar/GetHolidays/"
        FullCalendarNew(url, 'calendar', '', val);

    });

$(document).on("click", ".fc-list-event-time .fa-trash-can", function (e) {
    e.preventDefault();
    var id = $(this).data('id');
    var href = SnitzVars.baseUrl + "/Events/DeleteEvent/" + id;
    (async () => {
        const result = await b_confirm("Delete this event")
        if (result) {
            $.get(href, function (result) {
                window.location.reload();
            });
        }
    })();
});

$(document).on("click", ".fc-list-event-time .fa-pencil", function (e) {
    e.preventDefault();
    var id = $(this).data('id');
    location.href = SnitzVars.baseUrl + "/Events/AddEditEvent/" + id;
});

UpComingCalendar = function(url, divid) {
    console.log(url);
    var calendarEl = document.getElementById(divid);
    let selectedCountry = localStorage.getItem('pubCountry');
    var calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'upcomingListView',
        locale: SnitzVars.forumlang,
        views: {
            upcomingListView: {
                type: 'list',
                visibleRange: function (currentDate) {
                    // Generate a new date for manipulating in the next step
                    var startDate = new Date(currentDate.valueOf());
                    var endDate = new Date(currentDate.valueOf());

                    // Adjust the start & end dates, respectively
                    startDate.setDate(startDate.getDate() - 1); // One day in the past
                    endDate.setDate(endDate.getDate() + 21); // three weeks into the future
                    return { start: startDate, end: endDate };
                },
                buttonText: 'Custom List'
                }
        },
        eventDidMount: function (info) {
            // Create custom HTML for club-events
            if (info.event.classNames.includes('club-event')) {
                let starttime = info.event.start.toLocaleTimeString([], {
                    hour: '2-digit',
                    minute: '2-digit',
                    hour12: true
                });
                let endtime = info.event.end.toLocaleTimeString([], {
                    hour: '2-digit',
                    minute: '2-digit',
                    hour12: true
                });
                info.el.innerHTML = ''; //clear the display
                var extra = document.createElement('td');
                extra.setAttribute("colspan", "3");
                extra.innerHTML =
                    '<p><strong>' + (info.event.title || 'N/A') +
                    '</strong><br><b>Location:</b> ' + (info.event.extendedProps.location.name || 'N/A') +
                    '<br>' + (starttime || 'N/A') + ' - ' + (endtime || 'N/A') + '</p>';
                info.el.appendChild(extra);
            } else if (info.event.classNames.includes('topic-event')) {
                let starttime = info.event.start.toLocaleTimeString([], {
                    hour: '2-digit',
                    minute: '2-digit',
                    hour12: true
                });
                let endtime = info.event.end.toLocaleTimeString([], {
                    hour: '2-digit',
                    minute: '2-digit',
                    hour12: true
                });
                info.el.innerHTML = ''; //clear the display
                var extra = document.createElement('td');
                extra.setAttribute("colspan", "3");
                extra.innerHTML =
                    '<p><strong>' + (info.event.title || 'N/A') +
                    '</strong>' +
                    '<br>' + (starttime || 'N/A') + ' - ' + (endtime || 'N/A') + '</p>';
                info.el.appendChild(extra);
            }
        },
        height: 400,
        eventSources: [
            { url: url },
            { url: SnitzVars.baseUrl + "/Calendar/GetHolidays/?country=" + selectedCountry },
            { url: SnitzVars.baseUrl + "/Calendar/GetBirthDays" }
        ],
        loading: function(bool) {
            if (bool) {
                $("#calendar-list").css({ "visibility": "hidden" });
                $("#calendar-list").css({ "height": "0px" });
                $('#upcoming-events').show();
            } else {
                $("#calendar-list").css({ "visibility": "visible" });
                $('#cal-loading i').hide();
                $('#cal-loading').hide();
                $("#calendar-list").css({ "height": "auto" });
            }
        }
    });
    calendar.render();
};

ClubCalendar = function (url, divid, catfilter) {
    var d = new Date();
    var day = d.getDate();
    var month = d.getMonth() + 1;
    var year = d.getFullYear();
    var fullweeks = 52;

    if (catfilter.length > 1) {
        fullweeks = calcWeeks(catfilter);
    }
    var view = 'upComing';
    if (catfilter.length > 1) {
        view = 'disContinued';
    }
    if (view === "disContinued") {
        console.log(view);
        month = month - 12;
        if (month < 0) {
            month = 12 + month;
            year = year - 1;
        }
    }
    var date = '' + year + '-' + (month <= 9 ? '0' + month : month) + '-' + (day <= 9 ? '0' + day : day);
    var calendarEl = document.getElementById(divid);
    var calendar = new FullCalendar.Calendar(calendarEl, {
        headerToolbar: {
                left: '',
                center: 'title',
                right: ''
            },
            views: {
                upComing: {
                    type: 'list',
                    duration: { months: 2 },
                    //titleFormat: upComingEventsTitle,
                },
                disContinued: {
                    type: 'list',
                    duration: { months: 6 },
                    //titleFormat: pastEventsTitle,
                }
            },
            eventDidMount: function (info) {
                // Create custom HTML for extra fields
                var extra = document.createElement('div');
                extra.classList.add('custom-field');
                extra.innerHTML =
                    '<strong>Club:</strong> ' + (info.event.extendedProps.clublong || 'N/A') +
                    '<br><strong>Location:</strong> ' + (info.event.extendedProps.location.name || 'N/A') +
                    '<p> ' + (info.event.extendedProps.details || 'N/A') + '</p>';

                // Append to the event's main element
                info.el.querySelector('.fc-list-event-title').appendChild(extra);
                var time = document.createElement('div');
                time.innerHTML =
                    (info.event.extendedProps.club || 'N/A');
                info.el.querySelector('.fc-list-event-time').appendChild(time);
                if (SnitzVars.isAdmin) {
                    var trash = document.createElement("i");
                    trash.classList.add("fa");
                    trash.classList.add("fa-trash-can");
                    trash.setAttribute("data-id", info.event.id);
                    var edit = document.createElement("i");
                    edit.classList.add("fa");
                    edit.classList.add("fa-pencil");
                    edit.setAttribute("data-id", info.event.id);
                    info.el.querySelector('.fc-list-event-time').appendChild(trash);
                    info.el.querySelector('.fc-list-event-time').appendChild(edit);
                }

                //fc-list-event-time
            },
            initialView: view,
            initialDate: date,
            eventSources: [url],
            navLinks: false, // can click day/week names to navigate views
            editable: false,
            //eventLimit: true // allow "more" link when too many events
    });
    calendar.render();

    if ($('.my-fc-list>span').is(':empty')) {
        $('.my-fc-list>span').html('No Events');
    }
};

//get number of weeks between dates
calcWeeks = function(parm1) {
    var date1 = new Date(parm1);
    var date2 = new Date();
    return (date2 - date1) / (1000 * 60 * 60 * 24 * 7).toFixed(2);
};

postEvent = function(event, arr) {

    if ($('#startdate').val() === '') {
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
        error: function (jqXHR, textStatus, errorThrown) {
            appendAlert(jqXHR.responseText, 'warning');
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
                appendAlert(data.responseText, 'info');
                location.reload();
                return false;
            }

        },
        error: function (result) {
            appendAlert(result, 'danger');
        }
    });
};

FullCalendarNew = function (url, divid, firstday, country) {
    var calendarEl = document.getElementById('calendar');
    var url = SnitzVars.baseUrl + "/Calendar/GetCalendarEvents/"
    var holidayUrl = SnitzVars.baseUrl + "/Calendar/GetHolidays/?country=" + country;
    var eventsUrl = SnitzVars.baseUrl + "/Events/GetClubCalendarEvents/-1?old=0&calendar=1";
    var birthdayUrl = SnitzVars.baseUrl + "/Calendar/GetBirthDays";
    var calendar = new FullCalendar.Calendar(calendarEl, {
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'multiMonthYear,dayGridMonth,timeGridWeek'
        },
        firstDay: firstday,
        locale: SnitzVars.forumlang,
        editable: true,
        selectable: true,
        dayMaxEvents: true, 
        eventSources: [
            { url: url },
            { url: eventsUrl },
            { url: holidayUrl },
            { url: birthdayUrl }
        ]
    });

    calendar.render();

}