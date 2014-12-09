using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

public class VARNA : MonoBehaviour {
	public static string generateStructureString(int sequenceLength, List<int[]> pairs)
	{
		StringBuilder structure = new StringBuilder(new string('.', sequenceLength), sequenceLength);
		int pair_count = pairs.Count;
		for (int i = 0; i < pair_count; i++) 
		{
			int n1 = pairs[i][0];
			int n2 = pairs[i][1];
			if(n1 > n2) {
				int temp = n1;
				n1 = n2;
				n2 = temp;
			}
			if (n1 <= pair_count && n2 <= pair_count && n1 != n2) {
				structure[n1 - 1] = '(';
				structure[n2 - 1] = ')';
			}
		}
		return structure.ToString();
	}
	
	public static Process generateImage(string sequence, string structure, string outputDirectory, string filename)
	{		
		string cmd = "java";
		ProcessStartInfo startInfo = new ProcessStartInfo(cmd);
		startInfo.WorkingDirectory = outputDirectory;
		startInfo.UseShellExecute = false;
		startInfo.RedirectStandardInput = false;
		startInfo.RedirectStandardOutput = false;
		startInfo.Arguments = "-cp /Users/sebastien/Downloads/VARNAv3-9-src.jar fr.orsay.lri.varna.applications.VARNAcmd -sequenceDBN " + sequence + " -structureDBN " + structure + " -o " + filename;
		
		Process process = new Process();
		process.StartInfo = startInfo;
				
		process.EnableRaisingEvents = true;
		
		return process;
	}
}
