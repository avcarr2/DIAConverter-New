# DIAConverter-New
This console line application combines the output of DIA-Umpire with the origianl mzml file. It combines the MS2 spectra from the mgf files (DIA-Umpire's output) and the MS1 scans from the original mzml format files. I spent a LONG time troubleshooting the output to get the combined mzml files to run in PEAKs. The codebase contains a fork of the Smith Group's mzLib, as I had to modify the MsDataScan object contained in that codebase to allow for direct manipulation in order to get a valid mzml file at the end of processing.  

Initially, the goal was to automate the processing, calling DIA-Umpire and MSConvert as necessary to do the whole workflow. Quite a bit of the necessary code infrastructure to fully automate the workflow still exists, but I never had the time to fully debug it. 

Support will be limited. Use the Issues tab to report bugs.

## Requisites for running
1) .NET 6.0 (https://dotnet.microsoft.com/en-us/download)
2) Visual Studio (https://visualstudio.microsoft.com/#vs-section)
3) At least 16 GB of RAM. 
4) Windows 11. Issues causes by running on Linux-based machines will not be supported.  

## Building DIAConverter-New
1) Clone this repository to your local machine. 
2) Navigate to the directory on your local machine and open the InjectionTimeGetterApp.sln in Visual Studio. 
3) Build the application either for debug or release mode (Ctrl + Shift + B, or the first option in the Build dropdown menu).  
4) Make note of the final location of the .exe file output by release, which will be found in DIAConverter-New\bin\Debug\net6.0\.

## Command Line Interface Options 
Option 1: PATH TO MZML FILES:
  Original data location. Must be in mzml file format. 
Option 2: PATH TO MGF FILES: 
  DIA-Umpire output. mgf file format. 
Option 3: PATH TO OUTPUT FOLDER
  Desired output folder. 
  
## Running InjectionTimeGetterApp
1) open command prompt. 
2) type the following command 
> start /b "" "{path to InjectionTimeGetterApp.exe}" "{PATH TO MZML FILES}" "{PATH TO MGF FILES}" "{PATH TO OUTPUT FOLDER}"
