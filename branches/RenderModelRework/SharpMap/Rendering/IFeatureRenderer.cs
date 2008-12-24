﻿// Copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
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

using SharpMap.Data;
using SharpMap.Layers;
using SharpMap.Symbology;

namespace SharpMap.Rendering
{
    /// <summary>
    /// Interface to a graphical renderer of feature data.
    /// </summary>
    public interface IFeatureRenderer : IRenderer
    {
        /// <summary>
        /// Gets or sets the default style if no style or theme information is provided.
        /// </summary>
        FeatureStyle DefaultStyle { get; set; }

        /// <summary>
        /// Renders the attributes and/or spatial data in the <paramref name="feature"/>.
        /// </summary>
        /// <param name="feature">
        /// A <see cref="IFeatureDataRecord"/> instance with spatial data.
        /// </param>
        void RenderFeature(IScene scene, ILayer layer, IFeatureDataRecord feature, RenderState renderState);
    }
}
