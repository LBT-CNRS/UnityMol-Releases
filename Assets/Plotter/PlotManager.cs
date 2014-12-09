using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlotManager : MonoBehaviour {

    public int depth = 2;
    public Texture2D grid;

    private Dictionary<String, Plotter> plots = new Dictionary<String, Plotter>();
    private static PlotManager instance;

    /// <summary>
    /// Instance of object
    /// </summary>
    public static PlotManager Instance
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        instance = this;
    }

    void OnGUI()
    {

        GUI.depth = depth;

        // If we have graphs then show them
        if (plots.Count > 0)
        {
            foreach (KeyValuePair<String, Plotter> p in plots)
            {
                if (!p.Value.child) 
                {
                    p.Value.Draw();
                }
            }
        }
    }

    /// <summary>
    /// Add a value to plot graph
    /// </summary>
    /// <param name="plotName"></param>
    /// <param name="value"></param>
    public void PlotAdd(String plotName, float value)
    {
        if (plots.ContainsKey(plotName)) plots[plotName].Add(value);
    }

    /// <summary>
    /// Instantiate a new new plot graph
    /// </summary>
    /// <param name="plotName"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public void PlotCreate(String plotName, float min, float max, Color plotColor, Vector2 pos)
    {
        if (!plots.ContainsKey(plotName))
        {
            plots.Add(plotName, new Plotter(plotName, grid, min, max, plotColor, pos));
        }
    }

    /// <summary>
    /// Create child plotter
    /// </summary>
    /// <param name="plotName"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="plotColor"></param>
    /// <param name="parent"></param>
    public void PlotCreate(String plotName, float min, float max, Color plotColor, String parentName)
    {
        if (!plots.ContainsKey(plotName) && plots.ContainsKey(parentName))
        {
            plots.Add(plotName, new Plotter(plotName, grid, min, max, plotColor, plots[parentName]));
        }
    }

    public void PlotCreate(String plotName, Color plotColor, String parentName)
    {
        if (!plots.ContainsKey(plotName) && plots.ContainsKey(parentName))
            plots.Add(plotName, new Plotter(plotName, grid, plotColor, plots[parentName]));
    }

    /// <summary>
    /// Plotter class for generating graphs
    /// </summary>
    public class Plotter
    {

        public Boolean child = false;
        private Plotter Parent;
        private String name = "";
        private Color plotColor = Color.green;
        Rect gridRect;

        private Texture2D grid;
        private int gridWidth = 354;
        private int gridHeight = 262;

        private float minValue;
        private float maxValue;
        private float scale;
        private int floor;
        private int top;

        private Color[] buffer;
        private int[] data;
        private int dataIndex = -1;
        private bool dataFull = false;

        private int zeroLine = -1;

        private Dictionary<String, Plotter> children = new Dictionary<string, Plotter>();

        public Plotter(String name, Texture2D blankGraph, Color plotColor, Plotter parent)
        {
            InitPlotterChild(name, blankGraph, parent.minValue, parent.maxValue, plotColor, parent);
        }

        public Plotter(String name, Texture2D blankGraph, float min, float max, Color plotColor, Plotter parent)
        {
            InitPlotterChild(name, blankGraph, min, max, plotColor, parent);
        }

        public void InitPlotterChild(String name, Texture2D blankGraph, float min, float max, Color plotColor, Plotter parent)
        {

            this.name = name;
            this.plotColor = plotColor;

            minValue = min;
            maxValue = max;
            gridHeight = parent.grid.height;
            gridWidth = parent.grid.width;

            data = new int[gridWidth];

            floor = 0;
            top = gridHeight + Mathf.RoundToInt(gridHeight * 0.17f) + floor;

            scale = (max - min) / top;

            if (max > 0 && min < 0) zeroLine = (int)((-minValue) / scale) + floor;

            child = true;
            this.Parent = parent;

            parent.AddChild(this);
        }

        public Plotter(String name, Texture2D blankGraph, float min, float max, Color plotColor, Vector2 pos)
        {

            this.name = name;
            this.plotColor = plotColor;
            gridRect = new Rect(pos.x, pos.y, blankGraph.width, blankGraph.height);

            grid = new Texture2D(blankGraph.width, blankGraph.height);
            gridWidth = grid.width;
            gridHeight = grid.height;


            buffer = blankGraph.GetPixels();
            data = new int[gridWidth];

            floor = 0;
            top = gridHeight + Mathf.RoundToInt(gridHeight * 0.17f) + floor;

            // Calculate verticle scale
            minValue = min;
            maxValue = max;
            scale = (max - min) / top;

            if (max > 0 && min < 0) zeroLine = (int)((-minValue) / scale) + floor;

        }

        /// <summary>
        /// Add data to buffer
        /// </summary>
        /// <param name="y">Value to add</param>
        public void Add(float y)
        {

            int yPos = floor;

            // Move to next position in buffer
            dataIndex++;
            if (dataIndex == gridWidth) { dataIndex = 0; dataFull = true; }

            // Add value to buffer. If outside range, then set to min/max
            if (y > maxValue) yPos = top;
            else if (y < minValue) yPos = floor;
            else yPos = (int)((y - minValue) / scale) + floor;
            data[dataIndex] = yPos;
        }
        
		void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color col)
		{
			int dy = (int)(y1-y0);
			int dx = (int)(x1-x0);
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
        /// Draw the graph.  (Must be called from OnGui)
        /// </summary>
        public void Draw()
        {
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
            for (int i = dataIndex-1; i > 0; i--)
            {
                // grid.SetPixel(x, data[i], plotColor);
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
                    //grid.SetPixel(x, data[i], plotColor);
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

            // Draw all children graphs
            if (children.Count > 0)
            {
                foreach (KeyValuePair<String, Plotter> p in children)
                {
                    p.Value.DrawChild();
                }

            }

            // Draw graph
            GUI.DrawTexture(gridRect, grid);

        }

        /// <summary>
        /// Draw Child Graphs
        /// </summary>
        private void DrawChild()
        {
            int x = this.Parent.grid.width;
			int previousX = x;
			int previousY = data[dataIndex];
			if (dataIndex == -1)
			{
				previousY = 0;
			}
			else
			{
				previousY = data[dataIndex];
			}
			
			// Plot Data in buffer back from current position back to zero
            for (int i = dataIndex-1; i > 0; i--)
            {
                // this.Parent.grid.SetPixel(x, data[i], plotColor);
				DrawLine(this.Parent.grid, previousX, previousY, x, data[i], plotColor);
				previousX = x;
				previousY = data[i];
				x--;
			}
			
			// Plot data in buffer from last position down to current position
            if (dataFull)
            {
                for (int i = gridWidth - 1; i >= dataIndex; i--)
                {
                    // this.Parent.grid.SetPixel(x, data[i], plotColor);
					DrawLine(this.Parent.grid, previousX, previousY, x, data[i], plotColor);
					previousX = x;
					previousY = data[i];
					x--;
				}
            }

            // Update texture with pixels
            this.Parent.grid.Apply(false);

        }

        /// <summary>
        /// Link a child Plotter to this
        /// </summary>
        /// <param name="child">Child plotter reference</param>
        public void AddChild(Plotter child)
        {
            if (!children.ContainsKey(child.name))
                children.Add(child.name, child);
        }

    }
}
