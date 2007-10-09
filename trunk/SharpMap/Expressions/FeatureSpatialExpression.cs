﻿// Copyright 2006, 2007 - Rory Plaire (codekaizen@gmail.com)
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
using SharpMap.Geometries;

namespace SharpMap.Expressions
{
    public class FeatureSpatialExpression : SpatialExpression, IEquatable<FeatureSpatialExpression>
    {
        private readonly IEnumerable _oids;
        private readonly bool _hasOidFilter;

        public FeatureSpatialExpression(IEnumerable oids)
            : this(null, SpatialExpressionType.None, oids) { }

        public FeatureSpatialExpression(Geometry queryRegion, SpatialExpressionType type, IEnumerable oids)
            : base(queryRegion, type)
        {
            _oids = oids;
            _hasOidFilter = oids != null;
        }

        public IEnumerable Oids
        {
            get 
            {
                if (!_hasOidFilter)
                {
                    return null;
                }
                else
                {
                    return generateOidFilter();
                }
            }
        }

        public static bool operator !=(FeatureSpatialExpression lhs, FeatureSpatialExpression rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator ==(FeatureSpatialExpression lhs, FeatureSpatialExpression rhs)
        {
            if (ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            if (!ReferenceEquals(lhs, null))
            {
                return lhs.Equals(rhs);
            }
            else
            {
                return rhs.Equals(lhs);
            }
        }

        public bool Equals(FeatureSpatialExpression other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (!base.Equals(other))
            {
                return false;
            }

            if (ReferenceEquals(_oids, other._oids))
            {
                return true;
            }

            if (!ReferenceEquals(other._oids, null))
            {
                return other._oids.Equals(_oids);
            }
            else
            {
                return _oids.Equals(other._oids);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Equals(obj as FeatureSpatialExpression);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return base.GetHashCode() ^ 137 * 
                    (_oids != null ? _oids.GetHashCode() : 81040488);
            }
        }

        public FeatureSpatialExpression Clone()
        {
            FeatureSpatialExpression clone = new FeatureSpatialExpression(
                QueryRegion == null ? null : QueryRegion.Clone(), QueryType, Oids);

            return clone;
        }

        private IEnumerable generateOidFilter()
        {
            if (ReferenceEquals(_oids, null))
            {
                yield break;
            }

            foreach (object oid in _oids)
            {
                yield return oid;
            }
        }
    }
}