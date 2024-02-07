using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STR_Addon_PeruRamo.EL
{
    public class Operacion
    {
        public string codAnotacion { get; set; }
        public string desAnotacion { get; set; }
        public string numOperacion { get; set; }
        public DateTime fecTransaccion { get; set; }
        public string nomArchivoRvieRce { get; set; }
        public string nomArchivoInconsistencias { get; set; }
        public string nomArchivoExportadores { get; set; }
        public string nomArchivoCasillas { get; set; }
        public string nomArchivoConstanciaXML { get; set; }
        public string nomArchivoConstanciaPDF { get; set; }
        public string desHashArchivoRvieRce { get; set; }
        public string desHashArchivoInconsistencias { get; set; }
        public string desHashArchivoExportadores { get; set; }
    }
}
