using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MassSpectrometry;
using Proteomics;
using MzLibUtil;
using IO.Mgf;
using IO.MzML;

namespace Program
{
	public class Program
	{
		// args[0] path where the original mzml files can be found. 
		// args[1] path to the mgf files
		// args[2] path to output 
		// "scan number only nativeID format"
		static void Main(string[] args)
		{
            ProgramCombineFiles proc = new(); 
			proc.Main(args.ToArray()); 
        }
        public static void DoFileProcessing(string filePathMZML, string filePathMgf, string outfolderPath)
		{
			// load mzmls
			Mzml mzml = Mzml.LoadAllStaticData(filePathMZML);
            SourceFile sf = mzml.SourceFile;
            List<MsDataScan> ms1Scans = mzml.GetAllScansList(); 
			
            // load mgfs
			Mgf mgf = Mgf.LoadAllStaticData(filePathMgf);
            SourceFile mgfSf = mgf.SourceFile; 
            List<MsDataScan> ms2Scans = mgf.GetAllScansList();

            if (ms2Scans.Count < 1)
			{
				// write exception 
			}

			DIAImport.FixMissingMS2Fields(ms2Scans, "30", 1.2);

			List<MsDataScan> combinedScans = ms1Scans.Concat(ms2Scans).ToList().OrderBy(i => i.RetentionTime).ToList();

			for (int i = 0; i < combinedScans.Count; i++)
			{
				combinedScans[i].SetOneBasedScanNumber(i + 1);
			}

			int currentOneBasedScanNumber = 0;
			foreach (MsDataScan scan in combinedScans)
			{
				// updates the currentsOneBasedScanNumber with the current scan number if it detects an 
				// ms1 scan. 
				if (scan.MsnOrder == 1)
				{
					currentOneBasedScanNumber = scan.OneBasedScanNumber;
				}
				else
				{
					scan.SetOneBasedPrecursorScanNumber(currentOneBasedScanNumber);
				}
			}
			// i know this is the third concurrent iterator, but I don't care at this point. 
            foreach (MsDataScan scan in combinedScans)
            {
                scan.SetNativeID(null);
                scan.SetNativeID("controllerType=0 controllerNumber=1" + " scan=" + scan.OneBasedScanNumber.ToString());
				scan.SetPolarity(Polarity.Positive);
                StringBuilder sb = new StringBuilder();
                sb.AppendJoin("", "+ ", "NSI ", "Full ms ", "[", scan.ScanWindowRange.Minimum, "-",
                    scan.ScanWindowRange.Maximum, "]"); 
                scan.SetScanFilter(sb.ToString());
                if (scan.MsnOrder == 2 && scan.IsolationWidth == null)
                {
                    if (scan.IsolationMz == null)
                    {
						scan.SetIsolationMz(500);
                    }
					scan.SetIsolationWidth(1.1);
                }
            }

			// put scans into an array instead of a list 
			MsDataScan[] combinedScansArray = combinedScans.ToArray();
            //MsDataFile newFile = new MsDataFile(combinedScansArray, sf); 
            SourceFile genericSf = new SourceFile("no nativeID format", "mzML format",
                null, null, null);
            MsDataFile newDf = new MsDataFile(combinedScansArray, genericSf);

			// write file
			string outfileName = filePathMgf.Replace(".mgf", ".mzml");
			string outputFilePath = Path.Combine(outfolderPath, outfileName);
			MzmlMethods.CreateAndWriteMyMzmlWithCalibratedSpectra(newDf, outputFilePath, false);
		}
	}
}
