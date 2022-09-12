using System;
using System.IO;
using System.Collections.Generic;
using System.Linq; 
using MassSpectrometry;
using Proteomics;
using MzLibUtil;
using IO.Mgf;
using IO.MzML;
using DIAMS1Quant; 


namespace Program
{
	class Program1
	{
		static void Main(string[] args)
		{
            // args[0] = option to run. If "1", then it just runs the combination. If "0",
			// runs the combination only. 
            switch (args[0])
            {
				case "1": { ProgramCombineFiles proc = new(); 
						proc.Main(args.Skip(1).ToArray()); } 
					break;
				case "0": { Program2 proc = new(); 
						proc.Main(args.Skip(1).ToArray()); } 
					break;
			}
		}

		public static string[] FetchFilesFromFolder(string mgfFolderPath)
		{
			string[] mgfFileNames = Directory.GetFiles(mgfFolderPath, "*.mgf");
			return mgfFileNames; 
		}
		public static void DoFileProcessing(string filePathMZML, string fileNameMGF, string mgfFolderPath, string outfolderPath)
		{
			// Doesn't complete if file is empty. Need to fix this. 
			List<MsDataScan> ms1Scans = DIAImport.SelectMS1s(DIAImport.ImportMZXML(filePathMZML));
			List<MsDataScan> ms2Scans = DIAImport.ImportMGF(Path.Combine(mgfFolderPath, fileNameMGF));
			
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

			// put scans into an array instead of a list 
			MsDataScan[] combinedScansArray = combinedScans.ToArray();
			FakeMsDataFile fakeFile = new FakeMsDataFile(combinedScansArray.ToArray());

			// write file
			string outfileName = fileNameMGF.Replace(".mgf", ".mzml");
			string outputFilePath = Path.Combine(outfolderPath, outfileName);
			MzmlMethods.CreateAndWriteMyMzmlWithCalibratedSpectra(fakeFile, outputFilePath, false);
		}
		public static void DoFileProcessing(string filePathMZML, string filePathMgf, string outfolderPath)
		{
			// Doesn't complete if file is empty. Need to fix this. 
			List<MsDataScan> ms1Scans = DIAImport.SelectMS1s(DIAImport.ImportMZXML(filePathMZML));
			List<MsDataScan> ms2Scans = DIAImport.ImportMGF(filePathMgf);

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

			// put scans into an array instead of a list 
			MsDataScan[] combinedScansArray = combinedScans.ToArray();
			FakeMsDataFile fakeFile = new FakeMsDataFile(combinedScansArray.ToArray());

			// write file
			string outfileName = filePathMgf.Replace(".mgf", ".mzml");
			string outputFilePath = Path.Combine(outfolderPath, outfileName);
			MzmlMethods.CreateAndWriteMyMzmlWithCalibratedSpectra(fakeFile, outputFilePath, false);
		}
	}
}
