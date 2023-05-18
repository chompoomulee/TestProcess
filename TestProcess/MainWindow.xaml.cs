using DotRas;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Timers;
using System.Windows.Threading;

namespace TestProcess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RasPhoneBook rasphonebook;
        private RasEntry entry;
        private Process process;
        private BackgroundWorker waitforexit;
        private readonly string connectionName = "VPN4Games(IKEv2)";
        private readonly string domain = "th2-v4g.vpn4games.com"; // test-sg.bullfreedom.xyz
        private readonly string username = "chompoomulee";
        private readonly string password = "214965@mulee";
        private readonly string path = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers);
        // Declare a DispatcherTimer as a class-level variable
        private DispatcherTimer timer;
        private DateTime startTime;
        List<DetailDomain> domains;
        public MainWindow()
        {
            InitializeComponent();
            SetListData();
            //ConnectButton_Click();
            //DisconnectButton_Click();
            //LoadIkev2System();
            //ChangeRegistryConfig();
            //CreateEntry();
            //CreateConfigTimer();
            //CreateProcess();
            ////WaitForExitBackgroundWorker();
            CreateConfig();
        }
        #region new
        private void ConnectButton_Click()
        {
            RasPhoneBook rasPhoneBook = new RasPhoneBook();
            rasPhoneBook.Open(path);

            #region Set Entry
            RasDevice device = RasDevice.GetDevices().Where(x => x.Name.Contains("WAN Miniport (IKEv2)")).FirstOrDefault();
            entry = RasEntry.CreateVpnEntry(connectionName, domain, RasVpnStrategy.IkeV2First, device);
            entry.Options.UsePreSharedKey = true;
            entry.Options.RequireEap = false;
            entry.Options.RequireSpap = false;
            entry.Options.RequireMSEncryptedPassword = false;
            entry.Options.RequirePap = true;
            entry.Options.RequireDataEncryption = false;
            entry.Options.RequireMachineCertificates = false;
            entry.Options.RequireWin95MSChap = true;
            entry.Options.RequireEncryptedPassword = false;
            entry.EncryptionType = RasEncryptionType.Optional;
            #endregion

            rasPhoneBook.Entries.Add(entry);
            rasPhoneBook.Entries[entry.Name].UpdateCredentials(RasPreSharedKey.Client, "vpn");

            RasEntry rasEntry = rasPhoneBook.Entries.FirstOrDefault(entry => entry.Name == connectionName);
            if (rasEntry != null)
            {
                using (RasDialer rasDialer = new RasDialer())
                {
                    rasDialer.EntryName = rasEntry.Name;
                    rasDialer.PhoneBookPath = path;
                    rasDialer.Credentials = new System.Net.NetworkCredential(username, password);

                    try
                    {
                        rasDialer.Dial();
                        MessageBox.Show("Connection established!");
                    }
                    catch (RasException ex)
                    {
                        MessageBox.Show("Failed to establish the connection: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Connection entry not found.");
            }
        }
        private void DisconnectButton_Click()
        {
            using (RasDialer rasDialer = new RasDialer())
            {
                RasConnection connection = RasConnection.GetActiveConnections().FirstOrDefault();
                if (connection != null)
                {
                    try
                    {
                        connection.HangUp();
                        MessageBox.Show("Connection disconnected!");
                    }
                    catch (RasException ex)
                    {
                        MessageBox.Show("Failed to disconnect the connection: " + ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("No active connection found.");
                }
            }
        }
        #endregion
        #region old

        class DetailDomain
        {
            public string Domain = null;
            public string Time = null;
        }
        private void SetListData()
        {
            domains = new List<DetailDomain>();
            domains.Add(new DetailDomain() { Domain = "th1-v4g.vpn4games.com" });
            domains.Add(new DetailDomain() { Domain = "th2-v4g.vpn4games.com" });
            domains.Add(new DetailDomain() { Domain = "th3-v4g.vpn4games.com" });
            domains.Add(new DetailDomain() { Domain = "th4-v4g.vpn4games.com" });
            domains.Add(new DetailDomain() { Domain = "th5-v4g.vpn4games.com" });
            domains.Add(new DetailDomain() { Domain = "th6-v4g.vpn4games.com" });
            domains.Add(new DetailDomain() { Domain = "th7-v4g.vpn4games.com" });
            domains.Add(new DetailDomain() { Domain = "th8-v4g.vpn4games.com" });
            domains.Add(new DetailDomain() { Domain = "th9-v4g.vpn4games.com" });
            domains.Add(new DetailDomain() { Domain = "th10-v4g.vpn4games.com" });
            domains.Add(new DetailDomain() { Domain = "th11-v4g.vpn4games.com" });
            domains.Add(new DetailDomain() { Domain = "th12-v4g.vpn4games.com" });
            domains.Add(new DetailDomain() { Domain = "th13-v4g.vpn4games.com" });
            domains.Add(new DetailDomain() { Domain = "th14-v4g.vpn4games.com" });
            domains.Add(new DetailDomain() { Domain = "th15-v4g.vpn4games.com" });
            domains.Add(new DetailDomain() { Domain = "th16-v4g.vpn4games.com" });
            domains.Add(new DetailDomain() { Domain = "th17-v4g.vpn4games.com" });
            domains.Add(new DetailDomain() { Domain = "th19-v4g.vpn4games.com" });
            domains.Add(new DetailDomain() { Domain = "th20-v4g.vpn4games.com" });
            domains.Add(new DetailDomain() { Domain = "th21-v4g.vpn4games.com" });
            //foreach(DetailDomain domain in domains)
            //{
            //    Debug.WriteLine("Domain: " + domain.Domain + " Time:" + (domain.Time ?? "-"));
            //}
        }
        private void CreateConfig() 
        {
            LoadIkev2System();
            ChangeRegistryConfig();
            
            foreach (DetailDomain domain in domains)
            {
                Debug.WriteLine("Domain: " + domain.Domain);
                CreateEntry(domain.Domain);
                CreateConfigTimer();
                CreateProcess();
            }
        }
        private void CreateProcess()
        {
            try
            {
                process = new Process();
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.FileName = "rasdial.exe";
                process.StartInfo.Arguments = connectionName + " " + username + " " + password;
                process.StartInfo.RedirectStandardError = true;
                //process.OutputDataReceived += Process_OutputDataReceived;
                process.Start();

                startTime = DateTime.Now;
                Debug.WriteLine("Time Start:" + startTime.ToString("HH:mm:ss.fff"));

                process.WaitForExit();

                DateTime endTime = DateTime.Now;
                Debug.WriteLine("Time Stop:" + endTime.ToString("HH:mm:ss.fff"));

                TimeSpan time = endTime - startTime;
                string result = time.ToString(@"hh\:mm\:ss\:fff");
                Debug.WriteLine("Time Result:" + result);

                //waitforexit.RunWorkerAsync();
                //StartTimer();

                RasConnection connection = RasConnection.GetActiveConnections().FirstOrDefault();
                if (connection != null)
                {
                    try
                    {
                        //MessageBox.Show("Connection Connected!");
                        Console.WriteLine("Connection Connected!");
                        DisconnectProcess();
                    }
                    catch (RasException ex)
                    {
                        Console.WriteLine("Failed to disconnect the connection: " + ex.Message);
                        //MessageBox.Show("Failed to disconnect the connection: " + ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine("No active connection found.");
                    //MessageBox.Show("No active connection found.");
                }

                // Wait for a few seconds (for demonstration purposes).
                //Console.WriteLine("wait 6 seconds!!");
                //Thread.Sleep(6000);

                //Console.WriteLine("Begin Output Read Line");
                //process.BeginOutputReadLine();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void DisconnectProcess()
        {
            try
            {
                Process processDisconnect = new Process();
                processDisconnect.StartInfo.RedirectStandardOutput = true;
                processDisconnect.StartInfo.UseShellExecute = false;
                processDisconnect.StartInfo.CreateNoWindow = true;
                processDisconnect.StartInfo.FileName = "rasdial.exe";
                processDisconnect.StartInfo.Arguments = connectionName + " /d";
                processDisconnect.Start();
                processDisconnect.WaitForExit();
                // Check the output to see if the disconnection was successful
                string output = processDisconnect.StandardOutput.ReadToEnd();
                Debug.WriteLine("Output Process Disconnect IKEv2:" + output + "\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine("output: " + e.Data);
                // You can handle/process the output here
            }
        }
        private void Process_Exited(object sender, EventArgs e)
        {
            Console.WriteLine("The process has exited.");
            // Perform additional actions when the process exits
            DateTime endTime = DateTime.Now;
            Debug.WriteLine("Time process has exited:" + endTime.ToString("HH:mm:ss.fff"));

            TimeSpan time = endTime - startTime;
            string result = time.ToString(@"hh\:mm\:ss\:fff");
            Debug.WriteLine("Time Result:" + result);

            RasConnection connection = RasConnection.GetActiveConnections().FirstOrDefault();
            if (connection != null)
            {
                try
                {
                    //MessageBox.Show("Connection Connected!");
                    Console.WriteLine("Connection Connected!");
                    DisconnectProcess();
                }
                catch (RasException ex)
                {
                    Console.WriteLine("Failed to disconnect the connection: " + ex.Message);
                    //MessageBox.Show("Failed to disconnect the connection: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("No active connection found.");
                //MessageBox.Show("No active connection found.");
            }
        }
        #region RasPhoneBook
        private void LoadIkev2System()
        {
            string path = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers);
            rasphonebook = new RasPhoneBook();
            rasphonebook.Open(path);
        }
        private void CreateEntry(string domain)
        {
            try
            {
                var oldEntry = rasphonebook.Entries.Where(x => x.Name.Contains(connectionName));
                if (oldEntry.Count() > 0)
                    rasphonebook.Entries[connectionName].Remove();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("error remove entry: " + ex.Message);
            }
            #region Set Entry
            RasDevice device = RasDevice.GetDevices().Where(x => x.Name.Contains("WAN Miniport (IKEv2)")).FirstOrDefault();
            entry = RasEntry.CreateVpnEntry(connectionName, domain, RasVpnStrategy.IkeV2First, device);
            entry.Options.UsePreSharedKey = true;
            entry.Options.RequireEap = false;
            entry.Options.RequireSpap = false;
            entry.Options.RequireMSEncryptedPassword = false;
            entry.Options.RequirePap = true;
            entry.Options.RequireDataEncryption = false;
            entry.Options.RequireMachineCertificates = false;
            entry.Options.RequireWin95MSChap = true;
            entry.Options.RequireEncryptedPassword = false;
            entry.EncryptionType = RasEncryptionType.Optional;
            #endregion
            try
            {
                var resultEntry = rasphonebook.Entries.Where(x => x.Name.Contains(connectionName));
                if (resultEntry.Count() == 0)
                    rasphonebook.Entries.Add(entry);
                rasphonebook.Entries[entry.Name].UpdateCredentials(RasPreSharedKey.Client, "vpn");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("error add entry: " + ex.Message);
            }
        }
        private void ChangeRegistryConfig()
        {
            try
            {
                using (RegistryKey Registrykey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\RasMan\\Parameters", true))
                {
                    if (Registrykey == null) return;
                    Registrykey.SetValue("NegotiateDH2048_AES256", "0", RegistryValueKind.DWord);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ChangeRegistryConfig Function Exception Found : " + ex.Message);
            }
        }
        #endregion
        #endregion
        #region Background Worker
        private void WaitForExitBackgroundWorker()
        {
            waitforexit = new BackgroundWorker();
            waitforexit.WorkerSupportsCancellation = true;
            waitforexit.DoWork += bg_waitforexit_dowork;
            waitforexit.RunWorkerCompleted += bg_waitforexit_complete;
        }
        private void bg_waitforexit_dowork(object sender, DoWorkEventArgs e)
        {
            process.WaitForExit();
        }
        private void bg_waitforexit_complete(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                var result = process.ExitCode;
                StopTimer();
                Debug.WriteLine("bg complete");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        #endregion
        #region Check Time Connection
        // Create and configure the DispatcherTimer
        private void CreateConfigTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 5);
            timer.Tick += Timer_Tick;
        }
        // Start the timer when the connection is established
        private void StartTimer()
        {
            startTime = DateTime.Now;
            timer.Start();
        }

        // Stop the timer when the connection is disconnected
        private void StopTimer()
        {
            timer.Stop();
        }

        // Timer tick event handler
        private void Timer_Tick(object sender, EventArgs e)
        {
            if(process.HasExited)
            {
                TimeSpan elapsedTime = DateTime.Now - startTime;
                string time = elapsedTime.ToString(@"hh\:mm\:ss");
                Debug.WriteLine("Connecting time: " + time);
                StopTimer();
            }
        }
        #endregion
    }
}
