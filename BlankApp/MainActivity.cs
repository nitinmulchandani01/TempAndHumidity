using Android.App;
using Android.Widget;
using Android.OS;
using System;
using System.Security.Cryptography;
using System.Globalization;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;

namespace BlankApp
{
    [Activity(Label = "Temp And Humidity", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private TextView HumidityTextView;
        private TextView TemperatureTextView;
        private System.Threading.Timer timer;
        protected async override  void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
             SetContentView (Resource.Layout.Main);

            await BindData();

            timer = new Timer(async x => await BindData(), null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1));

        }

        private async Task BindData()
        {

            TemperatureTextView = FindViewById<TextView>(Resource.Id.TemperatureTextView);
            HumidityTextView = FindViewById<TextView>(Resource.Id.HumidityTextView);

            HttpClient client = new HttpClient();
            string temp = string.Empty;
            string humid = string.Empty;

            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            string token = GetSASToken("", "RootManageSharedAccessKey", "");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);

            var response = await client.DeleteAsync("SERVICEBUS_URL);

            var content = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(content))
            {
                int index = content.IndexOf('{');
                content = content.Substring(index);
                int lastindex = content.LastIndexOf('}');
                content = content.Substring(0, lastindex + 1);

                var result = (JObject)JsonConvert.DeserializeObject(content);
                temp = result["temp"].Value<string>().Replace("?C", "°C");
                humid = result["humid"].Value<string>();

                RunOnUiThread(() => { TemperatureTextView.Text = temp; });
                RunOnUiThread(() => { HumidityTextView.Text = humid; });
              
            }
      
        }

        private static string GetSASToken(string baseAddress, string SASKeyName, string SASKeyValue)
        {
            TimeSpan fromEpochStart = DateTime.UtcNow - new DateTime(1970, 1, 1);
            string expiry = Convert.ToString((int)fromEpochStart.TotalSeconds + 3600);
            string stringToSign = WebUtility.UrlEncode(baseAddress) + "\n" + expiry;

            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SASKeyValue));
            string signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));

            string sasToken = String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}",
                  WebUtility.UrlEncode(baseAddress), WebUtility.UrlEncode(signature), expiry, SASKeyName);
            return sasToken;
        }
    }
}

