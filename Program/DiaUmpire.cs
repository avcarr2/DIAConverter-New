using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Program
{
    public class DIAUmpire
    {
        public string PathToDIAUmpire { get; set; }
        public string DIAParametersFilePath { get; set; }
        public List<string> FilesList { get; set; }
        public DIAUmpire(string pathToDIAUmpire, string pathToDIAUmpireParametersFile)
        {
            PathToDIAUmpire = pathToDIAUmpire;
            DIAParametersFilePath = pathToDIAUmpireParametersFile;
        }

        /// <summary>
        /// Creates the DIA process. Only the file name changes between runs, so the goal is to use this function to generate the 
        ///  process and only vary the name of the file being processed. 
        /// </summary>
        /// <param name="pathToDIAUmpire"></param>
        /// <param name="pathToParametersFile"></param>
        /// <param name="pathToOutput"></param>
        /// <returns></returns>
        public ProcessStartInfo CreateDIAProcess(string pathToDIAUmpire, string pathToParametersFile,
            string filePath, string pathToOutput)
        {
            // java -Xmx16g -jar DIA_Umpire_se.jar [path to data] [parameter file]
            ProcessStartInfo startInfo = new();
            startInfo.FileName = "java";
            startInfo.Arguments = "-jar" + "\"" + pathToDIAUmpire + "\"" +
                " \"" + filePath + "\" " + "\"" + pathToParametersFile + "\"";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.WorkingDirectory = Path.GetDirectoryName(pathToOutput);
            return startInfo;
        }
        public void RunDIAUmpireOnAllFiles(string pathToDiaTempOutput, string pathToParametersFile,
            string pathToDIAOutput)
        {
            string[] mzXMLFiles = Directory.GetFiles(pathToDiaTempOutput);
            foreach (string mzXMLFile in mzXMLFiles)
            {
                string tempFolderPath = Path.Combine(Path.GetDirectoryName(mzXMLFile), Path.GetFileNameWithoutExtension(mzXMLFile));
                Directory.CreateDirectory(tempFolderPath);
                string tempFilePathThatHasBeenMoved = Path.Combine(tempFolderPath, Path.GetFileName(mzXMLFile));
                File.Move(mzXMLFile, tempFilePathThatHasBeenMoved);
                ProcessStartInfo startInfo = CreateDIAProcess(PathToDIAUmpire, pathToParametersFile,
                    tempFilePathThatHasBeenMoved, tempFolderPath);
                ExecuteDIAUmpire(startInfo);
                MoveMGFFilesToMGFDirectory(tempFilePathThatHasBeenMoved, pathToDIAOutput);
            }
        }
        // probably also do the rename in this step as well. 
        public void MoveMGFFilesToMGFDirectory(string pathToInitialDirectory, string pathToFinalDirectory)
        {
            string extension = ".mgf";
            string[] files = Directory.GetFiles(pathToInitialDirectory, extension);
            for (int i = 0; i < files.Length; i++)
            {
                string initial = Path.Combine(pathToInitialDirectory, files[i]);
                string final = Path.Combine(pathToFinalDirectory, files[i]);
                File.Move(initial, final);
                CleanUpTempDirectory(pathToInitialDirectory);
            }
        }
        public void CleanUpTempDirectory(string tempDirectoryPath)
        {
            Directory.Delete(tempDirectoryPath, true);
        }
        public void ExecuteDIAUmpire(ProcessStartInfo startInfo)
        {
            using (Process proc = new())
            {
                proc.StartInfo = startInfo;
                proc.Start();
                bool exitStatus = proc.WaitForExit(10800000);
                if (!exitStatus)
                {
                    Console.WriteLine("DIAUmpire exited early.");
                    proc.Kill();
                }
                proc.Close();
            }
        }
    }
}
