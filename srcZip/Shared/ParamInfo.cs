using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace srcZip
{
    public class ParamInfo
    {
        public readonly ParamKind Kind;

        public readonly List <string> Values = new List <string> ();

        public ParamInfo (ParamKind kind)
        {
            Kind = kind;
        }
    }
}
