# unity-wireframe-shader
![example](/docs/example.png)

Unity wireframe material using Geometry Shaders built for the [UCLA Game Lab](http://games.ucla.edu/resource/unity-wireframe-shader/) and [Unity Asset Store](https://www.assetstore.unity3d.com/en/#!/content/21897) in 2013.

Based on work from [this paper](http://cgg-journal.com/2008-2/06/index.html) (which is no longer available -- a web-archive version is available [here](http://web.archive.org/web/20130322011415/http://cgg-journal.com/2008-2/06/index.html))

## Use
Renders a line along every edge between every vertex. Requires Geometry Shaders, so this only works on DX11.

Only renders wireframe. Two passes can be rendered to render wireframe on top of another solid material or shader can be easily modified to render both in the same pass.

### Requirements

This material uses the geometry shader, which is only available with DirectX in Unity.

## Wireframe Options
#### Thickness
How thick the wireframe line is.

#### Line Firmness
How firm the edges of the wireframe line are when rendering _without_ cutout enabled.

#### Cutout
Whether or not to discard pixels outside the wireframe, creating a harder and aliased edge, but can draw to depth in a single pass.

#### Screenspace Thickness
Whether or not the wireframe line should be a consistent thickness in screenspace while the camera moves.

#### Double Sided
Whether or not to draw the back faces with wireframe or not

## Extending and Reuse

The functions used to generate the wireframe are available in the [UCLA GameLab Wireframe Functions.cginc](Assets/Wireframe/UCLA%20GameLab%20Wireframe%20Functions.cginc) so they can easily be added to and used with other shaders and functions.
