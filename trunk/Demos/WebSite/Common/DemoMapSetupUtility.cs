﻿/*
 *  The attached / following is part of SharpMap.Presentation.AspNet
 *  SharpMap.Presentation.AspNet is free software © 2008 Newgrove Consultants Limited, 
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
using System.Configuration;
using System.Web;
using SharpMap.Data.Providers;
using SharpMap.Data.Providers.Db.Expressions;
using SharpMap.Data.Providers.MsSqlServer2008.Expressions;
using SharpMap.Data.Providers.ShapeFile;
using SharpMap.Expressions;
using SharpMap.Layers;
using SharpMap.Rendering.Rendering2D;
using SharpMap.Styles;
using SharpMap.Utilities;
using SharpMap.Utilities.SridUtility;

namespace SharpMap.Presentation.AspNet.Demo.Common
{
    public static class DemoMapSetupUtility
    {
        /// <summary>
        /// little util wich just adds one vector layer to the map and assigns it a random theme.
        /// </summary>
        public static void SetupMap(HttpContext context, Map m)
        {
            setupMsSqlSpatial(m);
            //setupShapefile(context, m);
        }

        private static void setupShapefile(HttpContext context, Map m)
        {
            GeometryServices geometryServices = new GeometryServices();
            ShapeFileProvider shapeFile = new ShapeFileProvider(context.Server.MapPath("~/App_Data/Shapefiles/BCRoads.shp"),
                                                                geometryServices.DefaultGeometryFactory,
                                                                geometryServices.CoordinateSystemFactory, false);
            shapeFile.IsSpatiallyIndexed = false;

            AppStateMonitoringFeatureProvider provider = new AppStateMonitoringFeatureProvider(shapeFile);

            GeoJsonGeometryStyle style = RandomStyle.RandomGeometryStyle();
            /* include GeoJson styles */
            style.IncludeAttributes = false;
            style.IncludeBBox = true;
            style.PreProcessGeometries = false;
            style.CoordinateNumberFormatString = "{0:F}";

            GeometryLayer geometryLayer = new GeometryLayer("BCRoads", style, provider);
            geometryLayer.Features.IsSpatiallyIndexed = false;
            m.AddLayer(geometryLayer);
            provider.Open();
        }

        private static void setupMsSqlSpatial(Map m)
        {
            var layernames = new[]
                                 {
                                     "Countries",
                                     "Rivers",
                                     "Cities"
                                 };

            string sridstr = SridMap.DefaultInstance.Process(4326, "");

            foreach (string lyrname in layernames)
            {
                string tbl = lyrname;


                var provider = new AppStateMonitoringFeatureProvider(
                    new MsSqlSpatialProvider(
                        new GeometryServices()[sridstr],
                        ConfigurationManager.ConnectionStrings["db"].ConnectionString,
                        "st",
                        "dbo",
                        tbl,
                        "oid",
                        "Geometry")
                        {
                            DefaultProviderProperties
                                = new ProviderPropertiesExpression(
                                new ProviderPropertyExpression[]
                                    {
                                        new WithNoLockExpression(true)
                                    })
                        });

                
                var style = new GeoJsonGeometryStyle();

                switch (tbl)
                {
                    case "Rivers":
                        {
                            StyleBrush brush = new SolidStyleBrush(new StyleColor(255, 255, 0, 255));
                            var pen = new StylePen(brush, 1);
                            style.Enabled = true;
                            style.EnableOutline = true;
                            style.Line = pen;
                            style.Fill = brush;
                            break;
                        }

                    case "Countries":
                        {
                            StyleBrush brush = new SolidStyleBrush(new StyleColor(0, 0, 0, 255));
                            var pen = new StylePen(brush, 2);
                            StyleBrush trans = new SolidStyleBrush(new StyleColor(255, 255, 255, 0));
                            style.Enabled = true;
                            style.EnableOutline = true;
                            style.Line = pen;
                            style.Fill = trans;

                            break;
                        }

                    default:
                        {
                            style = RandomStyle.RandomGeometryStyle();
                            style.MaxVisible = 100000;
                            break;
                        }
                }


                /* include GeoJson styles */
                style.IncludeAttributes = false;
                style.IncludeBBox = true;
                style.PreProcessGeometries = false;
                style.CoordinateNumberFormatString = "{0:F}";
                var layer = new GeometryLayer(tbl, style, provider);
                layer.Features.IsSpatiallyIndexed = false;
                layer.AddProperty(AppStateMonitoringFeatureLayerProperties.AppStateMonitor, provider.Monitor);
                m.AddLayer(layer);
            }
        }
    }
}