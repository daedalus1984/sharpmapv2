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

using System;
using System.Xml.Serialization;

namespace SharpMap.Symbology
{
    /// <summary>
    /// Gives the X and Y displacements from the <see cref="AnchorPoint"/>. 
    /// </summary>
    /// <remarks>
    /// This element may be used to avoid over-plotting of multiple graphic symbols used as part of the same point symbol. 
    /// The displacements are in units of measure above and to the right of the point. The default displacement is X=0, Y=0. 
    /// If <see cref="Displacement"/> is used in conjunction with Size and/or Rotation then the graphic symbol whall be scaled 
    /// and/or rotated before it is displaced.
    /// </remarks>
    [Serializable]
    [XmlType(Namespace = "http://www.opengis.net/se", TypeName = "DisplacementType")]
    [XmlRoot("Displacement", Namespace = "http://www.opengis.net/se", IsNullable = false)]
    public class Displacement
    {
        private ParameterValue _displacementX;
        private ParameterValue _displacementY;

        public ParameterValue DisplacementX
        {
            get { return _displacementX; }
            set { _displacementX = value; }
        }

        public ParameterValue DisplacementY
        {
            get { return _displacementY; }
            set { _displacementY = value; }
        }
    }
}