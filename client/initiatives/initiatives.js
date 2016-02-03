var Fs = require("fs");
var Path = require("path");
var Config = require("config");
//var Hoek = require("hoek");
//var Joi = require("joi");
var JSON5 = require("json5");
//var Nunjucks = require("hapi-nunjucks");
var Nunjucks = require("/home/pvieira/github/hapi-nunjucks/index.js");
var Pre = require("../../server/common/prerequisites");
var Boom = require("boom");
var _ = require("underscore");

var internals = {};


/**
internals.definitions = JSON5.parse(Fs.readFileSync(Path.join(Config.get("rootDir"), "database/90_initial_data/9040_populate_definitions.json"), "utf8")); 

internals.getDefinitionsArray = function(definitionPrefix){

    return _.chain(internals.definitions)
            .filter(function(obj){
                return obj.id.indexOf(definitionPrefix) === 0;
            })
            .pluck("id")
            .value();
};

internals.validation = {};
internals.validation.typesIds = internals.getDefinitionsArray("type");
internals.validation.domainsIds = internals.getDefinitionsArray("domain");
internals.validation.baseLayersIds = ["stamen", "esri_satellite", "mapquest"];
*/


exports.register = function(server, options, next){

    var pluginName = exports.register.attributes.name;

    // configure nunjucks
    //var env = Nunjucks.configure(Path.join(__dirname, "templates"), { 
    var env = Nunjucks.configure(Config.get("rootDir"), { 
        autoescape: false,
        watch: false,
        noCache: process.env.NODE_ENV === "production" ? true : false,
        pluginName: pluginName,
        // throwOnUndefined: false,
    });

    internals.addNunjucksFilters(env);
    internals.addNunjucksGlobals(env);

    // expose the Environment object to the outside
    server.expose("env", env);

    // configure a view's manager using the nunjucks lib
    server.views({
        path: Config.get("rootDir"),
        allowAbsolutePaths: true,
        engines: {
            html: Nunjucks
            // html: {
            //     compile: Nunjucks
            // }
        },
        compileOptions: {
            pluginName: pluginName
        }
    });


    server.route({
        path: "/iniciativas",
        method: "GET",
        config: {
            handler: function(request, reply) {

                
                console.log(request.query);

                //console.log(request.pre.initiatives);
                var context = {
                    urlParam1: "initiatives",
                    initiatives: request.pre.initiatives,
                    definitions: request.pre.definitions,
                    //query: request.query
                };

                return reply.view(Path.join(__dirname, "templates/initiatives.html"), {ctx: context});
            },

            pre: [
                [Pre.readInitiativesSlim, Pre.readDefinitions2]
            ],
        },
    });

    server.route({
        //path: "/rc-app/{anyPath*}",
        path: "/initiatives-app/{anyPath*}",
        method: "GET",
        config: {
            handler: {
                directory: { 
                    path: Path.join(Config.get("rootDir"), "client/initiatives/app"),
                    index: false,
                    listing: false,
                    showHidden: false
                }
            },
            cache: {
                privacy: "public",
                expiresIn: 3600000
            },
            cors: {
                methods: ["GET"]
            },
            auth: false,
        }
    });

    return next();
};

internals.addNunjucksFilters = function(env){

     env.addFilter('stringify', function(obj) {

         return JSON.stringify(obj);
     });

     env.addFilter('getDomainLogo', function(array, elem) {

         if(typeof array !== "object"){
             return "";
         }

         for(var i=0; i<array.length; i++){
             if(array[i] === elem){
                 return "fa-check-square-o";
             }
         }

         return "fa-square-o";
     });

     env.addFilter('getDefinitionClass', function(array, elem) {

         if(typeof array !== "object"){
             return "";
         }

         for(var i=0; i<array.length; i++){
             if(array[i] === elem){
                 return "has-definition";
             }
         }

         return "";
     });

     env.addFilter('parseNewLines', function(text) {

         text = text.replace("<br /><br /><br /><br />", "<br />");
         text = text.replace("<br /><br /><br />", "<br />");
         text = text.replace("<br /><br />", "<br />");

         return text.replace("<br />", "<br /><br />");
    });

    env.addFilter('toFixed', function(num, precision) {

        if(typeof num === "string"){
            num = Number(num);
        }

        return num.toFixed(precision);
    });
};

internals.addNunjucksGlobals = function(env){

    var bundles  = JSON.parse(Fs.readFileSync(Path.join(Config.get("rootDir"), "bundles.json"), "utf8"));

    env.addGlobal("NODE_ENV", process.env.NODE_ENV);
    env.addGlobal("bundles", bundles);
    env.addGlobal("pluginTemplatesPath", Path.join(__dirname, "templates"));
    env.addGlobal("commonTemplatesPath", Path.join(Config.get("rootDir"), "templates"));
};

exports.register.attributes = {
    name: Path.parse(__dirname).name,  // use the name of the file
    dependencies: ["vision", "inert"]
};