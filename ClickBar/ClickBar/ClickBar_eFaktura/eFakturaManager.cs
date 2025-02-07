using ClickBar_eFaktura.Models.Request;
using ClickBar_eFaktura.Models.Response;
using ClickBar_Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ClickBar_eFaktura
{
    public sealed class eFakturaManager
    {
        #region Fields Singleton
        private static readonly object lockObject = new object();
        private static eFakturaManager instance = null;
        #endregion Fields Singleton

        #region Fields
        private static string _urlPravnaLica = @"https://webservices.nbs.rs/CommunicationOfficeService1_0/CoreXmlService.asmx";
        private static string _urlBudzetskiKorisnici = @"https://kjs.trezor.gov.rs/Home/Contact";
        private static string _url;
        private static string _urlLive = "https://efaktura.mfin.gov.rs/";
        private static string _urlDemo = "https://demoefaktura.mfin.gov.rs/";

        private static DateTime _lastApiTime;
        #endregion Fields

        #region Constructors
        private eFakturaManager() { }
        public static eFakturaManager Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new eFakturaManager();
                        _url = _urlDemo;
                    }
                    return instance;
                }
            }
        }
        #endregion Constructors

        #region Public methods
        //public async Task<bool> SendSalesInvoice(string idInvoicem,
        //    string invoiceXmlString,
        //    string apyKey,
        //    string requestId,
        //    bool sendToCirBool)
        //{
        //    try
        //    {
        //        string api = "api/publicApi/sales-invoice/ubl";

        //        string sendToCir = string.Empty;
        //        if (sendToCirBool)
        //        {
        //            sendToCir = "Yes";
        //        }
        //        else
        //        {
        //            sendToCir = "No";
        //        }

        //        using (var handler = new HttpClientHandler())
        //        {
        //            handler.UseDefaultCredentials = true;
        //            //handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
        //            handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 |
        //                System.Security.Authentication.SslProtocols.Tls13 |
        //                System.Security.Authentication.SslProtocols.Tls11 |
        //                System.Security.Authentication.SslProtocols.Tls;
        //            using (var client = new HttpClient(handler))
        //            {
        //                client.DefaultRequestHeaders.Accept.Clear();
        //                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
        //                client.DefaultRequestHeaders.Add("ApiKey", apyKey);
        //                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "multipart/form-data");

        //                //string xml = string.Empty;

        //                //using (var stringwriter = new System.IO.StringWriter())
        //                //{
        //                //    XmlSerializer serializerRequest = new XmlSerializer(typeof(Models.Invoice));

        //                //    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
        //                //    namespaces.Add("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
        //                //    namespaces.Add("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
        //                //    namespaces.Add("cec", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");

        //                //    serializerRequest.Serialize(stringwriter, invoice, namespaces);
        //                //    xml = stringwriter.ToString();
        //                //}

        //                string url = $"{_url}{api}?requestId={requestId}&sendToCir={sendToCir}";
        //                HttpResponseMessage restResponse = client.PostAsync($"{url}",
        //                    new StringContent(invoiceXmlString, Encoding.UTF8, "text/xml")).Result;

        //                if (!restResponse.IsSuccessStatusCode)
        //                {
        //                    Log.Error($"eFakturaManager - SendSalesInvoice -> Greska prilikom slanja xml file eFakture: {idInvoicem}");

        //                    return false;
        //                }
        //                try
        //                {
        //                    ResponseUBL? response = restResponse.Content.ReadFromJsonAsync<ResponseUBL>().Result;

        //                    if (response != null)
        //                    {
        //                        Log.Debug($"eFakturaManager - SendSalesInvoice -> Uspesno je poslat xml file: {idInvoicem}");
        //                    }
        //                    return true;
        //                }
        //                catch (Exception ex)
        //                {
        //                    Log.Error($"eFakturaManager - SendSalesInvoice -> Greska prilikom obrade odgovora za xml file: {idInvoicem}", ex);

        //                    return false;
        //                }
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.Message.Contains("One or more errors occurred. (A connection attempt failed because the connected party did not properly respond after a period of time"))
        //        {
        //            Thread.Sleep(1000);

        //            return SendSalesInvoice(idInvoicem, invoiceXmlString, apyKey, requestId, sendToCirBool).Result;
        //        }
        //        else
        //        {
        //            Log.Error("eFakturaManager - SendSalesInvoice -> Greska prilikom slanja xml file efakture: ", ex);

        //            return false;
        //        }
        //    }
        //    return false;
        //}
        public async Task<bool> ImportSalesInvoiceXmlFile(string pathToFile,
            string apyKey, 
            string requestId, 
            bool sendToCirBool)
        {
            while (!CheckApiTimer()) ;
            try
            {
                string[] split = pathToFile.Split("\\");
                string fileName = split[split.Length - 1];

                string sendToCir = string.Empty;
                if (sendToCirBool)
                {
                    sendToCir = "Yes";
                }
                else
                {
                    sendToCir = "No";
                }

                string api = "api/publicApi/sales-invoice/ubl/upload";

                using (var handler = new HttpClientHandler())
                {
                    handler.UseDefaultCredentials = true;
                    //handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                    handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 |
                        System.Security.Authentication.SslProtocols.Tls13 |
                        System.Security.Authentication.SslProtocols.Tls11 |
                        System.Security.Authentication.SslProtocols.Tls;
                    //handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    using (var client = new HttpClient(handler))
                    {
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
                        client.DefaultRequestHeaders.Add("ApiKey", apyKey);
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "multipart/form-data");

                        using (var multipartFormContent = new MultipartFormDataContent())
                        {
                            using (var stream = File.OpenRead(pathToFile))
                            {
                                byte[] file_bytes = File.ReadAllBytes(pathToFile);

                                //Add the file
                                multipartFormContent.Add(new ByteArrayContent(file_bytes, 0, file_bytes.Length), "ublFile", $"@{fileName}");

                                stream.Close();
                            }

                            string url = $"{_url}{api}?requestId={requestId}&sendToCir={sendToCir}";

                            HttpResponseMessage restResponse = client.PostAsync(url, multipartFormContent).Result;

                            if (!restResponse.IsSuccessStatusCode)
                            {
                                Log.Error($"eFakturaManager - ImportSalesInvoiceXmlFile -> Greska prilikom slanja xml file eFakture: {fileName}");
                                
                                return false;
                            }
                            try
                            {
                                ResponseUBL? response = restResponse.Content.ReadFromJsonAsync<ResponseUBL>().Result;

                                if (response != null)
                                {
                                    Log.Debug($"eFakturaManager - ImportSalesInvoiceXmlFile -> Uspesno je poslat xml file: {fileName}");
                                }
                                return true;
                            }
                            catch (Exception ex)
                            {
                                Log.Error($"eFakturaManager - ImportSalesInvoiceXmlFile -> Greska prilikom obrade odgovora za xml file: {fileName}", ex);

                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("One or more errors occurred. (A connection attempt failed because the connected party did not properly respond after a period of time"))
                {
                    Thread.Sleep(1000);

                    return ImportSalesInvoiceXmlFile(pathToFile, apyKey, requestId, sendToCirBool).Result;
                }
                else
                {
                    Log.Error("eFakturaManager - ImportSalesInvoiceXmlFile -> Greska prilikom slanja xml file efakture: ", ex);

                    return false;
                }
            }
        }
        public async Task<ResponsePravnaLica?> GetPravnaLica(Models.Request.Envelope request)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(_urlPravnaLica);
                    //string api = "api/Paymentpoints";

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));

                    string xmlRequest = string.Empty;
                    using (var stringwriter = new System.IO.StringWriter())
                    {
                        XmlSerializer serializerRequest = new XmlSerializer(typeof(Models.Request.Envelope));

                        XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                        //namespaces.Add("soap", "");

                        serializerRequest.Serialize(stringwriter, request, namespaces);
                        xmlRequest = stringwriter.ToString();
                    }

                    HttpResponseMessage restResponse = client.PostAsync($"{_urlPravnaLica}",
                        new StringContent(xmlRequest, Encoding.UTF8, "text/xml")).Result;

                    if (!restResponse.IsSuccessStatusCode)
                    {
                        return null;
                    }

                    try
                    {
                        var responseString = await restResponse.Content.ReadAsStringAsync();

                        if (responseString != null)
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(Models.Response.Envelope));

                            using (TextReader reader = new StringReader(responseString))
                            {
                                var response = serializer.Deserialize(reader);

                                reader.Close();

                                if (response is Models.Response.Envelope)
                                {
                                    var responsePravnaLica = SetResponsePravnaLica((Models.Response.Envelope)response);

                                    return responsePravnaLica;
                                }
                                else
                                {
                                    return null;
                                }
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                    catch
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return null;
        }

        public async Task<Models.Response.ResponseBudzetskiKorisnik?> GetBudzetskiKorisnik(string jbkjs)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(_urlBudzetskiKorisnici);
                    string api = $"/{jbkjs}";

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage restResponse = client.PostAsync($"{_urlBudzetskiKorisnici}{api}", null).Result;

                    if (!restResponse.IsSuccessStatusCode)
                    {
                        return null;
                    }

                    try
                    {
                        var responseString = await restResponse.Content.ReadFromJsonAsync<Models.Response.ResponseBudzetskiKorisnik>();

                        if (responseString != null)
                        {
                            return responseString;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    catch
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return null;
        }
        public async Task<bool> CheckIfCompanyRegisteredOnEfaktura(string pib, string apiKey, string? jbkjs = null)
        {
            while (!CheckApiTimer()) ;
            try
            {
                string api = "api/publicApi/Company/CheckIfCompanyRegisteredOnEfaktura";

                CompanyAccountIdentificationDto company = new CompanyAccountIdentificationDto()
                {
                    vatNumber = pib,
                    jbkjs = jbkjs
                };

                using (var handler = new HttpClientHandler())
                {
                    handler.UseDefaultCredentials = true;
                    //handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                    handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 |
                        System.Security.Authentication.SslProtocols.Tls13 |
                        System.Security.Authentication.SslProtocols.Tls11 |
                        System.Security.Authentication.SslProtocols.Tls;
                    //handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(30000);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
                        client.DefaultRequestHeaders.Add("ApiKey", apiKey);
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "multipart/form-data");

                        string url = $"{_url}{api}";

                        HttpResponseMessage restResponse = client.PostAsJsonAsync(url, company).Result;

                        if (!restResponse.IsSuccessStatusCode)
                        {
                            Log.Error($"eFakturaManager - CheckIfCompanyRegisteredOnEfaktura -> IsSuccessStatusCode = false!\n\rodgovor je: {restResponse.StatusCode} - " +
                                $"{restResponse.Content}");
                            return false;
                        }
                        try
                        {
                            CompanyAccountOnEfAkturaDto? response = restResponse.Content.ReadFromJsonAsync<CompanyAccountOnEfAkturaDto>().Result;

                            if (response != null)
                            {
                                return response.eFakturaRegisteredCompany;
                            }
                            return false;
                        }
                        catch (Exception ex)
                        {
                            Log.Error("eFakturaManager - CheckIfCompanyRegisteredOnEfaktura -> Greska prilikom obrade odgovora: ", ex);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("One or more errors occurred. (A connection attempt failed because the connected party did not properly respond after a period of time"))
                {
                    Thread.Sleep(1000);

                    return CheckIfCompanyRegisteredOnEfaktura(pib, apiKey, jbkjs).Result;
                }
                else
                {
                    Log.Error("eFakturaManager - CheckIfCompanyRegisteredOnEfaktura -> Greska prilikom provere firme: ", ex);
                    return false;
                }
            }
        }
        #endregion Public methods

        #region Private methods
        private ResponsePravnaLica? SetResponsePravnaLica(Models.Response.Envelope response)
        {
            try
            {
                CompanyDataSet? companyDataSet = null;
                XmlSerializer serializer = new XmlSerializer(typeof(CompanyDataSet));
                using (StringReader reader = new StringReader(response.Body.GetCompanyResponse.GetCompanyResult))
                {
                    companyDataSet = (CompanyDataSet?)serializer.Deserialize(reader);
                }

                if (companyDataSet == null)
                {
                    return null;
                }

                ResponsePravnaLica responsePravnaLica = new ResponsePravnaLica() 
                {
                    adresa = companyDataSet.Company.Address,
                    //email = companyDataSet.Company.,
                    mb = companyDataSet.Company.NationalIdentificationNumber.ToString("00000000"),
                    mesto = companyDataSet.Company.City,
                    naziv = companyDataSet.Company.Name,
                    pib = companyDataSet.Company.TaxIdentificationNumber.ToString("000000000"),
                    postanskiBroj = companyDataSet.Company.PostalCode
                };

                return responsePravnaLica;
            }
            catch(Exception ex)
            {
                return null;
            }
        }
        private bool CheckApiTimer()
        {
            DateTime dateTime = DateTime.Now;
            double timer = dateTime.Subtract(_lastApiTime).TotalMilliseconds;

            if (timer > 350)
            {
                _lastApiTime = dateTime;
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion Private methods
    }
}
