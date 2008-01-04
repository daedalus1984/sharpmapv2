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
using NPack;
using NPack.Interfaces;
using IMatrixD = NPack.Interfaces.IMatrix<NPack.DoubleComponent>;
using IVectorD = NPack.Interfaces.IVector<NPack.DoubleComponent>;

namespace SharpMap.Rendering.Rendering2D
{
    /// <summary>
    /// A 2 dimensional measure of size.
    /// </summary>
    [Serializable]
    public struct Size2D : IVectorD, IHasEmpty, IComparable<Size2D>
    {
        private DoubleComponent _width, _height;
        private Boolean _hasValue;

        public static readonly Size2D Empty = new Size2D();
        public static readonly Size2D Zero = new Size2D(0, 0);
        public static readonly Size2D Unit = new Size2D(1, 1);

        #region Constructors
        public Size2D(Double width, Double height)
        {
            _width = width;
            _height = height;
            _hasValue = true;
        }
        #endregion

        #region ToString
        public override String ToString()
        {
            return String.Format("[ViewSize2D] Width: {0}, Height: {1}", Width, Height);
        }
        #endregion

        #region GetHashCode
        public override Int32 GetHashCode()
        {
            return unchecked(Width.GetHashCode() ^ Height.GetHashCode());
        }
        #endregion

        #region Properties
        public Double Width
        {
            get { return (Double)_width; }
        }

        public Double Height
        {
            get { return (Double)_height; }
        }

        public Double this[Int32 element]
        {
            get
            {
                checkIndex(element);

                return element == 0 ? (Double)_width : (Double)_height;
            }
        }

        public Boolean IsEmpty
        {
            get { return !_hasValue; }
        }

        public Int32 ComponentCount
        {
            get { return 2; }
        }

        public DoubleComponent[] Components
        {
            get { return new DoubleComponent[] { _width, _height }; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                checkIndex(value.Length);

                _width = value[0];
                _height = value[1];
            }
        }

        public Size2D Add(Size2D size)
        {
            if(IsEmpty)
            {
                return size;
            }

            if(size.IsEmpty)
            {
                return this;
            }

            return new Size2D(Width + size.Width, Height + size.Height);
        }

        public Size2D Subtract(Size2D size)
        {
            return Add(size.Negative());
        }

        public static Size2D operator + (Size2D lhs, Size2D rhs)
        {
            return lhs.Add(rhs);
        }

        public static Size2D operator -(Size2D lhs, Size2D rhs)
        {
            return lhs.Subtract(rhs);
        }

        public static Size2D operator *(Size2D factor, Double multiplier)
        {
            if(factor.IsEmpty)
            {
                return factor;
            }

            return new Size2D(factor.Width * multiplier, factor.Height * multiplier);
        }
        #endregion

        #region Equality Testing
        public static Boolean operator ==(Size2D lhs, Size2D rhs)
        {
            return lhs.Equals(rhs);
        }

        public static Boolean operator !=(Size2D lhs, Size2D rhs)
        {
            return !lhs.Equals(rhs);
        }

        public static Boolean operator ==(Size2D lhs, IVectorD rhs)
        {
            return lhs.Equals(rhs);
        }

        public static Boolean operator !=(Size2D lhs, IVectorD rhs)
        {
            return !lhs.Equals(rhs);
        }

        public override Boolean Equals(object obj)
        {
            if (obj is Size2D)
            {
                return Equals((Size2D)obj);
            }

            if (obj is IVectorD)
            {
                return Equals(obj as IVectorD);
            }

            return false;
        }

        public Boolean Equals(Size2D size)
        {
            return _width.Equals(size._width) &&
                _height.Equals(size._height) &&
                IsEmpty == size.IsEmpty;
        }

        #region IEquatable<IViewVector> Members

        public Boolean Equals(IVectorD other)
        {
            if (other == null)
            {
                return false;
            }

            if (ComponentCount != other.ComponentCount)
            {
                return false;
            }

            if (!_width.Equals(other[0]) || !_height.Equals(other[1]))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region IEquatable<IMatrix<DoubleComponent>> Members

        ///<summary>
        ///Indicates whether the current object is equal to another object of the same type.
        ///</summary>
        ///
        ///<returns>
        ///true if the current object is equal to the other parameter; otherwise, false.
        ///</returns>
        ///
        ///<param name="other">An object to compare with this object.</param>
        public Boolean Equals(IMatrixD other)
        {
            if (other == null)
            {
                return false;
            }

            if (other.RowCount != 1 || other.ColumnCount != 2)
            {
                return false;
            }

            if (!other[0, 0].Equals(_width) || !other[0, 1].Equals(_height))
            {
                return false;
            }

            return true;
        }

        #endregion
        #endregion

        public Point2D Clone()
        {
            return new Point2D((Double)_width, (Double)_height);
        }

        public Size2D Negative()
        {
            return new Size2D(-((Double)_width), -((Double)_height));
        }

        #region Private Helper Methods

        private static void checkIndex(Int32 index)
        {
            if (index != 0 && index != 1)
            {
                throw new ArgumentOutOfRangeException("index", index, "The element index must be either 0 or 1 for a 2D point.");
            }
        }

        private static void checkIndexes(Int32 row, Int32 column)
        {
            if (row != 0)
            {
                throw new ArgumentOutOfRangeException("row", row, "A Point2D has only 1 row.");
            }

            if (column < 0 || column > 1)
            {
                throw new ArgumentOutOfRangeException("column", row, "A Point2D has only 2 columns.");
            }
        }
        #endregion

        #region IEnumerable<Double> Members

        public IEnumerator<Double> GetEnumerator()
        {
            yield return (Double)_width;
            yield return (Double)_height;
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IVector<DoubleComponent> Members

        MatrixFormat IMatrix<DoubleComponent>.Format
        {
            get { return MatrixFormat.Unspecified; }
        }

        IVectorD IVectorD.Clone()
        {
            return Clone();
        }

        IVectorD IVectorD.Negative()
        {
            return Negative();
        }

        DoubleComponent IVectorD.this[Int32 index]
        {
            get
            {
                checkIndex(index);
                return this[index];
            }
            set
            {
                checkIndex(index);

                if (index == 0)
                {
                    _width = value;
                }
                else
                {
                    _height = value;
                }

                _hasValue = true;
            }
        }

        #endregion

        #region IAddable<IMatrix<DoubleComponent>> Members

        /// <summary>
        /// Returns the sum of the object and <paramref name="b"/>.
        /// It must not modify the value of the object.
        /// </summary>
        /// <param name="b">The second operand.</param>
        /// <returns>The sum.</returns>
        IMatrixD IAddable<IMatrix<DoubleComponent>>.Add(IMatrixD b)
        {
            return MatrixProcessor<DoubleComponent>.Instance.Operations.Add(this, b);
        }

        #endregion

        #region ISubtractable<IMatrix<DoubleComponent>> Members

        /// <summary>
        /// Returns the difference of the object and <paramref name="b"/>.
        /// It must not modify the value of the object.
        /// </summary>
        /// <param name="b">The second operand.</param>
        /// <returns>The difference.</returns>
        IMatrixD ISubtractable<IMatrix<DoubleComponent>>.Subtract(IMatrixD b)
        {
            return MatrixProcessor<DoubleComponent>.Instance.Operations.Subtract(this, b);
        }

        #endregion

        #region IHasZero<IMatrix<DoubleComponent>> Members

        /// <summary>
        /// Returns the additive identity.
        /// </summary>
        /// <value>e</value>
        IMatrixD IHasZero<IMatrixD>.Zero
        {
            get { return Zero; }
        }

        #endregion

        #region INegatable<IMatrix<DoubleComponent>> Members

        /// <summary>
        /// Returns the negative of the object. Must not modify the object itself.
        /// </summary>
        /// <returns>The negative.</returns>
        IMatrixD INegatable<IMatrixD>.Negative()
        {
            return Negative();
        }

        #endregion

        #region IMultipliable<IMatrix<DoubleComponent>> Members

        /// <summary>
        /// Returns the product of the object and <paramref name="b"/>.
        /// It must not modify the value of the object.
        /// </summary>
        /// <param name="b">The second operand.</param>
        /// <returns>The product.</returns>
        IMatrixD IMultipliable<IMatrix<DoubleComponent>>.Multiply(IMatrixD b)
        {
            return MatrixProcessor<DoubleComponent>.Instance.Operations.Multiply(this, b);
        }

        #endregion

        #region IDivisible<IMatrix<DoubleComponent>> Members

        /// <summary>
        /// Returns the quotient of the object and <paramref name="b"/>.
        /// It must not modify the value of the object.
        /// </summary>
        /// <param name="b">The second operand.</param>
        /// <returns>The quotient.</returns>
        IMatrixD IDivisible<IMatrix<DoubleComponent>>.Divide(IMatrixD b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IHasOne<IMatrix<DoubleComponent>> Members

        /// <summary>
        /// Returns the multiplicative identity.
        /// </summary>
        /// <value>e</value>
        IMatrixD IHasOne<IMatrixD>.One
        {
            get { return Unit; }
        }

        #endregion

        #region IEnumerable<DoubleComponent> Members

        ///<summary>
        ///Returns an enumerator that iterates through the collection.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        ///</returns>
        ///<filterpriority>1</filterpriority>
        IEnumerator<DoubleComponent> IEnumerable<DoubleComponent>.GetEnumerator()
        {
            yield return _width;
            yield return _height;
        }

        #endregion

        #region IMatrix<DoubleComponent> Members
        /// <summary>
        /// Makes an element-by-element copy of the matrix.
        /// </summary>
        /// <returns>An exact copy of the matrix.</returns>
        IMatrixD IMatrixD.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Gets the determinant for the matrix, if it exists.
        /// </summary>
        Double IMatrixD.Determinant
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the number of columns in the matrix.
        /// </summary>
        Int32 IMatrixD.ColumnCount
        {
            get { return 2; }
        }

        /// <summary>
        /// Gets true if the matrix is singular (non-invertable).
        /// </summary>
        Boolean IMatrixD.IsSingular
        {
            get { return true; }
        }

        /// <summary>
        /// Gets true if the matrix is invertable (non-singular).
        /// </summary>
        Boolean IMatrixD.IsInvertible
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the inverse of the matrix, if one exists.
        /// </summary>
        IMatrixD IMatrixD.Inverse
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets true if the matrix is square (<c>RowCount == ColumnCount != 0</c>).
        /// </summary>
        Boolean IMatrixD.IsSquare
        {
            get { return false; }
        }

        /// <summary>
        /// Gets true if the matrix is symmetrical.
        /// </summary>
        Boolean IMatrixD.IsSymmetrical
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the number of rows in the matrix.
        /// </summary>
        Int32 IMatrixD.RowCount
        {
            get { return 1; }
        }

        /// <summary>
        /// Gets a submatrix.
        /// </summary>
        /// <param name="rowIndexes">The indexes of the rows to include.</param>
        /// <param name="j0">The starting column to include.</param>
        /// <param name="j1">The ending column to include.</param>
        /// <returns></returns>
        IMatrixD IMatrixD.GetMatrix(Int32[] rowIndexes, Int32 j0, Int32 j1)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets or sets an element in the matrix.
        /// </summary>
        /// <param name="row">The index of the row of the element.</param>
        /// <param name="column">The index of the column of the element.</param>
        /// <returns>The value of the element at the given index.</returns>
        DoubleComponent IMatrixD.this[Int32 row, Int32 column]
        {
            get
            {
                checkIndexes(row, column);

                return this[column];
            }
            set
            {
                checkIndexes(row, column);

                (this as IVectorD)[column] = value;
            }
        }

        /// <summary>
        /// Returns the transpose of the matrix.
        /// </summary>
        /// <returns>The matrix with the rows as columns and columns as rows.</returns>
        IMatrixD IMatrixD.Transpose()
        {
            return
                new Matrix<DoubleComponent>((this as IMatrix<DoubleComponent>).Format,
                    new DoubleComponent[][] { new DoubleComponent[] { _width }, new DoubleComponent[] { _height } });
        }

        #endregion

        #region INegatable<IVector<DoubleComponent>> Members

        IVector<DoubleComponent> INegatable<IVector<DoubleComponent>>.Negative()
        {
            return new Size2D(-Width, -Height);
        }

        #endregion

        #region ISubtractable<IVector<DoubleComponent>> Members

        public IVector<DoubleComponent> Subtract(IVector<DoubleComponent> b)
        {
            if (b == null) throw new ArgumentNullException("b");

            if (b.ComponentCount != 2)
            {
                throw new ArgumentException("Vector must have only 2 components.");
            }

            return new Size2D(Width - (Double)b[0], Height - (Double)b[1]);
        }

        #endregion

        #region IHasZero<IVector<DoubleComponent>> Members

        IVector<DoubleComponent> IHasZero<IVector<DoubleComponent>>.Zero
        {
            get { return Zero; }
        }

        #endregion

        #region IAddable<IVector<DoubleComponent>> Members

        IVector<DoubleComponent> IAddable<IVector<DoubleComponent>>.Add(IVector<DoubleComponent> b)
        {
            if (b == null) throw new ArgumentNullException("b");

            if (b.ComponentCount != 2)
            {
                throw new ArgumentException("Vector must have only 2 components.");
            }

            return new Size2D(Width + (Double)b[0], Height + (Double)b[1]);
        }

        #endregion

        #region IDivisible<IVector<DoubleComponent>> Members

        public IVector<DoubleComponent> Divide(IVector<DoubleComponent> b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IHasOne<IVector<DoubleComponent>> Members

        IVector<DoubleComponent> IHasOne<IVector<DoubleComponent>>.One
        {
            get { return new Size2D(1, 1); }
        }

        #endregion

        #region IMultipliable<IVector<DoubleComponent>> Members

        IVector<DoubleComponent> IMultipliable<IVector<DoubleComponent>>.Multiply(IVector<DoubleComponent> b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IComparable<Size2D> Members

        public Int32 CompareTo(Size2D other)
        {
            if (other.Equals(this))
            {
                return 0;
            }

            if(other.Width < Width || other.Height < Height)
            {
                return -1;
            }

            return 1;
        }

        #endregion
    }
}
