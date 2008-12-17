﻿/*
 *	This file is part of SharpMap.Demo.FormatConverter
 *  SharpMap.Demo.FormatConverter is free software © 2008 Newgrove Consultants Limited, 
 *  http://www.newgrove.com; you can redistribute it and/or modify it under the terms 
 *  of the current GNU Lesser General Public License (LGPL) as published by and 
 *  available from the Free Software Foundation, Inc., 
 *  59 Temple Place, Suite 330, Boston, MA 02111-1307 USA: http://fsf.org/    
 *  This program is distributed without any warranty; 
 *  without even the implied warranty of merchantability or fitness for purpose.  
 *  See the GNU Lesser General Public License for the full details. 
 *  
 *  Author: John Diss 2008
 * 
 */
using System;
using System.Data;
using System.Reflection;
using GeoAPI.CoordinateSystems;
using GeoAPI.Geometries;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Data.Providers.Db;
using SharpMap.Demo.FormatConverter.Common;
using SharpMap.Expressions;
using SharpMap.Utilities;

namespace SharpMap.Demo.FormatConverter.SqlServer2008
{
    [ConfigureProvider(typeof(MsSqlServer2008Provider<>), "Sql Server 2008")]
    public class ConfigureSqlServer2008Provider : IConfigureFeatureSource, IConfigureFeatureTarget
    {
        private string _oidColumn;
        private IFeatureProvider _sourceProvider;
        private IWritableFeatureProvider _targetProvider;
        private bool disposed;

        #region IConfigureFeatureSource Members

        public IFeatureProvider ConstructSourceProvider(IGeometryServices geometryServices)
        {
            Console.WriteLine("Please enter the connection string for the source server.");
            string connectionString = Console.ReadLine();
            Console.WriteLine("Please enter the data tables' schema");
            string dtschema = Console.ReadLine();
            Console.WriteLine("Please enter the table name.");
            string tableName = Console.ReadLine();
            Console.WriteLine("Please enter the id column name.");
            _oidColumn = Console.ReadLine();
            Console.WriteLine("Please enter the geometry column name.");
            string geometryColumn = Console.ReadLine();
            Console.WriteLine("Please enter the SRID (e.g EPSG:4326)");
            string srid = Console.ReadLine();

            Type type;
            var dbUtility = new SqlServerDbUtility();
            using (IDbConnection conn = dbUtility.CreateConnection(connectionString))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = string.Format("SELECT TOP 1 [{0}] FROM [{1}].[{2}] ", _oidColumn, dtschema,
                                                    tableName);
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    type = cmd.ExecuteScalar().GetType();
                }
            }

            Type t = typeof(MsSqlServer2008Provider<>);
            Type specialized = t.MakeGenericType(type);

            _sourceProvider =
                (IFeatureProvider)
                Activator.CreateInstance(specialized, geometryServices[srid], connectionString, dtschema, tableName,
                                         _oidColumn, geometryColumn);
            _sourceProvider.Open();

            return _sourceProvider;
        }

        public FeatureQueryExpression ConstructSourceQueryExpression()
        {
            return new FeatureQueryExpression(new AllAttributesExpression(), null);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string OidColumnName
        {
            get { return _oidColumn; }
        }

        #endregion

        #region IConfigureFeatureTarget Members

        public IWritableFeatureProvider ConstructTargetProvider(Type oidType, IGeometryFactory geometryFactory,
                                                                ICoordinateSystemFactory csFactory,
                                                                FeatureDataTable schemaTable)
        {
            Type typ = typeof(MsSqlServer2008Provider<>);
            Type specialized = typ.MakeGenericType(oidType);

            Console.WriteLine("Please enter the connection string for the target database server.");
            string connectionString = Console.ReadLine();

            Console.WriteLine("Please enter the schema for the table.");
            string schemaName = Console.ReadLine();

            Console.WriteLine("Please enter the table name.");
            string tableName = Console.ReadLine();

            _targetProvider = (IWritableFeatureProvider)specialized.GetMethod(
                "Create",
                BindingFlags.Public | BindingFlags.Static,
                null,
                CallingConventions.Standard,
                new[] { typeof(string), typeof(IGeometryFactory), typeof(string), typeof(string), typeof(FeatureDataTable) }, null)
                .Invoke(null, new object[] { connectionString, geometryFactory, schemaName, tableName, schemaTable });

            _targetProvider.Open();
            return _targetProvider;
        }

        #endregion



        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (_sourceProvider != null)
                    _sourceProvider.Close();

                if (_targetProvider != null)
                    _targetProvider.Close();

                disposed = true;
            }
        }

        ~ConfigureSqlServer2008Provider()
        {
            Dispose(false);
        }

        
    }
}