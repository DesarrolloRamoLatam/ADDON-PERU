using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STR_Addon_PeruRamo.Services
{
    public delegate void UIItemActionHandler(SAPbouiCOM.ItemEvent param);
    public delegate bool UIFunctionHandler(SAPbouiCOM.ItemEvent param);
    public delegate void UIDataActionHandler(SAPbouiCOM.BusinessObjectInfo param);
    public interface SBOActions
    {
        void execute(object sboEvent);
    }
}
