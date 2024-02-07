using STR_Addon_PeruRamo.EL;
using STR_Addon_PeruRamo.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SAPbouiCOM;
using SAPbobsCOM;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace STR_Addon_PeruRamo.BL.Sire
{
    public static class SIREMod
    {


        public static string fn_motivoProceso(string codEstadoProceso)
        {
            switch (codEstadoProceso)
            {
                case "01":
                    return "Cargado";
                case "02":
                    return "En Proceso";
                case "03":
                    return "Procesado con Errores";
                case "04":
                    return "Procesado sin Errores";
                case "05":
                    return "En Proceso";
                case "06":
                    return "Terminado";
                default:
                    return "";

            }
        }
        public static void cargarLogo(Form oForm)
        {
            string sPath = System.Windows.Forms.Application.StartupPath.ToString();
            Button oButton = (Button)oForm.Items.Item("btnLogo").Specific;
            oButton.Type = BoButtonTypes.bt_Image;
            oButton.Image = sPath + "\\Resources\\Imgs\\logo_empresa_1.png";
        }
        public static string fn_setArchiv(string carpeta)
        {
            try
            {
                SAPbobsCOM.UserTable go_userTable;

                go_userTable = Global.go_sboCompany.UserTables.Item("STR_CF_SIRE");
                go_userTable.GetByKey("7");
                return go_userTable.UserFields.Fields.Item("U_STR_Valor").Value + $"\\{carpeta}";
            }
            catch (Exception)
            {

                throw;
            }

        }

        public static bool fn_getValidConfig() {

            UserTable userTable;

            userTable = Global.go_sboCompany.UserTables.Item("STR_CF_SIRE");

            foreach (ItemConfigSire item in Enum.GetValues(typeof(ItemConfigSire)))
            {
                if (userTable.GetByKey(((int)item).ToString()))
                {
                    if (userTable.UserFields.Fields.Item("U_STR_Valor").Value == "" | userTable.UserFields.Fields.Item("U_STR_Valor").Value == null) { 
                        return false;
                    }
                }
            };
            userTable = null;
            return true;           
        }

        public static string fn_getConnectSire()
        {
            try
            {
                string client_id = string.Empty;


                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.UseNagleAlgorithm = true;
                ServicePointManager.Expect100Continue = false;
                ServicePointManager.CheckCertificateRevocationList = true;

                SAPbobsCOM.UserTable userTable;

                userTable = Global.go_sboCompany.UserTables.Item("STR_CF_SIRE");

                var body = new Dictionary<string, string>();
                string a;

                foreach (ItemConfigSire item in Enum.GetValues(typeof(ItemConfigSire)))
                {
                    if (userTable.GetByKey(((int)item).ToString()))
                    {
                        if (userTable.UserFields.Fields.Item("U_STR_Clave").Value != "perTributario" &
                            userTable.UserFields.Fields.Item("U_STR_Clave").Value != "ArchivosSire")
                        {

                            body.Add(userTable.UserFields.Fields.Item("U_STR_Clave").Value, userTable.UserFields.Fields.Item("U_STR_Valor").Value);

                            if (userTable.UserFields.Fields.Item("U_STR_Clave").Value == "client_id")
                            {
                                client_id = userTable.UserFields.Fields.Item("U_STR_Valor").Value;
                            }
                        }
                    }
                };
                string uri = $"https://api-seguridad.sunat.gob.pe/v1/clientessol/{client_id}/oauth2/token/";

                var client = new HttpClient();

                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Post,
                    Content = new FormUrlEncodedContent(body)
                };

                RSLogin login = System.Text.Json.JsonSerializer.Deserialize<RSLogin>(client.SendAsync(request).Result.Content.ReadAsStringAsync().Result);
                return login.access_token;
            }
            catch (Exception)
            {
                throw new Exception("Error al conectarse al Sire. Intentar mas tarde");
            }
        }

        public static HttpResponseMessage fn_verificaPropuesta(string token, string periodo)
        {
            try
            {
                HttpClient client = new HttpClient();

                string uri = $"https://api-sire.sunat.gob.pe/v1/contribuyente/migeigv/libros/rvierce/generales/web/{periodo}/verificapropuesta";

                HttpRequestMessage httpRequestMessage = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(uri),
                };

                httpRequestMessage.Headers.Add("Authorization", $"Bearer {token}");

                var response = client.SendAsync(httpRequestMessage).Result;

                return response;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static void sb_exportCsv(SAPbouiCOM.Grid po_grid, string ps_carpeta, string ps_perTributario, string ps_ruta)
        {

            try
            {
                int ls_hora = DateTime.Now.Hour;
                int ls_minuto = DateTime.Now.Minute;
                string ls_nombre = $"R-CMPRCN-{ls_hora}{ls_minuto}-{ps_perTributario}";

                string ls_ruta = GblSire.fn_createDrctry(ps_ruta + $"\\{ps_carpeta}");
                string ls_filePath = ls_ruta + $"\\{ls_nombre}.csv";
                StringBuilder lo_csvContent = new StringBuilder();

                // Agregar encabezados de columna al archivo CSV
                for (int i = 0; i < po_grid.DataTable.Columns.Count; i++)
                {
                    if (!po_grid.DataTable.Columns.Item(i).Name.Contains("DocumentEntry") &
                        !po_grid.DataTable.Columns.Item(i).Name.Contains("ObjectType") &
                        !po_grid.DataTable.Columns.Item(i).Name.Contains("Serie Sap Re") &
                            !po_grid.DataTable.Columns.Item(i).Name.Contains("|"))
                    {
                        lo_csvContent.Append($"{po_grid.DataTable.Columns.Item(i).Name};");
                    }
                }
                lo_csvContent.AppendLine();

                // Agregar datos de las filas al archivo CSV
                for (int row = 0; row < po_grid.DataTable.Rows.Count; row++)
                {
                    for (int col = 0; col < po_grid.DataTable.Columns.Count; col++)
                    {
                        if (col != 2 & col != 7 & col != 8 & col != 9)
                        {
                            string cellValue = Convert.ToString(po_grid.DataTable.GetValue(col, row));
                            lo_csvContent.Append($"{cellValue};");
                        }
                    }
                    lo_csvContent.AppendLine();
                }

                // Escribir el contenido al archivo
                File.WriteAllText(ls_filePath, lo_csvContent.ToString());

                // Global.go_sboApplictn.MessageBox($"El archivo fue exportado exitosamente en la ruta: {ls_filePath}", 1, "Ok");
                Global.go_sboApplictn.statusBarSuccessMsg($"El archivo fue exportado exitosamente en la ruta: {ls_filePath}");
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static int fn_getRgb(int red, int green, int blue)
        {
            return red | (green << 8) | (blue << 16);
        }

        public static List<string> fn_getMonth(string ps_perTributario)
        {
            try
            {


                // Supongamos que el valor original es 202310
                int li_mes = int.Parse(ps_perTributario.Remove(0, 4));

                string ls_nombreMes = string.Empty;

                List<string> lst_data = new List<string>();

                switch (li_mes)
                {
                    case 1:
                        ls_nombreMes = "Enero";
                        lst_data.Add(ls_nombreMes);
                        lst_data.Add(ps_perTributario);
                        return lst_data;
                    case 2:
                        ls_nombreMes = "Febrero";
                        lst_data.Add(ls_nombreMes);
                        lst_data.Add(ps_perTributario);
                        return lst_data;
                    case 3:
                        ls_nombreMes = "Marzo";
                        lst_data.Add(ls_nombreMes);
                        lst_data.Add(ps_perTributario);
                        return lst_data;
                    case 4:
                        ls_nombreMes = "Abril";
                        lst_data.Add(ls_nombreMes);
                        lst_data.Add(ps_perTributario);
                        return lst_data;
                    case 5:
                        ls_nombreMes = "Mayo";
                        lst_data.Add(ls_nombreMes);
                        lst_data.Add(ps_perTributario);
                        return lst_data;
                    case 6:
                        ls_nombreMes = "Junio";
                        lst_data.Add(ls_nombreMes);
                        lst_data.Add(ps_perTributario);
                        return lst_data;
                    case 7:
                        ls_nombreMes = "Julio";
                        lst_data.Add(ls_nombreMes);
                        lst_data.Add(ps_perTributario);
                        return lst_data;
                    case 8:
                        ls_nombreMes = "Agosto";
                        lst_data.Add(ls_nombreMes);
                        lst_data.Add(ps_perTributario);
                        return lst_data;
                    case 9:
                        ls_nombreMes = "Septiembre";
                        lst_data.Add(ls_nombreMes);
                        lst_data.Add(ps_perTributario);
                        return lst_data;
                    case 10:
                        ls_nombreMes = "Octubre";
                        lst_data.Add(ls_nombreMes);
                        lst_data.Add(ps_perTributario);
                        return lst_data;
                    case 11:
                        ls_nombreMes = "Noviembre";
                        lst_data.Add(ls_nombreMes);
                        lst_data.Add(ps_perTributario);
                        return lst_data;
                    case 12:
                        ls_nombreMes = "Diciembre";
                        lst_data.Add(ls_nombreMes);
                        lst_data.Add(ps_perTributario);
                        return lst_data;
                    default:
                        // Manejar el caso en el que el número no corresponda a ningún mes
                        ls_nombreMes = "Mes no válido";
                        throw new Exception("Mes no válido");

                }


            }
            catch (Exception)
            {

                throw;
            }
        }


        public static string fn_comprobanteModificado(List<DocumentoMod> plo_docMod)
        {
            if (plo_docMod == null)
                return string.Empty;
            if (plo_docMod.Count > 0)
            {
                string ls_documentoSet = plo_docMod[0].codTipoCDPMod + "-" + plo_docMod[0].numSerieCDPMod + "-" + plo_docMod[0].numCDPMod;
                return ls_documentoSet;
            }
            return string.Empty;
        }
    }
}
