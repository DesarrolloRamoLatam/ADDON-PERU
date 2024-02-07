using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SAPbouiCOM;
using STR_Addon_PeruRamo.BL.Sire;
using STR_Addon_PeruRamo.EL;
using STR_Addon_PeruRamo.EL.Sire;
using STR_Addon_PeruRamo.Services;
using STR_Addon_PeruRamo.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace STR_Addon_PeruRamo_V1.Forms.USRForms.Sire
{
    sealed partial class FormSireCompras : UIForm
    {
        private static FormSireCompras thisForm;

        private static Dictionary<string, string> ldc_dataMes = null;
        private static List<RSPeriodos> lot_periodos;
        private static SIREServicCompra lo_sunatSrvc;
        private static List<Registros> lot_cpsSap;
        private string ls_rtarchivo;
        private string ls_contrlFs = "";
        List<Registros> lo_comprobnts = new List<Registros>();
        private FormSireCompras() : base(gs_nombre, gs_ruta) { }

        public static FormSireCompras getForm()
        {
            thisForm = thisForm ?? new FormSireCompras();
            return thisForm;
        }
        protected override void sb_dataFormLoad()
        {
            try
            {

                form.Freeze(true);

                form.getOptionBtn(lrs_RbnSr).GroupWith(lrs_RbnSp);
                form.getOptionBtn(lrs_RbnSr).GroupWith(lrs_RbnDf);
                form.getOptionBtn(lrs_RbnNoDoc).GroupWith(lrs_RbnSr);

                bool validConfig = SIREMod.fn_getValidConfig();
                if (validConfig == false)
                {
                    sboApplication.MessageBox("Configuración incompleta, modificar en la tabla de Configuración SIRE", 1, "Ok");
                    return;
                }

                lo_sunatSrvc = new SIREServicCompra();
                ls_rtarchivo = lo_sunatSrvc.rutaArchivoTxt;
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
                    form.getCombobox(lrs_CmbAnios).ValidValues.Add(lot_periodos[i].numEjercicio, lot_periodos[i].desEstado);
                }
                SIREMod.cargarLogo(form);
            }
            finally
            {
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

            RSPeriodos lo_periodo = lot_periodos.FirstOrDefault(x => x.numEjercicio == form.getCombobox(lrs_CmbAnios).Value);

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

            string ls_fase = "";

            switch (ps_fase)
            {
                case "1":
                    ls_fase = "Propuesta";
                    break;
                case "2":
                    ls_fase = "Preliminar";
                    break;
                case "3":
                    ls_fase = "Preliminar";
                    break;
                case "4":
                    ls_fase = "Preliminar";
                    break;
                case "5":
                    ls_fase = "Pre. Final";
                    break;
                case "6":
                    ls_fase = "Pre. Final";
                    break;
                default:
                    break;
            }

            form.getStaticText(lrs_LblFase).Caption = $"Fase: {ls_fase}";

            ls_contrlFs = ps_fase;

        }
        public void sb_setDatosSire()
        {

            try
            {
                form.Freeze(true);

                RSDocumentos lo_rSComprobantes = new RSDocumentos();

                string ls_perTributario = ldc_dataMes[form.getCombobox(lrs_CmbMeses).Value];

                sb_faseAsignado(lo_sunatSrvc.fn_getProcesoFase(ls_perTributario));


                grid = form.getGrid(lrs_GrdCompras);
                grid.DataTable.Clear();


                form.enabledItems(false, lrs_BtnRemplzr);
                form.enabledItems(true, lrs_BtnAceptar);

                bool lb_preliminar = ls_contrlFs == "2" || (ls_contrlFs == "3" || ls_contrlFs == "4");

                if (lb_preliminar)
                {

                    if (ls_contrlFs == "2")
                    {
                        form.visibleItems(true, lrs_LblDetalleNodm);


                        Registro lo_registroAnterior = lo_sunatSrvc.fn_validaTicketAnterior(ls_perTributario, 7);


                        if (lo_registroAnterior != null)
                        {
                            form.getStaticText(lrs_LblDetalleNodm).Caption = $"Carga de No Domiciliado en estado: \"{SIREMod.fn_motivoProceso(lo_registroAnterior.codEstadoProceso)}\" con Ticket: \"{lo_registroAnterior.numTicket}\"";


                            if (Convert.ToInt32(lo_registroAnterior.codEstadoProceso) < 3)
                            {
                                form.Items.Item(lrs_LblDetalleNodm).ForeColor = SIREMod.fn_getRgb(255, 0, 0);
                                Global.go_sboApplictn.statusBarWarningMsg($"Carga de No Domiciliado en estado: \"{SIREMod.fn_motivoProceso(lo_registroAnterior.codEstadoProceso)}\" con Ticket: \"{lo_registroAnterior.numTicket}\"");
                            }
                            else
                            {
                                form.Items.Item(lrs_LblDetalleNodm).ForeColor = SIREMod.fn_getRgb(198, 255, 168);
                                Global.go_sboApplictn.statusBarSuccessMsg($"Carga de No Domiciliado en estado: \"{SIREMod.fn_motivoProceso(lo_registroAnterior.codEstadoProceso)}\" con Ticket: \"{lo_registroAnterior.numTicket}\"");
                            }
                        }
                        else
                        {
                            form.getStaticText(lrs_LblDetalleNodm).Caption = $"Pendiente de cargar no domiciliado";
                            Global.go_sboApplictn.statusBarWarningMsg($"Pendiente de cargar no domiciliado");
                            form.Items.Item(lrs_LblDetalleNodm).ForeColor = SIREMod.fn_getRgb(255, 0, 0);
                        }

                        // form.Items.Item(lrs_LblDetalleNodm).Width = 250;
                    }

                    form.visibleItems(true, lrs_BtnDelt);
                    form.enabledItems(true, lrs_BtnDelt);

                    form.getButton(lrs_BtnDelt).Caption = "Eliminar Preliminar";

                    Registro lo_registroAceptacion = lo_sunatSrvc.fn_validaPreliminardeAceptar(ls_perTributario);

                    RSDocumentos lo_comPreliminares = lo_sunatSrvc.fn_getPreliminares(ls_perTributario);

                    lo_rSComprobantes = lo_comPreliminares;         //comprobantesLibrosCompras
                    if (lo_rSComprobantes.comprobantesLibrosCompras.Count < 1 & lo_registroAceptacion != null)
                    {
                        lo_rSComprobantes = lo_sunatSrvc.fn_comprobantesSire(ls_perTributario);
                        lo_rSComprobantes.comprobantesLibrosCompras = lo_rSComprobantes.registros;
                        lo_rSComprobantes.montosTotales = lo_rSComprobantes.totales;
                        //(double)lo_rSComprobantes.montosTotales.mtoTotalCp
                    }

                    form.getButton(lrs_BtnAceptar).Caption = "Registrar Preliminar";
                }
                else if (ls_contrlFs == "1")
                {
                    lo_rSComprobantes = lo_sunatSrvc.fn_comprobantesSire(ls_perTributario);
                    form.getButton(lrs_BtnAceptar).Caption = "Aceptar Propuesta";
                }
                else if (ls_contrlFs == "5")
                {
                    form.visibleItems(true, lrs_BtnDelt);
                    form.enabledItems(true, lrs_BtnDelt);
                    form.enabledItems(false, lrs_BtnAceptar, lrs_BtnRemplzr);

                    form.getButton(lrs_BtnDelt).Caption = "Eliminar Pre. Final";

                    RSDocumentos lo_comPreliminares = lo_sunatSrvc.fn_getPreFinales(ls_perTributario);
                    lo_rSComprobantes = lo_comPreliminares;
                    lo_rSComprobantes.registros = lo_rSComprobantes.comprobantes;

                }
                else if (Convert.ToInt32(ls_contrlFs) > 4)
                {
                    form.enabledItems(false, lrs_BtnAceptar, lrs_BtnRemplzr);

                    Global.go_sboApplictn.MessageBox("El periodo escogido se encuentra ahora en estado de Generación", 1, "Ok");
                    Global.go_sboApplictn.statusBarWarningMsg("El periodo escogido se encuentra ahora en estado de Generación");
                    return;
                }

                lo_comprobnts = lb_preliminar ? lo_rSComprobantes.comprobantesLibrosCompras : lo_rSComprobantes.registros;

                try
                {
                    if (lo_comprobnts.Count < 1)
                    {
                        sboApplication.statusBarWarningMsg("No se ha encontrado información de comprobantes de pago en Periodo Seleccionado");
                        return;
                    }
                }
                catch (Exception)
                {
                    sboApplication.statusBarWarningMsg("No se ha encontrado información de comprobantes de pago en Periodo Seleccionado");
                    return;
                }

                sb_setGrilla();

                for (int i = 0; i < lo_comprobnts.Count; i++)
                {
                    try
                    {
                        grid.DataTable.Rows.Add();
                        grid.DataTable.SetValue(lrs_ClmEmision, i, Convert.ToDateTime(lo_comprobnts[i].fecEmision));
                        grid.DataTable.SetValue(lrs_ClmTipoCp, i, lo_comprobnts[i].codTipoCDP);
                        grid.DataTable.SetValue(lrs_ClmSerieCp, i, lo_comprobnts[i].numSerieCDP);
                        grid.DataTable.SetValue(lrs_ClmNroCp, i, lo_comprobnts[i].numCDP);
                        grid.DataTable.SetValue(lrs_ClmRznSoc, i, ls_contrlFs == "5" ? lo_comprobnts[i].nomRazonSocialCliente : lo_comprobnts[i].nomRazonSocialProveedor);
                        grid.DataTable.SetValue(lrs_ClmTipoMode, i, lo_comprobnts[i].codMoneda == null ? "" : lo_comprobnts[i].codMoneda);
                        grid.DataTable.SetValue(lrs_ClmMtoBIGrv, i, ls_contrlFs == "5" ? lo_comprobnts[i].mtoBIGravada : lo_comprobnts[i].montos.mtoBIGravadaDG);
                        grid.DataTable.SetValue(lrs_ClmIgv, i, ls_contrlFs == "5" ? lo_comprobnts[i].mtoIGV : lo_comprobnts[i].montos.mtoIgvIpmDG);
                        grid.DataTable.SetValue(lrs_ClmTotal, i, ls_contrlFs == "5" ? lo_comprobnts[i].mtoTotalCp.ToString("F2") : lo_comprobnts[i].montos.mtoTotalCp.ToString("F2"));
                        grid.DataTable.SetValue(lrs_ClmDocReferencia, i, ls_contrlFs == "5" ? SIREMod.fn_comprobanteModificado(lo_comprobnts[i].documentoMod) : SIREMod.fn_comprobanteModificado(lo_comprobnts[i].lisDocumentosMod));
                    }
                    catch (Exception)
                    {

                        
                    }
                }

                grid.AutoResizeColumns();

                grid.editableGridColumns(false, lrs_ClmEmision, lrs_ClmTipoCp, lrs_ClmSerieCp, lrs_ClmNroCp, lrs_ClmTotal, lrs_ClmTipoMode,
                    lrs_ClmRznSoc, lrs_ClmMtoBIGrv, lrs_ClmIgv, lrs_ClmDocReferencia);

                grid.getEditTextColumn(lrs_ClmEmision).TitleObject.Sortable = true;
                grid.getEditTextColumn(lrs_ClmTotal).TitleObject.Sortable = true;

                //double ls_total = ls_contrlFs == "5" ? (double)lo_rSComprobantes.totales.mtoTotalCP : lb_preliminar ? (double)lo_rSComprobantes.montosTotales.mtoTotalCp : (double)lo_rSComprobantes.totales.mtoTotalCp;
                double ls_total = (double)lo_rSComprobantes.totales.mtoTotalCP != null ? (double)lo_rSComprobantes.totales.mtoTotalCP : (double)lo_rSComprobantes.montosTotales.mtoTotalCp != null ? (double)lo_rSComprobantes.montosTotales.mtoTotalCp  :  (double)lo_rSComprobantes.totales.mtoTotalCp;

                string ls_totalFor = ls_total.ToString("C", CultureInfo.CreateSpecificCulture("es-PE"));

                int ls_cantidad = lb_preliminar ? lo_rSComprobantes.comprobantesLibrosCompras.Count : lo_rSComprobantes.registros.Count;

                form.getEditText(lrs_EdtTotlSl).Value = ls_totalFor;
                form.getEditText(lrs_EdtTotlSl).Active = false;
                form.getEditText(lrs_EdtTotlSl).Item.Visible = true;
                form.getItem(lrs_EdtTotlSl).Enabled = false;
                form.getEditText(lrs_EdtCntdCp).Value = ls_cantidad.ToString();

                sb_cleanRows();

                if (ls_cantidad < 1)
                    sboApplication.statusBarWarningMsg("No se tiene información de comprobantes del periodo seleccionado");
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

        public void sb_setDatosSap(bool ps_domiciliados = false)
        {
            try
            {
                form.Freeze(true);

                RSDocumentos lo_rSCpSap = new RSDocumentos();
                lot_cpsSap = new List<Registros>();

                string ls_perTributario = ldc_dataMes[form.getCombobox(lrs_CmbMeses).Value];
                sb_faseAsignado(lo_sunatSrvc.fn_getProcesoFase(ls_perTributario));

                grid = form.getGrid(lrs_GrdCompras);
                grid.DataTable.Clear();

                form.enabledItems(false, lrs_BtnAceptar);


                form.getButton(lrs_BtnRemplzr).Caption = ls_contrlFs == "2" ? "Registrar No Domiciliario" : "Reemplazar Propuesta";
                form.enabledItems(ls_contrlFs == "1" & !ps_domiciliados | ps_domiciliados & ls_contrlFs == "2" | ps_domiciliados & ls_contrlFs == "3", lrs_BtnRemplzr);

                lo_rSCpSap = ps_domiciliados ? lo_sunatSrvc.fn_getCompsND(ls_perTributario) : lo_sunatSrvc.fn_getCompSap(ls_perTributario);

                if (ps_domiciliados)
                {

                    if (ls_contrlFs == "2" | ls_contrlFs == "3")
                    {
                        form.visibleItems(true, lrs_LblDetalleNodm);


                        Registro lo_registroAnterior = lo_sunatSrvc.fn_validaTicketAnterior(ls_perTributario, 7);


                        if (lo_registroAnterior != null)
                        {
                            form.getStaticText(lrs_LblDetalleNodm).Caption = $"Carga de No Domiciliado en estado: \"{SIREMod.fn_motivoProceso(lo_registroAnterior.codEstadoProceso)}\" con Ticket: \"{lo_registroAnterior.numTicket}\"";


                            if (Convert.ToInt32(lo_registroAnterior.codEstadoProceso) < 3)
                            {
                                form.Items.Item(lrs_LblDetalleNodm).ForeColor = SIREMod.fn_getRgb(255, 0, 0);
                                Global.go_sboApplictn.statusBarWarningMsg($"Carga de No Domiciliado en estado: \"{SIREMod.fn_motivoProceso(lo_registroAnterior.codEstadoProceso)}\" con Ticket: \"{lo_registroAnterior.numTicket}\"");
                            }
                            else
                            {
                                form.Items.Item(lrs_LblDetalleNodm).ForeColor = SIREMod.fn_getRgb(198, 255, 168);
                                Global.go_sboApplictn.statusBarSuccessMsg($"Carga de No Domiciliado en estado: \"{SIREMod.fn_motivoProceso(lo_registroAnterior.codEstadoProceso)}\" con Ticket: \"{lo_registroAnterior.numTicket}\"");
                            }
                        }
                        else
                        {
                            form.getStaticText(lrs_LblDetalleNodm).Caption = $"Pendiente de cargar no domiciliado";
                            Global.go_sboApplictn.statusBarWarningMsg($"Pendiente de cargar no domiciliado");
                            form.Items.Item(lrs_LblDetalleNodm).ForeColor = SIREMod.fn_getRgb(255, 0, 0);
                        }

                        // form.Items.Item(lrs_LblDetalleNodm).Width = 250;
                    }
                }


                lot_cpsSap = lo_rSCpSap.registros;

                if (lot_cpsSap.Count < 1)
                {
                    sboApplication.statusBarWarningMsg("No se tiene información de comprobantes del periodo seleccionado");
                    return;
                }


                sb_setGrilla();

                for (int i = 0; i < lot_cpsSap.Count; i++)
                {
                    grid.DataTable.Rows.Add();
                    grid.DataTable.SetValue(lrs_ClmEmision, i, Convert.ToDateTime(lot_cpsSap[i].fecEmision));
                    grid.DataTable.SetValue(lrs_ClmTipoCp, i, lot_cpsSap[i].codTipoCDP);
                    grid.DataTable.SetValue(lrs_ClmTpCpSpR, i, lot_cpsSap[i].codTipoCDP);
                    grid.DataTable.SetValue(lrs_ClmSerieCp, i, lot_cpsSap[i].numSerieCDP);
                    grid.DataTable.SetValue(lrs_ClmNroCp, i, lot_cpsSap[i].numCDP);
                    grid.DataTable.SetValue(lrs_ClmRznSoc, i, lot_cpsSap[i].nomRazonSocialProveedor);
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

                double ls_total = (double)lo_rSCpSap.totales.mtoSumTotalCP; //(double(double)lo_rSCpSap.totales.mtoSumTotalCP).ToString("F2");
                string ls_totalFor = ls_total.ToString("C", CultureInfo.CreateSpecificCulture("es-PE"));

                form.getEditText(lrs_EdtTotlSl).Value = ls_totalFor;
                form.getEditText(lrs_EdtTotlSl).Active = false;
                form.getEditText(lrs_EdtTotlSl).Item.Visible = true;

                form.getItem(lrs_EdtTotlSl).Enabled = false;

                form.getEditText(lrs_EdtCntdCp).Value = lo_rSCpSap.registros.Count.ToString();

                grid.AutoResizeColumns();

                sb_cleanRows();

                form.Freeze(false);

                if (lo_rSCpSap.registros.Count < 1)
                {
                    sboApplication.statusBarWarningMsg("No se tiene información de comprobantes del periodo seleccionado");
                    return;
                }


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

                grid = form.getGrid(lrs_GrdCompras);
                grid.DataTable.Clear();

                form.visibleItems(false, lrs_LblFase);

                lo_rSCpSire = lo_sunatSrvc.fn_comprobantesSire(ls_perTributario);
                lo_rSCpSap = lo_sunatSrvc.fn_getCompSap(ls_perTributario);

                lot_cpsSire = lo_rSCpSire.registros;
                lot_cpsSap = lo_rSCpSap.registros;

                sb_setGrilla();


                //double ld_totalSire = (double)lo_rSCpSire.totales.mtoTotalCp == null ? (double)lo_rSCpSire.totales.mtoTotalCP : (double)lo_rSCpSire.totales.mtoTotalCp;
                double ld_totalSire = (double)lo_rSCpSire.totales.mtoTotalCP != null ? (double)lo_rSCpSire.totales.mtoTotalCP : (double)lo_rSCpSire.totales.mtoTotalCp != null ? (double)lo_rSCpSire.totales.mtoTotalCp : (double)lo_rSCpSire.totales.mtoTotalCp != null ? (double)lo_rSCpSire.totales.mtoTotalCp : 0.0;

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
                    lst_data.Add(lo_comSap.nomRazonSocialProveedor);
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
                    lst_data.Add(lo_comSire == null ? string.Empty : lo_comSire.nomRazonSocialProveedor);
                    lst_data.Add(lo_comSire == null ? string.Empty : lo_comSire.codMoneda);
                    lst_data.Add(lo_comSire == null ? string.Empty : ((double)lo_comSire.montos.mtoBIGravadaDG).ToString("F2"));
                    lst_data.Add(lo_comSire == null ? string.Empty : ((double)lo_comSire.montos.mtoIgvIpmDG).ToString("F2"));
                    lst_data.Add(lo_comSire == null ? string.Empty : ((double)lo_comSire.montos.mtoTotalCp).ToString("F2"));

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
                        lot_cpsSire[i].nomRazonSocialProveedor,
                        lot_cpsSire[i].codMoneda,
                        ((double)lot_cpsSire[i].montos.mtoBIGravadaDG).ToString("F2"),
                        ((double)lot_cpsSire[i].montos.mtoIgvIpmDG).ToString("F2"),
                        ((double)lot_cpsSire[i].montos.mtoTotalCp).ToString("F2")
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

                // form.enabledItems(false, lrs_EdtCantCpSr, lrs_EdtCntddSp, lrs_EdtTotalSolSr, lrs_EdtTtlSlSp);

                grid.getEditTextColumn(lrs_ClmTipoCpSap).LinkedObjectType = "2";

                grid.Columns.Item("|").Editable = false;

                GridColumns columns = grid.Columns;
                GridColumn col = columns.Item(12);
                col.TitleObject.Caption = string.Empty;


                grid.AutoResizeColumns();
                sb_cleanRows();
                sb_paintGrid();

                Global.go_sboApplictn.statusBarSuccessMsg("Carga exitosa: Se mostraron todos los comprobantes de Sire y Sap");

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

            grid = form.Items.Item(lrs_GrdCompras).Specific;

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
        /*
        public bool validGrid(Registros registro)
        {
            for (int i = 0; i < grid.Rows.Count; i++)
            {
                if (grid.DataTable.GetValue(clmTipoCp, i) == registro.codTipoCDP)
                    if (grid.DataTable.GetValue(clmSerieCp, i) == registro.numSerieCDP)
                        if (string.Format(grid.DataTable.GetValue(clmNroCp, i)).Trim('0') == string.Format(registro.numCDP).Trim('0'))
                            return false;
            }
            return true;
        }*/
        public static int RGB(int red, int green, int blue)
        {
            return red | (green << 8) | (blue << 16);
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
                bool dSap = form.getOptionBtn(lrs_RbnSp).Selected;
                bool dSire = form.getOptionBtn(lrs_RbnSr).Selected;
                bool dDif = form.getOptionBtn(lrs_RbnDf).Selected;
                bool dND = form.getOptionBtn(lrs_RbnNoDoc).Selected;
                string perTributario = ldc_dataMes[form.getCombobox(lrs_CmbMeses).Value];

                if (dSap)
                {

                    var response = lo_sunatSrvc.fn_descargaPSap(perTributario, "Envios", true);

                    if (response.codProceso == "61" | response.codProceso == "4")
                    {
                        var carpeta = response.codProceso == "4" ? "Aceptados" : "Aceptados - Observados";

                        Global.go_sboApplictn.MessageBox($"La propuesta de remplazo se registro con ticket: {response.numTicket} y fue {(response.codProceso == "4" ? "Aceptado exitosamente" : "Aceptado con Observaciones")}", 1, "Ok");

                        lo_sunatSrvc.descargaArchivo(response.nomArchivoImportacion, carpeta, false);
                        sboApplication.statusBarSuccessMsg($"Se guardo el detalle en la carpeta {ls_rtarchivo}\\{carpeta}\\{response.nomArchivoImportacion}");
                    }
                }
                else if (dND)
                {

                    Registro registro = lo_sunatSrvc.fn_validaTicketAnterior(perTributario, 3);

                    if (registro != null)
                    {
                        if (Global.go_sboApplictn.MessageBox($"Estimado contribuyente, ya cuentas con un registro pendiente con ticket {registro.numTicket} y estado: \"{SIREMod.fn_motivoProceso(registro.codEstadoProceso)}\". ¿Deseas reemplazarlo?", 1, "Si", "No") == 1)
                        {
                            string ruta = lo_sunatSrvc.fn_descargaND(perTributario, "Envios");
                            string nombreArchivo = ruta.Split('\\')[ruta.Split('\\').Length - 1];
                            string ruc = nombreArchivo.Substring(2, 11);



                            var response = lo_sunatSrvc.fn_setAgregarNodomclds(ruta + ".zip", lo_sunatSrvc.obtenerIdTusND(ruta + ".zip", nombreArchivo + ".zip", ruc, perTributario));

                            if (Convert.ToInt32(response.codEstadoProceso) > 2)
                            {
                                var carpeta = response.codEstadoProceso == "03" ? "Aceptados - Observados" : "Aceptados";

                                lo_sunatSrvc.descargaArchivo(response.nomArchivoImportacion, carpeta, false);
                                Global.go_sboApplictn.MessageBox($"La carga del archivo se registró con ticket {response.numTicket} y se encuentra en estado: \"{response.desEstadoProceso}\"");
                                sboApplication.statusBarWarningMsg($"Se guardo el detalle en la carpeta {ls_rtarchivo}\\{carpeta}\\{response.nomArchivoImportacion}");
                            }
                            else
                            {
                                Global.go_sboApplictn.MessageBox($"La carga del archivo se registró con ticket {response.numTicket} y se encuentra en estado: \"{response.desEstadoProceso}\"");
                                sboApplication.statusBarSuccessMsg($"Se guardo el archivo enviado en la ruta: {ruta}");

                            }
                        }
                    }
                    else
                    {
                        string ruta = lo_sunatSrvc.fn_descargaND(perTributario, "Envios");
                        string nombreArchivo = ruta.Split('\\')[ruta.Split('\\').Length - 1];
                        string ruc = nombreArchivo.Substring(2, 11);



                        var response = lo_sunatSrvc.fn_setAgregarNodomclds(ruta + ".zip", lo_sunatSrvc.obtenerIdTusND(ruta + ".zip", nombreArchivo + ".zip", ruc, perTributario));

                        if (Convert.ToInt32(response.codEstadoProceso) > 2)
                        {
                            var carpeta = response.codEstadoProceso == "03" ? "Aceptados - Observados" : "Aceptados";

                            lo_sunatSrvc.descargaArchivo(response.nomArchivoImportacion, carpeta, false);
                            Global.go_sboApplictn.MessageBox($"La carga del archivo se registró con ticket {response.numTicket} y se encuentra en estado: \"{response.desEstadoProceso}\"");
                            sboApplication.statusBarWarningMsg($"Se guardo el detalle en la carpeta {ls_rtarchivo}\\{carpeta}\\{response.nomArchivoImportacion}");
                        }
                        else
                        {
                            Global.go_sboApplictn.MessageBox($"La carga del archivo se registró con ticket {response.numTicket} y se encuentra en estado: \"{response.desEstadoProceso}\"");
                            sboApplication.statusBarSuccessMsg($"Se guardo el archivo enviado en la ruta: {ruta}");

                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public void sb_registraPreliminar(string perTri)
        {
            var response = lo_sunatSrvc.fn_registrarPreliminar(perTri);

            if (response.IsSuccessStatusCode)
            {

                string token = response.Content.ReadAsStringAsync().Result;

                Global.go_sboApplictn.MessageBox($"Se registró Preliminar Final exitosamente con token: {token}", 1, "Ok");
                Global.go_sboApplictn.statusBarSuccessMsg($"Se registró Preliminar Final exitosamente con token: {token}");
            }
            else
            {
                RSerror error = System.Text.Json.JsonSerializer.Deserialize<RSerror>(response.Content.ReadAsStringAsync().Result);

                Global.go_sboApplictn.MessageBox($"Error al registrar Preliminar final: \"{error.errors[0].msg}\"", 1, "Ok");
                Global.go_sboApplictn.statusBarSuccessMsg($"Error al registrar Preliminar final: \"{error.errors[0].msg}\"");
            }
        }
        public void sb_exportDocuments()
        {
            try
            {
                bool lb_sap = form.getOptionBtn(lrs_RbnSp).Selected;
                bool lb_sire = form.getOptionBtn(lrs_RbnSr).Selected;
                bool lb_dif = form.getOptionBtn(lrs_RbnDf).Selected;
                bool lb_nd = form.getOptionBtn(lrs_RbnNoDoc).Selected;

                string perTributario = ldc_dataMes[form.getCombobox(lrs_CmbMeses).Value];
                if (lb_sap)
                {
                    Registro registro = lo_sunatSrvc.fn_descargaPSap(perTributario, "Exportaciones\\Sap", false);
                    sboApplication.statusBarSuccessMsg("Exportado exitosamente en la carpeta: " + registro.nomArchivoImportacion);
                    Global.go_sboApplictn.MessageBox($"El archivo fue exportado exitosamente en la ruta: {ls_rtarchivo + "\\Exportaciones\\Sap"}", 1, "Ok");
                }
                else if (lb_sire)
                {
                    lo_sunatSrvc.sb_descargaPropuesta(perTributario, "Exportaciones\\Sire");
                    Global.go_sboApplictn.MessageBox($"El archivo fue exportado exitosamente en la ruta: {ls_rtarchivo + "\\Exportaciones\\Sire"}", 1, "Ok");
                }
                else if (lb_dif)
                {
                    SIREMod.sb_exportCsv(form.getGrid(lrs_GrdCompras), "Exportaciones\\Comparaciones", perTributario, ls_rtarchivo);
                    Global.go_sboApplictn.MessageBox($"El archivo fue exportado exitosamente en la ruta: {ls_rtarchivo + "\\Exportaciones\\Comparaciones"}", 1, "Ok");
                }
                else if (lb_nd)
                {
                    string rut = lo_sunatSrvc.fn_descargaND(perTributario, "\\Exportaciones\\ND");
                    Global.go_sboApplictn.MessageBox($"El archivo fue exportado exitosamente en la ruta: {ls_rtarchivo + "\\Exportaciones\\ND"}", 1, "Ok");
                    sboApplication.statusBarSuccessMsg("Exportado exitosamente en la carpeta: " + rut);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
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
                    grid = form.getGrid(lrs_GrdCompras);
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
                    Global.go_sboApplictn.statusBarSuccessMsg("Carga exitosa: Se filtró correctamente los documentos con incongruencia");
                    form.Freeze(false);
                }
                else if (filtro == "Faltantes")
                {
                    form.Freeze(true);
                    sb_setDatosComparar();
                    grid = form.getGrid(lrs_GrdCompras);
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
                    Global.go_sboApplictn.statusBarSuccessMsg("Carga exitosa: Se filtró correctamente los documentos faltantes");
                    form.Freeze(false);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void sb_setGrilla()
        {

            bool lb_sap = form.getOptionBtn(lrs_RbnSp).Selected;
            bool lb_dif = form.getOptionBtn(lrs_RbnDf).Selected;
            bool lb_nd = form.getOptionBtn(lrs_RbnNoDoc).Selected;
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
                if (lb_sap | lb_nd)
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

        public bool fn_verificaRespuesta()
        {

            string ls_perTri = ldc_dataMes[form.getCombobox(lrs_CmbMeses).Value];

            var ls_response = SIREMod.fn_verificaPropuesta(lo_sunatSrvc.token, ls_perTri);

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
            this.Add(new ItemAction(BoEventTypes.et_COMBO_SELECT, lrs_CmbAnios, e =>
            {
                if (!e.BeforeAction)
                {
                    form.enabledItems(true, lrs_CmbMeses);
                    sb_getMesesLoad();
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
            this.Add(new ItemAction(BoEventTypes.et_MATRIX_LINK_PRESSED, lrs_GrdCompras, e =>
            {
                grid = form.getGrid(lrs_GrdCompras);

                var lo_visualRow = grid.GetDataTableRowIndex(e.Row);


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
            /*
            this.Add(new ItemAction(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED, lrs_RbnSr, e =>
            {
                //string valor = form.getOptionBtn(lrs_RbnSr).V;
                if (!e.BeforeAction)
                {
                    form.getOptionBtn(lrs_RbnSr).Selected = true;
                }
            }));*/

            this.Add(new ItemAction(BoEventTypes.et_ITEM_PRESSED, lrs_BtnBuscar, e =>
            {
                bool lb_sap = form.getOptionBtn(lrs_RbnSp).Selected;
                bool lb_sire = form.getOptionBtn(lrs_RbnSr).Selected;
                bool lb_dif = form.getOptionBtn(lrs_RbnDf).Selected;
                bool lb_nd = form.getOptionBtn(lrs_RbnNoDoc).Selected;

                if (!e.BeforeAction)
                {
                    var mesSelect = form.getCombobox(lrs_CmbMeses).Value;
                    if (!lb_sap & !lb_sire & !lb_dif & !lb_nd)
                    {
                        sboApplication.statusBarErrorMsg("No has seleccionado el tipo de datos");
                    }
                    else if (string.IsNullOrEmpty(mesSelect))
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

                        form.visibleItems(false, lrs_EdtCntdCp, lrs_EdtTotlSl, lrs_LblCntddCp, lrs_LblTotalSl, lrs_BtnDelt, lrs_LblDetalleNodm);

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
                         lrs_EdtCantCpSr, lrs_EdtTotalSolSr, lrs_LblCantCpSr, lrs_LblTotalSolSr, lrs_BtnDelt, lrs_BtnFilt, lrs_LblDetalleNodm);

                        form.visibleItems(true, lrs_EdtCntdCp, lrs_EdtTotlSl, lrs_LblCntddCp, lrs_LblTotalSl);

                        form.getEditText(lrs_EdtTotlSl).Value = string.Empty;
                        form.getEditText(lrs_EdtCntdCp).Value = string.Empty;
                        if (lb_sire)
                            sb_setDatosSire();
                        else if (lb_sap)
                            sb_setDatosSap();
                        else if (lb_nd)
                            sb_setDatosSap(true);
                    }

                }
            }));

            this.Add(new ItemAction(BoEventTypes.et_CLICK, lrs_LblPortal, e =>
            {
                if (!e.BeforeAction)
                {
                    System.Diagnostics.Process.Start("https://api-seguridad.sunat.gob.pe/v1/clientessol/4f3b88b3-d9d6-402a-b85d-6a0bc857746a/oauth2/loginMenuSol");
                }
            }));

            this.Add(new ItemAction(BoEventTypes.et_ITEM_PRESSED, lrs_BtnRemplzr, e =>
            {
                if (!e.BeforeAction)
                {
                    bool dSap = form.getOptionBtn(lrs_RbnSp).Selected;
                    bool dNd = form.getOptionBtn(lrs_RbnNoDoc).Selected;

                    if (Global.go_sboApplictn.MessageBox($"¿Estás seguro que deseas {(dSap ? "Reemplazar propuesta" : "Agregar Comprobantes")}?", 1, "Si", "No") == 1)
                    {
                        if (fn_verificaRespuesta())
                        {

                            if (dSap | dNd)
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
            this.Add(new ItemAction(BoEventTypes.et_ITEM_PRESSED, lrs_BtnExprt, e =>
            {
                if (!e.BeforeAction)
                {
                    if (form.getGrid(lrs_GrdCompras).Rows.Count > 0)
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
            this.Add(new ItemAction(BoEventTypes.et_ITEM_PRESSED, lrs_BtnDelt, e =>
            {
                if (!e.BeforeAction)
                {
                    string perTri = ldc_dataMes[form.getCombobox(lrs_CmbMeses).Value];
                    if (Global.go_sboApplictn.MessageBox("¿Estás seguro que deseas eliminar propuesta o preliminar registrado?", 1, "Si", "No") == 1)
                    {

                        if (ls_contrlFs == "2" | ls_contrlFs == "3")
                        {
                            var response2 = lo_sunatSrvc.eliminarPreliminar(perTri);

                            if (response2.IsSuccessStatusCode)
                            {
                                sboApplication.statusBarSuccessMsg("Preliminar eliminado exitosamente");
                                form.Items.Item(lrs_BtnBuscar).Click();
                            }
                            else
                                sboApplication.statusBarErrorMsg("No se pudo eliminar: " + response2.Content.ReadAsStringAsync().Result);
                        }
                        else
                        {

                            var response = lo_sunatSrvc.eliminarPreFinal(perTri);

                            ls_contrlFs = lo_sunatSrvc.fn_getProcesoFase(perTri);

                            if (response.IsSuccessStatusCode)
                            {
                                sboApplication.statusBarSuccessMsg("Preliminar Final eliminado exitosamente");
                                form.Items.Item(lrs_BtnBuscar).Click();
                            }
                            else
                                sboApplication.statusBarErrorMsg("No se pudo eliminar: " + response.Content.ReadAsStringAsync().Result);

                            if (ls_contrlFs == "2")
                            {
                                var response2 = lo_sunatSrvc.eliminarPreliminar(perTri);

                                if (response2.IsSuccessStatusCode)
                                {
                                    sboApplication.statusBarSuccessMsg("Preliminar eliminado exitosamente");
                                    form.Items.Item(lrs_BtnBuscar).Click();
                                }
                                else
                                    sboApplication.statusBarErrorMsg("No se pudo eliminar: " + response2.Content.ReadAsStringAsync().Result);
                            }
                        }
                    }
                }
            }));
            this.Add(new ItemAction(BoEventTypes.et_ITEM_PRESSED, lrs_BtnAceptar, e =>
            {
                if (!e.BeforeAction)
                {
                    string perTributario = ldc_dataMes[form.getCombobox(lrs_CmbMeses).Value];
                    sb_faseAsignado(lo_sunatSrvc.fn_getProcesoFase(perTributario));
                    string fase = lo_sunatSrvc.fn_getProcesoFase(perTributario);

                    if (Global.go_sboApplictn.MessageBox("Estimado contribuyente ¿tiene operaciones con sujetos No Domiciliados?", 1, "Si", "No") == 1)
                    {

                        if (fn_verificaRespuesta())
                        {
                            if (fase == "1")
                            {
                                // update
                                string perTri = ldc_dataMes[form.getCombobox(lrs_CmbMeses).Value];

                                var responsee = lo_sunatSrvc.updateFase(perTri, "2");

                                if (responsee.IsSuccessStatusCode)
                                {
                                    Global.go_sboApplictn.MessageBox($"La propuesta con periodo {perTri} cambio a la fase Preliminar, dirígete a la opción de no domiciliados para continuar el proceso", 1, "Ok");
                                    sboApplication.statusBarWarningMsg($"La propuesta con periodo {perTri} cambio a la fase Preliminar, dirígete a la opción de no domiciliados para continuar el proceso");

                                    form.Items.Item(lrs_BtnBuscar).Click();
                                }
                                else
                                    sboApplication.statusBarErrorMsg("No se pudo ejecutar el proceso " + responsee.Content.ReadAsStringAsync().Result);
                            }
                            else
                            {
                                Global.go_sboApplictn.MessageBox($"Dirígete a la opción de no domiciliados para continuar el proceso", 1, "Ok");
                                sboApplication.statusBarWarningMsg($"Dirígete a la opción de no domiciliados para continuar el proceso");
                            }
                        }
                    }
                    else
                    {
                        if (fn_verificaRespuesta())
                        {

                            if (ls_contrlFs != "3" & ls_contrlFs != "2")
                            {

                                string perTri = ldc_dataMes[form.getCombobox(lrs_CmbMeses).Value];
                                HttpResponseMessage response = lo_sunatSrvc.aceptarPropuesta(perTri);
                                if (response.IsSuccessStatusCode)
                                    sboApplication.statusBarSuccessMsg("Se aceptó propuesta correctamente con Ticket: " + response.Content.ReadAsStringAsync().Result);
                                else
                                {
                                    sboApplication.statusBarErrorMsg("No se pudo aceptar propuesta: " + response.Content.ReadAsStringAsync().Result);
                                }

                                form.Items.Item(lrs_BtnBuscar).Click();
                            }
                            else
                            {

                                sb_registraPreliminar(perTributario);

                                form.Items.Item(lrs_BtnBuscar).Click();
                            }

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
