// /*
//  *  The attached / following is part of SharpMap.Data.Providers.Gml
//  *  SharpMap.Data.Providers.Gml is free software � 2008 Newgrove Consultants Limited, 
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
using System;
using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;
using SharpMap.Entities.Iso.Gco;

namespace SharpMap.Entities.Iso.Gmd
{
    [Serializable, XmlType(TypeName = "DQ_quantitativeResult_Type", Namespace = "http://www.isotc211.org/2005/gmd")]
    public class DQ_quantitativeResult_Type : AbstractDQ_result_Type
    {
        [XmlIgnore] private CharacterStringPropertyType _errorStatistic;
        [XmlIgnore] private List<RecordPropertyType> _value;
        [XmlIgnore] private RecordTypePropertyType _valueType;
        [XmlIgnore] private UnitOfMeasurePropertyType _valueUnit;

        [XmlElement(Type = typeof (CharacterStringPropertyType), ElementName = "errorStatistic", IsNullable = false,
            Form = XmlSchemaForm.Qualified, Namespace = "http://www.isotc211.org/2005/gmd")]
        public CharacterStringPropertyType ErrorStatistic
        {
            get { return _errorStatistic; }
            set { _errorStatistic = value; }
        }

        [XmlElement(Type = typeof (RecordPropertyType), ElementName = "value", IsNullable = false,
            Form = XmlSchemaForm.Qualified, Namespace = "http://www.isotc211.org/2005/gmd")]
        public List<RecordPropertyType> Value
        {
            get
            {
                if (_value == null)
                {
                    _value = new List<RecordPropertyType>();
                }
                return _value;
            }
            set { _value = value; }
        }

        [XmlElement(Type = typeof (RecordTypePropertyType), ElementName = "valueType", IsNullable = false,
            Form = XmlSchemaForm.Qualified, Namespace = "http://www.isotc211.org/2005/gmd")]
        public RecordTypePropertyType ValueType
        {
            get { return _valueType; }
            set { _valueType = value; }
        }

        [XmlElement(Type = typeof (UnitOfMeasurePropertyType), ElementName = "valueUnit", IsNullable = false,
            Form = XmlSchemaForm.Qualified, Namespace = "http://www.isotc211.org/2005/gmd")]
        public UnitOfMeasurePropertyType ValueUnit
        {
            get { return _valueUnit; }
            set { _valueUnit = value; }
        }

        public override void MakeSchemaCompliant()
        {
            base.MakeSchemaCompliant();
            ValueUnit.MakeSchemaCompliant();
            foreach (RecordPropertyType _c in Value)
            {
                _c.MakeSchemaCompliant();
            }
        }
    }
}