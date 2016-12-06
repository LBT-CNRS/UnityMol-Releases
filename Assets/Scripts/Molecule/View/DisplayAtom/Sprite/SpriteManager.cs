/// @file SpriteManager.cs
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
/// $Id: SpriteManager.cs 213 2013-04-06 21:13:42Z baaden $
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

//-----------------------------------------------------------------
//  SpriteManager v0.633 (8-02-2009)
//  Copyright 2009 Brady Wright and Above and Beyond Software
//  All rights reserved
//-----------------------------------------------------------------
// A class to allow the drawing of multiple "quads" as part of a
// single aggregated mesh so as to achieve multiple, independently
// moving objects using a single draw call.
//-----------------------------------------------------------------


using UnityEngine;
using System.Collections;

//-----------------------------------------------------------------
// Describes a UV animation
//-----------------------------------------------------------------
//  NOTE: Currently, you should assign at least two frames to an
//  animation, or else you can expect problems!
//-----------------------------------------------------------------
public class UVAnimation
{
    protected Vector2[] frames;      // Array of UV coordinates (for quads) defining the frames of an animation
    
    // Animation state vars:
    protected int curFrame = 0;      // The current frame
    protected int stepDir = 1;            // The direction we're currently playing the animation (1=forwards (default), -1=backwards)
    protected int numLoops = 0;      // Number of times we've looped since last animation

    public string name;        // The name of the 
    public int loopCycles = 0;            // How many times to loop the animation (-1 loop infinitely)
    public bool loopReverse = false;                // Reverse the play direction when the end of the animation is reached? (if true, a loop iteration isn't counted until we return to the beginning)
    public float framerate;       // The rate in frames per second at which to play the animation


    // Resets all the animation state vars to ready the object
    // for playing anew:
    public void Reset()
    {
        curFrame = 0;
        stepDir = 1;
        numLoops = 0;
    }

    // Sets the stepDir to -1 and sets the current frame to the end
    // so that the animation plays in reverse
    public void PlayInReverse()
    {
        stepDir = -1;
        curFrame = frames.Length - 1;
    }

    // Stores the UV of the next frame in 'uv', returns false if
    // we've reached the end of the animation (this will never
    // happen if it is set to loop infinitely)
    public bool GetNextFrame(ref Vector2 uv)
    {
        // See if we can advance to the next frame:
        if((curFrame + stepDir) >= frames.Length || (curFrame + stepDir) < 0)
        {
            // See if we need to loop (if we're reversing, we don't loop until we get back to the beginning):
            if( stepDir>0 && loopReverse )
            {
                stepDir = -1;   // Reverse playback direction
                curFrame += stepDir;

                uv = frames[curFrame];
            }else
            {
                // See if we can loop:
                if (numLoops + 1 > loopCycles && loopCycles != -1)
                    return false;
                else
                {   // Loop the animation:
                    ++numLoops;

                    if (loopReverse)
                    {
                        stepDir *= -1;
                        curFrame += stepDir;
                    }
                    else
                        curFrame = 0;

                    uv = frames[curFrame];
                }
            }
        }else
        {
            curFrame += stepDir;
            uv = frames[curFrame];
        }

        return true;
    }

    // Constructs an array of UV coordinates based upon the info
    // supplied.
    //
    // start    -   The UV of the lower-left corner of the first
    //        cell
    // cellSize -    width and height, in UV space, of each cell
    // cols  -    Number of columns in the grid
    // rows  -    Number of rows in the grid
    // totalCells-  Total number of cells in the grid (left-to-right,
    //        top-to-bottom ordering is assumed, just like reading
    //        English).
    // fps    - Framerate (frames per second)
    public Vector2[] BuildUVAnim(Vector2 start, Vector2 cellSize, int cols, int rows, int totalCells, float fps)
    {
        int cellCount = 0;

        frames = new Vector2[totalCells];
        framerate = fps;

        frames[0] = start;

        for(int row=0; row < rows; ++row)
        {
            for(int col=0; col<cols && cellCount < totalCells; ++col)
            {
                frames[cellCount].x = start.x + cellSize.x * ((float)col);
                frames[cellCount].y = start.y - cellSize.y * ((float)row);

                ++cellCount;
            }
        }

        return frames;
    }

    // Assigns the specified array of UV coordinates to the
    // animation, replacing its current contents
    public void SetAnim(Vector2[] anim)
    {
        frames = anim;
    }

    // Appends the specified array of UV coordinates to the
    // existing animation
    public void AppendAnim(Vector2[] anim)
    {
        Vector2[] tempFrames = frames;

        frames = new Vector2[frames.Length + anim.Length];
        tempFrames.CopyTo(frames, 0);
        anim.CopyTo(frames, tempFrames.Length);
    }
}




//-----------------------------------------------------------------
// Holds a single mesh object which is composed of an arbitrary
// number of quads that all use the same material, allowing
// multiple, independently moving objects to be drawn on-screen
// while using only a single draw call.
//-----------------------------------------------------------------
public class SpriteManager : MonoBehaviour 
{
    // In which plane should we create the sprites?
    public enum SPRITE_PLANE
    {
        XY,
        XZ,
        YZ
    };

    // Which way to wind polygons?
    public enum WINDING_ORDER
    {
        CCW,        // Counter-clockwise
        CW      // Clockwise
    };

    public Material material;            // The material to use for the sprites
    public int allocBlockSize;        // How many sprites to allocate space for at a time. ex: if set to 10, 10 new sprite blocks will be allocated at a time. Once all of these are used, 10 more will be allocated, and so on...
    public SPRITE_PLANE plane;        // The plane in which to create the sprites
    public WINDING_ORDER winding=WINDING_ORDER.CCW; // Which way to wind polygons
    public bool autoUpdateBounds = false;   // Automatically recalculate the bounds of the mesh when vertices change?

    protected ArrayList availableBlocks = new ArrayList(); // Array of references to sprites which are currently not in use
    protected bool vertsChanged = false;    // Have changes been made to the vertices of the mesh since the last frame?
    protected bool uvsChanged = false;    // Have changes been made to the UVs of the mesh since the last frame?
    protected bool colorsChanged = false;   // Have the colors changed?
    protected bool vertCountChanged = false;// Has the number of vertices changed?
    protected bool updateBounds = false;    // Update the mesh bounds?
    protected Sprite[] sprites;    // Array of all sprites (the offset of the vertices corresponding to each sprite should be found simply by taking the sprite's index * 4 (4 verts per sprite).
    protected ArrayList activeBlocks = new ArrayList(); // Array of references to all the currently active (non-empty) sprites
    protected ArrayList activeBillboards = new ArrayList(); // Array of references to all the *active* sprites which are to be rendered as billboards
    protected ArrayList playingAnimations = new ArrayList();// Array of references to all the sprites that are currently playing animation
    protected ArrayList spriteDrawOrder = new ArrayList();  // Array of indices of sprite objects stored in the order they are to be drawn (corresponding to the position of their vertex indices in the triIndices list)  Allows us to keep track of where a given Sprite is in the drawing order (triIndices)
    protected SpriteDrawLayerComparer drawOrderComparer = new SpriteDrawLayerComparer(); // Used to sort our draw order array
    protected float boundUpdateInterval;    // Interval, in seconds, to update the mesh bounds

    protected MeshFilter meshFilter;
    protected MeshRenderer meshRenderer;
    protected Mesh mesh;                    // Reference to our mesh (contained in the MeshFilter)

    protected Vector3[] vertices;         // The vertices of our mesh
    protected int[] triIndices;    // Indices into the vertex array
    protected Vector2[] UVs;                // UV coordinates
    protected Color[] colors;            // Color values
    //protected Vector3[] normals;      // Normals

    // Working vars:
    protected int i;
    protected Sprite tempSprite = null;
    protected float animTimeElapsed;

    //--------------------------------------------------------------
    // Utility functions:
    //--------------------------------------------------------------

    // Converts pixel-space values to UV-space scalar values
    // according to the currently assigned material.
    // NOTE: This is for converting widths and heights-not
    // coordinates (which have reversed Y-coordinates).
    // For coordinates, use PixelCoordToUVCoord()!
    public Vector2 PixelSpaceToUVSpace(Vector2 xy)
    {
        Texture t = material.GetTexture("_MainTex");
		
		if(t)
        	return new Vector2(xy.x / ((float)t.width), xy.y / ((float)t.height));
        else
        	return new Vector2(1.0f,1.0f);
        	
                //	return new Vector2(xy.x / ((float)t.width), xy.y / ((float)t.height));

    }

    // Converts pixel-space values to UV-space scalar values
    // according to the currently assigned material.
    // NOTE: This is for converting widths and heights-not
    // coordinates (which have reversed Y-coordinates).
    // For coordinates, use PixelCoordToUVCoord()!
    public Vector2 PixelSpaceToUVSpace(int x, int y)
    {
        return PixelSpaceToUVSpace(new Vector2((float)x, (float)y));
    }

    // Converts pixel coordinates to UV coordinates according to
    // the currently assigned material.
    // NOTE: This is for converting coordinates and will reverse
    // the Y component accordingly.  For converting widths and
    // heights, use PixelSpaceToUVSpace()!
    public Vector2 PixelCoordToUVCoord(Vector2 xy)
    {
        Vector2 p = PixelSpaceToUVSpace(xy);
        p.y = 1.0f - p.y;
        return p;
    }

    // Converts pixel coordinates to UV coordinates according to
    // the currently assigned material.
    // NOTE: This is for converting coordinates and will reverse
    // the Y component accordingly.  For converting widths and
    // heights, use PixelSpaceToUVSpace()!
    public Vector2 PixelCoordToUVCoord(int x, int y)
    {
        return PixelCoordToUVCoord(new Vector2((float)x, (float)y));
    }

    //--------------------------------------------------------------
    // End utility functions
    //--------------------------------------------------------------

    void Awake()
    {
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();

        meshFilter = (MeshFilter)GetComponent(typeof(MeshFilter));
        meshRenderer = (MeshRenderer)GetComponent(typeof(MeshRenderer));

        meshRenderer.GetComponent<Renderer>().material = material;
        mesh = meshFilter.mesh;

        // Create our first batch of sprites:
        EnlargeArrays(allocBlockSize);

        // Move the object to the origin so the objects drawn will not
        // be offset from the objects they are intended to represent.
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }

    // Allocates initial arrays
    protected void InitArrays()
    {
        sprites = new Sprite[1];
        sprites[0] = new Sprite();
        vertices = new Vector3[4];
        UVs = new Vector2[4];
        colors = new Color[4];
        triIndices = new int[6];
    }

    // Enlarges the sprite array by the specified count and also resizes
    // the UV and vertex arrays by the necessary corresponding amount.
    // Returns the index of the first newly allocated element
    // (ex: if the sprite array was already 10 elements long and is 
    // enlarged by 10 elements resulting in a total length of 20, 
    // EnlargeArrays() will return 10, indicating that element 10 is the 
    // first of the newly allocated elements.)
    protected int EnlargeArrays(int count)
    {
        int firstNewElement;

        if (sprites == null)
        {
            InitArrays();
            firstNewElement = 0;
            count = count - 1;  // Allocate one less since InitArrays already allocated one sprite for us
        }
        else
            firstNewElement = sprites.Length;

        // Resize sprite array:
        Sprite[] tempSprites = sprites;
        sprites = new Sprite[sprites.Length + count];
        tempSprites.CopyTo(sprites, 0);

        // Vertices:
        Vector3[] tempVerts = vertices;
        vertices = new Vector3[vertices.Length + count*4];
        tempVerts.CopyTo(vertices, 0);
        
        // UVs:
        Vector2[] tempUVs = UVs;
        UVs = new Vector2[UVs.Length + count*4];
        tempUVs.CopyTo(UVs, 0);

        // Colors:
        Color[] tempColors = colors;
        colors = new Color[colors.Length + count * 4];
        tempColors.CopyTo(colors, 0);

        // Triangle indices:
        int[] tempTris = triIndices;
        triIndices = new int[triIndices.Length + count*6];
        tempTris.CopyTo(triIndices, 0);

        // Inform existing sprites of the new vertex and UV buffers:
        for (int i = 0; i < firstNewElement; ++i)
        {
            sprites[i].SetBuffers(vertices, UVs);
        }

        // Setup the newly-added sprites and Add them to the list of available 
        // sprite blocks. Also initialize the triangle indices while we're at it:
        for (int i = firstNewElement; i < sprites.Length; ++i)
        {
            // Create and setup sprite:

            sprites[i] = new Sprite();
            sprites[i].index = i;
            sprites[i].manager = this;

            sprites[i].SetBuffers(vertices, UVs);

            // Setup indices of the sprite's vertices in the vertex buffer:
            sprites[i].mv1 = i * 4 + 0;
            sprites[i].mv2 = i * 4 + 1;
            sprites[i].mv3 = i * 4 + 2;
            sprites[i].mv4 = i * 4 + 3;

            // Setup the indices of the sprite's UV entries in the UV buffer:
            sprites[i].uv1 = i * 4 + 0;
            sprites[i].uv2 = i * 4 + 1;
            sprites[i].uv3 = i * 4 + 2;
            sprites[i].uv4 = i * 4 + 3;

            // Setup the indices to the color values:
            sprites[i].cv1 = i * 4 + 0;
            sprites[i].cv2 = i * 4 + 1;
            sprites[i].cv3 = i * 4 + 2;
            sprites[i].cv4 = i * 4 + 3;

            // Setup the default color:
            sprites[i].SetColor(Color.white);

            // Add as an available sprite:
            availableBlocks.Add(sprites[i]);

            // Init triangle indices:
            if(winding == WINDING_ORDER.CCW)
            {   // Counter-clockwise winding
                triIndices[i * 6 + 0] = i * 4 + 0;  //    0_ 2            0 ___ 3
                triIndices[i * 6 + 1] = i * 4 + 1;  //  | /      Verts:  |   /|
                triIndices[i * 6 + 2] = i * 4 + 3;  // 1|/                1|/__|2

                triIndices[i * 6 + 3] = i * 4 + 3;  //      3
                triIndices[i * 6 + 4] = i * 4 + 1;  //   /|
                triIndices[i * 6 + 5] = i * 4 + 2;  // 4/_|5
            }
            else
            {   // Clockwise winding
                triIndices[i * 6 + 0] = i * 4 + 0;  //    0_ 1            0 ___ 3
                triIndices[i * 6 + 1] = i * 4 + 3;  //  | /      Verts:  |   /|
                triIndices[i * 6 + 2] = i * 4 + 1;  // 2|/                1|/__|2

                triIndices[i * 6 + 3] = i * 4 + 3;  //      3
                triIndices[i * 6 + 4] = i * 4 + 2;  //   /|
                triIndices[i * 6 + 5] = i * 4 + 1;  // 5/_|4
            }

            // Add the index of this sprite to the draw order list
            spriteDrawOrder.Add(sprites[i]);
        }

        vertsChanged = true;
        uvsChanged = true;
        colorsChanged = true;
        vertCountChanged = true;

        return firstNewElement;
    }

    // Adds a sprite to the manager at the location and rotation of the client 
    // GameObject and with its transform.  Returns a reference to the new sprite
    // Width and height are in world space units
    // leftPixelX and bottomPixelY- the bottom-left position of the desired portion of the texture, in pixels
    // pixelWidth and pixelHeight - the dimensions of the desired portion of the texture, in pixels
    public Sprite AddSprite(GameObject client, float width, float height, int leftPixelX, int bottomPixelY, int pixelWidth, int pixelHeight, bool billboarded)
    {
        return AddSprite(client, width, height, PixelCoordToUVCoord(leftPixelX, bottomPixelY), PixelSpaceToUVSpace(pixelWidth, pixelHeight), billboarded);
    }

    // Adds a sprite to the manager at the location and rotation of the client 
    // GameObject and with its transform.  Returns a reference to the new sprite
    // Width and height are in world space units
    // lowerLeftUV - the UV coordinate for the upper-left corner
    // UVDimensions - the distance from lowerLeftUV to place the other UV coords
    public Sprite AddSprite(GameObject client, float width, float height, Vector2 lowerLeftUV, Vector2 UVDimensions, bool billboarded)
    {
        int spriteIndex;

        // Get an available sprite:
        if (availableBlocks.Count < 1)
            EnlargeArrays(allocBlockSize);  // If we're out of available sprites, allocate some more:

        // Use a sprite from the list of available blocks:
        spriteIndex = ((Sprite)availableBlocks[0]).index;
        availableBlocks.RemoveAt(0);    // Now that we're using this one, remove it from the available list

        // Assign the new sprite:
        Sprite newSprite = sprites[spriteIndex];
        newSprite.client = client;
        newSprite.lowerLeftUV = lowerLeftUV;
        newSprite.uvDimensions = UVDimensions;

        switch(plane)
        {
            case SPRITE_PLANE.XY:
                newSprite.SetSizeXY(width, height);
                break;
            case SPRITE_PLANE.XZ:
                newSprite.SetSizeXZ(width, height);
                break;
            case SPRITE_PLANE.YZ:
                newSprite.SetSizeYZ(width, height);
                break;
            default:
                newSprite.SetSizeXY(width, height);
                break;
        }

        // Save this to an active list now that it is in-use:
        if(billboarded)   
        {
            newSprite.billboarded = true;
            activeBillboards.Add(newSprite);
        }
        else
            activeBlocks.Add(newSprite);

        // Transform the sprite:
        newSprite.Transform();

        // Setup the UVs:
        UVs[newSprite.uv1] = lowerLeftUV + Vector2.up * UVDimensions.y;  // Upper-left
        UVs[newSprite.uv2] = lowerLeftUV;                         // Lower-left
        UVs[newSprite.uv3] = lowerLeftUV + Vector2.right * UVDimensions.x;// Lower-right
        UVs[newSprite.uv4] = lowerLeftUV + UVDimensions;                     // Upper-right

        // Set our flags:
        vertsChanged = true;
        uvsChanged = true;

        return newSprite;
    }

    public void SetBillboarded(Sprite sprite)
    {
        // Make sure the sprite isn't in the active list
        // or else it'll get handled twice:
        activeBlocks.Remove(sprite);
        activeBillboards.Add(sprite);
    }

    public void RemoveSprite(Sprite sprite)
    {
        sprite.SetSizeXY(0,0);
        sprite.v1 = Vector3.zero;
        sprite.v2 = Vector3.zero;
        sprite.v3 = Vector3.zero;
        sprite.v4 = Vector3.zero;

        vertices[sprite.mv1] = sprite.v1;
        vertices[sprite.mv2] = sprite.v2;
        vertices[sprite.mv3] = sprite.v3;
        vertices[sprite.mv4] = sprite.v4;

        // Remove the sprite from the billboarded list
        // since that list should only contain active
        // sprites:
        if (sprite.billboarded)
            activeBillboards.Remove(sprite);
        else
            activeBlocks.Remove(sprite);

        // Clean the sprite's settings:
        sprite.Clear();

        availableBlocks.Add(sprite);

        vertsChanged = true;
    }

    public void HideSprite(Sprite sprite)
    {
        // Remove the sprite from the billboarded list
        // since that list should only contain sprites
        // we intend to transform:
        if (sprite.billboarded)
            activeBillboards.Remove(sprite);
        else
            activeBlocks.Remove(sprite);

        sprite.m_hidden___DoNotAccessExternally = true;

        vertices[sprite.mv1] = Vector3.zero;
        vertices[sprite.mv2] = Vector3.zero;
        vertices[sprite.mv3] = Vector3.zero;
        vertices[sprite.mv4] = Vector3.zero;

        vertsChanged = true;
    }

    public void ShowSprite(Sprite sprite)
    {
        // Only show the sprite if it has a client:
        if(sprite.client == null)
            return;

        if (!sprite.m_hidden___DoNotAccessExternally)
            return;

        sprite.m_hidden___DoNotAccessExternally = false;

        // Update the vertices:
        sprite.Transform();

        if (sprite.billboarded)
            activeBillboards.Add(sprite);
        else
            activeBlocks.Add(sprite);

        vertsChanged = true;
    }

    // Moves the specified sprite to the end of the drawing order
    public void MoveToFront(Sprite s)
    {
        int[] indices = new int[6];
        int offset = spriteDrawOrder.IndexOf(s) * 6;

        if (offset < 0)
            return;

        // Save our indices:
        indices[0] = triIndices[offset];
        indices[1] = triIndices[offset + 1];
        indices[2] = triIndices[offset + 2];
        indices[3] = triIndices[offset + 3];
        indices[4] = triIndices[offset + 4];
        indices[5] = triIndices[offset + 5];

        // Shift all indices from here forward down 6 slots (each sprite occupies 6 index slots):
        for (int i = offset; i < triIndices.Length - 6; i += 6)
        {
            triIndices[i] = triIndices[i+6];
            triIndices[i+1] = triIndices[i+7];
            triIndices[i+2] = triIndices[i+8];
            triIndices[i+3] = triIndices[i+9];
            triIndices[i+4] = triIndices[i+10];
            triIndices[i+5] = triIndices[i+11];

            spriteDrawOrder[i / 6] = spriteDrawOrder[i / 6 + 1];
        }

        // Place our desired index value at the end:
        triIndices[triIndices.Length - 6] = indices[0];
        triIndices[triIndices.Length - 5] = indices[1];
        triIndices[triIndices.Length - 4] = indices[2];
        triIndices[triIndices.Length - 3] = indices[3];
        triIndices[triIndices.Length - 2] = indices[4];
        triIndices[triIndices.Length - 1] = indices[5];

        // Update the sprite's index offset:
        spriteDrawOrder[spriteDrawOrder.Count - 1] = s.index;

        vertCountChanged = true;
    }

    // Moves the specified sprite to the start of the drawing order
    public void MoveToBack(Sprite s)
    {
        int[] indices = new int[6];
        int offset = spriteDrawOrder.IndexOf(s) * 6;

        if (offset < 0)
            return;

        // Save our indices:
        indices[0] = triIndices[offset];
        indices[1] = triIndices[offset + 1];
        indices[2] = triIndices[offset + 2];
        indices[3] = triIndices[offset + 3];
        indices[4] = triIndices[offset + 4];
        indices[5] = triIndices[offset + 5];

        // Shift all indices from here back up 6 slots (each sprite occupies 6 index slots):
        for(int i=offset; i>5; i-=6)
        {
            triIndices[i] = triIndices[i-6];
            triIndices[i+1] = triIndices[i-5];
            triIndices[i+2] = triIndices[i-4];
            triIndices[i+3] = triIndices[i-3];
            triIndices[i+4] = triIndices[i-2];
            triIndices[i+5] = triIndices[i-1];

            spriteDrawOrder[i / 6] = spriteDrawOrder[i / 6 - 1];
        }

        // Place our desired index value at the beginning:
        triIndices[0] = indices[0];
        triIndices[1] = indices[1];
        triIndices[2] = indices[2];
        triIndices[3] = indices[3];
        triIndices[4] = indices[4];
        triIndices[5] = indices[5];

        // Update the sprite's index offset:
        spriteDrawOrder[0] = s.index;

        vertCountChanged = true;
    }

    // Moves the first sprite in front of the second sprite by
    // placing it later in the draw order. If the sprite is already
    // in front of the reference sprite, nothing is changed:
    public void MoveInfrontOf(Sprite toMove, Sprite reference)
    {
        int[] indices = new int[6];
        int offset = spriteDrawOrder.IndexOf(toMove) * 6;
        int refOffset = spriteDrawOrder.IndexOf(reference) * 6;

        if (offset < 0)
            return;

        // Check to see if the sprite is already in front:
        if(offset > refOffset)
            return;

        // Save our indices:
        indices[0] = triIndices[offset];
        indices[1] = triIndices[offset + 1];
        indices[2] = triIndices[offset + 2];
        indices[3] = triIndices[offset + 3];
        indices[4] = triIndices[offset + 4];
        indices[5] = triIndices[offset + 5];

        // Shift all indices from here to the reference sprite down 6 slots (each sprite occupies 6 index slots):
        for (int i = offset; i < refOffset; i += 6)
        {
            triIndices[i] = triIndices[i+6];
            triIndices[i+1] = triIndices[i+7];
            triIndices[i+2] = triIndices[i+8];
            triIndices[i+3] = triIndices[i+9];
            triIndices[i+4] = triIndices[i+10];
            triIndices[i+5] = triIndices[i+11];

            spriteDrawOrder[i / 6] = spriteDrawOrder[i / 6 + 1];
        }

        // Place our desired index value at the destination:
        triIndices[refOffset] = indices[0];
        triIndices[refOffset+1] = indices[1];
        triIndices[refOffset+2] = indices[2];
        triIndices[refOffset+3] = indices[3];
        triIndices[refOffset+4] = indices[4];
        triIndices[refOffset+5] = indices[5];

        // Update the sprite's index offset:
        spriteDrawOrder[refOffset/6] = toMove.index;

        vertCountChanged = true;
    }

    // Moves the first sprite behind the second sprite by
    // placing it earlier in the draw order. If the sprite
    // is already behind, nothing is done:
    public void MoveBehind(Sprite toMove, Sprite reference)
    {
        int[] indices = new int[6];
        int offset = spriteDrawOrder.IndexOf(toMove) * 6;
        int refOffset = spriteDrawOrder.IndexOf(reference) * 6;

        if (offset < 0)
            return;

        // Check to see if the sprite is already behind:
        if(offset < refOffset)
            return;

        // Save our indices:
        indices[0] = triIndices[offset];
        indices[1] = triIndices[offset + 1];
        indices[2] = triIndices[offset + 2];
        indices[3] = triIndices[offset + 3];
        indices[4] = triIndices[offset + 4];
        indices[5] = triIndices[offset + 5];

        // Shift all indices from here to the reference sprite up 6 slots (each sprite occupies 6 index slots):
        for (int i = offset; i > refOffset; i -= 6)
        {
            triIndices[i] = triIndices[i-6];
            triIndices[i+1] = triIndices[i-5];
            triIndices[i+2] = triIndices[i-4];
            triIndices[i+3] = triIndices[i-3];
            triIndices[i+4] = triIndices[i-2];
            triIndices[i+5] = triIndices[i-1];

            spriteDrawOrder[i / 6] = spriteDrawOrder[i / 6 - 1];
        }

        // Place our desired index value at the destination:
        triIndices[refOffset] = indices[0];
        triIndices[refOffset+1] = indices[1];
        triIndices[refOffset+2] = indices[2];
        triIndices[refOffset+3] = indices[3];
        triIndices[refOffset+4] = indices[4];
        triIndices[refOffset+5] = indices[5];

        // Update the sprite's index offset:
        spriteDrawOrder[refOffset/6] = toMove.index;

        vertCountChanged = true;
    }

    // Rebuilds the drawing order based upon the drawing order buffer
    public void SortDrawingOrder()
    {
        Sprite s;

        spriteDrawOrder.Sort(drawOrderComparer);

        // Now reconstitute the triIndices in the order we want:
        if (winding == WINDING_ORDER.CCW)
        {
            for (int i = 0; i < spriteDrawOrder.Count; ++i)
            {
                s = (Sprite) spriteDrawOrder[i];

                // Counter-clockwise winding
                triIndices[i * 6 + 0] = s.mv1;    //    0_ 2            1 ___ 4
                triIndices[i * 6 + 1] = s.mv2;    //  | /      Verts:  |   /|
                triIndices[i * 6 + 2] = s.mv4;    // 1|/                2|/__|3

                triIndices[i * 6 + 3] = s.mv4;    //      3
                triIndices[i * 6 + 4] = s.mv2;    //   /|
                triIndices[i * 6 + 5] = s.mv3;    // 4/_|5
            }
        }
        else
        {
            for (int i = 0; i < spriteDrawOrder.Count; ++i)
            {
                s = (Sprite)spriteDrawOrder[i];

                // Clockwise winding
                triIndices[i * 6 + 0] = s.mv1;    //    0_ 1            1 ___ 4
                triIndices[i * 6 + 1] = s.mv2;    //  | /      Verts:  |   /|
                triIndices[i * 6 + 2] = s.mv4;    // 2|/                2|/__|3

                triIndices[i * 6 + 3] = s.mv4;    //      3
                triIndices[i * 6 + 4] = s.mv2;    //   /|
                triIndices[i * 6 + 5] = s.mv3;    // 5/_|4
            }
        }

        vertCountChanged = true;
    }

    public void AnimateSprite(Sprite s)
    {
        // Add this sprite to our playingAnimation list:
        playingAnimations.Add(s);
    }

    public void StopAnimation(Sprite s)
    {
        playingAnimations.Remove(s);
    }

    public Sprite GetSprite(int i)
    {
        if (i < sprites.Length)
            return sprites[i];
        else
            return null;
    }

    // Updates the vertices of a sprite based on the transform
    // of its client GameObject
    public void Transform(Sprite sprite)
    {
        sprite.Transform();

        vertsChanged = true;
    }

    // Updates the vertices of a sprite such that it is oriented
    // more or less toward the camera
    public void TransformBillboarded(Sprite sprite)
    {
        Vector3 pos = sprite.clientTransform.position;
        Transform t = Camera.main.transform;

        vertices[sprite.mv1] = pos + t.TransformDirection(sprite.v1);
        vertices[sprite.mv2] = pos + t.TransformDirection(sprite.v2);
        vertices[sprite.mv3] = pos + t.TransformDirection(sprite.v3);
        vertices[sprite.mv4] = pos + t.TransformDirection(sprite.v4);

        vertsChanged = true;
    }

    // Informs the SpriteManager that some vertices have changed position
    // and the mesh needs to be reconstructed accordingly
    public void UpdatePositions()
    {
        vertsChanged = true;
    }

    // Updates the UVs of the specified sprite and copies the new values
    // into the mesh object.
    public void UpdateUV(Sprite sprite)
    {
        UVs[sprite.uv1] = sprite.lowerLeftUV + Vector2.up * sprite.uvDimensions.y;  // Upper-left
        UVs[sprite.uv2] = sprite.lowerLeftUV;                              // Lower-left
        UVs[sprite.uv3] = sprite.lowerLeftUV + Vector2.right * sprite.uvDimensions.x;// Lower-right
        UVs[sprite.uv4] = sprite.lowerLeftUV + sprite.uvDimensions;     // Upper-right
        
        uvsChanged = true;
    }

    // Updates the color values of the specified sprite and copies the
    // new values into the mesh object.
    public void UpdateColors(Sprite sprite)
    {
        colors[sprite.cv1] = sprite.color;
        colors[sprite.cv2] = sprite.color;
        colors[sprite.cv3] = sprite.color;
        colors[sprite.cv4] = sprite.color;

        colorsChanged = true;
    }

    // Instructs the manager to recalculate the bounds of the mesh
    public void UpdateBounds()
    {
        updateBounds = true;
    }

    // Schedules a recalculation of the mesh bounds to occur at a
    // regular interval (given in seconds):
    public void ScheduleBoundsUpdate(float seconds)
    {
        boundUpdateInterval = seconds;
        InvokeRepeating("UpdateBounds", seconds, seconds);
    }

    // Cancels any previously scheduled bounds recalculations:
    public void CancelBoundsUpdate()
    {
        CancelInvoke("UpdateBounds");
    }

    // Use this for initialization
    void Start () 
    {
    
    }
    
    // LateUpdate is called once per frame
    virtual public void LateUpdate () 
    {
        // See if we have any active animations:
        if(playingAnimations.Count > 0)
        {
            animTimeElapsed = Time.deltaTime;

            for(i=0; i<playingAnimations.Count; ++i)
            {
                tempSprite = (Sprite)playingAnimations[i];

                // Step the animation, and if it has finished
                // playing, remove it from the playing list:
                if (!tempSprite.StepAnim(animTimeElapsed))
                    playingAnimations.Remove(tempSprite);
            }

            uvsChanged = true;
        }

        // Were changes made to the mesh since last time?
        if (vertCountChanged)
        {
            vertCountChanged = false;
            colorsChanged = false;
            vertsChanged = false;
            uvsChanged = false;
            updateBounds = false;

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.uv = UVs;
            mesh.colors = colors;
            //mesh.normals = normals;
            mesh.triangles = triIndices;
        }
        else
        {
            if (vertsChanged)
            {
                vertsChanged = false;

                if (autoUpdateBounds)
                    updateBounds = true;

                mesh.vertices = vertices;
            }

            if (updateBounds)
            {
                mesh.RecalculateBounds();
                updateBounds = false;
            }

            if (colorsChanged)
            {
                colorsChanged = false;

                mesh.colors = colors;
            }

            if (uvsChanged)
            {
                uvsChanged = false;
                mesh.uv = UVs;
            }
        }
    }
}
 