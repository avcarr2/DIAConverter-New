using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO; 

namespace Program
{
    public class MsConvert
    {
        public string PathToMsConvert { get; set; }
        public string PathToOutputDirectory { get; set; }
        public string BinaryEncodingPrecision { get; set; }
        public string ZlibCompression { get; set; }
        public string TitleMakerFilter { get; set; }
        public Dictionary<string, string> MsConvertParameters { get; set; }
        public MsConvert(string path)
        {
            PathToMsConvert = path;
        }
        public MsConvert()
        {

        }

        // MsConvert needs: 
        /*
         * 1) output directory (-o)
         * 2) text files of a list of the filenames to convert. 
         * 3) filters 
         *      1. Peak Picking: peakPicking vendor msLevel=1-
         *      2. titleMaker: <RunId>.<ScanNumber>.<ScanNumber>.<ChargeState> File:"<SourcePath>".NativeID:"<Id>"
         * 4) command: set extension -e mzXML or mzML
         * 5) command: to convert --mzML or --mzXML 
         * 6) Binary encoding precision: 64-bit. --64
         * 7) Write index (doesn't need command) 
         * 8) zlib compression -z
         * 9) TPP compatibility (Idk what command this is) 
         */


        /// <summary>
        /// Uses MsConvert to convert all .raw files to mzml files. Output directory
        /// should be a the ConvertedData folder in output. 
        /// </summary>
        /// <param name="pathToData"></param>
        public ProcessStartInfo ConvertRawToMzML(string fileToConvert, string pathToOutputDirectory, 
            string pathToMsConvert)
        {
            ProcessStartInfo startInfo = new();
            startInfo.FileName = Path.Combine(PathToMsConvert, "msconvert.exe");
            startInfo.Arguments = "\"" + fileToConvert + "\"" +
                " --filter peakPicking \"true 1-\"" +
                " --mzML " +
                "--outdir " + "\"" + pathToOutputDirectory + "\""; 
            startInfo.WorkingDirectory = pathToMsConvert;
            return startInfo;
        }
        /// <summary>
        /// Converts to mzXML format for DIAUmpire processing. Puts folders in a temporary folder under DIAUmpire
        /// </summary>
        /// <param name="fileNamesFilePath"></param>
        public ProcessStartInfo ConvertRawToMzXml(string fileToConvert, string outputPath, 
            string pathToMsConvert)
        {
            ProcessStartInfo startInfo = new();
            startInfo.FileName = Path.Combine(pathToMsConvert,"msconvert.exe");
            startInfo.Arguments = "\"" + fileToConvert + "\"" +
                " --filter peakPicking \"true 1-\"" +
                " --mzXML" +
                " --outdir " + "\"" + outputPath + "\"";  
            startInfo.WorkingDirectory=PathToMsConvert;
            //startInfo.RedirectStandardOutput = true; 
            return startInfo;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <param name="pathToData"></param><summary> original data. not sure if I need this. </summary>
        /// <param name="pathToOutput"></param> <summary>Path to the "convertedData" folder in the output folder. </summary>
        public void ConvertFiles(string[] files, string pathToOutput,
            string pathToDIAUmpireOutput, string pathToConvertedDataOutput)
        {
            for(int i = 0; i < files.Length; i++)
            {
                var mzMLConversion = ConvertRawToMzML(files[i], pathToConvertedDataOutput, PathToMsConvert);
                var mzXMLConversion = ConvertRawToMzXml(files[i], pathToDIAUmpireOutput, PathToMsConvert);
                ExecuteMsConvertProcess(mzMLConversion);
                //ExecuteMsConvertProcess(mzXMLConversion);
            }            
        }
        public void ExecuteMsConvertProcess(ProcessStartInfo startInfo)
        {
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            using (Process proc = new Process())
            {
                proc.StartInfo = startInfo;
                proc.Start();
                proc.WaitForExit(); 
            }
        }
        public string WriteFileNamesTextFile(string[] filenames, string path)
        {
            string filenamesFilePath = Path.Combine(path, "FilenamesFile.txt");
            using (StreamWriter filenamesFile = new StreamWriter(filenamesFilePath))
            {
                foreach (string filename in filenames)
                {
                    filenamesFile.WriteLine(filename);
                }
            }
            return filenamesFilePath;
        }
    }
}
