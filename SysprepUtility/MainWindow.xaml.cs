using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace SysprepUtility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker backgroundWorker = new BackgroundWorker();
        string lblmsgText = "Loading";
        Dictionary<string, string> UserDetails = new Dictionary<string, string>();
        string jsonPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\Sysprep.json";
        string sysprepfile = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\Sysprep.xml";
       
        public MainWindow()
        {
            InitializeComponent();
            //UserDetails=jsonRead.Readjson(File.ReadAllText(jsonPath));
            //backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.ProgressChanged += ProgressChanged;
            backgroundWorker.DoWork += DoWork;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;           
        }
        
        #region Updatexml_RunSysprep
        public void UpdateSysprepxml(string path)
        {
            SysprepOperation.writeXMLFile(path,UserDetails);
            SysprepOperation.RunSysprep(sysprepfile); //commented for testing
        }
        #endregion
        private void DoWork(object sender, DoWorkEventArgs e)
        {  
            try
            {
                backgroundWorker.WorkerReportsProgress = true;
                lblmsgText = "Getting the Json";
                backgroundWorker.ReportProgress(35);
                Thread.Sleep(5000);
                lblmsgText = "Parsing the Json Data";
                UserDetails = jsonRead.Readjson(File.ReadAllText(jsonPath));
                backgroundWorker.ReportProgress(70);
                Thread.Sleep(5000);
                lblmsgText = "Running Sysprep";
                UpdateSysprepxml(sysprepfile);
                backgroundWorker.ReportProgress(100);
                Thread.Sleep(5000);
            }
            catch(Exception ex) { }           
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // This is called on the UI thread when ReportProgress method is called
            prgbar.Value = e.ProgressPercentage;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                lblProgressChange.Content = lblmsgText;
            }));
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lblProgressChange.Content = "Rebooting the Device...";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            backgroundWorker.RunWorkerAsync();
        }
    }
}

