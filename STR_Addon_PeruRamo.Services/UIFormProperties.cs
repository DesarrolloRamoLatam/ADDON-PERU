using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STR_Addon_PeruRamo.Services
{
    public abstract partial class UIForm : IDisposable
    {
        protected SAPbouiCOM.Application sboApplication;
        protected SAPbobsCOM.Company sboCompany;
        //Form Items
        protected SAPbouiCOM.Form form = null;
        protected SAPbouiCOM.Item item = null;
        protected SAPbouiCOM.StaticText statictext = null;
        protected SAPbouiCOM.EditText editText = null;
        protected SAPbouiCOM.ComboBox comboBox = null;
        protected SAPbouiCOM.Button button = null;
        protected SAPbouiCOM.Grid grid = null;
        protected SAPbouiCOM.Matrix matrix = null;
        protected SAPbouiCOM.OptionBtn optionButton = null;
        protected SAPbouiCOM.CheckBox checkBox = null;
        protected SAPbouiCOM.Folder folder = null;
        protected SAPbouiCOM.LinkedButton linkedButton = null;
        protected SAPbouiCOM.GridColumn gridColumn = null;
        protected SAPbouiCOM.EditTextColumn editTextColumn = null;

        public void Dispose()
        {
            form = null;
            item = null;
            statictext = null;
            editText = null;
            comboBox = null;
            button = null;
            grid = null;
            matrix = null;
            optionButton = null;
            checkBox = null;
            folder = null;
            linkedButton = null;
            gridColumn = null;
            editTextColumn = null;
        }
    }
}
