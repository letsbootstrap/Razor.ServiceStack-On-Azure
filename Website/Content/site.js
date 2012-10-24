$(function () {
    $('*[rel*="tooltip"]').tooltip({
        live: true
    });
    $("a[rel=popover]")
      .popover()
      .click(function (e) {
          e.preventDefault()
      });

    $.getJSON("/account/status", function (data) {
        if (data.IsAuthenticated) {
            $('#signin-link a').attr('data-original-title', 'Hi ' + data.DisplayName);
            $('#signin-link a').attr('href', '#');
            $('#signin-link a').addClass('signout');
            $('#signin-link a').text('Sign Out');
        } else {
            $('#signin-link a').attr('data-original-title', 'Sign In');
            $('#signin-link a').text('Sign In');
        }
    });

    $('#signin-link a.signout').live('click', function (e) {
        $.cookie('auth-session', '', { path: '/' });
        window.location = '/auth/logout';
    });
});

//////////////////////////////////////////////////////////

function uploadImageControl(element, url, size) {
    $(document).ready(function () {
        var uploader = new qq.FileUploader({
            element: $('#' + element + '-upload')[0],
            action: url,
            multiple: false, allowedExtensions: ['jpeg', 'jpg', 'png', 'gif'], sizeLimit: size,
            uploadButtonText: "Click, or drop image here...",
            onUpload: function (id, fileName) {
                showAlert('Uploading image...');
            },
            onProgress: function (id, fileName, uploadedBytes, totalBytes) { },
            onComplete: function (id, fileName, responseJSON) {
                $('#' + element).attr('src', responseJSON.Url);
            }
        });
    });
}

function showAlert(msg, type) {
    // type: info, error, success
    if (type == undefined)
        type = 'info';
    var jacked = humane.create({ baseCls: 'humane-jackedup', addnCls: 'humane-jackedup-' + type, timeout: 2000, waitForMove: true })
    jacked.log(msg)
}

function gup(name) {
    name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    var regexS = "[\\?&]" + name + "=([^&#]*)";
    var regex = new RegExp(regexS);
    var results = regex.exec(window.location.href);
    if (results == null)
        return "";
    else
        return results[1];
}

function getRandomString(length, nodupes) {
    // "typecast" the length to an integer
    length = parseInt(length, 10);

    // make sure the output is at least one character long
    if (length < 1) {
        length = 1;
    }

    // list of characters to use
    var chars = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';

    // final output string
    var output = '';

    // list of used characters (only used if nodupes === true)
    var used = '';

    // make sure we don't have an infinite loop
    if ((nodupes === true) && (length > chars.length)) {
        length = chars.length;
    }

    // loop as many times as `length` defines
    do {
        // get a random number between 0 and length of `chars`
        var randnum = Math.floor(Math.random() * chars.length)

        // extract the random character
        var chr = chars.charAt(randnum);

        // check if we are not allowed to include duplicate characters
        if (nodupes === true) {
            // true if character already used

            var added = (used.indexOf(chr) !== -1);

            // skip the character if already used
            if (added === true) {
                continue;
            }

            // add character to the list of used ones
            used += chr;
        }

        // add character to the output string
        output += chr;
    } while (output.length < length);

    return output;
}