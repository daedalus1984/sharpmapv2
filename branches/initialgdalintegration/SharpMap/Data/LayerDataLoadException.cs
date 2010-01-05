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
using System.Runtime.Serialization;

namespace SharpMap.Data
{
    /// <summary>
    /// Exception thrown when the layer data specified to the provider can not be
    /// found or is not accessible.
    /// </summary>
    [Serializable]
    public class LayerDataLoadException : SharpMapDataException
    {
        private readonly String _connectionId;

        public LayerDataLoadException(String connectionId)
            : this(connectionId, null)
        {
        }

        public LayerDataLoadException(String connectionId, String message)
            : this(connectionId, message, null)
        {
        }

        public LayerDataLoadException(String connectionId, String message, Exception inner)
            : base(message, inner)
        {
            _connectionId = connectionId;
        }

        public LayerDataLoadException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _connectionId = info.GetString("_connectionId");
        }

        public String ConnectionId
        {
            get { return _connectionId; }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("_connectionId", _connectionId);
        }
    }
}