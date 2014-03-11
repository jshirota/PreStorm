using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using PreStorm.Tool.Properties;

namespace PreStorm.Tool
{
    public partial class Form1 : Form
    {
        private string _url;
        private string _folder;
        private string _projectName;
        private ICredentials _credentials;
        private Layer[] _layers;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtFolder.Text = !Directory.Exists(Settings.Default.Folder)
                ? Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                : Settings.Default.Folder;

            txtUrl.Text = Settings.Default.Url;
            txtUrl_TextChanged(null, null);

            chkWindowsCredentials.Checked = Settings.Default.UseWindowsCredentials;
        }

        private void txtUrl_TextChanged(object sender, EventArgs e)
        {
            var projectName = Regex.Match(txtUrl.Text, @"(?<=(/))\w+(?=(/(MapServer|FeatureServer)))", RegexOptions.IgnoreCase).Value;

            if (projectName == "")
                return;

            txtProjectName.Text = projectName.ToSafeName(false, true, n => !Directory.Exists(txtFolder.Text + "\\" + n));
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            _url = txtUrl.Text;
            _folder = txtFolder.Text;
            _projectName = txtProjectName.Text;
            _credentials = chkWindowsCredentials.Checked ? CredentialCache.DefaultCredentials : null;

            if (!Regex.IsMatch(_url, @"https?://.*?/(MapServer|FeatureServer)/?$", RegexOptions.IgnoreCase))
            {
                MessageBox.Show(_url + " is not a valid map service url.", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (!Directory.Exists(_folder))
            {
                MessageBox.Show(_folder + " does not exist.", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            try
            {
                var serviceInfo = Http.Download(_url + "?f=json", _credentials).Deserialize<ServiceInfo>();

                var error = serviceInfo.error;

                if (error != null)
                    throw new Exception(error.message);

                if (serviceInfo.currentVersion < 10)
                {
                    MessageBox.Show("Versions prior to 10.0 are not supported.", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                if (serviceInfo.capabilities != null && !serviceInfo.capabilities.Contains("Query"))
                {
                    MessageBox.Show("This service does not support querying.", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                _layers = serviceInfo.layers.Concat(serviceInfo.tables).Where(l => l.subLayerIds == null).ToArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            lblStatus.Text = "Downloading...";

            txtUrl.Enabled = txtFolder.Enabled = txtProjectName.Enabled = chkWindowsCredentials.Enabled = btnGenerate.Enabled = false;

            worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var solution = new Solution(_folder, _projectName);

            solution.CreateFolder("BusinessObjects");
            solution.CreateFolder("Library");
            solution.CreateFolder("Properties");

            var assemblyInfo = @"using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyVersionAttribute(""1.0.0.0"")]
[assembly: ComVisibleAttribute(false)]
";

            solution.WriteCompileFile("Properties\\AssemblyInfo.cs", assemblyInfo);

            solution.AddDoNotCopyFile(@"Library\PreStorm.dll", Resources.PreStorm_dll);
            solution.AddDoNotCopyFile(@"Library\PreStorm.xml", Resources.PreStorm_xml);

            var stringBuilder = new StringBuilder();
            var counter = 0;

            var isMapServer = Regex.IsMatch(_url, @"/MapServer(/|$)", RegexOptions.IgnoreCase);

            var classNames = new List<string>();

            var useLayerId = _layers.GroupBy(l => l.name).Select(g => g.Count()).Max() > 1;

            foreach (var l in _layers)
            {
                counter++;

                var layerInfo = Http.Download(_url + "/" + l.id + "?f=json", _credentials).Deserialize<LayerInfo>();

                if (layerInfo.type != "Feature Layer" && layerInfo.type != "Table")
                    continue;

                var layerName = layerInfo.name;

                worker.ReportProgress(100 * counter / _layers.Length, "Processing " + layerName + "...");

                var variableName = layerName.ToSafeName(true, false);
                var className = layerName.ToSafeName(true, true, n => !classNames.Contains(n.ToLower()));
                classNames.Add(className.ToLower());

                var codeGenerator = new CodeGenerator(layerInfo.geometryType, layerInfo.fields, _projectName, className, isMapServer);

                solution.WriteCompileFile(@"BusinessObjects\" + className + ".cs", codeGenerator.ToCSharp());

                var snippet = @"
            foreach (var {1} in service.Download<{2}>({0}))
            {{
                Console.WriteLine({1}.{3});
            }}
";

                var displayField = layerInfo.displayField != null && layerInfo.fields.Any(f => f.name == layerInfo.displayField)
                    ? layerInfo.displayField.ToSafeName(false)
                    : "OID";

                if (useLayerId)
                    stringBuilder.AppendFormat(snippet, layerInfo.id, variableName, className, displayField);
                else
                    stringBuilder.AppendFormat(snippet, @"""" + layerName + @"""", variableName, className, displayField);
            }

            solution.WriteCompileFile("Program.cs", Resources.Program.Inject(_projectName, _url, stringBuilder, _credentials == null ? "" : ", CredentialCache.DefaultCredentials"));

            solution.FinalizeSolution("4.5");
        }

        private void worker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            lblStatus.Text = e.UserState as string;
            progressBar.Value = e.ProgressPercentage;
        }

        private void worker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Process.Start(string.Format(@"{0}\{1}\", txtFolder.Text, txtProjectName.Text));

            Settings.Default.Folder = _folder;
            Settings.Default.Url = _url;
            Settings.Default.UseWindowsCredentials = _credentials != null;
            Settings.Default.Save();

            Close();
        }
    }
}
