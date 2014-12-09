/// @file ImprovedFileBrowser.cs
/// @brief Details to be specified
/// @author FvNano/LBT team
/// @author Marc Baaden <baaden@smplinux.de>
/// @date   2013-4
///
/// Copyright Centre National de la Recherche Scientifique (CNRS)
///
/// contributors :
/// FvNano/LBT team, 2010-13
/// Marc Baaden, 2010-13
///
/// baaden@smplinux.de
/// http://www.baaden.ibpc.fr
///
/// This software is a computer program based on the Unity3D game engine.
/// It is part of UnityMol, a general framework whose purpose is to provide
/// a prototype for developing molecular graphics and scientific
/// visualisation applications. More details about UnityMol are provided at
/// the following URL: "http://unitymol.sourceforge.net". Parts of this
/// source code are heavily inspired from the advice provided on the Unity3D
/// forums and the Internet.
///
/// This software is governed by the CeCILL-C license under French law and
/// abiding by the rules of distribution of free software. You can use,
/// modify and/or redistribute the software under the terms of the CeCILL-C
/// license as circulated by CEA, CNRS and INRIA at the following URL:
/// "http://www.cecill.info".
/// 
/// As a counterpart to the access to the source code and rights to copy, 
/// modify and redistribute granted by the license, users are provided only 
/// with a limited warranty and the software's author, the holder of the 
/// economic rights, and the successive licensors have only limited 
/// liability.
///
/// In this respect, the user's attention is drawn to the risks associated 
/// with loading, using, modifying and/or developing or reproducing the 
/// software by the user in light of its specific status of free software, 
/// that may mean that it is complicated to manipulate, and that also 
/// therefore means that it is reserved for developers and experienced 
/// professionals having in-depth computer knowledge. Users are therefore 
/// encouraged to load and test the software's suitability as regards their 
/// requirements in conditions enabling the security of their systems and/or 
/// data to be ensured and, more generally, to use and operate it in the 
/// same conditions as regards security.
///
/// The fact that you are presently reading this means that you have had 
/// knowledge of the CeCILL-C license and that you accept its terms.
///
/// $Id: ImprovedFileBrowser.cs 635 2014-08-01 13:03:26Z tubiana $
///
/// References : 
/// If you use this code, please cite the following reference : 	
/// Z. Lv, A. Tek, F. Da Silva, C. Empereur-mot, M. Chavent and M. Baaden:
/// "Game on, Science - how video game technology may help biologists tackle
/// visualization challenges" (2013), PLoS ONE 8(3):e57990.
/// doi:10.1371/journal.pone.0057990
///
/// If you use the HyperBalls visualization metaphor, please also cite the
/// following reference : M. Chavent, A. Vanel, A. Tek, B. Levy, S. Robert,
/// B. Raffin and M. Baaden: "GPU-accelerated atom and dynamic bond visualization
/// using HyperBalls, a unified algorithm for balls, sticks and hyperboloids",
/// J. Comput. Chem., 2011, 32, 2924
///

using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

/*
    File browser for selecting files or folders at runtime.
 */
using UI;

public enum FileBrowserType {
    File,
    Directory
}

public class ImprovedFileBrowser {
   
	private int buttonWidth;
	
    // Called when the user clicks cancel or select
    public delegate void FinishedCallback(string path);
    // Defaults to working directory
    public string CurrentDirectory {
        get {
            return m_currentDirectory;
        }
        set {
            SetNewDirectory(value);
            SwitchDirectoryNow();
        }
    }
    protected string m_currentDirectory;
    // Optional pattern for filtering selectable files/folders. See:
    // http://msdn.microsoft.com/en-us/library/wz42302f(v=VS.90).aspx
    // and
    // http://msdn.microsoft.com/en-us/library/6ff71z1w(v=VS.90).aspx
    public string SelectionPattern {
        get {
            return m_filePattern;
        }
        set {
            m_filePattern = value;
            ReadDirectoryContents();
        }
    }
    protected string m_filePattern;
   
    // Optional image for directories
    public Texture2D DirectoryImage {
        get {
            return m_directoryImage;
        }
        set {
            m_directoryImage = value;
            BuildContent();
        }
    }
    protected Texture2D m_directoryImage;
   
    // Optional image for files
    public Texture2D FileImage {
        get {
            return m_fileImage;
        }
        set {
            m_fileImage = value;
            BuildContent();
        }
    }
    protected Texture2D m_fileImage;
   
    // Browser type. Defaults to File, but can be set to Folder
    public FileBrowserType BrowserType {
        get {
            return m_browserType;
        }
        set {
            m_browserType = value;
            ReadDirectoryContents();
        }
    }
    protected FileBrowserType m_browserType;
    protected string m_newDirectory;
    protected string[] m_currentDirectoryParts;
   
    protected string[] m_files;
    protected GUIContent[] m_filesWithImages;
    protected int m_selectedFile;
   
    protected string[] m_nonMatchingFiles;
    protected GUIContent[] m_nonMatchingFilesWithImages;
    protected int m_selectedNonMatchingDirectory;
   
    protected string[] m_directories;
    protected GUIContent[] m_directoriesWithImages;
    protected int m_selectedDirectory;
   
    protected string[] m_nonMatchingDirectories;
    protected GUIContent[] m_nonMatchingDirectoriesWithImages;
   
    protected bool m_currentDirectoryMatches;
   
    protected GUIStyle CentredText {
        get {
            if (m_centredText == null) {
                m_centredText = new GUIStyle(GUI.skin.label);
                m_centredText.alignment = TextAnchor.MiddleLeft;
                m_centredText.fixedHeight = GUI.skin.button.fixedHeight;
            }
            return m_centredText;
        }
    }
    protected GUIStyle m_centredText;
   
    protected string m_name;
    protected Rect m_screenRect;
   
    protected Vector2 m_scrollPosition;
   
    protected FinishedCallback m_callback;
   
    // Browsers need at least a rect, name and callback
    public ImprovedFileBrowser(Rect screenRect, string name, FinishedCallback callback) {
        m_name = name;
        m_screenRect = screenRect;
		buttonWidth = (int) m_screenRect.width / 3;
        m_browserType = FileBrowserType.File;
        m_callback = callback;
        SetNewDirectory(Directory.GetCurrentDirectory());
        SwitchDirectoryNow();
    }
	
	//Alex Tek modif
	public ImprovedFileBrowser(Rect screenRect, string name, FinishedCallback callback, string startingPath) {
        m_name = name;
        m_screenRect = screenRect;
		buttonWidth = (int) m_screenRect.width / 3;
        m_browserType = FileBrowserType.File;
        m_callback = callback;
        SetNewDirectory(startingPath);
        SwitchDirectoryNow();
    }
   
    protected void SetNewDirectory(string directory) {
        m_newDirectory = directory;
    }
   
    protected void SwitchDirectoryNow() {
        if (m_newDirectory == null || m_currentDirectory == m_newDirectory) {
            return;
        }
        m_currentDirectory = m_newDirectory;
        m_scrollPosition = Vector2.zero;
        m_selectedDirectory = m_selectedNonMatchingDirectory = m_selectedFile = -1;
        ReadDirectoryContents();
    }
   
    protected void ReadDirectoryContents() {
        if (m_currentDirectory == "/") {
            m_currentDirectoryParts = new string[] {""};
            m_currentDirectoryMatches = false;
        } else {
            m_currentDirectoryParts = m_currentDirectory.Split(Path.DirectorySeparatorChar);
            if (SelectionPattern != null) {
                string[] generation = Directory.GetDirectories(
                    Path.GetDirectoryName(m_currentDirectory),
                    SelectionPattern
                );
                m_currentDirectoryMatches = Array.IndexOf(generation, m_currentDirectory) >= 0;
            } else {
                m_currentDirectoryMatches = false;
            }
        }
       
        if (BrowserType == FileBrowserType.File || SelectionPattern == null) {
            m_directories = Directory.GetDirectories(m_currentDirectory);
            m_nonMatchingDirectories = new string[0];
        } else {
            m_directories = Directory.GetDirectories(m_currentDirectory, SelectionPattern);
            var nonMatchingDirectories = new List<string>();
            foreach (string directoryPath in Directory.GetDirectories(m_currentDirectory)) {
                if (Array.IndexOf(m_directories, directoryPath) < 0) {
                    nonMatchingDirectories.Add(directoryPath);
                }
            }
            m_nonMatchingDirectories = nonMatchingDirectories.ToArray();
            for (int i = 0; i < m_nonMatchingDirectories.Length; ++i) {
                int lastSeparator = m_nonMatchingDirectories[i].LastIndexOf(Path.DirectorySeparatorChar);
                m_nonMatchingDirectories[i] = m_nonMatchingDirectories[i].Substring(lastSeparator + 1);
            }
            Array.Sort(m_nonMatchingDirectories);
        }
       
        for (int i = 0; i < m_directories.Length; ++i) {
            m_directories[i] = m_directories[i].Substring(m_directories[i].LastIndexOf(Path.DirectorySeparatorChar) + 1);
        }
       
        if (BrowserType == FileBrowserType.Directory || SelectionPattern == null) {
            m_files = Directory.GetFiles(m_currentDirectory);
            m_nonMatchingFiles = new string[0];
        } else {
            m_files = Directory.GetFiles(m_currentDirectory, SelectionPattern);
            var nonMatchingFiles = new List<string>();
            foreach (string filePath in Directory.GetFiles(m_currentDirectory)) {
                if (Array.IndexOf(m_files, filePath) < 0) {
                    nonMatchingFiles.Add(filePath);
                }
            }
            m_nonMatchingFiles = nonMatchingFiles.ToArray();
            for (int i = 0; i < m_nonMatchingFiles.Length; ++i) {
                m_nonMatchingFiles[i] = Path.GetFileName(m_nonMatchingFiles[i]);
            }
            Array.Sort(m_nonMatchingFiles);
        }
        for (int i = 0; i < m_files.Length; ++i) {
            m_files[i] = Path.GetFileName(m_files[i]);
        }
        Array.Sort(m_files);
        BuildContent();
        m_newDirectory = null;
    }
   
    protected void BuildContent() {
        m_directoriesWithImages = new GUIContent[m_directories.Length];
        for (int i = 0; i < m_directoriesWithImages.Length; ++i) {
            m_directoriesWithImages[i] = new GUIContent(m_directories[i], DirectoryImage);
        }
        m_nonMatchingDirectoriesWithImages = new GUIContent[m_nonMatchingDirectories.Length];
        for (int i = 0; i < m_nonMatchingDirectoriesWithImages.Length; ++i) {
            m_nonMatchingDirectoriesWithImages[i] = new GUIContent(m_nonMatchingDirectories[i], DirectoryImage);
        }
        m_filesWithImages = new GUIContent[m_files.Length];
        for (int i = 0; i < m_filesWithImages.Length; ++i) {
            m_filesWithImages[i] = new GUIContent(m_files[i], FileImage);
        }
        m_nonMatchingFilesWithImages = new GUIContent[m_nonMatchingFiles.Length];
        for (int i = 0; i < m_nonMatchingFilesWithImages.Length; ++i) {
            m_nonMatchingFilesWithImages[i] = new GUIContent(m_nonMatchingFiles[i], FileImage);
        }
    }
   
    public void OnGUI() {
        GUILayout.BeginArea(
            m_screenRect,
            m_name,
            GUI.skin.window
        );
            GUILayout.BeginHorizontal();
                for (int parentIndex = 0; parentIndex < m_currentDirectoryParts.Length; ++parentIndex) {
                    if (parentIndex == m_currentDirectoryParts.Length - 1) {
                        GUILayout.Label(m_currentDirectoryParts[parentIndex], CentredText);
                    } else if (GUILayout.Button(m_currentDirectoryParts[parentIndex])) {
                        string parentDirectoryName = m_currentDirectory;
                        for (int i = m_currentDirectoryParts.Length - 1; i > parentIndex; --i) {
                            parentDirectoryName = Path.GetDirectoryName(parentDirectoryName);
                        }
                        SetNewDirectory(parentDirectoryName);
                    }
                }
                GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            m_scrollPosition = GUILayout.BeginScrollView(
                m_scrollPosition,
                false,
                true,
                GUI.skin.horizontalScrollbar,
                GUI.skin.verticalScrollbar,
                GUI.skin.box
            );
                m_selectedDirectory = GUILayoutx.SelectionList(
                    m_selectedDirectory,
                    m_directoriesWithImages,
                    DirectoryDoubleClickCallback
                );
                if (m_selectedDirectory > -1) {
                    m_selectedFile = m_selectedNonMatchingDirectory = -1;
                }
                m_selectedNonMatchingDirectory = GUILayoutx.SelectionList(
                    m_selectedNonMatchingDirectory,
                    m_nonMatchingDirectoriesWithImages,
                    NonMatchingDirectoryDoubleClickCallback
                );
                if (m_selectedNonMatchingDirectory > -1) {
                    m_selectedDirectory = m_selectedFile = -1;
                }
                GUI.enabled = BrowserType == FileBrowserType.File;
                m_selectedFile = GUILayoutx.SelectionList(
                    m_selectedFile,
                    m_filesWithImages,
                    FileDoubleClickCallback
                );
                GUI.enabled = true;
                if (m_selectedFile > -1) {
                    m_selectedDirectory = m_selectedNonMatchingDirectory = -1;
                }
                GUI.enabled = false;
                GUILayoutx.SelectionList(
                    -1,
                    m_nonMatchingFilesWithImages
                );
                GUI.enabled = true;
            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
		    UIData.loadHireRNA = GUILayout.Toggle(UIData.loadHireRNA, "HiRERNA model");
			UIData.readHetAtom = GUILayout.Toggle (UIData.readHetAtom, "Read HetAtm?");
			UIData.readWater = GUILayout.Toggle (UIData.readWater, "Read Water?");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Cancel", GUILayout.Width(buttonWidth))) {
                    m_callback(null);
                }
                if (BrowserType == FileBrowserType.File) {
                    GUI.enabled = m_selectedFile > -1;
                } else {
                    if (SelectionPattern == null) {
                        GUI.enabled = m_selectedDirectory > -1;
                    } else {
                        GUI.enabled =   m_selectedDirectory > -1 ||
                                        (
                                            m_currentDirectoryMatches &&
                                            m_selectedNonMatchingDirectory == -1 &&
                                            m_selectedFile == -1
                                        );
                    }
                }
                if (GUILayout.Button("Select", GUILayout.Width(buttonWidth))) {
                    if (BrowserType == FileBrowserType.File) {
                        m_callback(Path.Combine(m_currentDirectory, m_files[m_selectedFile]));
                    } else {
                        if (m_selectedDirectory > -1) {
                            m_callback(Path.Combine(m_currentDirectory, m_directories[m_selectedDirectory]));
                        } else {
                            m_callback(m_currentDirectory);
                        }
                    }
                }
                GUI.enabled = true;
            GUILayout.EndHorizontal();
        GUILayout.EndArea();
       
        if (Event.current.type == EventType.Repaint) {
            SwitchDirectoryNow();
        }
    }
   
    protected void FileDoubleClickCallback(int i) {
        if (BrowserType == FileBrowserType.File) {
            m_callback(Path.Combine(m_currentDirectory, m_files[i]));
        }
    }
   
    protected void DirectoryDoubleClickCallback(int i) {
        SetNewDirectory(Path.Combine(m_currentDirectory, m_directories[i]));
    }
   
    protected void NonMatchingDirectoryDoubleClickCallback(int i) {
        SetNewDirectory(Path.Combine(m_currentDirectory, m_nonMatchingDirectories[i]));
    }
   
}