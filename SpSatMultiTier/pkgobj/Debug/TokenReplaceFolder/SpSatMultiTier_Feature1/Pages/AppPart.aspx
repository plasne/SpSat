<%@ Page language="C#" Inherits="Microsoft.SharePoint.WebPartPages.WebPartPage, Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

<WebPartPages:AllowFraming ID="AllowFraming" runat="server" />

<html>
<head>
    <title></title>

    <script type="text/javascript" src="../Scripts/jquery-2.1.4.min.js"></script>
    <script type="text/javascript" src="/_layouts/15/MicrosoftAjax.js"></script>
    <script type="text/javascript" src="/_layouts/15/sp.runtime.js"></script>
    <script type="text/javascript" src="/_layouts/15/sp.js"></script>

    <script type="text/javascript">
        // Set the style of the client web part page to be consistent with the host web.
        (function () {
            'use strict';

            var hostUrl = '';
            if (document.URL.indexOf('?') != -1) {
                var params = document.URL.split('?')[1].split('&');
                for (var i = 0; i < params.length; i++) {
                    var p = decodeURIComponent(params[i]);
                    if (/^SPHostUrl=/i.test(p)) {
                        hostUrl = p.split('=')[1];
                        document.write('<link rel="stylesheet" href="' + hostUrl + '/_layouts/15/defaultcss.ashx" />');
                        break;
                    }
                }
            }
            if (hostUrl == '') {
                document.write('<link rel="stylesheet" href="/_layouts/15/1033/styles/themable/corev15.css" />');
            }
        })();
    </script>

    <script type="text/javascript">

        $(document).ready(function () {

            // listen for adal_token when it is provided
            var adal_token;
            window.addEventListener("message", function (event) {
                if (event.origin == "https://pelasne.sharepoint.com") {
                    if (event.data.adal_token != null && event.data.adal_token != undefined) {
                        adal_token = event.data.adal_token;
                        var container = $("#multitier-container");
                        container.html("");

                        // request the events
                        $.ajax({
                            url: "https://spsat.azurewebsites.net/multitier/logs",
                            headers: { "Authorization": "Bearer " + adal_token },
                            dataType: "json",
                            success: function (logs) {

                                // build the list of events
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

                    }
                }
            }, false);

            // request the adal_token
            parent.window.postMessage("request(adal_token)", "https://pelasne.sharepoint.com");

        });

    </script>

</head>
<body>
    <div id="multitier-container">awaiting authentication...</div>
</body>
</html>
