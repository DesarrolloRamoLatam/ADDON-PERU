using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STR_Addon_PeruRamo.Services
{
    public class DataAction : SBOActions
    {
        private SAPbouiCOM.BoEventTypes eventType;
        private string[] formTypes;
        private UIDataActionHandler uiDataAction;
        public string[] FormTypes { get => this.formTypes; }
        public SAPbouiCOM.BoEventTypes EventType { get => this.eventType; }

        public DataAction(SAPbouiCOM.BoEventTypes eventType, string formType, UIDataActionHandler uiDataAction)
        {
            this.eventType = eventType;
            this.formTypes = new string[] { formType };
            this.uiDataAction = uiDataAction;
        }

        public DataAction(SAPbouiCOM.BoEventTypes eventType, UIDataActionHandler uiDataAction, params string[] formTypes)
        {
            this.eventType = eventType;
            this.formTypes = formTypes;
            this.uiDataAction = uiDataAction;
        }

        public void execute(object sboEvent)
        {
            this.uiDataAction((SAPbouiCOM.BusinessObjectInfo)sboEvent);
        }
    }
}
