SessionUpdater = (function () {
    var clientMovedSinceLastTimeout = false;
    var keepSessionAliveUrl = null;
    //var timeout = 5 * 1000 * 60; // 5 minutes
    var timeout = 15000; // 15 seconds for testing

    function setupSessionUpdater(actionUrl) {
        // store local value
        keepSessionAliveUrl = actionUrl;
        // setup handlers
        listenForChanges();
        // start timeout - it'll run after n minutes
        checkToKeepSessionAlive();
    }

    function listenForChanges() {
        $("body").one("mousemove keydown", function () {
            clientMovedSinceLastTimeout = true;
        });
    }


    // fires every n minutes - if there's been movement ping server and restart timer
    function checkToKeepSessionAlive() {
        setTimeout(function () { keepSessionAlive(); }, timeout);
    }

    function keepSessionAlive() {
        // if we've had any movement since last run, ping the server
        if (clientMovedSinceLastTimeout && keepSessionAliveUrl != null) {
            $.ajax({
                type: "POST",
                url: keepSessionAliveUrl,
                success: function (data) {
                    // reset movement flag
                    clientMovedSinceLastTimeout = false;
                    // start listening for changes again
                    listenForChanges();
                    // restart timeout to check again in n minutes
                    checkToKeepSessionAlive();


                },
                error: function (data) {
                    console.log("Error posting to " & keepSessionAliveUrl);
                }
            });
        }
    }

    // export setup method
    return {
        Setup: setupSessionUpdater
    };

})();