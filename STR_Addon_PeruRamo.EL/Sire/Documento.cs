using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STR_Addon_PeruRamo.EL
{
    public class Documento
    {
        public string FechaEmision { get; set; }
        public string FechaVencimientoPago { get; set; }
        public string TipoCPDocumento { get; set; }
        public string SerieCDP { get; set; }
        public string NumeroCPDocumentoInicial { get; set; }
        public string NumeroFinalRango { get; set; }
        public string TipoDocIdentidad { get; set; }
        public string NumeroDocIdentidad { get; set; }
        public string ApellidosNombresRazonSocial { get; set; }
        public string ValorFacturadoExportacion { get; set; }
        public string BIGravada { get; set; }
        public string DescuentoBI { get; set; }
        public string IGV_IPM { get; set; }
        public string DescuentoIGV_IPM { get; set; }
        public string MontoExonerado { get; set; }
        public string MontoInafecto { get; set; }
        public string ISC { get; set; }
        public string BIGravadaIVAP { get; set; }
        public string IVAP { get; set; }
        public string ICBPER { get; set; }
        public string OtrosTributos { get; set; }
        public string TotalCP { get; set; }
        public string Moneda { get; set; }
        public string TipoCambio { get; set; }
        public string FechaEmisionDocModificado { get; set; }
        public string TipoCPModificado { get; set; }
        public string SerieCPModificado { get; set; }
        public string NumeroCPModificado { get; set; }
        public string IDProyectoOperadoresAtribucion { get; set; }
        public string TipoNota { get; set; }
        public string EstComp { get; set; }
        public string ValorFOBEmbarcado { get; set; }
        public string ValorOPGratuitas { get; set; }
        public string TipoOperacion { get; set; }
        public string DAMCP { get; set; }
        public string CLU { get; set; }
        public string CARSUNAT { get; set; }
    }
}
