using Newtonsoft.Json.Linq;
using SAPbobsCOM;
using SAPbouiCOM;
using STR_Addon_PeruRamo.BL.APR;
using STR_Addon_PeruRamo.Services;
using STR_Addon_PeruRamo.Util;
using STR_Addon_PeruRamo_V1.Forms.USRForms.Sire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace STR_Addon_PeruRamo_V1.Forms.USRForms
{
    sealed partial class FormMnuPE : UIForm
    {
        private static FormMnuPE thisForm;
        private FormMnuPE() : base(gs_nombre, gs_ruta) { }
        public static FormMnuPE getForm()
        {
            thisForm = thisForm ?? new FormMnuPE();
            return thisForm;
        }
        protected override void sb_dataFormLoad()
        {
            try
            {
                form.Freeze(true);
                enabledEdt();

            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                form.Freeze(false);
            }
        }

        public void sb_dataClaves()
        {

            SAPbobsCOM.Recordset go_RecorSet = Global.go_sboCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
            int li_idDB = Global.gi_queryPosition;
            if (li_idDB == 1)
                go_RecorSet.DoQuery("SELECT \"U_STR_Clave\" FROM \"@STR_ADDONSPERU\"");
            else
                go_RecorSet.DoQuery("SELECT [U_STR_Clave] FROM \"@STR_ADDONSPERU\"");


        }

        #region control de campos
        public void enabledEdt()
        {
            try
            {

                SAPbobsCOM.UserTable userTable = null;
                userTable = sboCompany.UserTables.Item("STR_ADDONSPERU");

                foreach (PeruAddon item in Enum.GetValues(typeof(PeruAddon)))
                {
                    if (userTable.GetByKey(((int)item).ToString()))
                    {
                        string valor = userTable.UserFields.Fields.Item("U_STR_Activo").Value;
                        switch (item)
                        {
                            case PeruAddon.Localizacion:
                                string ls_clv = userTable.UserFields.Fields.Item("U_STR_Clave").Value;
                                if (valor == "Y")
                                {
                                    form.getCheckBox(lrs_ChxLoca).Checked = true;

                                    if (Validacion.fn_getValidacion(ls_clv, (int)PeruAddon.Localizacion))
                                    {
                                        form.getEditText(lrs_EdtLocalizacion).BackColor = 198 | (255 << 8) | (168 << 16);
                                        form.getEditText(lrs_EdtLocalizacion).Active = false;
                                        form.enabledItems(false, lrs_BtnLocalizacion);
                                        //form.enabledItems(false, lrs_EdtLocalizacion);
                                    }
                                    else
                                    { 
                                        form.getEditText(lrs_EdtLocalizacion).BackColor = 255 | (255 << 8) | (2555 << 16);
                                        form.enabledItems(true, lrs_EdtLocalizacion);
                                    }
                                    form.getEditText(lrs_EdtLocalizacion).Value = userTable.UserFields.Fields.Item("U_STR_Clave").Value;
                                }
                                else
                                {
                                    form.getCheckBox(lrs_ChxLoca).Checked = false;
                                    form.getEditText(lrs_EdtLocalizacion).Value = userTable.UserFields.Fields.Item("U_STR_Clave").Value;
                                };
                                break;
                            case PeruAddon.Sire:
                                string ls_clvSire = userTable.UserFields.Fields.Item("U_STR_Clave").Value;
                                if (valor == "Y")
                                {
                                    form.getCheckBox(lrs_ChxSir).Checked = true;

                                    if (Validacion.fn_getValidacion(ls_clvSire, (int)PeruAddon.Sire))
                                    {
                                        form.getEditText(lrs_EdtSire).BackColor = 198 | (255 << 8) | (168 << 16);
                                        form.getEditText(lrs_EdtSire).Active = false;
                                        form.enabledItems(false, lrs_BtnSire);
                                        // form.enabledItems(false, lrs_EdtSire);
                                    }

                                    else
                                    {
                                        form.getEditText(lrs_EdtSire).BackColor = 255 | (255 << 8) | (2555 << 16);
                                        form.enabledItems(true, lrs_EdtSire);
                                    }
                                    form.getEditText(lrs_EdtSire).Value = userTable.UserFields.Fields.Item("U_STR_Clave").Value;
                                }
                                else
                                {
                                    form.getCheckBox(lrs_ChxSir).Checked = false;
                                    form.getEditText(lrs_EdtSire).Value = userTable.UserFields.Fields.Item("U_STR_Clave").Value;

                                }
                                break;
                            case PeruAddon.CCEAR:
                                string ls_clvCC = userTable.UserFields.Fields.Item("U_STR_Clave").Value;
                                if (valor == "Y")
                                {
                                    form.getCheckBox(lrs_ChxCCEAR).Checked = true;


                                    if (Validacion.fn_getValidacion(ls_clvCC, (int)PeruAddon.CCEAR))
                                    {
                                        form.getEditText(lrs_EdtCajaEar).BackColor = 198 | (255 << 8) | (168 << 16);
                                        form.getEditText(lrs_EdtCajaEar).Active = false;
                                        form.enabledItems(false, lrs_BtnCajaEar);
                                        //form.enabledItems(false, lrs_EdtCajaEar);
                                    }
                                    else
                                    {
                                        form.getEditText(lrs_EdtCajaEar).BackColor = 255 | (255 << 8) | (2555 << 16);
                                        form.enabledItems(true, lrs_EdtCajaEar);
                                    }
                                    form.getEditText(lrs_EdtCajaEar).Value = userTable.UserFields.Fields.Item("U_STR_Clave").Value;
                                }
                                else
                                {
                                    form.getCheckBox(lrs_ChxCCEAR).Checked = false;
                                    form.getEditText(lrs_EdtCajaEar).Value = userTable.UserFields.Fields.Item("U_STR_Clave").Value;
                                }
                                break;
                            case PeruAddon.Letras:
                                string ls_clvLet = userTable.UserFields.Fields.Item("U_STR_Clave").Value;
                                if (valor == "Y")
                                {

                                    form.getCheckBox(lrs_ChxLetrs).Checked = true;


                                    if (Validacion.fn_getValidacion(ls_clvLet, (int)PeruAddon.Letras))
                                    {
                                        form.getEditText(lrs_EdtLetras).BackColor = 198 | (255 << 8) | (168 << 16);
                                        form.getEditText(lrs_EdtLetras).Active = false;
                                        form.enabledItems(false, lrs_BtnLetras);
                                        //form.enabledItems(false, lrs_EdtLetras);
                                    }
                                    else
                                    {
                                        form.getEditText(lrs_EdtLetras).BackColor = 255 | (255 << 8) | (2555 << 16);
                                        form.enabledItems(true, lrs_EdtLetras);
                                    }
                                    form.getEditText(lrs_EdtLetras).Value = userTable.UserFields.Fields.Item("U_STR_Clave").Value;

                                }
                                else
                                {
                                    form.getCheckBox(lrs_ChxLetrs).Checked = false;
                                    form.getEditText(lrs_EdtLetras).Value = userTable.UserFields.Fields.Item("U_STR_Clave").Value;
                                }
                                break;

                            default:
                                break;
                        }
                    }

                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        private void updateCampos()
        {
            SAPbobsCOM.UserTable userTable;
            string value = "";
            try
            {
                userTable = sboCompany.UserTables.Item("STR_ADDONSPERU");

                foreach (SAPbouiCOM.Item item in form.Items)
                {
                    if (item.Type != BoFormItemTypes.it_CHECK_BOX) continue;
                    userTable.GetByKey(item.UniqueID);
                    switch (item.Type)
                    {
                        case BoFormItemTypes.it_CHECK_BOX:
                            dynamic val = item.Specific.Checked;
                            value = val ? "Y" : "N";
                            break;
                    }
                    userTable.UserFields.Fields.Item("U_STR_Activo").Value = value;
                    string instalado = userTable.UserFields.Fields.Item("U_STR_Instalacion").Value;
                    /* if (Convert.ToInt32(instalado) == 0)
                         if (Validacion.fn_getComparacion(Convert.ToInt32(item.UniqueID)))
                             if (Validacion.fn_createProcedures(Convert.ToInt32(item.UniqueID)))
                                 userTable.UserFields.Fields.Item("U_STR_Instalacion").Value = 1;
                     */
                    userTable.Update();
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                Global.go_sboApplictn.statusBarSuccessMsg("Se actualizo correctamente el Menu");
            }

        }
        #endregion control de campos
        public void sb_updtAddonP(bool pb_result, string ps_code, int pi_addonId)
        {
            SAPbobsCOM.Recordset go_RecorSet = Global.go_sboCompany.GetBusinessObject(BoObjectTypes.BoRecordset);

            int li_dbPs = Global.gi_queryPosition;

            if (!pb_result)
            {
                Global.go_sboApplictn.MessageBox("Codigo Incorrecto contacte con el proveedor", 1, "Ok");
                if (li_dbPs == 1)
                    go_RecorSet.DoQuery($"UPDATE \"@STR_ADDONSPERU\" SET \"U_STR_Clave\"  = NULL, \"U_STR_Activo\" = 'N'  where \"Code\" = '{pi_addonId}'");
                else
                    go_RecorSet.DoQuery($"UPDATE \"@STR_ADDONSPERU\" SET [U_STR_Clave]  = NULL, [U_STR_Activo] = 'N' where [Code] = '{pi_addonId}'");
            }
            else
            {

                Global.go_sboApplictn.MessageBox("¡Codigo Correcto! Se guardo la clave", 1, "Ok");

                if (li_dbPs == 1)
                    go_RecorSet.DoQuery($"UPDATE \"@STR_ADDONSPERU\" SET \"U_STR_Clave\"  = '{ps_code}', \"U_STR_Activo\" = 'Y'  where \"Code\" = '{pi_addonId}'");
                else
                    go_RecorSet.DoQuery($"UPDATE \"@STR_ADDONSPERU\" SET [U_STR_Clave]  = '{ps_code}', [U_STR_Activo] = 'Y' where [Code] = '{pi_addonId}'");


            }

        }

        public void sb_updtFocEdit(string ps_campo, string ps_btnCampo)
        {
            if (form.Items.Item(ps_campo).Visible) form.getEditText(ps_campo).Active = false;

            bool lb_visible = form.Items.Item(ps_campo).Visible;
            form.visibleItems(!lb_visible, ps_campo, ps_btnCampo);
        }

        protected override void loadDataActions()
        {



            this.Add(new ItemAction(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, btnSav, e =>
            {
                if (!e.BeforeAction)
                {

                    updateCampos();
                    form.Close();
                }
            }));

            this.Add(new ItemAction(BoEventTypes.et_FORM_UNLOAD, "", (e) =>
            {
                if (!e.BeforeAction)
                {
                    this.Dispose();
                    thisForm = null;
                }
            }));
            #region Click Events de los CheckBoxs 

            this.Add(new ItemAction(SAPbouiCOM.BoEventTypes.et_DOUBLE_CLICK, lrs_ChxSir, e =>
            {

                if (!e.BeforeAction)
                {
                    //  sb_updtFocEdit(lrs_EdtSire, lrs_BtnSire);
                }
            }));

            this.Add(new ItemAction(SAPbouiCOM.BoEventTypes.et_CLICK, lrs_ChxSir, e =>
            {

                if (!e.BeforeAction)
                {
                    //   sb_updtFocEdit(lrs_EdtSire, lrs_BtnSire);
                }
            }));
            this.Add(new ItemAction(SAPbouiCOM.BoEventTypes.et_DOUBLE_CLICK, lrs_ChxLoca, e =>
            {
                if (!e.BeforeAction)
                {
                    //   sb_updtFocEdit(lrs_EdtLocalizacion, lrs_BtnLocalizacion);
                }
            }));
            this.Add(new ItemAction(SAPbouiCOM.BoEventTypes.et_CLICK, lrs_ChxLoca, e =>
            {
                if (!e.BeforeAction)
                {
                    //sb_updtFocEdit(lrs_EdtLocalizacion, lrs_BtnLocalizacion);
                }
            }));

            this.Add(new ItemAction(SAPbouiCOM.BoEventTypes.et_DOUBLE_CLICK, lrs_ChxLetrs, e =>
            {
                if (!e.BeforeAction)
                {
                    //  sb_updtFocEdit(lrs_EdtLetras, lrs_BtnLetras);
                }
            }));

            this.Add(new ItemAction(SAPbouiCOM.BoEventTypes.et_CLICK, lrs_ChxLetrs, e =>
            {
                if (!e.BeforeAction)
                {
                    // sb_updtFocEdit(lrs_EdtLetras, lrs_BtnLetras);
                }
            }));
            this.Add(new ItemAction(SAPbouiCOM.BoEventTypes.et_DOUBLE_CLICK, lrs_ChxCCEAR, e =>
            {
                if (!e.BeforeAction)
                {
                    //  sb_updtFocEdit(lrs_EdtCajaEar, lrs_BtnCajaEar);
                }
            }));
            this.Add(new ItemAction(SAPbouiCOM.BoEventTypes.et_CLICK, lrs_ChxCCEAR, e =>
            {
                if (!e.BeforeAction)
                {
                    //  sb_updtFocEdit(lrs_EdtCajaEar, lrs_BtnCajaEar);
                }
            }));

            this.Add(new ItemAction(SAPbouiCOM.BoEventTypes.et_DOUBLE_CLICK, lrs_ChxTcambio, e =>
            {
                if (!e.BeforeAction)
                {
                    //   sb_updtFocEdit(lrs_EdtTipoCambio, lrs_BtnTipoCambio);
                }
            }));

            this.Add(new ItemAction(SAPbouiCOM.BoEventTypes.et_CLICK, lrs_ChxTcambio, e =>
            {
                if (!e.BeforeAction)
                {
                    //  sb_updtFocEdit(lrs_EdtTipoCambio, lrs_BtnTipoCambio);
                }
            }));

            #endregion Click Events de los CheckBoxs 

            this.Add(new ItemAction(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, lrs_BtnLocalizacion, e =>
            {
                if (!e.BeforeAction)
                {

                    try
                    {

                        int li_adId = 1;

                        string code = form.getEditText(lrs_EdtLocalizacion).Value;

                        bool lb_reuslt = Validacion.fn_getValidacion(code, li_adId);

                        sb_updtAddonP(lb_reuslt, code, li_adId);

                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    finally
                    {
                    }

                }
            }));

            this.Add(new ItemAction(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, lrs_BtnSire, e =>
            {
                if (!e.BeforeAction)
                {

                    try
                    {

                        int li_adId = 2;

                        string code = form.getEditText(lrs_EdtSire).Value;

                        bool lb_reuslt = Validacion.fn_getValidacion(code, li_adId);

                        sb_updtAddonP(lb_reuslt, code, li_adId);

                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    finally
                    {
                    }

                }
            }));

            this.Add(new ItemAction(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, lrs_BtnCajaEar, e =>
            {
                if (!e.BeforeAction)
                {

                    try
                    {

                        int li_adId = 3;

                        string code = form.getEditText(lrs_EdtCajaEar).Value;

                        bool lb_reuslt = Validacion.fn_getValidacion(code, li_adId);

                        sb_updtAddonP(lb_reuslt, code, li_adId);

                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    finally
                    {
                    }

                }
            }));


            this.Add(new ItemAction(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, lrs_BtnLetras, e =>
            {
                if (!e.BeforeAction)
                {

                    try
                    {

                        int li_adId = 4;

                        string code = form.getEditText(lrs_EdtLetras).Value;

                        bool lb_reuslt = Validacion.fn_getValidacion(code, li_adId);

                        sb_updtAddonP(lb_reuslt, code, li_adId);

                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    finally
                    {
                    }
                }
            }));


            this.Add(new ItemAction(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, lrs_BtnTipoCambio, e =>
            {
                if (!e.BeforeAction)
                {

                    try
                    {

                        int li_adId = 5;

                        string code = form.getEditText(lrs_EdtTipoCambio).Value;

                        bool lb_reuslt = Validacion.fn_getValidacion(code, li_adId);

                        sb_updtAddonP(lb_reuslt, code, li_adId);

                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    finally
                    {
                    }
                }
            }));

        }




        protected override void loadItemActions()
        {
            //throw new NotImplementedException();
        }
    }
}
