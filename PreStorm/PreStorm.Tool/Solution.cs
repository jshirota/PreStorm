using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PreStorm.Tool.Properties;

namespace PreStorm.Tool
{
    internal class Solution
    {
        private readonly string _guid = "{" + Guid.NewGuid().ToString().ToUpper() + "}";
        private readonly string _projectName;
        private readonly string _solutionFolder;
        private readonly string _projectFolder;
        private readonly List<string> _compileFiles = new List<string>();
        private readonly List<string> _doNotCopyFiles = new List<string>();

        public Solution(string folder, string projectName)
        {
            _projectName = projectName;
            _solutionFolder = folder + "\\" + projectName;
            _projectFolder = _solutionFolder + "\\" + projectName;
        }

        public void CreateFolder(string folderName)
        {
            Directory.CreateDirectory(_projectFolder + "\\" + folderName);
        }

        public void WriteCompileFile(string fileName, string content)
        {
            File.WriteAllText(_projectFolder + "\\" + fileName, content);
            _compileFiles.Add(fileName);
        }

        public void AddDoNotCopyFile(string fileName, byte[] data)
        {
            File.WriteAllBytes(_projectFolder + "\\" + fileName, data);
            _doNotCopyFiles.Add(fileName);
        }

        public void FinalizeSolution(string frameworkVersion)
        {
            Func<List<string>, string, string> create = (files, format) => string.Join("\r\n", files.Select(f => "    " + string.Format(format, f)).ToArray());

            var compileFiles = create(_compileFiles, "<Compile Include=\"{0}\" />");
            var doNotCopyFiles = create(_doNotCopyFiles, "<None Include=\"{0}\" />");

            var extension = ".csproj";
            var guid = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";

            var project = Resources.csproj.Inject(_guid, _projectName, compileFiles, doNotCopyFiles, frameworkVersion);
            File.WriteAllText(_projectFolder + "\\" + _projectName + extension, project);

            var solution = Resources.sln.Inject(_projectName, _guid, extension, guid);
            File.WriteAllText(_projectFolder + ".sln", solution);
        }
    }
}
