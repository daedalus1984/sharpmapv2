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
using System.Collections.Generic;
using System.Text;

using SharpMap.Data;
using SharpMap.Layers;
using SharpMap.Geometries;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;

namespace SharpMap.Rendering
{
    public abstract class BaseFeatureRenderer2D<TStyle, TRenderObject> : IFeatureRenderer<ViewPoint2D, ViewSize2D, ViewRectangle2D, PositionedRenderObject2D<TRenderObject>>
        where TStyle : class, IStyle
    {
        //private TLayer _layer;
        private TStyle _style;
        private ITheme _theme;
        //private IViewTransformer<ViewPoint2D, ViewRectangle2D> _viewTransform;
        //private List<PositionedRenderObject2D<TRenderObject>> _renderedObjects = new List<PositionedRenderObject2D<TRenderObject>>();
        private StyleRenderingMode _renderMode;

        ~BaseFeatureRenderer2D()
        {
            Dispose(false);
        }

        #region Events
        ///// <summary>
        ///// Event fired when the layer has been rendered.
        ///// </summary>
        //public event EventHandler<LayerRenderedEventArgs> LayerRendered;

        /// <summary>
        /// Event fired when a feature has been rendered.
        /// </summary>
        public event EventHandler FeatureRendered;
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
        #endregion

        #region IFeatureRenderer<ViewPoint2D,ViewSize2D,ViewRectangle2D> Members

        public IEnumerable<PositionedRenderObject2D<TRenderObject>> RenderFeature(FeatureDataRow feature)
        {
            return RenderFeature(feature, null);
        }

        public IEnumerable<PositionedRenderObject2D<TRenderObject>> RenderFeature(FeatureDataRow feature, IRenderContext renderContext)
        {
            IEnumerable<PositionedRenderObject2D<TRenderObject>> renderedObjects = DoRenderFeature(feature, renderContext);
            OnFeatureRendered();
            return renderedObjects;
        }

        protected abstract IEnumerable<PositionedRenderObject2D<TRenderObject>> DoRenderFeature(FeatureDataRow feature, IRenderContext renderContext);

        /// <summary>
        /// Gets or sets thematic settings for the layer. Set to null to ignore thematics
        /// </summary>
        public ITheme Theme
        {
            get { return _theme; }
            set { _theme = value; }
        }

        //public TLayer Layer
        //{
        //    get { return _layer; }
        //    set { _layer = value; }
        //}

        /// <summary>
        /// Render whether smoothing (antialiasing) is applied to lines 
        /// and curves and the edges of filled areas
        /// </summary>
        public StyleRenderingMode StyleRenderingMode
        {
            get { return _renderMode; }
            set { _renderMode = value; }
        }

        /// <summary>
        /// Gets or sets the rendering style of the layer.
        /// </summary>
        public TStyle Style
        {
            get { return _style; }
            set { _style = value; }
        }

        //public IViewTransformer<ViewPoint2D, ViewRectangle2D> ViewTransformer
        //{
        //    get { return _viewTransform; }
        //    set { _viewTransform = value; }
        //}

        //public IList<PositionedRenderObject2D<TRenderObject>> RenderedObjects
        //{
        //    get { return _renderedObjects; }
        //    protected set 
        //    { 
        //        _renderedObjects.Clear();
        //        _renderedObjects.AddRange(value);
        //    }
        //}

        ///// <summary>
        ///// Renders the layer to the <paramref name="view"/>.
        ///// </summary>
        ///// <param name="view"><see cref="IMapView2D"/> to render layer on.</param>
        //public void Render(IMapView2D view)
        //{
        //    Render(view, Layer.Envelope);
        //}

        ///// <summary>
        ///// Renders the layer to the <paramref name="view"/>.
        ///// </summary>
        ///// <param name="view"><see cref="IMapView2D"/> to render layer on.</param>
        ///// <param name="region">Area of the map to render.</param>
        //public virtual void Render(IMapView2D view, BoundingBox region)
        //{
        //    OnLayerRendered(view);
        //}

        //public double MinVisible
        //{
        //    get { return _minVisible; }
        //    set { _minVisible = value; }
        //}

        //public double MaxVisible
        //{
        //    get { return _maxVisible; }
        //    set { _maxVisible = value; }
        //}

        #endregion

        #region IFeatureRenderer<ViewPoint2D,ViewSize2D,ViewRectangle2D> Explicit Implementation
        IStyle IRenderer<ViewPoint2D, ViewSize2D, ViewRectangle2D, PositionedRenderObject2D<TRenderObject>>.Style
        {
            get { return Style; }
            set { Style = value as TStyle; }
        }

        #endregion

        //protected void OnLayerRendered()
        //{
        //    EventHandler<LayerRenderedEventArgs> @event = LayerRendered;
        //    if (@event != null)
        //        @event(this, new LayerRenderedEventArgs(Layer)); //Fire event
        //}

        protected void OnFeatureRendered()
        {
            EventHandler @event = FeatureRendered;
            if (@event != null)
                @event(this, EventArgs.Empty); //Fire event
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}