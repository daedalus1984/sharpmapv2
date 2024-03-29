﻿/*
 *	This file is part of SharpMap.Rendering.GeoJson
 *  SharpMap.Rendering.GeoJson is free software © 2008 Newgrove Consultants Limited, 
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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GeoAPI.DataStructures;
#if DOTNET35
using Enumerable = System.Linq.Enumerable;
using Caster = System.Linq.Enumerable;
#else

#endif

namespace SharpMap.Rendering.GeoJson
{
    public static class JsonUtility
    {
        private static readonly Type[] FloatingPointTypes = new[] {typeof (float), typeof (double), typeof (decimal)};

        private static readonly Type[] IntegralTypes = new[]
                                                           {
                                                               typeof (UInt16), typeof (UInt32), typeof (UInt64),
                                                               typeof (Int16), typeof (Int32), typeof (Int64)
                                                           };

        public static string FormatJsonAttribute(string name, object value)
        {
            return string.Format("\"{0}\":{1}", name, FormatJsonValue(value));
        }

        private static string FormatJsonValue(object v)
        {
            if (v == null || v is DBNull)
                return "null";

            Type tval = v.GetType();

            if (tval != typeof (string) && typeof (IEnumerable).IsAssignableFrom(tval))
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("[");

                IList<string> store = new List<string>();

                foreach (object o in (IEnumerable) v)
                    store.Add(FormatJsonValue(o));

                for (int i = 0; i < store.Count; i++)
                {
                    sb.Append(store[i]);
                    if (i < store.Count - 2)
                        sb.Append(",");
                }


                sb.Append("]");
                return sb.ToString();
            }

            if (tval.IsPrimitive)
            {
                if (tval == typeof (bool))
                    return v.ToString().ToLower();

                if (Enumerable.Contains(IntegralTypes, tval))
                    return v.ToString();

                if (Enumerable.Contains(FloatingPointTypes, tval))
                    return string.Format("{0:E}", v);
            }


            if (tval == typeof (string) || tval == typeof (Char))
                return string.Format("\"{0}\"", HttpUtility.HtmlEncode((string) v));


            ///todo : more complex classes

            throw new FormatException();
        }
    }
}