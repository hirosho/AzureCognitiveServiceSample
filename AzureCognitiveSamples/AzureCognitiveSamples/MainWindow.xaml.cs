using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AzureCognitiveSamples
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private string taAccessKey = "-- access key --";
        private string ttAccessKey = "-- access key --";
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            tbTAAccessKey.Text = taAccessKey;
            tbTTAccessKey.Text = ttAccessKey;
        }

        private bool CheckTAAccessKey()
        {
            if (tbTAAccessKey.Text == "-- access key --")
            {
                MessageBox.Show("Please set valid access key!");
                return false;
            }
            return true;
        }

        private bool CheckTTAccessKey()
        {
            if (tbTTAccessKey.Text == "-- access key --")
            {
                MessageBox.Show("Please set valid access key!");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Measure sentiment of specified text.
        /// This will be auto translation for Japanese.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void buttonTASentiment_Click(object sender, RoutedEventArgs e)
        {
            if ((!CheckTAAccessKey()) || (!CheckTTAccessKey())) return;
            var detectedLanguage = await DetectLanguage();
            string sourceText = tbSource.Text;
            string iso6391Name = detectedLanguage.iso6391Name;

            if (detectedLanguage.iso6391Name == "ja") {
                sourceText = await TranslateStringJAJP2ENUS(await GetAccessTokenForTranslation(), sourceText);
                iso6391Name = "en";
            }

            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", taAccessKey);
            var uri = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment" + queryString;

            var topicDocument = new Models.CSTAKeyPhraseDocument()
            {
                id = "1",
                text = sourceText,
                language = iso6391Name
            };

            var topicRequest = new Models.CSTAKeyPhraseRequest();
            topicRequest.documents.Add(topicDocument);
            var json = JsonConvert.SerializeObject(topicRequest);
            byte[] byteData = Encoding.UTF8.GetBytes(json);
            HttpResponseMessage response;

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    using (var resStream = await response.Content.ReadAsStreamAsync())
                    {
                        if (response.Content.Headers.ContentLength.Value > 0)
                        {
                            var buf = new byte[response.Content.Headers.ContentLength.Value];
                            int resRead = await resStream.ReadAsync(buf, 0, buf.Length);
                            string result = Encoding.UTF8.GetString(buf);
                            tbResult.Text = result;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Pickup key phrases.
        /// https://westus.dev.cognitive.microsoft.com/docs/services/TextAnalytics.V2.0/operations/56f30ceeeda5650db055a3c6
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void buttonTAKeyPhrase_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckTAAccessKey()) return;
            var detectedLanguage = await DetectLanguage();

            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", taAccessKey);
            var uri = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/keyPhrases" + queryString;

            var topicDocument = new Models.CSTAKeyPhraseDocument()
            {
                id = "1",
                text = tbSource.Text,
                language = detectedLanguage.iso6391Name
            };

            var topicRequest = new Models.CSTAKeyPhraseRequest();
            topicRequest.documents.Add(topicDocument);
            var json = JsonConvert.SerializeObject(topicRequest);
            byte[] byteData = Encoding.UTF8.GetBytes(json);
            HttpResponseMessage response;

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    using (var resStream = await response.Content.ReadAsStreamAsync())
                    {
                        if (response.Content.Headers.ContentLength.Value > 0)
                        {
                            var buf = new byte[response.Content.Headers.ContentLength.Value];
                            int resRead =await resStream.ReadAsync(buf, 0, buf.Length);
                            string result = Encoding.UTF8.GetString(buf);
                            tbResult.Text = result;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Detect Topics.
        /// This method is under construction.
        /// https://westus.dev.cognitive.microsoft.com/docs/services/TextAnalytics.V2.0/operations/56f30ceeeda5650db055a3ca
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void buttonTATopics_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckTAAccessKey()) return;
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", taAccessKey);
            var uri = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/topics" + queryString;
            HttpResponseMessage response;

            var srcDocs = new Models.CSTADocuments();
            GetDocuments(srcDocs, tbSource.Text);
            var topicRequest = new Models.CSTATopicsRequest();
            if (srcDocs.documents.Count < 100)
            {
                for (int i = 0; i < 100; i++) {
                    topicRequest.documents.Add(new Models.CSTADocument()
                    {
                        id = i.ToString(),
                        text = srcDocs.documents[(i % srcDocs.documents.Count)].text
                    });
                }
            }
            // above code is origin of failed in detecting.

            var json = JsonConvert.SerializeObject(topicRequest);
            byte[] byteData = Encoding.UTF8.GetBytes(json);

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                tbStatus.Text = "Positing...";
                response = await client.PostAsync(uri, content);
                if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    Debug.WriteLine("Start to get results...");
                    var operationId = response.Headers.GetValues("Operation-Location").ElementAt(0);
                    bool onGoing = true;
                    string status = "NotStarted";
                    int counter = 0;
                    while (onGoing)
                    {
                        var resClient = new HttpClient();
                        resClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", taAccessKey);
                        Debug.WriteLine("Start to get chunk...");
                        tbStatus.Text = "Getting chunk : "+counter+"...";
                        var resResponse = await resClient.GetAsync(operationId);
                        Debug.WriteLine("End to get chunk");
                        if (resResponse.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            using (var resResStram = await resResponse.Content.ReadAsStreamAsync())
                            {
                                var buf = new byte[resResStram.Length];
                                var readLen = await resResStram.ReadAsync(buf, 0, buf.Length);
                                var resultC = System.Text.UTF8Encoding.UTF8.GetString(buf);
                                var jobject = JObject.Parse(resultC);
                                var currentStatus = jobject.SelectToken("status");
                                Debug.WriteLine(resultC);
                                status = currentStatus.ToString();
                                if(currentStatus.ToString()== "Failed")
                                {
                                    onGoing = false;
                                    tbResult.Text = resultC;
                                }else if (currentStatus.ToString()=="Succeeded")
                                {
                                    // this block has not been reached.
                                    onGoing = false;
                                    tbResult.Text = resultC;
                                }
                            }
                        }
                        Thread.Sleep(1000);
                        counter++;
                        Debug.WriteLine("Status:"+status+"Round:" + counter);
                    }
                    Debug.WriteLine("Done");
                }
            }
        }

        private async void buttonLanguage_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckTAAccessKey()) return;

            var result = await DetectLanguage();
            tbLanguage.Text = result.iso6391Name + ":" + result.score;
        }

        /// <summary>
        /// Detect languege of text in tbSource
        /// This method get language of 1st document only.
        /// https://westus.dev.cognitive.microsoft.com/docs/services/TextAnalytics.V2.0/operations/56f30ceeeda5650db055a3c7
        /// </summary>
        /// <returns></returns>
        private async Task<DetectedLanguage> DetectLanguage()
        {
            var result = new DetectedLanguage();
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", taAccessKey);
            var uri = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/languages" + queryString;
            HttpResponseMessage response;

            var documents = new Models.CSTADocuments();
            GetDocuments(documents, tbSource.Text);

            var json = JsonConvert.SerializeObject(documents);
            byte[] byteData = Encoding.UTF8.GetBytes(json);

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (response.Content.Headers.ContentLength.Value > 0)
                    {
                        var buf = new byte[response.Content.Headers.ContentLength.Value];
                        using (var resStream = await response.Content.ReadAsStreamAsync())
                        {
                            int readLen = await resStream.ReadAsync(buf, 0, buf.Length);
                            var responseContent = Encoding.UTF8.GetString(buf);
                            Debug.WriteLine(responseContent);
                            var jobject = JObject.Parse(responseContent);
                            result.iso6391Name = (string)jobject.SelectToken("documents[0].detectedLanguages[0].iso6391Name");
                            result.score = (decimal)jobject.SelectToken("documents[0].detectedLanguages[0].score");
                            result.name = (string)jobject.SelectToken("documents[0].detectedLanguages[0].name");
                        }
                    }
                }
            }
            return result;
        }
        private void GetDocuments(Models.CSTADocuments doc, string source)
        {
            var reader = new StringReader(source);
            int id = 0;
            string line = "";
            while ((line = reader.ReadLine()) != null)
            {
                doc.documents.Add(new Models.CSTADocument()
                {
                    id = id.ToString(),
                    text = line
                });
                id++;
            }
        }

        private async Task<string> GetAccessTokenForTranslation()
        {
            string token = "";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ttAccessKey);
            string url = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
            string empty = "";
            byte[] byteData = Encoding.UTF8.GetBytes(empty);

            using (var content = new ByteArrayContent(byteData))
            {
                HttpResponseMessage response = await client.PostAsync(url, content);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    using (var resStream = await response.Content.ReadAsStreamAsync())
                    {
                        if (response.Content.Headers.ContentLength.Value > 0)
                        {
                            var buf = new byte[response.Content.Headers.ContentLength.Value];
                            var resLen = await resStream.ReadAsync(buf, 0, buf.Length);
                            token = Encoding.UTF8.GetString(buf);
                        }
                    }
                }
            }
            return token;
        }

        // reference - http://www.atmarkit.co.jp/ait/articles/1703/02/news012_2.html
        private async Task<string> TranslateStringJAJP2ENUS(string accessToken , string text, string fromLang="ja-jp", string toLang="en-us")
        {
            string result = "";
            string encoded = HttpUtility.UrlEncode(text);
            string url = "http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" + encoded
                + "&from=" + fromLang + "&to=" + toLang
                + "&category=generalnn";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            HttpResponseMessage response = await client.GetAsync(url);
            if(response.StatusCode== System.Net.HttpStatusCode.OK)
            {
                using (var resStream = await response.Content.ReadAsStreamAsync())
                {
                    if (response.Content.Headers.ContentLength.Value > 0)
                    {
                        var dcs = new DataContractSerializer(typeof(string));
                        result = (string)dcs.ReadObject(resStream);
                    }
                }
            }
            return result;
        }
        private async void buttonTranslate_Click(object sender, RoutedEventArgs e)
        {
            if ((!CheckTAAccessKey()) || (!CheckTTAccessKey())) return;
            var detectedLanguage = await DetectLanguage();
            var accessToken = await GetAccessTokenForTranslation();
            tbResult.Text = await TranslateStringJAJP2ENUS(accessToken, tbSource.Text);
        }


        private class DetectedLanguage
        {
            public string name { get; set; }
            public string iso6391Name { get; set; }
            public decimal score { get; set; }
        }

    }
}
