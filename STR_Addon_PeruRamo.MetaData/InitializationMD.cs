using SAPbouiCOM;
using STR_Addon_PeruRamo.Util;
using System;
using static STR_Addon_PeruRamo.Util.Global;

namespace STR_Addon_PeruRamo.MetaData
{
    public class InitializationMD
    {

        public bool verificarInstalacion()
        {

            crearTabla();
            validaData();

            return true;
        }


        public void crearTabla()
        {
            SAPbobsCOM.UserTablesMD userTableMD = null;
            SAPbobsCOM.UserFieldsMD userTable = null;
            SAPbobsCOM.UserObjectsMD userObjects = null;
            bool flagOK = true;

            try
            {
                userTableMD = go_sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserTables);
                if (!userTableMD.GetByKey("STR_ADDONSPERU"))
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTableMD);

                    SAPbobsCOM.UserTablesMD oUsrTbl = go_sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserTables);
                    oUsrTbl.TableName = "STR_ADDONSPERU";
                    oUsrTbl.TableDescription = "Tabla de Addon Perú";


                    if (oUsrTbl.Add() == 0)
                        go_sboApplictn.statusBarSuccessMsg("Se creo correctamente la tabla @STR_ADDONSPERU");

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oUsrTbl);

                    userTable = go_sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserFields);
                    userTable.TableName = "STR_ADDONSPERU";
                    userTable.Name = "STR_Activo";
                    userTable.Description = "Addon Activo";
                    userTable.EditSize = 3;
                    userTable.Type = SAPbobsCOM.BoFieldTypes.db_Alpha;


                    if (userTable.Add() == 0)
                        go_sboApplictn.statusBarSuccessMsg("Se creo correctamente la tabla la columna U_STR_Activo en la tabla @STR_ADDONSPERU");

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oUsrTbl);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                    userTable = null;

                    userTable = go_sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserFields);
                    userTable.TableName = "STR_ADDONSPERU";
                    userTable.Name = "STR_Clave";
                    userTable.Description = "Clave de Addon";
                    userTable.EditSize = 30;
                    userTable.Type = SAPbobsCOM.BoFieldTypes.db_Alpha;

                    if (userTable.Add() == 0)
                        go_sboApplictn.statusBarSuccessMsg("Se creo correctamente la tabla la columna U_STR_Clave en la tabla @STR_ADDONSPERU");


                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oUsrTbl);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                    userTable = null;

                    userTable = go_sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserFields);
                    userTable.TableName = "STR_ADDONSPERU";
                    userTable.Name = "STR_Instalacion";
                    userTable.Description = "Addon instalado";
                    userTable.EditSize = 2;
                    userTable.Type = SAPbobsCOM.BoFieldTypes.db_Alpha;

                    if (userTable.Add() == 0)
                        go_sboApplictn.statusBarSuccessMsg("Se creo correctamente la tabla la columna STR_Instalacion en la tabla @STR_ADDONSPERU");

                }
            }
            catch (Exception e)
            {
                go_sboApplictn.statusBarErrorMsg(e.Message);
            }

        }


        public void validaData()
        {

            SAPbobsCOM.UserTable userTable = null;
            userTable = go_sboCompany.UserTables.Item("STR_ADDONSPERU");

            foreach (PeruAddon item in Enum.GetValues(typeof(PeruAddon)))
            {
                if (!userTable.GetByKey(((int)item).ToString()))
                {
                    userTable.Code = ((int)item).ToString();
                    userTable.Name = Enum.GetName(typeof(PeruAddon), item);
                    userTable.UserFields.Fields.Item("U_STR_Activo").Value = "N";
                    userTable.UserFields.Fields.Item("U_STR_Instalacion").Value = "0";
                    userTable.Add();
                }
            }

        }
    }
}
