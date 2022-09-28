using System.Collections.Generic;
using System.Linq;
using IO.Mgf; 
using IO.MzML; 
using MassSpectrometry;


namespace Program
{
	public static class DIAImport
	{
		public static List<MsDataScan> ImportMGF(string path)
		{
			List<MsDataScan> data = Mgf.LoadAllStaticData(path).GetAllScansList();
			return data; 
		}
		public static List<MsDataScan> ImportMZXML(string path)
		{
			List<MsDataScan> data = Mzml.LoadAllStaticData(path).GetAllScansList();
			
			return data; 
		}
		public static List<MsDataScan> SelectMS1s(List<MsDataScan> allScans)
		{
			List<MsDataScan> ms1Scans = allScans.Where(i => i.MsnOrder == 1).Select(i => i).ToList().OrderBy(i => i.RetentionTime).ToList();
			return ms1Scans; 
			
		}
		public static void FixMissingMS2Fields(List<MsDataScan> allScans, string hcdEnergy, double width)
		{
			DIAScanModifications.UpdateMS2WithMZIsolationWidth(allScans, 1.2);
			DIAScanModifications.UpdateHcdEnergy(allScans, "30");
			DIAScanModifications.RecalculateIsolationRange(allScans);
		}
	}
	
	
}
