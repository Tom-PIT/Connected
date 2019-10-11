'use strict';

$.widget('tompit.tpDataHub', {
    options: {
        connections: []
    },
    create: function () {

    },
    connect: function (e) {
        var exists = false;
        var instance = this;

        this._ensureId(e);

        $.each(this.options.connections, function (i, v) {
            if (v.id === e.id) {
                exists = true;
                return false;
            }
        });

        if (exists)
            return;

        var qs = encodeURI('?microService=' + e.microService + '&hub=' + e.hub);

        var connection = {
            'url': e.url,
            'id': e.id,
            'microService': e.microService,
            'appBaseUrl': e.appBaseUrl,
            'hub': e.hub,
            'endpoints': e.endpoints,
            'connection': new signalR.HubConnectionBuilder()
                .withUrl(e.url + qs)
                .configureLogging(signalR.LogLevel.Information)
                .withAutomaticReconnect({
                    nextRetryDelayInMilliseconds: retryContext => {
                        if (retryContext.elapsedMilliseconds < 60000)
                            return 2500;
                        else
                            return 10000;
                    }
                })
                .build()
        };

        connection.connection.on('data', function (endpoint, policy, data) {
            console.log('ondata');
            var target = instance._findTarget(connection.id, endpoint, policy);

            if (target !== null)
                target.onData(data);
        });

        this.options.connections.push(connection);
        this._startConnection(connection);
    },
    _findTarget: function (connectionId, endpoint, policy) {
        var result = null;

        $.each(this.options.connections, function (i, v) {
            if (v.id === connectionId) {
                $.each(v.endpoints, function (i1, ep) {
                    if (ep.name.toLowerCase() === endpoint.toLowerCase()) {
                        $.each(ep.policies, function (i2, pc) {
                            if (pc.name.toLowerCase() === policy.toLowerCase()) {
                                result = pc;
                                return false;
                            }
                        });
                    }

                    if (result !== null)
                        return false;
                });
            }

            if (result !== null)
                return false;
        });

        return result;
    },
    _ensureId: function (e) {
        if (typeof e.id !== 'undefined')
            return;

        var index = 1;

        while (this._idExists(index)) {
            index++;
        }

        e.id = index;
    },
    _idExists: function (id) {
        var result = false;

        $.each(this.options.connection, function (i, v) {
            if (v.id == id) {
                result = true;
                return false;
            }
        });

        return result;
    },
    _getConnection: function (id) {
        var r = null;

        $.each(this.options.connections, function (i, v) {
            if (v.id === id) {
                r = v;
                return false;
            }
        });

        return r;
    },
    _startConnection(c) {
        var instance = this;

        c.connection.onreconnected(function () {
            instance._configureConnection(c);
        });

        c.connection.start().then(function () {
            instance._configureConnection(c);
        }).catch(function () {
            instance._startConnection(c);
        });
    },
    _configureConnection(c) {
        c.connection.invoke('configure', c.endpoints);
    }
});

(function (tompit, $, undefined) {
    'use strict';

    tompit.createDataHub = function (selector) {
        return $(selector).tpDataHub().data('tompit-tpDataHub');
    };
})(window.tompit = window.tompit || {}, jQuery);