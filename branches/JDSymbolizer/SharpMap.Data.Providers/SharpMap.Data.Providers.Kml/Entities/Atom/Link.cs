// /*
//  *  The attached / following is part of SharpMap.Data.Providers.Kml
//  *  SharpMap.Data.Providers.Kml is free software � 2008 Newgrove Consultants Limited, 
//  *  www.newgrove.com; you can redistribute it and/or modify it under the terms 
//  *  of the current GNU Lesser General Public License (LGPL) as published by and 
//  *  available from the Free Software Foundation, Inc., 
//  *  59 Temple Place, Suite 330, Boston, MA 02111-1307 USA: http://fsf.org/    
//  *  This program is distributed without any warranty; 
//  *  without even the implied warranty of merchantability or fitness for purpose.  
//  *  See the GNU Lesser General Public License for the full details. 
//  *  
//  *  Author: John Diss 2009
//  * 
//  */
#pragma warning disable 1591

// Copyright 2008, Microsoft Corporation
// Sample Code - Use restricted to terms of use defined in the accompanying license agreement (EULA.doc)

//--------------------------------------------------------------
// Autogenerated by XSDObjectGen version 2.0.0.0
// Schema file: ogckml22.xsd
// Creation Date: 14/05/2009 14:07:49
//--------------------------------------------------------------

using System;
using System.Xml.Serialization;

namespace SharpMap.Entities.Atom
{
    [XmlRoot(ElementName = "link", Namespace = Declarations.SchemaVersion, IsNullable = false), Serializable]
    public class Link
    {
        [XmlIgnore] private string _href;
        [XmlIgnore] private string _hreflang;
        [XmlIgnore] private string _length;

        [XmlIgnore] private string _rel;
        [XmlIgnore] private string _title;

        [XmlIgnore] private string _type;

        public Link()
        {
            Href = string.Empty;
        }

        [XmlAttribute(AttributeName = "href")]
        public string Href
        {
            get { return _href; }
            set { _href = value; }
        }

        [XmlAttribute(AttributeName = "rel")]
        public string Rel
        {
            get { return _rel; }
            set { _rel = value; }
        }

        [XmlAttribute(AttributeName = "type", DataType = "string")]
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        [XmlAttribute(AttributeName = "hreflang", DataType = "string")]
        public string HrefLang
        {
            get { return _hreflang; }
            set { _hreflang = value; }
        }

        [XmlAttribute(AttributeName = "title")]
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        [XmlAttribute(AttributeName = "length")]
        public string Length
        {
            get { return _length; }
            set { _length = value; }
        }

        public void MakeSchemaCompliant()
        {
        }
    }
}

#pragma warning restore 1591