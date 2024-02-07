using Newtonsoft.Json;
using SAPbouiCOM;
using System;
using STR_Addon_PeruRamo.EL;
using STR_Addon_PeruRamo.Util;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using static STR_Addon_PeruRamo.Util.GblSire;
using STR_Addon_PeruRamo.EL.Sire;

namespace STR_Addon_PeruRamo.BL.Sire
{
    public class SIREServicVenta
    {
        public string ls_token;
        public string ls_urlDomn;
        public string ls_rutaArchv;
        public SIREServicVenta()
        {
            ls_urlDomn = "https://api-sire.sunat.gob.pe/v1/contribuyente/migeigv/libros";
            sb_connectSire();
            ls_rutaArchv = SIREMod.fn_setArchiv("Ventas");
        }

        public void sb_connectSire()
        {
            ls_token = SIREMod.fn_getConnectSire();
        }

        public List<RSPeriodos> fn_getPerSire()
        {
            try
            {
                string ls_uri = ls_urlDomn + $"/rvierce/padron/web/omisos/140000/periodos";

                var lo_client = new HttpClient();

                var lo_request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(ls_uri),
                    Method = HttpMethod.Get
                };

                lo_request.Headers.Add("Authorization", $"Bearer {ls_token}");

                var response = lo_client.SendAsync(lo_request).Result;


                List<RSPeriodos> lo_listaPrds = new List<RSPeriodos>();
                if (response.IsSuccessStatusCode)
                {
                    lo_listaPrds = System.Text.Json.JsonSerializer.Deserialize<List<RSPeriodos>>(response.Content.ReadAsStringAsync().Result);
                }
                else
                {
                   
                }
                return lo_listaPrds;
            }
            catch (Exception e)
            {

                RSerror ls_error = System.Text.Json.JsonSerializer.Deserialize<RSerror>(e.Message.ToString());

                throw new Exception($"Error al obtener periodos: {ls_error.errors[0].cod}-{ls_error.errors[0].msg}"); ;
            }


        }

        public string fn_getProcesoFase(string ps_perTri)
        {
            string ls_uri = ls_urlDomn + $"/rvierce/generales/web/control/{ps_perTri}/controlproceso?codLibro=140000";

            HttpClient lo_client = new HttpClient();

            HttpRequestMessage lo_request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(ls_uri),
            };

            lo_request.Headers.Add("Authorization", $"Bearer {ls_token}");

            var lo_response = lo_client.SendAsync(lo_request).Result;

            if (lo_response.StatusCode == HttpStatusCode.Unauthorized) // 401 Authorization Required
            {
                // Aquí puedes agregar lógica adicional si es necesario antes de volver a intentar la conexión
                sb_connectSire(); // Vuelve a intentar la conexión
                return fn_getProcesoFase(ps_perTri); // Llamada recursiva después de intentar la conexión nuevamente
            }
            lo_response.EnsureSuccessStatusCode();

            if (!lo_response.IsSuccessStatusCode)
                throw new Exception(lo_response.Content.ReadAsStringAsync().Result);

            string fase = System.Text.Json.JsonSerializer.Deserialize<RSTicket>(lo_response.Content.ReadAsStringAsync().Result).codFase;

            return fase;
        }


        public RSDocumentos fn_getPreliminares(string ps_perTri)
        {

            try
            {

                RSDocumentos lo_comPreliminares = new RSDocumentos();

                HttpClient lo_cliente = new HttpClient();

                // Propuestas

                // Preliminares Pre Finales

                // Preliminares Finales
                string ls_uri = ls_urlDomn + $"/rvierce/gestionlibro/web/registroslibros/{ps_perTri}/consultapreliminar?mtoTotalDesde=" +
                    $"&mtoTotalHasta=&fecDocumentoDesde=&fecDocumentoHasta=&numRucAdquiriente=&numCarSunat=&codTipoCDP" +
                    $"=&codTipoInconsistencia=&codEstadoRegistro=2&page=1&perPage=20&codLibro=140000";

                HttpRequestMessage lo_request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(ls_uri)
                };

                lo_request.Headers.Add("Authorization", $"Bearer {ls_token}");

                var lo_response = lo_cliente.SendAsync(lo_request).Result;

                if (!lo_response.IsSuccessStatusCode)
                    throw new Exception(lo_response.Content.ReadAsStringAsync().Result);

                lo_comPreliminares = System.Text.Json.JsonSerializer.Deserialize<RSDocumentos>(lo_response.Content.ReadAsStringAsync().Result);

                Paginacion paginacion = new Paginacion();
                List<Registros> registrosPrev = new List<Registros>();
                Totales totales = lo_comPreliminares.totales;

                int cantidad = lo_comPreliminares.paginacion.totalRegistros;

                var elementosPorGrupo = 20;


                int cantidadDeGrupos = (int)Math.Ceiling((double)cantidad / elementosPorGrupo);

                for (int i = 1; i < cantidadDeGrupos + 1; i++)
                {

                    string ls_uril = ls_urlDomn + $"/rvierce/gestionlibro/web/registroslibros/{ps_perTri}/consultapreliminar?mtoTotalDesde=" +
                      $"&mtoTotalHasta=&fecDocumentoDesde=&fecDocumentoHasti=&numRucAdquiriente=&numCarSunat=&codTipoCDP" +
                      $"=&codTipoInconsistencia=&codEstadoRegistro=2&page={i}&perPage=20&codLibro=140000";

                    var lo_requestF = new HttpRequestMessage()
                    {
                        RequestUri = new Uri(ls_uri),
                        Method = HttpMethod.Get
                    };

                    lo_requestF.Headers.Add("Authorization", $"Bearer {ls_token}");

                    var lo_responseF = lo_cliente.SendAsync(lo_requestF).Result;
                    if (!lo_responseF.IsSuccessStatusCode)
                        throw new Exception(lo_responseF.Content.ReadAsStringAsync().Result);

                    RSDocumentos rSComprobantes = System.Text.Json.JsonSerializer.Deserialize<RSDocumentos>
                        (lo_responseF.Content.ReadAsStringAsync().Result);

                    // paginacion = rSComprobantes.paginacion;

                    rSComprobantes.registros.ForEach((e) =>
                    {
                        registrosPrev.Add(e);
                    });
                }

                RSDocumentos rSDocumentos = new RSDocumentos()
                {
                    registros = registrosPrev,
                    paginacion = paginacion,
                    totales = totales
                };


                return lo_comPreliminares;

            }
            catch (Exception e)
            {

                RSerror ls_error = System.Text.Json.JsonSerializer.Deserialize<RSerror>(e.Message.ToString());

                throw new Exception($"Error al obtener comprobantes: {ls_error.errors[0].cod}-{ls_error.errors[0].msg}"); ;
            }
        }
        public RSDocumentos fn_getPreFinales(string ps_perTri)
        {

            try
            {
                // Consulta cantidad

                string ls_uri = ls_urlDomn + $"/rvierce/gestionlibro/web/registroslibros/{ps_perTri}/consultadetallepreliminarregistro" +
                    $"?codTipoRegistro=3&codLibro=140000&codOrigenEnvio=1&page=1&perPage=20";

                var lo_client = new HttpClient();

                var lo_request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(ls_uri),
                    Method = HttpMethod.Get
                };

                lo_request.Headers.Add("Authorization", $"Bearer {ls_token}");

                var lo_response = lo_client.SendAsync(lo_request).Result;

                if (!lo_response.IsSuccessStatusCode)
                    throw new Exception("Error al traer preliminares finales: " + lo_response.Content.ReadAsStringAsync().Result);

                // Inicializar Nuevamente y obtener la cantidad de totales 
                RSDocumentos lo_rSResumComp = System.Text.Json.JsonSerializer.Deserialize<RSDocumentos>
                  (lo_response.Content.ReadAsStringAsync().Result);

                Paginacion paginacion = new Paginacion();
                List<Registros> registrosPrev = new List<Registros>();
                Totales totales = lo_rSResumComp.totales;

                int cantidad = lo_rSResumComp.paginacion.totalRegistros;

                var elementosPorGrupo = 20;
                int cantidadDeGrupos = (int)Math.Ceiling((double)cantidad / elementosPorGrupo);

                for (int i = 1; i < cantidadDeGrupos + 1; i++)
                {
                    ls_uri = ls_urlDomn + $"/rvierce/gestionlibro/web/registroslibros/{ps_perTri}/consultadetallepreliminarregistro" +
                    $"?codTipoRegistro=3&codLibro=140000&codOrigenEnvio=1&page={i}&perPage=20";

                    var lo_requestF = new HttpRequestMessage()
                    {
                        RequestUri = new Uri(ls_uri),
                        Method = HttpMethod.Get
                    };

                    lo_requestF.Headers.Add("Authorization", $"Bearer {ls_token}");

                    var lo_responseF = lo_client.SendAsync(lo_requestF).Result;
                    if (!lo_responseF.IsSuccessStatusCode)
                        throw new Exception(lo_responseF.Content.ReadAsStringAsync().Result);

                    RSDocumentos rSComprobantes = System.Text.Json.JsonSerializer.Deserialize<RSDocumentos>
                        (lo_responseF.Content.ReadAsStringAsync().Result);

                    // paginacion = rSComprobantes.paginacion;

                    rSComprobantes.comprobantes.ForEach((e) =>
                    {
                        registrosPrev.Add(e);
                    });
                }

                RSDocumentos rSDocumentos = new RSDocumentos()
                {
                    comprobantes = registrosPrev,
                    paginacion = paginacion,
                    totales = totales
                };

                return rSDocumentos;
            }
            catch (Exception e)
            {

                RSerror ls_error = System.Text.Json.JsonSerializer.Deserialize<RSerror>(e.Message.ToString());

                throw new Exception($"Error al obtener comprobantes: {ls_error.errors[0].cod}-{ls_error.errors[0].msg}"); ;
            }
        }

        public RSDocumentos fn_comprobantesSire(string ps_perTri)
        {

            try
            {
                // Consulta cantidad

                string ls_uri = ls_urlDomn + $"/rvierce/resumen/web/resumen/{ps_perTri}/resumencomprobantes/rvie/?codTipoResumen=1";

                var lo_client = new HttpClient();

                var lo_request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(ls_uri),
                    Method = HttpMethod.Get
                };

                lo_request.Headers.Add("Authorization", $"Bearer {ls_token}");

                var lo_response = lo_client.SendAsync(lo_request).Result;

                if (lo_response.StatusCode == HttpStatusCode.Unauthorized) // 401 Authorization Required
                {
                    // Aquí puedes agregar lógica adicional si es necesario antes de volver a intentar la conexión
                    sb_connectSire(); // Vuelve a intentar la conexión
                    return fn_comprobantesSire(ps_perTri); // Llamada recursiva después de intentar la conexión nuevamente
                }

                if (!lo_response.IsSuccessStatusCode)
                    throw new Exception(lo_response.Content.ReadAsStringAsync().Result);


                var lo_rSResumComp = System.Text.Json.JsonSerializer.Deserialize<RSDocumentos>
                   (lo_response.Content.ReadAsStringAsync().Result);

                Paginacion paginacion = new Paginacion();
                List<Registros> registrosPrev = new List<Registros>();
                Totales totales = lo_rSResumComp.totales;

                var elementosPorGrupo = 20;
                int cantidadDeGrupos = (int)Math.Ceiling((double)lo_rSResumComp.totales.cntDocumentos / elementosPorGrupo);

                for (int i = 1; i < cantidadDeGrupos + 1; i++)
                {

                    ls_uri = ls_urlDomn + $"/rvie/propuesta/web/propuesta/{ps_perTri}/comprobantes?codTipoOpe=1&mtoDesde=&mtoHasta" +
                      $"=&fecEmisionIni=&fecEmisionFin=&numDocAdquiriente=&codCar" + // dd/MM/yyyy
                      $"=&codTipoCDP=&codInconsistencia=&page={i}&perPage={20}";

                    var lo_requestF = new HttpRequestMessage()
                    {
                        RequestUri = new Uri(ls_uri),
                        Method = HttpMethod.Get
                    };

                    lo_requestF.Headers.Add("Authorization", $"Bearer {ls_token}");

                    var lo_responseF = lo_client.SendAsync(lo_requestF).Result;
                    if (!lo_responseF.IsSuccessStatusCode)
                        throw new Exception(lo_responseF.Content.ReadAsStringAsync().Result);

                    RSDocumentos rSComprobantes = System.Text.Json.JsonSerializer.Deserialize<RSDocumentos>
                        (lo_responseF.Content.ReadAsStringAsync().Result);

                    paginacion = rSComprobantes.paginacion;

                    rSComprobantes.registros.ForEach((e) =>
                    {
                        registrosPrev.Add(e);
                    });

                }

                RSDocumentos rSDocumentos = new RSDocumentos()
                {
                    registros = registrosPrev,
                    paginacion = paginacion,
                    totales = totales
                };

                return rSDocumentos;
            }
            catch (Exception e)
            {
                RSerror ls_error = System.Text.Json.JsonSerializer.Deserialize<RSerror>(e.Message.ToString());

                throw new Exception($"Error al obtener comprobantes: {ls_error.errors[0].cod}-{ls_error.errors[0].msg}");
            }

        }

        public void sb_descargaPropuesta(string ps_perTri, string ps_carpeta)
        {

            try
            {
                string ls_uriVali = ls_urlDomn + $"/rvierce/gestionprocesosmasivos/web/masivo/consultaestadotickets?perIni=202201&perFin={ps_perTri}&page=1&perPage=2000&numTicket=";

                var lo_clientVali = new HttpClient();

                var lo_requestVali = new HttpRequestMessage()
                {
                    RequestUri = new Uri(ls_uriVali),
                    Method = HttpMethod.Get
                };

                lo_requestVali.Headers.Add("Authorization", $"Bearer {ls_token}");

                RSConsultaTicket lo_rsConsultTckt = System.Text.Json.JsonSerializer.Deserialize<RSConsultaTicket>(lo_clientVali.SendAsync(lo_requestVali).Result.Content.ReadAsStringAsync().Result);

                Registro lo_registro = lo_rsConsultTckt.registros.FirstOrDefault(x => x.perTributario == ps_perTri & x.codProceso == "10" & Convert.ToDateTime(x.fecInicioProceso).Day == DateTime.Now.Day);

                if (lo_registro != null)
                    sb_consultaEstado(ps_perTri, lo_registro.numTicket, ps_carpeta);
                else
                {
                    string ls_uriDescarga = ls_urlDomn + $"/rvie/propuesta/web/propuesta/{ps_perTri}/exportapropuesta?mtoTotalDesde=&mtoTotalHasta=&fecDocumentoDesde" +
                        $"=&fecDocumentoHasta=&numRucAdquiriente=&numCarSunat=&codTipoCDP=&codTipoInconsistencia=&codTipoArchivo=0";

                    var lo_client = new HttpClient();

                    var lo_request = new HttpRequestMessage()
                    {
                        RequestUri = new Uri(ls_uriDescarga),
                        Method = HttpMethod.Get
                    };

                    lo_request.Headers.Add("Authorization", $"Bearer {ls_token}");

                    RSTicket rsTicket = System.Text.Json.JsonSerializer.Deserialize<RSTicket>(lo_client.SendAsync(lo_request).Result.Content.ReadAsStringAsync().Result);
                    string ticket = rsTicket.numTicket;

                    if (string.IsNullOrEmpty(ticket))
                        throw new Exception("No se pudo descargar la propuesta");

                    sb_consultaEstado(ps_perTri, ticket, ps_carpeta);
                }
            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
        }

        public void sb_consultaEstado(string ps_perTri, string ps_ticket, string ps_carpeta)
        {
            RSConsultaTicket lo_rsConsultaTicket = new RSConsultaTicket();

            string ls_uri = ls_urlDomn + $"/rvierce/gestionprocesosmasivos/web/masivo/consultaestadotickets?perIni=202201&perFin={ps_perTri}&page=1&perPage=20&numTicket={ps_ticket}";

            var lo_client = new HttpClient();

            var lo_request = new HttpRequestMessage()
            {
                RequestUri = new Uri(ls_uri),
                Method = HttpMethod.Get
            };

            lo_request.Headers.Add("Authorization", $"Bearer {ls_token}");

            Thread.Sleep(5000);

            lo_rsConsultaTicket = System.Text.Json.JsonSerializer.Deserialize<RSConsultaTicket>(lo_client.SendAsync(lo_request).Result.Content.ReadAsStringAsync().Result);

            if (lo_rsConsultaTicket.registros[0].detalleTicket.codEstadoEnvio != "06")
            {
                throw new Exception("No se pudo descargar la propuesta, intentar mas tardé");
            }
            else
            {
                string ls_nomArchvRprt = lo_rsConsultaTicket.registros[0].archivoReporte[0].nomArchivoReporte;
                sb_descargaArchivo(ls_nomArchvRprt, ps_carpeta, true, ps_perTri);
            }
        }

        public void sb_descargaArchivo(string ps_nomArchvRprt, string ps_carpeta, bool pb_descomprime, string ps_perTri = null)
        {
            string ls_uri = ls_urlDomn + "/rvierce/gestionprocesosmasivos/web/masivo/archivoreporte?nomArchivoReporte" +
                $"={ps_nomArchvRprt}&codTipoArchivoReporte=0&codLibro=140000";
            var lo_client = new HttpClient();

            var lo_request = new HttpRequestMessage()
            {
                RequestUri = new Uri(ls_uri),
                Method = HttpMethod.Get
            };

            lo_request.Headers.Add("Authorization", $"Bearer {ls_token}");
            var lo_result = lo_client.SendAsync(lo_request).Result.Content.ReadAsByteArrayAsync().Result;

            string ls_rutArchv = GblSire.fn_createDrctry(this.ls_rutaArchv + "\\" + ps_carpeta);
            string ls_archvTemprl = ls_rutArchv + $"\\{ps_nomArchvRprt}";
            File.WriteAllBytes(ls_archvTemprl, lo_result);

            if (pb_descomprime)
            {
                using (ZipArchive lo_archivoZip = ZipFile.OpenRead(ls_archvTemprl))    //  Lee el archivo dentro de zip
                {
                    if (lo_archivoZip.Entries.Count > 0)
                    {
                        string ls_nombreArchvDntZp = lo_archivoZip.Entries[0].FullName;     // Nombre del archivo  dentro de zip
                        string ls_rutaArchvCpi = ls_rutArchv + $"\\{ls_nombreArchvDntZp}"; // Nombre del archivo que se va a copiar
                        string ls_rutArchvCopido = ls_rutArchv + $"\\{ps_perTri + ls_nombreArchvDntZp}"; // Nombre del archivo donde se copia

                        if (File.Exists(ls_rutArchvCopido))
                            File.Delete(ls_rutArchvCopido);

                        if (File.Exists(ls_rutaArchvCpi))
                            File.Delete(ls_rutaArchvCpi);

                        // Extraer el archivo ZIP con el nuevo nombre
                        lo_archivoZip.ExtractToDirectory(ls_rutArchv);
                        File.Copy(ls_rutaArchvCpi, ls_rutArchvCopido);

                        Global.go_sboApplictn.statusBarSuccessMsg("Exportado exitosamente en la carpeta: " + ls_rutArchvCopido);

                        File.Delete(ls_rutaArchvCpi);


                    }
                }

                File.Delete(ls_archvTemprl);
            }
        }
        public HttpResponseMessage fn_aceptarPropuesta(string ps_perTri)
        {
            try
            {
                string ls_uri = ls_urlDomn + $"/rvie/propuesta/web/propuesta/{ps_perTri}/aceptapropuesta";

                var lo_client = new HttpClient();
                var lo_request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(ls_uri),
                    Method = HttpMethod.Post
                };

                lo_request.Headers.Add("Authorization", $"Bearer {ls_token}");

                var lo_result = lo_client.SendAsync(lo_request).Result;

                return lo_result;
            }
            catch (Exception e)
            {
                RSerror ls_error = System.Text.Json.JsonSerializer.Deserialize<RSerror>(e.Message.ToString());

                throw new Exception($"Error al aceptar propuesta: {ls_error.errors[0].cod}-{ls_error.errors[0].msg}");

            }
        }

        public HttpResponseMessage fn_registrarPreliminar(string ps_perTri)
        {

            var ls_uri = ls_urlDomn + $"/rvierce/gestionlibro/web/registroslibros/{ps_perTri}/registrapreliminar";

            HttpClient lo_cliente = new HttpClient();

            HttpRequestMessage lo_request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(ls_uri),
            };

            lo_request.Headers.Add("Authorization", $"Bearer {ls_token}");

            HttpResponseMessage lo_response = lo_cliente.SendAsync(lo_request).Result;

            return lo_response;

        }

        public HttpResponseMessage fn_deleteRegistro(string ps_perTri)
        {

            try
            {
                var ls_uri = ls_urlDomn + $"/rvierce/gestionlibro/web/registroslibros/{ps_perTri}/eliminapreliminar?codLibro=140000";

                HttpClient lo_cliente = new HttpClient();

                HttpRequestMessage lo_request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri(ls_uri),
                };

                lo_request.Headers.Add("Authorization", $"Bearer {ls_token}");

                HttpResponseMessage lo_response = lo_cliente.SendAsync(lo_request).Result;

                return lo_response;

            }
            catch (Exception e)
            {
                RSerror ls_error = System.Text.Json.JsonSerializer.Deserialize<RSerror>(e.Message.ToString());

                throw new Exception($"Error al eliminar preliminar: {ls_error.errors[0].cod}-{ls_error.errors[0].msg}");
            }


        }
        public RSDocumentos fn_comprobantesSap(string ps_perTrI)
        {
            string ls_anio = ps_perTrI.Remove(4, 2);
            int ls_mes = int.Parse(ps_perTrI.Remove(0, 4));



            DateTime lo_fPrimerDia = new DateTime(int.Parse(ls_anio), ls_mes, 01);
            DateTime lo_fUltimoDia = lo_fPrimerDia.AddMonths(1).AddDays(-1);

            string ls_fprimer = lo_fPrimerDia.ToString("yyyy-MM-dd").Replace("-", "");
            string ls_fultimo = lo_fUltimoDia.ToString("yyyy-MM-dd").Replace("-", "");

            SAPbobsCOM.Recordset go_recordSet = Global.go_sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);


            if (Global.gi_queryPosition == 1)
                go_recordSet.DoQuery($"CALL STR_AV_RegistroVentas_Sire('{ls_fprimer}','{ls_fultimo}')");
            else
                go_recordSet.DoQuery($"EXEC STR_AV_RegistroVentas_Sire '{ls_fprimer}','{ls_fultimo}'");

            RSDocumentos lo_rSComprobantes = new RSDocumentos();

            List<Registros> lo_registros = new List<Registros>();
            Paginacion lo_paginacion = new Paginacion();

            Totales lo_totales = new Totales();



            double ld_total = 0.00;
            while (!go_recordSet.EoF)
            {

                //List<string> dtSap = new List<string>(rec.Fields.Item(2).Value.ToString().Split('|').ToList());
                string ls_dtSap = go_recordSet.Fields.Item(4).Value.ToString();
                List<string> lst_data = ls_dtSap.Split('|').ToList();


                Registros lo_registro = new Registros();

                List<DocumentoMod> lso_documentoMods = new List<DocumentoMod>();

                lo_registro.ObjectType = go_recordSet.Fields.Item(2).Value;
                lo_registro.DocumentEntry = go_recordSet.Fields.Item(3).Value;
                lo_registro.fecEmision = lst_data[4];
                lo_registro.codTipoCDP = lst_data[12] == "Anulado" ? $"{lst_data[6]} - Anulado" : lst_data[6];
                lo_registro.numSerieCDP = lst_data[7];
                lo_registro.nomRazonSocialCliente = lst_data[12];
                lo_registro.mtoBIGravada = string.IsNullOrEmpty(lst_data[14]) ? 0.00 : Convert.ToDouble(lst_data[14]);
                lo_registro.mtoIGV = string.IsNullOrEmpty(lst_data[16]) ? 0.00 : Convert.ToDouble(lst_data[16]);
                lo_registro.numCDP = lst_data[8];
                lo_registro.mtoTotalCP = Convert.ToDouble(lst_data[25]);
                lo_registro.codMoneda = lst_data[26];

                if (lst_data[29] != string.Empty)
                {
                    DocumentoMod lo_documentoMod = new DocumentoMod();
                    lo_documentoMod.codTipoCDPMod = lst_data[29];
                    lo_documentoMod.numSerieCDPMod = lst_data[30];
                    lo_documentoMod.numCDPMod = lst_data[31];
                }

                lo_registro.documentoMod = lso_documentoMods;
                // public List<DocumentoMod> documentoMod { get; set; }

                ld_total += lo_registro.mtoTotalCP;

                lo_registros.Add(lo_registro);
                go_recordSet.MoveNext();
            }
            lo_totales.mtoSumTotalCP = ld_total;

            lo_rSComprobantes.registros = lo_registros;
            lo_rSComprobantes.totales = lo_totales;
            lo_rSComprobantes.paginacion = lo_paginacion;
            return lo_rSComprobantes;
        }

        public Registro fn_descargaPSap(string ps_perTri, string ps_carpeta, bool pb_reemplaza)
        {

            string ls_anio = ps_perTri.Remove(4, 2);
            string ls_mes = ps_perTri.Remove(0, 4);

            DateTime lo_fPrimerDia = new DateTime(int.Parse(ls_anio), int.Parse(ls_mes), 01);
            DateTime lo_fUltimoDia = lo_fPrimerDia.AddMonths(1).AddDays(-1);

            string ls_fprimer = lo_fPrimerDia.ToString("yyyy-MM-dd").Replace("-", "");
            string ls_fultimo = lo_fUltimoDia.ToString("yyyy-MM-dd").Replace("-", "");

            SAPbobsCOM.Recordset go_recordrSet = Global.go_sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

            if (Global.gi_queryPosition == 1)
                go_recordrSet.DoQuery($"CALL STR_AV_RegistroVentas_Sire('{ls_fprimer}','{ls_fultimo}')");
            else
                go_recordrSet.DoQuery($"EXEC STR_AV_RegistroVentas_Sire '{ls_fprimer}','{ls_fultimo}'");

            string ls_texto = "";
            string ls_ruc = "";

            while (!go_recordrSet.EoF)
            {
                ls_ruc = go_recordrSet.Fields.Item(0).Value;

                ls_texto += go_recordrSet.Fields.Item(4).Value;

                go_recordrSet.MoveNext();
                if (!go_recordrSet.EoF)
                    ls_texto += "\n";
            }



            //ZipFile.CreateFromDirectory(Path.GetDirectoryName(rutaCompleta + ".txt"), rutaCompleta + ".zip",CompressionLevel.Optimal, false);

            // Se llama a las facturas fisicas que estén registradas en Sunat

            RSDocumentos lo_compsSire = fn_comprobantesSire(ps_perTri);
            List<Registros> lot_comps = lo_compsSire.registros;
            if (lot_comps.Count > 0)
            {
                List<Registros> lot_comFis = lot_comps.Where(com =>
                com.numSerieCDP.Remove(1) != "F" & com.numSerieCDP.Remove(1) != "B").ToList();

                for (int i = 0; i < lot_comFis.Count; i++)
                {
                    ls_texto += "\n";

                    ls_texto += $"{lot_comFis[i].numRuc}|{lot_comFis[i].nomRazonSocial}|{lot_comFis[i].perPeriodoTributario}|" +
                        $"|{lot_comFis[i].fecEmision}|{(lot_comFis[i].codTipoCDP == "14" ? lot_comFis[i].fecVencPag : "")}|{lot_comFis[i].codTipoCDP}|{lot_comFis[i].numSerieCDP}|{lot_comFis[i].numCDP}" +
                        $"||{lot_comFis[i].codTipoDocIdentidad}|{lot_comFis[i].numRuc}|{lot_comFis[i].nomRazonSocialCliente}|" +
                        $"{lot_comFis[i].mtoValFactExpo.ToString("F2")}|{lot_comFis[i].mtoBIGravada.ToString("F2")}" +
                        $"|{lot_comFis[i].mtoDsctoBI.ToString("F2")}|{lot_comFis[i].mtoIGV.ToString("F2")}|{lot_comFis[i].mtoDsctoIGV.ToString("F2")}|{lot_comFis[i].mtoExonerado.ToString("F2")}|" +
                        $"{lot_comFis[i].mtoInafecto.ToString("F2")}|{lot_comFis[i].mtoISC.ToString("F2")}|{lot_comFis[i].mtoBIIvap.ToString("F2")}|{lot_comFis[i].mtoIvap.ToString("F2")}|" +
                        $"{lot_comFis[i].mtoIcbp.ToString("F2")}|{lot_comFis[i].mtoOtrosTrib.ToString("F2")}|{lot_comFis[i].mtoTotalCP.ToString("F2")}" +
                        $"|{lot_comFis[i].codMoneda}|{lot_comFis[i].mtoTipoCambio}|";

                    if (lot_comFis[i].documentoMod.Count > 0)
                    {
                        for (int d = 0; d < lot_comFis[i].documentoMod.Count; i++)
                        {
                            ls_texto += $"{lot_comFis[i].documentoMod[d].fecEmisionMod}|{lot_comFis[i].documentoMod[d].codTipoCDPMod}|" +
                                $"{lot_comFis[i].documentoMod[d].numSerieCDPMod}|{lot_comFis[i].documentoMod[d].numCDPMod}|";
                        }
                    }
                    else
                    {
                        ls_texto += "||||";
                    }

                    ls_texto += "|"; //CONTRATO


                }
            }
            // Se genera el archivo

            string ls_nombreArchv = "LE" + ls_ruc + ls_anio + ls_mes + "00" + "140400021112";
            string ls_rutaCompleta = GblSire.fn_createDrctry($"{ls_rutaArchv}\\{ps_carpeta}") + "\\" + ls_nombreArchv;

            File.WriteAllText(ls_rutaCompleta + ".txt", ls_texto);

            if (File.Exists(ls_rutaCompleta + ".zip"))
            {
                File.Delete(ls_rutaCompleta + ".zip");
            }

            using (ZipArchive lo_archivoZp = ZipFile.Open(ls_rutaCompleta + ".zip", ZipArchiveMode.Create))
            {
                // Agregar un archivo al archivo ZIP
                lo_archivoZp.CreateEntryFromFile(ls_rutaCompleta + ".txt", Path.GetFileName(ls_rutaCompleta + ".txt"));
            }
            if (pb_reemplaza)
                return fn_reemplazaPropuesta(ls_rutaCompleta + ".zip", fn_obtieneIdTus(ls_rutaCompleta + ".zip", ls_nombreArchv + ".zip", ls_ruc, ps_perTri));
            else
                return new Registro()
                {
                    nomArchivoImportacion = ls_rutaCompleta + ".zip"
                };
        }


        public void sb_exportDiferencias(Grid go_grilla, string gs_perTri)
        {

            string ls_text = "";

            if (go_grilla.Rows.Count > 0)
            {
                for (int i = 0; i < go_grilla.Rows.Count; i++)
                {

                    ls_text += go_grilla.DataTable.GetValue("F. Emisión", i) + "|";
                    ls_text += go_grilla.DataTable.GetValue("Tipo de documento", i) + "|";
                    ls_text += go_grilla.DataTable.GetValue("Serie", i) + "|";
                    ls_text += go_grilla.DataTable.GetValue("Correlativo", i) + "|";
                    ls_text += go_grilla.DataTable.GetValue("Tipo de Moneda", i) + "|";
                    ls_text += go_grilla.DataTable.GetValue("Total Sap", i) + "|";
                    ls_text += go_grilla.DataTable.GetValue("Total Sire", i) + "|";
                    ls_text += go_grilla.DataTable.GetValue("Observacion", i) + "|";
                    ls_text += "\n";
                }


                string ls_nombreArchivo = "LE" + gs_perTri + "EXP.txt";

                string ls_rutaCompleta = GblSire.fn_createDrctry(ls_rutaArchv + "\\Exportaciones\\Comparaciones") + "\\" + ls_nombreArchivo;

                File.WriteAllText(ls_rutaCompleta, ls_text);


                Global.go_sboApplictn.statusBarSuccessMsg("Exportado exitosamente en la carpeta: " + ls_rutaCompleta);

            }
        }

        public string fn_obtieneIdTus(string ps_rutArchv, string ps_nomArchv, string ps_ruc, string ps_perTri)
        {

            // Genera el upload Metada

            string ls_uri = ls_urlDomn + "/rvierce/receptorpropuesta/web/propuesta/upload";
            string ls_idTus = "";
            HttpClient lo_client = new HttpClient();



            // Convierte a metadata los datos --------------------------------------------------------------

            byte[] lby_nameArchivo = Encoding.UTF8.GetBytes(ps_nomArchv);
            var ls_filename = Convert.ToBase64String(lby_nameArchivo);

            byte[] lby_archivoTipo = Encoding.UTF8.GetBytes("application/x-zip-compressed");
            var ls_filetype = Convert.ToBase64String(lby_archivoTipo);

            byte[] lby_numeroRuc = Encoding.UTF8.GetBytes(ps_ruc);
            var ls_numRuc = Convert.ToBase64String(lby_numeroRuc);

            byte[] lby_periodoTri = Encoding.UTF8.GetBytes(ps_perTri);
            var ls_perTributario = Convert.ToBase64String(lby_periodoTri);

            byte[] lby_codigoOrigen = Encoding.UTF8.GetBytes("2");
            var ls_codOrigenEnvio = Convert.ToBase64String(lby_codigoOrigen);

            byte[] lby_codigoProceso = Encoding.UTF8.GetBytes("3");
            var ls_codProceso = Convert.ToBase64String(lby_codigoProceso);

            //----------------------------------------------------------------------------------

            string ls_uploadMetadata = $"filename {ls_filename}," +
                $"filetype {ls_filetype}," +
                $"numRuc {ls_numRuc},perTributario {ls_perTributario}," +
                $"codOrigenEnvio {ls_codOrigenEnvio}," +
                $"codProceso {ls_codProceso},codTipoCorrelativo MQ==," + // 01 - 
                $"nomArchivoImportacion {ls_filename}," +
                $"codLibro MTQwMDAw";

            //string metadata = "filename TEUyMDUxMTY0OTYyNjIwMjMwNTAwMTQwNDAwMDIxMTEyLnppcA==,filetype YXBwbGljYXRpb24veC16aXAtY29tcHJlc3NlZA==,numRuc MjA1MTE2NDk2MjY=,perTributario MjAyMzA1,codOrigenEnvio MQ==,codProceso NTg=,codTipoCorrelativo MDQ=,nomArchivoImportacion TEUyMDUxMTY0OTYyNjIwMjMwNTAwMTQwNDAwMDIxMTEyLnppcA==,codLibro MTQwMDAw";

            HttpRequestMessage lo_request = new HttpRequestMessage()
            {
                RequestUri = new Uri(ls_uri),
                Method = HttpMethod.Post
            };

            FileInfo lo_fileInfo = new FileInfo(ps_rutArchv);
            string ls_lengthFile = lo_fileInfo.Length.ToString();

            lo_request.Headers.Add("Authorization", $"Bearer {ls_token}");
            lo_request.Headers.Add("Tus-Resumable", "1.0.0");
            lo_request.Headers.Add("Upload-Length", ls_lengthFile);
            lo_request.Headers.Add("Upload-Metadata", ls_uploadMetadata);

            var lo_response = lo_client.SendAsync(lo_request).Result;

            // Capturas la cabecera el location


            if (lo_response.IsSuccessStatusCode)
            {
                var lo_location = lo_response.Headers.Location;
                string ls_location = Convert.ToString(lo_location);
                ls_idTus = ls_location.Split('/')[ls_location.Split('/').Length - 1];
            }
            else
            {
                throw new Exception("Error al conectarse con Sire: " + lo_response.Content.ReadAsStringAsync().Result);
            }
            return ls_idTus;

        }

        public Registro fn_reemplazaPropuesta(string ps_rutaArchivo, string ps_id)
        {

            try
            {
                string ls_uri = ls_urlDomn + $"/rvierce/receptorpropuesta/web/propuesta/upload/{ps_id}";

                HttpClient lo_client = new HttpClient();

                byte[] lby_archivoBin = File.ReadAllBytes(ps_rutaArchivo);

                ByteArrayContent lo_content = new ByteArrayContent(lby_archivoBin);
                lo_content.Headers.ContentType = new MediaTypeHeaderValue("application/offset+octet-stream");

                FileInfo lo_fileInfo = new FileInfo(ps_rutaArchivo);
                string ls_lengthFile = lo_fileInfo.Length.ToString();

                HttpRequestMessage lo_request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(ls_uri),
                    Method = new HttpMethod("PATCH"),
                    Content = lo_content
                };

                lo_request.Headers.Add("Authorization", $"Bearer {ls_token}");
                lo_request.Headers.Add("Tus-Resumable", "1.0.0");
                lo_request.Headers.Add("Upload-Length", ls_lengthFile);
                lo_request.Headers.Add("Upload-Offset", "0");


                var lo_response = lo_client.SendAsync(lo_request).Result;

                if (lo_response.IsSuccessStatusCode)
                {

                    string ls_ticket = lo_response.Content.ReadAsStringAsync().Result;

                    HttpClient lo_clien = new HttpClient();

                    string ls_periodo = DateTime.Now.ToString("yyyy-MM").Replace("-", "");

                    HttpRequestMessage lo_requestMesage = new HttpRequestMessage()
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri(ls_urlDomn + $"/rvierce/gestionprocesosmasivos/web/masivo/consultaestadotickets?perIni=202201&perFin={ls_periodo}&page=1&perPage=200&numTicket={ls_ticket}&codProceso="),
                    };

                    lo_requestMesage.Headers.Add("Authorization", $"Bearer {ls_token}");

                    System.Threading.Thread.Sleep(5000); ;

                    lo_response = lo_clien.SendAsync(lo_requestMesage).Result;

                    if (!lo_response.IsSuccessStatusCode)
                        throw new Exception(lo_response.Content.ReadAsStringAsync().Result);


                    var ls_respusta = lo_response.Content.ReadAsStringAsync().Result;
                    List<Registro> lo_responseTckt = System.Text.Json.JsonSerializer.Deserialize<RSConsultaTicket>(lo_response.Content.ReadAsStringAsync().Result).registros;

                    return lo_responseTckt[0];
                }

                throw new Exception(lo_response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception e)
            {
                RSerror ls_error = System.Text.Json.JsonSerializer.Deserialize<RSerror>(e.Message.ToString());

                throw new Exception($"Error al reemplazar propuesta: {ls_error.errors[0].cod}-{ls_error.errors[0].msg}");
            }
        }
        public HttpResponseMessage fn_eliminarPreliminar(string ps_perTri)
        {
            try
            {
                string ls_uri = ls_urlDomn + $"/rvierce/gestionlibro/web/registroslibros/{ps_perTri}/eliminarreemplazo?codLibro=140000";

                HttpClient lo_cliente = new HttpClient();

                var lo_request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(ls_uri),
                    Method = HttpMethod.Put
                };

                lo_request.Headers.Add("Authorization", $"Bearer {ls_token}");

                HttpResponseMessage lo_response = lo_cliente.SendAsync(lo_request).Result;

                return lo_response;
            }
            catch (Exception e)
            {

                RSerror ls_error = System.Text.Json.JsonSerializer.Deserialize<RSerror>(e.Message.ToString());

                throw new Exception($"Error al eliminar preliminar: {ls_error.errors[0].cod}-{ls_error.errors[0].msg}");
            }



        }

        public HttpResponseMessage fn_eliminarPreFinal(string ps_perTri)
        {

            try
            {
                string ls_uri = ls_urlDomn + $"/rvierce/gestionlibro/web/registroslibros/{ps_perTri}/eliminapreliminar?codLibro=140000";

                Registros lo_preliminar = fn_getPreliminar(ps_perTri);

                HttpClient lo_cliente = new HttpClient();

                var lo_body = new
                {
                    id = lo_preliminar.id,
                    codTipoRegistro = "3"
                };

                string ls_jsonBody = JsonConvert.SerializeObject(lo_body);
                HttpContent lo_content = new StringContent(ls_jsonBody, Encoding.UTF8, "application/json");

                var lo_request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(ls_uri),
                    Method = HttpMethod.Put,
                    Content = lo_content
                };

                lo_request.Headers.Add("Authorization", $"Bearer {ls_token}");

                HttpResponseMessage lo_response = lo_cliente.SendAsync(lo_request).Result;

                return lo_response;
            }
            catch (Exception e)
            {
                RSerror ls_error = System.Text.Json.JsonSerializer.Deserialize<RSerror>(e.Message.ToString());

                throw new Exception($"Error al eliminar preliminar: {ls_error.errors[0].cod}-{ls_error.errors[0].msg}");
            }
        }

        public Registros fn_getPreliminar(string ps_perTri)
        {


            try
            {
                string ls_uri = ls_urlDomn + $"/rvierce/gestionlibro/web/registroslibros/consultapreliminaresregistro?" +
                $"page=1&perPage=100&perIni=202001&perFin={ps_perTri}";

                HttpClient lo_cliente = new HttpClient();

                HttpRequestMessage lo_request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(ls_uri),
                    Method = HttpMethod.Get,
                };

                lo_request.Headers.Add("Authorization", $"Bearer {ls_token}");

                var lo_response = lo_cliente.SendAsync(lo_request).Result;

                if (lo_response.IsSuccessStatusCode)
                {

                    RSDocumentos lo_rSDocumentos = System.Text.Json.JsonSerializer.Deserialize<RSDocumentos>(lo_response.Content.ReadAsStringAsync().Result);

                    List<Registros> lot_registros = lo_rSDocumentos.registros.Where(re => re.perTributario == ps_perTri).ToList();

                    return lot_registros[0];
                }
                else
                    throw new Exception(lo_response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception e)
            {

                RSerror ls_error = System.Text.Json.JsonSerializer.Deserialize<RSerror>(e.Message.ToString());

                throw new Exception($"Error al obtener preliminar: {ls_error.errors[0].cod}-{ls_error.errors[0].msg}");
            }
        }
    }
}
