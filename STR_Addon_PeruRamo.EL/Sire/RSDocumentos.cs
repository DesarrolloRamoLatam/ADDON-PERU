using STR_Addon_PeruRamo.EL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STR_Addon_PeruRamo.EL
{
    public class RSDocumentos
    {
        public Paginacion paginacion { get; set; }
        public List<Registros> registros { get; set; }
        public Totales totales { get; set; }
        public Totales montosTotales { get; set; }
        public Registro registro { get; set; }
        public List<Registros> comprobantes { get; set; }
        public List<Registros> comprobantesLibrosCompras { get; set; }
    }
}
