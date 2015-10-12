
$(document).ready(function () {

    // init
    var context = new SP.ClientContext();
    var web = context.get_web();
    var actions = context.get_site().get_userCustomActions();

    // deploy
    $("#ui-deploy").click(function () {

        var newUserCustomAction = actions.add();
        newUserCustomAction.set_location("ScriptLink");
        newUserCustomAction.set_description("Custom UI");
        newUserCustomAction.set_scriptBlock("var customui_web = document.createElement('script'); customui_web.setAttribute('type', 'text/javascript'); customui_web.setAttribute('src', 'https://spsat.azurewebsites.net/customui.js'); document.getElementsByTagName('head').item(0).appendChild(customui_web);")
        newUserCustomAction.update();
        context.executeQueryAsync(function () {
            alert("success");
        }, function (xhr, err) {
            alert("failure - " + err.get_message());
        });

    });

    // retract
    $("#ui-retract").click(function () {

        context.load(actions);
        context.executeQueryAsync(function () {

            var deletions = [];
            var listOfActions = actions.getEnumerator();
            while (listOfActions.moveNext()) {
                var action = listOfActions.get_current();
                if (action.get_description() == "Custom UI") {
                    deletions.push(action);
                };
            }
            $(deletions).each(function (i, action) {
                action.deleteObject();
            });
            context.executeQueryAsync(function () {
                alert("success");
            }, function (xhr, err) {
                alert("failure - " + err.get_message());
            });

        }, function (xhr, err) {
            alert("failure - " + err.get_message());
        });

    });

});
