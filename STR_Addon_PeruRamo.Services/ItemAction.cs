using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STR_Addon_PeruRamo.Services
{
    public class ItemAction : SBOActions
    {
        private SAPbouiCOM.BoEventTypes eventType;
        private string itemId;
        private UIItemActionHandler uiActionHandler;
        private UIFunctionHandler uiFunctionHandler;

        public SAPbouiCOM.BoEventTypes EventType { get => eventType; }
        public string ItemID { get => itemId; }

        public ItemAction(SAPbouiCOM.BoEventTypes eventType, string itemId, UIItemActionHandler uiActionHandler)
        {
            this.eventType = eventType;
            this.itemId = itemId;
            this.uiActionHandler = uiActionHandler;
        }

        public ItemAction(SAPbouiCOM.BoEventTypes eventType, string itemId, UIFunctionHandler uiFunctionHandler)
        {
            this.eventType = eventType;
            this.itemId = itemId;
            this.uiFunctionHandler = uiFunctionHandler;
        }

        public void execute(object sboEvent)
        {
            this.uiActionHandler((SAPbouiCOM.ItemEvent)sboEvent);
        }
    }
}
