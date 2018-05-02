using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium;
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

namespace CaseLocator
{
	[Parallelizable]
	public class Locate
    {
        #region fields

        private PhantomJSDriver driver;

		private string DownloadPath = string.Concat(Directory.GetCurrentDirectory(), "\\Attachments\\");

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
            driver = new PhantomJSDriver();
            driver.Manage().Window.Size = new System.Drawing.Size(1240,1240);
            Console.WriteLine(driver.Capabilities.ToString());
            SignInUrl = "https://www.clarkcountycourts.us/Portal/Account/Login";
            SignInUrl1 = "https://odysseyadfs.tylertech.com/IdentityServer/account/signin?ReturnUrl=%2fIdentityServer%2fissue%2fwsfed%3fwa%3dwsignin1.0%26wtrealm%3dhttps%253a%252f%252fOdysseyADFS.tylertech.com%252fadfs%252fservices%252ftrust%26wctx%3d4d2b3478-8513-48ad-8998-2652d72a38e9%26wauth%3durn%253a46%26wct%3d2018-04-29T15%253a42%253a35Z%26whr%3dhttps%253a%252f%252fodysseyadfs.tylertech.com%252fidentityserver&wa=wsignin1.0&wtrealm=https%3a%2f%2fOdysseyADFS.tylertech.com%2fadfs%2fservices%2ftrust&wctx=4d2b3478-8513-48ad-8998-2652d72a38e9&wauth=urn%3a46&wct=2018-04-29T15%3a42%3a35Z&whr=https%3a%2f%2fodysseyadfs.tylertech.com%2fidentityserver";
            HomePageUrl = "https://www.clarkcountycourts.us/Portal/";
		}

        #endregion

        #region Public members

        [TestMethod]
        public string LocateCase(string refNum, DataGridView grd)
        {
            string stackTrace;
            List<Locate.CrossRefNumber> crossRefNumbers = new List<Locate.CrossRefNumber>();
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
                    SearchRefNum(refNum);
                    Thread.Sleep(2000);
                    ShowDriverState();
                    findCases();
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
                driver.Url = SignInUrl;
                driver.Navigate();
                IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(30.00));
                //wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
                takescreenshot("login screen");
                //the driver can now provide you with what you need (it will execute the script)
                //get the source of the page
                //fully navigate the dom
                Thread.Sleep(2000);
                ShowDriverState();
                var pathElement = driver.FindElementById("UserName");
                pathElement.SendKeys(username);
                Thread.Sleep(2000);
                var pass = driver.FindElementById("Password");
                pass.SendKeys(password);
                var signin = driver.FindElementByClassName("tyler-btn-primary");
                signin.Submit();
                wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
                if (driver.Url != HomePageUrl )
                {
                    Console.WriteLine(driver.Title);
                    Console.WriteLine(driver.Url);
                    Thread.Sleep(1000);
                    flag = false; 
                }
                else
                {
                    Console.WriteLine(driver.Title);
                    Console.WriteLine(driver.Url);
                    Thread.Sleep(1000);
                    flag = true;
                }
            }
            catch(Exception ex)
            {
                flag = false;
            }
            takescreenshot("afterLoginScreen");
            return flag;
        }

        public void logout()
        {
            try
            {
                driver.Url = "https://www.clarkcountycourts.us/Portal/Account/LogOff";
                driver.Navigate();
                Thread.Sleep(2000);
            }
            catch
            {
                
            }
        }

        #endregion

        #region private member functions

        private bool findCases()
        {
            try
            {
                takescreenshot("before finding cases");
                string tbodysel = "#CasesGrid > table:nth-child(1) > tbody:nth-child(3)";
                var tbody = driver.FindElementByCssSelector(tbodysel);
                var trs = tbody.FindElements(By.TagName("tr"));
                Console.WriteLine(trs.Count);
                foreach (var tr1 in trs)
                {
                    var caseLink = tr1.FindElement(By.CssSelector(".caseLink"));
                    new Actions(driver).KeyDown(OpenQA.Selenium.Keys.Control).Click(caseLink).KeyUp(OpenQA.Selenium.Keys.Control).Perform();
                    takescreenshot("Case clicked");
                    Thread.Sleep(1000);
                    process_case();
                }
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        private bool process_case()
        {
            try
            {
                string casenumber = get_case_number();
                string documentLinkSel = "#PrintMask > div:nth-child(4) > div:nth-child(1) > nav:nth-child(1) > div:nth-child(1) > ul:nth-child(1) > li:nth-child(5) > a:nth-child(1)";
                var documentLink = driver.FindElementByCssSelector(documentLinkSel);
                if(documentLink != null)
                {
                    documentLink.Click();
                    Thread.Sleep(1000);
                    takescreenshot("documents panel clicked");
                    Thread.Sleep(1000);
                    downloadDocuments();
                }
                Thread.Sleep(500);
                return true;
            }
            catch
            {
                return false;
            }
        }


        private void downloadDocuments()
        {
            string documents_tables_sel = "#divDocumentsInformation_body";
            var documents_tables = driver.FindElementByCssSelector(documents_tables_sel);
            var docs_p = documents_tables.FindElements(By.TagName("p"));
            foreach (var doc_p in docs_p)
            {
                Console.WriteLine(doc_p.Text);
                var doc_a = doc_p.FindElement(By.TagName("a"));
                Console.WriteLine(doc_a.GetAttribute("href"));
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

        private bool NavigateToSearchUrl()
        {
            try
            {
                var smartSearhDiv = driver.FindElementById("portlet-29");
                var smartSearchA = smartSearhDiv.FindElement(By.CssSelector("a"));
                string smartSearchUrl = smartSearchA.GetAttribute("href");
                driver.Url = smartSearchUrl;
                driver.Navigate();
                IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(30.00));
                wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void takescreenshot(string name)
        {
            var sc = driver.GetScreenshot();
            sc.SaveAsFile(name+".jpg");
        }

        //[TearDown]
        //private void Cleanup()
        //{
        //    this._d.Quit();
        //}

        //private string cookieString(IWebDriver driver)
        //{
        //    string str = string.Join("; ", 
        //        from c in driver.Manage().Cookies.AllCookies
        //        select string.Format("{0}={1}", c.Name, c.Value));
        //    return str;
        //}

        //private void createURLShortcut(string localPath, string linkUrl)
        //{
        //    using (StreamWriter streamWriter = new StreamWriter(localPath))
        //    {
        //        streamWriter.WriteLine("[{000214A0-0000-0000-C000-000000000046}]");
        //        streamWriter.WriteLine("Prop3 = 19, 11");
        //        streamWriter.WriteLine("[InternetShortcut]");
        //        streamWriter.WriteLine("IDList=");
        //        streamWriter.WriteLine(string.Concat("URL=", linkUrl));
        //        streamWriter.Flush();
        //    }
        //}

        //private void creatpdf(string path)
        //{
        //    PdfDocument pdfDocument = new PdfDocument();
        //    PdfPage pdfPage = pdfDocument.AddPage();
        //    XGraphics xGraphic = XGraphics.FromPdfPage(pdfPage);
        //    XFont xFont = new XFont("Verdana", 10, PdfSharp.Drawing.XFontStyle.Bold);
        //    XSolidBrush black = XBrushes.Black;
        //    double point = pdfPage.Width.Point;
        //    XUnit height = pdfPage.Height;
        //    //xGraphic.DrawString("YOUR USER DOES NOT HAVE PERMISSION TO VIEW THIS DOCUMENT", xFont, black, new XRect(0, 0, point, height.Point), XStringFormats.Center);
        //    pdfDocument.Save(path);
        //}

        //private bool downloadFile(string url, string localPath, string fileName, string fragmentID, int itemNum, bool retry)
        //{
        //    bool flag;
        //    WebClient webClient = new WebClient();
        //    webClient.Headers[HttpRequestHeader.Cookie] = this.cookieString(this._d);
        //    string str = "";
        //    int num = 0;
        //    fileName = this.RemoveIllegalChars(fileName);
        //    while (true)
        //    {
        //        if ((str != "" ? false : num < 10))
        //        {
        //            num++;
        //            string item = "";
        //            if (File.Exists(string.Concat(localPath, fileName)))
        //            {
        //                File.Delete(string.Concat(localPath, fileName));
        //            }
        //            try
        //            {
        //                webClient.DownloadFile(url, string.Concat(localPath, fileName));
        //                item = webClient.ResponseHeaders["Content-Type"];
        //            }
        //            catch (Exception exception)
        //            {
        //            }
        //            if (item.ToLower().Contains("tiff"))
        //            {
        //                str = ".tif";
        //            }
        //            else if (!item.ToLower().Contains("pdf"))
        //            {
        //                if (File.Exists(string.Concat(localPath, fileName)))
        //                {
        //                    File.Delete(string.Concat(localPath, fileName));
        //                }
        //                if ((fileName.ToLower().Contains("sealed") || fileName.ToLower().StartsWith("fus ") || fileName.ToLower().StartsWith("filed under seal") ? false : !fileName.ToLower().StartsWith("non-public")))
        //                {
        //                    if (this._d.Url.ToLower() != "https://www.clarkcountycourts.us/secure/casedocuments.aspx")
        //                    {
        //                        this.LoginSite(true);
        //                    }
        //                    try
        //                    {
        //                        IWebElement webElement = this._d.FindElement(By.CssSelector(string.Concat("a[href*= '=", fragmentID, "&']")));
        //                        webElement.Click();
        //                        if (this._d.FindElement(By.CssSelector(string.Concat("a[href*= '=", fragmentID, "&']"))).FindElement(By.XPath("./parent::*")).FindElement(By.XPath("./parent::*")).GetCssValue("background-color").ToString() == "rgba(255, 192, 203, 1)")
        //                        {
        //                            str = ".pdf";
        //                            this.creatpdf(this.finalPath(localPath, itemNum, fileName, str));
        //                            flag = true;
        //                            break;
        //                        }
        //                    }
        //                    catch (Exception exception1)
        //                    {
        //                    }
        //                    int num1 = 0;
        //                    while (num1 < 20)
        //                    {
        //                        Thread.Sleep(5000);
        //                        try
        //                        {
        //                            if (File.Exists(string.Concat(this.DownloadPath, "DocumentFragment_", fragmentID, ".pdf")))
        //                            {
        //                                File.Move(string.Concat(this.DownloadPath, "DocumentFragment_", fragmentID, ".pdf"), this.finalPath(localPath, itemNum, fileName, ".pdf"));
        //                                flag = true;
        //                                return flag;
        //                            }
        //                            else if (File.Exists(string.Concat(this.DownloadPath, "DocumentFragment_", fragmentID, ".tif")))
        //                            {
        //                                File.Move(string.Concat(this.DownloadPath, "DocumentFragment_", fragmentID, ".tif"), this.finalPath(localPath, itemNum, fileName, ".tif"));
        //                                flag = true;
        //                                return flag;
        //                            }
        //                        }
        //                        catch (Exception exception2)
        //                        {
        //                        }
        //                        if ((File.Exists(string.Concat(this.DownloadPath, "DocumentFragment_", fragmentID, ".pdf.crdownload")) ? true : File.Exists(string.Concat(this.DownloadPath, "DocumentFragment_", fragmentID, ".tif.crdownload"))))
        //                        {
        //                            num1++;
        //                        }
        //                        else
        //                        {
        //                            break;
        //                        }
        //                    }
        //                    if ((num != 10 ? false : !retry))
        //                    {
        //                        flag = false;
        //                        break;
        //                    }
        //                    else if (num == 10 & retry)
        //                    {
        //                        this.createURLShortcut(this.finalPath(localPath, itemNum, fileName, ".lnk"), url);
        //                        Console.WriteLine(string.Concat("Unable to download: ", this.finalPath(localPath, itemNum, fileName, ".lnk")));
        //                        flag = false;
        //                        break;
        //                    }
        //                }
        //                else
        //                {
        //                    str = ".pdf";
        //                    this.creatpdf(this.finalPath(localPath, itemNum, fileName, str));
        //                    flag = true;
        //                    break;
        //                }
        //            }
        //            else
        //            {
        //                str = ".pdf";
        //            }
        //        }
        //        else
        //        {
        //            File.Move(string.Concat(localPath, fileName), this.finalPath(localPath, itemNum, fileName, str));
        //            flag = true;
        //            break;
        //        }
        //    }
        //    return flag;
        //}

        //private string finalPath(string localPath, int fileNum, string fileName, string ext)
        //{
        //    int num = 1;
        //    while (true)
        //    {
        //        string[] strArrays = new string[] { localPath, fileNum.ToString("D3"), " - ", fileName, null, null };
        //        strArrays[4] = (num == 1 ? "" : string.Concat(" (", num.ToString(), ")"));
        //        strArrays[5] = ext;
        //        if (!File.Exists(string.Concat(strArrays)))
        //        {
        //            break;
        //        }
        //        num++;
        //    }
        //    string[] strArrays1 = new string[] { localPath, fileNum.ToString("D3"), " - ", fileName, null, null };
        //    strArrays1[4] = (num == 1 ? "" : string.Concat(" (", num.ToString(), ")"));
        //    strArrays1[5] = ext;
        //    return string.Concat(strArrays1);
        //}

        //private IWebElement FindElementIfExists(By by)
        //{
        //    IWebElement webElement;
        //    ReadOnlyCollection<IWebElement> webElements = this._d.FindElements(by);
        //    if (webElements.Count >= 1)
        //    {
        //        webElement = webElements.First<IWebElement>();
        //    }
        //    else
        //    {
        //        webElement = null;
        //    }
        //    return webElement;
        //}

        //private void Init()
        //{
        //    int num = 0;
        //    while (true)
        //    {
        //        if ((this._d != null ? true : num >= 5))
        //        {
        //            break;
        //        }
        //        num++;
        //        try
        //        {
        //            ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService();
        //            chromeDriverService.HideCommandPromptWindow = true; 
        //            this.DefaultPath = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "\\cases\\");
        //            this.DefaultPath = (string.IsNullOrEmpty(this.UserDefinedPath) ? this.DefaultPath : string.Concat(this.UserDefinedPath, "\\"));
        //            (new DirectoryInfo(this.DownloadPath)).Create();
        //            var chromeOption = new ChromeOptions
        //            {
        //                BinaryLocation = @"C:\cn\chromedriver.exe",
        //            }; 
        //            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        //            this._d = new ChromeDriver(chromeDriverService, chromeOption, TimeSpan.FromMinutes(3));
        //            this._d.Manage().Timeouts().ImplicitWait=TimeSpan.FromSeconds(5);
        //        }
        //        catch (Exception exception)
        //        {
        //            Thread.Sleep(5000);
        //        }
        //    }
        //}


        //private bool LoginSite(bool goToCaseFiles)
        //{
        //    bool flag;
        //    try
        //    {
        //        this.Login();
        //        if (goToCaseFiles)
        //        {
        //            this._d.Navigate().GoToUrl(this.caseURL);
        //            this._d.Navigate().GoToUrl(this.caseFilesURL);
        //            if (this._d.Url.ToLower().Contains("erroroccured.aspx"))
        //            {
        //                this.Login();
        //                this.SearchRefNum();
        //                this._d.Navigate().GoToUrl(this.caseURL);
        //                this._d.Navigate().GoToUrl(this.caseFilesURL);
        //            }
        //        }
        //        flag = (this.FindElementIfExists(By.CssSelector("a[href*= 'logout.aspx']")) == null ? false : true);
        //    }
        //    catch (Exception exception)
        //    {
        //        flag = false;
        //    }
        //    return flag;
        //}

        //private string RemoveIllegalChars(string fileName)
        //{
        //    char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
        //    for (int i = 0; i < (int)invalidFileNameChars.Length; i++)
        //    {
        //        fileName = fileName.Replace(invalidFileNameChars[i], '\u005F');
        //    }
        //    return fileName;
        //}

        private bool SearchRefNum(string refNum)
        {
            try
            {
                string searchCriteriaInput = "caseCriteria_SearchCriteria";
                string advanceOptionButtonId = "AdvOptions";
                string caseCriteriaName = "caseCriteria.SearchBy_input";
                string caseCriteriaInpId = "caseCriteria_SearchBy";
                string casesearchByListBox = "caseCriteria_SearchBy_listbox";
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
                IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(30.00));
                wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
                takescreenshot("after search finiched");
                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        private void ShowDriverState()
        {
            Console.WriteLine(driver.Url);
            Console.WriteLine(driver.Title);
            Console.WriteLine(driver.SessionId);
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

			public CaseDocument()
			{
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

			public CourtCase()
			{
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
			}
        }

        #endregion
    }
}