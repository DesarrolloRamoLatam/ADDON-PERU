namespace STR_Addon_PeruRamo_V1.Forms.USRForms.Sire
{
    sealed partial class FormSireVentas
    {
        public const string gs_nombre = "FrmRVIEV";
        public const string gs_ruta = "Resources/Sire/FrmRVIEV.srf";
        public const string gs_menu = "MNU_SIRE_RVIEV";

        //Controles UI
        //ComboBox
        private readonly string lrs_CmbAnio = "cbxAnio";
        private readonly string lrs_CmbMeses = "cbxMeses";
        //Grid  
        private readonly string lrs_GrdVentas = "grdVenta";

        //Column
        private readonly string lrs_ClmEmision = "F. Emisión";       // Fecha Emision
        private readonly string lrs_ClmTipoCp = "Tipo de doc."; // Tipo
        private readonly string lrs_ClmSerieCp = "Serie";            // Serie
        private readonly string lrs_ClmNroCp = "Correlativo";        // Numero
        private readonly string lrs_ClmRznSoc = "Razon Social";
        private readonly string lrs_ClmMtoBIGrv = "Total sin IGV";
        private readonly string lrs_ClmIgv = "Igv";
        private readonly string lrs_ClmTotal = "Total Sol";               // Importe Total
        private readonly string lrs_ClmTipoMode = "Moneda";  // Codigo Mo
        private readonly string lrs_ClmObser = "Observacion";
        private readonly string lrs_ClmTotalSr = "Total Sire";
        private readonly string lrs_ClmTotalSp = "Total Sap";
        private readonly string lrs_ClmObjType = "ObjectType";
        private readonly string lrs_ClmDocEntry = "DocumentEntry";
        private readonly string lrs_ClmDocReferencia = "Comprobante origen";

        //Column sap sire - compare
        private readonly string lrs_ClmEmisionSap = "F. Emision Sap";       // Fecha Emision
        private readonly string lrs_ClmTipoCpSap = "Tipo de doc. Sap"; // Tipo
        private readonly string lrs_ClmSerieCpSap = "Serie Sap";            // Serie
        private readonly string lrs_ClmNroCpSap = "Correlativo Sap";        // Numero
                                                                            // private readonly string clTotalSap = "Total Sol Sap";               // Importe Total
        private readonly string lrs_ClmTipMdSp = "Moneda Sap";  // Codigo Moneda
        private readonly string lrs_ClmTtlSpC = "Total Sap";
        private readonly string lrs_ClmTpCpSpR = "Serie Sap Re";  // este sirve como replica

        private readonly string lrs_ClmRznSocSr = "Razon Social Sire";
        private readonly string lrs_ClmMtoBIGrvSr = "Total Sin Igv Sire";
        private readonly string lrs_ClmIgvSr = "Igv Sire";

        //Column sap sire - compare
        private readonly string lrs_ClmEmisnSr = "F. Emision Sire";       // Fecha Emision
        private readonly string lrs_ClmTipoCpSr = "Tipo de doc. Sire"; // Tipo
        private readonly string lrs_ClmSerieCpSr = "Serie Sire";            // Serie
        private readonly string lrs_ClmNroCpSr = "Correlativo Sire";        // Numero
        private readonly string lrs_ClTotalSr = "Total Sol Sire";               // Importe Total
        private readonly string lrs_ClmTipoMdSr = "Moneda Sire";  // Codigo Moneda
        private readonly string lrs_ClmTotalSrC = "Total Sire";

        private readonly string lrs_ClmRznSocSp = "Razon Social Sap";
        private readonly string lrs_ClmMtoBIGrvSp = "Total Sin Igv Sap";
        private readonly string lrs_ClmIgvSp = "Igv Sap";

        // Columns Resumen
        private readonly string lrs_ClmTipoDoc = "Tipo de Documento";
        private readonly string lrs_ClmTotalDoc = "Total Documentos";

        //Button
        private readonly string lrs_BtnBuscar = "btnBuscar";
        private readonly string lrs_BtnAceptar = "btnAcepta";
        private readonly string lrs_BtnRemplzr = "btnReemp";
        private readonly string lrs_BtnDelt = "btnDelet";
        private readonly string lrs_BtnExprt = "btnExport";
        // Radio Button
        private readonly string lrs_RbnSr = "rbnSire";
        private readonly string lrs_RbnSp = "rbnSap";
        private readonly string lrs_RbnDf = "rbnDif";

        private readonly string lrs_BtnFilt = "btnFilt";

        // SOLO UNA OPCIO
        // Edit Text
        private readonly string lrs_EdtCntdCp = "edCantDocs";
        private readonly string lrs_EdtTotlSl = "edTotalSol";
        private readonly string lrs_EdTotalUsd = "edTotalUsd";
        // LabelText
        private readonly string lrs_LblCntddCp = "lblCant";
        private readonly string lrs_LblTotalSl = "lblSol";
        private readonly string lrs_LblTotlUsd = "lblUsd";

        private readonly string lrs_LblColCorr = "lblCorr";
        private readonly string lrs_LblColInco = "lblIncon";
        private readonly string lrs_LblColFalt = "lblFalt";
        private readonly string lrs_LblPortal = "lblPortal";

        // SAP
        // Edit Text
        private readonly string lrs_EdtCntddSp = "edSapCant";
        private readonly string lrs_EdtTtlSlSp = "edSapSol";
        private readonly string lrs_EdtTtlUsdSp = "edSapUsd";
        // LabelText
        private readonly string lrs_LblCntCpSp = "lblSapCant";
        private readonly string lrs_LblTtlSolSp = "lblSapSol";
        private readonly string lrs_LblTtlUsdSp = "lblSapUsd";

        // SIRE
        // Edit Text
        private readonly string lrs_EdtCantCpSr = "edSireCant";
        private readonly string lrs_EdtTotalSolSr = "edSireSol";
        private readonly string lrs_EdtTotalUsdSr = "edSireUsd";
        // LabelText
        private readonly string lrs_LblCantCpSr = "lblSireCan";
        private readonly string lrs_LblTotalSolSr = "lblSireSol";
        private readonly string lrs_LblTotalUsdSr = "lblSireUsd";

        private readonly string lrs_LblSr = "lblSire";
        private readonly string lrs_LblSp = "lblSap";
        private readonly string lrs_LblFase = "lblFase";



    }
}
