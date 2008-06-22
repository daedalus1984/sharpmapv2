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

namespace SharpMap.Data.Providers.ShapeFile
{
    /// <summary>
    /// Exception thrown when the the Dbase file is corrupt.
    /// </summary>
    public class InvalidDbaseFileException : Exception
    {
        private readonly String _filename;

        public InvalidDbaseFileException(String filename) : this(filename, null) { }
        public InvalidDbaseFileException(String filename, String message) : this(filename, message, null) { }
        
        public InvalidDbaseFileException(String filename, String message, Exception inner) : base(message, inner)
        {
            _filename = filename;
        }

        public InvalidDbaseFileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _filename = info.GetString("_filename");
        }

        public String Filename
        {
            get { return _filename; }   
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_filename", _filename);
        }
    }
}
