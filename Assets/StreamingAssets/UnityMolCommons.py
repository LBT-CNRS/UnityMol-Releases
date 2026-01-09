# UnityMolCommons.py
# UnityMol Development Script
# (c) 2025 by Marc BAADEN
# MIT license

import UnityEngine
import UMol
import System
from System.Diagnostics import Process, ProcessStartInfo

# HELPER SHORTCUT TO EXIT UNITYMOL
def q():
    UnityEngine.Application.Quit()

def run_command(cmd):
    psi = ProcessStartInfo()
    psi.FileName = "/bin/bash"  # Use bash to execute shell commands
    psi.Arguments = "-c \"" + cmd + "\""
    psi.RedirectStandardOutput = True
    psi.UseShellExecute = False
    psi.CreateNoWindow = True

    process = Process()
    process.StartInfo = psi
    process.Start()
    output = process.StandardOutput.ReadToEnd()
    process.WaitForExit()

    return output.strip()

def info():
    print("\nBelow information about version and build\n")
    print("  Version:" + UnityEngine.Application.version)
    print("  Platform: "+ str(UnityEngine.Application.platform))
    print("  productName: " + UnityEngine.Application.productName)
    print("  companyName: " + UnityEngine.Application.companyName)
    print("  unityVersion: " + UnityEngine.Application.unityVersion)
    print("  operatingSystem: " + UnityEngine.SystemInfo.operatingSystem)
    print("  deviceModel: " + UnityEngine.SystemInfo.deviceModel)
    print("  Processor Type: " + UnityEngine.SystemInfo.processorType)
    print("  OS Architecture: " + UnityEngine.SystemInfo.operatingSystem)
    print("  UnityMol version:" + UMol.UnityMolVersion.GetVersion())
    print("\n")

    if(UnityEngine.Application.platform == UnityEngine.RuntimePlatform.OSXEditor or UnityEngine.Application.platform == UnityEngine.RuntimePlatform.OSXPlayer):
        # Run lipo to check architecture
        arch_info = run_command("lipo -info " + System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)
        # Run file to check binary details
        file_info = run_command("file " + System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)
        print("  Built for architecture:" + arch_info)
        print("  Executable Info:" + file_info)

# MOUSESPEED INCREASE (OFTEN NEEDED ON MACs)
UMol.API.APIPython.setMouseMoveSpeed(5)
