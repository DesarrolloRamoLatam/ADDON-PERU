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
using SAPbobsCOM;
using STR_Addon_PeruRamo.EL.Sire;
using System.Net.Sockets;

namespace STR_Addon_PeruRamo.BL.Sire
{
    public class SIREServicCompra
    {
        public string token;
        public string ruta;
        public string rutaArchivoTxt;
        public SIREServicCompra()
        {
            ruta = "https://api-sire.sunat.gob.pe/v1/contribuyente/migeigv/libros";
            ConnectSire();
            rutaArchivoTxt = SIREMod.fn_setArchiv("Compras");
        }

        public void ConnectSire()
        {
            try
            {
                token = SIREMod.fn_getConnectSire();
            }
            catch (Exception)
            {
                throw new Exception("Error al conectarse al Sire. Intentar mas tarde");
            }
        }

        public List<RSPeriodos> fn_getPerSire()
        {

            try
            {

                string uri = ruta + $"/rvierce/padron/web/omisos/080000/periodos";

                var client = new HttpClient();

                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Get
                };

                request.Headers.Add("Authorization", $"Bearer {token}");

                List<RSPeriodos> listaPeriodos = System.Text.Json.JsonSerializer.Deserialize<List<RSPeriodos>>(client.SendAsync(request).Result.Content.ReadAsStringAsync().Result);

                return listaPeriodos;
            }
            catch (Exception e)
            {

                RSerror ls_error = System.Text.Json.JsonSerializer.Deserialize<RSerror>(e.Message.ToString());

                throw new Exception($"Error al obtener comprobantes: {ls_error.errors[0].cod}-{ls_error.errors[0].msg}");
            }


        }
        public RSDocumentos fn_getPreliminares(string perTri)
        {

            try
            {
                RSDocumentos comPreliminares = new RSDocumentos();

                HttpClient cliente = new HttpClient();

                string uri = ruta + $"/rce/preliminar/web/registroslibros/{perTri}/consultapreliminar?codTipoOpe=1&page=1&perPage=20";

                HttpRequestMessage request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(uri)
                };

                request.Headers.Add("Authorization", $"Bearer {token}");

                var response = cliente.SendAsync(request).Result;


                if (response.StatusCode == HttpStatusCode.Unauthorized) // 401 Authorization Required
                {
                    // Aquí puedes agregar lógica adicional si es necesario antes de volver a intentar la conexión
                    ConnectSire(); // Vuelve a intentar la conexión
                    return fn_getPreliminares(perTri); // Llamada recursiva después de intentar la conexión nuevamente
                }

                response.EnsureSuccessStatusCode(); // Verifica si la respuesta del servidor fue exitosa

                if (!response.IsSuccessStatusCode)
                    throw new Exception(response.Content.ReadAsStringAsync().Result);



                comPreliminares = System.Text.Json.JsonSerializer.Deserialize<RSDocumentos>(response.Content.ReadAsStringAsync().Result);


                //comPreliminares.totales.

                return comPreliminares;

            }
            catch (Exception e)
            {
                RSerror ls_error = System.Text.Json.JsonSerializer.Deserialize<RSerror>(e.Message.ToString());

                throw new Exception($"Error al obtener comprobantes: {ls_error.errors[0].cod}-{ls_error.errors[0].msg}");
            }
        }

        public HttpResponseMessage eliminarPreliminar(string perTri)
        {
            try
            {
                string uri = ruta + $"/rce/preliminar/web/registroslibros/{perTri}/1/eliminapreliminar";

                HttpClient cliente = new HttpClient();

                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Put
                };

                request.Headers.Add("Authorization", $"Bearer {token}");

                HttpResponseMessage response = cliente.SendAsync(request).Result;

                if (response.StatusCode == HttpStatusCode.Unauthorized) // 401 Authorization Required
                {
                    // Aquí puedes agregar lógica adicional si es necesario antes de volver a intentar la conexión
                    ConnectSire(); // Vuelve a intentar la conexión
                    return eliminarPreliminar(perTri); // Llamada recursiva después de intentar la conexión nuevamente
                }

                response.EnsureSuccessStatusCode(); // Verifica si la respuesta del servidor fue exitosa



                return response;
            }
            catch (Exception e)
            {
                RSerror ls_error = System.Text.Json.JsonSerializer.Deserialize<RSerror>(e.Message.ToString());

                throw new Exception($"Error al obtener comprobantes: {ls_error.errors[0].cod}-{ls_error.errors[0].msg}");
            }

        }

        public Registros fn_getPreliminar(string ps_perTri)
        {


            try
            {
                string ls_uri = ruta + $"/rvierce/gestionlibro/web/registroslibros/consultapreliminaresregistro?" +
                $"page=1&perPage=100&perIni=202001&perFin={ps_perTri}";

                HttpClient lo_cliente = new HttpClient();

                HttpRequestMessage lo_request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(ls_uri),
                    Method = HttpMethod.Get,
                };

                lo_request.Headers.Add("Authorization", $"Bearer {token}");

                var lo_response = lo_cliente.SendAsync(lo_request).Result;


                if (lo_response.StatusCode == HttpStatusCode.Unauthorized) // 401 Authorization Required
                {
                    // Aquí puedes agregar lógica adicional si es necesario antes de volver a intentar la conexión
                    ConnectSire(); // Vuelve a intentar la conexión
                    return fn_getPreliminar(ps_perTri); // Llamada recursiva después de intentar la conexión nuevamente
                }

                if (lo_response.IsSuccessStatusCode)
                {

                    RSDocumentos lo_rSDocumentos = System.Text.Json.JsonSerializer.Deserialize<RSDocumentos>(lo_response.Content.ReadAsStringAsync().Result);

                    List<Registros> lot_registros = lo_rSDocumentos.registros.Where(re => re.perTributario == ps_perTri & re.codLibro == "080000").ToList();

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

        public HttpResponseMessage eliminarPreFinal(string perTri)
        {
            try
            {
                string uri = ruta + $"/rvierce/gestionlibro/web/registroslibros/{perTri}/eliminapreliminar?codLibro=080000";

                Registros lo_preliminar = fn_getPreliminar(perTri);

                HttpClient lo_cliente = new HttpClient();

                var lo_body = new
                {
                    lo_preliminar.id,
                    codTipoRegistro = "7"
                };

                string ls_jsonBody = JsonConvert.SerializeObject(lo_body);
                HttpContent lo_content = new StringContent(ls_jsonBody, Encoding.UTF8, "application/json");

                HttpClient cliente = new HttpClient();

                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Put,
                    Content = lo_content
                };

                request.Headers.Add("Authorization", $"Bearer {token}");

                HttpResponseMessage response = cliente.SendAsync(request).Result;

                if (response.StatusCode == HttpStatusCode.Unauthorized) // 401 Authorization Required
                {
                    // Aquí puedes agregar lógica adicional si es necesario antes de volver a intentar la conexión
                    ConnectSire(); // Vuelve a intentar la conexión
                    return eliminarPreFinal(perTri); // Llamada recursiva después de intentar la conexión nuevamente
                }

                return response;
            }
            catch (Exception e)
            {
                RSerror ls_error = System.Text.Json.JsonSerializer.Deserialize<RSerror>(e.Message.ToString());

                throw new Exception($"Error al obtener comprobantes: {ls_error.errors[0].cod}-{ls_error.errors[0].msg}");
            }
        }
        /*
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
         */

        public RSDocumentos fn_getPreFinales(string perTri)
        {

            try
            {
                // Consulta cantidad

                string uri = ruta + $"/rvierce/gestionlibro/web/registroslibros/{perTri}/consultadetallepreliminarregistro?codTipoRegistro=7&codLibro=080000&codOrigenEnvio=1&page=1&perPage=20";

                var client = new HttpClient();

                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Get
                };

                request.Headers.Add("Authorization", $"Bearer {token}");

                RSDocumentos rSResumComp = System.Text.Json.JsonSerializer.Deserialize<RSDocumentos>
                    (client.SendAsync(request).Result.Content.ReadAsStringAsync().Result);

                if (rSResumComp.comprobantes == null)
                {

                    string url = ruta + $"/rvierce/gestionlibro/web/registroslibros/{perTri}/consultadetallepreliminarregistro?codTipoRegistro=2&codLibro=080000&codOrigenEnvio=1&page=1&perPage=20";

                    var clientN = new HttpClient();

                    var requestN = new HttpRequestMessage()
                    {
                        RequestUri = new Uri(uri),
                        Method = HttpMethod.Get
                    };

                    requestN.Headers.Add("Authorization", $"Bearer {token}");

                    RSDocumentos rSResumComp2 = System.Text.Json.JsonSerializer.Deserialize<RSDocumentos>
                        (client.SendAsync(request).Result.Content.ReadAsStringAsync().Result);

                    int cantidadTotal = rSResumComp2.paginacion.totalRegistros;

                    Paginacion paginacion = new Paginacion();
                    List<Registros> registrosPrev = new List<Registros>();
                    Totales totales = rSResumComp.totales;

                    var elementosPorGrupo = 20;

                    int cantidadDeGrupos = (int)Math.Ceiling((double)rSResumComp.paginacion.totalRegistros / elementosPorGrupo);

                    for (int i = 1; i < cantidadDeGrupos + 1; i++)
                    {
                        url = ruta + $"/rvierce/gestionlibro/web/registroslibros/{perTri}/consultadetallepreliminarregistro?codTipoRegistro=2&codLibro=080000&codOrigenEnvio=1&page={i}&perPage=20";

                        var requestF = new HttpRequestMessage()
                        {
                            RequestUri = new Uri(uri),
                            Method = HttpMethod.Get
                        };

                        requestF.Headers.Add("Authorization", $"Bearer {token}");

                        RSDocumentos rSComprobantes = System.Text.Json.JsonSerializer.Deserialize<RSDocumentos>
                            (client.SendAsync(requestF).Result.Content.ReadAsStringAsync().Result);

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

                    return rSResumComp2;
                }
                // Retorna comprobantes

                return rSResumComp;
            }
            catch (Exception e)
            {
                RSerror ls_error = System.Text.Json.JsonSerializer.Deserialize<RSerror>(e.Message.ToString());

                throw new Exception($"Error al obtener comprobantes: {ls_error.errors[0].cod}-{ls_error.errors[0].msg}");
            }
        }
        public string fn_getProcesoFase(string perTri)
        {
            try
            {
                string uri = ruta + $"/rvierce/generales/web/control/{perTri}/controlproceso?codLibro=080000";

                HttpClient client = new HttpClient();

                HttpRequestMessage request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(uri),
                };

                request.Headers.Add("Authorization", $"Bearer {token}");

                var response = client.SendAsync(request).Result;

                if (response.StatusCode == HttpStatusCode.Unauthorized) // 401 Authorization Required
                {
                    // Aquí puedes agregar lógica adicional si es necesario antes de volver a intentar la conexión
                    ConnectSire(); // Vuelve a intentar la conexión
                    return fn_getProcesoFase(perTri); // Llamada recursiva después de intentar la conexión nuevamente
                }

                if (!response.IsSuccessStatusCode)
                    throw new Exception(response.Content.ReadAsStringAsync().Result);

                string fase = System.Text.Json.JsonSerializer.Deserialize<RSTicket>(response.Content.ReadAsStringAsync().Result).codFase;

                return fase;
            }
            catch (Exception e)
            {

                RSerror ls_error = System.Text.Json.JsonSerializer.Deserialize<RSerror>(e.Message.ToString());

                throw new Exception($"Error al obtener comprobantes: {ls_error.errors[0].cod}-{ls_error.errors[0].msg}");
            }
        }

        public HttpResponseMessage fn_registrarPreliminar(string ps_perTri)
        {

            var ls_uri = ruta + $"/rce/preliminar/web/registroslibros/{ps_perTri}/registrapreliminares";

            HttpClient lo_cliente = new HttpClient();

            HttpRequestMessage lo_request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(ls_uri),
            };

            lo_request.Headers.Add("Authorization", $"Bearer {token}");

            HttpResponseMessage lo_response = lo_cliente.SendAsync(lo_request).Result;

            return lo_response;
        }

        public RSDocumentos fn_comprobantesSire(string perTri)
        {

            try
            {
                // Consulta cantidad

                string uri = ruta + $"/rvierce/resumen/web/resumen/{perTri}/resumencomprobantes/rce/?codTipoResumen=1";

                var client = new HttpClient();

                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Get
                };

                request.Headers.Add("Authorization", $"Bearer {token}");

                var response = client.SendAsync(request).Result;

                if (response.StatusCode == HttpStatusCode.Unauthorized) // 401 Authorization Required
                {
                    // Aquí puedes agregar lógica adicional si es necesario antes de volver a intentar la conexión
                    ConnectSire(); // Vuelve a intentar la conexión
                    return fn_comprobantesSire(perTri); // Llamada recursiva después de intentar la conexión nuevamente
                }
                response.EnsureSuccessStatusCode(); // Verifica si la respuesta del servidor fue exitosa
                // Retorna comprobantes


                RSDocumentos rSResumComp = System.Text.Json.JsonSerializer.Deserialize<RSDocumentos>
                    (response.Content.ReadAsStringAsync().Result);
                //

                Paginacion paginacion = new Paginacion();
                List<Registros> registrosPrev = new List<Registros>();
                Totales totales = rSResumComp.totales;

                var elementosPorGrupo = 20;

                int cantidadDeGrupos = (int)Math.Ceiling((double)rSResumComp.totales.cntDocumentos / elementosPorGrupo);

                for (int i = 1; i < cantidadDeGrupos + 1; i++)
                {
                    uri = ruta + $"/rce/propuesta/web/propuesta/{perTri}/busqueda?codTipoOpe=1&page={i}&perPage={20}";

                    var requestF = new HttpRequestMessage()
                    {
                        RequestUri = new Uri(uri),
                        Method = HttpMethod.Get
                    };

                    requestF.Headers.Add("Authorization", $"Bearer {token}");

                    RSDocumentos rSComprobantes = System.Text.Json.JsonSerializer.Deserialize<RSDocumentos>
                        (client.SendAsync(requestF).Result.Content.ReadAsStringAsync().Result);

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
        public HttpResponseMessage aceptarPropuesta(string perTri)
        {
            try
            {
                string uri = ruta + $"/rce/propuesta/web/registroslibros/{perTri}/aceptarpropuesta";

                var client = new HttpClient();
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Post
                };

                request.Headers.Add("Authorization", $"Bearer {token}");

                var result = client.SendAsync(request).Result;
                return result;
            }
            catch (Exception e)
            {
                RSerror ls_error = System.Text.Json.JsonSerializer.Deserialize<RSerror>(e.Message.ToString());

                throw new Exception($"Error al aceptar propuesta: {ls_error.errors[0].cod}-{ls_error.errors[0].msg}"); ;
            }
        }

        public HttpResponseMessage updateFase(string perTri, string fase)
        {

            try
            {
                string uri = ruta + $"/rce/libronodomiciliado/web/nodomiciliados/{perTri}/agregarfase";

                var body = new
                {
                    indOperacion = fase
                };

                string jsonBody = JsonConvert.SerializeObject(body);
                HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");


                var client = new HttpClient();
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Put,
                    Content = content
                };

                request.Headers.Add("Authorization", $"Bearer {token}");

                var result = client.SendAsync(request).Result;
                return result;
            }
            catch (Exception e)
            {
                RSerror ls_error = System.Text.Json.JsonSerializer.Deserialize<RSerror>(e.Message.ToString());

                throw new Exception($"Error al aceptar propuesta: {ls_error.errors[0].cod}-{ls_error.errors[0].msg}"); ;
            }
        }

        public RSDocumentos fn_getCompSap(string perTrI)
        {
            string anio = perTrI.Remove(4, 2);
            int mes = int.Parse(perTrI.Remove(0, 4));

            DateTime fPrimerDia = new DateTime(int.Parse(anio), mes, 01);
            DateTime fUltimoDia = fPrimerDia.AddMonths(1).AddDays(-1);

            string fprimer = fPrimerDia.ToString("yyyy-MM-dd").Replace("-", "");
            string fultimo = fUltimoDia.ToString("yyyy-MM-dd").Replace("-", "");

            SAPbobsCOM.Recordset go_recordSet = Global.go_sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);


            if (Global.gi_queryPosition == 1)
                go_recordSet.DoQuery($"CALL STR_AV_REGISTROCOMPRAS_SIRE('{fprimer}','{fultimo}')");
            else
                go_recordSet.DoQuery($"EXEC STR_AV_REGISTROCOMPRAS_SIRE '{fprimer}','{fultimo}'");

            RSDocumentos rSComprobantes = new RSDocumentos();

            List<Registros> lo_registros = new List<Registros>();
            Paginacion paginacion = new Paginacion();

            Totales totales = new Totales();


            double ld_total = 0.00;
            while (!go_recordSet.EoF)
            {
                try
                {
                    string ls_dtSap = go_recordSet.Fields.Item(4).Value.ToString();
                    List<string> lst_data = ls_dtSap.Split('|').ToList();



                    Registros lo_registro = new Registros();

                    List<DocumentoMod> lso_documentoMods = new List<DocumentoMod>();

                    lo_registro.ObjectType = go_recordSet.Fields.Item(2).Value;
                    lo_registro.DocumentEntry = go_recordSet.Fields.Item(3).Value;
                    lo_registro.fecEmision = lst_data[4];
                    lo_registro.codTipoCDP = lst_data[12] == "Anulado" ? $"{lst_data[6]} - Anulado" : lst_data[6];
                    lo_registro.numSerieCDP = lst_data[7];
                    lo_registro.numCDP = lst_data[9];
                    lo_registro.nomRazonSocialProveedor = lst_data[13];
                    lo_registro.mtoBIGravada = string.IsNullOrEmpty(lst_data[14]) ? 0.00 : Convert.ToDouble(lst_data[14]);
                    lo_registro.mtoIGV = string.IsNullOrEmpty(lst_data[15]) ? 0.00 : Convert.ToDouble(lst_data[15]);

                    lo_registro.mtoTotalCP = Convert.ToDouble(lst_data[24]);
                    lo_registro.codMoneda = lst_data[25];

                    if (lst_data[28] != string.Empty)
                    {
                        DocumentoMod lo_documentoMod = new DocumentoMod();
                        lo_documentoMod.codTipoCDPMod = lst_data[28];
                        lo_documentoMod.numSerieCDPMod = lst_data[29];
                        lo_documentoMod.numCDPMod = lst_data[30];
                    }

                    lo_registro.documentoMod = lso_documentoMods;
                    // public List<DocumentoMod> documentoMod { get; set; }

                    ld_total += lo_registro.mtoTotalCP;

                    lo_registros.Add(lo_registro);
                    go_recordSet.MoveNext();
                }
                catch (Exception)
                {

                    throw new Exception("No se encontró data de SAP");
                }
            }

            totales.mtoSumTotalCP = ld_total;

            rSComprobantes.registros = lo_registros;
            rSComprobantes.totales = totales;
            rSComprobantes.paginacion = paginacion;
            return rSComprobantes;
        }

        public RSDocumentos fn_getCompsND(string perTrI)
        {
            try
            {
                string anio = perTrI.Remove(4, 2);
                int mes = int.Parse(perTrI.Remove(0, 4));

                DateTime fPrimerDia = new DateTime(int.Parse(anio), mes, 01);
                DateTime fUltimoDia = fPrimerDia.AddMonths(1).AddDays(-1);

                string fprimer = fPrimerDia.ToString("yyyy-MM-dd").Replace("-", "");
                string fultimo = fUltimoDia.ToString("yyyy-MM-dd").Replace("-", "");

                SAPbobsCOM.Recordset go_recordSet = Global.go_sboCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);


                if (Global.gi_queryPosition == 1)
                    go_recordSet.DoQuery($"CALL STR_AV_RegistroComprasND_Sire('{fprimer}','{fultimo}')");
                else
                    go_recordSet.DoQuery($"EXEC STR_AV_RegistroComprasND_Sire '{fprimer}','{fultimo}'");

                RSDocumentos rSComprobantes = new RSDocumentos();

                List<Registros> lo_registros = new List<Registros>();
                Paginacion paginacion = new Paginacion();

                Totales totales = new Totales();

                double ld_total = 0.00;
                while (!go_recordSet.EoF)
                {
                    string ls_dtSap = go_recordSet.Fields.Item(4).Value.ToString();
                    List<string> lst_data = ls_dtSap.Split('|').ToList();


                    Registros lo_registro = new Registros();

                    List<DocumentoMod> lso_documentoMods = new List<DocumentoMod>();

                    lo_registro.ObjectType = go_recordSet.Fields.Item(2).Value;
                    lo_registro.DocumentEntry = go_recordSet.Fields.Item(3).Value;
                    lo_registro.fecEmision = lst_data[2];
                    lo_registro.codTipoCDP = lst_data[12] == "Anulado" ? $"{lst_data[3]} - Anulado" : lst_data[3];
                    lo_registro.numSerieCDP = lst_data[4];
                    lo_registro.numCDP = lst_data[5];
                    lo_registro.nomRazonSocialProveedor = lst_data[17];
                    lo_registro.mtoBIGravada = string.IsNullOrEmpty(lst_data[6]) ? 0.00 : Convert.ToDouble(lst_data[6]);
                    lo_registro.mtoIGV = string.IsNullOrEmpty(lst_data[7]) ? 0.00 : Convert.ToDouble(lst_data[7]);

                    lo_registro.mtoTotalCP = Convert.ToDouble(lst_data[8]);
                    lo_registro.codMoneda = lst_data[14];

                    if (lst_data[9] != string.Empty)
                    {
                        DocumentoMod lo_documentoMod = new DocumentoMod();
                        lo_documentoMod.codTipoCDPMod = lst_data[9];
                        lo_documentoMod.numSerieCDPMod = lst_data[10];
                        lo_documentoMod.numCDPMod = lst_data[12];
                    }

                    lo_registro.documentoMod = lso_documentoMods;
                    // public List<DocumentoMod> documentoMod { get; set; }

                    ld_total += lo_registro.mtoTotalCP;

                    lo_registros.Add(lo_registro);
                    go_recordSet.MoveNext();
                }

                totales.mtoSumTotalCP = ld_total;

                rSComprobantes.registros = lo_registros;
                rSComprobantes.totales = totales;
                rSComprobantes.paginacion = paginacion;
                return rSComprobantes;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void descargaArchivo(string nomArchivoReporte, string ps_carpeta, bool descomprime, string perTri = null)
        {
            string uri = ruta + "/rvierce/gestionprocesosmasivos/web/masivo/archivoreporte?nomArchivoReporte" +
                $"={nomArchivoReporte}&codTipoArchivoReporte=null&codLibro=080000";
            var client = new HttpClient();

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(uri),
                Method = HttpMethod.Get
            };

            request.Headers.Add("Authorization", $"Bearer {token}");
            var result = client.SendAsync(request).Result.Content.ReadAsByteArrayAsync().Result;

            string rutaArchivo = GblSire.fn_createDrctry(this.rutaArchivoTxt + "\\" + ps_carpeta); // ruta de descarga (dentro de la carpeta Exportaciones\Sire )
            string archivoTemporal = rutaArchivo + $"\\{nomArchivoReporte}";        //  ruta completa del archivo generado Exportaciones\Sire\.zip
            File.WriteAllBytes(archivoTemporal, result);

            if (descomprime)
            {
                using (ZipArchive archivoZip = ZipFile.OpenRead(archivoTemporal))    //  Lee el archivo dentro de zip
                {
                    if (archivoZip.Entries.Count > 0)
                    {
                        string nombreArchivoDentroZip = archivoZip.Entries[0].FullName;     // Nombre del archivo  dentro de zip
                        string rutaArchivoCopia = rutaArchivo + $"\\{nombreArchivoDentroZip}"; // Nombre del archivo que se va a copiar
                        string rutaArchivoCopiado = rutaArchivo + $"\\{perTri + nombreArchivoDentroZip}"; // Nombre del archivo donde se copia

                        if (File.Exists(rutaArchivoCopiado))
                            File.Delete(rutaArchivoCopiado);

                        if (File.Exists(rutaArchivoCopia))
                            File.Delete(rutaArchivoCopia);

                        // Extraer el archivo ZIP con el nuevo nombre
                        archivoZip.ExtractToDirectory(rutaArchivo);
                        File.Copy(rutaArchivoCopia, rutaArchivoCopiado);

                        Global.go_sboApplictn.statusBarSuccessMsg("Exportado exitosamente en la carpeta: " + rutaArchivoCopiado);

                        File.Delete(rutaArchivoCopia);

                    }
                }

                File.Delete(archivoTemporal);

            }
        }

        public void sb_descargaPropuesta(string perTri, string ps_carpeta)
        {

            try
            {
                string uriVali = ruta + $"/rvierce/gestionprocesosmasivos/web/masivo/consultaestadotickets?perIni=202201&perFin={perTri}&page=1&perPage=2000&numTicket=&codOrigenEnvio=1&codLibro=080000";

                var clientVali = new HttpClient();

                var requestVali = new HttpRequestMessage()
                {
                    RequestUri = new Uri(uriVali),
                    Method = HttpMethod.Get
                };

                requestVali.Headers.Add("Authorization", $"Bearer {token}");

                RSConsultaTicket rsConsultaTicket = System.Text.Json.JsonSerializer.Deserialize<RSConsultaTicket>(clientVali.SendAsync(requestVali).Result.Content.ReadAsStringAsync().Result);

                Registro registro = rsConsultaTicket.registros.FirstOrDefault(x => x.perTributario == perTri & x.codProceso == "10" & Convert.ToDateTime(x.fecInicioProceso).Day == DateTime.Now.Day);

                if (registro != null)
                    consultaEstado(perTri, registro.numTicket, ps_carpeta);
                else
                {
                    string uriDescarga = ruta + $"/rce/propuesta/web/propuesta/{perTri}/exportacioncomprobantepropuesta?codTipoArchivo=0&codOrigenEnvio=1&";

                    var client = new HttpClient();

                    var request = new HttpRequestMessage()
                    {
                        RequestUri = new Uri(uriDescarga),
                        Method = HttpMethod.Get
                    };

                    request.Headers.Add("Authorization", $"Bearer {token}");

                    RSTicket rsTicket = System.Text.Json.JsonSerializer.Deserialize<RSTicket>(client.SendAsync(request).Result.Content.ReadAsStringAsync().Result);
                    string ticket = rsTicket.numTicket;

                    if (string.IsNullOrEmpty(ticket))
                        throw new Exception("No se pudo descargar la propuesta");

                    consultaEstado(perTri, ticket, ps_carpeta);
                }
            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
        }

        public void consultaEstado(string perTri, string ticket, string ps_carpeta)
        {
            RSConsultaTicket rsConsultaTicket = new RSConsultaTicket();

            string uri = ruta + $"/rvierce/gestionprocesosmasivos/web/masivo/consultaestadotickets?perIni=202201&perFin={perTri}&page=1&perPage=20&numTicket={ticket}";

            var client = new HttpClient();

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(uri),
                Method = HttpMethod.Get
            };

            request.Headers.Add("Authorization", $"Bearer {token}");

            Thread.Sleep(5000);

            rsConsultaTicket = System.Text.Json.JsonSerializer.Deserialize<RSConsultaTicket>(client.SendAsync(request).Result.Content.ReadAsStringAsync().Result);

            if (rsConsultaTicket.registros[0].detalleTicket.codEstadoEnvio != "06")
            {
                throw new Exception("No se pudo descargar la propuesta, intentar mas tardé");
            }
            else
            {
                string nomArchivoReporte = rsConsultaTicket.registros[0].archivoReporte[0].nomArchivoReporte;
                descargaArchivo(nomArchivoReporte, ps_carpeta, true, perTri);
            }
        }

        public void exportDiferencias(Grid grilla, string perTri)
        {

            string text = "";

            if (grilla.Rows.Count > 0)
            {
                for (int i = 0; i < grilla.Rows.Count; i++)
                {

                    text += grilla.DataTable.GetValue("F. Emisión", i) + "|";
                    text += grilla.DataTable.GetValue("Tipo de documento", i) + "|";
                    text += grilla.DataTable.GetValue("Serie", i) + "|";
                    text += grilla.DataTable.GetValue("Correlativo", i) + "|";
                    text += grilla.DataTable.GetValue("Tipo de Moneda", i) + "|";
                    text += grilla.DataTable.GetValue("Total Sap", i) + "|";
                    text += grilla.DataTable.GetValue("Total Sire", i) + "|";
                    text += grilla.DataTable.GetValue("Observacion", i) + "|";
                    text += "\n";
                }


                string nombreArchivo = "LE" + perTri + "EXP.txt";

                string rutaCompleta = GblSire.fn_createDrctry(rutaArchivoTxt + "\\Exportaciones\\Comparaciones") + "\\" + nombreArchivo;

                File.WriteAllText(rutaCompleta, text);


                Global.go_sboApplictn.statusBarSuccessMsg("Exportado exitosamente en la carpeta: " + rutaCompleta);

            }
        }
        public string fn_descargaND(string perTri, string carpeta)
        {

            string anio = perTri.Remove(4, 2);
            string mes = perTri.Remove
                (0, 4);

            DateTime fPrimerDia = new DateTime(int.Parse(anio), int.Parse(mes), 01);
            DateTime fUltimoDia = fPrimerDia.AddMonths(1).AddDays(-1);

            string fprimer = fPrimerDia.ToString("yyyy-MM-dd").Replace("-", "");
            string fultimo = fUltimoDia.ToString("yyyy-MM-dd").Replace("-", "");

            Recordset rec = Global.go_sboCompany.GetBusinessObject(BoObjectTypes.BoRecordset);

            string texto = "";
            string ruc = "";

            if (Global.gi_queryPosition == 1)
                rec.DoQuery($"CALL STR_AV_REGISTROCOMPRASND_SIRE('{fprimer}','{fultimo}')");
            else
                rec.DoQuery($"EXEC STR_AV_REGISTROCOMPRASND_SIRE '{fprimer}','{fultimo}'");

            while (!rec.EoF)
            {
                ruc = rec.Fields.Item(0).Value;

                texto += rec.Fields.Item(4).Value;

                rec.MoveNext();
                if (!rec.EoF)
                    texto += "\n";
            }

            string nombreArchivo = "LE" + ruc + anio + mes + "00" + "080500001112";
            string rutaCompleta = GblSire.fn_createDrctry($"{rutaArchivoTxt}\\{carpeta}") + "\\" + nombreArchivo;

            File.WriteAllText(rutaCompleta + ".txt", texto);

            if (File.Exists(rutaCompleta + ".zip"))
            {
                File.Delete(rutaCompleta + ".zip");
            }

            using (ZipArchive archivoZip = ZipFile.Open(rutaCompleta + ".zip", ZipArchiveMode.Create))
            {
                // Agregar un archivo al archivo ZIP
                archivoZip.CreateEntryFromFile(rutaCompleta + ".txt", Path.GetFileName(rutaCompleta + ".txt"));
            }

            return rutaCompleta;

        }
        public Registro fn_descargaPSap(string perTri, string carpeta, bool reemplaza)
        {

            string anio = perTri.Remove(4, 2);
            string mes = perTri.Remove(0, 4);

            DateTime fPrimerDia = new DateTime(int.Parse(anio), int.Parse(mes), 01);
            DateTime fUltimoDia = fPrimerDia.AddMonths(1).AddDays(-1);

            string fprimer = fPrimerDia.ToString("yyyy-MM-dd").Replace("-", "");
            string fultimo = fUltimoDia.ToString("yyyy-MM-dd").Replace("-", "");

            Recordset rec = Global.go_sboCompany.GetBusinessObject(BoObjectTypes.BoRecordset);

            if (Global.gi_queryPosition == 1)
                rec.DoQuery($"CALL STR_AV_RegistroCompras_Sire('{fprimer}','{fultimo}')");
            else
                rec.DoQuery($"EXEC STR_AV_RegistroCompras_Sire '{fprimer}','{fultimo}'");

            string texto = "";
            string ruc = "";

            while (!rec.EoF)
            {
                ruc = rec.Fields.Item(0).Value;

                texto += rec.Fields.Item(4).Value;

                rec.MoveNext();
                if (!rec.EoF)
                    texto += "\n";
            }
            string nombreArchivo = "LE" + ruc + anio + mes + "00" + "080400021112";
            string rutaCompleta = fn_createDrctry($"{rutaArchivoTxt}\\{carpeta}") + "\\" + nombreArchivo;

            File.WriteAllText(rutaCompleta + ".txt", texto);

            if (File.Exists(rutaCompleta + ".zip"))
            {
                File.Delete(rutaCompleta + ".zip");
            }

            using (ZipArchive archivoZip = ZipFile.Open(rutaCompleta + ".zip", ZipArchiveMode.Create))
            {
                // Agregar un archivo al archivo ZIP
                archivoZip.CreateEntryFromFile(rutaCompleta + ".txt", Path.GetFileName(rutaCompleta + ".txt"));
            }
            if (reemplaza)
                return reemplazaPropuesta(rutaCompleta + ".zip", obtieneIdTus(rutaCompleta + ".zip", nombreArchivo + ".zip", ruc, perTri));
            else
                return new Registro()
                {
                    nomArchivoImportacion = rutaCompleta + ".zip"
                };
        }

        public string obtenerIdTusND(string rutaArchivo, string nomArchivo, string ruc, string perTri)
        {

            string uri = ruta + "/rvierce/receptorpreliminar/web/preliminar/upload";
            string idTus = "";
            HttpClient client = new HttpClient();



            // Convierte a metadata los datos --------------------------------------------------------------

            byte[] nameArchivo = Encoding.UTF8.GetBytes(nomArchivo);
            var filename = Convert.ToBase64String(nameArchivo);

            byte[] archivoTipo = Encoding.UTF8.GetBytes(".txt");
            var filetype = Convert.ToBase64String(archivoTipo);

            byte[] numeroRuc = Encoding.UTF8.GetBytes(ruc);
            var numRuc = Convert.ToBase64String(numeroRuc);

            byte[] periodoTri = Encoding.UTF8.GetBytes(perTri);
            var perTributario = Convert.ToBase64String(periodoTri);

            byte[] codigoOrigen = Encoding.UTF8.GetBytes("1");
            var codOrigenEnvio = Convert.ToBase64String(codigoOrigen);

            byte[] codigoProceso = Encoding.UTF8.GetBytes("56");
            var codProceso = Convert.ToBase64String(codigoProceso);

            byte[] codigoLibro = Encoding.UTF8.GetBytes("080000");
            var codLibro = Convert.ToBase64String(codigoLibro);

            byte[] codigoCorrelativo = Encoding.UTF8.GetBytes("1");
            var codTipoCorrelativo = Convert.ToBase64String(codigoCorrelativo);

            string uploadMetadata = $"filename {filename}," +
               $"filetype {filetype}," +
               $"numRuc {numRuc},perTributario {perTributario}," +
               $"codOrigenEnvio {codOrigenEnvio}," +
               $"codProceso {codProceso}=,codLibro {codLibro},codTipoCorrelativo {codTipoCorrelativo}," + // 01 - 
               $"nomArchivoImportacion {filename}";

            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(uri),
                Method = HttpMethod.Post
            };

            FileInfo fileInfo = new FileInfo(rutaArchivo);
            string lengthFile = fileInfo.Length.ToString();

            request.Headers.Add("Authorization", $"Bearer {token}");
            request.Headers.Add("Tus-Resumable", "1.0.0");
            request.Headers.Add("Upload-Length", lengthFile);
            request.Headers.Add("Upload-Metadata", uploadMetadata);

            var response = client.SendAsync(request).Result;

            if (response.IsSuccessStatusCode)
            {
                var location = response.Headers.Location;
                string locatio = Convert.ToString(location);
                idTus = locatio.Split('/')[locatio.Split('/').Length - 1];
            }
            else
            {
                throw new Exception(response.Content.ReadAsStringAsync().Result);
            }


            // Sube en Bites los datos con el IdTus

            return idTus;

        }
        public Registro fn_validaPreliminardeAceptar(string perTributario)
        {
            try
            {
                string uri = ruta + $"/rvierce/gestionprocesosmasivos/web/masivo/consultaestadotickets?perIni=202201&perFin={perTributario}&page=1&perPage=200&numTicket=&codProceso=56&codLibro=080000";


                HttpClient client = new HttpClient();

                HttpRequestMessage request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Get
                };
                request.Headers.Add("Authorization", $"Bearer {token}");

                RSConsultaTicket rsConsultaTicket = System.Text.Json.JsonSerializer.Deserialize<RSConsultaTicket>(client.SendAsync(request).Result.Content.ReadAsStringAsync().Result);

                Registro registro = rsConsultaTicket.registros.FirstOrDefault(x => x.perTributario == perTributario & x.codProceso == "2" & x.codEstadoProceso == "06" & Convert.ToDateTime(x.fecInicioProceso).Day == DateTime.Now.Day);

                if (registro != null)
                    return registro;
                return null;
            }
            catch (Exception e)
            {
                RSerror ls_error = System.Text.Json.JsonSerializer.Deserialize<RSerror>(e.Message.ToString());
                throw new Exception($"Error al validar preliminar: {ls_error.errors[0].cod}-{ls_error.errors[0].msg}"); ;
            }
        }
        public Registro fn_validaTicketAnterior(string perTributario, int pi_estadoProceso)
        {
            try
            {

                string uri = ruta + $"/rvierce/gestionprocesosmasivos/web/masivo/consultaestadotickets?perIni=202201&perFin={perTributario}&page=1&perPage=200&numTicket=&codProceso=56&codLibro=080000";


                HttpClient client = new HttpClient();

                HttpRequestMessage request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Get
                };
                request.Headers.Add("Authorization", $"Bearer {token}");



                RSConsultaTicket rsConsultaTicket = System.Text.Json.JsonSerializer.Deserialize<RSConsultaTicket>(client.SendAsync(request).Result.Content.ReadAsStringAsync().Result);

                Registro registro = rsConsultaTicket.registros.FirstOrDefault(x => x.perTributario == perTributario & x.codProceso == "56" & Convert.ToInt32(x.codEstadoProceso) < pi_estadoProceso & Convert.ToDateTime(x.fecInicioProceso).Day == DateTime.Now.Day);

                if (registro != null)
                    return registro;
                return null;
            }
            catch (Exception e)
            {
                RSerror ls_error = System.Text.Json.JsonSerializer.Deserialize<RSerror>(e.Message.ToString());
                throw new Exception($"Error al agregar documentos: {ls_error.errors[0].cod}-{ls_error.errors[0].msg}"); ;
            }
        }
        public Registro fn_setAgregarNodomclds(string rutaArchivo, string id)
        {

            try
            {
                string uri = ruta + $"/rvierce/receptorpreliminar/web/preliminar/upload/{id}";

                HttpClient client = new HttpClient();

                byte[] archivoBin = File.ReadAllBytes(rutaArchivo);

                ByteArrayContent content = new ByteArrayContent(archivoBin);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/offset+octet-stream");

                FileInfo fileInfo = new FileInfo(rutaArchivo);
                string lengthFile = fileInfo.Length.ToString();

                HttpRequestMessage request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(uri),
                    Method = new HttpMethod("PATCH"),
                    Content = content
                };

                request.Headers.Add("Authorization", $"Bearer {token}");
                request.Headers.Add("Tus-Resumable", "1.0.0");
                request.Headers.Add("Upload-Length", lengthFile);
                request.Headers.Add("Upload-Offset", "0");

                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {

                    string ticket = response.Content.ReadAsStringAsync().Result;

                    HttpClient clien = new HttpClient();

                    string periodo = DateTime.Now.ToString("yyyy-MM").Replace("-", "");

                    HttpRequestMessage requestMesage = new HttpRequestMessage()
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri(ruta + $"/rvierce/gestionprocesosmasivos/web/masivo/consultaestadotickets?perIni=202201&perFin={periodo}&page=1&perPage=200&numTicket={ticket}&codProceso="),
                    };

                    requestMesage.Headers.Add("Authorization", $"Bearer {token}");

                    System.Threading.Thread.Sleep(5000); ;

                    response = clien.SendAsync(requestMesage).Result;

                    if (!response.IsSuccessStatusCode)
                        throw new Exception(response.Content.ReadAsStringAsync().Result);


                    var respusta = response.Content.ReadAsStringAsync().Result;
                    List<Registro> responseTicket = System.Text.Json.JsonSerializer.Deserialize<RSConsultaTicket>(response.Content.ReadAsStringAsync().Result).registros;

                    return responseTicket[0];
                }
                else
                    throw new Exception(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception e)
            {
                RSerror ls_error = System.Text.Json.JsonSerializer.Deserialize<RSerror>(e.Message.ToString());
                throw new Exception($"Error al agregar documentos: {ls_error.errors[0].cod}-{ls_error.errors[0].msg}"); ;
            }
        }

        public string obtieneIdTus(string rutaArchivo, string nomArchivo, string ruc, string perTri)
        {

            // Genera el upload Metada

            string uri = ruta + "/rvierce/receptorpropuesta/web/propuesta/upload";
            string idTus = "";
            HttpClient client = new HttpClient();



            // Convierte a metadata los datos --------------------------------------------------------------

            byte[] nameArchivo = Encoding.UTF8.GetBytes(nomArchivo);
            var filename = Convert.ToBase64String(nameArchivo);

            byte[] archivoTipo = Encoding.UTF8.GetBytes("application/x-zip-compressed");
            var filetype = Convert.ToBase64String(archivoTipo);

            byte[] numeroRuc = Encoding.UTF8.GetBytes(ruc);
            var numRuc = Convert.ToBase64String(numeroRuc);

            byte[] periodoTri = Encoding.UTF8.GetBytes(perTri);
            var perTributario = Convert.ToBase64String(periodoTri);

            byte[] codigoOrigen = Encoding.UTF8.GetBytes("1");
            var codOrigenEnvio = Convert.ToBase64String(codigoOrigen);

            byte[] codigoProceso = Encoding.UTF8.GetBytes("3");
            var codProceso = Convert.ToBase64String(codigoProceso);

            //----------------------------------------------------------------------------------

            string uploadMetadata = $"filename {filename}," +
                $"filetype {filetype}," +
                $"numRuc {numRuc},perTributario {perTributario}," +
                $"codOrigenEnvio {codOrigenEnvio}," +
                $"codProceso NjE=,codTipoCorrelativo MQ==," + // 01 - 
                $"nomArchivoImportacion {filename}," +
                $"codLibro MDgwMDAw";

            string metadata = "filename TEUyMDUxMTY0OTYyNjIwMjMwNjAwMDgwNDAwMDIxMTEyLnppcA==,filetype YXBwbGljYXRpb24veC16aXAtY29tcHJlc3NlZA==,numRuc MjA1MTE2NDk2MjY=,perTributario MjAyMzA2,codOrigenEnvio Mg==,codProceso Mw==,codTipoCorrelativo MQ==,nomArchivoImportacion TEUyMDUxMTY0OTYyNjIwMjMwNjAwMDgwNDAwMDIxMTEyLnppcA==,codLibro MDgwMDAw";

            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(uri),
                Method = HttpMethod.Post
            };

            FileInfo fileInfo = new FileInfo(rutaArchivo);
            string lengthFile = fileInfo.Length.ToString();

            request.Headers.Add("Authorization", $"Bearer {token}");
            request.Headers.Add("Tus-Resumable", "1.0.0");
            request.Headers.Add("Upload-Length", lengthFile);
            request.Headers.Add("Upload-Metadata", uploadMetadata);

            var response = client.SendAsync(request).Result;

            // Capturas la cabecera el location


            if (response.IsSuccessStatusCode)
            {
                var location = response.Headers.Location;
                string locatio = Convert.ToString(location);
                idTus = locatio.Split('/')[locatio.Split('/').Length - 1];
            }
            else
            {
                throw new Exception("Error al conectarse con Sire");
            }


            // Sube en Bites los datos con el IdTus

            return idTus;



        }

        public Registro reemplazaPropuesta(string rutaArchivo, string id)
        {

            try
            {
                string uri = ruta + $"/rvierce/receptorpropuesta/web/propuesta/upload/{id}";

                HttpClient client = new HttpClient();

                byte[] archivoBin = File.ReadAllBytes(rutaArchivo);

                ByteArrayContent content = new ByteArrayContent(archivoBin);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/offset+octet-stream");

                FileInfo fileInfo = new FileInfo(rutaArchivo);
                string lengthFile = fileInfo.Length.ToString();

                HttpRequestMessage request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(uri),
                    Method = new HttpMethod("PATCH"),
                    Content = content
                };

                request.Headers.Add("Authorization", $"Bearer {token}");
                request.Headers.Add("Tus-Resumable", "1.0.0");
                request.Headers.Add("Upload-Length", lengthFile);
                request.Headers.Add("Upload-Offset", "0");


                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {

                    string ticket = response.Content.ReadAsStringAsync().Result;

                    HttpClient clien = new HttpClient();

                    string periodo = DateTime.Now.ToString("yyyy-MM").Replace("-", "");

                    HttpRequestMessage requestMesage = new HttpRequestMessage()
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri(ruta + $"/rvierce/gestionprocesosmasivos/web/masivo/consultaestadotickets?perIni=202201&perFin={periodo}&page=1&perPage=200&numTicket={ticket}&codProceso="),
                    };

                    requestMesage.Headers.Add("Authorization", $"Bearer {token}");

                    System.Threading.Thread.Sleep(5000); ;

                    response = clien.SendAsync(requestMesage).Result;

                    if (!response.IsSuccessStatusCode)
                        throw new Exception(response.Content.ReadAsStringAsync().Result);


                    var respusta = response.Content.ReadAsStringAsync().Result;
                    List<Registro> responseTicket = System.Text.Json.JsonSerializer.Deserialize<RSConsultaTicket>(response.Content.ReadAsStringAsync().Result).registros;

                    return responseTicket[0];
                }

                if (response.StatusCode == HttpStatusCode.InternalServerError)
                    throw new Exception("Error interno con el servidor de SUNAT. Por favor, inténtalo de nuevo más tarde.");
                throw new Exception(response.Content.ReadAsStringAsync().Result);
            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception(httpEx.Message);
            }
            catch (Exception e)
            {
                RSerror ls_error = System.Text.Json.JsonSerializer.Deserialize<RSerror>(e.Message);
                throw new Exception($"Error al reemplazar propuesta: {ls_error.errors[0].cod}-{ls_error.errors[0].msg}");
            }

        }



        public Registros getPreliminar(string perTri)
        {


            string uri = ruta + $"/rvierce/gestionlibro/web/registroslibros/consultapreliminaresregistro?" +
                $"page=1&perPage=100&perIni=202001&perFin={perTri}";

            HttpClient cliente = new HttpClient();

            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(uri),
                Method = HttpMethod.Get,
            };

            request.Headers.Add("Authorization", $"Bearer {token}");

            var response = cliente.SendAsync(request).Result;

            if (response.IsSuccessStatusCode)
            {

                RSDocumentos rSDocumentos = System.Text.Json.JsonSerializer.Deserialize<RSDocumentos>(response.Content.ReadAsStringAsync().Result);

                List<Registros> registros = rSDocumentos.registros.Where(re => re.perTributario == perTri).ToList();

                return registros[0];
            }
            else
                throw new Exception(response.Content.ReadAsStringAsync().Result);
        }

    }
}
