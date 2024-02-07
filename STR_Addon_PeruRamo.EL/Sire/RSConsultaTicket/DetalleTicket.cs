using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STR_Addon_PeruRamo.EL
{
    public class DetalleTicket
    {
        public string numTicket { get; set; }
        public string fecCargaImportacion { get; set; }
        public string horaCargaImportacion { get; set; }
        public string codEstadoEnvio { get; set; }
        public string desEstadoEnvio { get; set; }
        public object nomArchivoReporte { get; set; }
        public int cntFilasvalidada { get; set; }
        public int cntCPError { get; set; }
        public int cntCPInformados { get; set; }
    }
}
