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
        private readonly string domain = "test-sg.bullfreedom.xyz";
        private readonly string username = "chompoomulee";
        private readonly string password = "214965@mulee";
        public MainWindow()
        {
            InitializeComponent();
            LoadIkev2System();
            ChangeRegistryConfig();
            CreateEntry();
            CreateProcess();
        }
        [DllImport("kernel32.dll")]
        private static extern bool GenerateConsoleCtrlEvent(ConsoleCtrlEvent sigevent, int dwProcessGroupId);

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        // Enum to represent the different console control events
        private enum ConsoleCtrlEvent
        {
            CTRL_C = 0,
            CTRL_BREAK = 1,
            CTRL_CLOSE = 2,
            CTRL_LOGOFF = 5,
            CTRL_SHUTDOWN = 6
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
                process.OutputDataReceived += Process_OutputDataReceived;
                process.Start();

                // Wait for a few seconds (for demonstration purposes).
                Console.WriteLine("wait 6 seconds!!");
                Thread.Sleep(6000);

                Console.WriteLine("Begin Output Read Line");
                process.BeginOutputReadLine();

                // Check if the process has exited.
                //if (!process.HasExited)
                //{
                //    // If the process has not exited, forcefully kill it.
                //    //process.Kill();

                //    Process processCheck = new Process();
                //    processCheck.StartInfo.RedirectStandardOutput = true;
                //    processCheck.StartInfo.UseShellExecute = false;
                //    processCheck.StartInfo.CreateNoWindow = true;
                //    processCheck.StartInfo.FileName = "rasdial.exe";
                //    processCheck.StartInfo.Arguments = connectionName + " /d"; ;
                //    processCheck.StartInfo.RedirectStandardError = true;
                //    processCheck.Start();
                //    string output = processCheck.StandardOutput.ReadToEnd();
                //}
                //else
                //{

                //}

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
        #region RasPhoneBook
        private void LoadIkev2System()
        {
            string path = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers);
            rasphonebook = new RasPhoneBook();
            rasphonebook.Open(path);
        }
        private void CreateEntry()
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
    }
}
