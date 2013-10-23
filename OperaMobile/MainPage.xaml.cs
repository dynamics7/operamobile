using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using Microsoft.Phone.Controls;

namespace OperaMobile
{
    public partial class MainPage : PhoneApplicationPage
    {

        internal const string OperaFolder = "\\Program Files\\Opera Mobile 10";
        internal const string OperaClass = "OM10-ML-MainWndClass";

        public MainViewModel viewModel
        {
            get
            {
                return this.DataContext as MainViewModel;
            }
        }


        System.Windows.Threading.DispatcherTimer dt = new System.Windows.Threading.DispatcherTimer();

        public MainPage()
        {
            
            if (IsolatedStorageSettings.ApplicationSettings.Contains("SecondBoot") == false)
            {
                MessageBox.Show(LocalizedResources.OperaProperty, LocalizedResources.Warning, MessageBoxButton.OK);
                IsolatedStorageSettings.ApplicationSettings["SecondBoot"] = 1;
            }
            InitializeComponent();
            InteropSvc.InteropLib.Initialize();
            if (InteropSvc.InteropLib.HasRootAccess())
            {
                viewModel.IsRunning = true;
                uint hWnd = InteropSvc.InteropLib.Instance.FindWindow7(OperaClass, null);
                if (hWnd != 0)
                {
                    Opera_Run();
                    App.Quit();
                    return;
                }
            }
            else
            {
                MessageBox.Show(LocalizedResources.NoRootAccess, "Opera Mobile", MessageBoxButton.OK);
                App.Quit();
            }
        }

        private void StartStopTimer(bool start, int timeout = 3)
        {
            if (start)
            {
                viewModel.IsRunning = true;
                counter = timeout;
                dt.Interval = new TimeSpan(0, 0, 0, 1);
                dt.Tick -= dt_Tick;
                dt.Tick += new EventHandler(dt_Tick);
                dt_Tick(this, new EventArgs());
                dt.Start();

            }
            else
            {
                dt.Stop();
                viewModel.IsRunning = false;
            }
        }
        private class EnumFile
        {
            public string FileName;
            public bool isFolder;
        }

        EnumFile[] EnumerateFiles(string folder)
        {
        
            InteropSvc.InteropLib.WIN32_FIND_DATA data;
            uint handle = InteropSvc.InteropLib.Instance.FindFirstFile7(folder + "\\*", out data);
            var list = new List<EnumFile>();

            if (handle != 0xFFFFFFFF)
            {
                bool result = false;
                do
                {
                    if (data.cFileName != "." && data.cFileName != "..")
                    {
                        EnumFile ef = new EnumFile();
                        ef.FileName = data.cFileName;
                        bool t = ((data.dwFileAttributes & 0x10) == 0x10) ? true : false;
                        ef.isFolder = t ? true : false;
                        list.Add(ef);
                    }
                    result = InteropSvc.InteropLib.Instance.FindNextFile7(handle, out data);
                } while (result != false);
                InteropSvc.InteropLib.Instance.FindClose7(handle);
            }
            return list.ToArray();
        }

        internal static bool Opera_StuffCopied = false;

        void CopyDirectory(string src, string dest)
        {
            EnumFile[] files = EnumerateFiles(src);
            InteropSvc.InteropLib.Instance.CreateDirectory7(dest);
            if (files.Count() > 0)
            {
                foreach (EnumFile ef in files)
                {
                    if (ef.isFolder == true)
                        CopyDirectory(src + "\\" + ef.FileName, dest + "\\" + ef.FileName);
                    else
                        InteropSvc.InteropLib.Instance.CopyFile7(src + "\\" + ef.FileName, dest + "\\" + ef.FileName, true);

                }
            }
        }

        internal static void DeleteFilesInWindows()
        {
            InteropSvc.InteropLib.Instance.DeleteFile7("\\Windows\\omx32.dll");
            InteropSvc.InteropLib.Instance.DeleteFile7("\\Windows\\omxctr.dll");
            InteropSvc.InteropLib.Instance.DeleteFile7("\\Windows\\omxshell.dll");
            InteropSvc.InteropLib.Instance.DeleteFile7("\\Windows\\omxccore.dll");
            InteropSvc.InteropLib.Instance.DeleteFile7("\\Windows\\ommapi.dll");
            InteropSvc.InteropLib.Instance.DeleteFile7("\\Windows\\omxcdll.dll");
        }
        void Opera_Prepare()
        {
            InteropSvc.InteropLib.Instance.CreateDirectory7(OperaFolder);

            //DeleteFilesInWindows();

            //InteropSvc.InteropLib.Instance.DeleteFile7(OperaFolder + "\\Opera10-armv4i.exe");
            //InteropSvc.InteropLib.Instance.DeleteFile7(OperaFolder + "\\opera-wince5-armv4i.dll");

            // Copying hook and extra libraries to \Windows\
            CopyDirectory("\\Applications\\Install\\8f3f9a55-f40f-48e5-93b6-0465bf3a23bf\\Install\\WindowsFolder", "\\Windows");

            // Copying Opera to \Program Files\Opera Mini
            CopyDirectory("\\Applications\\Install\\8f3f9a55-f40f-48e5-93b6-0465bf3a23bf\\Install\\Opera", OperaFolder);


            Opera_StuffCopied = true;
        }

        void Opera_Run()
        {
            if (Opera_StuffCopied == true)
            {
             
                UInt32 handle = 0;

                bool result = InteropSvc.InteropLib.Instance.CreateProcess(OperaFolder + "\\Opera10-armv4i.exe", "", "standard", out handle);
               
                if (result)
                {
                    InteropSvc.InteropLib.Instance.CloseHandle7(handle);
                    System.Threading.Thread.Sleep(3000);
                }
                else
                {
                    AddToLog("");

                }
                App.Quit();
            }
            else
            {

                Opera_Prepare();
                if (Opera_StuffCopied == true)
                    Opera_Run();
                else
                    AddToLog("Opera isn't ready. Please, restart an application");
            }
        }


        void AddToLog(string text)
        {
            MessageBox.Show(text, "Opera Mobile", MessageBoxButton.OK);
        }


        int counter;

        void dt_Tick(object sender, EventArgs e)
        {
            if (counter == 0)
            {
                Opera_Run();
                StartStopTimer(false);
                return;
            }
            UpdateUi();
            counter--;
        }

        void UpdateUi()
        {
            txtCounter.Text = counter.ToString() + "...";
        }

        static bool loadedOnce = false;
        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (loadedOnce == false)
            {
                loadedOnce = true;
                
                if (InteropSvc.InteropLib.Instance != null)
                {
                    try
                    {
                        var thread = new System.Threading.Thread(Opera_Prepare);
                        thread.Start();
                    }
                    catch (Exception ex)
                    {
                        AddToLog("A problem occured while copying Opera files. Check your unlock state.");
                        StartStopTimer(false);
                        return;
                    }
                    StartStopTimer(true);
                }
                else
                {
                    AddToLog("A problem occured while instanciating native code. Check your unlock state.");
                    StartStopTimer(false);
                    return;
                }
            }
        }

        private void btnRunIt_Click(object sender, RoutedEventArgs e)
        {
            Opera_Run();
        }

        internal static void Cleanup()
        {
            InteropSvc.InteropLib.Instance.DeleteDirectory7(OperaFolder);
            DeleteFilesInWindows();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.IsRunning == true)
            {
                StartStopTimer(false);
                viewModel.IsRunning = false;
            }
            else
            {
                StartStopTimer(true, 0);
                viewModel.IsRunning = true;
            }
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            //pageAbout ab = new pageAbout();
            //NavigationService.Navigate(new Uri("/pageAbout.xaml", UriKind.Relative));
        }

        private void btnOptions_Click(object sender, RoutedEventArgs e)
        {
            StartStopTimer(false);
        }

    }

}