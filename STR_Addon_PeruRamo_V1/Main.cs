using Newtonsoft.Json;
using STR_Addon_PeruRamo.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using STR_Addon_PeruRamo.MetaData;
using STR_Addon_PeruRamo.BL.APR;
using SAPbobsCOM;

namespace STR_Addon_PeruRamo_V1
{
    partial class Main
    {
        public SAPbouiCOM.Application sboApplication = null;
        public SAPbobsCOM.Company sboCompany = null;
        public int queryPosition = 0;

        public Main()
        {

            sbApplication();

            InitializationMD init = new InitializationMD();
            init.verificarInstalacion();
            setFilters();
            addMenus();
            loadEvents();

            sb_initialValid();
            sb_menuItem();

        }
        public void sb_initialValid()
        {
            string ls_AboutsSap = Validacion.fn_getBits();
            bool lb_is64 = ls_AboutsSap.Contains("64-bit");


            if (Validacion.fn_getComparacion(3) == true)
                sb_activacionCajaEAR(lb_is64);
            if (Validacion.fn_getComparacion(1) == true)
            {
                sb_activacionLocalizacion(lb_is64);
                sb_activacionLocaliPagosDet(lb_is64);
            }
            if (Validacion.fn_getComparacion(4) == true)
                sb_activacionLetras(lb_is64);
        }
        public void sb_activacionLocalizacion(bool pb_is64)
        {
            try
            {
                string dllPath = pb_is64
                           ? $"{Application.StartupPath}\\Librerias\\Localizacion64\\STR_Localizacionx64.dll"
                           : $"{Application.StartupPath}\\Librerias\\Localizacion86\\STR_Localizacionx86.dll";

                Assembly assembly = Assembly.LoadFrom(dllPath);

                Type programType = assembly.GetType("STR_Localizacion.UI.Cls_Main");

                object instancia = Activator.CreateInstance(programType);
                programType.GetConstructor(Type.EmptyTypes);
            }
            catch (Exception e)
            {
                sboApplication.statusBarErrorMsg(e.Message);
                throw;
            }
        }
        public void sb_activacionLocaliPagosDet(bool pb_is64)
        {
            try
            {
                string dllPath = pb_is64
                           ? $"{Application.StartupPath}\\Librerias\\Localizacion64\\BPP.dll"
                           : $"{Application.StartupPath}\\Librerias\\Localizacion86\\BPP.dll";

                Assembly assembly = Assembly.LoadFrom(dllPath);
                Type GlobalType = assembly.GetType("BPP.SAPInit");

                object instancia = Activator.CreateInstance(GlobalType);
                GlobalType.GetConstructor(Type.EmptyTypes);
            }
            catch (Exception e)
            {
                sboApplication.statusBarErrorMsg(e.Message);
                throw;

            }
        }
        public void sb_activacionCajaEAR(bool pb_is64)
        {
            try
            {
                string dllPath = pb_is64
                            ? $"{Application.StartupPath}\\Librerias\\CajaChicaEar64\\STR_CajaChica_Entregas.UL.dll"
                            : $"{Application.StartupPath}\\Librerias\\CajaChicaEar86\\STR_CajaChica_Entregas.UL.dll";

                Assembly assembly = Assembly.LoadFrom(dllPath);
                Type typeCLS = assembly.GetType("STR_CajaChica_Entregas.UL.Cls_Main");

                object claseEar = Activator.CreateInstance(typeCLS);
                typeCLS.GetConstructor(Type.EmptyTypes);
            }
            catch (Exception e)
            {
                sboApplication.statusBarErrorMsg(e.Message);
                throw;
            }
        }
        public void sb_activacionLetras(bool pb_is64)
        {
            try
            {
                string dllPath = pb_is64
                           ? $"{Application.StartupPath}\\Librerias\\Letras64\\STR_H_Letras.dll"
                           : $"{Application.StartupPath}\\Librerias\\Letras86\\STR_H_Letras.dll";

                Assembly assembly = Assembly.LoadFrom(dllPath);
                Type typeCLS = assembly.GetType("STR_H_Letras.ClassMain");

                object claseEar = Activator.CreateInstance(typeCLS);
                typeCLS.GetConstructor(Type.EmptyTypes);
            }
            catch (Exception e)
            {
                sboApplication.statusBarErrorMsg(e.Message);
                throw;
            }
        }
        public void sb_menuItem()
        {
            try
            {
                string uidmenuBPP = "MNU01";

                SAPbouiCOM.Menus oMenus = null;
                SAPbouiCOM.MenuItem oMenuItem = null;
                SAPbouiCOM.MenuCreationParams oCreationPackage = null;

                oMenuItem = sboApplication.Menus.Item("43520");
                oMenus = oMenuItem.SubMenus;

                // Verificar si el menú ya existe
                if (oMenus.Exists(uidmenuBPP))
                {
                    // Obtener referencia al menú existente
                    oMenuItem = oMenus.Item(uidmenuBPP);

                    // Actualizar la imagen del menú
                    string sPath = Application.StartupPath;
                    oMenuItem.Image = sPath + "\\Resources\\ramologo.png";  // Ruta de la nueva imagen

                }
                else
                {
                    // El menú no existe, puedes mostrar un mensaje o tomar alguna acción adicional si es necesario
                    Console.WriteLine("El menú no existe. No se puede actualizar.");
                }
                if (oMenus.Exists("MNULOCALI"))
                {
                    SAPbouiCOM.MenuItem lo_menuItem = null;


                    oMenuItem = oMenus.Item("MNULOCALI");

                    string sPath = Application.StartupPath;
                    oMenuItem.Image = sPath + "\\Resources\\localizar.png";
                }
                if (oMenus.Exists("MNUSIRE"))
                {
                    SAPbouiCOM.MenuItem lo_menuItem = null;


                    oMenuItem = oMenus.Item("MNUSIRE");

                    string sPath = Application.StartupPath;
                    oMenuItem.Image = sPath + "\\Resources\\sire.png";
                }
                if (oMenus.Exists("MNU_CCH_GENERAL"))
                {
                    SAPbouiCOM.MenuItem lo_menuItem = null;


                    oMenuItem = oMenus.Item("MNU_CCH_GENERAL");

                    string sPath = Application.StartupPath;
                    oMenuItem.Image = sPath + "\\Resources\\ccee.png";
                }
                if (oMenus.Exists("MNU_EAR_GENERAL"))
                {
                    SAPbouiCOM.MenuItem lo_menuItem = null;
                    oMenuItem = oMenus.Item("MNU_EAR_GENERAL");

                    string sPath = Application.StartupPath;
                    oMenuItem.Image = sPath + "\\Resources\\entrega.png";
                }
                if (oMenus.Exists("ST_Letras"))
                {
                    SAPbouiCOM.MenuItem lo_menuItem = null;
                    oMenuItem = oMenus.Item("ST_Letras");

                    string sPath = Application.StartupPath;
                    oMenuItem.Image = sPath + "\\Resources\\letras.png";
                }

            }
            catch (Exception e)
            {
                sboApplication.statusBarErrorMsg(e.Message);
                throw;
            }
            finally
            {
                sboApplication.Forms.GetFormByTypeAndCount(169, 1).Freeze(false);
                sboApplication.Forms.GetFormByTypeAndCount(169, 1).Update();
            }

        }
        public void sbApplication()
        {

            string connectionString = string.Empty;
            SAPbouiCOM.SboGuiApi sboGuiApi = null;

            try
            {
                sboGuiApi = new SAPbouiCOM.SboGuiApi();
                connectionString = Environment.GetCommandLineArgs()
                    .GetValue(Environment.GetCommandLineArgs().Length > 1 ? 1 : 0).ToString();
                sboGuiApi.Connect(connectionString);
                this.sboApplication = sboGuiApi.GetApplication(-1);
                if (this.sboApplication == null) throw new NullReferenceException();
                this.sboCompany = sboApplication.Company.GetDICompany();
                Global.go_sboApplictn = this.sboApplication;
                Global.go_sboCompany = this.sboCompany;
                Global.sboBob = sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoBridge);
                Global.gs_hardwarek = Validacion.fn_getHrdKey();

                Global.gi_queryPosition = sboCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB ? 1 : 0;

            }
            catch (Exception e)
            {
                sboApplication.statusBarErrorMsg(e.Message);
                throw;
            }


        }
        private void setFilters()
        {
            SAPbouiCOM.EventFilters eventFilters = new SAPbouiCOM.EventFilters();
            SAPbouiCOM.EventFilter eventFilter = null;

            eventFilter = eventFilters.Add(SAPbouiCOM.BoEventTypes.et_ALL_EVENTS);
            Enum.GetNames(typeof(FormularioUsuario)).Cast<string>().ToList().ForEach(s => eventFilter.AddEx(s));
            sboApplication.SetFilter(eventFilters);
        }
        /*
        public void validationServer()
        {

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.UseNagleAlgorithm = true;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.CheckCertificateRevocationList = true;

            string server = this.sboCompany.Server;
            string name = this.sboCompany.CompanyDB;

            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            HttpClient cliente = new HttpClient(handler);

            var body = new
            {
                ip = server,
                nomEmpresa = name,
            };

            string jsonBody = JsonConvert.SerializeObject(body);
            HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://localhost:44382/api/ObtenerServidor"),
                Method = HttpMethod.Post,
                Content = content
            };

            var response = cliente.SendAsync(request).Result;

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new Exception("Servicio se encuentra suspendido por falta de Pago");
        }*/
        private void addMenus()
        {
            XmlDocument xmlDocument = new XmlDocument();
            string rutaMenuXML = string.Empty;

            try
            {
                sboApplication.Forms.GetFormByTypeAndCount(169, 1).Freeze(true);
                rutaMenuXML = $"{Application.StartupPath}\\Menus\\Menu.xml";
                xmlDocument.Load(rutaMenuXML);
                sboApplication.LoadBatchActions(xmlDocument.InnerXml);
                sboApplication.statusBarSuccessMsg("El menu Addon Perú fue cargado correctamente");

            }
            catch (System.IO.FileNotFoundException)
            {
                sboApplication.statusBarErrorMsg("El recurso menu.xml, no fue encontrado");
            }
            catch (Exception e)
            {
                sboApplication.statusBarErrorMsg(e.Message);
                throw;

            }
            finally
            {
                sboApplication.Forms.GetFormByTypeAndCount(169, 1).Freeze(false);
                sboApplication.Forms.GetFormByTypeAndCount(169, 1).Update();
            }
        }

    }
}
