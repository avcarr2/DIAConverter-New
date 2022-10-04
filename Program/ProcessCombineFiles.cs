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
                        Program.DoFileProcessing(firstKey, mgfFiles[i], PathToOutputFolder); 
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
                            Program.DoFileProcessing(matchedOutput.Key, matchedOutput.Value[i], PathToCombinedFiles);
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
            if (mgfFiles.Length == 0)
            {
                throw new ArgumentException(( "No associated mgf files found for mzml files."));
            } 
            
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
