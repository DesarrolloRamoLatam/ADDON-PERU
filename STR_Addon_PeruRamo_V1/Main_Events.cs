using STR_Addon_PeruRamo.Services;
using STR_Addon_PeruRamo.Util;
using STR_Addon_PeruRamo_V1.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace STR_Addon_PeruRamo_V1
{
    partial class Main
    {
        private void loadEvents()
        {
            try
            {
                sboApplication.AppEvent += SboApplication_AppEvent;
                sboApplication.MenuEvent += SboApplication_MenuEvent;
                sboApplication.ItemEvent += SboApplication_ItemEvent;
                sboApplication.FormDataEvent += SboApplication_FormDataEvent;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void SboApplication_FormDataEvent(ref SAPbouiCOM.BusinessObjectInfo businessObjectInfo, out bool BubbleEvent)
        {
            BubbleEvent = true;
            UIForm uiform = null;

            try
            {
                uiform = UIFormFactory.getForm(businessObjectInfo.FormTypeEx);
                uiform?.dataEvent(businessObjectInfo);
            }
            catch (Exception ex)
            {
                BubbleEvent = false;
                sboApplication.statusBarErrorMsg(ex.Message);
            }
        }

        private void SboApplication_ItemEvent(string FormUID, ref SAPbouiCOM.ItemEvent itemEvent, out bool BubbleEvent)
        {
            BubbleEvent = true;
            UIForm uiform = null;
            try
            {
                if (!string.IsNullOrEmpty(itemEvent.FormTypeEx))
                {
                    uiform = UIFormFactory.getForm(itemEvent.FormTypeEx);
                    uiform?.perform(itemEvent);
                }
            }
            catch (Exception ex)
            {
                BubbleEvent = false;
                sboApplication.statusBarErrorMsg(ex.Message);
            }
        }

        public void SboApplication_MenuEvent(ref SAPbouiCOM.MenuEvent menuEvent, out bool bubbleEvent)
         {
            bubbleEvent = true;
            UIForm uiform = null;
            try
            {
                if (!menuEvent.BeforeAction)
                {
                    uiform = UIFormFactory.getForm(menuEvent.MenuUID);
                    uiform?.formLoad();
                }
            }
            catch (Exception ex)
            {
                sboApplication.statusBarErrorMsg(ex.Message);
            }
        }

        private void SboApplication_AppEvent(SAPbouiCOM.BoAppEventTypes eventType)
        {
            try
            {
                switch (eventType)
                {
                    case SAPbouiCOM.BoAppEventTypes.aet_ShutDown:
                    case SAPbouiCOM.BoAppEventTypes.aet_CompanyChanged:
                    case SAPbouiCOM.BoAppEventTypes.aet_LanguageChanged:
                    case SAPbouiCOM.BoAppEventTypes.aet_ServerTerminition:
                        Application.Exit();
                        break;
                    case SAPbouiCOM.BoAppEventTypes.aet_FontChanged:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                sboApplication.statusBarErrorMsg(ex.Message);
            }
        }
    }
}
