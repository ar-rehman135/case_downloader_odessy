using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;


namespace CaseDownloader
{

    public class Locate : IDisposable
    {

        #region Events
        public delegate void ShowMessagebox(string data);
        public ShowMessagebox showMessage = delegate { };

        #endregion

        #region fields

        private PhantomJSDriver driver;

        private const int internal_thread_count = 1;

		private string DownloadPath = string.Concat(Directory.GetCurrentDirectory(), "\\Attachments\\");

        CrossRefNumber case_ref_num;

        private string SignInUrl;

        private string SignInUrl1;

        private string HomePageUrl;

		private string UserDefinedPath;

		private string DefaultPath;

		private string crossRefNum;

		private string caseURL;

		private string caseFilesURL;

        private int submit_wait = 300;

        private int web_el_wait = 20;

        private string username;

        private string password;

        #endregion

        #region constructors

        public Locate(string user, string pass)
		{
            username = user;
            password = pass;

            RefreshDriver();

            //var chromeOptions = new ChromeOptions(); 
            //chromeOptions.AddArguments("headless"); 
            //var chromeDriverService = ChromeDriverService.CreateDefaultService(); 
            //// chromeDriverService.HideCommandPromptWindow = true; 
            //driver = new ChromeDriver(chromeDriverService,chromeOptions);

            //driver.Navigate().GoToUrl("https://facebook.com");
            //Thread.Sleep(3000);
            //Console.WriteLine(driver.WindowHandles.Count);
            //driver.ExecuteScript("window.open('https://google.com','_blank');");
            //Thread.Sleep(3000);
            //Console.WriteLine(driver.WindowHandles.Count);

            Console.WriteLine(driver.Capabilities.ToString());
            SignInUrl = "https://www.clarkcountycourts.us/Portal/Account/Login";
            SignInUrl1 = "https://odysseyadfs.tylertech.com/IdentityServer/account/signin?ReturnUrl=%2fIdentityServer%2fissue%2fwsfed%3fwa%3dwsignin1.0%26wtrealm%3dhttps%253a%252f%252fOdysseyADFS.tylertech.com%252fadfs%252fservices%252ftrust%26wctx%3d4d2b3478-8513-48ad-8998-2652d72a38e9%26wauth%3durn%253a46%26wct%3d2018-04-29T15%253a42%253a35Z%26whr%3dhttps%253a%252f%252fodysseyadfs.tylertech.com%252fidentityserver&wa=wsignin1.0&wtrealm=https%3a%2f%2fOdysseyADFS.tylertech.com%2fadfs%2fservices%2ftrust&wctx=4d2b3478-8513-48ad-8998-2652d72a38e9&wauth=urn%3a46&wct=2018-04-29T15%3a42%3a35Z&whr=https%3a%2f%2fodysseyadfs.tylertech.com%2fidentityserver";
            HomePageUrl = "https://www.clarkcountycourts.us/Portal/";
            case_ref_num = new CrossRefNumber();
		}

        #endregion

        #region Public members

        [TestMethod]
        public string LocateCase(string refNum, DataGridView grd, string path)
        {
            string stackTrace;
            case_ref_num.refNum = refNum;
            {
                try
                {
                    path += "\\" + case_ref_num.refNum;
                    do
                    {
                        RefreshDriver();
                        bool is_login = false;
                        for (int i=0;i<3;i++)
                        {
                            is_login = Login(username, password);
                            if (is_login)
                                break;

                        }
                        if (!is_login)
                        {
                            stackTrace = "unable to do Login";
                        }
                        if (!this.NavigateToSearchUrl())
                        {
                            return "Naviagtion To search Url Failed";
                        }
                        //                    ShowDriverState();
                        if (!SearchRefNum(case_ref_num.refNum))
                        {
                            return "Unable TO find Ref Num";
                        }
                        ShowDriverState();
                        if (!findCases(path, case_ref_num))
                        {
                            return "Unable To Find Cases";
                        }
                        //          ShowDriverState();
                    } while (case_ref_num.caseProcessed < case_ref_num.caseCount);
                }
                catch (Exception exception5)
                {
                    stackTrace = exception5.StackTrace;
                    return stackTrace;
                }
                stackTrace = "0";
            }
            return stackTrace;
        }

        public bool Login(string username, string password)
        {
            this.username = username;
            this.password = password;
            bool flag = false;
            try
            {
                driver.Navigate().GoToUrl(SignInUrl);
                takescreenshot("login screen");
                //the driver can now provide you with what you need (it will execute the script)
                //get the source of the page
                //fully navigate the dom
                ShowDriverState();
                var pathElement = FindElementIfExists(By.Id("UserName"));
                if (pathElement == null)
                    return false;
                pathElement.SendKeys(username);
                var pass = FindElementIfExists(By.Id("Password"));
                if (pathElement == null)
                    return false;
                pass.SendKeys(password);
                var signin = driver.FindElementByClassName("tyler-btn-primary");
                signin.Click();
                var body = new WebDriverWait(driver, TimeSpan.FromSeconds(submit_wait)).Until(ExpectedConditions.UrlContains("Portal"));
                if (driver.Url != HomePageUrl )
                {
                    Console.WriteLine(driver.Title);
                    Console.WriteLine(driver.Url);
                    flag = false; 
                }
                else
                {
                    Console.WriteLine(driver.Title);
                    Console.WriteLine(driver.Url);
                    flag = true;
                }
            }
            catch(Exception ex)
            {
                takescreenshot("exception login");
                Console.WriteLine(driver.Url);
                Console.WriteLine(ex.Message);
                flag = false;
            }
            takescreenshot("afterLoginScreen");
            return flag;
        }

        public void logout()
        {
            try
            {
                driver.Navigate().GoToUrl("https://www.clarkcountycourts.us/Portal/Account/LogOff");
                takescreenshot("logout");
                Thread.Sleep(500);
                quit();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void quit()
        {
            driver.Quit();
            GC.Collect();
        }

        #endregion

        #region private member functions

        private bool findCases(string path, CrossRefNumber crossRef)
        {
            try
            {
                takescreenshot("before finding cases");
                var resFoundSel = "#SmartSearchResults";
                var resFound = FindElementIfExists(By.CssSelector(resFoundSel));
                if (resFound == null)
                {
                    logCaseNotFound(crossRef);
                    return true;
                }
                string tbodysel = "#CasesGrid > table:nth-child(1) > tbody:nth-child(3)";
                var tbody = driver.FindElementByCssSelector(tbodysel);
                var trs = tbody.FindElements(By.TagName("tr"));
                Console.WriteLine(trs.Count);
                crossRef.caseCount = trs.Count;
                string allcasesUrl = driver.Url;
//                foreach (var tr1 in trs)
                var tr1 = trs[crossRef.caseProcessed];
                {
                    CourtCase case1 = new CourtCase();
                    var caseLink = tr1.FindElement(By.CssSelector(".caseLink"));
                    case1.URL = caseLink.GetAttribute("data-url");
                    case1.caseNum = caseLink.Text;
                    case_ref_num.cases.Add(case1);
                    new Actions(driver).Click(caseLink).Perform();
//                    Thread.Sleep(1000);
                    string case_info_div_sel = "#divCaseInformation_body";
                    try
                    {
                        var body = new WebDriverWait(driver, TimeSpan.FromSeconds(submit_wait)).Until(ExpectedConditions.ElementExists(By.CssSelector(case_info_div_sel)));
                    }
                    catch(Exception ex)
                    {
//                        Thread.Sleep(1000);
                        try
                        {
                            var body = new WebDriverWait(driver, TimeSpan.FromSeconds(submit_wait)).Until(ExpectedConditions.ElementExists(By.CssSelector(case_info_div_sel)));
                        }
                        catch (Exception ex1)
                        {
                            Console.WriteLine(driver.Url);
                            Console.WriteLine(ex1.Message);
                            return false;
                        }
                    }

                    Console.WriteLine("case found" + caseLink.Text);
//                    Thread.Sleep(1000);
                    bool flg = process_case(path,case1);
                    if (!flg)
                    {
                        return flg;
                    }
                }
                crossRef.caseProcessed++;
                return true;
            }
            catch(Exception ex)
            {
                takescreenshot("exception findcases");
                Console.WriteLine(driver.Url);
                return false;
            }
        }

        private bool process_case(string path, CourtCase case1)
        {
            try
            {
//                Thread.Sleep(1000);
                Directory.CreateDirectory(path + "\\" + case1.caseNum);
                savePageInfo(path + "/" + case1.caseNum, case1);
                downloadDocuments(path + "/" + case1.caseNum, case1);
                CheckDataIntegrity(path + "/" + case1.caseNum, case1);
//                Thread.Sleep(500);
                return true;
            }
            catch (Exception es)
            {
                Console.WriteLine(es.Message);
                Console.WriteLine(driver.Url);
                return false;
            }
        }

        private void downloadDocuments(string path, CourtCase case1)
        {
            try
            {
                string documents_tables_sel = "#divDocumentsInformation_body";
                var documents_tables = FindElementIfExists(By.CssSelector(documents_tables_sel));
                if (documents_tables == null)
                {
                    logNoDocumentsFound(case1);
                    return;
                }
                var docs_p = documents_tables.FindElements(By.TagName("p"));
                int k = 0;
                foreach (var doc_p in docs_p)
                {
                    CaseDocument casedoc = new CaseDocument();
                    casedoc.inCase = case1;
                    var doc_a = doc_p.FindElement(By.TagName("a"));
                    casedoc.URL = doc_a.GetAttribute("href");
                    casedoc.description = doc_p.Text;
                    var doc_filename_span = doc_p.FindElement(By.TagName("span"));
                    casedoc.fileNumber = k + 1;
                    var file_num_str = casedoc.fileNumber.ToString().PadLeft(4, '0');
                    casedoc.fileName = RemoveIllegalChars( doc_filename_span.Text);
                    casedoc.fileName = path + "/" + file_num_str + "-" + casedoc.fileName;
                    case1.Documents.Add(casedoc);
                    k++;
                }

                if (case1.Documents.Count > 20)
                {
                    RefreshDriver();
                    bool is_login = false;
                    for (int i = 0; i < 3; i++)
                    {
                        is_login = Login(username, password);
                        if (is_login)
                            break;
                    }
                    if (!is_login)
                        return;
                }
                for (int i = 0; i < case1.Documents.Count ; i += internal_thread_count )
                {
                    int th_count = case1.Documents.Count - i < internal_thread_count ? case1.Documents.Count - i : internal_thread_count;
                    Thread[] ths = new Thread[th_count];
                    for (int j= 0 ;j<th_count;j++)
                    {
                        var docs = case1.Documents[i + j];
                        Console.WriteLine(docs.URL);

                        downloadDocument(docs, ths[j]);
                    }
                    //for (int j =0 ;j<th_count;j++)
                    //{
                    //    if (ths[j] != null)
                    //    {
                    //        try
                    //        {
                    //            ths[j].Join();
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            Console.WriteLine(ex.Message);
                    //        }

                    //    }
                    //}
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(driver.Url);

            }
        }

        private void RefreshDriver()
        {
            try
            {
                if (driver!= null)
                {
                    driver.Quit();
                }
                var service = PhantomJSDriverService.CreateDefaultService(Environment.CurrentDirectory);
                service.WebSecurity = false;
                service.HideCommandPromptWindow = true;
                driver = new PhantomJSDriver(service,new PhantomJSOptions(),TimeSpan.FromSeconds( submit_wait));
                driver.Manage().Window.Size = new System.Drawing.Size(1240, 1240);
            }
            catch
            {
                throw;
            }
        }

        private bool downloadDocument(CaseDocument case_doc, Thread th,bool on_th = true)
        {
            try
            {
                string downloadLinksel = "a.btn:nth-child(1)";
                takescreenshot("download document");
                driver.Navigate().GoToUrl(case_doc.URL);
                try
                {
                    var body = new WebDriverWait(driver, TimeSpan.FromSeconds(submit_wait)).Until(ExpectedConditions.ElementExists(By.CssSelector(downloadLinksel)));
                }
                catch(Exception ex)
                {
                    try
                    {
                        var body = new WebDriverWait(driver, TimeSpan.FromSeconds(submit_wait)).Until(ExpectedConditions.ElementExists(By.CssSelector(downloadLinksel)));
                    }
                    catch (Exception ex1)
                    {
                        Console.WriteLine(ex1.Message);
                        return false;
                    }
                }


                takescreenshot("download document view");

                var downloadLink = driver.FindElementByCssSelector(downloadLinksel);
                case_doc.D_URL = downloadLink.GetAttribute("href");
                Console.WriteLine(case_doc.D_URL);
                if (on_th)
                {
                    th = new Thread(() => { bool is_downloaded = TryDownloadFile(case_doc); });
                    th.Start();
                    return true;
                }
                else
                {
                    bool is_downloaded = TryDownloadFile(case_doc);
                    case_doc.downloaded = is_downloaded;
                }

            }
            catch(WebDriverTimeoutException ex2)
            {
                Console.WriteLine(ex2.Message);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return true;
        }

        private bool NavigateToSearchUrl()
        {
            try
            {
                IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(submit_wait));
//                Thread.Sleep(1000);
                wait.Until(ExpectedConditions.ElementExists(By.Id("portlet-29")));
                var smartSearhDiv = driver.FindElementById("portlet-29");
                var smartSearchA = smartSearhDiv.FindElement(By.CssSelector("a"));
                string smartSearchUrl = smartSearchA.GetAttribute("href");
                driver.Navigate().GoToUrl(smartSearchUrl);
                wait.Until(ExpectedConditions.ElementExists(By.CssSelector("#SSColumn")));
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool SearchRefNum(string refNum)
        {
            try
            {
                string searchCriteriaInput = "caseCriteria_SearchCriteria";
                string advanceOptionButtonId = "AdvOptions";
                string searchBtnId = "btnSSSubmit";

                var advanceOptionButton = FindElementIfExists(By.Id(advanceOptionButtonId));
                advanceOptionButton.SendKeys(OpenQA.Selenium.Keys.Enter);
//                Thread.Sleep(1000);
                takescreenshot("advance options selected");
                
                var maskdiv = FindElementIfExists(By.Id("AdvOptionsMask"));
                Console.WriteLine(maskdiv.Displayed);
                Console.WriteLine(driver.Url);

                string spantoClicksel = "#AdvOptionsMask > div:nth-child(1) > div:nth-child(2) > div:nth-child(1) > fieldset:nth-child(3) > span:nth-child(1) > span:nth-child(1) > span:nth-child(2)";
                var spantoclick = FindElementIfExists(By.CssSelector(spantoClicksel));
                spantoclick.SendKeys(OpenQA.Selenium.Keys.Enter);
                spantoclick.Click();
//                Thread.Sleep(1000);
                takescreenshot("span clicked");
//                Thread.Sleep(1000);

                string litoclicksel = "#caseCriteria_SearchBy_listbox > li:nth-child(5)";
                var litoclick = FindElementIfExists(By.CssSelector(litoclicksel));
                litoclick.SendKeys(OpenQA.Selenium.Keys.Enter);
//                Thread.Sleep(1000);
                takescreenshot("li clicked");
                litoclick.Click();
//                Thread.Sleep(1000);
                takescreenshot("li clicked1");
//                Thread.Sleep(1000);

                //// select Case Criteria Of Case Cross Reference Number
                //var caseCriteria = driver.FindElementByName(caseCriteriaName);
                //var actions = new OpenQA.Selenium.Interactions.Actions(driver).MoveToElement(caseCriteria);
                //actions.Perform();
                //Thread.Sleep(1000);
                //caseCriteria.Clear();
                //caseCriteria.SendKeys("Case Cross-Reference Number");
                //takescreenshot("added case criteria");
                //var caseCriteriahid = driver.FindElementById(caseCriteriaInpId);
                //string js = "arguments[0].style.display='block';";
                //driver.ExecuteScript(js, caseCriteriahid);
                //caseCriteriahid.Clear();
                //caseCriteriahid.SendKeys("CaseCrossReferenceNumber");
                //Thread.Sleep(1000);

                var searhInput = FindElementIfExists(By.Id(searchCriteriaInput));
                searhInput.Clear();
                searhInput.SendKeys(refNum);
                Console.WriteLine(searhInput.Displayed);
                takescreenshot("referrence num added");
                var searchBtn = driver.FindElementById(searchBtnId);
                if (!searchBtn.Displayed)
                {
                    var action = new OpenQA.Selenium.Interactions.Actions(driver).MoveToElement(searchBtn);
                    action.Perform();
                }
                searchBtn.Submit();

                IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(submit_wait));
                wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
                takescreenshot("after search finiched");
                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        private string RemoveIllegalChars(string fileName)
        {
            char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
            for (int i = 0; i < (int)invalidFileNameChars.Length; i++)
            {
                fileName = fileName.Replace(invalidFileNameChars[i], '\u005F');
            }
            return fileName;
        }

        private string cookieString(IWebDriver driver)
        {
            string str = string.Join("; ",
                from c in driver.Manage().Cookies.AllCookies
                select string.Format("{0}={1}", c.Name, c.Value));
            return str;
        }

        private bool TryDownloadFile(CaseDocument case_doc)
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                // create HttpWebRequest
                Uri uri = new Uri(case_doc.D_URL);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.ProtocolVersion = HttpVersion.Version10;

                // insert cookies
                request.CookieContainer = new CookieContainer();
                foreach (OpenQA.Selenium.Cookie c in driver.Manage().Cookies.AllCookies)
                {
                    System.Net.Cookie cookie =
                        new System.Net.Cookie(c.Name, c.Value, c.Path, c.Domain);
                    request.CookieContainer.Add(cookie);
                }

                // download file
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                {
                    string ext = ".";
                    if(response.ContentType.Contains("tiff"))
                    {
                        ext += "tif";
                    }
                    else if(response.ContentType.Contains("pdf"))
                    {
                        ext += "pdf";
                    }
                    else
                    {
                        Console.WriteLine("File Format " + response.ContentType + " is not Supported");
                        throw new Exception("File Format " + response.ContentType + " is not Supported");
                    }
                    using (FileStream fileStream = File.Create(case_doc.fileName + ext))
                    {
                        var buffer = new byte[4096];
                        int bytesRead;

                        while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fileStream.Write(buffer, 0, bytesRead);
                        }
                        case_doc.downloaded = true;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private bool savePageInfo(string path,CourtCase case1)
        {
            DocumentWriter doc = null;
            try
            {

                doc = new DocumentWriter(path+"/"+case1.caseNum);

                doc.addHeading(case1.caseNum);

                #region Div Case Information
                string caseInfoSel = "#divCaseInformation_body";
                var caseInfoDiv = FindElementIfExists(By.CssSelector(caseInfoSel));
                if(caseInfoDiv != null)
                {
                    var caseInfochildDivs = driver.FindElementsByCssSelector(caseInfoSel + " > div");
                    doc.addHeading("Case Information");

                    foreach(var cicd in caseInfochildDivs)
                    {
                        var attr = cicd.GetAttribute("class");
                        doc.addText(cicd.Text);
                    }
                }
                #endregion

                #region Case Parties
                string casePartiessel = "#partyInformationDiv";
                var casepartiesDiv = FindElementIfExists(By.CssSelector(caseInfoSel));
                if (casepartiesDiv != null)
                {
                    var caseInfohead = FindElementIfExists(By.CssSelector("#divPartyInformation_header"));
                    if(caseInfohead != null)
                        doc.addHeading(caseInfohead.Text);

                    var casePartiesBody = FindElementIfExists(By.CssSelector("#divPartyInformation_body"));

                    if (casePartiesBody != null)
                    {
                        var casepartieschildDivs = driver.FindElementsByCssSelector("#divPartyInformation_body > div");
                        foreach (var cicd in casepartieschildDivs)
                        {
                            doc.addText(cicd.Text);
                        }
                    }
                }
                #endregion

                #region Disposition Events
                string dispositionEventsSel = "#dispositionInformationDiv";
                var dispositionEventDiv = FindElementIfExists(By.CssSelector(dispositionEventsSel));
                if(dispositionEventDiv != null)
                {
                    doc.addHeading("Disposotion Events");
                    var dispositionEventBody = FindElementIfExists(By.CssSelector("#dispositionInformationDiv > div:nth-child(2)"));
                    if(dispositionEventBody != null)
                    {
                        var childDivs = dispositionEventBody.FindElements(By.CssSelector("div > div > div"));
                        foreach(var div in childDivs)
                        {
                            doc.addText(div.Text);
                        }
                    }
                }
                #endregion

                #region Events and Hearings
                string eventInfoSel = "#eventsInformationDiv";
                var eventInfoDiv = FindElementIfExists(By.CssSelector(eventInfoSel));
                if(eventInfoSel != null)
                {
                    doc.addHeading("Events and Hearings");

                    var eventInfoBody = FindElementIfExists(By.CssSelector(".list-group"));
                    if(eventInfoBody != null)
                    {
                        var childLi = eventInfoBody.FindElements(By.TagName("li"));
                        foreach(var ch in childLi)
                        {
                            doc.addText(ch.Text);
                        }
                    }

                }
                #endregion

                #region Financial
                string financialSel = "#financialSlider";
                var financialDiv = FindElementIfExists(By.CssSelector(financialSel));
                if (financialDiv != null)
                {
                    doc.addHeading("Financial");
                    var financialBody = FindElementIfExists(By.CssSelector("#financialSlider > div:nth-child(1)"));
                    var childs = driver.FindElementsByCssSelector("#financialSlider > div:nth-child(1) > div");
                    foreach (var ch in childs)
                    {
                        var attr = ch.GetAttribute("class");
                        doc.addText(ch.Text);
                    }
                }
                #endregion

                doc.Save();

                return true;
            }
            catch (Exception ex)
            {
                if (doc!= null)
                    doc.Save();
                return false;
            }
        }

        private void CheckDataIntegrity(string path, CourtCase case1)
        {
            foreach(var c_doc in case1.Documents)
            {
                if(c_doc.downloaded == false)
                {
                    retryDownloadFile(path,c_doc);
                    if(c_doc.downloaded == false)
                    {
                        logFileUnableToDownload(c_doc);
                    }
                }
                else
                {
                    if (!checkFileExist(path, c_doc))
                    {
                        c_doc.downloaded = false;
                        retryDownloadFile(path, c_doc);
                        if (c_doc.downloaded == false)
                        {
                            logFileUnableToDownload(c_doc);
                        }
                    }
                }
            }
        }

        private bool retryDownloadFile(string path, CaseDocument c_doc)
        {
            for (int i = 0; i < 3 && !c_doc.downloaded; i++)
                if (downloadDocument(c_doc, null,false))
                    return true;
            return false;
        }

        private void logNoDocumentsFound(CourtCase case1)
        {
            Console.WriteLine("No Documents Found For Case " + case1.caseNum.ToString());
        }

        private void logCaseNotFound(CrossRefNumber crossRef)
        {
            Console.WriteLine("No Cases are Found Against Cross Ref Number " + crossRef.refNum, ToString());
        }

        private void logFileUnableToDownload(CaseDocument c_doc)
        {
            Console.WriteLine("Unable To Download File " + Path.GetDirectoryName(c_doc.fileName) );
        }

        private bool checkFileExist(string path, CaseDocument c_doc)
        {
            return File.Exists(c_doc.fileName + ".tif") || File.Exists(c_doc.fileName + ".pdf");
        }

        private bool DownloadFile(string url)
        {
            try
            {
                // Construct HTTP request to get the file
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.CookieContainer = new System.Net.CookieContainer();
                httpRequest.ProtocolVersion = HttpVersion.Version10;
                for (int i = 0; i < driver.Manage().Cookies.AllCookies.Count - 1; i++)
                {
                    System.Net.Cookie ck = new System.Net.Cookie(driver.Manage().Cookies.AllCookies[i].Name, driver.Manage().Cookies.AllCookies[i].Value, driver.Manage().Cookies.AllCookies[i].Path, driver.Manage().Cookies.AllCookies[i].Domain);
                    httpRequest.CookieContainer.Add(ck);
                }
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;

                httpRequest.Accept = "text/html, application/xhtml+xml, */*";
                httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";

                //HttpStatusCode responseStatus;

                // Get back the HTTP response for web server
                HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                Stream httpResponseStream = httpResponse.GetResponseStream();

                // Define buffer and buffer size
                int bufferSize = 1024;
                byte[] buffer = new byte[bufferSize];
                int bytesRead = 0;

                // Read from response and write to file
                FileStream fileStream = File.Create("File1.pdf");
                while ((bytesRead = httpResponseStream.Read(buffer, 0, bufferSize)) != 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private string get_case_number()
        {
            try
            {
                string casenumbersel = "#divCaseInformation_body > div:nth-child(3) > div:nth-child(1) > p:nth-child(1)";
                var casenumberp = driver.FindElementByCssSelector(casenumbersel);
                Console.WriteLine(casenumberp.Text);
                return casenumberp.Text;
            }
            catch
            {
                return "";
            }
        }

        private void takescreenshot(string name)
        {
            //var sc = driver.GetScreenshot();
            //sc.SaveAsFile(name+".jpg");
        }

        private IWebElement FindElementIfExists(By by)
        {
            try
            {
                IWebElement webElement;
                new WebDriverWait(driver, TimeSpan.FromSeconds(web_el_wait)).Until(ExpectedConditions.ElementExists(by));
                var webElements = driver.FindElements(by);
                if (webElements.Count >= 1)
                {
                    webElement = webElements.First<IWebElement>();
                }
                else
                {
                    webElement = null;
                }
                return webElement;
            }
            catch
            {
                return null;
            }
        }
        private void ShowDriverState()
        {
            Console.WriteLine(driver.Url);
            Console.WriteLine(driver.Title);
            Console.WriteLine(driver.SessionId);
        }

        private bool myDownloadFile(string url, string filename)
        {
            try
            {
                bool flag;
                WebClient webClient = new WebClient();
                webClient.Headers[HttpRequestHeader.Cookie] = this.cookieString(driver);
                webClient.DownloadFile(url, filename + ".tif");
                string item = webClient.ResponseHeaders["Content-Type"];
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region Private member classes

        private class CaseDocument
		{
			public string description
			{
				get;
				set;
			}

			public string DocType
			{
				get;
				set;
			}

			public bool downloaded
			{
				get;
				set;
			}

			public string fileName
			{
				get;
				set;
			}

            public int fileNumber { get; set; }

			public string FragmentID
			{
				get;
				set;
			}

			public string pages
			{
				get;
				set;
			}

			public string URL
			{
				get;
				set;
			}

            public string D_URL
            {
                get;
                set;
            }

            public CourtCase inCase
            {
                get;
                set;
            }

			public CaseDocument()
			{
                downloaded = false;
			}
		}

		private class CourtCase
		{
			public string caseNum
			{
				get;
				set;
			}

			public List<Locate.CaseDocument> Documents
			{
				get;
				set;
			}

			public string URL
			{
				get;
				set;
			}

            public string CasePath
            {
                get;
                set;
            }

			public CourtCase()
			{
                Documents = new List<CaseDocument>();
			}
		}

		private class CrossRefNumber
		{
			public int caseCount
			{
				get;
				set;
			}

            public int caseProcessed { get; set; }

			public List<Locate.CourtCase> cases
			{
				get;
				set;
			}

			public string refNum
			{
				get;
				set;
			}

			public CrossRefNumber()
			{
                cases = new List<CourtCase>();
                caseProcessed = 0;
                caseCount = 0;
			}
        }

        #endregion

        #region Destructors

        ~Locate()
        {
            if (driver != null)
                driver.Quit();
        }

        public void Dispose()
        {
            driver.Quit();
        }

        #endregion
    }
}