using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics.Contracts;
using System.IO;


using System.Windows.Forms;
using System.Web;
using System.Drawing;
using System.Threading;
using Coypu;
using Coypu.Drivers.Selenium;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace UrlInspector
{
    public partial class Form1 : Form
    {
        public struct UrlInfo
        {
            public string Url { set; get; }
            public bool IsAccessible { set; get; }
            public string Remark { set; get; }
            public string Content { set; get; }
        }
        public string _myDocumentFolderPath = "";
        public string MyDocumentFolderPath
        {
            get
            {
                if (string.IsNullOrEmpty(_myDocumentFolderPath))
                    _myDocumentFolderPath = string.Format(@"{0}\{1}\", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"UrlInspector");

                FileInfo info = new FileInfo(_myDocumentFolderPath);
                if (!info.Directory.Exists)
                    System.IO.Directory.CreateDirectory(info.DirectoryName);

                return _myDocumentFolderPath;
            }
        }


        WebBrowser browser;

        public Form1()
        {
            InitializeComponent();
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {

            var sessionConfiguration = new SessionConfiguration();

            sessionConfiguration.Driver = typeof(SeleniumWebDriver);
            sessionConfiguration.Browser = Coypu.Drivers.Browser.Firefox;

            using (var browser = new BrowserSession(sessionConfiguration))
            {
                browser.MaximiseWindow();
                List<UrlInfo> urlList = new List<UrlInfo>();

                UrlInfo url;
                Uri theUri;
                foreach (string theUrlString in inputTb.Lines)
                {
                    url = new UrlInfo();
                    url.Url = theUrlString;
                    url.Content = "";
                    url.IsAccessible = false;
                    theUri = null;
                    bool isUri = Uri.TryCreate(theUrlString, UriKind.Absolute, out theUri);
                    if (!isUri)
                    {
                        url.IsAccessible = false;
                        url.Remark = "Not a valid URL";
                        urlList.Add(url);
                        continue;
                    }

                    bool IsRespond = isValidURL(theUrlString);

                    if (!IsRespond)
                    {
                        url.Remark = "No response from server";
                        urlList.Add(url);
                        continue;
                    }
                    url.IsAccessible = true;
                    //browser.Navigate(theUri);
                    url.Remark = "Accessible";
                    //url.Content = new WebClient().DownloadString(theUri);

                    browser.Visit(theUrlString);
                    url.Content = browser.Text;
                    url.Remark = browser.Title;

                    if (pageCheck.Checked)
                        browser.SaveScreenshot(string.Format("{0}{1}.jpeg", MyDocumentFolderPath, DateTime.Now.ToString("ddMMyyyyhhmmsstt")), ImageFormat.Jpeg);

                    if (windowsCheck.Checked)
                        PrintScreen(string.Format("{0}{1}.jpeg", MyDocumentFolderPath, DateTime.Now.ToString("ddMMyyyyhhmmsstt")));

                    urlList.Add(url);
                }

                resultDataGrid.DataSource = urlList;
            }
        }

        public void PrintScreen(string path)
        {
            Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            Graphics graphics = Graphics.FromImage(printscreen as Image);

            graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);

            printscreen.Save(path, ImageFormat.Jpeg);
        }

        public bool isValidURL(string url)
        {
            WebRequest webRequest = WebRequest.Create(url);
            WebResponse webResponse;
            try
            {
                webResponse = webRequest.GetResponse();
            }
            catch //If exception thrown then couldn't get response from address
            {
                return false;
            }
            return true;
        }

        private void resultDataGrid_CellContentClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {

        }

        private void OpenFolderBtn_Click(object sender, EventArgs e)
        {
            Process.Start(MyDocumentFolderPath);
        }
    }
}
