// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
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

// SOURCECODE IS MODIFIED FROM ANOTHER WORK AND IS ORIGINALLY BASED ON GeoTools.NET:
/*
 *  Copyright (C) 2002 Urban Science Applications, Inc. 
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU Lesser General Public
 *  License as published by the Free Software Foundation; either
 *  version 2.1 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public
 *  License along with this library; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using GeoAPI.CoordinateSystems;
using NPack.Interfaces;
using ProjNet.CoordinateSystems;

namespace ProjNet.IO.CoordinateSystems
{
    /// <summary>
    /// Creates an object based on the supplied Well Known Text (Wkt).
    /// </summary>
    public class CoordinateSystemWktReader<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>, IComputable<TCoordinate>,
            IConvertible
    {
        /// <summary>
        /// Reads and parses a Wkt-formatted projection string.
        /// </summary>
        /// <param name="wkt">String containing Wkt.</param>
        /// <returns>Object representation of the Wkt.</returns>
        /// <exception cref="System.ArgumentException">If a token is not recognised.</exception>
        public static IInfo Parse(string wkt)
        {
            IInfo returnObject;
            StringReader reader = new StringReader(wkt);
            WktStreamTokenizer tokenizer = new WktStreamTokenizer(reader);
            tokenizer.NextToken();

            string objectName = tokenizer.GetStringValue();

            switch (objectName)
            {
                case "UNIT":
                    returnObject = readUnit(tokenizer);
                    break;
                    //case "VERT_DATUM":
                    //    IVerticalDatum verticalDatum = ReadVerticalDatum(tokenizer);
                    //    returnObject = verticalDatum;
                    //    break;
                case "SPHEROID":
                    returnObject = readEllipsoid(tokenizer);
                    break;
                case "DATUM":
                    returnObject = readHorizontalDatum(tokenizer);
                    ;
                    break;
                case "PRIMEM":
                    returnObject = readPrimeMeridian(tokenizer);
                    break;
                case "VERT_CS":
                case "GEOGCS":
                case "PROJCS":
                case "COMPD_CS":
                case "GEOCCS":
                case "FITTED_CS":
                case "LOCAL_CS":
                    returnObject = readCoordinateSystem(wkt, tokenizer);
                    break;
                default:
                    throw new ArgumentException(String.Format("'{0}' is not recognized.", objectName));
            }

            reader.Close();
            return returnObject;
        }

        /// <summary>
        /// Returns a IUnit given a Well-Known Text representation.
        /// </summary>
        /// <param name="tokenizer">WktStreamTokenizer that has the Wkt.</param>
        /// <returns>An object that implements the IUnit interface.</returns>
        private static IUnit readUnit(WktStreamTokenizer tokenizer)
        {
            tokenizer.ReadToken("[");
            string unitName = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.NextToken();
            double unitsPerUnit = tokenizer.GetNumericValue();
            string authority = String.Empty;
            long authorityCode = -1;
            tokenizer.NextToken();

            if (tokenizer.GetStringValue() == ",")
            {
                tokenizer.ReadAuthority(ref authority, ref authorityCode);
                tokenizer.ReadToken("]");
            }

            return new Unit(unitsPerUnit, unitName, authority, authorityCode, String.Empty, String.Empty, String.Empty);
        }

        /// <summary>
        /// Returns a <see cref="LinearUnit"/> given a Well-Known Text representation.
        /// </summary>
        /// <param name="tokenizer">WktStreamTokenizer that has the Wkt.</param>
        /// <returns>An object that implements the IUnit interface.</returns>
        private static ILinearUnit readLinearUnit(WktStreamTokenizer tokenizer)
        {
            tokenizer.ReadToken("[");
            string unitName = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.NextToken();
            double unitsPerUnit = tokenizer.GetNumericValue();
            string authority = String.Empty;
            long authorityCode = -1;
            tokenizer.NextToken();

            if (tokenizer.GetStringValue() == ",")
            {
                tokenizer.ReadAuthority(ref authority, ref authorityCode);
                tokenizer.ReadToken("]");
            }

            return
                new LinearUnit(unitsPerUnit, unitName, authority, authorityCode, String.Empty, String.Empty,
                               String.Empty);
        }

        /// <summary>
        /// Returns a <see cref="AngularUnit"/> given a Well-Known Text representation.
        /// </summary>
        /// <param name="tokenizer">WktStreamTokenizer that has the Wkt.</param>
        /// <returns>An object that implements the IUnit interface.</returns>
        private static IAngularUnit readAngularUnit(WktStreamTokenizer tokenizer)
        {
            tokenizer.ReadToken("[");
            string unitName = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.NextToken();
            double unitsPerUnit = tokenizer.GetNumericValue();
            string authority = String.Empty;
            long authorityCode = -1;
            tokenizer.NextToken();

            if (tokenizer.GetStringValue() == ",")
            {
                tokenizer.ReadAuthority(ref authority, ref authorityCode);
                tokenizer.ReadToken("]");
            }

            return
                new AngularUnit(unitsPerUnit, unitName, authority, authorityCode, String.Empty, String.Empty,
                                String.Empty);
        }

        /// <summary>
        /// Returns a <see cref="AxisInfo"/> given a Well-Known Text representation.
        /// </summary>
        /// <param name="tokenizer">WktStreamTokenizer that has the Wkt.</param>
        /// <returns>An AxisInfo object.</returns>
        private static AxisInfo readAxis(WktStreamTokenizer tokenizer)
        {
            if (tokenizer.GetStringValue() != "AXIS")
            {
                tokenizer.ReadToken("AXIS");
            }

            tokenizer.ReadToken("[");
            string axisName = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.NextToken();
            string unitname = tokenizer.GetStringValue();
            tokenizer.ReadToken("]");

            switch (unitname.ToUpper(CultureInfo.InvariantCulture))
            {
                case "DOWN":
                    return new AxisInfo(axisName, AxisOrientation.Down);
                case "EAST":
                    return new AxisInfo(axisName, AxisOrientation.East);
                case "NORTH":
                    return new AxisInfo(axisName, AxisOrientation.North);
                case "OTHER":
                    return new AxisInfo(axisName, AxisOrientation.Other);
                case "SOUTH":
                    return new AxisInfo(axisName, AxisOrientation.South);
                case "UP":
                    return new AxisInfo(axisName, AxisOrientation.Up);
                case "WEST":
                    return new AxisInfo(axisName, AxisOrientation.West);
                default:
                    throw new ArgumentException("Invalid axis name '" + unitname + "' in Wkt");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordinateSystem"></param>
        /// <param name="tokenizer"></param>
        /// <returns></returns>
        private static ICoordinateSystem<TCoordinate> readCoordinateSystem(string coordinateSystem,
                                                                           WktStreamTokenizer tokenizer)
        {
            switch (tokenizer.GetStringValue())
            {
                case "GEOGCS":
                    return readGeographicCoordinateSystem(tokenizer);
                case "PROJCS":
                    return readProjectedCoordinateSystem(tokenizer);
                case "COMPD_CS":
                    /*	ICompoundCoordinateSystem compoundCS = ReadCompoundCoordinateSystem(tokenizer);
                    returnCS = compoundCS;
                    break;*/
                case "VERT_CS":
                    /*	IVerticalCoordinateSystem verticalCS = ReadVerticalCoordinateSystem(tokenizer);
                    returnCS = verticalCS;
                    break;*/
                case "GEOCCS":
                case "FITTED_CS":
                case "LOCAL_CS":
                    throw new NotSupportedException(String.Format(
                                                        "{0} coordinate system is not supported.", coordinateSystem));
                default:
                    throw new InvalidOperationException(String.Format(
                                                            "{0} coordinate system is not recognized.", coordinateSystem));
            }
        }

        /// <summary>
        /// Reads either 3, 6 or 7 parameter Bursa-Wolf values from TOWGS84 token
        /// </summary>
        /// <param name="tokenizer"></param>
        /// <returns></returns>
        private static Wgs84ConversionInfo readWgs84ConversionInfo(WktStreamTokenizer tokenizer)
        {
            tokenizer.ReadToken("[");

            tokenizer.NextToken();
            Double dx = tokenizer.GetNumericValue();
            tokenizer.ReadToken(",");

            tokenizer.NextToken();
            Double dy = tokenizer.GetNumericValue();
            tokenizer.ReadToken(",");

            tokenizer.NextToken();
            Double dz = tokenizer.GetNumericValue();
            tokenizer.NextToken();

            Double ex = 0, ey = 0, ez = 0, ppm = 0;

            if (tokenizer.GetStringValue() == ",")
            {
                tokenizer.NextToken();
                ex = tokenizer.GetNumericValue();

                tokenizer.ReadToken(",");
                tokenizer.NextToken();
                ey = tokenizer.GetNumericValue();

                tokenizer.ReadToken(",");
                tokenizer.NextToken();
                ez = tokenizer.GetNumericValue();

                tokenizer.NextToken();

                if (tokenizer.GetStringValue() == ",")
                {
                    tokenizer.NextToken();
                    ppm = tokenizer.GetNumericValue();
                }
            }

            if (tokenizer.GetStringValue() != "]")
            {
                tokenizer.ReadToken("]");
            }

            return new Wgs84ConversionInfo(dx, dy, dz, ex, ey, ez, ppm);
        }

        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenizer"></param>
        /// <returns></returns>
        private static ICompoundCoordinateSystem ReadCompoundCoordinateSystem(WktStreamTokenizer tokenizer)
        {
			
            //COMPD_CS[
            //"OSGB36 / British National Grid + ODN",
            //PROJCS[]
            //VERT_CS[]
            //AUTHORITY["EPSG","7405"]
            //]

            tokenizer.ReadToken("[");
            string name=tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.NextToken();
            string headCSCode =  tokenizer.GetStringValue();
            ICoordinateSystem headCS = ReadCoordinateSystem(headCSCode,tokenizer);
            tokenizer.ReadToken(",");
            tokenizer.NextToken();
            string tailCSCode =  tokenizer.GetStringValue();
            ICoordinateSystem tailCS = ReadCoordinateSystem(tailCSCode,tokenizer);
            tokenizer.ReadToken(",");
            string authority=String.Empty;
            string authorityCode=String.Empty; 
            tokenizer.ReadAuthority(ref authority, ref authorityCode);
            tokenizer.ReadToken("]");
            ICompoundCoordinateSystem compoundCS = new CompoundCoordinateSystem(headCS,tailCS,String.Empty,authority,authorityCode,name,String.Empty,String.Empty); 
            return compoundCS;
			
        }*/

        private static IEllipsoid readEllipsoid(WktStreamTokenizer tokenizer)
        {
            //SPHEROID["Airy 1830",6377563.396,299.3249646,AUTHORITY["EPSG","7001"]]
            tokenizer.ReadToken("[");
            string name = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.NextToken();
            double majorAxis = tokenizer.GetNumericValue();
            tokenizer.ReadToken(",");
            tokenizer.NextToken();
            double e = tokenizer.GetNumericValue();
            //

            //tokenizer.ReadToken(",");
            tokenizer.NextToken();
            string authority = String.Empty;
            long authorityCode = -1;

            if (tokenizer.GetStringValue() == ",") //Read authority
            {
                tokenizer.ReadAuthority(ref authority, ref authorityCode);
                tokenizer.ReadToken("]");
            }

            IEllipsoid ellipsoid =
                new Ellipsoid(majorAxis, 0.0, e, true, LinearUnit.Metre, name, authority, authorityCode, String.Empty,
                              string.Empty, string.Empty);
            return ellipsoid;
        }

        private static IProjection readProjection(WktStreamTokenizer tokenizer)
        {
            //tokenizer.NextToken();// PROJECTION
            tokenizer.ReadToken("PROJECTION");
            tokenizer.ReadToken("["); //[
            string projectionName = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken("]"); //]
            tokenizer.ReadToken(","); //,
            tokenizer.ReadToken("PARAMETER");
            List<ProjectionParameter> paramList = new List<ProjectionParameter>();

            while (tokenizer.GetStringValue() == "PARAMETER")
            {
                tokenizer.ReadToken("[");
                string paramName = tokenizer.ReadDoubleQuotedWord();
                tokenizer.ReadToken(",");
                tokenizer.NextToken();
                double paramValue = tokenizer.GetNumericValue();
                tokenizer.ReadToken("]");
                tokenizer.ReadToken(",");
                paramList.Add(new ProjectionParameter(paramName, paramValue));
                tokenizer.NextToken();
            }

            string authority = String.Empty;
            long authorityCode = -1;
            IProjection projection =
                new Projection(projectionName, paramList, projectionName, authority, authorityCode, String.Empty,
                               String.Empty, string.Empty);
            return projection;
        }

        private static IProjectedCoordinateSystem<TCoordinate> readProjectedCoordinateSystem(
            WktStreamTokenizer tokenizer)
        {
            /*PROJCS[
                "OSGB 1936 / British National Grid",
                GEOGCS[
                    "OSGB 1936",
                    DATUM[...]
                    PRIMEM[...]
                    AXIS["Geodetic latitude","NORTH"]
                    AXIS["Geodetic longitude","EAST"]
                    AUTHORITY["EPSG","4277"]
                ],
                PROJECTION["Transverse Mercator"],
                PARAMETER["latitude_of_natural_origin",49],
                PARAMETER["longitude_of_natural_origin",-2],
                PARAMETER["scale_factor_at_natural_origin",0.999601272],
                PARAMETER["false_easting",400000],
                PARAMETER["false_northing",-100000],
                AXIS["Easting","EAST"],
                AXIS["Northing","NORTH"],
                AUTHORITY["EPSG","27700"]
            ]
            */

            tokenizer.ReadToken("[");
            string name = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.ReadToken("GEOGCS");
            IGeographicCoordinateSystem<TCoordinate> geographicCS = readGeographicCoordinateSystem(tokenizer);
            tokenizer.ReadToken(",");
            IProjection projection = readProjection(tokenizer);
            IUnit unit = readLinearUnit(tokenizer);

            string authority = String.Empty;
            long authorityCode = -1;
            tokenizer.NextToken();
            List<AxisInfo> axes = new List<AxisInfo>(2);

            if (tokenizer.GetStringValue() == ",")
            {
                tokenizer.NextToken();
                while (tokenizer.GetStringValue() == "AXIS")
                {
                    axes.Add(readAxis(tokenizer));
                    tokenizer.NextToken();
                    if (tokenizer.GetStringValue() == ",")
                    {
                        tokenizer.NextToken();
                    }
                }
                if (tokenizer.GetStringValue() == ",")
                {
                    tokenizer.NextToken();
                }
                if (tokenizer.GetStringValue() == "AUTHORITY")
                {
                    tokenizer.ReadAuthority(ref authority, ref authorityCode);
                    tokenizer.ReadToken("]");
                }
            }

            //This is default axis values if not specified.
            if (axes.Count == 0)
            {
                axes.Add(new AxisInfo("X", AxisOrientation.East));
                axes.Add(new AxisInfo("Y", AxisOrientation.North));
            }

            IProjectedCoordinateSystem<TCoordinate> projectedCS =
                new ProjectedCoordinateSystem<TCoordinate>(geographicCS.HorizontalDatum, geographicCS,
                                                           unit as LinearUnit, projection, axes, name, authority,
                                                           authorityCode, String.Empty, String.Empty, String.Empty);
            return projectedCS;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenizer"></param>
        /// <returns></returns>
        private static IGeographicCoordinateSystem<TCoordinate> readGeographicCoordinateSystem(
            WktStreamTokenizer tokenizer)
        {
            /*
            GEOGCS["OSGB 1936",
            DATUM["OSGB 1936",SPHEROID["Airy 1830",6377563.396,299.3249646,AUTHORITY["EPSG","7001"]]TOWGS84[0,0,0,0,0,0,0],AUTHORITY["EPSG","6277"]]
            PRIMEM["Greenwich",0,AUTHORITY["EPSG","8901"]]
            AXIS["Geodetic latitude","NORTH"]
            AXIS["Geodetic longitude","EAST"]
            AUTHORITY["EPSG","4277"]
            ]
            */
            tokenizer.ReadToken("[");
            string name = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.ReadToken("DATUM");
            IHorizontalDatum horizontalDatum = readHorizontalDatum(tokenizer);
            tokenizer.ReadToken(",");
            tokenizer.ReadToken("PRIMEM");
            IPrimeMeridian primeMeridian = readPrimeMeridian(tokenizer);
            tokenizer.ReadToken(",");
            tokenizer.ReadToken("UNIT");
            IAngularUnit angularUnit = readAngularUnit(tokenizer);

            string authority = String.Empty;
            long authorityCode = -1;
            tokenizer.NextToken();
            List<AxisInfo> info = new List<AxisInfo>(2);

            if (tokenizer.GetStringValue() == ",")
            {
                tokenizer.NextToken();
                while (tokenizer.GetStringValue() == "AXIS")
                {
                    info.Add(readAxis(tokenizer));
                    tokenizer.NextToken();
                    if (tokenizer.GetStringValue() == ",")
                    {
                        tokenizer.NextToken();
                    }
                }
                if (tokenizer.GetStringValue() == ",")
                {
                    tokenizer.NextToken();
                }
                if (tokenizer.GetStringValue() == "AUTHORITY")
                {
                    tokenizer.ReadAuthority(ref authority, ref authorityCode);
                    tokenizer.ReadToken("]");
                }
            }

            //This is default axis values if not specified.
            if (info.Count == 0)
            {
                info.Add(new AxisInfo("Lon", AxisOrientation.East));
                info.Add(new AxisInfo("Lat", AxisOrientation.North));
            }

            IGeographicCoordinateSystem<TCoordinate> geographicCS = new GeographicCoordinateSystem<TCoordinate>(
                angularUnit, horizontalDatum, primeMeridian, info, name, authority, authorityCode,
                String.Empty, String.Empty, String.Empty);

            return geographicCS;
        }

        private static IHorizontalDatum readHorizontalDatum(WktStreamTokenizer tokenizer)
        {
            //DATUM["OSGB 1936",SPHEROID["Airy 1830",6377563.396,299.3249646,AUTHORITY["EPSG","7001"]]TOWGS84[0,0,0,0,0,0,0],AUTHORITY["EPSG","6277"]]
            Wgs84ConversionInfo wgsInfo = null;
            string authority = String.Empty;
            long authorityCode = -1;

            tokenizer.ReadToken("[");
            string name = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.ReadToken("SPHEROID");
            IEllipsoid ellipsoid = readEllipsoid(tokenizer);
            tokenizer.NextToken();

            while (tokenizer.GetStringValue() == ",")
            {
                tokenizer.NextToken();
                if (tokenizer.GetStringValue() == "TOWGS84")
                {
                    wgsInfo = readWgs84ConversionInfo(tokenizer);
                    tokenizer.NextToken();
                }
                else if (tokenizer.GetStringValue() == "AUTHORITY")
                {
                    tokenizer.ReadAuthority(ref authority, ref authorityCode);
                    tokenizer.ReadToken("]");
                }
            }

            // make an assumption about the datum type.
            IHorizontalDatum horizontalDatum = new HorizontalDatum(ellipsoid, wgsInfo,
                                                                   DatumType.HorizontalGeocentric, name, authority,
                                                                   authorityCode, String.Empty, String.Empty,
                                                                   String.Empty);

            return horizontalDatum;
        }

        private static IPrimeMeridian readPrimeMeridian(WktStreamTokenizer tokenizer)
        {
            //PRIMEM["Greenwich",0,AUTHORITY["EPSG","8901"]]
            tokenizer.ReadToken("[");
            string name = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.NextToken();
            double longitude = tokenizer.GetNumericValue();

            tokenizer.NextToken();
            string authority = String.Empty;
            long authorityCode = -1;

            if (tokenizer.GetStringValue() == ",")
            {
                tokenizer.ReadAuthority(ref authority, ref authorityCode);
                tokenizer.ReadToken("]");
            }

            // make an assumption about the Angular units - degrees.
            IPrimeMeridian primeMeridian =
                new PrimeMeridian(longitude, AngularUnit.Degrees, name, authority, authorityCode, String.Empty,
                                  String.Empty, String.Empty);

            return primeMeridian;
        }

        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenizer"></param>
        /// <returns></returns>
        private static IVerticalCoordinateSystem ReadVerticalCoordinateSystem(WktStreamTokenizer tokenizer)
        {
            //VERT_CS["Newlyn",
            //VERT_DATUM["Ordnance Datum Newlyn",2005,AUTHORITY["EPSG","5101"]]
            //UNIT["metre",1,AUTHORITY["EPSG","9001"]]
            //AUTHORITY["EPSG","5701"]
			
            tokenizer.ReadToken("[");
            string name=tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.ReadToken("VERT_DATUM");
            IVerticalDatum verticalDatum = ReadVerticalDatum(tokenizer);
            tokenizer.ReadToken("UNIT");
            IUnit unit = ReadUnit(tokenizer);
            string authority=String.Empty;
            string authorityCode=String.Empty; 
            tokenizer.ReadAuthority(ref authority, ref authorityCode);
            tokenizer.ReadToken("]");

            IVerticalCoordinateSystem verticalCS = new VerticalCoordinateSystem(name,verticalDatum,String.Empty,authority,authorityCode,String.Empty,String.Empty);
            return verticalCS;
        }*/

        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenizer"></param>
        /// <returns></returns>
        private static IVerticalDatum  ReadVerticalDatum(WktStreamTokenizer tokenizer)
        {
            //VERT_DATUM["Ordnance Datum Newlyn",2005,AUTHORITY["5101","EPSG"]]
            tokenizer.ReadToken("[");
            string datumName=tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.NextToken();
            string datumTypeNumber = tokenizer.GetStringValue();
            tokenizer.ReadToken(",");
            string authority=String.Empty;
            string authorityCode=String.Empty; 
            tokenizer.ReadAuthority(ref authority, ref authorityCode);
            DatumType datumType = (DatumType)Enum.Parse(typeof(DatumType),datumTypeNumber);
            IVerticalDatum verticalDatum = new VerticalDatum(datumType,String.Empty,authorityCode,authority,datumName,String.Empty,String.Empty);
            tokenizer.ReadToken("]");
            return verticalDatum;
        }*/


        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenizer"></param>
        /// <returns></returns>
        [Obsolete("Since the related objects have not been implemented")]
        private static IGeocentricCoordinateSystem ReadGeocentricCoordinateSystem(WktStreamTokenizer tokenizer)
        {
            throw new NotImplementedException("IGeocentricCoordinateSystem is not implemented");
        }*/
    }
}