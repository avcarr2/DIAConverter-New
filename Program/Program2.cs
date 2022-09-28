using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using MassSpectrometry;
using IO.Mgf;
using IO.MzML; 

namespace Program
{
    public class Program2
    {
        public string PathToOutputFolder { get; set; }
        public string PathToMsConvert { get; set; }
        public string PathToDIAUmpire { get; set; }
        public string PathToDIAUmpireOutput { get; set; }
        public string PathToDIAUmpireTempFolder { get; set; }
        public string PathToOriginalData { get; set; }
        public string PathToCombinedFiles { get; set; }
        public string PathToMzMLConvertedFiles { get; set; }    
        public string PathToDIAUmpireParametersFile { get; set; }

        public void Main(string[] args)
        {
            // creates the directories in the output folder. 
            FileHandling.CreateOutputFolder(args[1], out string convertedDataPath, out string diaUmpireOutput, 
                out string diaUmpireTempFiles, out string combinedFiles);
            PathToOutputFolder = args[1]; 
            PathToCombinedFiles = combinedFiles; 
            PathToMzMLConvertedFiles = convertedDataPath; 
            PathToDIAUmpireOutput = diaUmpireOutput;
            PathToDIAUmpireTempFolder = diaUmpireTempFiles; 
            PathToDIAUmpireParametersFile = args[2]; 
            //PathToDIAUmpireTempFolder =
            //
            //diaUmpireTempFiles; 

            // 1) convert .raw files to .mzxml and .mzml using msconvert.
            // Ashley: Change DIA Umpire vs 2.0 to 2.2.2. Make time out 3 hours
            PathToMsConvert = Path.Combine(@"D:\Program Files\ProteoWizard");
            PathToDIAUmpire = Path.Combine(@"D:\DIA-Umpire_v2_0\DIA_Umpire_SE.jar");  
           
            // just a test process
            // test start infor 
            //ProcessStartInfo testInfo = new ProcessStartInfo();
            //testInfo.FileName = "msconvert.exe";
            //testInfo.Arguments = "--help"; 
            //testInfo.UseShellExecute = true;
            //testInfo.WorkingDirectory = @"D:\Program Files\ProteoWizard"; 
            //Process.Start(testInfo);

            PathToOriginalData = args[0]; 
            // get all the files with .raw extension 
            string[] fileNames = FileHandling.GetAllThermoRawFilesFromDir(PathToOriginalData);
            
            MsConvert msConv = new(PathToMsConvert);
            msConv.ConvertFiles(fileNames, PathToOutputFolder, 
                PathToDIAUmpireTempFolder, PathToMzMLConvertedFiles);

            // 2) Run DIAUmpire with .mzxml files using a configuration file. Output is .mgf file with postfix of _Q1, _Q2, _Q3. 
            DIAUmpire diaUmpire = new(PathToDIAUmpire, PathToDIAUmpireParametersFile);
            diaUmpire.RunDIAUmpireOnAllFiles(PathToDIAUmpireTempFolder, 
                PathToDIAUmpireParametersFile, PathToDIAUmpireOutput);

            // 3) Combine the files. 
            CombineFiles();

            Console.WriteLine("File combination completed");    
            // Still need to test the whole thing 
        }
        /// <summary>
        /// Return value needs to be Dictionary with list because there is no guarantee or way to determine what the 
        /// length of the string[] array made from the .mgf files will be. 
        /// </summary>
        /// <param name="mzMLFile"></param>
        /// <param name="mgfDirectoryPath"></param>
        /// <param name="experimentIndicator"></param>
        /// <returns></returns>
        public Dictionary<string, List<string>> MatchMzMLAndMGFFiles(string mzmlFolderPath, string mgfDirectoryPath, 
            string experimentIndicator = "Exp")
        {
            Dictionary<string, List<string>> valuesDict = new();
            // get the mzml file names from the folder path. 
            string[] mzmlFiles = Directory.GetFiles(mzmlFolderPath, ".mzML");
            // get the mgf file names from their folder. 
            string[] mgfFiles = Directory.GetFiles(mgfDirectoryPath, ".mgf");
            // populate the dictionary based on the mzml file names and the experimentIndicator
            // regex match experimentIndicator and all characters after it until the (negative look-ahead) period.  
            Regex experimentRegex = new Regex(@""+ experimentIndicator + @".*(?=_Q)"); 
            foreach(string mzmlFile in mzmlFiles)
            {
                // get the exp number using the regex. 
                var matchGroup = experimentRegex.Match(mzmlFile);
                string experimentNumber = matchGroup.Groups[0].Value;
                // experimentNumber is now the new string to match against all the mgf files. 
                List<string> matchingMgfs = mgfFiles
                    .Where(x => Regex.IsMatch(x, experimentNumber, RegexOptions.IgnoreCase) == true)
                    .ToList();
                valuesDict[mzmlFile] = matchingMgfs; 
            }
            return valuesDict; 
        }
        public void CombineFiles()
        {
            var matchedOutputs = MatchMzMLAndMGFFiles(PathToMzMLConvertedFiles, PathToDIAUmpireOutput);
            // matchedOutput is key value pair
            int fileCount = 1;
            int totalFiles = matchedOutputs.Count; 
            foreach(var matchedOutput in matchedOutputs)
            {

                for(int i = 0; i < matchedOutput.Value.Count; i++)
                {
                    // Ashley doesn't want to use Q3, so just break after the first 2. 
                    // but may need to order by the string just to make sure that it's in 
                    // some semblance of order. 
                    if (i == 2) break;
                    try
                    {
                        Program1.DoFileProcessing(matchedOutput.Key, matchedOutput.Value[i], PathToCombinedFiles); 
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error: " + e.Message + matchedOutput.Value[i]); 
                    }
                }
                Console.WriteLine("File {0} of {1} completed.", fileCount, totalFiles); 
            }
        }
    }
    public class ProgramCombineFiles
    {
        public string PathToOutputFolder { get; set; }
        public string PathToMsConvert { get; set; }
        public string PathToDIAUmpire { get; set; }
        public string PathToDIAUmpireOutput { get; set; }
        public string PathToDIAUmpireTempFolder { get; set; }
        public string PathToOriginalData { get; set; }
        public string PathToCombinedFiles { get; set; }
        public string PathToMzMLConvertedFiles { get; set; }
        public string PathToDIAUmpireParametersFile { get; set; }
        public void Main(string[] args)
        {
            // args[0] path where the original data can be found. 
            // args[1] path to the DIAUmpireOutput
            // args[2] path to output 
            FileHandling.CreateOutputFolder(args[2], out string convertedDataPath, out string diaUmpireOutput,
                out string diaUmpireTempFiles, out string combinedFiles);
            PathToOutputFolder = args[2];
            PathToCombinedFiles = combinedFiles;
            PathToMzMLConvertedFiles = args[0]; 
            PathToDIAUmpireTempFolder = diaUmpireTempFiles;
            PathToOriginalData = args[0];
            PathToDIAUmpireOutput = args[1]; 

            //string[] fileNames = FileHandling.GetAllThermoRawFilesFromDir(PathToOriginalData);
            //MsConvert msConv = new(PathToMsConvert);
            //msConv.ConvertFiles(fileNames, PathToOutputFolder,
            //    PathToDIAUmpireTempFolder, PathToMzMLConvertedFiles);
            //GrabAllMgfFiles(PathToDIAUmpireOutput, PathToCombinedFiles); 
            CombineFiles(); 
        }
        public void CombineFiles()
        {
            
           var matchedOutputs = MatchMzMLAndMGFFiles(PathToMzMLConvertedFiles, PathToDIAUmpireOutput);
            // matchedOutput is key value pair
            int fileCount = 1;
            int totalFiles = matchedOutputs.Count;
              
            if(totalFiles == 1)
            {

                int numberElements = matchedOutputs.Values.First().Count;
                string firstKey = matchedOutputs.Keys.First();
                string[] mgfFiles = matchedOutputs.Values.First().ToArray(); 
                for (int i = 0; i < numberElements; i++)
                {
                    try
                    {
                        Program1.DoFileProcessing(firstKey, mgfFiles[i], PathToOutputFolder); 
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Error: " + e.Message + mgfFiles[i]);
                    }
                }
            }else if(totalFiles > 1)
            {
                foreach (var matchedOutput in matchedOutputs)
                {

                    for (int i = 0; i < matchedOutput.Value.Count; i++)
                    {
                        // Ashley doesn't want to use Q3, so just break after the first 2. 
                        // but may need to order by the string just to make sure that it's in 
                        // some semblance of order. 
                        if (i == 2) break;
                        try
                        {
                            Program1.DoFileProcessing(matchedOutput.Key, matchedOutput.Value[i], PathToCombinedFiles);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error: " + e.Message + matchedOutput.Value[i]);
                        }
                    }
   
                    fileCount++;
                }
            }

            
        }
        public Dictionary<string, List<string>> MatchMzMLAndMGFFiles(string mzmlFolderPath, string mgfDirectoryPath,
            string experimentIndicator = "")
        {
            Dictionary<string, List<string>> valuesDict = new();
            // get the mzml file names from the folder path. 
            string[] mzmlFiles = Directory.GetFiles(mzmlFolderPath, "*mzML")
                .Select(i => Path.GetFileName(i))
                .ToArray();
            // get the mgf file names from their folder. 
            string[] mgfFiles = Directory.GetFiles(mgfDirectoryPath, "*mgf")
                .Select(i => Path.GetFileName(i))
                .ToArray();
            
            // adding support for the specific use case defined by ashley for a 
            // single file in the mzMl file folder

            
            // populate the dictionary based on the mzml file names and the experimentIndicator
            // regex match experimentIndicator and all characters after it until the (negative look-ahead) period.  
            Regex experimentRegex = new Regex(@"" + experimentIndicator + @".*(?=\.)");
            foreach (string mzmlFile in mzmlFiles)
            {
                // get the exp number using the regex. 
                var matchGroup = experimentRegex.Match(mzmlFile);
                string experimentNumber = matchGroup.Groups[0].Value;
                // experimentNumber is now the new string to match against all the mgf files. 
                List<string> matchingMgfs = mgfFiles
                    .Where(x => Regex.IsMatch(x, experimentNumber, RegexOptions.IgnoreCase) == true)
                    .Select(i => Path.Combine(mgfDirectoryPath, i))
                    .ToList();
                
                valuesDict[Path.Combine(mzmlFolderPath, mzmlFile)] = matchingMgfs;
            }
                       
            return valuesDict;
        }
    }
}
