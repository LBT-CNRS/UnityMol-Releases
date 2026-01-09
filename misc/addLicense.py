# updates the copyright information for all .cs files
# usage: call recursive_traversal, with the following parameters
# parent directory, old copyright text content, new copyright text content

import os

folder_to_apply = "Assets/Scripts/"
excluded_files = ["MiniJSON.cs","FortuneVoronoi","CustomRaycastMethod.cs"]

def update_source(filename, copyr, oldcopyright=None):
    utfstr = chr(0xef)+chr(0xbb)+chr(0xbf)
    fdata = open(filename,"r+").read()
    isUTF = False
    if (fdata.startswith(utfstr)):
        isUTF = True
        fdata = fdata[3:]
    if (oldcopyright != None):
        if (fdata.startswith(oldcopyright)):
            fdata = fdata[len(oldcopyright):]
    if "opyright" in fdata:
    	print("------> Contains copyright "+filename +"\n")
    if not (fdata.startswith(copyr)):
        print("updating "+filename)
        fdata = copyr + fdata
        if (isUTF):
            open(filename,"w").write(utfstr+fdata)
        else:
            open(filename,"w").write(fdata)

def recursive_traversal(folder,  copyr, oldcopyright=None):
    fns = os.listdir(folder)
    for fn in fns:
        fullfn = os.path.join(folder,fn)
        if (os.path.isdir(fullfn)):
            recursive_traversal(fullfn, copyr, oldcopyright)
        else:
            if (fullfn.endswith(".cs")):
                if( any(excluded in fullfn for excluded in excluded_files)):
                    print("Ignoring :" + fullfn)
                    continue
                try:
                    update_source(fullfn, copyr, oldcopyright)
                except Exception as inst:
                    print("! ! ! ! ! Failed to process file: "+fullfn+"\n"+str(inst))


if __name__ == "__main__":

    try:
        oldcright = open("oldcpyr.txt","r+").read()
    except FileNotFoundError as e:
        oldcright = None

    cright = open("miniLicense","r+").read()
    recursive_traversal("../"+folder_to_apply, cright, oldcright)
    exit()
