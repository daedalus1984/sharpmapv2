﻿using System;
using System.Xml.Serialization;

namespace SharpMap.Expressions
{
    [Serializable]
    [XmlType(Namespace = "http://www.opengis.net/ogc", TypeName = "PropertyIsNullType")]
    [XmlRoot("PropertyIsNull", Namespace = "http://www.opengis.net/ogc", IsNullable = false)]
    public class PropertyIsNullExpression : ComparisonExpression
    {
        private PropertyNameExpression _propertyName;

        public PropertyNameExpression PropertyName
        {
            get { return _propertyName; }
            set { _propertyName = value; }
        }
    }
}