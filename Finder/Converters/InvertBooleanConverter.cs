using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Finder.Converters
{
    class InvertBooleanConverter : BooleanConverter<bool>
    {
        public InvertBooleanConverter() : base(false, true)
        {
            
        }
    }
}
