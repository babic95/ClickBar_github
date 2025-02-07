using ClickBar_Common.Models.CCS_Server;
using ClickBar_Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_API
{
    public sealed class CCS_Fiscalization_ApiManager
    {
        #region Fields Singleton
        private static readonly object lockObject = new object();
        private static CCS_Fiscalization_ApiManager instance = null;
        #endregion Fields Singleton

        #region Fields
        private string? _url;
        #endregion Fields

        #region Constructors
        private CCS_Fiscalization_ApiManager() { }
        public static CCS_Fiscalization_ApiManager Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new CCS_Fiscalization_ApiManager();
                    }
                    return instance;
                }
            }
        }
        #endregion Constructors

        #region Public methods
        public async Task<bool> Initialization()
        {
            _url = SettingsManager.Instance.GetUrlToCCS_Server();

            if (!string.IsNullOrEmpty(_url))
            {
                return true;
            }

            return false;
        }

        public async Task<bool> InsertPaymentPoint(PaymentPoint paymentPoint)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(_url);
                    string api = "api/Paymentpoints";

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage restResponse = client.PostAsync($"{_url}{api}",
                        new StringContent(JsonConvert.SerializeObject(paymentPoint), Encoding.UTF8, "application/json")).Result;

                    if (!restResponse.IsSuccessStatusCode)
                    {
                        return false;
                    }

                    try
                    {
                        PaymentPoint? response = await restResponse.Content.ReadFromJsonAsync<PaymentPoint>();

                        if (response != null)
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        return false;
                    }

                    return false;
                }
                catch (Exception ex)
                {

                }
            }
            return false;
        }

        public async Task<DateTime?> GetValidTo(string idPaymentPont)
        {
            string api = $"api/Paymentpoints/{idPaymentPont}";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage restResponse = client.GetAsync($"{_url}{api}").Result;

                if (!restResponse.IsSuccessStatusCode)
                {
                    return null;
                }

                try
                {
                    DateTime? response = restResponse.Content.ReadFromJsonAsync<DateTime>().Result;

                    if (response != null && response.HasValue)
                    {
                        return response.Value;
                    }
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}
