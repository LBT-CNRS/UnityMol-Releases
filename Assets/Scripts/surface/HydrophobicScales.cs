
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HydrophobicScales{

	private static Dictionary<string, Color32> hydro_scale = null;

	// Color surface using Kyte & Doolittle hydrophobic scale
	public static void InitKyteDoo() {

		hydro_scale = new Dictionary <string, Color32> ();

		hydro_scale.Add ("ILE", new Color32 (255,   0,  0, 255));
		hydro_scale.Add ("VAL", new Color32 (255,  17, 17, 255));
		hydro_scale.Add ("LEU", new Color32 (255,  40, 40, 255));

		hydro_scale.Add ("PHE", new Color32 (255,  96, 96, 255));
		hydro_scale.Add ("CYS", new Color32 (255, 113, 113, 255));
		hydro_scale.Add ("MET", new Color32 (255, 147, 147, 255));
		hydro_scale.Add ("ALA", new Color32 (255, 153, 153, 255));

		hydro_scale.Add ("GLY", new Color32 ( 232, 232, 255, 255));
		hydro_scale.Add ("THR", new Color32 ( 215, 215, 255, 255));
		hydro_scale.Add ("SER", new Color32 ( 210, 210, 255, 255));
		hydro_scale.Add ("TRP", new Color32 ( 204, 204, 255, 255));
		hydro_scale.Add ("TYR", new Color32 ( 181, 181, 255, 255));
		hydro_scale.Add ("PRO", new Color32 ( 164, 164, 255, 255));

		hydro_scale.Add ("HIS", new Color32 (  74,  74, 255, 255));
		hydro_scale.Add ("ASN", new Color32 (  57,  57, 255, 255));
		hydro_scale.Add ("GLN", new Color32 (  57,  57, 255, 255));
		hydro_scale.Add ("ASP", new Color32 (  57,  57, 255, 255));
		hydro_scale.Add ("GLU", new Color32 (  57,  57, 255, 255));
		hydro_scale.Add ("LYS", new Color32 (  34,  34, 255, 255));

		hydro_scale.Add ("ARG", new Color32 (   0, 0, 255, 255));
	}

	public static void InitEngleman() {
		
		hydro_scale = new Dictionary <string, Color32> ();

		hydro_scale.Add ("PHE", new Color32 ( 255,   0,   0, 255));
		hydro_scale.Add ("MET", new Color32 ( 255,  21,  21, 255));
		hydro_scale.Add ("ILE", new Color32 ( 255,  41,  41, 255));
		hydro_scale.Add ("LEU", new Color32 ( 255,  62,  62, 255));
		hydro_scale.Add ("VAL", new Color32 ( 255,  76,  76, 255));
		hydro_scale.Add ("CYS", new Color32 ( 255, 117, 117, 255));
		hydro_scale.Add ("TRP", new Color32 ( 255, 124, 124, 255));
		hydro_scale.Add ("ALA", new Color32 ( 255, 145, 145, 255));
		hydro_scale.Add ("THR", new Color32 ( 255, 172, 172, 255));
		hydro_scale.Add ("GLY", new Color32 ( 255, 186, 186, 255));
		hydro_scale.Add ("SER", new Color32 ( 255, 214, 214, 255));
		
		hydro_scale.Add ("PRO", new Color32 ( 251, 251, 255, 255));
		hydro_scale.Add ("TYR", new Color32 ( 240, 240, 255, 255));
		hydro_scale.Add ("HIS", new Color32 ( 193, 193, 255, 255));
		hydro_scale.Add ("GLN", new Color32 ( 170, 170, 255, 255));
		hydro_scale.Add ("ASN", new Color32 ( 155, 155, 255, 255));
		hydro_scale.Add ("GLU", new Color32 (  85,  85, 255, 255));
		hydro_scale.Add ("LYS", new Color32 (  73,  73, 255, 255));
		hydro_scale.Add ("ASP", new Color32 (  64,  64, 255, 255));
		hydro_scale.Add ("ARG", new Color32 (   0, 0, 255, 255));
	}

	public static void InitEisenberg() {
		
		hydro_scale = new Dictionary <string, Color32> ();

		hydro_scale.Add ("ILE", new Color32 ( 255,   0,   0, 255));
		hydro_scale.Add ("PHE", new Color32 ( 255,  35,  35, 255));
		hydro_scale.Add ("VAL", new Color32 ( 255,  56,  56, 255));
		hydro_scale.Add ("LEU", new Color32 ( 255,  59,  59, 255));
		hydro_scale.Add ("TRP", new Color32 ( 255, 105, 105, 255));
		hydro_scale.Add ("MET", new Color32 ( 255, 137, 137, 255));
		hydro_scale.Add ("ALA", new Color32 ( 255, 140, 140, 255));
		hydro_scale.Add ("GLY", new Color32 ( 255, 166, 166, 255));
		hydro_scale.Add ("CYS", new Color32 ( 255, 201, 201, 255));
		hydro_scale.Add ("TYR", new Color32 ( 255, 207, 207, 255));
		hydro_scale.Add ("PRO", new Color32 ( 255, 233, 233, 255));

		hydro_scale.Add ("THR", new Color32 ( 252, 252, 255, 255));
		hydro_scale.Add ("SER", new Color32 ( 237, 237, 255, 255));
		hydro_scale.Add ("HIS", new Color32 ( 215, 215, 255, 255));
		hydro_scale.Add ("GLU", new Color32 ( 180, 180, 255, 255));
		hydro_scale.Add ("ASN", new Color32 ( 176, 176, 255, 255));
		hydro_scale.Add ("GLN", new Color32 ( 169, 169, 255, 255));
		hydro_scale.Add ("ASP", new Color32 ( 164, 164, 255, 255));
		hydro_scale.Add ("ARG", new Color32 ( 104, 104, 255, 255));
		hydro_scale.Add ("LYS", new Color32 (   0,   0, 255, 255));

	}

	public static void InitWhiteOct() {
		
		hydro_scale = new Dictionary <string, Color32> ();

		hydro_scale.Add ("TRP", new Color32 ( 255,   0,   0, 255));
		hydro_scale.Add ("PHE", new Color32 ( 255,  47,  47, 255));
		hydro_scale.Add ("LEU", new Color32 ( 255, 102, 102, 255));
		hydro_scale.Add ("ILE", new Color32 ( 255, 118, 118, 255));
		hydro_scale.Add ("TYR", new Color32 ( 255, 168, 168, 255));
		hydro_scale.Add ("MET", new Color32 ( 255, 173, 173, 255));
		hydro_scale.Add ("VAL", new Color32 ( 255, 199, 199, 255));
		hydro_scale.Add ("CYS", new Color32 ( 255, 253, 253, 255));

		hydro_scale.Add ("PRO", new Color32 ( 245, 245, 255, 255));
		hydro_scale.Add ("THR", new Color32 ( 238, 238, 255, 255));
		hydro_scale.Add ("SER", new Color32 ( 223, 223, 255, 255));
		hydro_scale.Add ("ALA", new Color32 ( 220, 220, 255, 255));
		hydro_scale.Add ("GLN", new Color32 ( 201, 201, 255, 255));
		hydro_scale.Add ("ASN", new Color32 ( 195, 195, 255, 255));
		hydro_scale.Add ("GLY", new Color32 ( 174, 174, 255, 255));
		hydro_scale.Add ("ARG", new Color32 ( 128, 128, 255, 255));
		hydro_scale.Add ("HIS", new Color32 (  92,  92, 255, 255));
		hydro_scale.Add ("LYS", new Color32 (  59,  59, 255, 255));
		hydro_scale.Add ("GLU", new Color32 (   1,   1, 255, 255));
		hydro_scale.Add ("ASP", new Color32 (   0,   0, 255, 255));
		
	}
	
	public static void InitPhysChim() {

		hydro_scale = new Dictionary <string, Color32> ();

		//Apolar (white)
		hydro_scale.Add ("ILE", new Color32 (255, 255, 255, 255));
		hydro_scale.Add ("VAL", new Color32 (255, 255, 255, 255));
		hydro_scale.Add ("LEU", new Color32 (255, 255, 255, 255));
		hydro_scale.Add ("PHE", new Color32 (255, 255, 255, 255));
		hydro_scale.Add ("MET", new Color32 (255, 255, 255, 255));
		hydro_scale.Add ("ALA", new Color32 (255, 255, 255, 255));
		hydro_scale.Add ("GLY", new Color32 (255, 255, 255, 255));
		hydro_scale.Add ("TRP", new Color32 (255, 255, 255, 255));
		hydro_scale.Add ("PRO", new Color32 (255, 255, 255, 255));

		//Polar (Green)
		hydro_scale.Add ("THR", new Color32 (0, 255, 0, 255));
		hydro_scale.Add ("SER", new Color32 (0, 255, 0, 255));
		hydro_scale.Add ("TYR", new Color32 (0, 255, 0, 255));
		hydro_scale.Add ("ASN", new Color32 (0, 255, 0, 255));
		hydro_scale.Add ("GLN", new Color32 (0, 255, 0, 255));
		hydro_scale.Add ("CYS", new Color32 (0, 255, 0, 255));

		//Acid (red)
		hydro_scale.Add ("ASP", new Color32 (255, 0, 0, 255));
		hydro_scale.Add ("GLU", new Color32 (255, 0, 0, 255));

		//Base (blue)
		hydro_scale.Add ("LYS", new Color32 (0, 0, 255, 255));
		hydro_scale.Add ("ARG", new Color32 (0, 0, 255, 255));
		hydro_scale.Add ("HIS", new Color32 (0, 0, 255, 255));

	}

	public static void InitShapelyColors() {
		
		hydro_scale = new Dictionary <string, Color32> ();

		hydro_scale.Add ("ASP", new Color32 (230,  10,  10, 255));
		hydro_scale.Add ("GLU", new Color32 (230,  10,  10, 255));

		hydro_scale.Add ("CYS", new Color32 (230, 230,   0, 255));
		hydro_scale.Add ("MET", new Color32 (230, 230,   0, 255));

		hydro_scale.Add ("LYS", new Color32 ( 20,  90, 255, 255));
		hydro_scale.Add ("ARG", new Color32 ( 20,  90, 255, 255));

		hydro_scale.Add ("SER", new Color32 (250, 150,   0, 255));
		hydro_scale.Add ("THR", new Color32 (250, 150,   0, 255));

		hydro_scale.Add ("PHE", new Color32 ( 50,  50, 170, 255));
		hydro_scale.Add ("TYR", new Color32 ( 50,  50, 170, 255));

		hydro_scale.Add ("ASN", new Color32 (  0, 220, 220, 255));
		hydro_scale.Add ("GLN", new Color32 (  0, 220, 220, 255));

		hydro_scale.Add ("GLY", new Color32 (235, 235, 235, 255));

		hydro_scale.Add ("LEU", new Color32 ( 15, 130,  15, 255));
		hydro_scale.Add ("VAL", new Color32 ( 15, 130,  15, 255));
		hydro_scale.Add ("ILE", new Color32 ( 15, 130,  15, 255));

		hydro_scale.Add ("ALA", new Color32 (200, 200, 200, 255));
		hydro_scale.Add ("TRP", new Color32 (180,  90, 180, 255));
		hydro_scale.Add ("HIS", new Color32 (130, 130, 210, 255));
		hydro_scale.Add ("PRO", new Color32 (220, 150, 130, 255));
	}

			
	public static Color32 GetColorHydro(string type) {
		Color32 value = new Color32(255, 255, 255, 255);
		if(hydro_scale == null)
			return new Color32(255, 255, 255, 255);
		else if(!hydro_scale.TryGetValue(type, out value))
			return new Color32(255, 255, 255, 255);
		else
			return hydro_scale[type];
	}
				
}


