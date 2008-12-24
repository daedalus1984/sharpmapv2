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
using SharpMap.Expressions;

namespace SharpMap.Symbology
{
    [Serializable]
    [XmlType(Namespace = "http://www.opengis.net/se", TypeName = "ChangeCaseType")]
    [XmlRoot("ChangeCase", Namespace = "http://www.opengis.net/se", IsNullable = false)]
    public class ChangeCaseExpression : SymbologyFunctionExpression
    {
        private ParameterValue _stringValue;
        private ChangeCaseDirection _direction;
        private Boolean _directionSpecified;

        [XmlElement(Order = 0)]
        public ParameterValue StringValue
        {
            get { return _stringValue; }
            set { _stringValue = value; }
        }

        [XmlAttribute(AttributeName = "direction")]
        public ChangeCaseDirection Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        [XmlIgnore]
        public Boolean DirectionSpecified
        {
            get { return _directionSpecified; }
            set { _directionSpecified = value; }
        }

        #region Overrides of Expression

        public override Boolean Contains(Expression other)
        {
            throw new System.NotImplementedException();
        }

        public override Expression Clone()
        {
            throw new System.NotImplementedException();
        }

        public override Boolean Equals(Expression other)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }

    [Serializable]
    [XmlType(Namespace = "http://www.opengis.net/se", TypeName = "directionType")]
    public enum ChangeCaseDirection
    {
        [XmlEnum(Name = "toUpper")] ToUpper,
        [XmlEnum(Name = "toLower")] ToLower,
    }
}