// Copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
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
using GeoAPI.Coordinates;
using NPack.Interfaces;
using IMatrixD = NPack.Interfaces.IMatrix<NPack.DoubleComponent>;
using IVectorD = NPack.Interfaces.IVector<NPack.DoubleComponent>;

namespace SharpMap.Rendering
{
    /// <summary>
    /// Provides a base class for generating rendered objects from vector shapes.
    /// </summary>
    /// <remarks>
    /// This class is used to create a new <see cref="IVectorRenderer{TCoordinate}"/> for various graphics systems.
    /// </remarks>
    public abstract class VectorRenderer<TCoordinate> : Renderer, IVectorRenderer<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        #region Implementation of IVectorRenderer<TCoordinate>

        public abstract void RenderPaths(IScene scene, IEnumerable<Path<TCoordinate>> paths, IPen stroke, IBrush fill);
        public abstract void RenderSymbols(IScene scene, IEnumerable<TCoordinate> locations, ISymbol symbolData);

        #endregion
    }
}