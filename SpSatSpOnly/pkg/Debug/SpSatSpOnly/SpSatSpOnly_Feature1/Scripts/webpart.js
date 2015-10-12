
$(document).ready(function () {

    // get all events
    var context = SP.ClientContext.get_current();
    var web = context.get_web();
    var list = web.get_lists().getByTitle("Events");
    var events = list.getItems("");
    context.load(events);
    context.executeQueryAsync(function () {
        
        // clear the loader
        $("#events-container").html("");

        // build the display of each item
        var count = 0;
        var listOfEvents = events.getEnumerator();
        while (listOfEvents.moveNext()) {
            var event = listOfEvents.get_current();

            var div = $("<div></div>").appendTo("#events-container").css({
                "float": "left",
                "width": "400",
                "height": "200",
                "position": "relative"
            });

            var image = $("<div></div>").appendTo(div).css({
                "position": "absolute",
                "width": "400",
                "height": "200",
                "background": "url('" + event.get_fieldValues().PictureURL.get_url() + "') no-repeat",
                "background-size": "cover"
            });

            var caption = $("<div></div>").appendTo(div).css({
                "position": "absolute",
                "left": "0",
                "top": "170",
                "width": "400",
                "height": "30",
                "background-color": "rgba(0, 0, 0, 0.7)",
                "color": "white",
                "font-size": "14pt"
            });

            var captionText = $("<div></div>").appendTo(caption).css({
                "margin-left": "8"
            }).text(event.get_fieldValues().Title);

            count++;
        }

        // expand
        $("#events-container").css("width", 400 * count);

        // slide every 4 seconds
        var position = 0;
        window.setInterval(function () {
            position++;
            if (position >= count) position = 0;
            $("#events-container").animate({ "margin-left": -400 * position }, 500);
        }, 4000);

    }, function (xhr, err) {
        alert("failure - " + err.get_message());
    });

});
