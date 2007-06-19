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
using System.IO;

using SharpMap.Layers;
using SharpMap.Styles;

namespace SharpMap.Rendering.Renderers
{
    public abstract class BaseRasterRenderer2D<TStyle, TRenderObject> : IRasterRenderer<ViewPoint2D, ViewSize2D, ViewRectangle2D, TRenderObject>
        where TStyle : class, IStyle
    {
        #region IRasterLayerRenderer<ViewPoint2D,ViewSize2D,ViewRectangle2D> Members

        public abstract void RenderRaster(Stream rasterData, ViewRectangle2D viewBounds, ViewRectangle2D rasterBounds);

        public abstract void RenderRaster(Stream rasterData, ViewRectangle2D viewBounds, ViewRectangle2D rasterBounds, IViewMatrix rasterTransform);

        #endregion

        #region IRenderer<ViewPoint2D,ViewSize2D,ViewRectangle2D,TRenderObject> Members

        public IStyle Style
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public SharpMap.Rendering.Thematics.ITheme Theme
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public IViewMatrix ViewTransform
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public StyleRenderingMode StyleRenderingMode
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public IList<TRenderObject> RenderedObjects
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
