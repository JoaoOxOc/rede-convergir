require("bootstrap.css");
require("bootflat.css");
require("fs.background.css");
require("fs.background.css");
require("fs.checkbox.css");
require("fs.dropdown.css");
require("fs.light-theme.css");
require("font-awesome.css");

var $ = require("jquery");
require("fs.background");
require("fs.checkbox");
require("fs.dropdown");

// bootstrap javascript has to be imported using the imports loader
// https://github.com/webpack/imports-loader
require("imports?jQuery=jquery!bootstrap"); 

/*
$("select").dropdown();
$("input[type=checkbox], input[type=radio]").checkbox();

$("#bg").background({
    source: "/public/images/9f8335c934d027c1e31c4f25ba6b511a_1360x765.jpg"
});
*/


var _ = require("underscore");

// initial configuration

// the following require calls are necessary to call the code that makes the plugin 
// attach to the respective objects (example: Marionette.Service)

// backbone.marionette will require: backbone, underscore
// backbone will require: underscore, jquery
var Backbone = require("backbone");
var Mn = require("backbone.marionette");
var Radio = require("backbone.radio");
var BaseRouter = require("backbone.base-router");

// marionette.state will require: underscore, backbone, backbone.marionette
var State = require("marionette.state");
Mn.State = State;


// override the default renderer (this works because Marionette.renderer has been
// changed according to pr #2911 (add a custom renderer
Mn.View.prototype.renderer = function(template, data) {
    //debugger;
    if (!template) {
        throw new Mn.Error({
            name: 'TemplateNotFoundError',
            message: 'Cannot render the template since its false, null or undefined.'
        });
    }

    var output = "";
    try {
        // nunjucks will look for the pre-compiled template at window.nunjucksPrecompiled;
        // more details here: https://mozilla.github.io/nunjucks/api.html#browser-usage
        // however here we are using webpack's "nunjucks-loader"
        output = template.render(data);

        return output;
    } catch (err) {
        throw new Mn.Error({
            name: 'NunjucksError',
            message: err.message
        });
    }
};


Mn.Router = BaseRouter.extend({
    onNavigate: function(routeData) {
        //debugger;
        var routeObj = routeData.linked;
        var keys = ['query', 'params', 'uriFragment', 'originalRoute'];
        routeObj.onNavigate(_.pick(routeData, keys));
    }
});

var modalRegion = new Mn.Region({ el: $("div.mn-r-modal-contents") });
Radio.channel("commonRegions").reply("modal", function(){
    return modalRegion;
});


require("./plugin");

if (NODE_ENV === "dev") {
    window.$ = $;
    // window._ = _;
    // window.Mn = Mn;
    window.Radio = Radio;
    Radio.DEBUG = true;
}