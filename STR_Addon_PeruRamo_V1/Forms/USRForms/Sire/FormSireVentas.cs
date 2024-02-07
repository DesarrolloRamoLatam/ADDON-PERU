using SAPbobsCOM;
using SAPbouiCOM;
using STR_Addon_PeruRamo.BL.APR;
using STR_Addon_PeruRamo.BL.Sire;
using STR_Addon_PeruRamo.EL;
using STR_Addon_PeruRamo.EL.Sire;
using STR_Addon_PeruRamo.Services;
using STR_Addon_PeruRamo.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace STR_Addon_PeruRamo_V1.Forms.USRForms.Sire
{
    sealed partial class FormSireVentas : UIForm
    {
        private static FormSireVentas thisForm;

        private static Dictionary<string, string> ldc_dataMes = null;
        private static List<RSPeriodos> lot_periodos;
        private static SIREServicVenta lo_sunatSrvc;
        private static List<Registros> lot_cpsSap;
        private string ls_rutArchv;
        private string ls_contrlFs = "";

        List<Registros> lo_comprobnts = new List<Registros>();
        private FormSireVentas() : base(gs_nombre, gs_ruta) { }
        public static FormSireVentas getForm()
        {

            thisForm = thisForm ?? new FormSireVentas();
            return thisForm;
        }
        protected override void sb_dataFormLoad()
        {
            try
            {
                form.Freeze(true);

                form.getOptionBtn(lrs_RbnSr).GroupWith(lrs_RbnSp);
                form.getOptionBtn(lrs_RbnSr).GroupWith(lrs_RbnDf);

                bool validConfig = SIREMod.fn_getValidConfig();
                if (validConfig == false)
                {
                    sboApplication.MessageBox("Configuración incompleta, modificar en la tabla de Configuración SIRE", 1, "Ok");
                    form.Close();
                    form = null;
                    return;
                }

                lo_sunatSrvc = new SIREServicVenta();
                ls_rutArchv = lo_sunatSrvc.ls_rutaArchv;

                lot_periodos = lo_sunatSrvc.fn_getPerSire();

                form.Items.Item(lrs_LblColCorr).BackColor = SIREMod.fn_getRgb(198, 255, 168);
                form.Items.Item(lrs_LblColInco).BackColor = SIREMod.fn_getRgb(242, 249, 144);
                form.Items.Item(lrs_LblColFalt).BackColor = SIREMod.fn_getRgb(250, 136, 136);
                form.Items.Item(lrs_LblPortal).ForeColor = SIREMod.fn_getRgb(0, 2, 255);
                form.Items.Item(lrs_LblPortal).TextStyle = (int)BoTextStyle.ts_UNDERLINE + (int)BoTextStyle.ts_BOLD;

                form.visibleItems(false, lrs_EdtCntddSp, lrs_EdtTtlSlSp, lrs_LblCntCpSp, lrs_LblTtlSolSp, lrs_LblSr, lrs_LblSp,
                    lrs_EdtCantCpSr, lrs_EdtTotalSolSr, lrs_LblCantCpSr, lrs_LblTotalSolSr);

                form.Items.Item(lrs_LblFase).ForeColor = SIREMod.fn_getRgb(0, 2, 255);
                form.Items.Item(lrs_LblFase).TextStyle = (int)BoTextStyle.ts_UNDERLINE + (int)BoTextStyle.ts_BOLD;

                for (int i = 0; i < lot_periodos.Count; i++)
                {
                    form.getCombobox(lrs_CmbAnio).ValidValues.Add(lot_periodos[i].numEjercicio, lot_periodos[i].desEstado);
                }
                SIREMod.cargarLogo(form);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                if (form != null)
                    form.Freeze(false);
            }
        }

        public void sb_getMesesLoad()
        {

            for (int i = form.getCombobox(lrs_CmbMeses).ValidValues.Count - 1; i >= 0; i--)
            {
                form.getCombobox(lrs_CmbMeses).ValidValues.Remove(i, SAPbouiCOM.BoSearchKey.psk_Index);
            }


            ldc_dataMes = new Dictionary<string, string>();

            RSPeriodos lo_periodo = lot_periodos.FirstOrDefault(x => x.numEjercicio == form.getCombobox(lrs_CmbAnio).Value);

            for (int i = 0; i < lo_periodo.lisPeriodos.Count; i++)
            {
                var data = SIREMod.fn_getMonth(lo_periodo.lisPeriodos[i].perTributario);
                ldc_dataMes.Add(data[0], data[1]);

                //sb_getMonth(lo_periodo.lisPeriodos[i].perTributario);

                form.getCombobox(lrs_CmbMeses).ValidValues.Add(ldc_dataMes.FirstOrDefault(x => x.Value == lo_periodo.lisPeriodos[i].perTributario).Key,
                    lo_periodo.lisPeriodos[i].desEstado);
            }
        }


        public void sb_faseAsignado(string ps_fase)
        {

            form.enabledItems(true, lrs_LblFase);
            form.visibleItems(true, lrs_LblFase);

            FaseSire faseName = (FaseSire)Convert.ToInt32(ps_fase);
            string ls_faseEstado = faseName == FaseSire.PreliminarFinal ? "Pre. Final" : faseName == FaseSire.GeneracionRegistro ? "Generación de Registro" : faseName.ToString();

            form.getStaticText(lrs_LblFase).Caption = $"Fase: {ls_faseEstado}";

            ls_contrlFs = ps_fase;

        }
        public void sb_setDatosSire()
        {
            try
            {
                RSDocumentos lo_rSComprobantes = new RSDocumentos();

                form.Freeze(true);

                string ls_perTributario = ldc_dataMes[form.getCombobox(lrs_CmbMeses).Value];

                sb_faseAsignado(lo_sunatSrvc.fn_getProcesoFase(ls_perTributario));


                grid = form.getGrid(lrs_GrdVentas);
                grid.DataTable.Clear();

                form.enabledItems(false, lrs_BtnRemplzr);
                form.enabledItems(true, lrs_BtnAceptar);


                if (ls_contrlFs == "2")
                {

                    form.visibleItems(true, lrs_BtnDelt);
                    form.enabledItems(true, lrs_BtnDelt);

                    form.getButton(lrs_BtnDelt).Caption = "Eliminar Preliminar";

                    RSDocumentos lo_comPreliminares = lo_sunatSrvc.fn_getPreliminares(ls_perTributario);
                    lo_rSComprobantes = lo_comPreliminares;

                    form.getButton(lrs_BtnAceptar).Caption = "Registrar Preliminar";
                }
                else if (ls_contrlFs == "3")
                {
                    form.visibleItems(true, lrs_BtnDelt);
                    form.enabledItems(true, lrs_BtnDelt);
                    form.enabledItems(false, lrs_BtnAceptar, lrs_BtnRemplzr);

                    form.getButton(lrs_BtnDelt).Caption = "Eliminar Pre. Final";

                    RSDocumentos lo_comPreliminares = lo_sunatSrvc.fn_getPreFinales(ls_perTributario);
                    lo_rSComprobantes = lo_comPreliminares;
                    lo_rSComprobantes.registros = lo_rSComprobantes.comprobantes;
                }
                else if (ls_contrlFs == "1")
                {
                    lo_rSComprobantes = lo_sunatSrvc.fn_comprobantesSire(ls_perTributario);
                    form.getButton(lrs_BtnAceptar).Caption = "Aceptar Propuesta";
                }
                else if (Convert.ToInt32(ls_contrlFs) > 4)
                {
                    form.enabledItems(false, lrs_BtnAceptar, lrs_BtnRemplzr);

                    Global.go_sboApplictn.MessageBox("El periodo escogido se encuentra ahora en estado de Generación", 1, "Ok");
                    Global.go_sboApplictn.statusBarWarningMsg("El periodo escogido se encuentra ahora en estado de Generación");
                    return;
                }




                lo_comprobnts = lo_rSComprobantes.registros;
                if (lo_comprobnts == null)
                {
                    sboApplication.statusBarWarningMsg("No se ha encontrado información de comprobantes por ERROR en SUNAT, intenta eliminar preliminar y hacer de nuevo la operación");
                    return;
                }
                if (lo_comprobnts.Count < 1)
                {
                    sboApplication.statusBarWarningMsg("No se ha encontrado información de comprobantes de pago en Periodo Seleccionado");
                    return;
                }

                sb_setGrilla();

                for (int i = 0; i < lo_comprobnts.Count; i++)
                {
  
                        grid.DataTable.Rows.Add();
                        grid.DataTable.SetValue(lrs_ClmEmision, i, Convert.ToDateTime(lo_comprobnts[i].fecEmision));
                        grid.DataTable.SetValue(lrs_ClmTipoCp, i, lo_comprobnts[i].codTipoCDP);
                        grid.DataTable.SetValue(lrs_ClmSerieCp, i, lo_comprobnts[i].numSerieCDP);
                        grid.DataTable.SetValue(lrs_ClmNroCp, i, lo_comprobnts[i].numCDP);
                        grid.DataTable.SetValue(lrs_ClmRznSoc, i, lo_comprobnts[i].nomRazonSocialCliente);
                        grid.DataTable.SetValue(lrs_ClmTipoMode, i, lo_comprobnts[i].codMoneda == null ? "" : lo_comprobnts[i].codMoneda);
                        grid.DataTable.SetValue(lrs_ClmMtoBIGrv, i, lo_comprobnts[i].mtoBIGravada);
                        grid.DataTable.SetValue(lrs_ClmIgv, i, lo_comprobnts[i].mtoIGV);
                        grid.DataTable.SetValue(lrs_ClmTotal, i, ls_contrlFs == "2" ? lo_comprobnts[i].montos.mtoTotalCP.ToString("F2") : lo_comprobnts[i].mtoTotalCP.ToString("F2"));
                        grid.DataTable.SetValue(lrs_ClmDocReferencia, i, SIREMod.fn_comprobanteModificado(lo_comprobnts[i].documentoMod));

                }

                grid.AutoResizeColumns();

                grid.editableGridColumns(false, lrs_ClmEmision, lrs_ClmTipoCp, lrs_ClmSerieCp, lrs_ClmNroCp, lrs_ClmTotal, lrs_ClmTipoMode,
                    lrs_ClmRznSoc, lrs_ClmMtoBIGrv, lrs_ClmIgv, lrs_ClmDocReferencia);

                grid.getEditTextColumn(lrs_ClmEmision).TitleObject.Sortable = true;
                grid.getEditTextColumn(lrs_ClmTotal).TitleObject.Sortable = true;

               // double ls_total = ls_contrlFs == "1" ? ((double)lo_rSComprobantes.totales.mtoSumTotalCP) : (double)lo_rSComprobantes.totales.mtoTotalCP;
                double ls_total = lo_rSComprobantes.totales.mtoSumTotalCP == null ? (double)lo_rSComprobantes.totales.mtoTotalCP : (double)lo_rSComprobantes.totales.mtoSumTotalCP;


                string ls_totalFor = ls_total.ToString("C", CultureInfo.CreateSpecificCulture("es-PE"));


                form.getEditText(lrs_EdtTotlSl).Value = ls_totalFor;
                form.getEditText(lrs_EdtTotlSl).Active = false;
                form.getEditText(lrs_EdtTotlSl).Item.Visible = true;
                form.getItem(lrs_EdtTotlSl).Enabled = false;
                form.getEditText(lrs_EdtCntdCp).Value = lo_rSComprobantes.registros.Count.ToString();


                sb_cleanRows();
                if (lo_rSComprobantes.registros.Count < 1)
                    sboApplication.statusBarWarningMsg("No se tiene data del periodo seleccionado");

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                form.Freeze(false);
            }
        }
        public void sb_setDatosSap()
        {
            try
            {

                RSDocumentos lo_rSCpSap = new RSDocumentos();
                lot_cpsSap = new List<Registros>();

                form.Freeze(true);


                string ls_perTributario = ldc_dataMes[form.getCombobox(lrs_CmbMeses).Value];
                grid = form.getGrid(lrs_GrdVentas);
                grid.DataTable.Clear();

                lo_rSCpSap = lo_sunatSrvc.fn_comprobantesSap(ls_perTributario);

                sb_faseAsignado(lo_sunatSrvc.fn_getProcesoFase(ls_perTributario));

                lot_cpsSap = lo_rSCpSap.registros;

                form.enabledItems(false, lrs_BtnAceptar);
                form.enabledItems(ls_contrlFs == "1", lrs_BtnRemplzr);

                if (lot_cpsSap.Count < 1)
                    throw new Exception("No se ha encontrado información de comprobantes de pago en Periodo Seleccionado");

                sb_setGrilla();

                for (int i = 0; i < lot_cpsSap.Count; i++)
                {
                    grid.DataTable.Rows.Add();
                    grid.DataTable.SetValue(lrs_ClmEmision, i, Convert.ToDateTime(lot_cpsSap[i].fecEmision));
                    grid.DataTable.SetValue(lrs_ClmTipoCp, i, lot_cpsSap[i].codTipoCDP);
                    grid.DataTable.SetValue(lrs_ClmTpCpSpR, i, lot_cpsSap[i].codTipoCDP);
                    grid.DataTable.SetValue(lrs_ClmSerieCp, i, lot_cpsSap[i].numSerieCDP);
                    grid.DataTable.SetValue(lrs_ClmNroCp, i, lot_cpsSap[i].numCDP);
                    grid.DataTable.SetValue(lrs_ClmRznSoc, i, lot_cpsSap[i].nomRazonSocialCliente);
                    grid.DataTable.SetValue(lrs_ClmTipoMode, i, lot_cpsSap[i].codMoneda);
                    grid.DataTable.SetValue(lrs_ClmMtoBIGrv, i, lot_cpsSap[i].mtoBIGravada);
                    grid.DataTable.SetValue(lrs_ClmIgv, i, lot_cpsSap[i].mtoIGV);
                    grid.DataTable.SetValue(lrs_ClmTotal, i, lot_cpsSap[i].mtoTotalCP.ToString("F2"));
                    grid.DataTable.SetValue(lrs_ClmObjType, i, lot_cpsSap[i].ObjectType);
                    grid.DataTable.SetValue(lrs_ClmDocEntry, i, lot_cpsSap[i].DocumentEntry);
                    grid.DataTable.SetValue(lrs_ClmDocReferencia, i, SIREMod.fn_comprobanteModificado(lot_cpsSap[i].documentoMod));
                }

                grid.AutoResizeColumns();

                grid.editableGridColumns(false, lrs_ClmEmision, lrs_ClmTipoCp, lrs_ClmSerieCp, lrs_ClmNroCp, lrs_ClmTotal,
                    lrs_ClmTipoMode, lrs_ClmRznSoc, lrs_ClmMtoBIGrv, lrs_ClmIgv, lrs_ClmDocReferencia);

                grid.visibleGridColumns(false, lrs_ClmObjType, lrs_ClmDocEntry, lrs_ClmTpCpSpR);

                grid.getEditTextColumn(lrs_ClmTipoCp).LinkedObjectType = "2";

                grid.getEditTextColumn(lrs_ClmEmision).TitleObject.Sortable = true;
                grid.getEditTextColumn(lrs_ClmTotal).TitleObject.Sortable = true;

                double ls_total = (double)lo_rSCpSap.totales.mtoSumTotalCP;
                string ls_totalFor = ls_total.ToString("C", CultureInfo.CreateSpecificCulture("es-PE"));

                form.getEditText(lrs_EdtTotlSl).Value = ls_totalFor;
                form.getEditText(lrs_EdtTotlSl).Active = false;
                form.getEditText(lrs_EdtTotlSl).Item.Visible = true;

                form.getItem(lrs_EdtTotlSl).Enabled = false;

                form.getEditText(lrs_EdtCntdCp).Value = lo_rSCpSap.registros.Count.ToString();

                grid.AutoResizeColumns();

                sb_cleanRows();


            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                form.Freeze(false);
            }
        }
        public void sb_setDatosComparar()
        {

            try
            {
                RSDocumentos lo_rSCpSire = new RSDocumentos();
                RSDocumentos lo_rSCpSap = new RSDocumentos();
                List<Registros> lot_cpsSire = new List<Registros>();
                lot_cpsSap = new List<Registros>();

                form.Freeze(true);

                string ls_perTributario = ldc_dataMes[form.getCombobox(lrs_CmbMeses).Value];

                grid = form.getGrid(lrs_GrdVentas);
                grid.DataTable.Clear();

                form.visibleItems(false, lrs_LblFase);

                lo_rSCpSire = lo_sunatSrvc.fn_comprobantesSire(ls_perTributario);
                lo_rSCpSap = lo_sunatSrvc.fn_comprobantesSap(ls_perTributario);

                lot_cpsSire = lo_rSCpSire.registros;
                lot_cpsSap = lo_rSCpSap.registros;

                sb_setGrilla();

                double ld_totalSire = lo_rSCpSire.totales.mtoTotalCP == null ? (double)lo_rSCpSire.totales.mtoSumTotalCP : (double)lo_rSCpSire.totales.mtoTotalCP;
                double ld_totalSap = (double)lo_rSCpSap.totales.mtoSumTotalCP;

                string ld_ttSireSol = ld_totalSire.ToString("C", CultureInfo.CreateSpecificCulture("es-PE"));
                string ld_ttSapSol = ld_totalSap.ToString("C", CultureInfo.CreateSpecificCulture("es-PE")); ;


                form.getEditText(lrs_EdtCantCpSr).Value = lot_cpsSire.Count.ToString();
                form.getEditText(lrs_EdtCantCpSr).Active = false;
                form.Items.Item(lrs_EdtCantCpSr).Enabled = false;

                form.getEditText(lrs_EdtCntddSp).Value = lot_cpsSap.Count.ToString();
                form.getEditText(lrs_EdtCntddSp).Active = false;
                form.Items.Item(lrs_EdtCntddSp).Enabled = false;

                form.getEditText(lrs_EdtTotalSolSr).Value = ld_ttSireSol;
                form.getEditText(lrs_EdtTotalSolSr).Active = false;
                form.Items.Item(lrs_EdtTotalSolSr).Enabled = false;

                form.getEditText(lrs_EdtTtlSlSp).Value = ld_ttSapSol;
                form.getEditText(lrs_EdtTtlSlSp).Active = false;
                form.Items.Item(lrs_EdtTtlSlSp).Enabled = false;

                /* int cantidadSap = rSCpSap.comprobantes.Count;
                 int cantidadSir = rSCpSire.comprobantes.Count;
                 int cont = cantidadSap > cantidadSir ? cantidadSap : cantidadSir;
                */

                //List<Dictionary<string,string>> valueCom = new List<Dictionary<string,string>>();
                List<List<string>> lst_listaDif = new List<List<string>>();

                for (int i = 0; i < lot_cpsSap.Count; i++)
                {

                    List<string> lst_data = new List<string>();

                    //Dictionary<string,string> data =  new Dictionary<string, string>();-
                    Registros lo_comSap = lot_cpsSap[i];
                    Registros lo_comSire = new Registros();

                    lo_comSire = lot_cpsSire.FirstOrDefault(cpU => cpU.codTipoCDP == lo_comSap.codTipoCDP & cpU.numSerieCDP == lo_comSap.numSerieCDP &
                        string.Format(cpU.numCDP).Trim('0') == string.Format(lo_comSap.numCDP).Trim('0'));

                    lst_data.Add(lo_comSap.fecEmision);
                    lst_data.Add(lo_comSap.codTipoCDP);
                    lst_data.Add(lo_comSap.numSerieCDP);
                    lst_data.Add(lo_comSap.numCDP);
                    lst_data.Add(lo_comSap.nomRazonSocialCliente);
                    lst_data.Add(lo_comSap.codMoneda);
                    lst_data.Add(lo_comSap.mtoBIGravada.ToString("F2"));
                    lst_data.Add(lo_comSap.mtoIGV.ToString("F2"));
                    lst_data.Add(lo_comSap.mtoTotalCP.ToString("F2"));
                    lst_data.Add(lo_comSap.ObjectType);
                    lst_data.Add(lo_comSap.DocumentEntry);

                    lst_data.Add(lo_comSire == null ? string.Empty : lo_comSire.fecEmision);
                    lst_data.Add(lo_comSire == null ? string.Empty : lo_comSire.codTipoCDP);
                    lst_data.Add(lo_comSire == null ? string.Empty : lo_comSire.numSerieCDP);
                    lst_data.Add(lo_comSire == null ? string.Empty : lo_comSire.numCDP);
                    lst_data.Add(lo_comSire == null ? string.Empty : lo_comSire.nomRazonSocialCliente);
                    lst_data.Add(lo_comSire == null ? string.Empty : lo_comSire.codMoneda);
                    lst_data.Add(lo_comSire == null ? string.Empty : lo_comSire.mtoBIGravada.ToString("F2"));
                    lst_data.Add(lo_comSire == null ? string.Empty : lo_comSire.mtoIGV.ToString("F2"));
                    lst_data.Add(lo_comSire == null ? string.Empty : lo_comSire.mtoTotalCP.ToString("F2"));

                    lst_listaDif.Add(lst_data);

                }

                for (int i = 0; i < lot_cpsSire.Count; i++)
                {
                    bool datoEncontrado = false;

                    // Recorre las listas internas de lst_listaDif
                    for (int j = 0; j < lst_listaDif.Count; j++)
                    {
                        var ls_tipoCp = lst_listaDif[j][12];
                        var ls_serie = lst_listaDif[j][13];
                        var ls_correlativo = lst_listaDif[j][14];

                        // Compara los datos
                        if (lot_cpsSire[i].codTipoCDP == ls_tipoCp && lot_cpsSire[i].numSerieCDP == ls_serie && lot_cpsSire[i].numCDP == ls_correlativo)
                        {
                            datoEncontrado = true;
                            break; // Si encuentra coincidencia, no es necesario seguir buscando en lst_listaDif
                        }
                    }

                    // Si no se encuentra el dato en lst_listaDif, agrégalo a listaDiferencias
                    if (!datoEncontrado)
                    {
                        List<string> lst_data = new List<string>
                    {
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,

                        lot_cpsSire[i].fecEmision,
                        lot_cpsSire[i].codTipoCDP,
                        lot_cpsSire[i].numSerieCDP,
                        lot_cpsSire[i].numCDP,
                        lot_cpsSire[i].nomRazonSocialCliente,
                        lot_cpsSire[i].codMoneda,
                        lot_cpsSire[i].mtoBIGravada.ToString("F2"),
                        lot_cpsSire[i].mtoIGV.ToString("F2"),
                        lot_cpsSire[i].mtoTotalCP.ToString("F2")
                    };

                        lst_listaDif.Add(lst_data);

                    }
                }


                for (int i = 0; i < lst_listaDif.Count; i++)
                {

                    try
                    {
                        grid.DataTable.Rows.Add();
                        grid.DataTable.SetValue(lrs_ClmEmisionSap, i, lst_listaDif[i][0] == string.Empty ? string.Empty : Convert.ToDateTime(lst_listaDif[i][0]).ToString("dd/MM/yyyy"));
                        grid.DataTable.SetValue(lrs_ClmTipoCpSap, i, lst_listaDif[i][1]);
                        grid.DataTable.SetValue(lrs_ClmTpCpSpR, i, lst_listaDif[i][1]);
                        grid.DataTable.SetValue(lrs_ClmSerieCpSap, i, lst_listaDif[i][2]);
                        grid.DataTable.SetValue(lrs_ClmNroCpSap, i, lst_listaDif[i][3]);
                        grid.DataTable.SetValue(lrs_ClmRznSoc, i, lst_listaDif[i][4]);
                        grid.DataTable.SetValue(lrs_ClmTipMdSp, i, lst_listaDif[i][5]);
                        grid.DataTable.SetValue(lrs_ClmMtoBIGrv, i, lst_listaDif[i][6]);
                        grid.DataTable.SetValue(lrs_ClmIgv, i, lst_listaDif[i][7]);
                        grid.DataTable.SetValue(lrs_ClmTtlSpC, i, lst_listaDif[i][8]);
                        grid.DataTable.SetValue(lrs_ClmObjType, i, lst_listaDif[i][9]);
                        grid.DataTable.SetValue(lrs_ClmDocEntry, i, lst_listaDif[i][10]);

                        grid.DataTable.SetValue("|", i, string.Empty);

                        grid.DataTable.SetValue(lrs_ClmEmisnSr, i, lst_listaDif[i][11] == string.Empty ? string.Empty : Convert.ToDateTime(lst_listaDif[i][11]).ToString("dd/MM/yyyy"));
                        grid.DataTable.SetValue(lrs_ClmTipoCpSr, i, lst_listaDif[i][12]);
                        grid.DataTable.SetValue(lrs_ClmSerieCpSr, i, lst_listaDif[i][13]);
                        grid.DataTable.SetValue(lrs_ClmNroCpSr, i, lst_listaDif[i][14]);
                        grid.DataTable.SetValue(lrs_ClmRznSocSr, i, lst_listaDif[i][15]);
                        grid.DataTable.SetValue(lrs_ClmTipoMdSr, i, lst_listaDif[i][16] == null ? "" : lst_listaDif[i][16]);
                        grid.DataTable.SetValue(lrs_ClmMtoBIGrvSr, i, lst_listaDif[i][17]);
                        grid.DataTable.SetValue(lrs_ClmIgvSr, i, lst_listaDif[i][18]);
                        grid.DataTable.SetValue(lrs_ClmTotalSr, i, lst_listaDif[i][19]);

                    }
                    catch (Exception)
                    {

                    }

                }

                /*
                grid.getEditTextColumn(lrs_ClmEmisionSap).TitleObject.Sortable = true;
                grid.getEditTextColumn(lrs_ClmTotalSrC).TitleObject.Sortable = true;
                */
                grid.editableGridColumns(false, lrs_ClmEmisionSap, lrs_ClmTipoCpSap, lrs_ClmSerieCpSap, lrs_ClmNroCpSap, lrs_ClmTipMdSp,
                    lrs_ClmTtlSpC, lrs_ClmTpCpSpR, lrs_ClmObjType, lrs_ClmDocEntry, lrs_ClmRznSoc, lrs_ClmRznSocSr, lrs_ClmIgv, lrs_ClmIgvSr,
                    lrs_ClmMtoBIGrv, lrs_ClmMtoBIGrvSr, lrs_ClmEmisnSr, lrs_ClmTipoCpSr, lrs_ClmSerieCpSr, lrs_ClmNroCpSr, lrs_ClmTipoMdSr, lrs_ClmTotalSr);

                grid.visibleGridColumns(false, lrs_ClmTpCpSpR, lrs_ClmObjType, lrs_ClmDocEntry);

                //form.enabledItems(false, lrs_EdtCantCpSr, lrs_EdtCntddSp, lrs_EdtTotalSolSr, lrs_EdtTtlSlSp);

                grid.getEditTextColumn(lrs_ClmTipoCpSap).LinkedObjectType = "2";

                grid.Columns.Item("|").Editable = false;

                GridColumns columns = grid.Columns;
                GridColumn col = columns.Item(12);
                col.TitleObject.Caption = string.Empty;


                grid.AutoResizeColumns();
                sb_cleanRows();
                sb_paintGrid();


            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                form.Freeze(false);
            }
        }
        public void sb_paintGrid()
        {

            grid = form.Items.Item(lrs_GrdVentas).Specific;

            for (int i = 0; i < grid.Rows.Count; i++)
            {
                if (string.Format(grid.DataTable.GetValue(lrs_ClmNroCpSap, i)).Trim('0') == string.Format(grid.DataTable.GetValue(lrs_ClmNroCpSr, i)).Trim('0') &
                  grid.DataTable.GetValue(lrs_ClmTipoCpSap, i) == grid.DataTable.GetValue(lrs_ClmTipoCpSr, i) &
                  grid.DataTable.GetValue(lrs_ClmSerieCpSap, i) == grid.DataTable.GetValue(lrs_ClmSerieCpSr, i))
                {
                    if (grid.DataTable.GetValue(lrs_ClmTtlSpC, i) == grid.DataTable.GetValue(lrs_ClmTotalSrC, i))
                    {
                        grid.CommonSetting.SetRowBackColor(i + 1, SIREMod.fn_getRgb(198, 255, 168));
                        grid.CommonSetting.SetCellBackColor(i + 1, 13, SIREMod.fn_getRgb(231, 231, 231));
                    }
                    else
                    {
                        grid.CommonSetting.SetRowBackColor(i + 1, SIREMod.fn_getRgb(242, 249, 144));
                        grid.CommonSetting.SetCellBackColor(i + 1, 22, SIREMod.fn_getRgb(249, 241, 32));
                        grid.CommonSetting.SetCellBackColor(i + 1, 10, SIREMod.fn_getRgb(249, 241, 32));
                        grid.CommonSetting.SetCellBackColor(i + 1, 13, SIREMod.fn_getRgb(231, 231, 231));
                        //11 y 22 son los que pinta la celda
                    }
                }
                else if (grid.DataTable.GetValue(lrs_ClmTipoCpSr, i) == string.Empty)
                {
                    grid.CommonSetting.SetCellBackColor(i + 1, 13, SIREMod.fn_getRgb(231, 231, 231));
                    grid.CommonSetting.SetCellBackColor(i + 1, 14, SIREMod.fn_getRgb(250, 136, 136));
                    grid.CommonSetting.SetCellBackColor(i + 1, 15, SIREMod.fn_getRgb(250, 136, 136));
                    grid.CommonSetting.SetCellBackColor(i + 1, 16, SIREMod.fn_getRgb(250, 136, 136));
                    grid.CommonSetting.SetCellBackColor(i + 1, 17, SIREMod.fn_getRgb(250, 136, 136));
                    grid.CommonSetting.SetCellBackColor(i + 1, 18, SIREMod.fn_getRgb(250, 136, 136));
                    grid.CommonSetting.SetCellBackColor(i + 1, 19, SIREMod.fn_getRgb(250, 136, 136));
                    grid.CommonSetting.SetCellBackColor(i + 1, 20, SIREMod.fn_getRgb(250, 136, 136));
                    grid.CommonSetting.SetCellBackColor(i + 1, 21, SIREMod.fn_getRgb(250, 136, 136));
                    grid.CommonSetting.SetCellBackColor(i + 1, 22, SIREMod.fn_getRgb(250, 136, 136));
                }
                else if (grid.DataTable.GetValue(lrs_ClmTipoCpSap, i) == string.Empty)
                {
                    //   grid.CommonSetting.SetCellBackColor(i + 1, 0, SIREMod.fn_getRgb(231, 231, 231));
                    // grid.CommonSetting.SetRowBackColor(i + 1, SIREMod.fn_getRgb(231, 231, 231));
                    grid.CommonSetting.SetCellBackColor(i + 1, 1, SIREMod.fn_getRgb(250, 136, 136));
                    grid.CommonSetting.SetCellBackColor(i + 1, 2, SIREMod.fn_getRgb(250, 136, 136));
                    grid.CommonSetting.SetCellBackColor(i + 1, 3, SIREMod.fn_getRgb(250, 136, 136));
                    grid.CommonSetting.SetCellBackColor(i + 1, 4, SIREMod.fn_getRgb(250, 136, 136));
                    grid.CommonSetting.SetCellBackColor(i + 1, 5, SIREMod.fn_getRgb(250, 136, 136));
                    grid.CommonSetting.SetCellBackColor(i + 1, 6, SIREMod.fn_getRgb(250, 136, 136));
                    grid.CommonSetting.SetCellBackColor(i + 1, 7, SIREMod.fn_getRgb(250, 136, 136));
                    grid.CommonSetting.SetCellBackColor(i + 1, 8, SIREMod.fn_getRgb(250, 136, 136));
                    grid.CommonSetting.SetCellBackColor(i + 1, 9, SIREMod.fn_getRgb(250, 136, 136));
                    grid.CommonSetting.SetCellBackColor(i + 1, 10, SIREMod.fn_getRgb(250, 136, 136));
                    grid.CommonSetting.SetCellBackColor(i + 1, 11, SIREMod.fn_getRgb(250, 136, 136));
                }
                // grid.CommonSetting.SetCellBackColor(i + 1, 10, SIREMod.fn_getRgb(250, 250, 250));
            }
        }
        public void sb_cleanRows()
        {
            for (int i = 0; i < grid.Rows.Count; i++)
            {
                grid.CommonSetting.SetRowBackColor(i + 1, SIREMod.fn_getRgb(231, 231, 231));
            }
        }
        public void sb_reemplazoPropuesta()
        {
            try
            {
                HttpResponseMessage lo_responseRegistro = new HttpResponseMessage();
                string ls_perTributario = ldc_dataMes[form.getCombobox(lrs_CmbMeses).Value];
                Registro lo_response = lo_sunatSrvc.fn_descargaPSap(ls_perTributario, "Envios", true);

                if (lo_response.codProceso == "3" | lo_response.codProceso == "4")
                {
                    var ls_carpeta = lo_response.codProceso == "4" ? "Aceptados" : "Aceptados - Observados";

                    Global.go_sboApplictn.MessageBox($"La propuesta de remplazo se registro con ticket: {lo_response.numTicket} y fue {(lo_response.codProceso == "4" ? "Aceptado exitosamente" : "Aceptado con Observaciones")}", 1, "Ok");

                    lo_sunatSrvc.sb_descargaArchivo(lo_response.nomArchivoImportacion, ls_carpeta, false);
                    sboApplication.statusBarWarningMsg($"Se guardo el detalle en la carpeta {ls_rutArchv}\\{ls_carpeta}\\{lo_response.nomArchivoImportacion}");

                    lo_responseRegistro = lo_sunatSrvc.fn_registrarPreliminar(ls_perTributario);

                    if (lo_responseRegistro.IsSuccessStatusCode)
                    {
                        sboApplication.statusBarSuccessMsg($"Se Registró preliminar correctamente");
                    }
                    else
                        sboApplication.statusBarErrorMsg("No se pudo aceptar propuesta: " + lo_responseRegistro.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public void sb_exportDocuments()
        {
            bool lb_sap = form.getOptionBtn(lrs_RbnSp).Selected;
            bool lb_sire = form.getOptionBtn(lrs_RbnSr).Selected;
            bool lb_dif = form.getOptionBtn(lrs_RbnDf).Selected;

            string ls_perTributario = ldc_dataMes[form.getCombobox(lrs_CmbMeses).Value];
            if (lb_sap)
            {
                Registro lo_registro = lo_sunatSrvc.fn_descargaPSap(ls_perTributario, "Exportaciones\\SAP", false);
                sboApplication.statusBarSuccessMsg("Exportado exitosamente en la carpeta: " + lo_registro.nomArchivoImportacion);
                Global.go_sboApplictn.MessageBox($"El archivo fue exportado exitosamente en la ruta: {ls_rutArchv + "\\Exportaciones\\SAP"}", 1, "Ok");
            }
            else if (lb_sire)
            {
                lo_sunatSrvc.sb_descargaPropuesta(ls_perTributario, "Exportaciones\\SIRE");
                Global.go_sboApplictn.MessageBox($"El archivo fue exportado exitosamente en la ruta: {ls_rutArchv + "\\Exportaciones\\SIRE"}", 1, "Ok");
            }
            else if (lb_dif)
            {

                SIREMod.sb_exportCsv(form.getGrid(lrs_GrdVentas), "Exportaciones\\Comparaciones", ls_perTributario, ls_rutArchv);
                Global.go_sboApplictn.MessageBox($"El archivo fue exportado exitosamente en la ruta: {ls_rutArchv + "\\Exportaciones\\Comparaciones"}", 1, "Ok");

            }
        }
        public void sb_setGrilla()
        {

            bool lb_sap = form.getOptionBtn(lrs_RbnSp).Selected;
            bool lb_dif = form.getOptionBtn(lrs_RbnDf).Selected;
            // Sire

            if (!lb_dif)
            {
                grid.DataTable.Columns.Add(lrs_ClmEmision, BoFieldsType.ft_Date);
                grid.DataTable.Columns.Add(lrs_ClmTipoCp, BoFieldsType.ft_AlphaNumeric, 20);
                grid.DataTable.Columns.Add(lrs_ClmSerieCp, BoFieldsType.ft_AlphaNumeric, 50);
                grid.DataTable.Columns.Add(lrs_ClmNroCp, BoFieldsType.ft_AlphaNumeric, 50);
                grid.DataTable.Columns.Add(lrs_ClmRznSoc, BoFieldsType.ft_AlphaNumeric, 200);
                grid.DataTable.Columns.Add(lrs_ClmTipoMode, BoFieldsType.ft_AlphaNumeric, 50);
                grid.DataTable.Columns.Add(lrs_ClmMtoBIGrv, BoFieldsType.ft_Float, 50);
                grid.DataTable.Columns.Add(lrs_ClmIgv, BoFieldsType.ft_Float, 50);
                grid.DataTable.Columns.Add(lrs_ClmTotal, BoFieldsType.ft_Float, 50);
                grid.DataTable.Columns.Add(lrs_ClmDocReferencia, BoFieldsType.ft_AlphaNumeric, 30);

                // Sap
                if (lb_sap)
                {
                    grid.DataTable.Columns.Add(lrs_ClmTpCpSpR, BoFieldsType.ft_AlphaNumeric, 20);
                    grid.DataTable.Columns.Add(lrs_ClmObjType, BoFieldsType.ft_AlphaNumeric, 20);
                    grid.DataTable.Columns.Add(lrs_ClmDocEntry, BoFieldsType.ft_AlphaNumeric, 20);
                }
            }
            else
            {

                grid.DataTable.Columns.Add(lrs_ClmEmisionSap, BoFieldsType.ft_AlphaNumeric, 20);
                grid.DataTable.Columns.Add(lrs_ClmTipoCpSap, BoFieldsType.ft_AlphaNumeric, 20);
                grid.DataTable.Columns.Add(lrs_ClmTpCpSpR, BoFieldsType.ft_AlphaNumeric, 20);
                grid.DataTable.Columns.Add(lrs_ClmSerieCpSap, BoFieldsType.ft_AlphaNumeric, 50);
                grid.DataTable.Columns.Add(lrs_ClmNroCpSap, BoFieldsType.ft_AlphaNumeric, 50);
                grid.DataTable.Columns.Add(lrs_ClmRznSoc, BoFieldsType.ft_AlphaNumeric, 200);
                grid.DataTable.Columns.Add(lrs_ClmTipMdSp, BoFieldsType.ft_AlphaNumeric, 50);
                grid.DataTable.Columns.Add(lrs_ClmMtoBIGrv, BoFieldsType.ft_AlphaNumeric, 50);
                grid.DataTable.Columns.Add(lrs_ClmIgv, BoFieldsType.ft_AlphaNumeric, 50);
                grid.DataTable.Columns.Add(lrs_ClmTtlSpC, BoFieldsType.ft_AlphaNumeric, 50);
                grid.DataTable.Columns.Add(lrs_ClmObjType, BoFieldsType.ft_AlphaNumeric, 20);
                grid.DataTable.Columns.Add(lrs_ClmDocEntry, BoFieldsType.ft_AlphaNumeric, 20);


                grid.DataTable.Columns.Add("|", BoFieldsType.ft_AlphaNumeric, 50);

                grid.DataTable.Columns.Add(lrs_ClmEmisnSr, BoFieldsType.ft_AlphaNumeric, 50);
                grid.DataTable.Columns.Add(lrs_ClmTipoCpSr, BoFieldsType.ft_AlphaNumeric, 20);
                grid.DataTable.Columns.Add(lrs_ClmSerieCpSr, BoFieldsType.ft_AlphaNumeric, 50);
                grid.DataTable.Columns.Add(lrs_ClmNroCpSr, BoFieldsType.ft_AlphaNumeric, 50);
                grid.DataTable.Columns.Add(lrs_ClmRznSocSr, BoFieldsType.ft_AlphaNumeric, 200);
                grid.DataTable.Columns.Add(lrs_ClmTipoMdSr, BoFieldsType.ft_AlphaNumeric, 50);
                grid.DataTable.Columns.Add(lrs_ClmMtoBIGrvSr, BoFieldsType.ft_AlphaNumeric, 50);
                grid.DataTable.Columns.Add(lrs_ClmIgvSr, BoFieldsType.ft_AlphaNumeric, 50);
                grid.DataTable.Columns.Add(lrs_ClmTotalSr, BoFieldsType.ft_AlphaNumeric, 50);
            }


        }
        public void sb_orderGrid(string filtro)
        {

            try
            {
                if (filtro == "Todos")
                {
                    sb_setDatosComparar();
                }
                else if (filtro == "Incongruencia")
                {

                    form.Freeze(true);
                    sb_setDatosComparar();
                    grid = form.getGrid(lrs_GrdVentas);
                    for (int i = 0; i < grid.Rows.Count; i++)
                    {
                        for (int c = 0; c < grid.Columns.Count; c++)
                        {
                            if (grid.CommonSetting.GetCellBackColor(i + 1, c + 1) == SIREMod.fn_getRgb(250, 136, 136) |
                             grid.CommonSetting.GetCellBackColor(i + 1, c + 1) == SIREMod.fn_getRgb(198, 255, 168))
                            {
                                grid.DataTable.Rows.Remove(i);
                                i--;
                                sb_cleanRows();
                                sb_paintGrid();
                                break;
                            }
                        }
                    }
                    /*  sb_cleanRows();
                      sb_paintGrid();*/
                    form.Freeze(false);
                }
                else if (filtro == "Faltantes")
                {
                    form.Freeze(true);
                    sb_setDatosComparar();
                    grid = form.getGrid(lrs_GrdVentas);
                    for (int i = 0; i < grid.Rows.Count; i++)
                    {

                        for (int c = 0; c < grid.Columns.Count; c++)
                        {
                            if (grid.CommonSetting.GetCellBackColor(i + 1, c + 1) == SIREMod.fn_getRgb(198, 255, 168) |
                                grid.CommonSetting.GetCellBackColor(i + 1, 2) == SIREMod.fn_getRgb(242, 249, 32))
                            {
                                grid.DataTable.Rows.Remove(i);
                                i--;
                                sb_cleanRows();
                                sb_paintGrid();
                                break;
                            }

                        }
                    }
                    form.Freeze(false);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool fn_verificaRespuesta()
        {

            string ls_perTri = ldc_dataMes[form.getCombobox(lrs_CmbMeses).Value];

            var ls_response = SIREMod.fn_verificaPropuesta(lo_sunatSrvc.ls_token, ls_perTri);

            if (ls_response.IsSuccessStatusCode)
            {
                return true;

            }
            else
            {
                RSerror rSerror = System.Text.Json.JsonSerializer.Deserialize<RSerror>(ls_response.Content.ReadAsStringAsync().Result);

                Global.go_sboApplictn.MessageBox("No se pudo continuar con la aceptación o reemplazo: " + rSerror.errors[0].msg, 1, "Volver");
                Global.go_sboApplictn.statusBarErrorMsg("No se pudo continuar con la aceptación o reemplazo: " + rSerror.errors[0].msg);

                return false;
            }

        }
        protected override void loadDataActions()
        {

            this.Add(new ItemAction(BoEventTypes.et_COMBO_SELECT, lrs_CmbAnio, e =>
            {
                if (!e.BeforeAction)
                {
                    form.enabledItems(true, lrs_CmbMeses);
                    sb_getMesesLoad();
                }
            }));

            this.Add(new ItemAction(BoEventTypes.et_MATRIX_LINK_PRESSED, lrs_GrdVentas, e =>
            {
                grid = form.getGrid(lrs_GrdVentas);
                if (e.BeforeAction)
                {

                    form.Freeze(true);

                    var lo_visualRowIndex = grid.GetDataTableRowIndex(e.Row);
                    string ls_docE = grid.DataTable.GetValue(lrs_ClmDocEntry, lo_visualRowIndex);

                    grid.DataTable.SetValue(e.ColUID, lo_visualRowIndex, ls_docE);

                    var ls_objType = grid.DataTable.GetValue(lrs_ClmObjType, lo_visualRowIndex);
                    var ls_doc = grid.DataTable.GetValue(e.ColUID, lo_visualRowIndex);

                    // Obtener el índice visual de la fila seleccionada


                    // Utilizar el DocEntry y el ObjectType para abrir el formulario

                    grid.getEditTextColumn(e.ColUID).LinkedObjectType = ls_objType;


                }
                if (!e.BeforeAction)
                {

                    var lo_visualRowIndex = grid.GetDataTableRowIndex(e.Row);

                    grid.DataTable.SetValue(e.ColUID, lo_visualRowIndex, grid.DataTable.GetValue(lrs_ClmTpCpSpR, lo_visualRowIndex));
                    form.Freeze(false);

                }
            })
            {

            });

            this.Add(new ItemAction(BoEventTypes.et_ITEM_PRESSED, lrs_BtnAceptar, e =>
            {
                if (!e.BeforeAction)
                {
                    if (Global.go_sboApplictn.MessageBox($"¿Estás seguro que deseas {(ls_contrlFs == "2" ? "Registrar el preliminar" : "Aceptar la propuesta")}?", 1, "Si", "No") == 1)
                    {

                        if (fn_verificaRespuesta())
                        {
                            string ls_perTri = ldc_dataMes[form.getCombobox(lrs_CmbMeses).Value];

                            HttpResponseMessage lo_response = new HttpResponseMessage();

                            if (ls_contrlFs == "2")
                                lo_response = lo_sunatSrvc.fn_registrarPreliminar(ls_perTri);
                            else
                            {
                                lo_response = lo_sunatSrvc.fn_aceptarPropuesta(ls_perTri);
                            }
                            if (lo_response.IsSuccessStatusCode)
                            {
                                sboApplication.statusBarSuccessMsg($"Se {(ls_contrlFs == "2" ? "Registró preliminar" : "Aceptó propuesta")} correctamente con Ticket: " +
                                    $"{(ls_contrlFs == "2" ? lo_response.Content.ReadAsStringAsync().Result : JsonSerializer.Deserialize<RSTicket>(lo_response.Content.ReadAsStringAsync().Result).numTicket)}");

                                Global.go_sboApplictn.MessageBox("Se registró Preliminar final correctamente", 1, "Ok");

                                if (ls_contrlFs == "1")
                                    sboApplication.statusBarWarningMsg("Se cambio el estado a Registrado");

                                form.Items.Item(lrs_BtnBuscar).Click();
                            }
                            else
                            {
                                sboApplication.statusBarErrorMsg("No se pudo aceptar propuesta: " + lo_response.Content.ReadAsStringAsync().Result);
                            }
                        }
                    }
                }
            }));
            this.Add(new ItemAction(BoEventTypes.et_ITEM_PRESSED, lrs_BtnFilt, e =>
            {
                if (!e.BeforeAction)
                {
                    SAPbouiCOM.ButtonCombo go_btn = form.Items.Item(lrs_BtnFilt).Specific;

                    if (go_btn.Selected != null)
                        sb_orderGrid(go_btn.Selected.Value.ToString());
                }
            }));
            this.Add(new ItemAction(BoEventTypes.et_CLICK, lrs_LblPortal, e =>
            {
                if (!e.BeforeAction)
                {
                    System.Diagnostics.Process.Start("https://api-seguridad.sunat.gob.pe/v1/clientessol/4f3b88b3-d9d6-402a-b85d-6a0bc857746a/oauth2/loginMenuSol");
                }
            }));
            this.Add(new ItemAction(BoEventTypes.et_ITEM_PRESSED, lrs_BtnBuscar, e =>
            {
                bool lb_sap = form.getOptionBtn(lrs_RbnSp).Selected;
                bool lb_sire = form.getOptionBtn(lrs_RbnSr).Selected;
                bool lb_dif = form.getOptionBtn(lrs_RbnDf).Selected;

                if (!e.BeforeAction)
                {

                    var ls_mesSelect = form.getCombobox(lrs_CmbMeses).Value;
                    if (!lb_sap & !lb_sire & !lb_dif)
                    {
                        sboApplication.statusBarErrorMsg("No has seleccionado el tipo de datos");
                    }
                    else if (string.IsNullOrEmpty(ls_mesSelect))
                    {
                        sboApplication.statusBarWarningMsg("No se ha seleccionado ningun mes");
                    }
                    else if (lb_dif)
                    {
                        form.enabledItems(false, lrs_BtnAceptar, lrs_BtnRemplzr);

                        form.enabledItems(true, lrs_EdtCntddSp, lrs_EdtTtlSlSp,
                         lrs_EdtCantCpSr, lrs_EdtTotalSolSr);

                        form.visibleItems(true, lrs_EdtCntddSp, lrs_EdtTtlSlSp, lrs_LblCntCpSp, lrs_LblTtlSolSp, lrs_LblSr, lrs_LblSp,
                         lrs_EdtCantCpSr, lrs_EdtTotalSolSr, lrs_LblCantCpSr, lrs_LblTotalSolSr, lrs_BtnFilt);

                        form.visibleItems(false, lrs_EdtCntdCp, lrs_EdtTotlSl, lrs_LblCntddCp, lrs_LblTotalSl, lrs_BtnDelt);



                        SAPbouiCOM.ButtonCombo lo_btn = form.Items.Item(lrs_BtnFilt).Specific;

                        if (lo_btn.ValidValues.Count == 0)
                        {
                            lo_btn.ValidValues.Add("Todos", "Todos los  comprobantes");
                            lo_btn.ValidValues.Add("Incongruencia", "Comprobantes con diferencia en el monto");
                            lo_btn.ValidValues.Add("Faltantes", "Comprobantes faltantes en sire o sap");
                        }

                        sb_setDatosComparar();
                    }
                    else
                    {
                        form.enabledItems(false, lrs_EdtCntddSp, lrs_EdtTtlSlSp,
                        lrs_EdtCantCpSr, lrs_EdtTotalSolSr);

                        form.visibleItems(false, lrs_EdtCntddSp, lrs_EdtTtlSlSp, lrs_LblCntCpSp, lrs_LblTtlSolSp, lrs_LblSr, lrs_LblSp,
                         lrs_EdtCantCpSr, lrs_EdtTotalSolSr, lrs_LblCantCpSr, lrs_LblTotalSolSr, lrs_BtnDelt, lrs_BtnFilt);

                        form.visibleItems(true, lrs_EdtCntdCp, lrs_EdtTotlSl, lrs_LblCntddCp, lrs_LblTotalSl);

                        if (lb_sire)
                            sb_setDatosSire();
                        else
                            sb_setDatosSap();
                    }

                }
            }));
            this.Add(new ItemAction(BoEventTypes.et_ITEM_PRESSED, lrs_BtnDelt, e =>
            {
                if (!e.BeforeAction)
                {
                    string ls_perTri = ldc_dataMes[form.getCombobox(lrs_CmbMeses).Value];
                    if (Global.go_sboApplictn.MessageBox("¿Estás seguro que deseas eliminar propuesta o preliminar registrado?", 1, "Si", "No") == 1)
                    {

                        if (ls_contrlFs == "2")
                        {
                            var lo_response2 = lo_sunatSrvc.fn_eliminarPreliminar(ls_perTri);

                            if (lo_response2.IsSuccessStatusCode)
                            {
                                sboApplication.statusBarSuccessMsg("Preliminar eliminado exitosamente");
                                form.Items.Item(lrs_BtnBuscar).Click();
                            }
                            else
                                sboApplication.statusBarErrorMsg("No se pudo eliminar: " + lo_response2.Content.ReadAsStringAsync().Result);
                        }
                        else
                        {

                            var lo_response = lo_sunatSrvc.fn_eliminarPreFinal(ls_perTri);

                            ls_contrlFs = lo_sunatSrvc.fn_getProcesoFase(ls_perTri);

                            if (lo_response.IsSuccessStatusCode)
                            {
                                sboApplication.statusBarSuccessMsg("Registro eliminado exitosamente");
                                form.Items.Item(lrs_BtnBuscar).Click();
                            }
                            else
                                sboApplication.statusBarErrorMsg("No se pudo eliminar: " + lo_response.Content.ReadAsStringAsync().Result);

                            if (ls_contrlFs == "2")
                            {
                                var lo_response2 = lo_sunatSrvc.fn_eliminarPreliminar(ls_perTri);

                                if (lo_response2.IsSuccessStatusCode)
                                {
                                    sboApplication.statusBarSuccessMsg("Preliminar eliminado exitosamente");
                                    form.Items.Item(lrs_BtnBuscar).Click();
                                }
                                else
                                    sboApplication.statusBarErrorMsg("No se pudo eliminar: " + lo_response2.Content.ReadAsStringAsync().Result);
                            }
                        }
                    }
                }
            }));
            this.Add(new ItemAction(BoEventTypes.et_ITEM_PRESSED, lrs_BtnExprt, e =>
            {
                if (!e.BeforeAction)
                {
                    if (form.getGrid(lrs_GrdVentas).Rows.Count > 0)
                    {
                        if (Global.go_sboApplictn.MessageBox("¿Estás seguro que deseas exportar?", 1, "Si", "No") == 1)
                        {
                            sb_exportDocuments();
                        }
                    }
                    else
                        throw new Exception("No se tiene documentos a exportar");
                }
            }));
            this.Add(new ItemAction(BoEventTypes.et_ITEM_PRESSED, lrs_BtnRemplzr, e =>
            {
                if (!e.BeforeAction)
                {

                    if (Global.go_sboApplictn.MessageBox("¿Estás seguro que deseas reemplazar propuesta?", 1, "Si", "No") == 1)
                    {
                        if (fn_verificaRespuesta())
                        {
                            bool lb_sap = form.getOptionBtn(lrs_RbnSp).Selected;

                            if (lb_sap & grid.Rows.Count > 0)
                            {
                                sb_reemplazoPropuesta();

                                form.Items.Item(lrs_BtnBuscar).Click();

                            }
                            else
                                throw new Exception("No hay comprobantes registrados");
                        }
                    }
                }
            }));
        }
        protected override void loadItemActions()
        {
            // Nothing
        }
    }
}
