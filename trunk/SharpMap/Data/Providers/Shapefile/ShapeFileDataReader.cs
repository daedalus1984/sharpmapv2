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
using System.Data;
using System.Data.Common;
using SharpMap.Geometries;

namespace SharpMap.Data.Providers.ShapeFile
{
    public class ShapeFileDataReader : IFeatureDataReader
    {
        private readonly ShapeFile _shapeFile;
        private readonly BoundingBox _queryRegion;
        private readonly DataTable _schemaTable;
        private FeatureDataRow<uint> _currentFeature;
        private uint _currentRecord;
        private bool _isDisposed;

        #region Object Construction / Disposal
        internal ShapeFileDataReader(ShapeFile source, BoundingBox queryRegion)
        {
            _queryRegion = queryRegion;
            _shapeFile = source;
            _schemaTable = source.GetSchemaTable();
        }
        
        #region Dispose Pattern

        ~ShapeFileDataReader()
        {
            Dispose(false);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            Dispose(true);
            IsDisposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion

        private void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                _shapeFile.Close();
            }

            OnDisposed();
        }

        public bool IsDisposed
        {
            get { return _isDisposed; }
            private set { _isDisposed = value; }
        }

        private void OnDisposed()
        {
            EventHandler e = Disposed;

            if (e != null)
            {
                e(this, EventArgs.Empty);
            }
        }

        public event EventHandler Disposed;

        #endregion

        #endregion

        #region IFeatureDataReader Members

        public Geometry GetGeometry()
        {
            checkState();

            if (_currentFeature.Geometry == null)
            {
                return null;
            }
            else
            {
                return _currentFeature.Geometry.Clone();
            }
        }

        public object GetOid()
        {
            checkState();
            return _currentRecord;
        }

        public bool HasOid
        {
            get
            {
                checkState(); 
                return true;
            }
        }

        #endregion

        #region IDataReader Members

        public void Close()
        {
            Dispose();
        }

        public int Depth
        {
            get
            {
                checkState();
                return 0;
            }
        }

        public DataTable GetSchemaTable()
        {
            checkState();
            return _schemaTable.Copy();
        }

        public bool IsClosed
        {
            get { return IsDisposed; }
        }

        public bool NextResult()
        {
            checkState();
            return false;
        }

        public bool Read()
        {
            checkState();

            BoundingBox featureBounds = BoundingBox.Empty;
            int featureCount = _shapeFile.GetFeatureCount();

            do
            {
                _currentRecord++;
                _currentFeature = _shapeFile.GetFeature(_currentRecord);
                if (_currentFeature != null && _currentFeature.Geometry != null)
                {
                    featureBounds = _currentFeature.Geometry.GetBoundingBox();
                }
            }
            while (_currentRecord < featureCount && !_queryRegion.Intersects(featureBounds));

            return _currentRecord < featureCount;
        }

        public int RecordsAffected
        {
            get { return _shapeFile.GetFeatureCount(); }
        }

        #endregion

        #region IDataRecord Members

        public int FieldCount
        {
            get
            {
                checkState(); 
                return _schemaTable.Rows.Count;
            }
        }

        public bool GetBoolean(int i)
        {
            checkState();
            checkIndex(i);

            return Convert.ToBoolean(_currentFeature[i]);
        }

        public byte GetByte(int i)
        {
            checkState();
            checkIndex(i);

            return Convert.ToByte(_currentFeature[i]);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            checkState();
            checkIndex(i);

            return Convert.ToChar(_currentFeature[i]);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotSupportedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            checkState();
            checkIndex(i);

            return Convert.ToDateTime(_currentFeature[i]);
        }

        public decimal GetDecimal(int i)
        {
            checkState();
            checkIndex(i);

            return Convert.ToDecimal(_currentFeature[i]);
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(int i)
        {
            checkState();
            checkIndex(i);

            return Convert.ToSingle(_currentFeature[i]);
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            checkState();
            checkIndex(i);

            return Convert.ToInt16(_currentFeature[i]);
        }

        public int GetInt32(int i)
        {
            checkState();
            checkIndex(i);

            return Convert.ToInt32(_currentFeature[i]);
        }

        public long GetInt64(int i)
        {
            checkState();
            checkIndex(i);

            return Convert.ToInt64(_currentFeature[i]);
        }

        public string GetName(int i)
        {
            checkState();
            return _schemaTable.Rows[i][0] as string;
        }

        public int GetOrdinal(string name)
        {
            checkState();

            if (name == null) throw new ArgumentNullException("name");

            int fieldCount = FieldCount;

            for (int i = 0; i < fieldCount; i++)
            {
                if (name.Equals(GetName(i)))
                {
                    return i;
                }
            }

            for (int i = 0; i < fieldCount; i++)
            {
                if (String.Compare(name, GetName(i), StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    return i;
                }
            }

            throw new IndexOutOfRangeException("Column name not found");
        }

        public string GetString(int i)
        {
            checkState();
            checkIndex(i);

            return Convert.ToString(_currentFeature[i]);
        }

        public object GetValue(int i)
        {
            checkState();
            checkIndex(i);

            return _currentFeature[i];
        }

        public int GetValues(object[] values)
        {
            checkState();

            if (values == null) throw new ArgumentNullException("values");

            int count = values.Length > FieldCount ? FieldCount : values.Length;

            for (int i = 0; i < count; i++)
            {
                values[i] = this[i];
            }

            return count;
        }

        public bool IsDBNull(int i)
        {
            checkState();
            checkIndex(i);

            return _currentFeature.IsNull(i);
        }

        public object this[string name]
        {
            get { checkState(); return _currentFeature[name]; }
        }

        public object this[int i]
        {
            get
            {
                checkState();
                checkIndex(i);

                return _currentFeature[i]; 
            }
        }

        #endregion

        #region Private helper methods


        private void checkIndex(int i)
        {
            if (i >= FieldCount)
            {
                throw new IndexOutOfRangeException("Index must be less than FieldCount.");
            }
        }

        private void checkState()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().ToString());
            if (_currentFeature == null)
            {
                throw new InvalidOperationException("The Read method must be called before accessing values.");
            }
        }
        #endregion
    }
}
