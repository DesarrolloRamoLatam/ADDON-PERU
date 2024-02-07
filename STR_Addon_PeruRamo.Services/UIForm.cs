using SAPbouiCOM;
using STR_Addon_PeruRamo.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace STR_Addon_PeruRamo.Services
{
    public abstract partial class UIForm : List<SBOActions>
    {
        private string nombre;
        private string ruta;

        protected UIForm()
        {
            this.sboApplication = Global.go_sboApplictn;
            this.sboCompany = Global.go_sboCompany;
        }

        /// <summary>
        /// LLamar a este constructor para forms de usuario
        /// </summary>
        /// <param name="nombre">Nombre del formulario</param>
        /// <param name="ruta">Ruta del XML</param>
        protected UIForm(string nombre, string ruta)
        {
            this.nombre = nombre;
            this.ruta = ruta;
            this.sboApplication = Global.go_sboApplictn;
            this.sboCompany = Global.go_sboCompany;
        }

        public void formLoad()
        {
            //if (this.form is null)
            //{
            this.form = this.añadirForm();
            sb_dataFormLoad();
            loadItemActions();
            loadDataActions();
            // }
        }

        public bool perform(SAPbouiCOM.ItemEvent itemEvent)
        {
            bool bubbleEvent = true;
            SBOActions sboAction = null;

            try
            {
                if (itemEvent == null)
                    return bubbleEvent;
                else if (itemEvent.EventType == BoEventTypes.et_FORM_UNLOAD || itemEvent.EventType == BoEventTypes.et_FORM_LOAD)
                    sboAction = this.Where(e => e is ItemAction).FirstOrDefault(e => ((ItemAction)e).EventType.Equals(itemEvent.EventType));
                else if (string.IsNullOrEmpty(itemEvent.ItemUID))
                    return bubbleEvent;
                else
                    sboAction = this.Where(e => e is ItemAction).FirstOrDefault(e => ((ItemAction)e).EventType.Equals(itemEvent.EventType)
                    && ((ItemAction)e).ItemID.Equals(itemEvent.ItemUID));
                sboAction?.execute(itemEvent);
            }
            catch { throw; }
            return bubbleEvent;
        }

        public bool dataEvent(SAPbouiCOM.BusinessObjectInfo businessObjInfo)
        {
            bool bubleEvent = true;
            SBOActions sboAction = null;
            try
            {
                if (businessObjInfo is null)
                    return bubleEvent;
                else if (string.IsNullOrEmpty(businessObjInfo.FormUID))
                    return bubleEvent;
                else
                    sboAction = this.Where(a => a is DataAction).FirstOrDefault(a => ((DataAction)a).EventType.Equals(businessObjInfo.EventType)
                    && ((DataAction)a).FormTypes.Any(s => s.Equals(businessObjInfo.FormTypeEx)));
                sboAction?.execute(businessObjInfo);
            }
            catch { throw; }
            return bubleEvent;
        }

        protected SAPbouiCOM.Form añadirForm()
        {
            System.Xml.XmlDocument lo_XMLForm = null;
            SAPbouiCOM.FormCreationParams lo_FrmCrtPrms = null;

            try
            {
                if (nombre != null && ruta != null)
                {
                    lo_XMLForm = new System.Xml.XmlDocument();
                    lo_FrmCrtPrms = sboApplication.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_FormCreationParams);
                    lo_XMLForm.Load(this.ruta);
                    lo_FrmCrtPrms.XmlData = lo_XMLForm.InnerXml;
                    lo_FrmCrtPrms.FormType = this.nombre;
                    lo_FrmCrtPrms.UniqueID = this.nombre;
                    return sboApplication.Forms.AddEx(lo_FrmCrtPrms);
                }
                else
                    return null;
            }
            catch { throw; }
        }

        protected abstract void sb_dataFormLoad();

        protected abstract void loadItemActions();

        protected abstract void loadDataActions();
    }
}
