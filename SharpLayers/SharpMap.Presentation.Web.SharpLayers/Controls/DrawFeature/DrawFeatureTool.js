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
Type.registerNamespace('SharpMap.Presentation.Web.SharpLayers.Controls.DrawFeature');

SharpMap.Presentation.Web.SharpLayers.Controls.DrawFeature.DrawFeatureTool = function(element) {
    SharpMap.Presentation.Web.SharpLayers.Controls.DrawFeature.DrawFeatureTool.initializeBase(this);

}
SharpMap.Presentation.Web.SharpLayers.Controls.DrawFeature.DrawFeatureTool.prototype = {
    initialize: function() {
        SharpMap.Presentation.Web.SharpLayers.Controls.DrawFeature.DrawFeatureTool.callBaseMethod(this, 'initialize');

    },

    dispose: function() {
        SharpMap.Presentation.Web.SharpLayers.Controls.DrawFeature.DrawFeatureTool.callBaseMethod(this, 'dispose');
    },
    _toolBuilderDelegate: function() {
        var options = this.get_builderParams();
        options.div = typeof options.div == "string" ? $get(options.div) : options.div;
        return new OpenLayers.Control.DrawFeature(options);
    }
}
SharpMap.Presentation.Web.SharpLayers.Controls.DrawFeature.DrawFeatureTool.registerClass('SharpMap.Presentation.Web.SharpLayers.Controls.DrawFeature.DrawFeatureTool', SharpMap.Presentation.Web.SharpLayers.Controls.ToolBaseComponent);
