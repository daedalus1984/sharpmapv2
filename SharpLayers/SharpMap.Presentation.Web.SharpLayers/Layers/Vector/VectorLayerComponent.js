/*
*  The attached / following is part of SharpMap.Presentation.Web.SharpLayers
*  SharpMap.Presentation.Web.SharpLayers is free software � 2008 Newgrove Consultants Limited, 
*  www.newgrove.com; you can redistribute it and/or modify it under the terms 
*  of the current GNU Lesser General Public License (LGPL) as published by and 
*  available from the Free Software Foundation, Inc., 
*  59 Temple Place, Suite 330, Boston, MA 02111-1307 USA: http://fsf.org/    
*  This program is distributed without any warranty; 
*  without even the implied warranty of merchantability or fitness for purpose.  
*  See the GNU Lesser General Public License for the full details. 
*  
*  Author: John Diss 2008
* 
*/
Type.registerNamespace('SharpMap.Presentation.Web.SharpLayers.Layers.Vector');

SharpMap.Presentation.Web.SharpLayers.Layers.Vector.VectorLayerComponent = function() {
    SharpMap.Presentation.Web.SharpLayers.Layers.Vector.VectorLayerComponent.initializeBase(this);


}
SharpMap.Presentation.Web.SharpLayers.Layers.Vector.VectorLayerComponent.prototype = {
    initialize: function() {
        this._builderDelegate = Function.createDelegate(this, this._layerBuilderDelegate);
        this._loadDelegate = Function.createDelegate(this, this._sldLoaded);
        SharpMap.Presentation.Web.SharpLayers.Layers.Vector.VectorLayerComponent.callBaseMethod(this, 'initialize');
    },

    dispose: function() {
        SharpMap.Presentation.Web.SharpLayers.Layers.Vector.VectorLayerComponent.callBaseMethod(this, 'dispose');
    },
    _layerBuilderDelegate: function() {
        var options = this.get_builderParams();
        if (options["strategies"] != null) {
            var strats = [];
            for (var ndx in options.strategies) {
                strats.push($olFactory.buildOpenLayersObject(options.strategies[ndx]));
            }
            options.strategies = strats;
        }
        if (options["protocol"] != null)
            if (typeof options.protocol == "string")
            options.protocol = $find(options.protocol).get_hostedItem();

        if (options["sld"] != null && options["styleSelector"] != null) {
            if (typeof options.sld == "string")
                options.sld = $find(options.sld);
            if (typeof options.styleSelector == "string") {
                options.styleSelector = eval(options.styleSelector);
            }

            if (options.sld.get_initDone()) {
                options.styleMap = new OpenLayers.StyleMap(options.styleSelector(options.sld.get_hostedItem()));
            }
            else {
                options.sld.add_sldLoaded(this._loadDelegate);
            }
        }


        return new OpenLayers.Layer.Vector(this.get_name(), options);
    },
    _sldLoaded: function(e) {
        var opts = this.get_builderParams();
        opts.sld.remove_sldLoaded(this._loadDelegate);
        if (opts.styleSelector)
            this.get_hostedItem().styleMap = new OpenLayers.StyleMap(opts.styleSelector(opts.sld.get_hostedItem()));
    }



}
SharpMap.Presentation.Web.SharpLayers.Layers.Vector.VectorLayerComponent.registerClass('SharpMap.Presentation.Web.SharpLayers.Layers.Vector.VectorLayerComponent', SharpMap.Presentation.Web.SharpLayers.Layers.LayerComponent);
