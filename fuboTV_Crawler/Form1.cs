using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PuppeteerSharp;
using HtmlAgilityPack;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace fuboTV_Crawler
{
    public static class publicExtension
    {
        public static int ToInt(this string val)
        {
            return Convert.ToInt32(val);
        }
    }

    public partial class Form1 : Form
    {
        //Browser browser;
        //Frame frame;
        //Page page;
        //bool usingFrame = false;

        class fuboSportModel
        {
            public string programId { get; set; }
            public int sportId { get; set; }
            public string sport { get; set; }
            public int team_h_id { get; set; }
            public string team_h { get; set; }
            public int team_c_id { get; set; }
            public string team_c { get; set; }
            public int lea_id { get; set; }
            public string league { get; set; }
            public DateTime? ga_time { get; set; }
        }

        public Form1()
        {
            InitializeComponent();
            button1.Click += (s, e) =>
            {
                List<fuboSportModel> listFuboData = new List<fuboSportModel>();
                string startTime = DateTime.Now.AddDays(-1).ToString("yyyy-MM-ddT16:00:00.000Z");
                string endTime = DateTime.Now.AddDays(7).ToString("yyyy-MM-ddT15:59:59.999Z");
                var response = RequestAPI($"https://api.fubo.tv/content?programType=match&playing=stream%2Clookback&qualifiers=live&sportId=-1&startTime={ startTime }&endTime={ endTime }&upcoming=true&limit=-1");
                //var response = RequestAPI("https://api.fubo.tv/content?promoted=match&upcoming&types=programWithAssets,link&limit=1000");
                if (string.IsNullOrEmpty(response))
                    return;

                JObject jo = (JObject)JsonConvert.DeserializeObject(response);
                if (!jo.ContainsKey("response")) return;

                JArray ja = (JArray)JsonConvert.DeserializeObject(jo["response"].ToString());
                foreach (JObject game in ja)
                {
                    JObject joData = (JObject)game["data"]["program"];
                    JArray jaAssets = (JArray)game["data"]["assets"];

                    //只抓比賽
                    if (joData["type"].ToString() == "match")
                    {
                        fuboSportModel tempData = new fuboSportModel();
                        JObject data = (JObject)joData["metadata"];
                        if (!data.ContainsKey("teamsMetadata"))
                            continue;

                        tempData.programId = joData["programId"].ToString();
                        tempData.sportId = data["sports"][0]["id"].ToString().ToInt();
                        tempData.sport = data["sports"][0]["name"].ToString();
                        tempData.lea_id = data["leagues"][0]["id"].ToString().ToInt();
                        tempData.league = data["leagues"][0]["leagueName"].ToString();
                        tempData.team_h_id = data["teamsMetadata"]["homeTeam"]["id"].ToString().ToInt();
                        tempData.team_h = data["teamsMetadata"]["homeTeam"]["name"].ToString();
                        tempData.team_c_id = data["teamsMetadata"]["awayTeam"]["id"].ToString().ToInt();
                        tempData.team_c = data["teamsMetadata"]["awayTeam"]["name"].ToString();
                        var accessRights = (JObject)jaAssets[0]["accessRights"];
                        tempData.ga_time = Convert.ToDateTime(accessRights["startTime"].ToString());

                        if (listFuboData.Where(it => it.programId == tempData.programId).Count() == 0)
                            listFuboData.Add(tempData);
                    }
                }
                Console.WriteLine("OK");
            };
        }

        private string RequestAPI(string apiAddress)
        {
            System.Diagnostics.Stopwatch wt = new System.Diagnostics.Stopwatch();
            wt.Restart();

            var client = new RestClient(apiAddress);
            client.Timeout = -1;

            var request = new RestRequest(Method.GET);
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            request.AddHeader("Accept-Encoding", "gzip, deflate");
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.130 Safari/537.36");
            IRestResponse response = client.Execute(request);
            wt.Stop();
            Console.WriteLine($"API花費時間: {wt.ElapsedMilliseconds} ms");

            return response.Content;
        }

        #region puppeteer
        //public async Task browserInit()
        //{
        //    await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
        //    browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = Properties.Settings.Default.headless });
        //    page = await browser.NewPageAsync();
        //    await page.SetViewportAsync(new ViewPortOptions { Width = 1024 });

        //    await page.GoToAsync("https://www.fubo.tv/sports");
        //    await waitElement_ByCssSelector("input[name='email']");
        //    await TypeText_ByCssSelector("input[name='email']","livesportsaddict@gmail.com");

        //    await waitElement_ByCssSelector("input[name='password']");
        //    await TypeText_ByCssSelector("input[name='password']", "Playhard1@");

        //    await waitElement_ByCssSelector("button[type=submit]");
        //    await Click_ByCssSelector("button[type=submit]");
        //    await page.WaitForNavigationAsync();

        //    MessageBox.Show("ok");
        //    page.Dialog += (s, e) =>
        //    {
        //        e.Dialog.Accept();
        //    };
        //}

        //public void SwitchMainFrame()
        //{
        //    frame = page.MainFrame;
        //    usingFrame = false;
        //}

        //public void SwitchFrame(string name, string url = null)
        //{
        //    var allFrames = page.Frames;
        //    if (string.IsNullOrEmpty(url))
        //        frame = allFrames.Where(it => it.Name == name).FirstOrDefault();
        //    else
        //        frame = allFrames.Where(it => it.Url == url).FirstOrDefault();

        //    usingFrame = true;
        //}

        //public async Task<string> GetPageSource()
        //{
        //    return await page.GetContentAsync();
        //}

        //public async Task<string> GetFrameSource()
        //{
        //    return await frame.GetContentAsync();
        //}

        //public void GoToUrl(string url)
        //{
        //    page.GoToAsync(url).Wait();
        //}

        //public async Task WaitForNavigate()
        //{
        //    await page.WaitForNavigationAsync();
        //}

        //public async Task FocusElement_ByCssSelector(string selector)
        //{
        //    if (usingFrame)
        //        await frame.FocusAsync(selector);
        //    else
        //        await page.FocusAsync(selector);
        //}

        //public async Task TypeText_ByCssSelector(string selector, string val)
        //{
        //    if (usingFrame)
        //        await frame.TypeAsync(selector, val);
        //    else
        //        await page.TypeAsync(selector, val);
        //}

        //public async Task Click_ByCssSelector(string selector)
        //{
        //    if (usingFrame)
        //        await frame.ClickAsync(selector);
        //    else
        //        await page.ClickAsync(selector);
        //}

        //public async Task<bool> waitElement_ByCssSelector(string selector)
        //{
        //    ElementHandle ele;
        //    try
        //    {
        //        if (usingFrame)
        //            ele = await frame.WaitForSelectorAsync(selector);
        //        else
        //            ele = await page.WaitForSelectorAsync(selector);

        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        //public async Task<bool> waitElement_ByXPath(string XPath)
        //{
        //    ElementHandle ele;
        //    try
        //    {
        //        if (usingFrame)
        //            ele = await frame.WaitForXPathAsync(XPath);
        //        else
        //            ele = await page.WaitForXPathAsync(XPath);

        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        //public async Task<ElementHandle> GetElement_ByCssSelector(string selector)
        //{
        //    ElementHandle ele;
        //    if (usingFrame)
        //        ele = await frame.QuerySelectorAsync(selector);
        //    else
        //        ele = await page.QuerySelectorAsync(selector);

        //    return ele;
        //}

        //public async Task<ElementHandle[]> GetAllElement_ByCssSelector(string selector)
        //{
        //    ElementHandle[] eles;
        //    if (usingFrame)
        //        eles = await frame.QuerySelectorAllAsync(selector);
        //    else
        //        eles = await page.QuerySelectorAllAsync(selector);

        //    return eles;
        //}

        //public async Task<bool> elementExist_ByCssSelector(string selector)
        //{
        //    ElementHandle ele;
        //    if (usingFrame)
        //        ele = await frame.QuerySelectorAsync(selector);
        //    else
        //        ele = await page.QuerySelectorAsync(selector);

        //    if (ele != null) return true;
        //    else return false;
        //}

        //public async Task ExecuteJS_Script(string jsText)
        //{
        //    if (usingFrame)
        //        await frame.EvaluateExpressionAsync(jsText);
        //    else
        //        await page.EvaluateExpressionAsync(jsText);

        //    //if (usingFrame)
        //    //    await frame.EvaluateFunctionAsync(jsText);
        //    //else
        //    //    await page.EvaluateFunctionAsync(jsText);
        //}
        #endregion

    }
}
