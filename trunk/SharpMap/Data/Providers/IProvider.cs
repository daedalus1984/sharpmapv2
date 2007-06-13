// Portions copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
// Portions copyright 2006, 2007 - Rory Plaire (codekaizen@gmail.com)
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
using System.Collections.ObjectModel;
using System.Text;

using SharpMap.Geometries;

namespace SharpMap.Data.Providers
{

    /// <summary>
    /// Interface for data providers.
    /// </summary>
    public interface IProvider : IDisposable
    {
        /// <summary>
        /// Gets the features within the specified <see cref="SharpMap.Geometries.BoundingBox"/>.
        /// </summary>
        /// <param name="boundingBox"></param>
        /// <returns>Features within the specified <see cref="SharpMap.Geometries.BoundingBox"/>.</returns>
        ReadOnlyCollection<Geometry> GetGeometriesInView(BoundingBox boundingBox);

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by <paramref name="geom"/>.
        /// </summary>
        /// <param name="geom">Geometry to intersect with.</param>
        /// <param name="ds">FeatureDataSet to fill data into.</param>
        void ExecuteIntersectionQuery(Geometry geom, FeatureDataSet ds);

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by <paramref name="box"/>.
        /// </summary>
        /// <param name="box">BoundingBox to intersect with.</param>
        /// <param name="ds">FeatureDataSet to fill data into.</param>
        void ExecuteIntersectionQuery(BoundingBox box, FeatureDataSet ds);

        /// <summary>
        /// Returns the number of features in the entire dataset.
        /// </summary>
        /// <returns>Count of the features in the entire dataset.</returns>
        int GetFeatureCount();

        /// <summary>
        /// Geometric extent of the entire dataset.
        /// </summary>
        /// <returns>The extents of the dataset as a BoundingBox.</returns>
        BoundingBox GetExtents();

        /// <summary>
        /// Gets the connection ID of the datasource.
        /// </summary>
        /// <remarks>
        /// <para>The ConnectionId should be unique to the datasource (for instance the filename or the
        /// connectionstring), and is meant to be used for connection pooling.</para>
        /// <para>If connection pooling doesn't apply to this datasource, the ConnectionId should return String.Empty</para>
        /// </remarks>
        string ConnectionId { get; }

        /// <summary>
        /// Opens the datasource.
        /// </summary>
        void Open();

        /// <summary>
        /// Closes the datasource.
        /// </summary>
        void Close();

        /// <summary>
        /// Returns true if the datasource is currently open.
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// The spatial reference ID (CRS).
        /// </summary>
        int Srid { get; set; }
    }

	/// <summary>
	/// Interface for data providers with features having ID values.
	/// </summary>
    public interface IProvider<TOid> : IProvider
	{
		/// <summary>
        /// Returns all objects whose <see cref="SharpMap.Geometries.BoundingBox"/> intersects 'boundingBox'.
		/// </summary>
		/// <remarks>
		/// This method is usually much faster than the QueryFeatures method, because intersection tests
		/// are performed on objects simplifed by their <see cref="SharpMap.Geometries.BoundingBox"/>, and using the Spatial Index
		/// </remarks>
        /// <param name="boundingBox">Box that objects should intersect.</param>
		/// <returns></returns>
        ReadOnlyCollection<TOid> GetObjectIdsInView(BoundingBox boundingBox);

		/// <summary>
		/// Returns the geometry corresponding to the Object ID
		/// </summary>
        /// <param name="oid">Object ID.</param>
        /// <returns>The geometry corresponding to the <paramref name="oid"/>.</returns>
        Geometry GetGeometryById(TOid oid);

		/// <summary>
		/// Returns a <see cref="SharpMap.Data.FeatureDataRow"/> based on an OID.
		/// </summary>
        /// <param name="oid">The object id (OID) of the feature.</param>
		/// <returns>The feature corresponding to the <paramref name="oid"/>.</returns>
		FeatureDataRow<TOid> GetFeature(TOid oid);
	}
}