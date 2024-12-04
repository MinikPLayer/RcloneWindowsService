using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace RcloneWindowsService
{
    public partial class RcloneService : ServiceBase
    {
        Process rcloneProcess = null;
        string serviceName = "";

        public RcloneService()
        {
            InitializeComponent();

            this.ServiceName = "Rclone Windows Service";
        }

        protected string GetServiceName()
        {
            int processId = Process.GetCurrentProcess().Id;
            var query = "SELECT * FROM Win32_Service where ProcessId = " + processId;
            System.Management.ManagementObjectSearcher searcher =
                new System.Management.ManagementObjectSearcher(query);

            foreach (System.Management.ManagementObject queryObj in searcher.Get())
            {
                return queryObj["Name"].ToString();
            }

            throw new Exception("Can not get the ServiceName");
        }

        private string GetDisplayName()
        {
            ServiceController sc = new ServiceController(GetServiceName());
            var displayName = sc.DisplayName;
            return displayName;
        }

        void Log(string log, EventLogEntryType type = EventLogEntryType.Information, int id = 101, short category = 1)
        {
            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = serviceName;
                eventLog.WriteEntry(log, type, id, category);
            }
        }

        private string CreateEventSource()
        {
            string eventSource = serviceName;
            bool sourceExists;
            try
            {
                // searching the source throws a security exception ONLY if not exists!
                sourceExists = EventLog.SourceExists(eventSource);
                if (!sourceExists)
                {   // no exception until yet means the user as admin privilege
                    EventLog.CreateEventSource(eventSource, "Application");
                }
            }
            catch (SecurityException)
            {
                eventSource = "Application";
            }

            return eventSource;
        }

        protected override void OnStart(string[] args)
        {
            serviceName = GetServiceName();
            CreateEventSource();

            var programArgs = Environment.GetCommandLineArgs().ToList();
            // Remove the first argument which is the path to the executable
            programArgs.RemoveAt(0);

            var now = DateTime.Now;
            var argsString = "";
            foreach (var arg in programArgs)
            {
                argsString += $"\"{arg}\" ";
            }
            var command = "rclone";
            var commandArgs = $"mount {argsString}";

            rcloneProcess = new Process();
            rcloneProcess.StartInfo.FileName = command;
            rcloneProcess.StartInfo.Arguments = commandArgs;
            rcloneProcess.StartInfo.UseShellExecute = false;
            rcloneProcess.StartInfo.RedirectStandardOutput = true;
            rcloneProcess.StartInfo.RedirectStandardError = true;
            rcloneProcess.Exited += RcloneProcess_Exited;
            rcloneProcess.OutputDataReceived += RcloneProcess_OutputDataReceived;
            rcloneProcess.ErrorDataReceived += RcloneProcess_ErrorDataReceived;
            rcloneProcess.EnableRaisingEvents = true;
            rcloneProcess.Start();
            rcloneProcess.BeginErrorReadLine();
            rcloneProcess.BeginOutputReadLine();

            Log($"Rclone service started. \nFull command: {command} {commandArgs}");
        }

        private void RcloneProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Log("Rclone process output:\n" + e.Data, EventLogEntryType.Error, 104, 2);
        }

        private void RcloneProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Log("Rclone process output:\n" + e.Data, EventLogEntryType.Information, 103, 2);
        }

        private void RcloneProcess_Exited(object sender, EventArgs e)
        {
            Log("Rclone process exited unexpectedly.", EventLogEntryType.Error, 102);
            rcloneProcess = null;
            this.Stop();
        }

        protected override void OnStop()
        {
            if (rcloneProcess == null)
                return;

            if(rcloneProcess.HasExited)
            {
                Log("Rclone process has already exited.");
                return;
            }

            rcloneProcess.Kill();
            Log("Rclone service stopped.");
        }
    }
}
