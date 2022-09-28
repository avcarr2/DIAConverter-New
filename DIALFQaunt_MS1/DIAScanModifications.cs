using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassSpectrometry;

namespace DIALFQaunt_MS1
{
    public static class DIAScanModifications
    {
        public static void UpdateMS2WithMZIsolationWidth(List<MsDataScan> allScans, double mzWindow)
        {
            foreach (MsDataScan scan in allScans)
            {
                scan.SetIsolationWidth(mzWindow);
            }
        }
        public static void UpdateHcdEnergy(List<MsDataScan> allScans, string energy)
        {
            foreach (MsDataScan scan in allScans)
            {
                scan.SetHcdEnergy(energy);
            }
        }
        public static void UpdateIsolationWidth(List<MsDataScan> allScans, double value)
        {
            foreach (MsDataScan scan in allScans)
            {
                scan.SetIsolationWidth(value);
            }
        }
        public static void RecalculateIsolationRange(List<MsDataScan> allScans)
        {
            foreach (MsDataScan scan in allScans)
            {
                scan.UpdateIsolationRange();
            }
        }
        public static void UpdateOneBasedPrecursorScan(List<MsDataScan> combinedScans)
        {
            int currentOneBasedScanNumber = 0;
            foreach (MsDataScan scan in combinedScans)
            {
                if (scan.MsnOrder == 1)
                {
                    currentOneBasedScanNumber = scan.OneBasedScanNumber;
                }
                else
                {
                    scan.SetOneBasedPrecursorScanNumber(currentOneBasedScanNumber);
                }
            }
        }
        public static void UpdatePrecursorIntensity(List<MsDataScan> combinedScans)
        {
            foreach (MsDataScan scan in combinedScans)
            {
                if (scan.OneBasedPrecursorScanNumber == null)
                {
                    break;
                }
                else
                {
                    int precursorScanNumber = scan.OneBasedPrecursorScanNumber.Value;
                }
            }
        }

        /* public static List<MsDataScan> UpdateMissingValuesInMS2Spectra(List<MsDataScan> allScans)
		{
			List<MsDataScan> updatedScans = new List<MsDataScan>(); 
			
			foreach(MsDataScan scan in allScans)
			{
				if(scan.MsnOrder != 1)
				{
					MzRange range; 
					if(scan.IsolationRange == null)
					{
						range = MzRange(scan.IsolationMz)
					}

					MsDataScan newScan = new MsDataScan(scan.MassSpectrum, 
						oneBasedPrecursorScanNumber: scan.OneBasedScanNumber, msnOrder: scan.OneBasedScanNumber, 
						isCentroid: scan.IsCentroid, polarity: scan.Polarity, retentionTime: scan.RetentionTime)
				}
				else
				{
					updatedScans.Add(scan); 
				}
			}
		}
		*/
    }
}
