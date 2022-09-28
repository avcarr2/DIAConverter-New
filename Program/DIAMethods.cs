using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassSpectrometry;

namespace Program
{
    public static class DIAMethods
    {
        public static void FillMissingPrecursorScans(List<MsDataScan> listScans)
        {
            int scanIndex = 1;
            for (int i = 0; i < listScans.Count; i++)
            {
                if (listScans[i].MsnOrder != 1)
                {
                    listScans[i].SetOneBasedPrecursorScanNumber(scanIndex);
                }
                else
                {
                    scanIndex = i + 1;
                }
            }
        }
    }
}
