using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STR_Addon_PeruRamo.Util
{
    public class Global
    {
        public static SAPbobsCOM.Company go_sboCompany = null;
        public static SAPbouiCOM.Application go_sboApplictn = null;
        public static SAPbobsCOM.SBObob sboBob = null;
        public static int gi_queryPosition;
        public static int gi_addnPosition = 0;
        public static string gs_hardwarek = "";
    }

    public enum PeruAddon
    {
        Localizacion = 1,
        Sire = 2,
        CCEAR = 3,
        Letras = 4
    }

    public enum FormularioUsuario
    {
        FrmAdPE,
        FrmRVIEV,
        FrmRVIEC
    }


    public enum CodigoAddon
    {
        RAMOLOCALI = 1,
        RAMOSIRE = 2,
        RAMOEAR = 3,
        RAMOLETRAS = 4
    }
}
