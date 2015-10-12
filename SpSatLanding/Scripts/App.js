
var appWebUrl;
var hostWebUrl;
var appContext;
var hostContext;
var accessToken;

function getQueryStringParameter(paramToRetrieve) {
    var params = document.URL.split("?")[1].split("&");
    var strParams = "";
    for (var i = 0; i < params.length; i = i + 1) {
        var singleParam = params[i].split("=");
        if (singleParam[0] == paramToRetrieve)
            return singleParam[1];
    }
}

function copyFile(path, filename, list) {
    var deferred = new $.Deferred();
    try {

        // download the file
        $.ajax({
            url: path + filename,
            converters: { // this prevents javascript from executing when downloaded
                "text script": function (text) {
                    return text;
                }
            },
            success: function (file) {

                // upload the file
                var digest = $("#__REQUESTDIGEST").val();
                var url = appWebUrl + "/_api/SP.AppContextSite(@TargetSite)/web/lists/getByTitle(@TargetLibrary)/RootFolder/Files/add(url=@TargetFileName,overwrite='true')?" +
                    "@TargetSite='" + hostWebUrl + "'" +
                    "&@TargetLibrary='" + list + "'" +
                    "&@TargetFileName='" + filename + "'";
                var reqExecutor = new SP.RequestExecutor(appWebUrl);
                reqExecutor.executeAsync({
                    url: url,
                    method: "POST",
                    headers: {
                        "Accept": "application/json; odata=verbose",
                        "X-RequestDigest": digest
                    },
                    contentType: "application/json;odata=verbose",
                    binaryStringRequestBody: true,
                    body: file,
                    success: function () {

                        // success
                        var statusId = SP.UI.Status.addStatus("Successfully uploaded " + filename + ".");
                        SP.UI.Status.setStatusPriColor(statusId, "green");
                        deferred.resolve();

                    },
                    error: function (sender, code, error) {

                        // upload failed
                        var msg = $.parseJSON(sender.body).error.message.value;
                        var statusId = SP.UI.Status.addStatus("Upload failed for " + filename + ". EXCEPTION: " + msg);
                        SP.UI.Status.setStatusPriColor(statusId, "red");
                        deferred.reject();

                    }
                });
            },
            error: function (xhr, status, error) {

                // download failed
                var statusId = SP.UI.Status.addStatus("Download failed for " + filename + ". EXCEPTION: " + error);
                SP.UI.Status.setStatusPriColor(statusId, "red");
                deferred.reject();

            }
        });

    } catch (ex) {

        // exception
        var statusId = SP.UI.Status.addStatus("Code exception when copying the file " + filename + ". EXCEPTION: " + ex.message);
        SP.UI.Status.setStatusPriColor(statusId, "red");
        deferred.reject();

    }
    return deferred;
}

function addContentEditorWebPartToForm(title, pageRef, htmlRef, zone, index) {
    var deferred = new $.Deferred();
    try {

        // find out if the web part is already on the page
        var web = hostContext.get_web();
        var file = web.getFileByServerRelativeUrl(pageRef);
        var mgr = file.getLimitedWebPartManager(SP.WebParts.PersonalizationScope.shared);
        var webparts = mgr.get_webParts();
        appContext.load(webparts, "Include(WebPart)");
        appContext.executeQueryAsync(function () {

            // iterate through all existing web parts
            var found = false;
            var enumerator = webparts.getEnumerator();
            while (enumerator.moveNext()) {
                var defin = enumerator.get_current();
                var webpart = defin.get_webPart();
                if (webpart.get_title() == title) found = true;
            }

            // if the web part wasn't found, add it
            if (found) {
                deferred.resolve();
            } else {

                // define
                var xml =
                    '<?xml version="1.0" encoding="utf-8"?>' +
                    '<WebPart xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.microsoft.com/WebPart/v2">' +
                        '<Title>' + title + '</Title>' +
                        '<FrameType>None</FrameType>' +
                        '<Description>For Landing Page.</Description>' +
                        '<PartImageLarge>/_layouts/15/images/mscontl.gif</PartImageLarge>' +
                        '<IsVisible>true</IsVisible>' +
                        '<Assembly>Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c</Assembly>' +
                        '<TypeName>Microsoft.SharePoint.WebPartPages.ContentEditorWebPart</TypeName>' +
                        '<ContentLink xmlns="http://schemas.microsoft.com/WebPart/v2/ContentEditor">' + htmlRef + '</ContentLink>' +
                        '<Content xmlns="http://schemas.microsoft.com/WebPart/v2/ContentEditor" />' +
                        '<PartStorage xmlns="http://schemas.microsoft.com/WebPart/v2/ContentEditor" />' +
                    '</WebPart>';

                // construct
                var defin = mgr.importWebPart(xml);
                var webpart = defin.get_webPart();
                mgr.addWebPart(webpart, zone, index);

                // apply
                appContext.executeQueryAsync(function () {

                    // success
                    var statusId = SP.UI.Status.addStatus("Successfully added a web part to " + htmlRef + ".");
                    SP.UI.Status.setStatusPriColor(statusId, "green");
                    deferred.resolve();

                }, function (sender, error) {

                    // failed to add
                    var statusId = SP.UI.Status.addStatus("Failed to add a web part to " + htmlRef + ". EXCEPTION: " + error.get_message());
                    SP.UI.Status.setStatusPriColor(statusId, "red");
                    deferred.reject();

                });

            }

        }, function (sender, error) {

            // couldn't get current status
            var statusId = SP.UI.Status.addStatus("Couldn't determine whether the web part was already on " + htmlRef + ". EXCEPTION: " + error.get_message());
            SP.UI.Status.setStatusPriColor(statusId, "red");
            deferred.reject();

        });

    } catch (ex) {

        // exception
        var statusId = SP.UI.Status.addStatus("Code exception when adding a web part to " + htmlRef + ". EXCEPTION: " + ex.message);
        SP.UI.Status.setStatusPriColor(statusId, "red");
        deferred.reject();

    }
    return deferred;
}

function copyAllFiles() {
    var deferred = new $.Deferred();
    try {

        // copy all files for the landing page
        copyFile("../Scripts/", "jquery-2.1.4.min.js", "Site Assets").then(function () {
            copyFile("../Pages/", "chart.html", "Site Assets").then(function () {
                copyFile("../Scripts/", "material-charts.js", "Site Assets").then(function () {
                    copyFile("../Content/", "material-charts.css", "Site Assets").then(function () {
                        copyFile("../Pages/", "quote.html", "Site Assets").then(function () {
                            deferred.resolve();
                        }, function () {
                            deferred.reject();
                        });
                    }, function () {
                        deferred.reject();
                    });
                }, function () {
                    deferred.reject();
                });
            }, function () {
                deferred.reject();
            });
        }, function () {
            deferred.reject();
        });

    } catch (ex) {

        // exception
        var statusId = SP.UI.Status.addStatus("Code exception when copying files. EXCEPTION: " + error.get_message());
        SP.UI.Status.setStatusPriColor(statusId, "red");
        deferred.reject();

    }
    return deferred;
}

function addAllWebParts() {
    var deferred = new $.Deferred();
    try {

        // chart
        var pageRef1 = hostWebUrl.replace("https://pelasne.sharepoint.com", "") + "/SitePages/Landing.aspx";
        var htmlRef1 = hostWebUrl.replace("https://pelasne.sharepoint.com", "") + "/SiteAssets/chart.html";
        addContentEditorWebPartToForm("Chart", pageRef1, htmlRef1, "Main", 10).then(function () {

            // quote
            var htmlRef2 = hostWebUrl.replace("https://pelasne.sharepoint.com", "") + "/SiteAssets/quote.html";
            addContentEditorWebPartToForm("Quote", pageRef1, htmlRef2, "Main", 10).then(function () {
                deferred.resolve();
            }, function () {
                deferred.reject();
            });

        }, function () {
            deferred.reject();
        });

    } catch (ex) {

        // exception
        var statusId = SP.UI.Status.addStatus("Code exception when adding web parts. EXCEPTION: " + error.get_message());
        SP.UI.Status.setStatusPriColor(statusId, "red");
        deferred.reject();

    }
    return deferred;
}

$(document).ready(function () {

    // wait for status and notifications
    ExecuteOrDelayUntilScriptLoaded(function () {
        ExecuteOrDelayUntilScriptLoaded(function () {

            // get the context variables
            appWebUrl = decodeURIComponent(getQueryStringParameter("SPAppWebUrl"));
            hostWebUrl = decodeURIComponent(getQueryStringParameter("SPHostUrl"));
            appContext = new SP.ClientContext(appWebUrl);
            hostContext = new SP.AppContextSite(appContext, hostWebUrl);
            accessToken = decodeURIComponent(getQueryStringParameter("adal_token"));

            // get volumes
            $("#landing-getvol").click(function () {
                $.ajax({
                    url: "https://spsat.azurewebsites.net/landing/volumes",
                    headers: { "Authorization": "Bearer " + accessToken },
                    dataType: "json",
                    success: function (volumes) {
                        $("#landing-output").text(JSON.stringify(volumes));
                    },
                    error: function (xhr, msg) {
                        $("#landing-output").text(msg);
                    }
                });
            });

            // get quote
            $("#landing-getquote").click(function () {
                $.ajax({
                    url: "https://spsat.azurewebsites.net/landing/stockquote",
                    dataType: "json",
                    success: function (quote) {
                        $("#landing-output").text(JSON.stringify(quote));
                    },
                    error: function (xhr, msg) {
                        $("#landing-output").text(msg);
                    }
                });
            });

            // copy all files
            $("#landing-copy").click(function () {
                SP.UI.Status.removeAllStatus(true);
                copyAllFiles();
            });

            // add all web parts
            $("#landing-add").click(function () {
                SP.UI.Status.removeAllStatus(true);
                addAllWebParts();
            });

            // read the property bag
            $("#landing-propbag").click(function () {
                SP.UI.Status.removeAllStatus(true);
                var web = hostContext.get_web();
                var webProperties = web.get_allProperties();
                appContext.load(webProperties);
                appContext.executeQueryAsync(function () {
                    var token = webProperties.get_item("accessToken");
                    $("#landing-output").text(token);
                }, function (xhr, err) {
                    var statusId = SP.UI.Status.addStatus("Exception when reading from the property bag. EXCEPTION: " + err.get_message());
                    SP.UI.Status.setStatusPriColor(statusId, "red");
                });
            });

        }, 'sp.js');
    }, 'core.js');

});
