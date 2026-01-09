/*
    ================================================================================
    Copyright Centre National de la Recherche Scientifique (CNRS)
        Contributors and copyright holders :

        Xavier Martinez, 2017-2022
        Hubert Santuz, 2022-2026
        Marc Baaden, 2010-2026
        unitymol@gmail.com
        https://unity.mol3d.tech/

        This file is part of UnityMol, a general framework whose purpose is to provide
        a prototype for developing molecular graphics and scientific
        visualisation applications based on the Unity3D game engine.
        More details about UnityMol are provided at the following URL: https://unity.mol3d.tech/

        This program is free software: you can redistribute it and/or modify
        it under the terms of the GNU General Public License as published by
        the Free Software Foundation, either version 3 of the License, or
        (at your option) any later version.

        This program is distributed in the hope that it will be useful,
        but WITHOUT ANY WARRANTY; without even the implied warranty of
        MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
        GNU General Public License for more details.

        You should have received a copy of the GNU General Public License
        along with this program. If not, see <https://www.gnu.org/licenses/>.

        To help us with UnityMol development, we ask that you cite
        the research papers listed at https://unity.mol3d.tech/cite-us/.
    ================================================================================
*/
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UMol {
public class GraphManager {

    Dictionary<string, Texture2D> nameToTex = new Dictionary<string, Texture2D>();
    Dictionary<string, Plotter> plots = new Dictionary<string, Plotter>();

    int defaultTexSizeW = 300;
    int defaultTexSizeH = 300;

    //To be called by UI
    public Texture2D GetPlotTexture(string name) {
        if (nameToTex.ContainsKey(name)) {
            return nameToTex[name];
        }
        return null;
    }

    public void CreatePlot(string name, Color col, float minV = -100.0f, float maxV = 10.0f) {
        if (nameToTex.ContainsKey(name)) {
            DeletePlot(name);
        }

        Texture2D curTex = new Texture2D(defaultTexSizeW, defaultTexSizeH);
        nameToTex[name] = curTex;

        plots[name] = new Plotter(name, minV, maxV, col, ref curTex);
    }

    public void AddToPlot(string name, float v) {
        if (!plots.ContainsKey(name))
            return;
        plots[name].Add(v);
        if (plots[name].needUpdate) {
            plots[name].Draw();
        }
    }

    public void DeletePlot(string name) {
       if (!plots.Values.Any(value => value != null))
          return;
        if (name == null) 
          return;
        if (!plots.ContainsKey(name))
            return;
        GameObject.Destroy(nameToTex[name]);
        nameToTex.Remove(name);
        plots.Remove(name);
    }


}

/// <summary>
/// Plotter class for generating graphs
/// </summary>
public class Plotter
{

    public bool needUpdate = false;
    private Color plotColor = Color.green;

    private Texture2D grid;
    private int gridWidth = 354;
    private int gridHeight = 262;

    private float minValue;
    private float scale;
    private int floor;
    private int top;

    private Color[] buffer;
    private int[] data;
    private float[] raw_data;
    private int dataIndex = -1;
    private bool dataFull = false;

    private int zeroLine = -1;

    public float getLatestValue() {
        if (dataIndex != -1)
            return raw_data[dataIndex];
        else
            return 0.0f;
    }

    // Overload the plot constructor to use a texture passed as reference. This texture has to be allocated
    public Plotter(string name, float min, float max, Color plotColor, ref Texture2D renderTextureReference)
    {
        this.plotColor = plotColor;

        grid = renderTextureReference;

        gridWidth = grid.width;
        gridHeight = grid.height;


        buffer = new Color[gridWidth * gridHeight];
        raw_data = new float[gridWidth];
        data = new int[gridWidth];

        floor = 0;
        top = gridHeight;

        // Calculate verticle scale
        minValue = min;
        scale = (max - min) / top;

        if (max > 0 && min < 0) zeroLine = (int)((-minValue) / scale) + floor;

    }

    /// Add data to buffer.
    public void Add(float y)
    {
        // Move to next position in buffer
        dataIndex++;
        if (dataIndex == gridWidth) { dataIndex = 0; dataFull = true; }

        // Add value to the buffer.
        raw_data[dataIndex] = y;
        needUpdate = true;
    }

    public void SetRawData(int size, float[] data)
    {
        for (int i = 0; i < size; i++) {
            raw_data[i] = data[i];
        }
        for (int i = size; i < gridWidth; i++) {
            raw_data[i] = 0f;
        }
        dataIndex = 0;
        dataFull = true;
        needUpdate = true;
    }

    void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color col)
    {
        int dy = (int)(y1 - y0);
        int dx = (int)(x1 - x0);
        int stepx, stepy;

        if (dy < 0) {dy = -dy; stepy = -1;}
        else {stepy = 1;}
        if (dx < 0) {dx = -dx; stepx = -1;}
        else {stepx = 1;}
        dy <<= 1;
        dx <<= 1;

        float fraction = 0;

        tex.SetPixel(x0, y0, col);
        if (dx > dy) {
            fraction = dy - (dx >> 1);
            while (Mathf.Abs(x0 - x1) > 1) {
                if (fraction >= 0) {
                    y0 += stepy;
                    fraction -= dx;
                }
                x0 += stepx;
                fraction += dy;
                tex.SetPixel(x0, y0, col);
            }
        }
        else {
            fraction = dx - (dy >> 1);
            while (Mathf.Abs(y0 - y1) > 1) {
                if (fraction >= 0) {
                    x0 += stepx;
                    fraction -= dy;
                }
                y0 += stepy;
                fraction += dx;
                tex.SetPixel(x0, y0, col);
            }
        }
    }


    /// <summary>
    /// Draw the graph.
    /// </summary>
    public void Draw()
    {
//          float min = raw_data.Min();
//          float max = raw_data.Max();
//          float scale = (max - min) / gridHeight;

//          if (max > 0 && min < 0) zeroLine = (int)((-minValue) / scale);

        for (int i = 0; i < gridWidth; i++) {
            data[i] = (int) ((raw_data[i] - minValue) / scale);
        }

        grid.SetPixels(buffer);
        int x = grid.width;
        int previousX = x;
        int previousY;
        if (dataIndex == -1)
        {
            previousY = 0;
        }
        else
        {
            previousY = data[dataIndex];
        }

        // Plot Data in buffer back from current position back to zero
        for (int i = dataIndex - 1; i > 0; i--)
        {
            DrawLine(grid, previousX, previousY, x, data[i], plotColor);
            previousX = x;
            previousY = data[i];
            x--;
        }

        // Plot data in buffer from last position down to current position
        if (dataFull)
        {
            for (int i = gridWidth - 1; i >= dataIndex; i--)
            {
                DrawLine(grid, previousX, previousY, x, data[i], plotColor);
                previousX = x;
                previousY = data[i];
                x--;
            }
        }

        // Draw a line at Zero
        if (zeroLine > 0)
        {
            for (int i = 0; i < (gridWidth - 1); i++) grid.SetPixel(i, zeroLine, Color.yellow);
        }

        // Update texture with pixels
        grid.Apply(false);
        needUpdate = false;
    }

}


}
