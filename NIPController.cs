using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using System.Xml;
using WebAPIStandard.UslugaBIRzewnPubl;

namespace WebAPIStandard.Controllers
{
    public class NipController : ApiController
    {
        public IEnumerable<string> Get()
        {
            // Create the binding
            var bb = new WSHttpBinding();
            bb.Security.Mode = SecurityMode.Transport;
            bb.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            bb.MessageEncoding = WSMessageEncoding.Mtom;

            EndpointAddress ea = new EndpointAddress("https://wyszukiwarkaregon.stat.gov.pl/wsBIR/UslugaBIRzewnPubl.svc");
            UslugaBIRzewnPublClient cc = new UslugaBIRzewnPublClient(bb, ea);
            cc.Open();

            string strSID = cc.Zaloguj("ef2263ea796747f0a5a4");

            string searchResult = "";

            using (new OperationContextScope(cc.InnerChannel))
            {
                var requestMessage = new HttpRequestMessageProperty();
                requestMessage.Headers.Add("sid", strSID);
                OperationContext.Current.OutgoingMessageProperties.Add(HttpRequestMessageProperty.Name, requestMessage);

                ParametryWyszukiwania objParametryGR1 = new ParametryWyszukiwania();
                objParametryGR1.Nip = "7540013376";
                searchResult = cc.DaneSzukaj(objParametryGR1);
            }

            cc.Wyloguj(strSID);
            return new string[] {
                ((searchResult.Length == 0) ? "does not exists" : "exists")
            };
        }

        public IHttpActionResult Get(string id)
        {
            try
            {
                // Create the binding
                var bb = new WSHttpBinding();
                bb.Security.Mode = SecurityMode.Transport;
                bb.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                bb.MessageEncoding = WSMessageEncoding.Mtom;

                EndpointAddress ea = new EndpointAddress("https://wyszukiwarkaregon.stat.gov.pl/wsBIR/UslugaBIRzewnPubl.svc");
                UslugaBIRzewnPublClient cc = new UslugaBIRzewnPublClient(bb, ea);
                cc.Open();

                string strSID = cc.Zaloguj("ef2263ea796747f0a5a4");

                string searchResult = "";

                using (new OperationContextScope(cc.InnerChannel))
                {
                    var requestMessage = new HttpRequestMessageProperty();
                    requestMessage.Headers.Add("sid", strSID);
                    OperationContext.Current.OutgoingMessageProperties.Add(HttpRequestMessageProperty.Name, requestMessage);

                    ParametryWyszukiwania objParametryGR1 = new ParametryWyszukiwania();
                    objParametryGR1.Nip = id;
                    searchResult = cc.DaneSzukaj(objParametryGR1);
                }
                cc.Wyloguj(strSID);

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(searchResult);

                return Ok(new
                {
                    regon = doc.GetElementsByTagName("Regon")[0].InnerXml,
                    nazwa = doc.GetElementsByTagName("Nazwa")[0].InnerXml,
                    wojewodztwo = doc.GetElementsByTagName("Wojewodztwo")[0].InnerXml,
                    powiat = doc.GetElementsByTagName("Powiat")[0].InnerXml,
                    gmina = doc.GetElementsByTagName("Gmina")[0].InnerXml,
                    miejscowosc = doc.GetElementsByTagName("Miejscowosc")[0].InnerXml,
                    kod_pocztowy = doc.GetElementsByTagName("KodPocztowy")[0].InnerXml,
                    ulica = doc.GetElementsByTagName("Ulica")[0].InnerXml,
                    numer_budynku = string.Empty
                });
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.ToString());
                return BadRequest(ex.Message);
            }
            //return new string[] {
            //    ((searchResult.Length == 0) ? "does not exists" : "exists")
            //};
        }

    }
}
