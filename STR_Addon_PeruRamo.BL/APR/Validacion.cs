using SAPbobsCOM;
using SAPbouiCOM;
using STR_Addon_PeruRamo.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace STR_Addon_PeruRamo.BL.APR
{
    public static class Validacion
    {
        //public bool fn_getPreviaValidacion(int ps_addn)
        //{

        //    if ()

        //}


        public static bool fn_getComparacion(int pi_addnId)
        {

            try
            {
                SAPbouiCOM.Application lo_app = Global.go_sboApplictn;
                SAPbobsCOM.Recordset go_RecordSet = Global.go_sboCompany.GetBusinessObject(BoObjectTypes.BoRecordset);

                go_RecordSet.DoQuery($"SELECT \"U_STR_Clave\",\"U_STR_Activo\" FROM \"@STR_ADDONSPERU\" where \"Code\" = '{pi_addnId}'");
                if (go_RecordSet.RecordCount > 0)
                {

                    string ls_code = go_RecordSet.Fields.Item(0).Value;
                    string ls_activo = go_RecordSet.Fields.Item(1).Value;

                    if (ls_code == null | ls_code == "" | fn_getValidacion(ls_code, pi_addnId) == false)
                    {
                        lo_app.statusBarWarningMsg($"Funcionalidades del Addon {(PeruAddon)pi_addnId} restringido, contacta con tu proveedor para adquirirla");
                        return false;
                    }
                    else if (ls_activo == "N")
                    {
                        lo_app.statusBarWarningMsg($"Funcionalidades del Addon {(PeruAddon)pi_addnId} se encuentra desactiva, si deseas activiarlo dirigite al Menu Addon Perú");
                        return false;
                    }
                    return true;
                }
                lo_app.MessageBox("La tabla @STR_ADDONSPERU no contiene está funcionalidad, contacta con tu proveedor");
                return false;
            }
            catch (Exception)
            {

                throw;
            }

        }


        public static bool fn_getValidacion(string ps_key, int ps_addn)
        {
            try
            {
                SAPbobsCOM.Company sboCompany = Global.go_sboCompany;

                if (ps_key == string.Empty)
                    return false;


                CodigoAddon codigoAddon = (CodigoAddon)ps_addn;

                string ls_nombDb = sboCompany.CompanyDB;
                string ls_nomAdd = codigoAddon.ToString();
                string ls_hardKey = Global.gs_hardwarek;

                var lst_serial = Code.GetToken(ls_nombDb, ls_nomAdd, ls_hardKey);

                string ls_key = lst_serial;
                string[] listKey = ps_key.Split('|');

                if (listKey[0].ToString() == ls_key)
                {
                    string date = listKey[1];

                    if (string.IsNullOrWhiteSpace(date))
                        return false;

                    date = DecryptDate(date);
                    DateTime fecha = DateTime.ParseExact(date, "yyMMdd", null);
                    if (fecha >= DateTime.Now)
                        return true;
                    else
                    {
                        Global.go_sboApplictn.statusBarErrorMsg("Se validó que las credenciales del Addon Perú ya vencieron. Contacta con tu proveedor");
                        return false;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string fn_getHrdKey()
        {

            try
            {
                SAPbouiCOM.Application app = Global.go_sboApplictn;

                app.Menus.Item("257").Activate();
                SAPbouiCOM.Form aboutSAP = app.Forms.GetForm("999999", 0);
                string a = ((SAPbouiCOM.EditText)aboutSAP.Items.Item("79").Specific).Value;
                aboutSAP.Close();

                return a;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private static string DecryptDate(string encryptedDate)
        {
            int encryptionKey = 42; // Utiliza el mismo valor que usaste para cifrar

            char[] dateChars = encryptedDate.ToCharArray();

            for (int i = 0; i < dateChars.Length; i++)
            {
                dateChars[i] = (char)(dateChars[i] - encryptionKey);
            }

            return new string(dateChars);
        }

        public static string fn_getBits()
        {
            try
            {
                SAPbouiCOM.Application app = Global.go_sboApplictn;

                app.Menus.Item("257").Activate();
                SAPbouiCOM.Form aboutSAP = app.Forms.GetForm("999999", 0);
                string a = ((SAPbouiCOM.StaticText)aboutSAP.Items.Item("26").Specific).Caption;
                aboutSAP.Close();

                return a;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public static string fn_getCodigo(int ps_addn)
        {
            int li_tipodb = Global.gi_queryPosition;
            SAPbobsCOM.Recordset go_Recorset = Global.go_sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

            if (li_tipodb == 1)
                go_Recorset.DoQuery($"SELECT \"U_STR_Clave\" FROM \"@STR_ADDONSPERU\" WHERE \"Code\" = '{ps_addn}'");
            else
                go_Recorset.DoQuery($"SELECT [U_STR_Clave] FROM \"@STR_ADDONSPERU\" WHERE [Code] = '{ps_addn}'");


            if (go_Recorset.RecordCount > 0)
                return go_Recorset.Fields.Item(0).Value == null ? string.Empty : go_Recorset.Fields.Item(0).Value;
            else
                throw new Exception("No se encontró addon en la tabla @STR_ADDONSPERU");
        }


    }
}
