/// <reference path="jquery-1.10.2.js" />
/// <reference path="bootstrap.js" />

var cflashsoftUtil = {

    rootPath: null, //Set by View
    apiPath: null, //Set by View
    queryString: {},
    isAndroid: false,
    _isBusy: false,
    waitSpinner: null,
    currentPopup: null,
    overlays: null,
    alertBox: null,
    alertBoxOkCallback: null,
    alertBoxCancelCallback: null,
    clientFunctions: {},

    init: function () {
        this.queryString = this.parseQueryString(window.location.search.substring(1));
        this.isAndroid = /android/i.test(navigator.userAgent.toLowerCase());
        var body = $("body");
        this.waitSpinner = $("<div style=\"position:absolute;z-index:10001;width:100px;height:100px;display:none\"><img src=\"" + this.rootPath + "images/cflashsoft/spinner.gif\" width=\"100\" height=\"100\" /></div>").appendTo(body);
        this.overlays = [$("<div class=\"overlay1\" style=\"position:fixed;z-index:10000;width:5px;height:5px;display:none\"></div>").appendTo(body),
        $("<div class=\"overlay2\" style=\"position:fixed;z-index:10000;width:5px;height:5px;display:none\"></div>").appendTo(body)];
        this.alertBox = $("<div id=\"cflashsoftAlertBox\" class=\"modal fade\" data-keyboard=\"false\" data-backdrop=\"static\" data-focus=\"true\" style=\"display:none;\"><div class=\"modal-dialog\"><div class=\"modal-content\"><div class=\"modal-body\"><span id=\"cflashsoftAlertBoxOkIcon\" class=\"text-success glyphicon glyphicon-ok\" style=\"display:none;\"></span><span id=\"cflashsoftAlertBoxHandIcon\" class=\"text-primary glyphicon glyphicon-hand-right\" style=\"display:none;\"></span><span id=\"cflashsoftAlertBoxInfoIcon\" class=\"text-info glyphicon glyphicon-info-sign\" style=\"display:none;\"></span><span id=\"cflashsoftAlertBoxQuestIcon\" class=\"text-primary glyphicon glyphicon-question-sign\" style=\"display:none;\"></span><span id=\"cflashsoftAlertBoxExclIcon\" class=\"text-warning glyphicon glyphicon-exclamation-sign\" style=\"display:none;\"></span><span id=\"cflashsoftAlertBoxErrIcon\" class=\"text-danger glyphicon glyphicon-remove-sign\" style=\"display:none;\"></span>&nbsp;<span id=\"cflashsoftAlertBoxMessage\"></span></div><div class=\"modal-footer\"><button id=\"cflashsoftAlertOk\" type=\"button\" class=\"btn btn-success\" onclick=\"cflashsoftUtil.alertOk();\"><span class=\"glyphicon glyphicon-ok\" aria-hidden=\"true\"></span> Ok</button><button id=\"cflashsoftAlertCancel\" type=\"button\" class=\"btn btn-default\" onclick=\"cflashsoftUtil.alertCancel();\">Cancel</button></div></div></div></div>").appendTo(body);
        this.alertBox.on('shown.bs.modal', function () {
            cflashsoftUtil.alertBox.find("button:first").focus();
        });
    },

    getApiData: function (name, params) {
        return this.apiAjax("GET", name, params, "json", null);
    },

    postApiData: function (name, data) {
        return this.apiAjax("POST", name, JSON.stringify(data), "json", null);
    },

    putApiData: function (name, data) {
        return this.apiAjax("PUT", name, JSON.stringify(data), "json", null);
    },

    deleteApiData: function (name, data) {
        return this.apiAjax("DELETE", name, JSON.stringify(data), "json", null);
    },

    getApiDataAsync: function (name, params, successCallback, failureCallback) {
        return this.apiAjaxAsync("GET", name, params, successCallback, failureCallback, "json");
    },

    postApiDataAsync: function (name, data, successCallback, failureCallback) {
        return this.apiAjaxAsync("POST", name, JSON.stringify(data), successCallback, failureCallback, "json");
    },

    postApiFormDataAsync: function (name, data, successCallback, failureCallback) {
        return this.apiFormAjaxAsync("POST", name, data, successCallback, failureCallback, "json");
    },

    postApiTextDataAsync: function (name, data, successCallback, failureCallback) {
        return this.apiAjaxAsync("POST", name, JSON.stringify(data), successCallback, failureCallback, "text");
    },

    putApiDataAsync: function (name, data, successCallback, failureCallback) {
        return this.apiAjaxAsync("PUT", name, JSON.stringify(data), successCallback, failureCallback, "json");
    },

    deleteApiDataAsync: function (name, data, successCallback, failureCallback) {
        return this.apiAjaxAsync("DELETE", name, JSON.stringify(data), successCallback, failureCallback, "json");
    },

    apiAjax: function (type, name, data, dataType, errorResult) {
        this.resetAutoLogout(true);
        var result = null;
        var contentType = "";
        if (dataType == "json")
            contentType = "application/json";
        $.ajax({
            type: type,
            url: this.apiPath + name,
            data: data,
            dataType: dataType,
            async: false,
            cache: false,
            timeout: 120000,
            success: function (resultData) { result = resultData; },
            failure: function (xhr, textStatus, errorThrown) {
                if (typeof errorResult !== "undefined") result = errorResult; else throw -1;
            },
            error: function (xhr, textStatus, errorThrown) {
                if (typeof errorResult !== "undefined") result = errorResult; else throw -1;
            },
            processData: false,
            contentType: contentType,
            xhrFields: {
                withCredentials: true
            }
        });
        return result;
    },

    apiAjaxAsync: function (type, name, data, successCallback, failureCallback, dataType) {
        this.resetAutoLogout(true);
        var contentType = "";
        if (dataType == "json")
            contentType = "application/json";
        return $.ajax({
            type: type,
            url: this.apiPath + name,
            data: data,
            dataType: dataType,
            async: true,
            cache: false,
            timeout: 120000,
            success: successCallback,
            failure: failureCallback,
            error: failureCallback,
            processData: false,
            contentType: false,
            xhrFields: {
                withCredentials: true
            }
        });
    },

    apiFormAjaxAsync: function (type, name, data, successCallback, failureCallback, dataType) {
        this.resetAutoLogout(true);
        var contentType = "";
        //if (dataType == "json")
        //    contentType = "application/json";
        return $.ajax({
            url: this.apiPath + name,
            dataType: "json",
            type: type,
            async: true,
            data: data,
            processData: false,
            contentType: false,
            timeout: 120000,
            success: successCallback,
            failure: failureCallback,
            error: failureCallback,
            xhrFields: {
                withCredentials: true
            }
        });
    },

    isBusy: function (value) {
        var result = false;
        if (typeof value === "undefined") {
            result = this._isBusy;
        }
        else {
            this._isBusy = value;
            if (value)
                this.showWaitSpinner();
            else
                this.hideWaitSpinner();
            result = value;
        }
        return result;
    },

    showWaitSpinner: function () {
        var w = $(window);
        var spinner = this.waitSpinner;
        this.showOverlay(0);
        spinner.show();
        spinner.css("top", Math.max(0, ((w.height() - spinner.outerHeight()) / 2) + w.scrollTop()) + "px");
        spinner.css("left", Math.max(0, ((w.width() - spinner.outerWidth()) / 2) + w.scrollLeft()) + "px");
    },

    hideWaitSpinner: function () {
        this.waitSpinner.hide();
        this.hideOverlay();
    },

    tipTimer: null,

    showTip: function (box, target, html, autohide) {
        if (html != null) {
            box.html(html);
        }
        box.show();
        box.position({ my: "left top", at: "left top+100%", of: target, collision: "flipfit" });
        box.hide();
        box.fadeIn();
        if (autohide) {
            if (this.tipTimer != null) {
                try {
                    window.clearTimeout(this.tipTimer);
                }
                catch (err) { }
            }
            this.tipTimer = window.setTimeout(function () { cflashsoftUtil.tipTimer = null; box.fadeOut(); }, 5000);
        }
    },

    hideTip: function (box) {
        box.fadeOut();
    },

    showOverlay: function (index) {
        var overlay = this.overlays[index];
        overlay.show();
        overlay.css("top", "0");
        overlay.css("left", "0");
        overlay.css("width", "100%");
        overlay.css("height", "100%");
    },

    hideOverlay: function () {
        for (var index = 0; index < this.overlays.length; index++) {
            this.overlays[index].hide();
        }
    },

    showPopup: function (popup, cssClass, overlayIndex) {
        if (this.currentPopup == null) {
            if (typeof overlayIndex === "undefined")
                overlayIndex = 0;
            this.showOverlay(overlayIndex);
            if (cssClass)
                popup.attr("class", cssClass);
            popup.show();
            this.centerPopup(popup);
            this.currentPopup = popup;
            $(window).on("resize", this.onWindowResizeForPopup);
        }
    },

    hidePopup: function () {
        var popup = this.currentPopup;
        $(window).off("resize", this.onWindowResizeForPopup);
        this.currentPopup = null;
        popup.hide();
        this.hideOverlay();
    },

    centerPopup: function (popup) {
        popup.css("position", "absolute");
        popup.css("top", Math.max(0, (($(window).height() - popup.outerHeight()) / 2) + $(window).scrollTop()) + "px");
        popup.css("left", Math.max(0, (($(window).width() - popup.outerWidth()) / 2) + $(window).scrollLeft()) + "px");
    },

    showErrorAlert: function (id, text) {
        var alert = $("#" + id);
        alert.find("span:first").empty().html(text);
        alert.show();
    },

    hideErrorAlert: function (id) {
        var alert = $("#" + id);
        alert.hide();
    },

    findKeyValueItemValue: function (items, key) {
        var result = null;
        if (items != null) {
            for (var index = 0; index < items.length; index++) {
                if (items[index].Key == key) {
                    result = items[index].Value;
                    break;
                }
            }
        }
        return result;
    },

    findKeyValueItem: function (items, key) {
        var result = null;
        if (items != null) {
            for (var index = 0; index < items.length; index++) {
                if (items[index].Key == key) {
                    result = items[index];
                    break;
                }
            }
        }
        return result;
    },

    findInArray: function (items, property, value) {
        var result = null;
        for (var index = 0; index < items.length; index++) {
            if (property == null) {
                if (items[index] == value) {
                    result = items[index];
                    break;
                }
            }
            else {
                if (items[index][property] == value) {
                    result = items[index];
                    break;
                }
            }
        }
        return result;
    },

    isNumericCharCode: function (charCode, includeDot, box, target, errorMessage) {
        var result = (charCode >= 48 && charCode <= 57) || (includeDot && charCode == 46);
        if (box != null && !result) {
            this.showTip(box, target, errorMessage, true);
        }
        return result;
    },

    getElementValue: function (el) {
        var result = "";
        var item = $(el)
        switch (item.prop("tagName").toLowerCase()) {
            case "input":
                switch (item.attr("type").toLowerCase()) {
                    case "text":
                        result = item.val();
                        break;
                    case "checkbox":
                    case "radio":
                        result = (item.is(":checked") ? "true" : "false");
                        break;
                }
                break;
            case "textarea":
                result = item.val();
                break;
            case "select":
                //TODO: might not want to rely on toString() to get comma-delimited string
                //change this to a loop and erase commas that might exist in the field
                result = item.val().toString();
                break;
        }
        return result;
    },

    setElementValue: function (el, value, forPrint) {
        var item = $(el);
        switch (item.prop("tagName").toLowerCase()) {
            case "input":
                switch (item.attr("type").toLowerCase()) {
                    case "text":
                        if (forPrint) {
                            item.replaceWith(this.getSafeValue(value));
                        }
                        else {
                            item.val(value);
                        }
                        break;
                    case "checkbox":
                    case "radio":
                        if (forPrint) {
                            if (value.toLowerCase() == "true") {
                                item.replaceWith("[X]");
                            }
                            else {
                                item.replaceWith("[&nbsp;]");
                            }
                        }
                        else {
                            item.prop("checked", (value.toLowerCase() == "true"))
                        }
                        break;
                }
                break;
            case "textarea":
                if (forPrint) {
                    item.replaceWith(this.getSafeValue(value));
                }
                else {
                    item.val(value);
                }
                break;
            case "select":
                item.val(value.split(","));
                break;
            case "div":
            case "span":
                item.html(value);
                break;
        }
    },

    getSafeValue: function (value) {
        if (value == null || value == "") {
            return "&nbsp;";
        }
        else {
            return value;
        }
    },

    checkboxAsRadioClick: function (el) {
        var checkbox = $(el);
        $("[name='" + checkbox.attr("name") + "']").each(function (i) {
            var item = $(this);
            if (!item.is(checkbox)) {
                item.prop("checked", false);
            }
        });
    },

    formatErrors: function (errors) {
        var result = "";
        if (errors != null && errors.length > 0) {
            if (errors.length > 1) {
                result += "<ul>"
                for (var index = 0; index < errors.length; index++) {
                    var item = errors[index];
                    if (item.constructor === Array) {
                        for (var index2 = 0; index2 < item.length; index2++) {
                            result += "<li>";
                            result += item[index2];
                            result += "</li>";
                        }
                    }
                    else {
                        result += "<li>";
                        result += item;
                        result += "</li>";
                    }
                }
                result += "</ul>";
            }
            else {
                result += "<span>" + errors[0] + "</span>";
            }
        }
        return result;
    },

    formatRangeString: function (n1, n2, singular, plural) {
        var result = "";
        if (n1 == 0 && n2 == 0)
            result = "";
        else if (n1 == n2)
            result = n1.toString() + singular;
        else if (n1 == 0)
            result = n2.toString() + singular;
        else if (n2 == 0)
            result = n1.toString() + singular;
        else
            result = n1.toString() + " - " + n2.toString() + plural;
        return result;
    },

    formatShortDateString: function (date) {
        if (date != null && date != "")
            return (date.getMonth() + 1) + "/" + date.getDate() + "/" + date.getFullYear();
        else
            return "N/A";
    },

    formatLongDateString: function (date) {
        if (date != null && date != "") {
            var result = this.formatShortDateString(date);
            result += " " + this.formatTimeString(date);
            return result;
        }
        else {
            return "N/A";
        }
    },

    formatTimeString: function (date) {
        if (date != null && date != "") {
            var result = "";
            var hours = date.getHours();
            var ap = "";
            if (hours > 11)
                ap = "PM";
            else
                ap = "AM";
            if (hours > 12)
                hours -= 12;
            else if (hours === 0)
                hours = 12;
            result += "" + hours + ":" + this.formatNumber(date.getMinutes(), "00") + ":" + this.formatNumber(date.getSeconds(), "00") + " " + ap;
            return result;
        }
        else {
            return "";
        }
    },

    formatShortDateInput: function (date) {
        if (date != null && date != "")
            return date.getFullYear() + "-" + (date.getMonth() + 1 < 10 ? "0" + (date.getMonth() + 1) : date.getMonth() + 1) + "-" + (date.getDate() < 10 ? "0" + date.getDate().toString() : date.getDate());
        else
            return "";
    },

    formatNumber: function (value, format) {
        var result = value.toString();
        var ch = format.substr(0, 1);
        while (result.length < format.length) result = ch + result;
        return result;
    },

    cleanNumber: function (value) {
        if (typeof value === "string")
            return Number(value.replace(/\%|\$|\,/g, ""));
        else
            return value;
    },

    convertUTCDateToLocalDate: function (date) {
        var result = new Date(date.getTime() + date.getTimezoneOffset() * 60 * 1000);
        var offset = date.getTimezoneOffset() / 60;
        var hours = date.getHours();
        result.setHours(hours - offset);
        return result;
    },

    formatBreadcrumbs: function (items) {
        var result = [];
        for (var index = 0; index < items.length; index += 2) {
            if (items[index] != null) {
                var breadcrumb = $("<li></li>");
                if (items[index + 1] != null) {
                    $("<a></a>").attr("href", items[index + 1]).text(items[index]).appendTo(breadcrumb);
                }
                else {
                    breadcrumb.text(items[index]);
                }
                if (index == items.length - 2) {
                    breadcrumb.addClass("active");
                }
                result.push(breadcrumb);
            }
        }
        return result;
    },

    createCookie: function (name, value, seconds) {
        var expires = "";
        if (typeof seconds !== "undefined") {
            var date = new Date();
            date.setTime(date.getTime() + (seconds * 1000));
            expires = "; expires=" + date.toGMTString();
        }
        document.cookie = name + "=" + value + expires + "; path=/";
    },

    getCookie: function (name) {
        var nameval = name + "=";
        var cookies = document.cookie.split(';');
        for (var index = 0; index < cookies.length; index++) {
            var cookie = cookies[index];
            while (cookie.charAt(0) == ' ') cookie = cookie.substring(1, cookie.length);
            if (cookie.indexOf(nameval) == 0) return cookie.substring(nameval.length, cookie.length);
        }
        return null;
    },

    deleteCookie: function (name) {
        this.createCookie(name, "", -1);
    },

    setTimezoneCookie: function (reloadPage) {
        var cookie = this.getCookie("timeZoneOffset");
        var timezoneOffset = (new Date()).getTimezoneOffset();
        if (cookie == null || cookie != timezoneOffset) {
            this.createCookie("timeZoneOffset", timezoneOffset);
            if (typeof reloadPage !== "undefined" && reloadPage) {
                document.location = document.location;
            }
        }
    },

    parseQueryString: function (queryString) {
        var result = {};
        var match;
        var pl = /\+/g;
        var search = /([^&=]+)=?([^&]*)/g;
        var decode = function (s) { return decodeURIComponent(s.replace(pl, " ")); };
        while (match = search.exec(queryString))
            result[decode(match[1]).toLowerCase()] = decode(match[2]);
        return result;
    },

    parseModelStateErrors: function (xhr, force) {
        var result = new Array();
        try {
            if (xhr != null && xhr.status == 400) {
                var data = JSON.parse(xhr.responseText);
                if (data != null) {
                    var modelState = data.ModelState;
                    if (modelState != null) {
                        if (modelState.constructor === Array) {
                            for (var index = 0; index < modelState.length; index++) {
                                result.push(modelState[index]);
                            }
                        }
                        else {
                            for (var key in modelState) {
                                if (modelState.hasOwnProperty(key)) {
                                    result.push(modelState[key]);
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (err) { }
        if (force == true && result.length == 0 && xhr != null && xhr.responseText != null && $.trim(xhr.responseText) != "") {
            result.push(xhr.responseText);
        }
        if (force == true && result.length == 0)
            result.push("An unexpected error occurred");
        return result;
    },

    round: function (value, exp) {
        if (typeof exp === 'undefined' || +exp === 0)
            return Math.round(value);

        value = +value;
        exp = +exp;

        if (isNaN(value) || !(typeof exp === 'number' && exp % 1 === 0))
            return NaN;

        // Shift
        value = value.toString().split('e');
        value = Math.round(+(value[0] + 'e' + (value[1] ? (+value[1] + exp) : exp)));

        // Shift back
        value = value.toString().split('e');
        return +(value[0] + 'e' + (value[1] ? (+value[1] - exp) : -exp));
    },

    getDataAttributes: function (item, namespace, lowerCaseFirstChar) {
        var result = {};
        var data = item.data();
        for (var key in data) {
            if (data.hasOwnProperty(key)) {
                var destKey = null;
                var destValue = null;
                if (namespace === null || namespace === "" || typeof namespace === "undefined") {
                    destKey = key;
                }
                else if (key.substring(0, namespace.length) === namespace) {
                    destKey = key.substring(namespace.length);
                }
                if (destKey != null) {
                    destValue = data[key];
                    if (destValue != null && typeof destValue === "string") {
                        if (destValue === "#cflashsofttruefunc") {
                            destValue = cflashsoftUtil.trueFunc;
                        }
                        else if (destValue === "#cflashsoftfalsefunc") {
                            destValue = cflashsoftUtil.falseFunc;
                        }
                        else if (destValue.substring(0, 6) == "#JSON{") {
                            destValue = JSON.parse(destValue.substring(5));
                        }
                        else if (destValue.substring(0, 6) == "#FUNC{") {
                            var functionName = destValue.substring(6, destValue.length - 1);
                            destValue = this.clientFunctions[functionName];
                        }
                    }
                    if (lowerCaseFirstChar) {
                        result[destKey.substring(0, 1).toLowerCase() + destKey.substring(1)] = destValue;
                    }
                    else {
                        result[destKey] = destValue;
                    }
                }
            }
        }
        return result;
    },

    trueFunc: function () {
        return true;
    },

    falseFunc: function () {
        return false;
    },

    alert: function (msg, ok, icon) {
        this.alertBoxOkCallback = null;
        this.alertBoxCancelCallback = null;
        if (typeof ok !== "undefined") {
            this.alertBoxOkCallback = ok;
            this.alertBoxCancelCallback = ok;
        }
        this.updateAlertIcon(icon);
        $("#cflashsoftAlertBoxMessage").html(msg);
        $("#cflashsoftAlertCancel").hide();
        $("#cflashsoftAlertBox").modal("show");
    },

    confirm: function (msg, ok, cancel, icon) {
        this.alertBoxOkCallback = null;
        this.alertBoxCancelCallback = null;
        if (typeof ok !== "undefined") {
            this.alertBoxOkCallback = ok;
        }
        if (typeof cancel !== "undefined") {
            this.alertBoxCancelCallback = cancel;
        }
        this.updateAlertIcon(icon);
        $("#cflashsoftAlertBoxMessage").html(msg);
        $("#cflashsoftAlertCancel").show();
        $("#cflashsoftAlertBox").modal("show");
    },

    updateAlertIcon: function (icon) {
        $("#cflashsoftAlertBoxOkIcon").hide();
        $("#cflashsoftAlertBoxHandIcon").hide();
        $("#cflashsoftAlertBoxInfoIcon").hide();
        $("#cflashsoftAlertBoxQuestIcon").hide();
        $("#cflashsoftAlertBoxExclIcon").hide();
        $("#cflashsoftAlertBoxErrIcon").hide();
        if (icon != null) {
            switch (icon.toLowerCase()) {
                case "ok":
                case "success":
                    $("#cflashsoftAlertBoxOkIcon").show();
                    break;
                case "hand":
                    $("#cflashsoftAlertBoxHandIcon").show();
                    break;
                case "info":
                case "information":
                    $("#cflashsoftAlertBoxInfoIcon").show();
                    break;
                case "question":
                    $("#cflashsoftAlertBoxQuestIcon").show();
                    break;
                case "exclamation":
                case "warning":
                    $("#cflashsoftAlertBoxExclIcon").show();
                    break;
                case "error":
                case "danger":
                    $("#cflashsoftAlertBoxErrIcon").show();
                    break;
            }
        }
    },

    hideAlert: function () {
        $("#cflashsoftAlertBox").modal("hide");
    },

    alertOk: function () {
        if (this.alertBoxOkCallback != null) {
            if (this.alertBoxOkCallback()) {
                this.hideAlert();
            }
        }
        else {
            this.hideAlert();
        }
    },

    alertCancel: function () {
        if (this.alertBoxCancelCallback != null) {
            if (this.alertBoxCancelCallback()) {
                this.hideAlert();
            }
        }
        else {
            this.hideAlert();
        }
    },

    autoLogoutEnabled: false,
    autoLogoutTimer: null,
    autoLogoutStartTime: null,
    autoLogoutMinutes: 5,
    autoLogoutWarnMinutes: 2,
    autoLogoutRedirUrl: 'logout',
    autoLogoutPingUrl: 'ping',

    initAutoLogout: function () {
        this.autoLogoutEnabled = true;
        $("<div id=\"cflashsoftAutoLogoutBox\" class=\"modal fade\" style=\"display:none;\"><div class=\"modal-dialog\"><div class=\"modal-content\"><div class=\"modal-body\"><img src=\"" + cflashsoftUtil.rootPath + "images/cflashsoft/timeout2.gif\" /> <span id=\"cflashsoftAutoLogoutMessage\"></span></div><div class=\"modal-footer\"><button type=\"button\" class=\"btn btn-success\" onclick=\"cflashsoftUtil.resetAutoLogout(true);\"><span class=\"glyphicon glyphicon-ok\" aria-hidden=\"true\"></span> Continue</button></div></div></div></div>").appendTo($("body"));
        this.resetAutoLogout(false);
    },

    resetAutoLogout: function (sendPing) {
        if (this.autoLogoutEnabled) {
            if (this.autoLogoutTimer != null) {
                window.clearTimeout(this.autoLogoutTimer);
                this.autoLogoutTimer = null;
                this.autoLogoutStartTime = null;
            }
            if (sendPing)
                $.ajax({ url: this.rootPath + this.autoLogoutPingUrl, cache: false, type: "GET" });
            this.autoLogoutStartTime = new Date();
            this.autoLogoutTimer = window.setInterval(this.onAutoLogoutInterval, 20000);
            if ($("#cflashsoftAutoLogoutBox").is(":visible"))
                $("#cflashsoftAutoLogoutBox").modal("hide");
        }
    },

    onAutoLogoutInterval: function () {
        var timeDiffMinutes = (((new Date()) - cflashsoftUtil.autoLogoutStartTime) / 60000) | 0;
        if (cflashsoftUtil.autoLogoutWarnMinutes > 0) {
            if (timeDiffMinutes >= cflashsoftUtil.autoLogoutMinutes - cflashsoftUtil.autoLogoutWarnMinutes) {
                var timeLeftMinutes = cflashsoftUtil.autoLogoutMinutes - timeDiffMinutes;
                $("#cflashsoftAutoLogoutMessage").html("You will be logged out in " + timeLeftMinutes + " " + (timeLeftMinutes == 1 ? "minute" : "minutes") + " unless you click 'Continue'");
                if (!$("#cflashsoftAutoLogoutBox").is(":visible"))
                    $("#cflashsoftAutoLogoutBox").modal("show");
            }
        }
        if (timeDiffMinutes >= cflashsoftUtil.autoLogoutMinutes)
            window.location = cflashsoftUtil.rootPath + cflashsoftUtil.autoLogoutRedirUrl;
    },

    onWindowResizeForPopup: function (event) {
        cflashsoftUtil.centerPopup(cflashsoftUtil.currentPopup);
    }

};
