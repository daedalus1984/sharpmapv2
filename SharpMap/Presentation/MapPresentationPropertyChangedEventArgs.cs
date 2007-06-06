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
using System.Text;

namespace SharpMap.Presentation
{
    [Serializable]
    public class MapPresentationPropertyChangedEventArgs<TParam> : EventArgs
    {
        private TParam _previousValue;
        private TParam _currentValue;

        public MapPresentationPropertyChangedEventArgs(TParam previousValue, TParam currentValue)
        {
            _previousValue = previousValue;
            _currentValue = currentValue;
        }

        public TParam PreviousValue
        {
            get { return _previousValue; }
        }

        public TParam CurrentValue
        {
            get { return _currentValue; }
        }
    }

    [Serializable]
    public class MapPresentationPropertyChangedEventArgs<TViewValue, TGeoValue> : EventArgs
    {
        private TViewValue _previousViewValue;
        private TViewValue _currentViewValue;
        private TGeoValue _previousGeoValue;
        private TGeoValue _currentGeoValue;

        public MapPresentationPropertyChangedEventArgs(TGeoValue previousGeoValue, TGeoValue currentGeoValue, TViewValue previousViewValue, TViewValue currentViewValue)
        {
            _previousViewValue = previousViewValue;
            _currentViewValue = currentViewValue;

            _previousGeoValue = previousGeoValue;
            _currentGeoValue = currentGeoValue;
        }

        public TViewValue PreviousViewValue
        {
            get { return _previousViewValue; }
        }

        public TViewValue CurrentViewValue
        {
            get { return _currentViewValue; }
        }

        public TGeoValue PreviousGeoValue
        {
            get { return _previousGeoValue; }
        }

        public TGeoValue CurrentGeoValue
        {
            get { return _currentGeoValue; }
        }
    }
}
