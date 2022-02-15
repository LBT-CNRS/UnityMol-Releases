# Unity color wheel GUI element
![My image](https://github.com/TPen/Unity_ColorWheel/blob/master/Color_Wheel_Preview.png)

Working Color-Picker element for Unity 5 (made for the new GUI System) made to resemble the Paint-Tool-Sai Color Picker.
Completly drawn by shader by manipulating the uvs so no textures needed.

Usage:

1) Put the ColorWheel folder somewhere into your Unity Asset folder

2) Create Canvas (or use an existing one) and drag the ColorWheel Prefab onto it

3) Rescale / change position etc. it should now be ready to use

The selected color will be shown as public Color variable on the prefabs gameobject so grab that for whatever should be influenced by the picker.
If you want to set the picker to a certain color reference the object and use the GetComponent<ColorWheelControl>().PickColor(value) method.

Feel free to modify it however you want as this is only meant as a base addition to unitys UI elements.
