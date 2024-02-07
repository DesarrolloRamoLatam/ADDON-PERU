using STR_Addon_PeruRamo.BL.APR;
using STR_Addon_PeruRamo.Services;
using STR_Addon_PeruRamo.Util;
using STR_Addon_PeruRamo_V1.Forms.USRForms;
using STR_Addon_PeruRamo_V1.Forms.USRForms.Sire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STR_Addon_PeruRamo_V1.Forms
{
    class UIFormFactory
    {

        public static UIForm getForm(string ps_formID)
        {
            try
            {

                if (ps_formID == FormMnuPE.gs_menu || ps_formID == FormMnuPE.gs_nombre)
                    return FormMnuPE.getForm();

                if (ps_formID == FormSireVentas.gs_menu || ps_formID == FormSireVentas.gs_nombre)
                {
                    if (Validacion.fn_getComparacion(2) == true)
                        return FormSireVentas.getForm();
                    else
                        return null;
                }
                if (ps_formID == FormSireCompras.gs_menu || ps_formID == FormSireCompras.gs_nombre)
                {
                    if (Validacion.fn_getComparacion(2) == true)
                        return FormSireCompras.getForm();
                    else
                        return null;
                }

                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
