
function getQueryStringParameter(paramToRetrieve) {
    var params = document.URL.split("?")[1].split("&");
    var strParams = "";
    for (var i = 0; i < params.length; i = i + 1) {
        var singleParam = params[i].split("=");
        if (singleParam[0] == paramToRetrieve)
            return singleParam[1];
    }
}

$(document).ready(function () {

    // get the context variables
    var accessToken = decodeURIComponent(getQueryStringParameter("adal_token"));

    // wait for status and notifications
    ExecuteOrDelayUntilScriptLoaded(function () {

        // Azure SQL Database authentication using a SQL account
        $.ajax({
            url: "https://spsat.azurewebsites.net/multitier/logs",
            headers: { "Authorization": "Bearer " + accessToken },
            dataType: "json",
            success: function (logs) {

                var container = $("#multitier-container1");
                $(logs).each(function (i, log) {

                    var outer = $("<div></div>").appendTo(container);
                    $("<h1></h1>").appendTo(outer).text(log.Type + "." + log.Scope);
                    $("<h2></h2>").appendTo(outer).text(log.Timestamp);
                    $("<p></p>").appendTo(outer).text(log.Message);

                });
            
            },
            error: function (xhr, msg) {
                alert("error - " + xhr.status + ": " + xhr.statusCode());
            }
        });

        // Azure SQL Database authentication using Integrated Auth
        //$.ajax({
        //    url: "https://spsat.azurewebsites.net/multitier/pairs",
        //    headers: { "Authorization": "Bearer " + accessToken },
        //    dataType: "json",
        //    success: function (logs) {

        //        var container = $("#multitier-container2");
        //        $(logs).each(function (i, pairs) {

        //            var outer = $("<div></div>").appendTo(container).text(pairs.Key + ": " + pairs.Value);

        //        });

        //    },
        //    error: function (xhr, msg) {
        //        alert("error - " + xhr.status + ": " + xhr.statusCode());
        //    }
        //});

    }, 'core.js');

});
