using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SeleniumExtras.WaitHelpers;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
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

	[Parallelizable]
    public class Locate : IDisposable
    {

        #region Events
        public delegate void ShowMessagebox(string data);
        public ShowMessagebox showMessage = delegate { };

        #endregion

        #region fields

        private PhantomJSDriver driver;

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

        #endregion

        #region constructors

        public Locate()
		{


            var service = PhantomJSDriverService.CreateDefaultService(Environment.CurrentDirectory);
            service.WebSecurity = false;
            service.HideCommandPromptWindow = true;
            driver = new PhantomJSDriver(service);
            driver.Manage().Window.Size = new System.Drawing.Size(1240,1240);

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
            if (this.driver.Url != HomePageUrl)
            {
                return "Login Before Locating Cases";
            }
            else
            {
                try
                {
                    if (!this.NavigateToSearchUrl())
                    {
                        return "Naviagtion To search Url Failed";
                    }
                    ShowDriverState();
                    if (!SearchRefNum(case_ref_num.refNum))
                    {
                        return "Unable TO find Ref Num";
                    }
                    Thread.Sleep(2000);
                    ShowDriverState();
                    if (!findCases(path, case_ref_num))
                    {
                        return "Unable To Find Cases";
                    }
                    ShowDriverState();
                    #region OLD code
                    //    ReadOnlyCollection<IWebElement> webElements = this._d.FindElements(By.CssSelector("a[href*= 'CaseDetail.aspx?CaseID=']"));
                    //    List<Locate.CourtCase> courtCases = new List<Locate.CourtCase>();
                    //    Thread.Sleep(2000);
                    //    foreach (IWebElement webElement in webElements)
                    //    {
                    //        try
                    //        {
                    //            string str = webElement.GetAttribute("href").Replace("https://www.clarkcountycourts.us/Secure/", "");
                    //            if (this.FindElementIfExists(By.CssSelector(string.Concat("a[href= '", str, "']"))) == null)
                    //            {
                    //                Console.WriteLine(string.Concat(new string[] { refNum, " Case not found on this page: ", webElement.Text, ", ", str }));
                    //            }
                    //            else
                    //            {
                    //                courtCases.Add(new Locate.CourtCase()
                    //                {
                    //                    URL = webElement.GetAttribute("href"),
                    //                    caseNum = webElement.Text
                    //                });
                    //            }
                    //        }
                    //        catch (Exception exception)
                    //        {
                    //            Console.WriteLine(string.Concat("Case not found on this page: ", webElement.Text));
                    //        }
                    //    }
                    //    crossRefNumbers.Add(new Locate.CrossRefNumber()
                    //    {
                    //        refNum = refNum,
                    //        caseCount = webElements.Count,
                    //        cases = courtCases
                    //    });
                    //    if (webElements.Count > 3)
                    //    {
                    //        Console.WriteLine("{0} has {1} cases", refNum, webElements.Count.ToString());
                    //        foreach (Locate.CourtCase courtCase in courtCases)
                    //        {
                    //            Console.WriteLine(courtCase.caseNum);
                    //        }
                    //        this._d.SwitchTo().Window(this._d.WindowHandles.Last<string>());
                    //    }
                    //    try
                    //    {
                    //        DataGridViewRow dataGridViewRow = (
                    //            from DataGridViewRow r in grd.Rows
                    //            where r.Cells[0].Value.ToString().Equals(refNum)
                    //            select r).First<DataGridViewRow>();
                    //        dataGridViewRow.Cells[1].Value = webElements.Count.ToString();
                    //        grd.Refresh();
                    //    }
                    //    catch (Exception exception1)
                    //    {
                    //    }
                    //    foreach (Locate.CourtCase courtCase1 in courtCases)
                    //    {
                    //        List<Locate.CaseDocument> caseDocuments = new List<Locate.CaseDocument>();
                    //        this.caseURL = courtCase1.URL;
                    //        this._d.Navigate().GoToUrl(courtCase1.URL);
                    //        Thread.Sleep(500);
                    //        IWebElement webElement1 = this.FindElementIfExists(By.CssSelector("a[href*= 'CPR.aspx?CaseID=']"));
                    //        while (webElement1 == null)
                    //        {
                    //            this.caseFilesURL = this.caseURL;
                    //            if (this.LoginSite(true))
                    //            {
                    //                webElement1 = this.FindElementIfExists(By.CssSelector("a[href*= 'CPR.aspx?CaseID=']"));
                    //            }
                    //            else
                    //            {
                    //                stackTrace = "Cannot login";
                    //                return stackTrace;
                    //            }
                    //        }
                    //        this.caseFilesURL = webElement1.GetAttribute("href");
                    //        this._d.Navigate().GoToUrl(this.caseFilesURL);
                    //        Thread.Sleep(500);
                    //        string str1 = string.Concat(this.DefaultPath, refNum.ToUpper(), "\\", courtCase1.caseNum);
                    //        DirectoryInfo directoryInfo = new DirectoryInfo(str1);
                    //        if (directoryInfo.Exists)
                    //        {
                    //            FileInfo[] files = directoryInfo.GetFiles();
                    //            for (int i = 0; i < (int)files.Length; i++)
                    //            {
                    //                files[i].Delete();
                    //            }
                    //            DirectoryInfo[] directories = directoryInfo.GetDirectories();
                    //            for (int j = 0; j < (int)directories.Length; j++)
                    //            {
                    //                directories[j].Delete(true);
                    //            }
                    //        }
                    //        (new DirectoryInfo(str1)).Create();
                    //        Thread.Sleep(1000);
                    //        (new DirectoryInfo(string.Concat(str1, "/pleadings"))).Create();
                    //        (new DirectoryInfo(string.Concat(str1, "/transcripts"))).Create();
                    //        Thread.Sleep(1000);
                    //        try
                    //        {
                    //            ReadOnlyCollection<IWebElement> webElements1 = this._d.FindElements(By.TagName("table"))[4].FindElements(By.TagName("a"));
                    //            foreach (IWebElement webElement2 in webElements1)
                    //            {
                    //                if ((string.IsNullOrEmpty(webElement2.Text) ? false : webElement2.GetAttribute("href").ToLower().Contains("viewdocumentfragment.aspx?documentfragmentid=")))
                    //                {
                    //                    string item = HttpUtility.ParseQueryString(webElement2.GetAttribute("href"))[0];
                    //                    caseDocuments.Add(new Locate.CaseDocument()
                    //                    {
                    //                        DocType = "pleadings",
                    //                        URL = webElement2.GetAttribute("href"),
                    //                        fileName = webElement2.Text,
                    //                        FragmentID = item
                    //                    });
                    //                }
                    //            }
                    //        }
                    //        catch (Exception exception2)
                    //        {
                    //            stackTrace = exception2.StackTrace;
                    //            return stackTrace;
                    //        }
                    //        Thread.Sleep(1000);
                    //        try
                    //        {
                    //            ReadOnlyCollection<IWebElement> webElements2 = this._d.FindElements(By.TagName("table"))[5].FindElements(By.TagName("a"));
                    //            foreach (IWebElement webElement3 in webElements2)
                    //            {
                    //                if ((string.IsNullOrEmpty(webElement3.Text) ? false : webElement3.GetAttribute("href").ToLower().Contains("viewdocumentfragment.aspx?documentfragmentid=")))
                    //                {
                    //                    string item1 = HttpUtility.ParseQueryString(webElement3.GetAttribute("href"))[0];
                    //                    caseDocuments.Add(new Locate.CaseDocument()
                    //                    {
                    //                        DocType = "transcripts",
                    //                        URL = webElement3.GetAttribute("href"),
                    //                        fileName = webElement3.Text,
                    //                        FragmentID = item1
                    //                    });
                    //                }
                    //            }
                    //        }
                    //        catch (Exception exception3)
                    //        {
                    //            stackTrace = exception3.StackTrace;
                    //            return stackTrace;
                    //        }
                    //        courtCase1.Documents = caseDocuments;
                    //        Thread.Sleep(1000);
                    //        int num = 1;
                    //        bool flag = false;
                    //        List<Locate.CaseDocument> caseDocuments1 = new List<Locate.CaseDocument>();
                    //        List<int> nums = new List<int>();
                    //        caseDocuments1.Clear();
                    //        nums.Clear();
                    //        foreach (Locate.CaseDocument caseDocument in caseDocuments)
                    //        {
                    //            if ((flag ? false : caseDocument.DocType == "transcripts"))
                    //            {
                    //                flag = true;
                    //                num = 1;
                    //            }
                    //            Thread.Sleep(100);
                    //            if (!this.downloadFile(caseDocument.URL, string.Concat(str1, "\\", caseDocument.DocType, "\\"), caseDocument.fileName, caseDocument.FragmentID, num, false))
                    //            {
                    //                caseDocuments1.Add(caseDocument);
                    //                nums.Add(num);
                    //            }
                    //            num++;
                    //        }
                    //        if (caseDocuments1.Count > 0)
                    //        {
                    //            try
                    //            {
                    //                this.LoginSite(true);
                    //            }
                    //            catch (Exception exception4)
                    //            {
                    //                stackTrace = "Cannot login";
                    //                return stackTrace;
                    //            }
                    //            int num1 = 0;
                    //            foreach (Locate.CaseDocument caseDocument1 in caseDocuments1)
                    //            {
                    //                Thread.Sleep(100);
                    //                this.downloadFile(caseDocument1.URL, string.Concat(str1, "\\", caseDocument1.DocType, "\\"), caseDocument1.fileName, caseDocument1.FragmentID, nums[num1], true);
                    //                num1++;
                    //            }
                    //        }
                    //    }
                    #endregion
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
            bool flag = false;
            try
            {
                driver.Navigate().GoToUrl(SignInUrl);
                Thread.Sleep(2000);
                takescreenshot("login screen");
                //the driver can now provide you with what you need (it will execute the script)
                //get the source of the page
                //fully navigate the dom
                ShowDriverState();
                var pathElement = driver.FindElementById("UserName");
                pathElement.SendKeys(username);
                Thread.Sleep(2000);
                var pass = driver.FindElementById("Password");
                pass.SendKeys(password);
                var signin = driver.FindElementByClassName("tyler-btn-primary");
                Thread.Sleep(1000);
                signin.Submit();
                var body = new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(ExpectedConditions.UrlContains("Portal"));
                Thread.Sleep(1000);
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
                Thread.Sleep(1000);
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
                string allcasesUrl = driver.Url;
                foreach (var tr1 in trs)
                {
                    CourtCase case1 = new CourtCase();
                    var caseLink = tr1.FindElement(By.CssSelector(".caseLink"));
                    case1.URL = caseLink.GetAttribute("data-url");
                    case1.caseNum = caseLink.Text;
                    case_ref_num.cases.Add(case1);
                    new Actions(driver).Click(caseLink).Perform();
                    Thread.Sleep(1000);
                    string case_info_div_sel = "#divCaseInformation_body";
                    try
                    {
                        var body = new WebDriverWait(driver, TimeSpan.FromSeconds(120)).Until(ExpectedConditions.ElementExists(By.CssSelector(case_info_div_sel)));
                    }
                    catch(Exception ex)
                    {
                        Thread.Sleep(1000);
                        try
                        {
                            var body = new WebDriverWait(driver, TimeSpan.FromSeconds(120)).Until(ExpectedConditions.ElementExists(By.CssSelector(case_info_div_sel)));
                        }
                        catch (Exception ex1)
                        {
                            Console.WriteLine(driver.Url);
                            Console.WriteLine(ex1.Message);
                            return false;
                        }
                    }

                    Console.WriteLine("case found" + caseLink.Text);
                    Thread.Sleep(1000);
                    bool flg = process_case(path,case1);
                    if (!flg)
                    {
                        return flg;
                    }
                }
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
                string documentLinkSel = "#PrintMask > div:nth-child(4) > div:nth-child(1) > nav:nth-child(1) > div:nth-child(1) > ul:nth-child(1) > li:nth-child(5) > a:nth-child(1)";
                var documentLink = FindElementIfExists(By.CssSelector(documentLinkSel));
                if(documentLink != null)
                {
                    new Actions(driver).Click(documentLink).Perform();
                    try
                    {
                        var body = new WebDriverWait(driver, TimeSpan.FromSeconds(60)).Until(ExpectedConditions.ElementExists(By.CssSelector("#DocumentsPrintSection")));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    Thread.Sleep(1000);
                    takescreenshot("document panel clicked");
                    Thread.Sleep(1000);
                    Directory.CreateDirectory(path + "/" + case1.caseNum);
                    savePageInfo(path + "/" + case1.caseNum, case1);
                    downloadDocuments(path + "/" + case1.caseNum,case1);
                    CheckDataIntegrity(path + "/" + case1.caseNum, case1);
                }
                Thread.Sleep(500);
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

                foreach (var doc_p in docs_p)
                {
                    CaseDocument casedoc = new CaseDocument();
                    casedoc.inCase = case1;
                    var doc_a = doc_p.FindElement(By.TagName("a"));
                    casedoc.URL = doc_a.GetAttribute("href");
                    casedoc.description = doc_p.Text;
                    var doc_filename_span = doc_p.FindElement(By.TagName("span"));
                    casedoc.fileName = RemoveIllegalChars( doc_filename_span.Text);
                    case1.Documents.Add(casedoc);
                }

                for (int i = 0; i < case1.Documents.Count;i+=20 )
                {
                    int th_count = case1.Documents.Count - i < 20 ? case1.Documents.Count - i : 20;
                    Thread[] ths = new Thread[th_count];
                    for (int j= 0 ;j<th_count;j++)
                    {
                        var docs = case1.Documents[i + j];
                        Console.WriteLine(docs.URL);
                        docs.fileNumber = i + j + 1;
                        var file_num_str = docs.fileNumber.ToString().PadLeft(4, '0');
                        docs.fileName = path + "/" + file_num_str + "-" + docs.fileName;
                        downloadDocument(docs, path, ths[j]);
                    }
                    for (int j =0 ;j<th_count;j++)
                    {
                        if (ths[j] != null)
                        {
                            try
                            {
                                ths[j].Join();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }

                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(driver.Url);

            }
        }

        private bool downloadDocument(CaseDocument case_doc,string filename, Thread th)
        {
            try
            {
                string downloadLinksel = "a.btn:nth-child(1)";
                takescreenshot("download document");
                driver.Navigate().GoToUrl(case_doc.URL);
                try
                {
                    var body = new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(ExpectedConditions.ElementExists(By.CssSelector(downloadLinksel)));
                }
                catch(Exception ex)
                {
                    Thread.Sleep(1000);
                    try
                    {
                        var body = new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(ExpectedConditions.ElementExists(By.CssSelector(downloadLinksel)));
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

                if (th != null)
                {
                    th = new Thread(() => { bool is_downloaded = TryDownloadFile(case_doc); });
                    th.Start();
                    return true;
                }
                else
                {
                    return TryDownloadFile(case_doc);
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
                IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(120.00));
                Thread.Sleep(1000);
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

                var advanceOptionButton = driver.FindElementById(advanceOptionButtonId);
                advanceOptionButton.SendKeys(OpenQA.Selenium.Keys.Enter);
                Thread.Sleep(1000);
                takescreenshot("advance options selected");
                
                var maskdiv = driver.FindElementById("AdvOptionsMask");
                Console.WriteLine(maskdiv.Displayed);
                Console.WriteLine(driver.Url);

                string spantoClicksel = "#AdvOptionsMask > div:nth-child(1) > div:nth-child(2) > div:nth-child(1) > fieldset:nth-child(3) > span:nth-child(1) > span:nth-child(1) > span:nth-child(2)";
                var spantoclick = driver.FindElementByCssSelector(spantoClicksel);
                spantoclick.SendKeys(OpenQA.Selenium.Keys.Enter);
                spantoclick.Click();
                Thread.Sleep(1000);
                takescreenshot("span clicked");
                Thread.Sleep(1000);

                string litoclicksel = "#caseCriteria_SearchBy_listbox > li:nth-child(5)";
                var litoclick = driver.FindElementByCssSelector(litoclicksel);
                litoclick.SendKeys(OpenQA.Selenium.Keys.Enter);
                Thread.Sleep(1000);
                takescreenshot("li clicked");
                litoclick.Click();
                Thread.Sleep(1000);
                takescreenshot("li clicked1");
                Thread.Sleep(1000);

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

                var searhInput = driver.FindElementById(searchCriteriaInput);
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
                IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(120.00));
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
                    if(!response.ContentType.Contains("tiff"))
                    {
                        Console.WriteLine(response.ContentType);
                        return false;
                    }
                    string ext = "." + response.ContentType;
                    using (FileStream fileStream = File.Create(case_doc.fileName + ".tif"))
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
                if (downloadDocument(c_doc, path, null))
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
            return File.Exists(c_doc.fileName+".tif");
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
            IWebElement webElement;
            ReadOnlyCollection<IWebElement> webElements = driver.FindElements(by);
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