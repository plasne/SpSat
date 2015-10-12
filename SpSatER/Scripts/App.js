
var appWebUrl;
var hostWebUrl;
var appContext;
var hostContext;

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

    ExecuteOrDelayUntilScriptLoaded(function () {
        ExecuteOrDelayUntilScriptLoaded(function () {

            // get the context variables
            appWebUrl = decodeURIComponent(getQueryStringParameter("SPAppWebUrl"));
            hostWebUrl = decodeURIComponent(getQueryStringParameter("SPHostUrl"));
            appContext = new SP.ClientContext(appWebUrl);
            hostContext = new SP.AppContextSite(appContext, hostWebUrl);

            $("#er-register").click(function () {
                var list = hostContext.get_web().get_lists().getByTitle("ER Sample");
                var receivers = list.get_eventReceivers();
                var receiver = new SP.EventReceiverDefinitionCreationInformation();
                receiver.set_receiverName("SpSatTest OnItemAdding");
                receiver.set_eventType(SP.EventReceiverType.itemAdding);
                receiver.set_receiverUrl("https://spsat.azurewebsites.net/EventReceiver.svc");
                receivers.add(receiver);
                appContext.executeQueryAsync(function () {
                    var statusId = SP.UI.Status.addStatus("Event receiver registered successfully.");
                    SP.UI.Status.setStatusPriColor(statusId, "green");
                }, function (sender, args) {
                    var statusId = SP.UI.Status.addStatus("Exception when registering event receiver. EXCEPTION: " + err.get_message());
                    SP.UI.Status.setStatusPriColor(statusId, "red");
                });
            });

            $("#er-show").click(function () {
                var list = hostContext.get_web().get_lists().getByTitle("ER Sample");
                var receivers = list.get_eventReceivers();
                appContext.load(receivers);
                appContext.executeQueryAsync(function () {
                    var enumerator = receivers.getEnumerator();
                    while (enumerator.moveNext()) {
                        var receiver = enumerator.get_current();
                        $("<div></div>").appendTo("#er-output").text(receiver.get_receiverName());
                    }
                }, function (xhr, err) {
                    var statusId = SP.UI.Status.addStatus("Exception when registering event receiver. EXCEPTION: " + err.get_message());
                    SP.UI.Status.setStatusPriColor(statusId, "red");
                });

            });

        }, 'sp.js');
    }, 'core.js');

});
