using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STR_Addon_PeruRamo.EL.Sire
{
    public class RSerror
    {
        public int cod { get; set; }
        public string msg { get; set; }

        public List<Error> errors { get; set; }
    }


    public class Error
    {

        public int cod { get; set; }
        public string msg { get; set; }
    }
}
