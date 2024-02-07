using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STR_Addon_PeruRamo.Util
{
    public class GblSire
    {
        public static string fn_createDrctry(string ps_ruta)
        {

            DirectoryInfo lo_info = Directory.CreateDirectory(ps_ruta);
            return lo_info.FullName;

        }



    }
    /*
    public enum FormularioUsuario
    {
        FrmRVIEV,
        FrmRVIEC
    }
    */
    public enum ItemConfigSire
    {
        grant_type = 1,
        scope = 2,
        client_id = 3,
        client_secret = 4,
        username = 5,
        password = 6,
        ArchivosSire = 7
    }

    public enum FaseSire
    {

        Propuesta = 1,
        Preliminar = 2,
        PreliminarFinal = 3,
        GeneracionRegistro = 4

    }


}
