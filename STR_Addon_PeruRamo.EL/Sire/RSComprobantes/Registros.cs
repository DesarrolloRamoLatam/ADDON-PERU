using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace STR_Addon_PeruRamo.EL
{
    public class Registros
    {
        public string codTipoCDP { get; set; }
        public string desCodTipoCDP { get; set; }
        public int cntDocumentos { get; set; }
        public double mtoISC { get; set; }
        public double mtoIcbper { get; set; }
        public double mtoOtrosTrib { get; set; }
        public double mtoTotalCp { get; set; }
        public double mtoTotalCP { get; set; }
        public double mtoTotalValFactExpo { get; set; }
        public double mtoTotalBIGravada { get; set; }
        public double mtoTotalDsctoBI { get; set; }
        public double mtoTotalIGV { get; set; }
        public double mtoTotalDsctoIGV { get; set; }
        public double mtoTotalExonerado { get; set; }
        public double mtoTotalInafecto { get; set; }
        public double mtoTotalBIIvap { get; set; }
        public double mtoTotalIvap { get; set; }
        public string id { get; set; }
        public string numRuc { get; set; }
        public string nomRazonSocial { get; set; }
        public string perPeriodoTributario { get; set; }
        public string codCar { get; set; }
        public string numSerieCDP { get; set; }
        public string numCDP { get; set; }
        public string codTipoCarga { get; set; }
        public string codSituacion { get; set; }
        public string fecEmision { get; set; }
        public string fecVencPag { get; set; }
        public string codTipoDocIdentidad { get; set; }
        public string numDocIdentidad { get; set; }
        public string nomRazonSocialCliente { get; set; }
        public double mtoValFactExpo { get; set; }
        public double mtoBIGravada { get; set; }
        public double mtoDsctoBI { get; set; }
        public double mtoIGV { get; set; }
        public double mtoDsctoIGV { get; set; }
        public double mtoExonerado { get; set; }
        public double mtoInafecto { get; set; }
        public double mtoBIIvap { get; set; }
        public double mtoIvap { get; set; }
        public double mtoIcbp { get; set; }
        public string codMoneda { get; set; }
        public double? mtoTipoCambio { get; set; }
        public string codEstadoComprobante { get; set; }
        public string desEstadoComprobante { get; set; }
        public string indOperGratuita { get; set; }
        public double? mtoValorOpGratuitas { get; set; }
        public double? mtoValorFob { get; set; }
        public string indTipoOperacion { get; set; }
        public double? mtoPorcParticipacion { get; set; }
        public double? mtoValorFobDolar { get; set; }
        public double? mtoBIGravadoDG { get; set; }
        public double? mtoIgvIpmDG { get; set; }
        public double? mtoBiGravadoDGNG { get; set; }
        public double? mtoIgvIpmDGNG { get; set; }
        public double? mtoBiGravadoDNG { get; set; }
        public double? mtoIgvIpmDNG { get; set; }
        public double? mtoValorAdqNG { get; set; }
        public double? mtoTotalIMB { get; set; }
        public string desTipoCDP { get; set; }
        public string numCDPRangoFinal { get; set; }
        public string codTipoDocIdentidadProveedor { get; set; }
        public string numDocIdentidadProveedor { get; set; }
        public string nomRazonSocialProveedor { get; set; }
        public object codTipoMotivoNota { get; set; }
        public object desTipoMotivoNota { get; set; }
        public object indEditable { get; set; }
        public string perTributario { get; set; }
        public object numInconsistencias { get; set; }
        public object indInfIncompleta { get; set; }
        public object indModificadoContribuyente { get; set; }
        public object plazoVisualizacion { get; set; }
        public string indDetraccion { get; set; }
        public int indIncluExcluCar { get; set; }
        public object porParticipacion { get; set; }
        public string codBbss { get; set; }
        public object codIdProyecto { get; set; }
        public object annCDP { get; set; }
        public object codDepAduanera { get; set; }
        public string indFuenteCP { get; set; }
        public List<string> liscodInconsistencia { get; set; }
        public List<string> lisNumCasilla { get; set; }
        public int codClaseIncons { get; set; }
        public object porTasaRetencion { get; set; }
        public object desMsjOriginal { get; set; }
        public object numCarIndIE { get; set; }
        public int numCorrelativo { get; set; }
        public double? porTasaIGV { get; set; }
        public object archivoCarga { get; set; }
        public TipoCambio tipoCambio { get; set; }
        public Montos montos { get; set; }
        public object camposLibres { get; set; }
        public Auditoria auditoria { get; set; }
        public List<DocumentoMod> documentoMod { get; set; }
        public List<DocumentoMod> lisDocumentosMod { get; set; }
        public string numCDPAsocDam { get; set; }
        public string codTipoRegistro { get; set; }
        public string desTipoRegistro { get; set; }
        public string codLibro { get; set; }
        public string indContenido { get; set; }
        public string desContenido { get; set; }
        public string codTipoPreliminar { get; set; }
        public string codEstadoRegistro { get; set; }
        public string desEstadoRegistro { get; set; }
        public string indInfoPreliminar { get; set; }
        public string desInfoPreliminar { get; set; }
        public string numAjustePosterior { get; set; }
        public string codEstadoGeneracion { get; set; }
        public string desEstadoGeneracion { get; set; }
        public string indActualizacion { get; set; }
        public string desCorreoOpcional { get; set; }
        public string fechaGeneracionPreliminar { get; set; }
        public string ObjectType { get; set; }
        public string DocumentEntry { get; set; }
        public string _id { get; set; }
        public string codTipoOpe { get; set; }
        public string original { get; set; }
    }
}

