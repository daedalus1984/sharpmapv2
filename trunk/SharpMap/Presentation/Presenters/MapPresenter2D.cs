// Copyright 2006, 2007 - Rory Plaire (codekaizen@gmail.com)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Threading;
using SharpMap.Data;
using SharpMap.Geometries;
using SharpMap.Layers;
using SharpMap.Presentation.Views;
using SharpMap.Rendering;
using SharpMap.Rendering.Rendering2D;
using SharpMap.Styles;
using SharpMap.Tools;
using GeoPoint = SharpMap.Geometries.Point;
using IMatrixD = NPack.Interfaces.IMatrix<NPack.DoubleComponent>;
using IVectorD = NPack.Interfaces.IVector<NPack.DoubleComponent>;
using IRasterRenderer2D = SharpMap.Rendering.IRasterRenderer<SharpMap.Rendering.Rendering2D.Rectangle2D>;

namespace SharpMap.Presentation.Presenters
{
    /// <summary>
    /// Provides the input-handling and view-updating logic for a 2D map view.
    /// </summary>
    public abstract class MapPresenter2D : BasePresenter<IMapView2D>
    {
        //#region Nested types
        //private struct RenderedObjectCacheKey : IEquatable<RenderedObjectCacheKey>
        //{
        //    private static readonly object _nullKey = new object();

        //    internal string LayerName;
        //    internal object Oid;

        //    public override string ToString()
        //    {
        //        return String.Format("[RenderedObjectCacheKey] LayerName: {0}; Oid: {1}",
        //            LayerName, Oid);
        //    }

        //    public override bool Equals(object obj)
        //    {
        //        if (!(obj is RenderedObjectCacheKey))
        //        {
        //            return false;
        //        }

        //        return Equals((RenderedObjectCacheKey)obj);
        //    }

        //    public override int GetHashCode()
        //    {
        //        return (Oid ?? _nullKey).GetHashCode() ^ (LayerName ?? "").GetHashCode();
        //    }

        //    #region IEquatable<RenderedObjectCacheKey> Members

        //    public bool Equals(RenderedObjectCacheKey other)
        //    {
        //        if (other.LayerName != LayerName)
        //        {
        //            return false;
        //        }

        //        if (other.Oid == null && Oid == null)
        //        {
        //            return true;
        //        }

        //        if (other.Oid == null || Oid == null)
        //        {
        //            return false;
        //        }

        //        return other.Oid.Equals(Oid);
        //    }

        //    #endregion
        //} 
        //#endregion

        #region Private static fields
        private static readonly object _rendererInitSync = new object();
        private static object _vectorRenderer;
        private static object _rasterRenderer;
        #endregion

        #region Private instance fields
        private readonly ViewSelection2D _selection;
        private double _maximumWorldWidth = Double.PositiveInfinity;
        private double _minimumWorldWidth = 0.0;
        // Offsets the origin of the spatial reference system so that the 
        // center of the view coincides with the center of the extents of the map.
        private bool _viewIsEmpty = true;
        private readonly Matrix2D _originNormalizeTransform = new Matrix2D();
        private readonly Matrix2D _rotationTransform = new Matrix2D();
        private readonly Matrix2D _translationTransform = new Matrix2D();
        private readonly Matrix2D _scaleTransform = new Matrix2D();
        private readonly Matrix2D _toViewCoordinates = new Matrix2D();
        private Matrix2D _toViewTransform;
        private Matrix2D _toWorldTransform;
        private StyleColor _backgroundColor;
        private readonly List<IFeatureLayer> _wiredLayers = new List<IFeatureLayer>();

        //private readonly Dictionary<RenderedObjectCacheKey, IEnumerable> _renderObjectsCache
        //    = new Dictionary<RenderedObjectCacheKey, IEnumerable>();
        #endregion

        #region Object construction / disposal

        /// <summary>
        /// Creates a new MapPresenter2D.
        /// </summary>
        /// <param name="map">The map to present.</param>
        /// <param name="mapView">The view to present the map on.</param>
        protected MapPresenter2D(Map map, IMapView2D mapView)
            : base(map, mapView)
        {
            createRenderers();

            //_selection = ViewSelection2D.CreateRectangluarSelection(Point2D.Zero, Size2D.Zero);
            _selection = new ViewSelection2D();

            wireupExistingLayers(Map.Layers);
            Map.Layers.ListChanged += handleLayersChanged;
            Map.PropertyChanged += handleMapPropertyChanged;

            View.Hover += handleViewHover;
            View.BeginAction += handleViewBeginAction;
            View.MoveTo += handleViewMoveTo;
            View.EndAction += handleViewEndAction;
            View.BackgroundColorChangeRequested += handleViewBackgroundColorChangeRequested;
            View.GeoCenterChangeRequested += handleViewGeoCenterChangeRequested;
            View.MaximumWorldWidthChangeRequested += handleViewMaximumWorldWidthChangeRequested;
            View.MinimumWorldWidthChangeRequested += handleViewMinimumWorldWidthChangeRequested;
            View.OffsetChangeRequested += handleViewOffsetChangeRequested;
            View.SizeChangeRequested += handleViewSizeChangeRequested;
            View.ViewEnvelopeChangeRequested += handleViewViewEnvelopeChangeRequested;
            View.WorldAspectRatioChangeRequested += handleViewWorldAspectRatioChangeRequested;
            View.ZoomToExtentsRequested += handleViewZoomToExtentsRequested;
            View.ZoomToViewBoundsRequested += handleViewZoomToViewBoundsRequested;
            View.ZoomToWorldBoundsRequested += handleViewZoomToWorldBoundsRequested;
            View.ZoomToWorldWidthRequested += handleViewZoomToWorldWidthRequested;
            _selection.SelectionChanged += handleSelectionChanged;

            _toViewCoordinates.Scale(1, -1);
            _toViewCoordinates.OffsetX = View.ViewSize.Width / 2;
            _toViewCoordinates.OffsetY = View.ViewSize.Height / 2;

            if (Map.Extents != BoundingBox.Empty)
            {
                normalizeOrigin();
            }
        }

        private void wireupExistingLayers(IEnumerable<ILayer> layerCollection)
        {
            foreach (ILayer layer in layerCollection)
            {
                if (layer is IFeatureLayer)
                {
                    IFeatureLayer featureLayer = layer as IFeatureLayer;
                    Debug.Assert(featureLayer != null);
                    if (!_wiredLayers.Contains(featureLayer))
                    {
                        _wiredLayers.Add(featureLayer);
                        featureLayer.SelectedFeatures.ListChanged += handleSelectedFeaturesListChanged;
                    }
                }
            }
        }

        #endregion

        #region Abstract methods

        protected abstract IVectorRenderer2D CreateVectorRenderer();
        protected abstract IRasterRenderer2D CreateRasterRenderer();
        protected abstract Type GetRenderObjectType();

        #endregion

        protected void RenderSelection(ViewSelection2D selection)
        {
            OnRenderingSelection();

            IVectorRenderer2D renderer = VectorRenderer;

            Debug.Assert(renderer != null);

            Path2D path = selection.Path.Clone() as Path2D;
            Debug.Assert(path != null);
            path.TransformPoints(ToWorldTransformInternal);
            IEnumerable<Path2D> paths = new Path2D[] { path };

            IEnumerable renderedSelection =
                renderer.RenderPaths(paths,
                    selection.FillBrush, null, null,
                    selection.OutlineStyle, null, null, RenderState.Normal);

            View.ShowRenderedObjects(renderedSelection);

            OnRenderedSelection();
        }

        protected virtual void RenderFeatureLayer(IFeatureLayer layer)
        {
            IFeatureRenderer renderer = GetRenderer<IFeatureRenderer>(layer);

            Debug.Assert(renderer != null);

            IEnumerable<FeatureDataRow> features
                = layer.Features.Select(ViewEnvelopeInternal.ToGeometry());

            Debug.Assert(layer.Style is VectorStyle);
            VectorStyle style = layer.Style as VectorStyle;

            foreach (FeatureDataRow feature in features)
            {
                IEnumerable renderedFeature;

                renderedFeature = renderer.RenderFeature(feature, style, RenderState.Normal);
                View.ShowRenderedObjects(renderedFeature);
            }

            foreach (FeatureDataRow selectedFeature in (IEnumerable<FeatureDataRow>)layer.SelectedFeatures)
            {
                IEnumerable renderedFeature = renderer.RenderFeature(selectedFeature, style, RenderState.Selected);
                View.ShowRenderedObjects(renderedFeature);
            }
        }

        protected void RenderRasterLayer(IRasterLayer layer)
        {
            throw new NotImplementedException();
        }

        #region Properties

        /// <summary>
        /// Gets or sets map background color.
        /// </summary>
        /// <remarks>
        /// Defaults to transparent.
        /// </remarks>
        protected StyleColor BackgroundColorInternal
        {
            get { return _backgroundColor; }
            set
            {
                if (_backgroundColor != value)
                {
                    _backgroundColor = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets center of map in world coordinates.
        /// </summary>
        protected GeoPoint GeoCenterInternal
        {
            get
            {
                if (ToWorldTransformInternal == null)
                {
                    return Point.Empty;
                }

                Point2D values = ToWorldTransformInternal.TransformVector(
                    View.ViewSize.Width / 2, View.ViewSize.Height / 2);

                return new GeoPoint(values.X, values.Y);
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                setViewMetricsInternal(View.ViewSize, value, WorldWidthInternal);
            }
        }

        /// <summary>
        /// Gets or sets the minimum width in world units of the view.
        /// </summary>
        protected double MaximumWorldWidthInternal
        {
            get { return _maximumWorldWidth; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value",
                                                          value, "Maximum world width must greater than 0.");
                }

                if (_maximumWorldWidth != value)
                {
                    _maximumWorldWidth = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum width in world units of the view.
        /// </summary>
        protected double MinimumWorldWidthInternal
        {
            get { return _minimumWorldWidth; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value",
                                                          value, "Minimum world width must be 0 or greater.");
                }

                if (_minimumWorldWidth != value)
                {
                    _minimumWorldWidth = value;
                }
            }
        }

        /// <summary>
        /// Gets the width of a pixel in world coordinate units.
        /// </summary>
        /// <remarks>
        /// The value returned is the same as <see cref="WorldUnitsPerPixelInternal"/>.
        /// </remarks>
        protected double PixelWorldWidthInternal
        {
            get { return WorldUnitsPerPixelInternal; }
        }

        /// <summary>
        /// Gets the height of a pixel in world coordinate units.
        /// </summary>
        /// <remarks>
        /// The value returned is the same as <see cref="WorldUnitsPerPixelInternal"/> 
        /// unless <see cref="WorldAspectRatioInternal"/> is different from 1.
        /// </remarks>
        protected double PixelWorldHeightInternal
        {
            get { return WorldUnitsPerPixelInternal * WorldAspectRatioInternal; }
        }

        /// <summary>
        /// Gets the instance of the concrete
        /// <see cref="RasterRenderer2D{TRenderObject}"/> used
        /// for the specific display technology which a base class
        /// is created to support.
        /// </summary>
        protected IRasterRenderer2D RasterRenderer
        {
            get
            {
                if (Thread.VolatileRead(ref _rasterRenderer) == null)
                {
                    lock (_rendererInitSync)
                    {
                        if (Thread.VolatileRead(ref _rasterRenderer) == null)
                        {
                            IRenderer rasterRenderer = CreateRasterRenderer();
                            Thread.VolatileWrite(ref _rasterRenderer, rasterRenderer);
                        }
                    }
                }

                return _rasterRenderer as IRasterRenderer2D;
            }
        }

        /// <summary>
        /// A selection on a view.
        /// </summary>
        protected ViewSelection2D SelectionInternal
        {
            get { return _selection; }
            //private set { _selection = value; }
        }

        /// <summary>
        /// Gets a <see cref="Matrix2D"/> used to project the world
        /// coordinate system into the view coordinate system. 
        /// The inverse of the <see cref="ToWorldTransformInternal"/> matrix.
        /// </summary>
        protected Matrix2D ToViewTransformInternal
        {
            get
            {
                if (_viewIsEmpty)
                {
                    return null;
                }

                return _toViewTransform;
            }
            private set
            {
                _toViewTransform = value;
            }
        }

        /// <summary>
        /// Gets a <see cref="Matrix2D"/> used to reverse the view projection
        /// transform to get world coordinates.
        /// The inverse of the <see cref="ToViewTransformInternal"/> matrix.
        /// </summary>
        protected Matrix2D ToWorldTransformInternal
        {
            get { return _toWorldTransform; }
            private set { _toWorldTransform = value; }
        }

        /// <summary>
        /// Gets the instance of the concrete
        /// <see cref="VectorRenderer2D{TRenderObject}"/> used
        /// for the specific display technology which a base class
        /// is created to support.
        /// </summary>
        protected IVectorRenderer2D VectorRenderer
        {
            get
            {
                if (Thread.VolatileRead(ref _vectorRenderer) == null)
                {
                    lock (_rendererInitSync)
                    {
                        if (Thread.VolatileRead(ref _vectorRenderer) == null)
                        {
                            IRenderer vectorRenderer = CreateVectorRenderer();
                            Thread.VolatileWrite(ref _vectorRenderer, vectorRenderer);
                        }
                    }
                }

                return _vectorRenderer as IVectorRenderer2D;
            }
        }

        /// <summary>
        /// Gets or sets the extents of the current view in world units.
        /// </summary>
        protected BoundingBox ViewEnvelopeInternal
        {
            get
            {
                if (ToWorldTransformInternal == null)
                {
                    return BoundingBox.Empty;
                }

                Point2D lowerLeft = ToWorldTransformInternal.TransformVector(0, View.ViewSize.Height);
                Point2D upperRight = ToWorldTransformInternal.TransformVector(View.ViewSize.Width, 0);
                return new BoundingBox(lowerLeft.X, lowerLeft.Y, upperRight.X, upperRight.Y);
            }
            set { setViewEnvelopeInternal(value); }
        }

        /// <summary>
        /// Gets or sets the aspect ratio of the <see cref="WorldHeightInternal"/> 
        /// to the <see cref="WorldWidthInternal"/>.
        /// </summary>
        /// <remarks> 
        /// A value less than 1 will make the map stretch upwards 
        /// (the view will cover less world distance vertically), 
        /// and greater than 1 will make it more squat (the view will 
        /// cover more world distance vertically).
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Throws an exception when value is 0 or less.
        /// </exception>
        protected double WorldAspectRatioInternal
        {
            get { return 1 / Math.Abs(_scaleTransform.M22 / _scaleTransform.M11); }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value",
                                                          value, "Invalid pixel aspect ratio.");
                }

                double currentRatio = WorldAspectRatioInternal;

                if (currentRatio != value)
                {
                    double ratioModifier = value / currentRatio;

                    _scaleTransform.M22 /= ratioModifier;

                    if (ToViewTransformInternal != null)
                    {
                        ToViewTransformInternal = concatenateViewMatrix();
                        ToWorldTransformInternal = ToViewTransformInternal.Inverse;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the height of view in world units.
        /// </summary>
        /// <returns>
        /// The height of the view in world units, taking into account <see cref="WorldAspectRatioInternal"/> 
        /// (<see cref="WorldWidthInternal"/> * <see cref="WorldAspectRatioInternal"/> * 
        /// <see cref="Presentation.Presenters.BasePresenter{TView}.View"/> ViewSize height 
        /// / <see cref="Presentation.Presenters.BasePresenter{TView}.View"/> ViewSize width).
        /// </returns>
        protected double WorldHeightInternal
        {
            get { return WorldWidthInternal * WorldAspectRatioInternal * View.ViewSize.Height / View.ViewSize.Width; }
        }

        /// <summary>
        /// Gets the width of a pixel in world coordinate units.
        /// </summary>
        protected double WorldUnitsPerPixelInternal
        {
            get
            {
                return ToWorldTransformInternal == null
                           ? 0
                           : ToWorldTransformInternal.M11;
            }
        }

        /// <summary>
        /// Gets the width of view in world units.
        /// </summary>
        /// <returns>The width of the view in world units (<see cref="Presentation.Presenters.BasePresenter{TView}.View" /> 
        /// height * <see cref="WorldUnitsPerPixelInternal"/>).</returns>
        protected double WorldWidthInternal
        {
            get { return View.ViewSize.Width * WorldUnitsPerPixelInternal; }
        }

        #endregion

        #region Methods

        protected virtual void SetViewBackgroundColor(StyleColor fromColor, StyleColor toColor) { }
        protected virtual void SetViewGeoCenter(Point fromGeoPoint, Point toGeoPoint) { }
        protected virtual void SetViewEnvelope(BoundingBox fromEnvelope, BoundingBox toEnvelope) { }
        protected virtual void SetViewMaximumWorldWidth(double fromMaxWidth, double toMaxWidth) { }
        protected virtual void SetViewMinimumWorldWidth(double fromMinWidth, double toMinWidth) { }
        protected virtual void SetViewSize(Size2D fromSize, Size2D toSize) { }
        protected virtual void SetViewWorldAspectRatio(double fromRatio, double toRatio) { }

        protected Point2D ToViewInternal(Point point)
        {
            return worldToView(point);
        }

        protected Point2D ToViewInternal(double x, double y)
        {
            return ToViewTransformInternal.TransformVector(x, y);
        }

        protected Point ToWorldInternal(Point2D point)
        {
            return viewToWorld(point);
        }

        protected Point ToWorldInternal(double x, double y)
        {
            Point2D values = ToWorldTransformInternal.TransformVector(x, y);
            return new GeoPoint(values.X, values.Y);
        }

        protected BoundingBox ToWorldInternal(Rectangle2D bounds)
        {
            if(ToWorldTransformInternal == null)
            {
                return BoundingBox.Empty;
            }

            Point2D lowerRight = ToWorldTransformInternal.TransformVector(bounds.Left, bounds.Bottom);
            Point2D upperLeft = ToWorldTransformInternal.TransformVector(bounds.Right, bounds.Top);
            return new BoundingBox(lowerRight.X, lowerRight.Y, upperLeft.X, upperLeft.Y);
        }

        /// <summary>
        /// Zooms to the extents of all visible layers in the current <see cref="Map"/>.
        /// </summary>
        protected void ZoomToExtentsInternal()
        {
            ZoomToWorldBoundsInternal(Map.Extents);
        }

        /// <summary>
        /// Zooms the map to fit a view bounding box. 
        /// </summary>
        /// <remarks>
        /// Transforms view coordinates into
        /// world coordinates using <see cref="ToWorldTransformInternal"/> 
        /// to perform zoom. 
        /// This means the heuristic to determine the final value of 
        /// <see cref="ViewEnvelopeInternal"/> after the zoom is the same as in 
        /// <see cref="ZoomToWorldBoundsInternal"/>.
        /// </remarks>
        /// <param name="viewBounds">
        /// The view bounds, translated into world bounds,
        /// to set the zoom to.
        /// </param>
        protected void ZoomToViewBoundsInternal(Rectangle2D viewBounds)
        {
            if (_viewIsEmpty)
            {
                throw new InvalidOperationException("No visible region is set for this view.");
            }

            BoundingBox worldBounds = new BoundingBox(
                ToWorldInternal(viewBounds.LowerLeft), ToWorldInternal(viewBounds.UpperRight));

            setViewEnvelopeInternal(worldBounds);
        }

        /// <summary>
        /// Zooms the map to fit a world bounding box.
        /// </summary>
        /// <remarks>
        /// The <paramref name="zoomBox"/> value is stretched
        /// to fit the current view. For example, if a view has a size of
        /// 100 x 100, which is a 1:1 ratio, and the bounds of zoomBox are 
        /// 200 x 100, which is a 2:1 ratio, the bounds are stretched to be 
        /// 200 x 200 to match the view size ratio. This ensures that at least
        /// all of the area selected in the zoomBox is displayed, and possibly more
        /// area.
        /// </remarks>
        /// <param name="zoomBox">
        /// <see cref="BoundingBox"/> to set zoom to.
        /// </param>
        protected void ZoomToWorldBoundsInternal(BoundingBox zoomBox)
        {
            setViewEnvelopeInternal(zoomBox);
        }

        /// <summary>
        /// Zooms the view to the given width.
        /// </summary>
        /// <remarks>
        /// View modifiers <see cref="MinimumWorldWidthInternal"/>, 
        /// <see cref="MaximumWorldWidthInternal"/> and 
        /// <see cref="WorldAspectRatioInternal"/>
        /// are taken into account when zooming to this width.
        /// </remarks>
        protected void ZoomToWorldWidthInternal(double newWorldWidth)
        {
            double newHeight;

            if (WorldWidthInternal == 0)
            {
                newHeight = newWorldWidth * (Map.Extents.Height / Map.Extents.Height);
            }
            else
            {
                newHeight = newWorldWidth * (WorldHeightInternal / WorldWidthInternal);
            }

            double halfWidth = newWorldWidth * 0.5;
            double halfHeight = newHeight * 0.5;

            GeoPoint center = GeoCenterInternal;

            if (center.IsEmpty())
            {
                center = Map.Extents.GetCentroid();
            }

            double centerX = center.X, centerY = center.Y;

            BoundingBox widthWorldBounds = new BoundingBox(
                centerX - halfWidth,
                centerY - halfHeight,
                centerX + halfWidth,
                centerY + halfHeight);

            ZoomToWorldBoundsInternal(widthWorldBounds);
        }

        #endregion

        #region Protected methods

        protected TRenderer GetRenderer<TRenderer>(ILayer layer)
            where TRenderer : class
        {
            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }

            return LayerRendererRegistry.Instance.Get<TRenderer>(layer);
        }

        protected virtual void OnRenderingAllLayers() { }

        protected virtual void OnRenderedAllLayers() { }

        protected virtual void OnRenderingLayer(ILayer layer) { }

        protected virtual void OnRenderedLayer(ILayer layer) { }

        protected virtual void OnRenderingSelection() { }

        protected virtual void OnRenderedSelection() { }

        #endregion

        #region Event handlers

        #region Map events

        private void handleLayersChanged(object sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    if (Map.Layers[e.NewIndex] is IFeatureLayer)
                    {
                        IFeatureLayer layer = Map.Layers[e.NewIndex] as IFeatureLayer;
                        Debug.Assert(layer != null);
                        BoundingBox viewEnvelope = View.ViewEnvelope;
                        _wiredLayers.Add(layer);
                        layer.SelectedFeatures.ListChanged += handleSelectedFeaturesListChanged;
                    }
                    RenderLayer(Map.Layers[e.NewIndex]);
                    break;
                case ListChangedType.ItemDeleted:
                    // LayerCollection defines an e.NewIndex as -1 when an item is being 
                    // deleted and not yet removed from the collection.
                    if (e.NewIndex >= 0)
                    {
                        RenderAllLayers();
                    }
                    else
                    {
                        IFeatureLayer layer = Map.Layers[e.OldIndex] as IFeatureLayer;

                        if (layer != null)
                        {
                            _wiredLayers.Remove(layer);
                            layer.SelectedFeatures.ListChanged -= handleSelectedFeaturesListChanged;
                        }
                    }
                    break;
                case ListChangedType.ItemMoved:
                    RenderAllLayers();
                    break;
                case ListChangedType.Reset:
                    wireupExistingLayers(Map.Layers);
                    RenderAllLayers();
                    break;
                case ListChangedType.ItemChanged:
                    if (e.PropertyDescriptor.Name == Layer.EnabledProperty.Name)
                    {
                        RenderAllLayers();
                    }
                    break;
                default:
                    break;
            }
        }

        protected void RenderAllLayers()
        {
            OnRenderingAllLayers();

            for (int i = Map.Layers.Count - 1; i >= 0; i--)
            {
                RenderLayer(Map.Layers[i]);
            }

            OnRenderedAllLayers();
        }

        protected void RenderLayer(ILayer layer)
        {
            OnRenderingLayer(layer);

            if (!layer.Enabled ||
                layer.Style.MinVisible > WorldWidthInternal ||
                layer.Style.MaxVisible < WorldWidthInternal)
            {
                return;
            }

            if (layer is IFeatureLayer)
            {
                RenderFeatureLayer(layer as IFeatureLayer);
            }
            else if (layer is IRasterLayer)
            {
                RenderRasterLayer(layer as IRasterLayer);
            }

            OnRenderedLayer(layer);
        }

        private void handleMapPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Map.SpatialReferenceProperty.Name)
            {
                //throw new NotImplementedException();
            }

            if (e.PropertyName == Map.SelectedLayersProperty.Name)
            {
                //throw new NotImplementedException();
            }

            if (e.PropertyName == Map.ActiveToolProperty.Name)
            {
                //throw new NotImplementedException();
            }

            if (e.PropertyName == Map.ExtentsProperty.Name)
            {
                normalizeOrigin();
            }
        }

        #endregion

        #region View events

        // Handles the offset request from the view
        private void handleViewOffsetChangeRequested(object sender, MapViewPropertyChangeEventArgs<Point2D> e)
        {
            GeoPoint geoOffset = ToWorldInternal(e.RequestedValue);
            GeoPoint newCenter = GeoCenterInternal + geoOffset;
            GeoCenterInternal = newCenter;
        }

        // Handles the size-change request from the view
        private void handleViewSizeChangeRequested(object sender, MapViewPropertyChangeEventArgs<Size2D> e)
        {
            setViewMetricsInternal(e.RequestedValue, GeoCenterInternal, WorldWidthInternal);
            SetViewSize(e.CurrentValue, e.RequestedValue);
            RenderAllLayers();
        }

        // Handles the hover request from the view
        private void handleViewHover(object sender, MapActionEventArgs<Point2D> e)
        {
            Map.GetActiveTool<IMapView2D, Point2D>().QueryAction(new ActionContext<IMapView2D, Point2D>(Map, View, e));
        }

        // Handles the begin action request from the view
        private void handleViewBeginAction(object sender, MapActionEventArgs<Point2D> e)
        {
            Map.GetActiveTool<IMapView2D, Point2D>().BeginAction(new ActionContext<IMapView2D, Point2D>(Map, View, e));
        }

        // Handles the move-to request from the view
        private void handleViewMoveTo(object sender, MapActionEventArgs<Point2D> e)
        {
            Map.GetActiveTool<IMapView2D, Point2D>().ExtendAction(new ActionContext<IMapView2D, Point2D>(Map, View, e));
        }

        // Handles the end action request from the view
        private void handleViewEndAction(object sender, MapActionEventArgs<Point2D> e)
        {
            Map.GetActiveTool<IMapView2D, Point2D>().EndAction(new ActionContext<IMapView2D, Point2D>(Map, View, e));
        }

        // Handles the background color change request from the view
        private void handleViewBackgroundColorChangeRequested(object sender, MapViewPropertyChangeEventArgs<StyleColor> e)
        {
            SetViewBackgroundColor(e.CurrentValue, e.RequestedValue);
        }

        // Handles the geographic view center change request from the view
        private void handleViewGeoCenterChangeRequested(object sender, MapViewPropertyChangeEventArgs<Point> e)
        {
            GeoCenterInternal = e.RequestedValue;

            SetViewGeoCenter(e.CurrentValue, e.RequestedValue);
        }

        // Handles the maximum world width change request from the view
        private void handleViewMaximumWorldWidthChangeRequested(object sender, MapViewPropertyChangeEventArgs<double> e)
        {
            MaximumWorldWidthInternal = e.RequestedValue;

            SetViewMaximumWorldWidth(e.CurrentValue, e.RequestedValue);
        }

        // Handles the minimum world width change request from the view
        private void handleViewMinimumWorldWidthChangeRequested(object sender, MapViewPropertyChangeEventArgs<double> e)
        {
            MinimumWorldWidthInternal = e.RequestedValue;

            SetViewMinimumWorldWidth(e.CurrentValue, e.RequestedValue);
        }

        // Handles the view envelope change request from the view
        private void handleViewViewEnvelopeChangeRequested(object sender, MapViewPropertyChangeEventArgs<BoundingBox> e)
        {
            ViewEnvelopeInternal = e.RequestedValue;

            SetViewEnvelope(e.CurrentValue, e.RequestedValue);
        }

        // Handles the world aspect ratio change request from the view
        private void handleViewWorldAspectRatioChangeRequested(object sender, MapViewPropertyChangeEventArgs<double> e)
        {
            WorldAspectRatioInternal = e.RequestedValue;

            SetViewWorldAspectRatio(e.CurrentValue, e.RequestedValue);
        }

        // Handles the view zoom to extents request from the view
        private void handleViewZoomToExtentsRequested(object sender, EventArgs e)
        {
            ZoomToExtentsInternal();
        }

        // Handles the view zoom to specified view bounds request from the view
        private void handleViewZoomToViewBoundsRequested(object sender, MapViewPropertyChangeEventArgs<Rectangle2D> e)
        {
            ZoomToViewBoundsInternal(e.RequestedValue);
        }

        // Handles the view zoom to specified world bounds request from the view
        private void handleViewZoomToWorldBoundsRequested(object sender, MapViewPropertyChangeEventArgs<BoundingBox> e)
        {
            ZoomToWorldBoundsInternal(e.RequestedValue);
        }

        // Handles the view zoom to specified world width request from the view
        private void handleViewZoomToWorldWidthRequested(object sender, MapViewPropertyChangeEventArgs<double> e)
        {
            ZoomToWorldWidthInternal(e.RequestedValue);
        }

        // Handles the selection changes on the view
        private void handleSelectionChanged(object sender, EventArgs e)
        {
            if (SelectionInternal.Path.CurrentFigure == null)
            {
                return;
            }

            RenderSelection(SelectionInternal);
        }

        #endregion

        #endregion

        #region Private helper methods

        private void createRenderers()
        {
            Type renderObjectType = GetRenderObjectType();

            // TODO: load renderers from configuration...
            Type basicGeometryRendererType = typeof(BasicGeometryRenderer2D<>);
            Type constructedGeom = basicGeometryRendererType.MakeGenericType(renderObjectType);
            IRenderer renderer = Activator.CreateInstance(constructedGeom, VectorRenderer) as IRenderer;
            LayerRendererRegistry.Instance.Register(typeof(GeometryLayer), renderer);

            // TODO: figure out how to factor label renderer...
            //Type labelRendererType = typeof (LabelRenderer2D<>);
            //Type constructedLabel = labelRendererType.MakeGenericType(renderObjectType);
            //renderer = Activator.CreateInstance(constructedLabel, VectorRenderer) as IRenderer;
            //LayerRendererRegistry.Instance.Register(typeof(LabelLayer), renderer);

            // TODO: create raster renderer
        }

        private void normalizeOrigin()
        {
            BoundingBox extents = Map.Extents;
            _originNormalizeTransform.OffsetX = -extents.Left;
            _originNormalizeTransform.OffsetY = -extents.Bottom;
        }

        // Performs computations to set the view metrics to the given envelope.
        private void setViewEnvelopeInternal(BoundingBox newEnvelope)
        {
            if (newEnvelope.IsEmpty)
            {
                throw new ArgumentException("newEnvelope cannot be empty.");
            }

            BoundingBox oldEnvelope = ViewEnvelopeInternal;

            if (oldEnvelope == newEnvelope)
            {
                return;
            }

            double oldWidth, oldHeight;

            if (oldEnvelope.IsEmpty)
            {
                oldWidth = 1;
                oldHeight = 1;
            }
            else
            {
                oldWidth = oldEnvelope.Width;
                oldHeight = oldEnvelope.Height;
            }

            double widthZoomRatio = newEnvelope.Width / oldWidth;
            double heightZoomRatio = newEnvelope.Height / oldHeight;

            double newWorldWidth = widthZoomRatio > heightZoomRatio
                                       ? newEnvelope.Width
                                       : newEnvelope.Width * heightZoomRatio / widthZoomRatio;

            if (newWorldWidth < _minimumWorldWidth)
            {
                newWorldWidth = _minimumWorldWidth;
            }

            if (newWorldWidth > _maximumWorldWidth)
            {
                newWorldWidth = _maximumWorldWidth;
            }

            setViewMetricsInternal(View.ViewSize, newEnvelope.GetCentroid(), newWorldWidth);
        }

        // Performs computations to set the view metrics given the parameters
        // of the view size, the geographic center of the view, and how much
        // of the world to show in the view by width.
        private void setViewMetricsInternal(Size2D newViewSize, GeoPoint newCenter, double newWorldWidth)
        {
            GeoPoint oldCenter = GeoCenterInternal;

            // Flag to indicate world matrix needs to be recomputed
            bool viewMatrixChanged = false;

            // Change geographic center of the view by translating pan matrix
            if (!oldCenter.Equals(newCenter))
            {
                if (oldCenter.IsEmpty())
                {
                    _translationTransform.OffsetX += -newCenter.X;
                    _translationTransform.OffsetY += -newCenter.Y;
                }
                else
                {
                    _translationTransform.OffsetX += -(newCenter.X - oldCenter.X);
                    _translationTransform.OffsetY += -(newCenter.Y - oldCenter.Y);
                }

                viewMatrixChanged = true;
            }

            // Compute new world units per pixel based on view size and desired 
            // world width, and scale the transform accordingly
            double newWorldUnitsPerPixel = newWorldWidth / newViewSize.Width;

            if (newWorldUnitsPerPixel != WorldUnitsPerPixelInternal)
            {
                double newScale = 1 / newWorldUnitsPerPixel;
                _scaleTransform.M22 = newScale / WorldAspectRatioInternal;
                _scaleTransform.M11 = newScale;

                viewMatrixChanged = true;
            }

            // Change view size
            if (newViewSize != View.ViewSize)
            {
                _toViewCoordinates.OffsetX = newViewSize.Width / 2;
                _toViewCoordinates.OffsetY = -newViewSize.Height / 2;

                View.ViewSize = newViewSize;
            }

            // If the view metrics have changed, recompute the inverse view matrix
            // and set the visible region of the map
            if (viewMatrixChanged)
            {
                _viewIsEmpty = false;

                ToViewTransformInternal = concatenateViewMatrix();
                ToWorldTransformInternal = ToViewTransformInternal.Inverse;

                RenderAllLayers();
            }
        }

        private Matrix2D concatenateViewMatrix()
        {
            IMatrixD combined = _originNormalizeTransform
                .Multiply(_rotationTransform)
                .Multiply(_translationTransform)
                .Multiply(_scaleTransform)
                .Multiply(_toViewCoordinates);

            return new Matrix2D(combined);
        }

        // Computes the view space coordinates of a point in world space
        private Point2D worldToView(GeoPoint geoPoint)
        {
            if (ToViewTransformInternal == null)
            {
                return Point2D.Empty;
            }
            else
            {
                return ToViewTransformInternal.TransformVector(geoPoint.X, geoPoint.Y);
            }
        }

        // Computes the world space coordinates of a point in view space
        private GeoPoint viewToWorld(Point2D viewPoint)
        {
            if (ToWorldTransformInternal == null)
            {
                return GeoPoint.Empty;
            }
            else
            {
                Point2D values = ToWorldTransformInternal.TransformVector(viewPoint.X, viewPoint.Y);
                return new GeoPoint(values.X, values.Y);
            }
        }

        private void handleSelectedFeaturesListChanged(object sender, ListChangedEventArgs e)
        {
            IFeatureLayer featureLayer = null;

            foreach (ILayer layer in Map.Layers)
            {
                if (layer is IFeatureLayer)
                {
                    if (ReferenceEquals((layer as IFeatureLayer).SelectedFeatures, sender))
                    {
                        featureLayer = layer as IFeatureLayer;
                        break;
                    }
                }
            }

            if (featureLayer == null)
            {
                return;
            }

            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    RenderFeatureLayer(featureLayer);
                    break;
                case ListChangedType.Reset:
                    if (featureLayer.SelectedFeatures.Count > 0)
                    {
                        RenderFeatureLayer(featureLayer);
                    }
                    break;
                case ListChangedType.ItemChanged:
                case ListChangedType.ItemDeleted:
                case ListChangedType.ItemMoved:
                case ListChangedType.PropertyDescriptorAdded:
                case ListChangedType.PropertyDescriptorChanged:
                case ListChangedType.PropertyDescriptorDeleted:
                default:
                    break;
            }
        }
        #endregion
    }
}