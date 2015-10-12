
function getQueryStringParameter(paramToRetrieve) {
    var params = document.URL.split("?");
    if (params.length > 1) params = params[1].split("&");
    var strParams = "";
    for (var i = 0; i < params.length; i = i + 1) {
        var singleParam = params[i].split("=");
        if (singleParam[0] == paramToRetrieve)
            return singleParam[1];
    }
}

// if not in edit mode, look for any requests to authenticate App Parts
ExecuteOrDelayUntilScriptLoaded(function () {
    if (SP.Ribbon.PageState.Handlers.isInEditMode() == false) {

        // listen for any AppParts that request the adal_token
        window.addEventListener("message", function (event) {
            if (event.origin.substring(0, 15) == "https://pelasne" && event.origin.substr(event.origin.length - 15) == ".sharepoint.com") {
                if (event.data == "request(adal_token)") {

                    // if there are no credentials, request them via ADAL, otherwise return them
                    var adal_token = getQueryStringParameter("adal_token");
                    if (adal_token == undefined) {
                        window.location.href = "https://spsat.azurewebsites.net/adal.html?SPRedirectPage=" + encodeURIComponent(window.location.href);
                    } else {
                        event.source.postMessage({ "adal_token": decodeURIComponent(adal_token) }, event.origin);
                    }

                }
            }
        }, false);
    }
}, 'sp.ribbon.js');

// styling
var siteIcons = document.getElementsByClassName("ms-siteicon-img");
for (var i = 0; i < siteIcons.length; i++) {
    siteIcons[i].setAttribute("src", "https://spsat.azurewebsites.net/siteIcon.png");
}
document.getElementById("RibbonContainer").style.backgroundColor = "#CCCCFF";
