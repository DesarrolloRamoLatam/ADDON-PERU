using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STR_Addon_PeruRamo.EL
{
    public class Registro
    {
        public string showReporteDescarga { get; set; }
        public string perTributario { get; set; }
        public string numTicket { get; set; }
        public object fecCargaImportacion { get; set; }
        public string fecInicioProceso { get; set; }
        public string codProceso { get; set; }
        public string desProceso { get; set; }
        public string codEstadoProceso { get; set; }
        public string desEstadoProceso { get; set; }
        public string nomArchivoImportacion { get; set; }
        public string numRuc { get; set; }
        public string nomRazonSocial { get; set; }
        public int cntComprobantes { get; set; }
        public DetalleTicket detalleTicket { get; set; }
        public List<ArchivoReporte> archivoReporte { get; set; }
    }
}
