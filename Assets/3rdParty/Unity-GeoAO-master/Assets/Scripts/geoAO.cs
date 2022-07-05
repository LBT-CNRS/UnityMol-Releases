// MIT License

// Copyright (c) 2017 Xavier Martinez

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

// #define DEBUGAO
using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class geoAO : MonoBehaviour {



	public enum samplesAOpreset  {
		VeryLow = 16,
		Low = 36,
		Medium = 64,
		High = 144,
		VeryHigh = 256,
		Ultra = 512,
		TooMuch = 1024,
		WayTooMuch = 2048
	}


	public samplesAOpreset samplesAO = samplesAOpreset.Ultra;

	private MeshFilter[] mfs;
	private Vector3[] saveScale;
	private Transform[] saveParent;
	private Vector3[] savePos;
	private Quaternion[] saveRot;
	private Material[] saveMat;

	private Resolution saveResolution;
	private bool wasFullScreen;

	private Vector3[] rayDir;

	private Bounds allBounds;

	private Camera AOCam;
	public RenderTexture AORT;
	public RenderTexture AORT2;
	private Texture2D vertTex;

	private Material AOMat;

	private int nbVert = 0;

	private int vertByRow = 512;
	private int usedMeshes = 0;
	public LayerMask AOLayer;

	public void ComputeAO (List<GameObject> meshTransforms) {

		List<int> savedLayers = new List<int>();
		AOLayer = 1 << LayerMask.NameToLayer("AOLayer");

		float timerAO = Time.realtimeSinceStartup;

		nbVert = 0;
		List<MeshFilter> tmpMFlist = new List<MeshFilter>(meshTransforms.Count);
		foreach (GameObject g in meshTransforms) {
			savedLayers.Add(g.layer);
			g.layer = LayerMask.NameToLayer("AOLayer");
			tmpMFlist.Add(g.GetComponent<MeshFilter>());
		}
		mfs = tmpMFlist.ToArray();
		for (int i = 0; i < mfs.Length; i++) {
			if (mfs[i].gameObject.GetComponent<MeshRenderer>().enabled) {
				nbVert += mfs[i].mesh.vertices.Length;
				usedMeshes++;
			}
		}
		if(nbVert == 0){
			return;
		}

		InitSamplePos();

		CreateAOCam();

		DoAO();

		DisplayAO();

		float timerAO2 = Time.realtimeSinceStartup;
		// Debug.Log("Time for AO  = " + (timerAO2 - timerAO));

#if (!DEBUGAO)
		for (int i = 0; i < meshTransforms.Count; i++) {
			GameObject g = meshTransforms[i];
			g.layer = savedLayers[i];
		}
#endif
	}

	void InitSamplePos() {

		getBounds();

		Vector3 boundMax = allBounds.size;
		float radSurface = Mathf.Max(boundMax.x, Mathf.Max(boundMax.y, boundMax.z));
		rayDir = new Vector3[(int)samplesAO];

		float golden_angle = Mathf.PI * (3 - Mathf.Sqrt(5));
		float start =  1 - 1.0f / (int)samplesAO;
		float end = 1.0f / (int)samplesAO - 1;

		for (int i = 0; i < (int)samplesAO; i++) {
			float theta = golden_angle * i;
			float z = start + i * (end - start) / (int)samplesAO;
			float radius = Mathf.Sqrt(1 - z * z);
			rayDir[i].x = radius * Mathf.Cos(theta);
			rayDir[i].y = radius * Mathf.Sin(theta);
			rayDir[i].z = z;
			rayDir[i] *= radSurface;
			rayDir[i] += allBounds.center;

#if (DEBUGAO)
			//// Debug
			GameObject test = GameObject.CreatePrimitive(PrimitiveType.Cube);
			test.transform.localScale = Vector3.one * 1f;
			test.transform.position = rayDir[i];
			// test.transform.parent = transform;
#endif
		}
	}

	void getBounds() {
		saveScale = new Vector3[usedMeshes];
		saveParent = new Transform[usedMeshes];
		savePos = new Vector3[usedMeshes];
		saveRot = new Quaternion[usedMeshes];
		saveMat = new Material[usedMeshes];

		for (int i = 0; i < mfs.Length; i++) {
			if (mfs[i].gameObject.GetComponent<MeshRenderer>().enabled) {

				saveScale[i] = mfs[i].transform.localScale;
				saveParent[i] = mfs[i].transform.parent;
				savePos[i] = mfs[i].transform.position;
				saveRot[i] = mfs[i].transform.rotation;
				saveMat[i] = mfs[i].GetComponent<Renderer>().sharedMaterial;
				if (i == 0)
					allBounds = mfs[i].mesh.bounds;
				else
					allBounds.Encapsulate(mfs[i].mesh.bounds);
			}
		}
		#if DEBUGAO
		Debug.Log("Center = " + allBounds.center.ToString("F3")+" Min " + allBounds.min.ToString("F3") + " Max " + allBounds.max.ToString("F3") + " size = " + allBounds.size.ToString("F3"));
		#endif
	}

	void CreateAOCam() {

		AOCam = gameObject.AddComponent<Camera>();
		if (AOCam == null)
			AOCam = gameObject.GetComponent<Camera>();


		AOCam.enabled = false;

		// AOCam.CopyFrom(Camera.main);
		AOCam.orthographic = true;
		// AOCam.cullingMask=1<<0; // default layer for now
		AOCam.cullingMask = AOLayer;
		AOCam.clearFlags = CameraClearFlags.Depth;
		AOCam.nearClipPlane = 0.1f;
		AOCam.farClipPlane = 500f;
		AOCam.stereoTargetEye = StereoTargetEyeMask.None;

		AOCam.depthTextureMode = DepthTextureMode.Depth ;

		saveResolution = Screen.currentResolution;
		wasFullScreen = Screen.fullScreen;

		changeAspectRatio();

		float screenRatio = 1f;

		float targetRatio = allBounds.size.x / allBounds.size.y;

		if (screenRatio >= targetRatio)
			AOCam.orthographicSize = 2.0f * (allBounds.size.y / 2);
		else {
			float differenceInSize = targetRatio / screenRatio;
			AOCam.orthographicSize = 2.0f * (allBounds.size.y / 2 * differenceInSize);
		}

		AOMat = new Material(Shader.Find("Custom/VertexAO"));


		int height = (int) Mathf.Ceil(nbVert / (float)vertByRow);

		// Debug.Log("Creating a texture of size : " + vertByRow + " x " + height);
		// Debug.Log("Vertices = " + nbVert);

		AORT = new RenderTexture(vertByRow, height, 0, RenderTextureFormat.ARGBHalf);
		AORT.anisoLevel = 0;
		AORT.filterMode = FilterMode.Point;

		AORT2 = new RenderTexture(vertByRow, height, 0, RenderTextureFormat.ARGBHalf);
		AORT2.anisoLevel = 0;
		AORT2.filterMode = FilterMode.Point;

		vertTex = new Texture2D(vertByRow, height, TextureFormat.RGBAFloat, false);
		vertTex.anisoLevel = 0;
		vertTex.filterMode = FilterMode.Point;

		FillVertexTexture();
	}

	void FillVertexTexture() {
		int idVert = 0;
		int sizeRT = vertTex.width * vertTex.height;
		Color[] vertInfo = new Color[sizeRT];
		for (int i = 0; i < mfs.Length; i++) {
			if (mfs[i].gameObject.GetComponent<MeshRenderer>().enabled) {

				Vector3[] vert = mfs[i].mesh.vertices;
				for (int j = 0; j < vert.Length; j++) {
					vertInfo[idVert].r = vert[j].x;
					vertInfo[idVert].g = vert[j].y;
					vertInfo[idVert].b = vert[j].z;
					idVert++;
				}
			}
		}
		vertTex.SetPixels(vertInfo);
		vertTex.Apply(false, false);
	}

	void changeAspectRatio() {
		float targetaspect = 1.0f;

		// determine the game window's current aspect ratio
		float windowaspect = (float)Screen.width / (float)Screen.height;

		// current viewport height should be scaled by this amount
		float scaleheight = windowaspect / targetaspect;


		// if scaled height is less than current height, add letterbox
		if (scaleheight < 1.0f)
		{
			Rect rect = AOCam.rect;

			rect.width = 1.0f;
			rect.height = scaleheight;
			rect.x = 0;
			rect.y = (1.0f - scaleheight) / 2.0f;

			AOCam.rect = rect;
		}
		else // add pillarbox
		{
			float scalewidth = 1.0f / scaleheight;

			Rect rect = AOCam.rect;

			rect.width = scalewidth;
			rect.height = 1.0f;
			rect.x = (1.0f - scalewidth) / 2.0f;
			rect.y = 0;

			AOCam.rect = rect;
		}

	}


	void DoAO() {


		AOMat.SetInt("_uCount", (int)samplesAO);
		AOMat.SetTexture("_AOTex", AORT);
		AOMat.SetTexture("_AOTex2", AORT2);
		AOMat.SetTexture("_uVertex", vertTex);

		Material standardMat = new Material(Shader.Find("Standard"));

		for (int i = 0; i < mfs.Length; i++) {
			if (mfs[i].gameObject.GetComponent<MeshRenderer>().enabled) {

				mfs[i].transform.parent = null;
				mfs[i].transform.position = Vector3.zero;
				mfs[i].transform.rotation = Quaternion.Euler(0f, 0f, 0f);
				mfs[i].transform.localScale = Vector3.one;
				mfs[i].GetComponent<Renderer>().sharedMaterial = standardMat;

			}
		}

		for (int i = 0; i < (int)samplesAO; i++) {

			AOCam.transform.position = rayDir[i];
			AOCam.transform.LookAt(allBounds.center);

			Matrix4x4 V = AOCam.worldToCameraMatrix;
			Matrix4x4 P = AOCam.projectionMatrix;

			bool d3d = SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1;
			if (d3d) {
				// Invert Y for rendering to a render texture
				for (int a = 0; a < 4; a++) {
					P[1, a] = -P[1, a];
				}
				// Scale and bias from OpenGL -> D3D depth range
				for (int a = 0; a < 4; a++) {
					P[2, a] = P[2, a] * 0.5f + P[3, a] * 0.5f;
				}
			}

			AOMat.SetMatrix("_VP", (P * V));
			AOCam.Render();
		}

		for (int i = 0; i < mfs.Length; i++) {
			if (mfs[i].gameObject.GetComponent<MeshRenderer>().enabled) {

#if (!DEBUGAO)
				mfs[i].transform.parent = saveParent[i];
				mfs[i].transform.position = savePos[i];
				mfs[i].transform.localScale = saveScale[i];
				mfs[i].transform.rotation = saveRot[i];
#endif
				mfs[i].GetComponent<Renderer>().sharedMaterial = saveMat[i];
			}
		}

	}
	void OnRenderImage (RenderTexture source, RenderTexture destination) {


		var matrix = AOCam.cameraToWorldMatrix;
		AOMat.SetMatrix("_InverseView", matrix);
		Graphics.Blit(null, AORT, AOMat);
		AOCam.targetTexture = null;
		Graphics.Blit(AORT, AORT2);
		// Graphics.Blit(source, destination);
	}

	void DisplayAO() {

		if (false) { //Create a texture containing AO information read by the mesh shader
			List<Vector2[]> alluv = new List<Vector2[]>(usedMeshes);

			Material matShowAO = new Material(Shader.Find("AO/VertAOOpti"));
			matShowAO.SetTexture("_AOTex", AORT);
			float w = (float)(AORT2.width - 1);
			float h = (float)(AORT2.height - 1);
			int idVert = 0;
			for (int i = 0; i < mfs.Length; i++) {
				if (mfs[i].gameObject.GetComponent<MeshRenderer>().enabled) {

					Vector3[] vert = mfs[i].mesh.vertices;
					alluv.Add( new Vector2[vert.Length] );
					for (int j = 0; j < vert.Length; j++) {
						alluv[i][j] = new Vector2((idVert % vertByRow) / w, (idVert / (vertByRow) / h));
						idVert++;
					}
					mfs[i].mesh.uv2 = alluv[i];
					mfs[i].gameObject.GetComponent<Renderer>().material = matShowAO;
				}
			}
		}
		else { //Directly modify the colors of the mesh (slower)
			// Material matShowAO = new Material(Shader.Find("Vertex Colored"));

			List<Color> allColors = new List<Color>(nbVert);
			RenderTexture.active = AORT2;
			Texture2D resulTex = new Texture2D(AORT2.width, AORT2.height, TextureFormat.RGBAHalf, false);
			resulTex.ReadPixels( new Rect(0, 0, AORT2.width, AORT2.height), 0, 0);

			for (int i = 0; i < nbVert; i++) {
				allColors.Add(resulTex.GetPixel(i % vertByRow, i / (vertByRow)));
			}


			int idVert = 0;
			for (int i = 0; i < mfs.Length; i++) {
				if (mfs[i].gameObject.GetComponent<MeshRenderer>().enabled) {

					int nbVerts = mfs[i].mesh.vertices.Length;
					Color[] colors = mfs[i].mesh.colors;
					if (colors.Length == 0) {
						colors = new Color[nbVerts];
						for (int c = 0; c < nbVerts; c++) {
							colors[c] = Color.white;
						}

					}

					if (colors.Length == 0) {
						//Override mesh colors
						// mfs[i].mesh.colors = allColors.GetRange(idVert, nbVerts).ToArray();
						// idVert += nbVerts;

						colors = new Color[nbVerts];
						for (int c = 0; c < nbVerts; c++) {
							Color tmpCol = Color.white;
							tmpCol.a = allColors[idVert++].r;
							colors[c] = tmpCol;
						}
						mfs[i].mesh.colors = colors;
					}
					else {

						//Fill the alpha value of mesh colors
						for (int c = 0; c < colors.Length; c++) {
							Color tmpCol = colors[c];

							tmpCol.a = allColors[idVert++].r;
							colors[c] = tmpCol;
						}
						mfs[i].mesh.colors = colors;
					}

				}
			}
		}
	}



#if DEBUGAO
	void Update() {
		DrawBox();
	}
	void DrawBox() {
		Vector3 v3Center = allBounds.center;
		Vector3 v3Extents = allBounds.extents;

		Vector3 v3FrontTopLeft     = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  // Front top left corner
		Vector3 v3FrontTopRight    = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  // Front top right corner
		Vector3 v3FrontBottomLeft  = new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  // Front bottom left corner
		Vector3 v3FrontBottomRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  // Front bottom right corner
		Vector3 v3BackTopLeft      = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top left corner
		Vector3 v3BackTopRight        = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top right corner
		Vector3 v3BackBottomLeft   = new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  // Back bottom left corner
		Vector3 v3BackBottomRight  = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  // Back bottom right corner

		v3FrontTopLeft     = transform.TransformPoint(v3FrontTopLeft);
		v3FrontTopRight    = transform.TransformPoint(v3FrontTopRight);
		v3FrontBottomLeft  = transform.TransformPoint(v3FrontBottomLeft);
		v3FrontBottomRight = transform.TransformPoint(v3FrontBottomRight);
		v3BackTopLeft      = transform.TransformPoint(v3BackTopLeft);
		v3BackTopRight     = transform.TransformPoint(v3BackTopRight);
		v3BackBottomLeft   = transform.TransformPoint(v3BackBottomLeft);
		v3BackBottomRight  = transform.TransformPoint(v3BackBottomRight);


		//if (Input.GetKey (KeyCode.S)) {
		Debug.DrawLine (v3FrontTopLeft, v3FrontTopRight, Color.yellow);
		Debug.DrawLine (v3FrontTopRight, v3FrontBottomRight, Color.yellow);
		Debug.DrawLine (v3FrontBottomRight, v3FrontBottomLeft, Color.yellow);
		Debug.DrawLine (v3FrontBottomLeft, v3FrontTopLeft, Color.yellow);

		Debug.DrawLine (v3BackTopLeft, v3BackTopRight, Color.yellow);
		Debug.DrawLine (v3BackTopRight, v3BackBottomRight, Color.yellow);
		Debug.DrawLine (v3BackBottomRight, v3BackBottomLeft, Color.yellow);
		Debug.DrawLine (v3BackBottomLeft, v3BackTopLeft, Color.yellow);

		Debug.DrawLine (v3FrontTopLeft, v3BackTopLeft, Color.yellow);
		Debug.DrawLine (v3FrontTopRight, v3BackTopRight, Color.yellow);
		Debug.DrawLine (v3FrontBottomRight, v3BackBottomRight, Color.yellow);
		Debug.DrawLine (v3FrontBottomLeft, v3BackBottomLeft, Color.yellow);
		//}
	}
#endif

}

