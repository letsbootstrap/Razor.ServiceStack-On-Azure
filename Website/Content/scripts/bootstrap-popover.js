/* ===========================================================
 * bootstrap-popover.js v2.0.0
 * http://twitter.github.com/bootstrap/javascript.html#popovers
 * ===========================================================
 * Copyright 2012 Twitter, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * =========================================================== */


!function ($) {

    "use strict"

    var Popover = function (element, options) {
        this.init('popover', element, options)
    }

    /* NOTE: POPOVER EXTENDS BOOTSTRAP-TOOLTIP.js
       ========================================== */

    Popover.prototype = $.extend({}, $.fn.tooltip.Constructor.prototype, {

        constructor: Popover

    , setContent: function () {
        var $tip = this.tip()
          , title = this.getTitle()
          , content = this.getContent()

        $tip.find('.popover-title')[$.type(title) == 'object' ? 'append' : 'html'](title)
        $tip.find('.popover-content > *')[$.type(content) == 'object' ? 'append' : 'html'](content)

        if (this.$element.attr('data-noPadding') != undefined) {
            $tip.find('.popover-content').css('padding', '4px')
            $tip.find('.popover-content p').css('line-height', '4px')
        }
        if (this.$element.attr('data-popoverId') != undefined) {
            $tip.attr('id', this.$element.attr('data-popoverId'));
        }
        if (this.$element.attr('data-width') != undefined) {
            $tip.find('.popover-inner').css('width', this.$element.attr('data-width') + 'px')
        }
        if (this.$element.attr('data-noHeader') != undefined) {
            $tip.find('.popover-title').hide();
        }

        $tip.removeClass('fade top bottom left right in')
    }

    , hasContent: function () {
        return this.getTitle() || this.getContent()
    }

    , getContent: function () {
        var content
          , $e = this.$element
          , o = this.options

        content = $e.attr('data-content')
          || (typeof o.content == 'function' ? o.content.call($e[0]) : o.content)

        if (content.indexOf('#') == 0) {
            if ($e.attr('data-newId') != undefined)
                content = '<span id="' + $e.attr('data-newId') + '">' + $(content).html() + '</span>';
            else
                content = $(content).html();
        }

        content = content.toString().replace(/(^\s*|\s*$)/, "")

        return content
    }

    , tip: function () {
        if (!this.$tip) {
            this.$tip = $(this.options.template)
        }
        return this.$tip
    }
    , destroy: function () {
        this.$tip.remove();
    }
    })


    /* POPOVER PLUGIN DEFINITION
     * ======================= */

    $.fn.popover = function (option) {
        return this.each(function () {
            var $this = $(this)
              , data = $this.data('popover')
              , options = typeof option == 'object' && option
            if (!data) $this.data('popover', (data = new Popover(this, options)))
            if (typeof option == 'string') data[option]()
        })
    }

    $.fn.popover.Constructor = Popover

    $.fn.popover.defaults = $.extend({}, $.fn.tooltip.defaults, {
        placement: 'right'
    , content: ''
    , template: '<div class="popover"><div class="arrow"></div><div class="popover-inner"><h3 class="popover-title"></h3><div class="popover-content"><p></p></div></div></div>'
    })

}(window.jQuery)