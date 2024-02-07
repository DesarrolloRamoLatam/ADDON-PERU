using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STR_Addon_PeruRamo.Util
{
    public static class UIExtensions
    {
        public static int iDocEntry = 0;
        public static void statusBarWarningMsg(this SAPbouiCOM.Application application, string message)
        {
            application.StatusBar.SetText(message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
        }

        public static void statusBarSuccessMsg(this SAPbouiCOM.Application application, string message)
        {
            application.StatusBar.SetText(message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
        }

        public static void statusBarErrorMsg(this SAPbouiCOM.Application application, string message)
        {
            application.StatusBar.SetText(message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="form"></param>
        /// <param name="isenabled">Define whether the item is enabled or not</param>
        /// <param name="items">Item to appply estate</param>
        public static void enabledItems(this SAPbouiCOM.Form form, bool isEnabled, params string[] items)
        {
            foreach (var item in items)
            {
                form.Items.Item(item).Enabled = isEnabled;
            }
        }

        public static void comboboxLoadData(this SAPbouiCOM.ComboBox combobox, SAPbobsCOM.Recordset recordSet, bool addInitValue = false)
        {
            try
            {
                while (combobox.ValidValues.Count > 0)
                    combobox.ValidValues.Remove(0, SAPbouiCOM.BoSearchKey.psk_Index);

                if (addInitValue)
                    combobox.ValidValues.Add(" - - ", " - - ");

                while (!recordSet.EoF)
                {
                    combobox.ValidValues.Add(recordSet.Fields.Item(0).Value, recordSet.Fields.Item(1).Value);
                    recordSet.MoveNext();
                }
            }
            catch { throw; }
            finally { recordSet = null; combobox = null; }
        }

        public static void visibleGridColumns(this SAPbouiCOM.Grid grid, bool isVisible, params string[] columns)
        {
            foreach (var column in columns)
            {
                grid.Columns.Item(column).Visible = isVisible;
            }
        }

        public static void editableGridColumns(this SAPbouiCOM.Grid grid, bool isEditable, params string[] columns)
        {
            foreach (var column in columns)
            {
                grid.Columns.Item(column).Editable = isEditable;
            }
        }

        public static void visibleItems(this SAPbouiCOM.Form form, bool isVisible, params string[] items)
        {
            foreach (var item in items)
            {
                form.Items.Item(item).Visible = isVisible;
            }
        }

        public static SAPbouiCOM.Item getItem(this SAPbouiCOM.Form form, string itemID)
        {
            return form.Items.Item(itemID);
        }

        public static SAPbouiCOM.ComboBox getCombobox(this SAPbouiCOM.Form form, string itemID)
        {
            return (SAPbouiCOM.ComboBox)form.Items.Item(itemID).Specific;
        }

        public static SAPbouiCOM.StaticText getStaticText(this SAPbouiCOM.Form form, string itemID)
        {
            return (SAPbouiCOM.StaticText)form.Items.Item(itemID).Specific;
        }

        public static SAPbouiCOM.OptionBtn getOptionBtn(this SAPbouiCOM.Form form, string itemID)
        {
            return (SAPbouiCOM.OptionBtn)form.Items.Item(itemID).Specific;
        }

        public static SAPbouiCOM.EditText getEditText(this SAPbouiCOM.Form form, string itemID)
        {
            return (SAPbouiCOM.EditText)form.Items.Item(itemID).Specific;
        }

        public static SAPbouiCOM.LinkedButton getLnkButton(this SAPbouiCOM.Form form, string itemID)
        {
            return (SAPbouiCOM.LinkedButton)form.Items.Item(itemID).Specific;
        }

        public static SAPbouiCOM.Grid getGrid(this SAPbouiCOM.Form form, string itemID)
        {
            return (SAPbouiCOM.Grid)form.Items.Item(itemID).Specific;
        }

        public static SAPbouiCOM.EditTextColumn getEditTextColumn(this SAPbouiCOM.Grid grid, string columnID)
        {
            return (SAPbouiCOM.EditTextColumn)grid.Columns.Item(columnID);
        }

  
        public static SAPbouiCOM.Button getButton(this SAPbouiCOM.Form form, string itemID)
        {
            return (SAPbouiCOM.Button)form.Items.Item(itemID).Specific;
        }

        public static SAPbouiCOM.UserDataSource getUserDataSource(this SAPbouiCOM.Form form, string dataSourceID)
        {
            return form.DataSources.UserDataSources.Item(dataSourceID);
        }

        public static SAPbouiCOM.DBDataSource getDBDataSource(this SAPbouiCOM.Form form, string dataSourceID)
        {
            return form.DataSources.DBDataSources.Item(dataSourceID);
        }

        public static SAPbouiCOM.CheckBox getCheckBox(this SAPbouiCOM.Form form, string itemID)
        {
            return (SAPbouiCOM.CheckBox)form.Items.Item(itemID).Specific;
        }

        public static string getDBHeaderFieldValue(this SAPbouiCOM.Form form, string fieldID)
        {
            return form.DataSources.DBDataSources.Item(0).GetValue(fieldID, 0);
        }
        public static string fn_FormGetValueFromDBDataSource(this SAPbouiCOM.Form form, string ps_tabla, string ps_campo)
        {
            return form.DataSources.DBDataSources.Item(ps_tabla).GetValue(ps_campo, 0).Trim();
        }



    }
}
