(function ($) {
    $.widget("ui.endlessScroll", {
        options: {
            pixelBuffer: 100,
            pagesToPause: 5,
            scrollContainer: '#view-inner-content',
            showMoreAttribute: 'data-kind="show-more"'
        },
    
        _create: function () {

            var self = this;

            self.loading = false;
            self.page = 0;
            self.stop = self.options.stop;
            self.pause = false;
            self.lastScrollPos = null;
            self.position = null;
            self.appearedOnce = false;

            if (typeof self.options.container === 'undefined') {
                self.options.container = document;
            }

            if (self.options.pagesToPause == 0 && !self.stop) {
                self.stop = true;
                self._appendShowMore();
            }
            else {
                self.checkAppend();

                $(self.options.scrollContainer).scroll(function () {

                    if (typeof self.disposed !== 'undefined')
                        return;

                    self.checkAppend();
                })
            }

            $(self.options.container).on('click', '[' + self.options.showMoreAttribute + ']', function () {
                if (typeof self.disposed !== 'undefined')
                    return;

                self.append();
                $(this).remove();
                self.pause = false;

                return false;
            });
        },
        _destroy: function () {
            var self = this;

            self.disposed = true;
        },
        _appendShowMore: function (eof) {

            if (typeof eof !== 'undefined')
                return;

            var self = this;

            $(self.options.appendTo).append('<div ' + self.options.showMoreAttribute + '><a class="small btn btn-link">' + self.options.moreText + '</a></div>');

            self.pause = true;
        },
        checkAppend: function () {
            var self = this;

            if (typeof self.disposed !== 'undefined')
                return;

            var element = $(self.options.container);
            var height = Math.abs($(self.options.container).height() - $(self.options.scrollContainer).height());
            var scrollTop = $(self.options.scrollContainer).scrollTop();

            if (scrollTop == 0) {
                var scrollTop = $($(self.options.container)).scrollTop();
            }

            if (scrollTop >= height - self.options.pixelBuffer) {

                if (!self.loading) {

                    if (self.stop)
                        return;

                    if (!self.pause) {

                        self.pause = true;
                        self.append();
                    }
                }
            }
        },

        append: function () {

            var self = this;

            self.loading = true;
            self.page++;

            if (self.options.appending) self.options.appending.apply(self);

            amt.ajaxPost({
                url: self.options.url,
                data: self.options.params,
                container: $(self.options.container),
                success: function (html, status, request) {
                    var eof = request.getResponseHeader('tompit-eof');

                    if (eof != undefined && eof == 'true') {
                        self.stop = true;
                    }
                    else {
                        self.pause = false;
                    }

                    var data = $(html);

                    data.appendTo($(self.options.appendTo)).hide().fadeIn(500);

                    var mod = self.page % self.options.pagesToPause;

                    if (self.options.pagesToPause == 0 ||( mod == 0 && !self.stop)) {
                        self._appendShowMore(eof);
                    }

                    if (self.options.callback) self.options.callback.call(self, data, request);

                    self.loading = false;
                }
            });
        },

        reset: function () {
            var self = this;

            self.page = 0;
            self.stop = false;
            self.pause = false;
            self.loading = false;
        }
    });
})(jQuery);
