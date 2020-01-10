
var BOT = {

    baseURL: "http://localhost:5000/api/Bot/",

    countText: 0,
    countServerText: 0,

    init: function () {

        if (document.location.href.toLowerCase().indexOf("localhost") > -1) {
            BOT.baseURL = "http://localhost:5000/api/Bot/";
        }

        BOT.bemVindo();

        setTimeout(function () {
            document.getElementById("preloader").style.opacity = "0";

            setTimeout(function () {
                document.getElementById("preloader").style.display = "none";
                document.getElementById("query").focus();
            }, 500);

        }, 2000);


        document.getElementById("agentForm").onsubmit = function (e) {
            e.preventDefault();
            BOT.submitText(e);
            document.getElementById("query").focus();
            return false;
        }

        document.getElementById("mic").onclick = function () {

            recVoz.startDictation();
        }

        document.getElementById("close").onclick = function () {
            window.close();
                
        }
    },

    submitText: function () {

        var q = document.getElementById("query").value.trim();
        var sessionId = document.getElementById("hfSessionId").value.trim();

        if (q != "") {

            var idElementResponse = BOT.addUserText(q);

            var promisse = BOT.sendIntent(q, sessionId, idElementResponse);

            promisse.then(function (r) {

                if (r.error == null) {

                    var request = r[0]; //retorno da promisses
                    var idElementResponse = r[1]; //retorno da promisses

                    BOT.addAgentText(
                        {
                            agentResponse: request,
                            idElementResponse: idElementResponse
                        });

                    document.getElementById("query").value = "";
                }
                else {
                    BOT.addAgentText({
                        error: r.error,
                        idElementResponse: idElementResponse
                    })
                        (r.error, idElementResponse, redirectTo, imageURL, list, true);
                }
            });

            promisse.catch(function (idElementResponse) {
                BOT.addAgentText({
                    error: "Um erro ocorreu. Tente novamente.",
                    idElementResponse: idElementResponse
                });
            });

        }

        recVoz.endDictation();

    },

    addUserText: function (text) {

        var result = document.getElementById("result");
        result.innerHTML += "<div class=\"user-request tri-right\">" + text + "</div>";

        BOT.countServerText++;
        var idElementResponse = "response" + BOT.countServerText;

        result.innerHTML += "<div id=\"" + idElementResponse + "\" class=\"server-response server-typing\">" +
            "<div class=\"typing\"></div><div class=\"typing\"></div><div class=\"typing\"></div>" +
            "</div > ";
        result.scrollIntoView(false);

        return idElementResponse;

    },

    addAgentText: function (obj) {

        var textOk = "";
        var redirectTo = "";
        var imageURL = "";
        var list = null;
        var listHTML = "";
        var textHTML = "";

        var agentResponse = obj.agentResponse;
        var idElementResponse = obj.idElementResponse
        var error = obj.error;
        if (error != null && error != "") {
            textOk = error;
        }
        else {
            try {

                var temPayLoadRedirectTo = (
                    agentResponse.fulfillmentMessages.length >= 1 &&
                    agentResponse.fulfillmentMessages[1] != null &&
                    agentResponse.fulfillmentMessages[1].payload != null &&
                    agentResponse.fulfillmentMessages[1].payload.fields.redirectTo != null);

                if (temPayLoadRedirectTo)
                    redirectTo = agentResponse.fulfillmentMessages[1].payload.fields.redirectTo.stringValue;


                var temImageURL = (
                    agentResponse.fulfillmentMessages.length >= 1 &&
                    agentResponse.fulfillmentMessages[1] != null &&
                    agentResponse.fulfillmentMessages[1].payload != null &&
                    agentResponse.fulfillmentMessages[1].payload.fields.imageURL != null);

                if (temImageURL)
                    imageURL = agentResponse.fulfillmentMessages[1].payload.fields.imageURL.stringValue;

                var temList = agentResponse.fulfillmentMessages.length >= 1 &&
                    agentResponse.fulfillmentMessages[1] != null &&
                    agentResponse.fulfillmentMessages[1].payload != null &&
                    agentResponse.fulfillmentMessages[1].payload.fields.list != null

                if (temList) {
                    list = {
                        replacementKey: agentResponse.fulfillmentMessages[1].payload.fields.list.structValue.fields.replacementKey.stringValue,
                        invokeEvent: agentResponse.fulfillmentMessages[1].payload.fields.list.structValue.fields.invokeEvent.boolValue,
                        afterDialog: agentResponse.fulfillmentMessages[1].payload.fields.list.structValue.fields.afterDialog.boolValue,
                        itemsName: [],
                        itemsEventName: []
                    }

                    var itemsName = agentResponse.fulfillmentMessages[1].payload.fields.list.structValue.fields.itemsName.listValue.values;
                    var itemsEventName = agentResponse.fulfillmentMessages[1].payload.fields.list.structValue.fields.itemsEventName.listValue.values;

                    itemsName.map(function (item) {
                        list.itemsName.push(item.stringValue);
                    });

                    itemsEventName.map(function (item) {
                        list.itemsEventName.push(item.stringValue);
                    });
                }


                var temTextHTML = agentResponse.fulfillmentMessages.length >= 1 &&
                    agentResponse.fulfillmentMessages[1] != null &&
                    agentResponse.fulfillmentMessages[1].payload != null &&
                    agentResponse.fulfillmentMessages[1].payload.fields.textHTML != null

                if (temTextHTML) {

                    textHTML = agentResponse.fulfillmentMessages[1].payload.fields.textHTML.stringValue;
                }
                else {
                    temTextHTML = agentResponse.webhookPayload.fields.textHTML != null;
                    if (temTextHTML)
                        textHTML = agentResponse.webhookPayload.fields.textHTML.stringValue
                }
            }
            catch (ex) { };

            textOk = agentResponse.fulfillmentText;

            if (temTextHTML) {
                textOk = textHTML;
            }

            if (imageURL != null && imageURL != "") {

                textOk = "<img src=\"" + imageURL + "\" />" + textOk;
            }

            if (redirectTo != null && redirectTo != "") {
                textOk = "<a target=\"blank\" href=\"" + redirectTo + "\"> " + textOk + "</a>";
            }

            if (list != null) {
                try {
                    var listHTML = ""

                    for (var i = 0; i < list.itemsName.length; i++) {
                        listHTML += "<li data-eventname=\"" + list.itemsEventName[i] + "\">" + list.itemsName[i] + "</li>";
                    }

                    listHTML = "<ul data-invokeevent=\"" + list.invokeEvent + "\">" + listHTML + "</ul>";

                }
                catch (ex) {
                    listHTML = "error: valid items.";
                }
            }
        }


        var result = document.getElementById("result");
        var ballon = null;
        if (idElementResponse == null) {

            ballon = document.createElement("div");
            result.appendChild(ballon);
        }
        else {
            ballon = document.getElementById(idElementResponse);
            ballon.innerHTML = textOk;
        }

        ballon.classList.remove("server-typing");
        ballon.classList.add("server-response");
        if (error != null && error != "") {
            ballon.classList.add("server-response-error");
        }
        ballon.classList.add("tri-left");

        if (list != null && !list.afterDialog)
            textOk = textOk.replace(list.replacementKey, listHTML);

        ballon.innerHTML = textOk;

        if (list != null && list.afterDialog) {
            var listAfterDialog = document.createElement("div");
            listAfterDialog.innerHTML = listHTML;
            listAfterDialog.classList.add("server-response");
            listAfterDialog.classList.add("list");

            result.appendChild(listAfterDialog);
        }

        BOT.listOnClick();

        result.scrollIntoView(false);
    },

    sendIntent: function (q, sessionId, idElementResponse) {

        var promisse = new Promise(function (resolve, reject) {

            var req = $.ajax({
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                url: BOT.baseURL + "DetectIntentFromTexts?q=" + encodeURI(q) + "&sessionid=" + sessionId,
                type: "GET"
            });

            req.done(function (r) {

                resolve([r, idElementResponse]);
            });

            req.fail(function (jqxhr, status, msg) {
                reject(idElementResponse);
            });
        });

        return promisse;

    },

    invokeEvent: function (eventName, sessionId, idElementResponse) {

        var promisse = new Promise(function (resolve, reject) {

            var req = $.ajax({
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                url: BOT.baseURL + "GetEvent?eventName=" + eventName + "&sessionid=" + sessionId,
                type: "GET"
            });

            req.done(function (r) {
                resolve([r, idElementResponse]);
            });

            req.fail(function (jqxhr, status, msg) {
                reject(jqxhr);
            });
        });

        return promisse;

    },

    bemVindo: function () {

        console.log("bem-vindo");
        var sessionId = document.getElementById("hfSessionId").value.trim();

        var eventNameBemVindo = "Welcome";
        if (utils.queryString("init") != null) {
            eventNameBemVindo = utils.queryString("init");
        }

        var promisse = BOT.invokeEvent(eventNameBemVindo, sessionId, null, null);

        promisse.then(function (r) {
            if (r.error == null) {

                var agentResponse = r[0]; //retorno da promisses
                var idElementResponse = r[1]; //retorno da promisses

                BOT.addAgentText(
                    {
                        agentResponse: agentResponse,
                        idElementResponse: idElementResponse
                    });
            }
        });

        promisse.catch(function (e) {
            console.log("Não foi possível invocar um evento. " + JSON.stringify(e));
        });

    },

    listOnClick: function () {

        var lists = document.querySelectorAll("#result ul[data-invokeevent=true] li");
        if (lists.length == 0)
            return;

        for (var i = 0; i < lists.length; i++) {
            lists[i].onclick = function () {


                var result = document.getElementById("result");
                BOT.countServerText++;
                var idElementResponse = "response" + BOT.countServerText;

                result.innerHTML += "<div id=\"" + idElementResponse + "\" class=\"server-response server-typing\">" +
                    "<div class=\"typing\"></div><div class=\"typing\"></div><div class=\"typing\"></div>" +
                    "</div > ";
                result.scrollIntoView(false);


                var sessionId = document.getElementById("hfSessionId").value.trim();
                var promisse = BOT.invokeEvent(this.getAttribute("data-eventname"), sessionId, idElementResponse);

                promisse.then(function (r) {
                    if (r.error == null) {

                        var agentResponse = r[0]; //retorno da promisses
                        var idElementResponse = r[1];
                        BOT.addAgentText({
                            agentResponse: agentResponse,
                            idElementResponse: idElementResponse
                        });
                    }
                });

                promisse.catch(function () {
                    console.log("Não foi possível invocar a intent.");
                    BOT.addAgentText({
                        error: "Um erro ocorreu"
                    });
                });

            }
        }

    }


}



var recVoz = {

    recognition: null,
    recognizing: false,

    init: function () {

        if (!utils.isMobile() ||
            document.location.href.indexOf("https://") == -1) {

            document.getElementById("mic").style.display = "none";
            return;
        }

        try {
            if (window.parent != null)
                var x = window.parent.document;
        }
        catch (ex) {
            document.getElementById("mic").style.display = "none";
            return;
        }


        var speechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition || null;

        if (speechRecognition == null) {
            document.getElementById("mic").style.display = "none";
        }
        else {
            document.getElementById("mic").style.display = "block";
            recVoz.recognition = new speechRecognition();
            recVoz.recognition.continuous = false;
            recVoz.recognition.interimResults = true;

            recVoz.recognition.onstart = function () {
                recVoz.recognizing = true;
            };

            recVoz.recognition.onerror = function (event) {
                recVoz.recognizing = false;
                console.log("Microfone não identificado ou sem permissão (" + event.error + ").");
                recVoz.endDictation();
                document.getElementById("mic").style.display = "none";

            };

            recVoz.recognition.onend = function () {
                recVoz.recognizing = false;
            };

            recVoz.recognition.onresult = function (event) {

                if (event.results != null && event.results[0].isFinal) {
                    document.getElementById("query").value = event.results[0][0].transcript;
                    BOT.submitText();

                }


            };
        }
    },

    endDictation: function (event) {
        recVoz.recognition.stop();
        document.getElementById("mic").classList.remove("active");

    },

    startDictation: function (event) {

        document.getElementById("mic").classList.remove("active");

        if (recVoz.recognizing) {
            recVoz.recognition.stop();
            return;
        }
        recVoz.recognition.lang = 'pt-br';
        recVoz.recognition.start();
        document.getElementById("query").value = "";


        document.getElementById("mic").classList.add("active");
    }
}


BOT.init();
recVoz.init();
