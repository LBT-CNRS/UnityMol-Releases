# API Documentation generated from XML comments

## APIPython Class

Defines all the functions available from the console.
`APIPython` derives from `MonoBehaviour` to access the coroutines for a few methods.
The rest of the methods are static because no instance is needed.

```csharp
public class APIPython : UnityEngine.MonoBehaviour
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [UnityEngine.Object](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Object 'UnityEngine.Object') &#129106; [UnityEngine.Component](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Component 'UnityEngine.Component') &#129106; [UnityEngine.Behaviour](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Behaviour 'UnityEngine.Behaviour') &#129106; [UnityEngine.MonoBehaviour](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.MonoBehaviour 'UnityEngine.MonoBehaviour') &#129106; APIPython
### Fields

<a name='UMol.API.APIPython.culture'></a>

## APIPython.culture Field

Output correctly formated floats

```csharp
private static readonly CultureInfo culture;
```

#### Field Value
[System.Globalization.CultureInfo](https://docs.microsoft.com/en-us/dotnet/api/System.Globalization.CultureInfo 'System.Globalization.CultureInfo')

<a name='UMol.API.APIPython.extCom'></a>

## APIPython.extCom Field

Component for external TCP commands.

```csharp
private static TCPServerCommand extCom;
```

#### Field Value
[TCPServerCommand](UMol.TCPServerCommand.md 'UMol.TCPServerCommand')

<a name='UMol.API.APIPython.instance'></a>

## APIPython.instance Field

Uniq instance of the class (Singleton).

```csharp
private static APIPython instance;
```

#### Field Value
[APIPython](UMol.API.APIPython.md 'UMol.API.APIPython')

<a name='UMol.API.APIPython.limitSizeSelectionString'></a>

## APIPython.limitSizeSelectionString Field

Limit the size of selection string query, switch to atomid ranges when over this limit

```csharp
private const int limitSizeSelectionString = 500;
```

#### Field Value
[System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

<a name='UMol.API.APIPython.path'></a>

## APIPython.path Field

Path of data folder

```csharp
private static string path;
```

#### Field Value
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

<a name='UMol.API.APIPython.pythonConsole'></a>

## APIPython.pythonConsole Field

Reference to the python console

```csharp
private static PythonConsole2 pythonConsole;
```

#### Field Value
[UMol.PythonConsole2](https://docs.microsoft.com/en-us/dotnet/api/UMol.PythonConsole2 'UMol.PythonConsole2')
### Methods

<a name='UMol.API.APIPython.activateExternalCommands()'></a>

## APIPython.activateExternalCommands() Method

Activate the TCP server command.
Allow to receive external commands from the TCP socket.

```csharp
public static void activateExternalCommands();
```

<a name='UMol.API.APIPython.addHydrogensHaad(string)'></a>

## APIPython.addHydrogensHaad(string) Method

Use HAAD method to add hydrogens

```csharp
public static void addHydrogensHaad(string structureName);
```
#### Parameters

<a name='UMol.API.APIPython.addHydrogensHaad(string).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The name of the structure concerned

<a name='UMol.API.APIPython.addHydrogensReduce(string)'></a>

## APIPython.addHydrogensReduce(string) Method

Use Reduce method to add hydrogens to a given structure

```csharp
public static void addHydrogensReduce(string structureName);
```
#### Parameters

<a name='UMol.API.APIPython.addHydrogensReduce(string).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The name of the structure concerned

<a name='UMol.API.APIPython.addSelectionKeyword(string,string)'></a>

## APIPython.addSelectionKeyword(string, string) Method

Add a keyword to the selection language for the selection 'selName'

```csharp
public static void addSelectionKeyword(string keyword, string selName);
```
#### Parameters

<a name='UMol.API.APIPython.addSelectionKeyword(string,string).keyword'></a>

`keyword` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the new keyword

<a name='UMol.API.APIPython.addSelectionKeyword(string,string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the selection name concerned.

<a name='UMol.API.APIPython.addSelectionToTour(string)'></a>

## APIPython.addSelectionToTour(string) Method

Add a selection to the Tour.

```csharp
public static void addSelectionToTour(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.addSelectionToTour(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the selection name concerned

<a name='UMol.API.APIPython.addToSelection(string,string,bool,bool)'></a>

## APIPython.addToSelection(string, string, bool, bool) Method

Look for an existing selection named 'name' and add atoms to it based on MDAnalysis selection language

```csharp
public static void addToSelection(string selMDA, string name="selection", bool silent=false, bool allModels=false);
```
#### Parameters

<a name='UMol.API.APIPython.addToSelection(string,string,bool,bool).selMDA'></a>

`selMDA` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the selection query

<a name='UMol.API.APIPython.addToSelection(string,string,bool,bool).name'></a>

`name` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the name of the selection.

<a name='UMol.API.APIPython.addToSelection(string,string,bool,bool).silent'></a>

`silent` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Print the new selection in the console

<a name='UMol.API.APIPython.addToSelection(string,string,bool,bool).allModels'></a>

`allModels` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

apply the selection to all models of structures concerned?

<a name='UMol.API.APIPython.annotate2DText(UnityEngine.Vector2,float,string,UnityEngine.Color)'></a>

## APIPython.annotate2DText(Vector2, float, string, Color) Method

Add a 2D annotation text over everything
The screenP defines the position based on the percentage from bottom/left to top/right of the screen
with 0/0 means bottom/left and 1/1 means top/right

```csharp
public static void annotate2DText(UnityEngine.Vector2 screenP, float scale, string text, UnityEngine.Color textCol);
```
#### Parameters

<a name='UMol.API.APIPython.annotate2DText(UnityEngine.Vector2,float,string,UnityEngine.Color).screenP'></a>

`screenP` [UnityEngine.Vector2](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector2 'UnityEngine.Vector2')

the position of the annotation

<a name='UMol.API.APIPython.annotate2DText(UnityEngine.Vector2,float,string,UnityEngine.Color).scale'></a>

`scale` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the scale of the annotation

<a name='UMol.API.APIPython.annotate2DText(UnityEngine.Vector2,float,string,UnityEngine.Color).text'></a>

`text` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the text of the annotation

<a name='UMol.API.APIPython.annotate2DText(UnityEngine.Vector2,float,string,UnityEngine.Color).textCol'></a>

`textCol` [UnityEngine.Color](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Color 'UnityEngine.Color')

the color of the text

<a name='UMol.API.APIPython.annotateAngle(string,int,string,int,string,int)'></a>

## APIPython.annotateAngle(string, int, string, int, string, int) Method

Create an annotation of type "Angle" between 3 atoms.
It adds a surrounding sphere around atoms and add a text for the angle value.

```csharp
public static void annotateAngle(string structureName, int atomId, string structureName2, int atomId2, string structureName3, int atomId3);
```
#### Parameters

<a name='UMol.API.APIPython.annotateAngle(string,int,string,int,string,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the first atom

<a name='UMol.API.APIPython.annotateAngle(string,int,string,int,string,int).atomId'></a>

`atomId` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the first atom ID

<a name='UMol.API.APIPython.annotateAngle(string,int,string,int,string,int).structureName2'></a>

`structureName2` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the second atom

<a name='UMol.API.APIPython.annotateAngle(string,int,string,int,string,int).atomId2'></a>

`atomId2` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the second atom ID

<a name='UMol.API.APIPython.annotateAngle(string,int,string,int,string,int).structureName3'></a>

`structureName3` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the third atom

<a name='UMol.API.APIPython.annotateAngle(string,int,string,int,string,int).atomId3'></a>

`atomId3` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the third atom ID

<a name='UMol.API.APIPython.annotateArcLine(string,int,string,int,string,int)'></a>

## APIPython.annotateArcLine(string, int, string, int, string, int) Method

Create an annotation of type "ArcLine" between 3 atoms.

```csharp
public static void annotateArcLine(string structureName, int atomId, string structureName2, int atomId2, string structureName3, int atomId3);
```
#### Parameters

<a name='UMol.API.APIPython.annotateArcLine(string,int,string,int,string,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the first atom

<a name='UMol.API.APIPython.annotateArcLine(string,int,string,int,string,int).atomId'></a>

`atomId` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the first atom ID

<a name='UMol.API.APIPython.annotateArcLine(string,int,string,int,string,int).structureName2'></a>

`structureName2` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the second atom

<a name='UMol.API.APIPython.annotateArcLine(string,int,string,int,string,int).atomId2'></a>

`atomId2` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the second atom ID

<a name='UMol.API.APIPython.annotateArcLine(string,int,string,int,string,int).structureName3'></a>

`structureName3` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the third atom

<a name='UMol.API.APIPython.annotateArcLine(string,int,string,int,string,int).atomId3'></a>

`atomId3` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the third atom ID

<a name='UMol.API.APIPython.annotateAtom(string,int)'></a>

## APIPython.annotateAtom(string, int) Method

Annotate an atom by creating a surrounding sphere around it

```csharp
public static void annotateAtom(string structureName, int atomId);
```
#### Parameters

<a name='UMol.API.APIPython.annotateAtom(string,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the atom

<a name='UMol.API.APIPython.annotateAtom(string,int).atomId'></a>

`atomId` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the atom ID

<a name='UMol.API.APIPython.annotateAtomText(string,int,string,UnityEngine.Color,bool)'></a>

## APIPython.annotateAtomText(string, int, string, Color, bool) Method

Annotate an atom by with a text

```csharp
public static void annotateAtomText(string structureName, int atomId, string text, UnityEngine.Color textCol, bool showLine=false);
```
#### Parameters

<a name='UMol.API.APIPython.annotateAtomText(string,int,string,UnityEngine.Color,bool).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the atom

<a name='UMol.API.APIPython.annotateAtomText(string,int,string,UnityEngine.Color,bool).atomId'></a>

`atomId` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the atom ID

<a name='UMol.API.APIPython.annotateAtomText(string,int,string,UnityEngine.Color,bool).text'></a>

`text` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The text of the annotation

<a name='UMol.API.APIPython.annotateAtomText(string,int,string,UnityEngine.Color,bool).textCol'></a>

`textCol` [UnityEngine.Color](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Color 'UnityEngine.Color')

the color of the text

<a name='UMol.API.APIPython.annotateAtomText(string,int,string,UnityEngine.Color,bool).showLine'></a>

`showLine` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

display lines around the annotation?

<a name='UMol.API.APIPython.annotateDihedralAngle(string,int,string,int,string,int,string,int)'></a>

## APIPython.annotateDihedralAngle(string, int, string, int, string, int, string, int) Method

Create an annotation of type "Dihedral" between 4 atoms.
It adds a surrounding sphere around atoms and add a text for the angle value.

```csharp
public static void annotateDihedralAngle(string structureName, int atomId, string structureName2, int atomId2, string structureName3, int atomId3, string structureName4, int atomId4);
```
#### Parameters

<a name='UMol.API.APIPython.annotateDihedralAngle(string,int,string,int,string,int,string,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the first atom

<a name='UMol.API.APIPython.annotateDihedralAngle(string,int,string,int,string,int,string,int).atomId'></a>

`atomId` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the first atom ID

<a name='UMol.API.APIPython.annotateDihedralAngle(string,int,string,int,string,int,string,int).structureName2'></a>

`structureName2` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the second atom

<a name='UMol.API.APIPython.annotateDihedralAngle(string,int,string,int,string,int,string,int).atomId2'></a>

`atomId2` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the second atom ID

<a name='UMol.API.APIPython.annotateDihedralAngle(string,int,string,int,string,int,string,int).structureName3'></a>

`structureName3` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the third atom

<a name='UMol.API.APIPython.annotateDihedralAngle(string,int,string,int,string,int,string,int).atomId3'></a>

`atomId3` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the third atom ID

<a name='UMol.API.APIPython.annotateDihedralAngle(string,int,string,int,string,int,string,int).structureName4'></a>

`structureName4` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the fourth atom

<a name='UMol.API.APIPython.annotateDihedralAngle(string,int,string,int,string,int,string,int).atomId4'></a>

`atomId4` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the fourth atom ID

<a name='UMol.API.APIPython.annotateDistance(string,int,string,int)'></a>

## APIPython.annotateDistance(string, int, string, int) Method

Create an annotation of type "Distance" between 2 atoms : draw a line between the same and add a text for the distance

```csharp
public static void annotateDistance(string structureName, int atomId, string structureName2, int atomId2);
```
#### Parameters

<a name='UMol.API.APIPython.annotateDistance(string,int,string,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the first atom

<a name='UMol.API.APIPython.annotateDistance(string,int,string,int).atomId'></a>

`atomId` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the first atom ID

<a name='UMol.API.APIPython.annotateDistance(string,int,string,int).structureName2'></a>

`structureName2` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the second atom

<a name='UMol.API.APIPython.annotateDistance(string,int,string,int).atomId2'></a>

`atomId2` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the second atom ID

<a name='UMol.API.APIPython.annotateDrawLine(string,System.Collections.Generic.List_UnityEngine.Vector3_,UnityEngine.Color)'></a>

## APIPython.annotateDrawLine(string, List<Vector3>, Color) Method

Add an annotation of type "DrawLine" linked to a structure.

```csharp
public static void annotateDrawLine(string structureName, System.Collections.Generic.List<UnityEngine.Vector3> line, UnityEngine.Color col);
```
#### Parameters

<a name='UMol.API.APIPython.annotateDrawLine(string,System.Collections.Generic.List_UnityEngine.Vector3_,UnityEngine.Color).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the name of the structure concerned

<a name='UMol.API.APIPython.annotateDrawLine(string,System.Collections.Generic.List_UnityEngine.Vector3_,UnityEngine.Color).line'></a>

`line` [System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')

the list of positions of the line

<a name='UMol.API.APIPython.annotateDrawLine(string,System.Collections.Generic.List_UnityEngine.Vector3_,UnityEngine.Color).col'></a>

`col` [UnityEngine.Color](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Color 'UnityEngine.Color')

the color of the line

<a name='UMol.API.APIPython.annotateLine(string,int,string,int)'></a>

## APIPython.annotateLine(string, int, string, int) Method

Create an annotation line between 2 atoms

```csharp
public static void annotateLine(string structureName, int atomId, string structureName2, int atomId2);
```
#### Parameters

<a name='UMol.API.APIPython.annotateLine(string,int,string,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the first atom

<a name='UMol.API.APIPython.annotateLine(string,int,string,int).atomId'></a>

`atomId` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the first atom ID

<a name='UMol.API.APIPython.annotateLine(string,int,string,int).structureName2'></a>

`structureName2` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the second atom

<a name='UMol.API.APIPython.annotateLine(string,int,string,int).atomId2'></a>

`atomId2` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the second atom ID

<a name='UMol.API.APIPython.annotateRotatingArrow(string,int,string,int)'></a>

## APIPython.annotateRotatingArrow(string, int, string, int) Method

Create an annotation of type "arrow" between 2 atoms

```csharp
public static void annotateRotatingArrow(string structureName, int atomId, string structureName2, int atomId2);
```
#### Parameters

<a name='UMol.API.APIPython.annotateRotatingArrow(string,int,string,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the first atom

<a name='UMol.API.APIPython.annotateRotatingArrow(string,int,string,int).atomId'></a>

`atomId` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the first atom ID

<a name='UMol.API.APIPython.annotateRotatingArrow(string,int,string,int).structureName2'></a>

`structureName2` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the second atom

<a name='UMol.API.APIPython.annotateRotatingArrow(string,int,string,int).atomId2'></a>

`atomId2` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the second atom ID

<a name='UMol.API.APIPython.annotateSphere(UnityEngine.Vector3,float)'></a>

## APIPython.annotateSphere(Vector3, float) Method

Create an annotation sphere

```csharp
public static void annotateSphere(UnityEngine.Vector3 worldP, float scale=1f);
```
#### Parameters

<a name='UMol.API.APIPython.annotateSphere(UnityEngine.Vector3,float).worldP'></a>

`worldP` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the center position of the sphere (World reference)

<a name='UMol.API.APIPython.annotateSphere(UnityEngine.Vector3,float).scale'></a>

`scale` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the scale of the sphere

<a name='UMol.API.APIPython.annotateWorldLine(UnityEngine.Vector3,UnityEngine.Vector3,float,UnityEngine.Color)'></a>

## APIPython.annotateWorldLine(Vector3, Vector3, float, Color) Method

Add a global annotation line between 2 positions.

```csharp
public static void annotateWorldLine(UnityEngine.Vector3 p1, UnityEngine.Vector3 p2, float sizeLine, UnityEngine.Color lineCol);
```
#### Parameters

<a name='UMol.API.APIPython.annotateWorldLine(UnityEngine.Vector3,UnityEngine.Vector3,float,UnityEngine.Color).p1'></a>

`p1` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the starting position

<a name='UMol.API.APIPython.annotateWorldLine(UnityEngine.Vector3,UnityEngine.Vector3,float,UnityEngine.Color).p2'></a>

`p2` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the ending position

<a name='UMol.API.APIPython.annotateWorldLine(UnityEngine.Vector3,UnityEngine.Vector3,float,UnityEngine.Color).sizeLine'></a>

`sizeLine` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the size of the line

<a name='UMol.API.APIPython.annotateWorldLine(UnityEngine.Vector3,UnityEngine.Vector3,float,UnityEngine.Color).lineCol'></a>

`lineCol` [UnityEngine.Color](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Color 'UnityEngine.Color')

the color of the line

<a name='UMol.API.APIPython.annotateWorldText(UnityEngine.Vector3,float,string,UnityEngine.Color)'></a>

## APIPython.annotateWorldText(Vector3, float, string, Color) Method

Create a global text annotation in the scene

```csharp
public static void annotateWorldText(UnityEngine.Vector3 worldP, float scale, string text, UnityEngine.Color textCol);
```
#### Parameters

<a name='UMol.API.APIPython.annotateWorldText(UnityEngine.Vector3,float,string,UnityEngine.Color).worldP'></a>

`worldP` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the center position of the annotation (World reference)

<a name='UMol.API.APIPython.annotateWorldText(UnityEngine.Vector3,float,string,UnityEngine.Color).scale'></a>

`scale` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the scale of the annotation

<a name='UMol.API.APIPython.annotateWorldText(UnityEngine.Vector3,float,string,UnityEngine.Color).text'></a>

`text` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the text of the annotation

<a name='UMol.API.APIPython.annotateWorldText(UnityEngine.Vector3,float,string,UnityEngine.Color).textCol'></a>

`textCol` [UnityEngine.Color](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Color 'UnityEngine.Color')

the color of the text

<a name='UMol.API.APIPython.areRepresentationsOn(string,string)'></a>

## APIPython.areRepresentationsOn(string, string) Method

Test whether a representation of type 'type' is shown for a specified selection

```csharp
public static bool areRepresentationsOn(string selName, string type);
```
#### Parameters

<a name='UMol.API.APIPython.areRepresentationsOn(string,string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.areRepresentationsOn(string,string).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of representation

#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')
True if representation is shown. False otherwise

<a name='UMol.API.APIPython.areRepresentationsOn(string)'></a>

## APIPython.areRepresentationsOn(string) Method

Test whether at least one representation of a given structure is shown or not

```csharp
public static bool areRepresentationsOn(string structureName);
```
#### Parameters

<a name='UMol.API.APIPython.areRepresentationsOn(string).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The structure name concerned

#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')
True if representation is shown. False otherwise

<a name='UMol.API.APIPython.bg_color(string)'></a>

## APIPython.bg_color(string) Method

Change the background color of the camera based on a color name, also changes the fog color

```csharp
public static void bg_color(string colorS);
```
#### Parameters

<a name='UMol.API.APIPython.bg_color(string).colorS'></a>

`colorS` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the new color as a string

<a name='UMol.API.APIPython.bg_color(UnityEngine.Color)'></a>

## APIPython.bg_color(Color) Method

Change the background color of the camera based on a color name, also changes the fog color

```csharp
public static void bg_color(UnityEngine.Color col);
```
#### Parameters

<a name='UMol.API.APIPython.bg_color(UnityEngine.Color).col'></a>

`col` [UnityEngine.Color](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Color 'UnityEngine.Color')

the new color as a Color object

<a name='UMol.API.APIPython.canRunCommand()'></a>

## APIPython.canRunCommand() Method

Can one execute python command through the Python console?

```csharp
private static bool canRunCommand();
```

#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')
True if one can execute a command. False otherwise

<a name='UMol.API.APIPython.cBoolToPy(bool)'></a>

## APIPython.cBoolToPy(bool) Method

Return a boolean string depending on the boolean 'val'

```csharp
public static string cBoolToPy(bool val);
```
#### Parameters

<a name='UMol.API.APIPython.cBoolToPy(bool).val'></a>

`val` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

the boolean value

#### Returns
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
"True" if true. "False" if false

<a name='UMol.API.APIPython.cd(string)'></a>

## APIPython.cd(string) Method

Change the current directory to a new path

```csharp
public static void cd(string newPath);
```
#### Parameters

<a name='UMol.API.APIPython.cd(string).newPath'></a>

`newPath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the new path

<a name='UMol.API.APIPython.cealign(string,string)'></a>

## APIPython.cealign(string, string) Method

Use CEAlign algorithm to align two selections (usually molecules), uses only C-alpha atoms
<remarks>For more details: https://pymolwiki.org/index.php/Cealign</remarks>

```csharp
public static void cealign(string selNameTarget, string selNameMobile);
```
#### Parameters

<a name='UMol.API.APIPython.cealign(string,string).selNameTarget'></a>

`selNameTarget` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the reference selection

<a name='UMol.API.APIPython.cealign(string,string).selNameMobile'></a>

`selNameMobile` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the target selection

<a name='UMol.API.APIPython.centerOnSelection(string,bool,float,float)'></a>

## APIPython.centerOnSelection(string, bool, float, float) Method

Center the selections 'selName' by offsets all representations
If lerp is true and duration is > 0, centering is done during 'duration' seconds
Fit the selection in the camera field of view if distance is negative, otherwise the molecule will be placed at "distance" from the camera

```csharp
public static void centerOnSelection(string selName, bool lerp=false, float distance=-1f, float duration=0.25f);
```
#### Parameters

<a name='UMol.API.APIPython.centerOnSelection(string,bool,float,float).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The name of the structure concerned

<a name='UMol.API.APIPython.centerOnSelection(string,bool,float,float).lerp'></a>

`lerp` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Center with a linear interpolation?

<a name='UMol.API.APIPython.centerOnSelection(string,bool,float,float).distance'></a>

`distance` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

distance of the selection from the camera

<a name='UMol.API.APIPython.centerOnSelection(string,bool,float,float).duration'></a>

`duration` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

Duration of the centering with interpolation

<a name='UMol.API.APIPython.centerOnStructure(string,bool,bool)'></a>

## APIPython.centerOnStructure(string, bool, bool) Method

Center the structure 'structureName' by offsets all representations
Instead of moving the camera, move the loaded molecules GO to center them in the center of the camera

```csharp
public static void centerOnStructure(string structureName, bool lerp=false, bool recordCommand=true);
```
#### Parameters

<a name='UMol.API.APIPython.centerOnStructure(string,bool,bool).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The name of the structure concerned

<a name='UMol.API.APIPython.centerOnStructure(string,bool,bool).lerp'></a>

`lerp` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Center with a linear interpolation?

<a name='UMol.API.APIPython.centerOnStructure(string,bool,bool).recordCommand'></a>

`recordCommand` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Record this command in the history?

<a name='UMol.API.APIPython.changeGeneralScale_cog(float)'></a>

## APIPython.changeGeneralScale_cog(float) Method

Change the scale of the parent of the representations of each molecules
Try to not move the center of mass

```csharp
public static void changeGeneralScale_cog(float newVal);
```
#### Parameters

<a name='UMol.API.APIPython.changeGeneralScale_cog(float).newVal'></a>

`newVal` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new value of the scale

<a name='UMol.API.APIPython.changeGeneralScale(float)'></a>

## APIPython.changeGeneralScale(float) Method

Change the scale of the parent of the representations of each molecules
Keep relative positions of molecules, use the first loaded molecule center of gravity to compensate the translation due to scaling

```csharp
public static void changeGeneralScale(float newVal);
```
#### Parameters

<a name='UMol.API.APIPython.changeGeneralScale(float).newVal'></a>

`newVal` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new value of the scale

<a name='UMol.API.APIPython.changeHighlightMaterial(UnityEngine.Material)'></a>

## APIPython.changeHighlightMaterial(Material) Method

Utility function to change the material of highlighted selection

```csharp
public static void changeHighlightMaterial(UnityEngine.Material newMat);
```
#### Parameters

<a name='UMol.API.APIPython.changeHighlightMaterial(UnityEngine.Material).newMat'></a>

`newMat` [UnityEngine.Material](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Material 'UnityEngine.Material')

the new material as a Material object.

<a name='UMol.API.APIPython.changeRotationSpeedX(float)'></a>

## APIPython.changeRotationSpeedX(float) Method

Change the rotation speed around the X axis

```csharp
public static void changeRotationSpeedX(float val);
```
#### Parameters

<a name='UMol.API.APIPython.changeRotationSpeedX(float).val'></a>

`val` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new speed

<a name='UMol.API.APIPython.changeRotationSpeedY(float)'></a>

## APIPython.changeRotationSpeedY(float) Method

Change the rotation speed around the Y axis

```csharp
public static void changeRotationSpeedY(float val);
```
#### Parameters

<a name='UMol.API.APIPython.changeRotationSpeedY(float).val'></a>

`val` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new speed

<a name='UMol.API.APIPython.changeRotationSpeedZ(float)'></a>

## APIPython.changeRotationSpeedZ(float) Method

Change the rotation speed around the Z axis

```csharp
public static void changeRotationSpeedZ(float val);
```
#### Parameters

<a name='UMol.API.APIPython.changeRotationSpeedZ(float).val'></a>

`val` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new speed

<a name='UMol.API.APIPython.cleanHighlight()'></a>

## APIPython.cleanHighlight() Method

Directly clear the highlight manager, this does not unselect the current selection

```csharp
public static void cleanHighlight();
```

<a name='UMol.API.APIPython.clearAnnotations()'></a>

## APIPython.clearAnnotations() Method

Remove all annotations + Drawings

```csharp
public static void clearAnnotations();
```

<a name='UMol.API.APIPython.clearDrawings()'></a>

## APIPython.clearDrawings() Method

Remove all drawing annotations

```csharp
public static void clearDrawings();
```

<a name='UMol.API.APIPython.clearHyperballAO(string)'></a>

## APIPython.clearHyperballAO(string) Method

Remove Ambient Occlusion from hyperball representation in a given selection

```csharp
public static void clearHyperballAO(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.clearHyperballAO(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.clearSelections()'></a>

## APIPython.clearSelections() Method

Clear the current selection in UnityMolSelectionManager

```csharp
public static void clearSelections();
```

<a name='UMol.API.APIPython.clearSurfaceAO(string)'></a>

## APIPython.clearSurfaceAO(string) Method

Remove Ambient Occlusion for surface representations of a given selection.

```csharp
public static void clearSurfaceAO(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.clearSurfaceAO(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.clearTour()'></a>

## APIPython.clearTour() Method

Stop the Tour feature

```csharp
public static void clearTour();
```

<a name='UMol.API.APIPython.colorAtomType(string,string,string,UnityEngine.Color)'></a>

## APIPython.colorAtomType(string, string, string, Color) Method

Change the color of the atom type 'atomType' in the representation 'type' in the given selection

```csharp
public static void colorAtomType(string selName, string type, string atomType, UnityEngine.Color col);
```
#### Parameters

<a name='UMol.API.APIPython.colorAtomType(string,string,string,UnityEngine.Color).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.colorAtomType(string,string,string,UnityEngine.Color).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

<a name='UMol.API.APIPython.colorAtomType(string,string,string,UnityEngine.Color).atomType'></a>

`atomType` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the type of atoms to change the color

<a name='UMol.API.APIPython.colorAtomType(string,string,string,UnityEngine.Color).col'></a>

`col` [UnityEngine.Color](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Color 'UnityEngine.Color')

the new color

<a name='UMol.API.APIPython.colorByAtom(string,string)'></a>

## APIPython.colorByAtom(string, string) Method

Use the color palette to color by atom the representation of type 'type' in the selection 'selName'

```csharp
public static void colorByAtom(string selName, string type);
```
#### Parameters

<a name='UMol.API.APIPython.colorByAtom(string,string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.colorByAtom(string,string).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

<a name='UMol.API.APIPython.colorByChain(string,string)'></a>

## APIPython.colorByChain(string, string) Method

Use the color palette to color by chain the representation of type 'type' in the selection 'selName'
type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"

```csharp
public static void colorByChain(string selName, string type);
```
#### Parameters

<a name='UMol.API.APIPython.colorByChain(string,string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.colorByChain(string,string).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

<a name='UMol.API.APIPython.colorByHydrophobicity(string,string)'></a>

## APIPython.colorByHydrophobicity(string, string) Method

Use the color palette to color by hydrophobicity the representation of type 'type' in the selection 'selName'

```csharp
public static void colorByHydrophobicity(string selName, string type);
```
#### Parameters

<a name='UMol.API.APIPython.colorByHydrophobicity(string,string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.colorByHydrophobicity(string,string).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

<a name='UMol.API.APIPython.colorByResid(string,string)'></a>

## APIPython.colorByResid(string, string) Method

Use the color palette to color by residue ID the representation of type 'type' in the selection 'selName'
type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"

```csharp
public static void colorByResid(string selName, string type);
```
#### Parameters

<a name='UMol.API.APIPython.colorByResid(string,string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.colorByResid(string,string).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

<a name='UMol.API.APIPython.colorByResidue(string,string)'></a>

## APIPython.colorByResidue(string, string) Method

Use the color palette to color by residue the representation of type 'type' in the selection 'selName'
type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"

```csharp
public static void colorByResidue(string selName, string type);
```
#### Parameters

<a name='UMol.API.APIPython.colorByResidue(string,string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.colorByResidue(string,string).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

<a name='UMol.API.APIPython.colorByResidueCharge(string,string)'></a>

## APIPython.colorByResidueCharge(string, string) Method

Use the color palette to color by residue charge the representation of type 'type' in the selection 'selName'
Colors: negatively charge = red, positively charged = blue, neutral = white

```csharp
public static void colorByResidueCharge(string selName, string type);
```
#### Parameters

<a name='UMol.API.APIPython.colorByResidueCharge(string,string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.colorByResidueCharge(string,string).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

<a name='UMol.API.APIPython.colorByResidueType(string,string)'></a>

## APIPython.colorByResidueType(string, string) Method

Use the color palette to color by residue type the representation of type 'type' in the selection 'selName'
Colors: negatively charge = red, positively charged = blue, nonpolar = light yellow, polar = green, cys = orange

```csharp
public static void colorByResidueType(string selName, string type);
```
#### Parameters

<a name='UMol.API.APIPython.colorByResidueType(string,string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.colorByResidueType(string,string).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

<a name='UMol.API.APIPython.colorByResnum(string,string)'></a>

## APIPython.colorByResnum(string, string) Method

Use the color palette to color by residue number representation of type 'type' in the selection 'selName'
type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"

```csharp
public static void colorByResnum(string selName, string type);
```
#### Parameters

<a name='UMol.API.APIPython.colorByResnum(string,string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.colorByResnum(string,string).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

<a name='UMol.API.APIPython.colorBySequence(string,string)'></a>

## APIPython.colorBySequence(string, string) Method

Use the color palette to color by sequence (rainbow effect) representation of type 'type' in the selection 'selName'
type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"

```csharp
public static void colorBySequence(string selName, string type);
```
#### Parameters

<a name='UMol.API.APIPython.colorBySequence(string,string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.colorBySequence(string,string).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

<a name='UMol.API.APIPython.colorSelection(string,string,string)'></a>

## APIPython.colorSelection(string, string, string) Method

Change the color of all representation of type 'type' in the selection
ype can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
olorS can be "black", "white", "yellow", "green", "red", "blue", "pink", "gray"

```csharp
public static void colorSelection(string selName, string type, string colorS);
```
#### Parameters

<a name='UMol.API.APIPython.colorSelection(string,string,string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.colorSelection(string,string,string).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

<a name='UMol.API.APIPython.colorSelection(string,string,string).colorS'></a>

`colorS` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the new color as a string for the representations

<a name='UMol.API.APIPython.colorSelection(string,string,System.Collections.Generic.List_UnityEngine.Color32_)'></a>

## APIPython.colorSelection(string, string, List<Color32>) Method

Change the color of all representation of type 'type' in the selection
ype can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
olors is a list of colors for each atom of the selection

```csharp
public static void colorSelection(string selName, string type, System.Collections.Generic.List<UnityEngine.Color32> colors);
```
#### Parameters

<a name='UMol.API.APIPython.colorSelection(string,string,System.Collections.Generic.List_UnityEngine.Color32_).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.colorSelection(string,string,System.Collections.Generic.List_UnityEngine.Color32_).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

<a name='UMol.API.APIPython.colorSelection(string,string,System.Collections.Generic.List_UnityEngine.Color32_).colors'></a>

`colors` [System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[UnityEngine.Color32](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Color32 'UnityEngine.Color32')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')

the new color for each atom of the representation

<a name='UMol.API.APIPython.colorSelection(string,string,UnityEngine.Color)'></a>

## APIPython.colorSelection(string, string, Color) Method

Change the color of all representation of type 'type' in the selection
ype can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"

```csharp
public static void colorSelection(string selName, string type, UnityEngine.Color col);
```
#### Parameters

<a name='UMol.API.APIPython.colorSelection(string,string,UnityEngine.Color).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.colorSelection(string,string,UnityEngine.Color).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

<a name='UMol.API.APIPython.colorSelection(string,string,UnityEngine.Color).col'></a>

`col` [UnityEngine.Color](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Color 'UnityEngine.Color')

the new color as a Color object for the representations

<a name='UMol.API.APIPython.colorSelection(string,UnityEngine.Color,string,string)'></a>

## APIPython.colorSelection(string, Color, string, string) Method

From a global selection name,
change the color of all atoms selected on the selection query "selQuery" in the representation 'type'.
If 'type' is not specified, change the color for all representations concerned.

```csharp
public static void colorSelection(string selName, UnityEngine.Color col, string selQuery, string type="");
```
#### Parameters

<a name='UMol.API.APIPython.colorSelection(string,UnityEngine.Color,string,string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.colorSelection(string,UnityEngine.Color,string,string).col'></a>

`col` [UnityEngine.Color](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Color 'UnityEngine.Color')

the new color

<a name='UMol.API.APIPython.colorSelection(string,UnityEngine.Color,string,string).selQuery'></a>

`selQuery` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the selection query

<a name='UMol.API.APIPython.colorSelection(string,UnityEngine.Color,string,string).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the type of representation to modify color. "" means all representations

<a name='UMol.API.APIPython.colorSelection(string,UnityEngine.Color)'></a>

## APIPython.colorSelection(string, Color) Method

Change the color of all representations in a selection

```csharp
public static void colorSelection(string selName, UnityEngine.Color col);
```
#### Parameters

<a name='UMol.API.APIPython.colorSelection(string,UnityEngine.Color).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.colorSelection(string,UnityEngine.Color).col'></a>

`col` [UnityEngine.Color](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Color 'UnityEngine.Color')

The new color

<a name='UMol.API.APIPython.computeSurfaceAO(string)'></a>

## APIPython.computeSurfaceAO(string) Method

Compute object space Ambient Occlusion for surface representations of a given selection.

```csharp
public static void computeSurfaceAO(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.computeSurfaceAO(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.connectIMD(string,string,int)'></a>

## APIPython.connectIMD(string, string, int) Method

Connect to a running simulation using the IMD protocol implemented in MDDriver
The running simulation is bound to a UnityMolStructure

```csharp
public static bool connectIMD(string structureName, string adress, int port);
```
#### Parameters

<a name='UMol.API.APIPython.connectIMD(string,string,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The name of the 1st structure concerned

<a name='UMol.API.APIPython.connectIMD(string,string,int).adress'></a>

`adress` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the IP address of the running simulation

<a name='UMol.API.APIPython.connectIMD(string,string,int).port'></a>

`port` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the port of the running simulation

#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')
True if the connexion is successful. False otherwise

<a name='UMol.API.APIPython.cStringToPy(string)'></a>

## APIPython.cStringToPy(string) Method

Return a string from the string value 's'

```csharp
private static string cStringToPy(string s);
```
#### Parameters

<a name='UMol.API.APIPython.cStringToPy(string).s'></a>

`s` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the string

#### Returns
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
the string with quote

<a name='UMol.API.APIPython.cVec3ToPy(UnityEngine.Vector3)'></a>

## APIPython.cVec3ToPy(Vector3) Method

Return a Vector 3 string from the Vector3 value 'val'

```csharp
private static string cVec3ToPy(UnityEngine.Vector3 val);
```
#### Parameters

<a name='UMol.API.APIPython.cVec3ToPy(UnityEngine.Vector3).val'></a>

`val` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the value

#### Returns
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
the string representation

<a name='UMol.API.APIPython.defaultRep(string)'></a>

## APIPython.defaultRep(string) Method

Create selections and default representations: all in cartoon, not protein in hyperballs
Also create a selection containing "not protein and not water and not ligand and not ions"

```csharp
private static bool defaultRep(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.defaultRep(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The default selection of the whole structure

#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')
True if success. False otherwise

<a name='UMol.API.APIPython.delayedSetHyperballParam(string,UnityEngine.Vector3,UnityEngine.Vector3,float)'></a>

## APIPython.delayedSetHyperballParam(string, Vector3, Vector3, float) Method

Coroutine handling the changes of hyperball parameters

```csharp
private System.Collections.IEnumerator delayedSetHyperballParam(string selName, UnityEngine.Vector3 prevScaleShrink, UnityEngine.Vector3 scaleShrink, float duration);
```
#### Parameters

<a name='UMol.API.APIPython.delayedSetHyperballParam(string,UnityEngine.Vector3,UnityEngine.Vector3,float).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the selection name concerned

<a name='UMol.API.APIPython.delayedSetHyperballParam(string,UnityEngine.Vector3,UnityEngine.Vector3,float).prevScaleShrink'></a>

`prevScaleShrink` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the previous parameters

<a name='UMol.API.APIPython.delayedSetHyperballParam(string,UnityEngine.Vector3,UnityEngine.Vector3,float).scaleShrink'></a>

`scaleShrink` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the new parameters

<a name='UMol.API.APIPython.delayedSetHyperballParam(string,UnityEngine.Vector3,UnityEngine.Vector3,float).duration'></a>

`duration` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

Duration of the change

#### Returns
[System.Collections.IEnumerator](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.IEnumerator 'System.Collections.IEnumerator')
IEnumerator

<a name='UMol.API.APIPython.delayedSetTransform(UnityEngine.Transform,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,bool,float)'></a>

## APIPython.delayedSetTransform(Transform, Vector3, Vector3, Vector3, Vector3, bool, float) Method

Coroutine to modify the transform component of the LoadedMolecules GameObject.

```csharp
private System.Collections.IEnumerator delayedSetTransform(UnityEngine.Transform t, UnityEngine.Vector3 endpos, UnityEngine.Vector3 scale, UnityEngine.Vector3 rot, UnityEngine.Vector3 centerOfRotation, bool lerp, float duration=1f);
```
#### Parameters

<a name='UMol.API.APIPython.delayedSetTransform(UnityEngine.Transform,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,bool,float).t'></a>

`t` [UnityEngine.Transform](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Transform 'UnityEngine.Transform')

the current transform component

<a name='UMol.API.APIPython.delayedSetTransform(UnityEngine.Transform,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,bool,float).endpos'></a>

`endpos` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the new position

<a name='UMol.API.APIPython.delayedSetTransform(UnityEngine.Transform,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,bool,float).scale'></a>

`scale` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the new scale

<a name='UMol.API.APIPython.delayedSetTransform(UnityEngine.Transform,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,bool,float).rot'></a>

`rot` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the new rotation

<a name='UMol.API.APIPython.delayedSetTransform(UnityEngine.Transform,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,bool,float).centerOfRotation'></a>

`centerOfRotation` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the new center of rotation

<a name='UMol.API.APIPython.delayedSetTransform(UnityEngine.Transform,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,bool,float).lerp'></a>

`lerp` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Use a linear interpolation between the current of the new values?

<a name='UMol.API.APIPython.delayedSetTransform(UnityEngine.Transform,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,bool,float).duration'></a>

`duration` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

Duration of the linear interpolation

#### Returns
[System.Collections.IEnumerator](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.IEnumerator 'System.Collections.IEnumerator')

<a name='UMol.API.APIPython.delete(string)'></a>

## APIPython.delete(string) Method

Delete a molecule based on its UnityMolStructure name.
Delete also all its UnityMolSelection and UnityMolRepresentation

```csharp
public static void delete(string structureName);
```
#### Parameters

<a name='UMol.API.APIPython.delete(string).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.deleteAllSelectionsStructure(string)'></a>

## APIPython.deleteAllSelectionsStructure(string) Method

Delete all representations of the given structure.

```csharp
public static void deleteAllSelectionsStructure(string structureName);
```
#### Parameters

<a name='UMol.API.APIPython.deleteAllSelectionsStructure(string).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The structure name concerned

<a name='UMol.API.APIPython.deleteRepresentationInSelection(string,string)'></a>

## APIPython.deleteRepresentationInSelection(string, string) Method

Delete every representation of type 'type' of the specified selection
type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"

```csharp
public static void deleteRepresentationInSelection(string selName, string type);
```
#### Parameters

<a name='UMol.API.APIPython.deleteRepresentationInSelection(string,string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.deleteRepresentationInSelection(string,string).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of representation

<a name='UMol.API.APIPython.deleteRepresentationsInSelection(string)'></a>

## APIPython.deleteRepresentationsInSelection(string) Method

Delete all representations of the specified selection

```csharp
public static void deleteRepresentationsInSelection(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.deleteRepresentationsInSelection(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.deleteSelection(string)'></a>

## APIPython.deleteSelection(string) Method

Delete selection 'selName' and all its representations

```csharp
public static void deleteSelection(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.deleteSelection(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.disableDepthCueing()'></a>

## APIPython.disableDepthCueing() Method

Disable depth cueing effect

```csharp
public static void disableDepthCueing();
```

<a name='UMol.API.APIPython.disableDOF()'></a>

## APIPython.disableDOF() Method

Disable DOF (Depth of field) effect
<remarks>Not available in VR</remarks>

```csharp
public static void disableDOF();
```

<a name='UMol.API.APIPython.disableExternalCommands()'></a>

## APIPython.disableExternalCommands() Method

Disable the TCP server command.

```csharp
public static void disableExternalCommands();
```

<a name='UMol.API.APIPython.disableLimitedView(string,string)'></a>

## APIPython.disableLimitedView(string, string) Method

Disable the limited view which is a part of the representation inside a sphere.
<remarks>Only works with surface or cartoon types for now</remarks>

```csharp
public static void disableLimitedView(string selName, string type);
```
#### Parameters

<a name='UMol.API.APIPython.disableLimitedView(string,string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.disableLimitedView(string,string).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

<a name='UMol.API.APIPython.disableOutline()'></a>

## APIPython.disableOutline() Method

Disable outline effect

```csharp
public static void disableOutline();
```

<a name='UMol.API.APIPython.disconnectIMD(string)'></a>

## APIPython.disconnectIMD(string) Method

Disconnect from the IMD simulation for the specified structure

```csharp
public static void disconnectIMD(string structureName);
```
#### Parameters

<a name='UMol.API.APIPython.disconnectIMD(string).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The name of the 1st structure concerned

<a name='UMol.API.APIPython.drawCartoonAsBfactorTube(string,bool)'></a>

## APIPython.drawCartoonAsBfactorTube(string, bool) Method

Draw cartoon representation as tube with Bfactor as a tube size for a selection

```csharp
public static void drawCartoonAsBfactorTube(string selName, bool drawAsBTube=true);
```
#### Parameters

<a name='UMol.API.APIPython.drawCartoonAsBfactorTube(string,bool).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.drawCartoonAsBfactorTube(string,bool).drawAsBTube'></a>

`drawAsBTube` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Whether to draw it as a tube or not

<a name='UMol.API.APIPython.drawCartoonAsTube(string,bool)'></a>

## APIPython.drawCartoonAsTube(string, bool) Method

Draw cartoon representation as tube for a selection

```csharp
public static void drawCartoonAsTube(string selName, bool drawAsTube=true);
```
#### Parameters

<a name='UMol.API.APIPython.drawCartoonAsTube(string,bool).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.drawCartoonAsTube(string,bool).drawAsTube'></a>

`drawAsTube` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Whether to draw it as a tube or not

<a name='UMol.API.APIPython.duplicateSelection(string)'></a>

## APIPython.duplicateSelection(string) Method

Duplicate selection 'selName' without the representations
The duplicated selection  will have the same name of the original one + a suffix number

```csharp
public static string duplicateSelection(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.duplicateSelection(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

#### Returns
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The new name of the duplicated selection

<a name='UMol.API.APIPython.enableDepthCueing()'></a>

## APIPython.enableDepthCueing() Method

Enable depth cueing effect

```csharp
public static void enableDepthCueing();
```

<a name='UMol.API.APIPython.enableDOF()'></a>

## APIPython.enableDOF() Method

Enable DOF (Depth of field) effect
<remarks>Not available in VR</remarks>

```csharp
public static void enableDOF();
```

<a name='UMol.API.APIPython.enableLimitedView(string,string)'></a>

## APIPython.enableLimitedView(string, string) Method

Show only a part of the representation inside a sphere.
<remarks>Only works with surface or cartoon types for now</remarks>

```csharp
public static void enableLimitedView(string selName, string type);
```
#### Parameters

<a name='UMol.API.APIPython.enableLimitedView(string,string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.enableLimitedView(string,string).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

<a name='UMol.API.APIPython.enableOutline()'></a>

## APIPython.enableOutline() Method

Enable outline post-process effect

```csharp
public static void enableOutline();
```

<a name='UMol.API.APIPython.ExecuteCommand(string,bool)'></a>

## APIPython.ExecuteCommand(string, bool) Method

Allow to call python API commands and record them in the history from C#

```csharp
public static bool ExecuteCommand(string command, bool force=false);
```
#### Parameters

<a name='UMol.API.APIPython.ExecuteCommand(string,bool).command'></a>

`command` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

command to execute

<a name='UMol.API.APIPython.ExecuteCommand(string,bool).force'></a>

`force` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Force the execution of the command

#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')
True if success. False otherwise

<a name='UMol.API.APIPython.exportRepsToFBXFile(string,string,bool)'></a>

## APIPython.exportRepsToFBXFile(string, string, bool) Method

Export the given structure to an FBX file containing several meshes
BondOrder/Point/Hbonds/Fieldlines are ignored

```csharp
public static void exportRepsToFBXFile(string structureName, string fullPath, bool withAO=true);
```
#### Parameters

<a name='UMol.API.APIPython.exportRepsToFBXFile(string,string,bool).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure name concerned

<a name='UMol.API.APIPython.exportRepsToFBXFile(string,string,bool).fullPath'></a>

`fullPath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the path to write the .fbx file

<a name='UMol.API.APIPython.exportRepsToFBXFile(string,string,bool).withAO'></a>

`withAO` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

whether Ambient Occlusion is exported

### Remarks
Only available on Windows or Mac

<a name='UMol.API.APIPython.exportRepsToOBJFile(string,string,bool)'></a>

## APIPython.exportRepsToOBJFile(string, string, bool) Method

Export the given structure to an OBJ file containing several meshes
BondOrder/Point/Hbonds are ignored

```csharp
public static void exportRepsToOBJFile(string structureName, string fullPath, bool withAO=true);
```
#### Parameters

<a name='UMol.API.APIPython.exportRepsToOBJFile(string,string,bool).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure name concerned

<a name='UMol.API.APIPython.exportRepsToOBJFile(string,string,bool).fullPath'></a>

`fullPath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the path to write the .obj file

<a name='UMol.API.APIPython.exportRepsToOBJFile(string,string,bool).withAO'></a>

`withAO` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

whether Ambient Occlusion is exported

<a name='UMol.API.APIPython.fetch_URL(string,bool,bool,bool,bool,bool,int)'></a>

## APIPython.fetch_URL(string, bool, bool, bool, bool, bool, int) Method

Fetch a remote molecular file (pdb/mmcif/gro/mol2/sdf/xyz formats) from a URL

```csharp
public static UMol.UnityMolStructure fetch_URL(string urlPath, bool readHetm=true, bool forceDSSP=false, bool showDefaultRep=true, bool center=true, bool modelsAsTraj=true, int forceStructureType=-1);
```
#### Parameters

<a name='UMol.API.APIPython.fetch_URL(string,bool,bool,bool,bool,bool,int).urlPath'></a>

`urlPath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

URL of the file

<a name='UMol.API.APIPython.fetch_URL(string,bool,bool,bool,bool,bool,int).readHetm'></a>

`readHetm` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Read hetero atoms?

<a name='UMol.API.APIPython.fetch_URL(string,bool,bool,bool,bool,bool,int).forceDSSP'></a>

`forceDSSP` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Compute secondary structure through DSSP?

<a name='UMol.API.APIPython.fetch_URL(string,bool,bool,bool,bool,bool,int).showDefaultRep'></a>

`showDefaultRep` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Show the default representation?

<a name='UMol.API.APIPython.fetch_URL(string,bool,bool,bool,bool,bool,int).center'></a>

`center` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

center the molecule?

<a name='UMol.API.APIPython.fetch_URL(string,bool,bool,bool,bool,bool,int).modelsAsTraj'></a>

`modelsAsTraj` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

If several models are present in the file, treat them as a trajectory?

<a name='UMol.API.APIPython.fetch_URL(string,bool,bool,bool,bool,bool,int).forceStructureType'></a>

`forceStructureType` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

Type of molecular file (-1 = auto-detect / 0 = standard / 1 = CG / 2 = OPEP / 3 = HIRERNA)

#### Returns
[UnityMolStructure](UMol.UnityMolStructure.md 'UMol.UnityMolStructure')
the molecule as a UnityMolStructure

<a name='UMol.API.APIPython.fetch(string,bool,bool,bool,bool,bool,bool,int,bool)'></a>

## APIPython.fetch(string, bool, bool, bool, bool, bool, bool, int, bool) Method

Fetch a PDB Id from RCSB server (pdb or mmcif zipped)

```csharp
public static UMol.UnityMolStructure fetch(string PDBId, bool usemmCIF=true, bool readHetm=true, bool forceDSSP=false, bool showDefaultRep=true, bool center=true, bool modelsAsTraj=true, int forceStructureType=-1, bool bioAssembly=false);
```
#### Parameters

<a name='UMol.API.APIPython.fetch(string,bool,bool,bool,bool,bool,bool,int,bool).PDBId'></a>

`PDBId` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

PDB id

<a name='UMol.API.APIPython.fetch(string,bool,bool,bool,bool,bool,bool,int,bool).usemmCIF'></a>

`usemmCIF` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Use mmcif type?

<a name='UMol.API.APIPython.fetch(string,bool,bool,bool,bool,bool,bool,int,bool).readHetm'></a>

`readHetm` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Read hetero atoms?

<a name='UMol.API.APIPython.fetch(string,bool,bool,bool,bool,bool,bool,int,bool).forceDSSP'></a>

`forceDSSP` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Compute secondary structure through DSSP?

<a name='UMol.API.APIPython.fetch(string,bool,bool,bool,bool,bool,bool,int,bool).showDefaultRep'></a>

`showDefaultRep` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Show the default representation?

<a name='UMol.API.APIPython.fetch(string,bool,bool,bool,bool,bool,bool,int,bool).center'></a>

`center` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

center the molecule?

<a name='UMol.API.APIPython.fetch(string,bool,bool,bool,bool,bool,bool,int,bool).modelsAsTraj'></a>

`modelsAsTraj` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

If several models are present in the file, treat them as a trajectory?

<a name='UMol.API.APIPython.fetch(string,bool,bool,bool,bool,bool,bool,int,bool).forceStructureType'></a>

`forceStructureType` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

Type of molecular file (-1 = auto-detect / 0 = standard / 1 = CG / 2 = OPEP / 3 = HIRERNA)

<a name='UMol.API.APIPython.fetch(string,bool,bool,bool,bool,bool,bool,int,bool).bioAssembly'></a>

`bioAssembly` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Show the macromolecular assembly?

#### Returns
[UnityMolStructure](UMol.UnityMolStructure.md 'UMol.UnityMolStructure')
the molecule as a UnityMolStructure

<a name='UMol.API.APIPython.getHyperBallMetaphore(string)'></a>

## APIPython.getHyperBallMetaphore(string) Method

Get the hyperball metaphor of given selection as a string
Possible output are : "Smooth", "BallsAndSticks", "VdW", "Licorice", "Hidden" or "".

```csharp
public static string getHyperBallMetaphore(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.getHyperBallMetaphore(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The name of the selection concerned

#### Returns
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
the metaphor of the HyperBall. Empty if no HB representation is available for the selection.

<a name='UMol.API.APIPython.getLimitedView(string,string)'></a>

## APIPython.getLimitedView(string, string) Method

Test if the limited view is active or not.
<remarks>Only works with surface or cartoon types for now</remarks>

```csharp
public static bool getLimitedView(string selName, string type);
```
#### Parameters

<a name='UMol.API.APIPython.getLimitedView(string,string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.getLimitedView(string,string).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

<a name='UMol.API.APIPython.getLimitedViewCenter(string,string)'></a>

## APIPython.getLimitedViewCenter(string, string) Method

Retrieve the current center of the limited view

```csharp
public static UnityEngine.Vector3 getLimitedViewCenter(string selName, string type);
```
#### Parameters

<a name='UMol.API.APIPython.getLimitedViewCenter(string,string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.getLimitedViewCenter(string,string).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

#### Returns
[UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')
The current center

<a name='UMol.API.APIPython.getManipulationManager()'></a>

## APIPython.getManipulationManager() Method

Get the current ManipulationManager, creates one if there is none

```csharp
public static UMol.ManipulationManager getManipulationManager();
```

#### Returns
[ManipulationManager](UMol.ManipulationManager.md 'UMol.ManipulationManager')
the ManipulationManager

<a name='UMol.API.APIPython.getMolParentTransform()'></a>

## APIPython.getMolParentTransform() Method

Save as a string all the Transform information of the LoadedMolecules GameObject

```csharp
public static string getMolParentTransform();
```

#### Returns
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
the string

<a name='UMol.API.APIPython.getRepType(string)'></a>

## APIPython.getRepType(string) Method

Return a RepType object from the name of the type of representation encoded in a string
If no match is found, a Reptype object with no representation for atom and bond will be return

```csharp
public static UMol.RepType getRepType(string type);
```
#### Parameters

<a name='UMol.API.APIPython.getRepType(string).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the name of the type of representation

#### Returns
[UMol.RepType](https://docs.microsoft.com/en-us/dotnet/api/UMol.RepType 'UMol.RepType')
a RepType object corresponding

<a name='UMol.API.APIPython.getSelectionListString()'></a>

## APIPython.getSelectionListString() Method

Return as a string the list of selections name

```csharp
public static string getSelectionListString();
```

#### Returns
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
the string

### Example
[sel1, sel2, sel3]

<a name='UMol.API.APIPython.getStructureGroup(string)'></a>

## APIPython.getStructureGroup(string) Method

Utility function to be able to get the group of the structure
This group is used to be able to move all the loaded molecules in the same group
Groups can be between 0 and 9 included

```csharp
public static int getStructureGroup(string structureName);
```
#### Parameters

<a name='UMol.API.APIPython.getStructureGroup(string).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

#### Returns
[System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')
The group of the structure. -1 means an issue.

<a name='UMol.API.APIPython.getStructureListString()'></a>

## APIPython.getStructureListString() Method

Return as a string the list of structures name

```csharp
public static string getStructureListString();
```

#### Returns
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
the string

### Example
[struct1, struct2, struct3]

<a name='UMol.API.APIPython.getStructurePositionRotation(string,UnityEngine.Vector3,UnityEngine.Vector3)'></a>

## APIPython.getStructurePositionRotation(string, Vector3, Vector3) Method

Get the current position and rotation of the given structure

```csharp
public static void getStructurePositionRotation(string structureName, ref UnityEngine.Vector3 pos, ref UnityEngine.Vector3 rot);
```
#### Parameters

<a name='UMol.API.APIPython.getStructurePositionRotation(string,UnityEngine.Vector3,UnityEngine.Vector3).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure name concerned

<a name='UMol.API.APIPython.getStructurePositionRotation(string,UnityEngine.Vector3,UnityEngine.Vector3).pos'></a>

`pos` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

Reference of the position

<a name='UMol.API.APIPython.getStructurePositionRotation(string,UnityEngine.Vector3,UnityEngine.Vector3).rot'></a>

`rot` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

Reference of the rotation

<a name='UMol.API.APIPython.getStructuresOfGroup(int)'></a>

## APIPython.getStructuresOfGroup(int) Method

Utility function to be able to get all structures of the group
his group is used to be able to move all the loaded molecules in the same group
roups can be between 0 and 9 included

```csharp
public static System.Collections.Generic.HashSet<UMol.UnityMolStructure> getStructuresOfGroup(int group);
```
#### Parameters

<a name='UMol.API.APIPython.getStructuresOfGroup(int).group'></a>

`group` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

ID of the group

#### Returns
[System.Collections.Generic.HashSet&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.HashSet-1 'System.Collections.Generic.HashSet`1')[UnityMolStructure](UMol.UnityMolStructure.md 'UMol.UnityMolStructure')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.HashSet-1 'System.Collections.Generic.HashSet`1')
a list of UnityMolStructure belonging to the group. Can be empty.

<a name='UMol.API.APIPython.getSurfaceType(string)'></a>

## APIPython.getSurfaceType(string) Method

Get the current type of surface of given selection as a string
Possible output are : "Solid", "Wireframe", "Transparent" or "".

```csharp
public static string getSurfaceType(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.getSurfaceType(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The name of the selection concerned

#### Returns
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
the type of the surface. Empty if no surface representation is available for the selection.

<a name='UMol.API.APIPython.getTypeFromRepType(UMol.RepType)'></a>

## APIPython.getTypeFromRepType(RepType) Method

From a RepType object, return the type of representation as a string
If no match is found, return an empty string

```csharp
public static string getTypeFromRepType(UMol.RepType rept);
```
#### Parameters

<a name='UMol.API.APIPython.getTypeFromRepType(UMol.RepType).rept'></a>

`rept` [UMol.RepType](https://docs.microsoft.com/en-us/dotnet/api/UMol.RepType 'UMol.RepType')

the repType object

#### Returns
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
the corresponding string

<a name='UMol.API.APIPython.getVersion()'></a>

## APIPython.getVersion() Method

Return the version of UnityMol

```csharp
public static void getVersion();
```

<a name='UMol.API.APIPython.hide(string)'></a>

## APIPython.hide(string) Method

Hide all representations of type 'type'
type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"

```csharp
public static void hide(string type);
```
#### Parameters

<a name='UMol.API.APIPython.hide(string).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of representation

<a name='UMol.API.APIPython.hideBoundingBox(string)'></a>

## APIPython.hideBoundingBox(string) Method

Hide bounding box around the structure
<remarks>This box is based on the max/min coordinates of the atoms. It is not the CRYSTAL box or the simulation box</remarks>

```csharp
public static void hideBoundingBox(string structureName);
```
#### Parameters

<a name='UMol.API.APIPython.hideBoundingBox(string).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.hideDXLines(string)'></a>

## APIPython.hideDXLines(string) Method

Hide lines around the Density (DX) map

```csharp
public static void hideDXLines(string structureName);
```
#### Parameters

<a name='UMol.API.APIPython.hideDXLines(string).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.hideSelection(string,string)'></a>

## APIPython.hideSelection(string, string) Method

Hide every representation of type 'type' of the specified selection
type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"

```csharp
public static void hideSelection(string selName, string type);
```
#### Parameters

<a name='UMol.API.APIPython.hideSelection(string,string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.hideSelection(string,string).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of representation

<a name='UMol.API.APIPython.hideSelection(string)'></a>

## APIPython.hideSelection(string) Method

Hide all representations of the selection named 'selName'

```csharp
public static void hideSelection(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.hideSelection(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned.

<a name='UMol.API.APIPython.hideStructureAllRepresentations(string)'></a>

## APIPython.hideStructureAllRepresentations(string) Method

Hide all representations of the given structure.

```csharp
public static void hideStructureAllRepresentations(string structureName);
```
#### Parameters

<a name='UMol.API.APIPython.hideStructureAllRepresentations(string).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The structure name concerned

<a name='UMol.API.APIPython.isATrajectoryPlaying()'></a>

## APIPython.isATrajectoryPlaying() Method

Test if a trajectory is playing for any loaded molecule

```csharp
public static bool isATrajectoryPlaying();
```

#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')
True if a trajectory is playing. False otherwise

<a name='UMol.API.APIPython.isSurfaceAOOn(string)'></a>

## APIPython.isSurfaceAOOn(string) Method

Is Ambient Occlusion for surface representations of a given selection activate or not?

```csharp
public static bool isSurfaceAOOn(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.isSurfaceAOOn(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')
True if AO activated. False otherwise

<a name='UMol.API.APIPython.last()'></a>

## APIPython.last() Method

Return the lastly loaded UnityMolStructure

```csharp
public static UMol.UnityMolStructure last();
```

#### Returns
[UnityMolStructure](UMol.UnityMolStructure.md 'UMol.UnityMolStructure')

<a name='UMol.API.APIPython.load(string,bool,bool,bool,bool,bool,int)'></a>

## APIPython.load(string, bool, bool, bool, bool, bool, int) Method

Load a local molecular file (pdb/mmcif/gro/mol2/sdf/xyz formats)

```csharp
public static UMol.UnityMolStructure load(string filePath, bool readHetm=true, bool forceDSSP=false, bool showDefaultRep=true, bool center=true, bool modelsAsTraj=true, int forceStructureType=-1);
```
#### Parameters

<a name='UMol.API.APIPython.load(string,bool,bool,bool,bool,bool,int).filePath'></a>

`filePath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Path of the local molecular file

<a name='UMol.API.APIPython.load(string,bool,bool,bool,bool,bool,int).readHetm'></a>

`readHetm` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Read hetero atoms?

<a name='UMol.API.APIPython.load(string,bool,bool,bool,bool,bool,int).forceDSSP'></a>

`forceDSSP` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Compute secondary structure through DSSP?

<a name='UMol.API.APIPython.load(string,bool,bool,bool,bool,bool,int).showDefaultRep'></a>

`showDefaultRep` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Show the default representation?

<a name='UMol.API.APIPython.load(string,bool,bool,bool,bool,bool,int).center'></a>

`center` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

center the molecule?

<a name='UMol.API.APIPython.load(string,bool,bool,bool,bool,bool,int).modelsAsTraj'></a>

`modelsAsTraj` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

If several models are present in the file, treat them as a trajectory?

<a name='UMol.API.APIPython.load(string,bool,bool,bool,bool,bool,int).forceStructureType'></a>

`forceStructureType` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

Type of molecular file (-1 = auto-detect / 0 = standard / 1 = CG / 2 = OPEP / 3 = HIRERNA)

#### Returns
[UnityMolStructure](UMol.UnityMolStructure.md 'UMol.UnityMolStructure')
the molecule as a UnityMolStructure

<a name='UMol.API.APIPython.loadBondsXML(string,string,int)'></a>

## APIPython.loadBondsXML(string, string, int) Method

Load an XML file containing covalent and non-covalent bonds
Possible bond types are: 'covalent' or 'db_geom', 'hbond' or 'h-bond' or 'hbond_weak',
'halogen', 'ionic', 'aromatic', 'hydrophobic', 'carbonyl'

```csharp
public static void loadBondsXML(string structureName, string filePath, int modelId=-1);
```
#### Parameters

<a name='UMol.API.APIPython.loadBondsXML(string,string,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.loadBondsXML(string,string,int).filePath'></a>

`filePath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

path of the XML file

<a name='UMol.API.APIPython.loadBondsXML(string,string,int).modelId'></a>

`modelId` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

ID of the model in the structure. -1 means current model

<a name='UMol.API.APIPython.loadDXmap(string,string)'></a>

## APIPython.loadDXmap(string, string) Method

Load a density map for a specific structure
This function creates a DXReader instance in the UnityMolStructure

```csharp
public static void loadDXmap(string structureName, string filePath);
```
#### Parameters

<a name='UMol.API.APIPython.loadDXmap(string,string).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.loadDXmap(string,string).filePath'></a>

`filePath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

path of the density map file

<a name='UMol.API.APIPython.loadFromString(string,string,bool,bool,bool,bool,bool,int)'></a>

## APIPython.loadFromString(string, string, bool, bool, bool, bool, bool, int) Method

Load a local molecular file (pdb/mmcif/gro/mol2/sdf/xyz formats) from a string

```csharp
public static UMol.UnityMolStructure loadFromString(string molecularName, string molecularData, bool readHetm=true, bool forceDSSP=false, bool showDefaultRep=true, bool center=true, bool modelsAsTraj=true, int forceStructureType=-1);
```
#### Parameters

<a name='UMol.API.APIPython.loadFromString(string,string,bool,bool,bool,bool,bool,int).molecularName'></a>

`molecularName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the molecule

<a name='UMol.API.APIPython.loadFromString(string,string,bool,bool,bool,bool,bool,int).molecularData'></a>

`molecularData` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Molecular data

<a name='UMol.API.APIPython.loadFromString(string,string,bool,bool,bool,bool,bool,int).readHetm'></a>

`readHetm` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Read hetero atoms?

<a name='UMol.API.APIPython.loadFromString(string,string,bool,bool,bool,bool,bool,int).forceDSSP'></a>

`forceDSSP` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Compute secondary structure through DSSP?

<a name='UMol.API.APIPython.loadFromString(string,string,bool,bool,bool,bool,bool,int).showDefaultRep'></a>

`showDefaultRep` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Show the default representation?

<a name='UMol.API.APIPython.loadFromString(string,string,bool,bool,bool,bool,bool,int).center'></a>

`center` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

center the molecule?

<a name='UMol.API.APIPython.loadFromString(string,string,bool,bool,bool,bool,bool,int).modelsAsTraj'></a>

`modelsAsTraj` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

If several models are present in the file, treat them as a trajectory?

<a name='UMol.API.APIPython.loadFromString(string,string,bool,bool,bool,bool,bool,int).forceStructureType'></a>

`forceStructureType` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

Type of molecular file (-1 = auto-detect / 0 = standard / 1 = CG / 2 = OPEP / 3 = HIRERNA)

#### Returns
[UnityMolStructure](UMol.UnityMolStructure.md 'UMol.UnityMolStructure')
the molecule as a UnityMolStructure

<a name='UMol.API.APIPython.loadHistoryScript(string)'></a>

## APIPython.loadHistoryScript(string) Method

Load a history file of commands (.py file)

```csharp
public static void loadHistoryScript(string filePath);
```
#### Parameters

<a name='UMol.API.APIPython.loadHistoryScript(string).filePath'></a>

`filePath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

path to read the history file

<a name='UMol.API.APIPython.loadMartiniITP(string,string)'></a>

## APIPython.loadMartiniITP(string, string) Method

Load a Martini ITP file to parse elastic network and secondary structure for a structure

```csharp
public static void loadMartiniITP(string structureName, string filePath);
```
#### Parameters

<a name='UMol.API.APIPython.loadMartiniITP(string,string).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.loadMartiniITP(string,string).filePath'></a>

`filePath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Path of the Martini ITP file

<a name='UMol.API.APIPython.loadPSFTopology(string,string,int)'></a>

## APIPython.loadPSFTopology(string, string, int) Method

Load topology information from a PSF file for a structure.

```csharp
public static void loadPSFTopology(string structureName, string psfPath, int modelId=-1);
```
#### Parameters

<a name='UMol.API.APIPython.loadPSFTopology(string,string,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.loadPSFTopology(string,string,int).psfPath'></a>

`psfPath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

path of the PSF file

<a name='UMol.API.APIPython.loadPSFTopology(string,string,int).modelId'></a>

`modelId` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

ID of the model in the structure. -1 means current model

<a name='UMol.API.APIPython.loadRTMaterialsJSONFile(string)'></a>

## APIPython.loadRTMaterialsJSONFile(string) Method

Read a Raytracing material(s) from a json file (VTK material files) and store it in the RT material bank

```csharp
public static void loadRTMaterialsJSONFile(string filePath);
```
#### Parameters

<a name='UMol.API.APIPython.loadRTMaterialsJSONFile(string).filePath'></a>

`filePath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

path of the JSON file

<a name='UMol.API.APIPython.loadScript(string)'></a>

## APIPython.loadScript(string) Method

Load a history file of commands (.py file)

```csharp
public static void loadScript(string filePath);
```
#### Parameters

<a name='UMol.API.APIPython.loadScript(string).filePath'></a>

`filePath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

path to read the history file

<a name='UMol.API.APIPython.loadTOPTopology(string,string,int,string)'></a>

## APIPython.loadTOPTopology(string, string, int, string) Method

Load topology information from a TOP file for a structure.
specialBondString

```csharp
public static void loadTOPTopology(string structureName, string topPath, int modelId=-1, string specialBondString="restrain");
```
#### Parameters

<a name='UMol.API.APIPython.loadTOPTopology(string,string,int,string).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.loadTOPTopology(string,string,int,string).topPath'></a>

`topPath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

path of the PSF file

<a name='UMol.API.APIPython.loadTOPTopology(string,string,int,string).modelId'></a>

`modelId` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

ID of the model in the structure. -1 means current model

<a name='UMol.API.APIPython.loadTOPTopology(string,string,int,string).specialBondString'></a>

`specialBondString` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

When not empty is used to create a selection containing only these special bonds,
            shown as hbondtube

<a name='UMol.API.APIPython.loadTraj(string,string)'></a>

## APIPython.loadTraj(string, string) Method

Load a trajectory file (XTC or TRR) for a structure
It creates a XDRFileReader in the corresponding UnityMolStructure and a TrajectoryPlayer

```csharp
public static void loadTraj(string structureName, string filePath);
```
#### Parameters

<a name='UMol.API.APIPython.loadTraj(string,string).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.loadTraj(string,string).filePath'></a>

`filePath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

path of the trajectory file

<a name='UMol.API.APIPython.ls()'></a>

## APIPython.ls() Method

Print the content of the current directory, outputs only the files

```csharp
public static System.Collections.Generic.List<string> ls();
```

#### Returns
[System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')

<a name='UMol.API.APIPython.mergeStructure(string,string,string)'></a>

## APIPython.mergeStructure(string, string, string) Method

Merge 2 UnityMolStructures using a different chain name to avoid conflict
Keep the name of the first UnityMolStructure

```csharp
public static void mergeStructure(string structureName, string structureName2, string chainName="Z");
```
#### Parameters

<a name='UMol.API.APIPython.mergeStructure(string,string,string).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The name of the 1st structure concerned

<a name='UMol.API.APIPython.mergeStructure(string,string,string).structureName2'></a>

`structureName2` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The name of the 2nd structure

<a name='UMol.API.APIPython.mergeStructure(string,string,string).chainName'></a>

`chainName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the name of the chain for the 2nd structure merged into the first one

<a name='UMol.API.APIPython.overrideBondsWithXML(string,int)'></a>

## APIPython.overrideBondsWithXML(string, int) Method

Override the current bonds of the model modelId of the structure 'structureName'
and saves the previous one in model.savedBonds

```csharp
public static void overrideBondsWithXML(string structureName, int modelId=-1);
```
#### Parameters

<a name='UMol.API.APIPython.overrideBondsWithXML(string,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.overrideBondsWithXML(string,int).modelId'></a>

`modelId` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

ID of the model in the structure. -1 means current model

<a name='UMol.API.APIPython.pauseVideo()'></a>

## APIPython.pauseVideo() Method

Pause recording

```csharp
public static void pauseVideo();
```

<a name='UMol.API.APIPython.pickTrajectoryFrames(string,string,int,int,int)'></a>

## APIPython.pickTrajectoryFrames(string, string, int, int, int) Method

Create a special selection containing frames from the trajectory

```csharp
public static string pickTrajectoryFrames(string structureName, string selectionQuery="all", int frameStart=0, int frameEnd=1, int step=1);
```
#### Parameters

<a name='UMol.API.APIPython.pickTrajectoryFrames(string,string,int,int,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.pickTrajectoryFrames(string,string,int,int,int).selectionQuery'></a>

`selectionQuery` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Selection query

<a name='UMol.API.APIPython.pickTrajectoryFrames(string,string,int,int,int).frameStart'></a>

`frameStart` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

starting frame in the trajectory

<a name='UMol.API.APIPython.pickTrajectoryFrames(string,string,int,int,int).frameEnd'></a>

`frameEnd` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

ending frame in the trajectory

<a name='UMol.API.APIPython.pickTrajectoryFrames(string,string,int,int,int).step'></a>

`step` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

Step between frames

#### Returns
[System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')
The selection name

<a name='UMol.API.APIPython.playSoundAtPosition(UnityEngine.Vector3)'></a>

## APIPython.playSoundAtPosition(Vector3) Method

Play a sonar sound at a world position

```csharp
public static void playSoundAtPosition(UnityEngine.Vector3 wpos);
```
#### Parameters

<a name='UMol.API.APIPython.playSoundAtPosition(UnityEngine.Vector3).wpos'></a>

`wpos` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the position

<a name='UMol.API.APIPython.pwd()'></a>

## APIPython.pwd() Method

Print the current directory

```csharp
public static void pwd();
```

<a name='UMol.API.APIPython.readJSONFieldlines(string,string)'></a>

## APIPython.readJSONFieldlines(string, string) Method

Read a JSON file and display fieldLines for the specified structure

```csharp
public static void readJSONFieldlines(string structureName, string filePath);
```
#### Parameters

<a name='UMol.API.APIPython.readJSONFieldlines(string,string).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.readJSONFieldlines(string,string).filePath'></a>

`filePath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

path of the JSON file

<a name='UMol.API.APIPython.readSessionFromFile(string)'></a>

## APIPython.readSessionFromFile(string) Method

Read a JSON file and restore the session.
Remove all molecules loaded previously first.

```csharp
public static void readSessionFromFile(string filePath);
```
#### Parameters

<a name='UMol.API.APIPython.readSessionFromFile(string).filePath'></a>

`filePath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

path to read the JSON file

<a name='UMol.API.APIPython.removeAnnotation2DText(UnityEngine.Vector2,float,string,UnityEngine.Color)'></a>

## APIPython.removeAnnotation2DText(Vector2, float, string, Color) Method

Remove a 2D annotation text over everything
The screenP defines the position based on the percentage from bottom/left to top/right of the screen
with 0/0 means bottom/left and 1/1 means top/right

```csharp
public static void removeAnnotation2DText(UnityEngine.Vector2 screenP, float scale, string text, UnityEngine.Color textCol);
```
#### Parameters

<a name='UMol.API.APIPython.removeAnnotation2DText(UnityEngine.Vector2,float,string,UnityEngine.Color).screenP'></a>

`screenP` [UnityEngine.Vector2](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector2 'UnityEngine.Vector2')

the position of the annotation

<a name='UMol.API.APIPython.removeAnnotation2DText(UnityEngine.Vector2,float,string,UnityEngine.Color).scale'></a>

`scale` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the scale of the annotation

<a name='UMol.API.APIPython.removeAnnotation2DText(UnityEngine.Vector2,float,string,UnityEngine.Color).text'></a>

`text` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the text of the annotation

<a name='UMol.API.APIPython.removeAnnotation2DText(UnityEngine.Vector2,float,string,UnityEngine.Color).textCol'></a>

`textCol` [UnityEngine.Color](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Color 'UnityEngine.Color')

the color of the text

<a name='UMol.API.APIPython.removeAnnotationAngle(string,int,string,int,string,int)'></a>

## APIPython.removeAnnotationAngle(string, int, string, int, string, int) Method

Remove an annotation of type "Angle" between 3 atoms.

```csharp
public static void removeAnnotationAngle(string structureName, int atomId, string structureName2, int atomId2, string structureName3, int atomId3);
```
#### Parameters

<a name='UMol.API.APIPython.removeAnnotationAngle(string,int,string,int,string,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the first atom

<a name='UMol.API.APIPython.removeAnnotationAngle(string,int,string,int,string,int).atomId'></a>

`atomId` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the first atom ID

<a name='UMol.API.APIPython.removeAnnotationAngle(string,int,string,int,string,int).structureName2'></a>

`structureName2` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the second atom

<a name='UMol.API.APIPython.removeAnnotationAngle(string,int,string,int,string,int).atomId2'></a>

`atomId2` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the second atom ID

<a name='UMol.API.APIPython.removeAnnotationAngle(string,int,string,int,string,int).structureName3'></a>

`structureName3` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the third atom

<a name='UMol.API.APIPython.removeAnnotationAngle(string,int,string,int,string,int).atomId3'></a>

`atomId3` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the third atom ID

<a name='UMol.API.APIPython.removeAnnotationArcLine(string,int,string,int,string,int)'></a>

## APIPython.removeAnnotationArcLine(string, int, string, int, string, int) Method

Remove an annotation of type "ArcLine" between 3 atoms.

```csharp
public static void removeAnnotationArcLine(string structureName, int atomId, string structureName2, int atomId2, string structureName3, int atomId3);
```
#### Parameters

<a name='UMol.API.APIPython.removeAnnotationArcLine(string,int,string,int,string,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the first atom

<a name='UMol.API.APIPython.removeAnnotationArcLine(string,int,string,int,string,int).atomId'></a>

`atomId` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the first atom ID

<a name='UMol.API.APIPython.removeAnnotationArcLine(string,int,string,int,string,int).structureName2'></a>

`structureName2` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the second atom

<a name='UMol.API.APIPython.removeAnnotationArcLine(string,int,string,int,string,int).atomId2'></a>

`atomId2` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the second atom ID

<a name='UMol.API.APIPython.removeAnnotationArcLine(string,int,string,int,string,int).structureName3'></a>

`structureName3` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the third atom

<a name='UMol.API.APIPython.removeAnnotationArcLine(string,int,string,int,string,int).atomId3'></a>

`atomId3` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the third atom ID

<a name='UMol.API.APIPython.removeAnnotationAtom(string,int)'></a>

## APIPython.removeAnnotationAtom(string, int) Method

Remove the annotation of an atom

```csharp
public static void removeAnnotationAtom(string structureName, int atomId);
```
#### Parameters

<a name='UMol.API.APIPython.removeAnnotationAtom(string,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the atom

<a name='UMol.API.APIPython.removeAnnotationAtom(string,int).atomId'></a>

`atomId` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the atom ID

<a name='UMol.API.APIPython.removeAnnotationAtomText(string,int,string)'></a>

## APIPython.removeAnnotationAtomText(string, int, string) Method

Remove the text annotation of an atom

```csharp
public static void removeAnnotationAtomText(string structureName, int atomId, string text);
```
#### Parameters

<a name='UMol.API.APIPython.removeAnnotationAtomText(string,int,string).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the atom

<a name='UMol.API.APIPython.removeAnnotationAtomText(string,int,string).atomId'></a>

`atomId` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the atom ID

<a name='UMol.API.APIPython.removeAnnotationAtomText(string,int,string).text'></a>

`text` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The text of the annotation

<a name='UMol.API.APIPython.removeAnnotationDihedralAngle(string,int,string,int,string,int,string,int)'></a>

## APIPython.removeAnnotationDihedralAngle(string, int, string, int, string, int, string, int) Method

Remove an annotation of type "Dihedral" between 4 atoms.

```csharp
public static void removeAnnotationDihedralAngle(string structureName, int atomId, string structureName2, int atomId2, string structureName3, int atomId3, string structureName4, int atomId4);
```
#### Parameters

<a name='UMol.API.APIPython.removeAnnotationDihedralAngle(string,int,string,int,string,int,string,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the first atom

<a name='UMol.API.APIPython.removeAnnotationDihedralAngle(string,int,string,int,string,int,string,int).atomId'></a>

`atomId` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the first atom ID

<a name='UMol.API.APIPython.removeAnnotationDihedralAngle(string,int,string,int,string,int,string,int).structureName2'></a>

`structureName2` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the second atom

<a name='UMol.API.APIPython.removeAnnotationDihedralAngle(string,int,string,int,string,int,string,int).atomId2'></a>

`atomId2` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the second atom ID

<a name='UMol.API.APIPython.removeAnnotationDihedralAngle(string,int,string,int,string,int,string,int).structureName3'></a>

`structureName3` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the third atom

<a name='UMol.API.APIPython.removeAnnotationDihedralAngle(string,int,string,int,string,int,string,int).atomId3'></a>

`atomId3` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the third atom ID

<a name='UMol.API.APIPython.removeAnnotationDihedralAngle(string,int,string,int,string,int,string,int).structureName4'></a>

`structureName4` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the fourth atom

<a name='UMol.API.APIPython.removeAnnotationDihedralAngle(string,int,string,int,string,int,string,int).atomId4'></a>

`atomId4` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the fourth atom ID

<a name='UMol.API.APIPython.removeAnnotationDistance(string,int,string,int)'></a>

## APIPython.removeAnnotationDistance(string, int, string, int) Method

Remove an annotation of type "Distance" between 2 atoms

```csharp
public static void removeAnnotationDistance(string structureName, int atomId, string structureName2, int atomId2);
```
#### Parameters

<a name='UMol.API.APIPython.removeAnnotationDistance(string,int,string,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the first atom

<a name='UMol.API.APIPython.removeAnnotationDistance(string,int,string,int).atomId'></a>

`atomId` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the first atom ID

<a name='UMol.API.APIPython.removeAnnotationDistance(string,int,string,int).structureName2'></a>

`structureName2` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the second atom

<a name='UMol.API.APIPython.removeAnnotationDistance(string,int,string,int).atomId2'></a>

`atomId2` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the second atom ID

<a name='UMol.API.APIPython.removeAnnotationLine(string,int,string,int)'></a>

## APIPython.removeAnnotationLine(string, int, string, int) Method

Remove an annotation line between 2 atoms

```csharp
public static void removeAnnotationLine(string structureName, int atomId, string structureName2, int atomId2);
```
#### Parameters

<a name='UMol.API.APIPython.removeAnnotationLine(string,int,string,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the first atom

<a name='UMol.API.APIPython.removeAnnotationLine(string,int,string,int).atomId'></a>

`atomId` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the first atom ID

<a name='UMol.API.APIPython.removeAnnotationLine(string,int,string,int).structureName2'></a>

`structureName2` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the second atom

<a name='UMol.API.APIPython.removeAnnotationLine(string,int,string,int).atomId2'></a>

`atomId2` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the second atom ID

<a name='UMol.API.APIPython.removeAnnotationRotatingArrow(string,int,string,int)'></a>

## APIPython.removeAnnotationRotatingArrow(string, int, string, int) Method

Remove an annotation of type "arrow" between 2 atoms

```csharp
public static void removeAnnotationRotatingArrow(string structureName, int atomId, string structureName2, int atomId2);
```
#### Parameters

<a name='UMol.API.APIPython.removeAnnotationRotatingArrow(string,int,string,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the first atom

<a name='UMol.API.APIPython.removeAnnotationRotatingArrow(string,int,string,int).atomId'></a>

`atomId` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the first atom ID

<a name='UMol.API.APIPython.removeAnnotationRotatingArrow(string,int,string,int).structureName2'></a>

`structureName2` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure which belongs the second atom

<a name='UMol.API.APIPython.removeAnnotationRotatingArrow(string,int,string,int).atomId2'></a>

`atomId2` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the second atom ID

<a name='UMol.API.APIPython.removeAnnotationSphere(UnityEngine.Vector3,float)'></a>

## APIPython.removeAnnotationSphere(Vector3, float) Method

Remove an annotation sphere of the given parameters

```csharp
public static void removeAnnotationSphere(UnityEngine.Vector3 worldP, float scale=1f);
```
#### Parameters

<a name='UMol.API.APIPython.removeAnnotationSphere(UnityEngine.Vector3,float).worldP'></a>

`worldP` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the center position of the sphere (World reference)

<a name='UMol.API.APIPython.removeAnnotationSphere(UnityEngine.Vector3,float).scale'></a>

`scale` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the scale of the sphere

<a name='UMol.API.APIPython.removeAnnotationWorldText(UnityEngine.Vector3,float,string,UnityEngine.Color)'></a>

## APIPython.removeAnnotationWorldText(Vector3, float, string, Color) Method

Remove a global text annotation of the scene

```csharp
public static void removeAnnotationWorldText(UnityEngine.Vector3 worldP, float scale, string text, UnityEngine.Color textCol);
```
#### Parameters

<a name='UMol.API.APIPython.removeAnnotationWorldText(UnityEngine.Vector3,float,string,UnityEngine.Color).worldP'></a>

`worldP` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the center position of the annotation (World reference)

<a name='UMol.API.APIPython.removeAnnotationWorldText(UnityEngine.Vector3,float,string,UnityEngine.Color).scale'></a>

`scale` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the scale of the annotation

<a name='UMol.API.APIPython.removeAnnotationWorldText(UnityEngine.Vector3,float,string,UnityEngine.Color).text'></a>

`text` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the text of the annotation

<a name='UMol.API.APIPython.removeAnnotationWorldText(UnityEngine.Vector3,float,string,UnityEngine.Color).textCol'></a>

`textCol` [UnityEngine.Color](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Color 'UnityEngine.Color')

the color of the text

<a name='UMol.API.APIPython.removeFromSelection(string,string,bool,bool)'></a>

## APIPython.removeFromSelection(string, string, bool, bool) Method

Look for an existing selection named 'name' and remove atoms to it based on MDAnalysis selection language

```csharp
public static void removeFromSelection(string selMDA, string name="selection", bool silent=false, bool allModels=false);
```
#### Parameters

<a name='UMol.API.APIPython.removeFromSelection(string,string,bool,bool).selMDA'></a>

`selMDA` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the selection query

<a name='UMol.API.APIPython.removeFromSelection(string,string,bool,bool).name'></a>

`name` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the name of the selection.

<a name='UMol.API.APIPython.removeFromSelection(string,string,bool,bool).silent'></a>

`silent` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Print the new selection in the console

<a name='UMol.API.APIPython.removeFromSelection(string,string,bool,bool).allModels'></a>

`allModels` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

apply the selection to all models of structures concerned?

<a name='UMol.API.APIPython.removeLastDrawLine(string,int)'></a>

## APIPython.removeLastDrawLine(string, int) Method

Remove an annotation of type "DrawLine" linked to a structure.

```csharp
public static void removeLastDrawLine(string structureName, int id);
```
#### Parameters

<a name='UMol.API.APIPython.removeLastDrawLine(string,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the name of the structure concerned

<a name='UMol.API.APIPython.removeLastDrawLine(string,int).id'></a>

`id` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the id of the annotation

<a name='UMol.API.APIPython.removeSelectionFromTour(string)'></a>

## APIPython.removeSelectionFromTour(string) Method

Remove a selection to the Tour.

```csharp
public static void removeSelectionFromTour(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.removeSelectionFromTour(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the selection name concerned

<a name='UMol.API.APIPython.removeSelectionKeyword(string,string)'></a>

## APIPython.removeSelectionKeyword(string, string) Method

Remove a keyword to the selection language for the selection 'selName'

```csharp
public static void removeSelectionKeyword(string keyword, string selName);
```
#### Parameters

<a name='UMol.API.APIPython.removeSelectionKeyword(string,string).keyword'></a>

`keyword` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the keyword to remove

<a name='UMol.API.APIPython.removeSelectionKeyword(string,string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the selection name concerned.

<a name='UMol.API.APIPython.removeWorldAnnotationLine(UnityEngine.Vector3,UnityEngine.Vector3,float,UnityEngine.Color)'></a>

## APIPython.removeWorldAnnotationLine(Vector3, Vector3, float, Color) Method

Remove a global annotation line between 2 positions.

```csharp
public static void removeWorldAnnotationLine(UnityEngine.Vector3 p1, UnityEngine.Vector3 p2, float sizeLine, UnityEngine.Color lineCol);
```
#### Parameters

<a name='UMol.API.APIPython.removeWorldAnnotationLine(UnityEngine.Vector3,UnityEngine.Vector3,float,UnityEngine.Color).p1'></a>

`p1` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the starting position

<a name='UMol.API.APIPython.removeWorldAnnotationLine(UnityEngine.Vector3,UnityEngine.Vector3,float,UnityEngine.Color).p2'></a>

`p2` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the ending position

<a name='UMol.API.APIPython.removeWorldAnnotationLine(UnityEngine.Vector3,UnityEngine.Vector3,float,UnityEngine.Color).sizeLine'></a>

`sizeLine` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the size of the line

<a name='UMol.API.APIPython.removeWorldAnnotationLine(UnityEngine.Vector3,UnityEngine.Vector3,float,UnityEngine.Color).lineCol'></a>

`lineCol` [UnityEngine.Color](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Color 'UnityEngine.Color')

the color of the line

<a name='UMol.API.APIPython.renameSelection(string,string)'></a>

## APIPython.renameSelection(string, string) Method

Change the selection named 'oldSelName' to 'newSelName'

```csharp
public static bool renameSelection(string oldSelName, string newSelName);
```
#### Parameters

<a name='UMol.API.APIPython.renameSelection(string,string).oldSelName'></a>

`oldSelName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the current name of the selection

<a name='UMol.API.APIPython.renameSelection(string,string).newSelName'></a>

`newSelName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the new name of the selection

#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')
True if success. False otherwise

<a name='UMol.API.APIPython.reset()'></a>

## APIPython.reset() Method

Delete all the loaded molecules/structures

```csharp
public static void reset();
```

<a name='UMol.API.APIPython.resetColorSelection(string,string)'></a>

## APIPython.resetColorSelection(string, string) Method

Reset the color of all representations of type 'type' in selection to the default value
type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"

```csharp
public static void resetColorSelection(string selName, string type);
```
#### Parameters

<a name='UMol.API.APIPython.resetColorSelection(string,string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.resetColorSelection(string,string).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the type of representation to modify color. "" means all representations

<a name='UMol.API.APIPython.resetRep(string)'></a>

## APIPython.resetRep(string) Method

Restore all representations of a structure to the default representation

```csharp
public static void resetRep(string structureName);
```
#### Parameters

<a name='UMol.API.APIPython.resetRep(string).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.restoreBonds(string,int)'></a>

## APIPython.restoreBonds(string, int) Method

Restore bonds saved in the 'model.savedBonds'

```csharp
public static void restoreBonds(string structureName, int modelId=-1);
```
#### Parameters

<a name='UMol.API.APIPython.restoreBonds(string,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.restoreBonds(string,int).modelId'></a>

`modelId` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

ID of the model in the structure. -1 means current model

<a name='UMol.API.APIPython.saveDockingState(string)'></a>

## APIPython.saveDockingState(string) Method

Save the current positions of the loaded structures in a single PDB file
If no filepath provided, used a predefined filename

```csharp
public static void saveDockingState(string filepath=null);
```
#### Parameters

<a name='UMol.API.APIPython.saveDockingState(string).filepath'></a>

`filepath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

path to write the PDB file

<a name='UMol.API.APIPython.saveHistoryScript(string)'></a>

## APIPython.saveHistoryScript(string) Method

Save the history of commands executed in a file

```csharp
public static void saveHistoryScript(string filepath);
```
#### Parameters

<a name='UMol.API.APIPython.saveHistoryScript(string).filepath'></a>

`filepath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

path to write the file

<a name='UMol.API.APIPython.saveScript(string)'></a>

## APIPython.saveScript(string) Method

Save the history of commands executed in a file

```csharp
public static void saveScript(string filepath);
```
#### Parameters

<a name='UMol.API.APIPython.saveScript(string).filepath'></a>

`filepath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

path to write the file

<a name='UMol.API.APIPython.saveToPDB(string,string,bool)'></a>

## APIPython.saveToPDB(string, string, bool) Method

Save current atom positions of the selection to a PDB file
World atom positions are transformed to be relative to the first structure in the selection

```csharp
public static void saveToPDB(string selName, string fullPath, bool writeSSinfo=false);
```
#### Parameters

<a name='UMol.API.APIPython.saveToPDB(string,string,bool).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The name of the selection concerned

<a name='UMol.API.APIPython.saveToPDB(string,string,bool).fullPath'></a>

`fullPath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

path to write the PDB file

<a name='UMol.API.APIPython.saveToPDB(string,string,bool).writeSSinfo'></a>

`writeSSinfo` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Save the secondary structure information in the file?

<a name='UMol.API.APIPython.screenshot(string,int,int,bool)'></a>

## APIPython.screenshot(string, int, int, bool) Method

Take a screenshot of the current viewpoint with a specific resolution

```csharp
public static void screenshot(string filePath, int resolutionWidth=1280, int resolutionHeight=720, bool transparentBG=false);
```
#### Parameters

<a name='UMol.API.APIPython.screenshot(string,int,int,bool).filePath'></a>

`filePath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Path to save the screenshot

<a name='UMol.API.APIPython.screenshot(string,int,int,bool).resolutionWidth'></a>

`resolutionWidth` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

Width of the image

<a name='UMol.API.APIPython.screenshot(string,int,int,bool).resolutionHeight'></a>

`resolutionHeight` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

Height of the image

<a name='UMol.API.APIPython.screenshot(string,int,int,bool).transparentBG'></a>

`transparentBG` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Set the background to transparent?

<a name='UMol.API.APIPython.select(string,string,bool,bool,bool,bool,bool,bool,bool)'></a>

## APIPython.select(string, string, bool, bool, bool, bool, bool, bool, bool) Method

Create a UnityMolSelection based on MDAnalysis selection language (https://www.mdanalysis.org/docs/documentation_pages/selections.html)
Returns a UnityMolSelection object, adding it to the selection manager if createSelection is true
If a selection with the same name already exists and addToExisting is true, add atoms to the already existing selection
Set forceCreate to true if the selection is empty but still need to generate the selection

```csharp
public static UMol.UnityMolSelection select(string selMDA, string name="selection", bool createSelection=true, bool addToExisting=false, bool silent=false, bool setAsCurrentSelection=true, bool forceCreate=false, bool allModels=false, bool addToSelectionKeyword=true);
```
#### Parameters

<a name='UMol.API.APIPython.select(string,string,bool,bool,bool,bool,bool,bool,bool).selMDA'></a>

`selMDA` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the selection query

<a name='UMol.API.APIPython.select(string,string,bool,bool,bool,bool,bool,bool,bool).name'></a>

`name` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the name of the selection.

<a name='UMol.API.APIPython.select(string,string,bool,bool,bool,bool,bool,bool,bool).createSelection'></a>

`createSelection` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Add the selection to the manager?

<a name='UMol.API.APIPython.select(string,string,bool,bool,bool,bool,bool,bool,bool).addToExisting'></a>

`addToExisting` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

add the atoms of this query to an existing selection if present?

<a name='UMol.API.APIPython.select(string,string,bool,bool,bool,bool,bool,bool,bool).silent'></a>

`silent` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Print the new selection in the console

<a name='UMol.API.APIPython.select(string,string,bool,bool,bool,bool,bool,bool,bool).setAsCurrentSelection'></a>

`setAsCurrentSelection` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Make the selection the current one

<a name='UMol.API.APIPython.select(string,string,bool,bool,bool,bool,bool,bool,bool).forceCreate'></a>

`forceCreate` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Create the selection even if it's empty

<a name='UMol.API.APIPython.select(string,string,bool,bool,bool,bool,bool,bool,bool).allModels'></a>

`allModels` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

apply the selection to all models of structures concerned?

<a name='UMol.API.APIPython.select(string,string,bool,bool,bool,bool,bool,bool,bool).addToSelectionKeyword'></a>

`addToSelectionKeyword` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

add this selection as a keyword?

#### Returns
[UnityMolSelection](UMol.UnityMolSelection.md 'UMol.UnityMolSelection')
the UnityMolSelection object

<a name='UMol.API.APIPython.selectInRectangle(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3)'></a>

## APIPython.selectInRectangle(Vector3, Vector3, Vector3, Vector3) Method

Select atoms of all loaded molecules inside a parallelepiped defined by a molecular space position and 3 axis
and create a new selection from it.

```csharp
public static UMol.UnityMolSelection selectInRectangle(UnityEngine.Vector3 lowerLeft, UnityEngine.Vector3 xaxis, UnityEngine.Vector3 yaxis, UnityEngine.Vector3 zaxis);
```
#### Parameters

<a name='UMol.API.APIPython.selectInRectangle(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3).lowerLeft'></a>

`lowerLeft` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

position of the lower left point of the parallelepiped

<a name='UMol.API.APIPython.selectInRectangle(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3).xaxis'></a>

`xaxis` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

X Axis of the parallelepiped

<a name='UMol.API.APIPython.selectInRectangle(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3).yaxis'></a>

`yaxis` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

Y Axis of the parallelepiped

<a name='UMol.API.APIPython.selectInRectangle(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3).zaxis'></a>

`zaxis` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

Z Axis of the parallelepiped

#### Returns
[UnityMolSelection](UMol.UnityMolSelection.md 'UMol.UnityMolSelection')
the new UnityMolSelection object

<a name='UMol.API.APIPython.selectInSphere(UnityEngine.Vector3,float)'></a>

## APIPython.selectInSphere(Vector3, float) Method

Select atoms of all loaded molecules inside a sphere defined by a molecular space position and a radius in Angstrom
and create a new selection from it.

```csharp
public static UMol.UnityMolSelection selectInSphere(UnityEngine.Vector3 position, float radius);
```
#### Parameters

<a name='UMol.API.APIPython.selectInSphere(UnityEngine.Vector3,float).position'></a>

`position` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

center of the sphere

<a name='UMol.API.APIPython.selectInSphere(UnityEngine.Vector3,float).radius'></a>

`radius` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

radius of the sphere

#### Returns
[UnityMolSelection](UMol.UnityMolSelection.md 'UMol.UnityMolSelection')
the new UnityMolSelection object

<a name='UMol.API.APIPython.setAmbientLightIntensity(float)'></a>

## APIPython.setAmbientLightIntensity(float) Method

Set the global ambient light intensity

```csharp
public static void setAmbientLightIntensity(float ambientLightValue);
```
#### Parameters

<a name='UMol.API.APIPython.setAmbientLightIntensity(float).ambientLightValue'></a>

`ambientLightValue` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new ambient light intensity

<a name='UMol.API.APIPython.setAsLigand(string,bool)'></a>

## APIPython.setAsLigand(string, bool) Method

Set the atoms of the selection named 'selName' to ligand

```csharp
public static void setAsLigand(string selName, bool updateAllSelections=true);
```
#### Parameters

<a name='UMol.API.APIPython.setAsLigand(string,bool).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The name of the selection concerned

<a name='UMol.API.APIPython.setAsLigand(string,bool).updateAllSelections'></a>

`updateAllSelections` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Update all selections?

<a name='UMol.API.APIPython.setBondOrderTexture(string,int)'></a>

## APIPython.setBondOrderTexture(string, int) Method

Change all bond order representations in the selection with a new texture

```csharp
public static void setBondOrderTexture(string selName, int idTex);
```
#### Parameters

<a name='UMol.API.APIPython.setBondOrderTexture(string,int).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setBondOrderTexture(string,int).idTex'></a>

`idTex` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

Texture Index in UnityMolMain.atomColors.textures

<a name='UMol.API.APIPython.setBoundingBoxLineSize(string,float)'></a>

## APIPython.setBoundingBoxLineSize(string, float) Method

Set the size of the bounding box lines

```csharp
public static void setBoundingBoxLineSize(string structureName, float size=0.005f);
```
#### Parameters

<a name='UMol.API.APIPython.setBoundingBoxLineSize(string,float).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.setBoundingBoxLineSize(string,float).size'></a>

`size` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

size of the lines

<a name='UMol.API.APIPython.setCameraFarPlane(float)'></a>

## APIPython.setCameraFarPlane(float) Method

Set camera far plane
<remarks>This has an impact on shadow map quality</remarks>

```csharp
public static void setCameraFarPlane(float newV);
```
#### Parameters

<a name='UMol.API.APIPython.setCameraFarPlane(float).newV'></a>

`newV` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new value

<a name='UMol.API.APIPython.setCameraNearPlane(float)'></a>

## APIPython.setCameraNearPlane(float) Method

Set camera near plane
<remarks>This has an impact on shadow map quality</remarks>

```csharp
public static void setCameraNearPlane(float newV);
```
#### Parameters

<a name='UMol.API.APIPython.setCameraNearPlane(float).newV'></a>

`newV` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new value

<a name='UMol.API.APIPython.setCameraOrtho()'></a>

## APIPython.setCameraOrtho() Method

Activate the orthographic mode of the camera

```csharp
public static void setCameraOrtho();
```

<a name='UMol.API.APIPython.setCameraOrthoSize(float)'></a>

## APIPython.setCameraOrthoSize(float) Method

Set the size of the orthographic mode of the camera

```csharp
public static void setCameraOrthoSize(float orthoSize);
```
#### Parameters

<a name='UMol.API.APIPython.setCameraOrthoSize(float).orthoSize'></a>

`orthoSize` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new size

<a name='UMol.API.APIPython.setCartoonColorSS(string,string,UnityEngine.Color)'></a>

## APIPython.setCartoonColorSS(string, string, Color) Method

Set the color of the cartoon representation of the specified selection based on the nature of secondary structure assigned

```csharp
public static void setCartoonColorSS(string selName, string ssType, UnityEngine.Color col);
```
#### Parameters

<a name='UMol.API.APIPython.setCartoonColorSS(string,string,UnityEngine.Color).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setCartoonColorSS(string,string,UnityEngine.Color).ssType'></a>

`ssType` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Cartoon representation type: "helix", "sheet" or "coil"

<a name='UMol.API.APIPython.setCartoonColorSS(string,string,UnityEngine.Color).col'></a>

`col` [UnityEngine.Color](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Color 'UnityEngine.Color')

the new color

<a name='UMol.API.APIPython.setCurrentSelection(string)'></a>

## APIPython.setCurrentSelection(string) Method

Set the selection as the current selection in the UnityMolSelectionManager

```csharp
public static void setCurrentSelection(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.setCurrentSelection(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the selection name concerned.

<a name='UMol.API.APIPython.setDepthCueingColor(UnityEngine.Color)'></a>

## APIPython.setDepthCueingColor(Color) Method

Set depth cueing color

```csharp
public static void setDepthCueingColor(UnityEngine.Color col);
```
#### Parameters

<a name='UMol.API.APIPython.setDepthCueingColor(UnityEngine.Color).col'></a>

`col` [UnityEngine.Color](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Color 'UnityEngine.Color')

the new color

<a name='UMol.API.APIPython.setDepthCueingDensity(float)'></a>

## APIPython.setDepthCueingDensity(float) Method

Set depth cueing density

```csharp
public static void setDepthCueingDensity(float v);
```
#### Parameters

<a name='UMol.API.APIPython.setDepthCueingDensity(float).v'></a>

`v` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new density

<a name='UMol.API.APIPython.setDepthCueingFollow(bool)'></a>

## APIPython.setDepthCueingFollow(bool) Method

Enable/Disable depth cueing update when zooming in or out

```csharp
public static void setDepthCueingFollow(bool v);
```
#### Parameters

<a name='UMol.API.APIPython.setDepthCueingFollow(bool).v'></a>

`v` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Enable or disable

<a name='UMol.API.APIPython.setDepthCueingStart(float)'></a>

## APIPython.setDepthCueingStart(float) Method

Set depth cueing starting position in world space

```csharp
public static void setDepthCueingStart(float v);
```
#### Parameters

<a name='UMol.API.APIPython.setDepthCueingStart(float).v'></a>

`v` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the starting position

<a name='UMol.API.APIPython.setDirLightColor(UnityEngine.Color)'></a>

## APIPython.setDirLightColor(Color) Method

Set light color of all directional lights found in the scene

```csharp
public static void setDirLightColor(UnityEngine.Color c);
```
#### Parameters

<a name='UMol.API.APIPython.setDirLightColor(UnityEngine.Color).c'></a>

`c` [UnityEngine.Color](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Color 'UnityEngine.Color')

the new light color

<a name='UMol.API.APIPython.setDirLightDirection(UnityEngine.Vector3)'></a>

## APIPython.setDirLightDirection(Vector3) Method

Set light direction based on eulers for all directional lights found in the scene

```csharp
public static void setDirLightDirection(UnityEngine.Vector3 eulers);
```
#### Parameters

<a name='UMol.API.APIPython.setDirLightDirection(UnityEngine.Vector3).eulers'></a>

`eulers` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

The light direction

<a name='UMol.API.APIPython.setDirLightIntensity(float)'></a>

## APIPython.setDirLightIntensity(float) Method

Set light intensity of all directional lights found in the scene

```csharp
public static void setDirLightIntensity(float lightIntensity);
```
#### Parameters

<a name='UMol.API.APIPython.setDirLightIntensity(float).lightIntensity'></a>

`lightIntensity` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new light intensity

<a name='UMol.API.APIPython.setDirLightShadow(float)'></a>

## APIPython.setDirLightShadow(float) Method

Set light shadow strength of all directional lights found in the scene
0 is no shadow at all, 1 is full black shadow

```csharp
public static void setDirLightShadow(float lightShadow);
```
#### Parameters

<a name='UMol.API.APIPython.setDirLightShadow(float).lightShadow'></a>

`lightShadow` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new light shadow

<a name='UMol.API.APIPython.setDOFAperture(float)'></a>

## APIPython.setDOFAperture(float) Method

Set DOF aperture

```csharp
public static void setDOFAperture(float a);
```
#### Parameters

<a name='UMol.API.APIPython.setDOFAperture(float).a'></a>

`a` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new aperture

<a name='UMol.API.APIPython.setDOFFocalLength(float)'></a>

## APIPython.setDOFFocalLength(float) Method

Set DOF focal length

```csharp
public static void setDOFFocalLength(float f);
```
#### Parameters

<a name='UMol.API.APIPython.setDOFFocalLength(float).f'></a>

`f` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new length

<a name='UMol.API.APIPython.setDOFFocusDistance(float)'></a>

## APIPython.setDOFFocusDistance(float) Method

Set DOF focus distance
this is used by the MouseAutoFocus script

```csharp
public static void setDOFFocusDistance(float v);
```
#### Parameters

<a name='UMol.API.APIPython.setDOFFocusDistance(float).v'></a>

`v` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new distance

<a name='UMol.API.APIPython.setFieldlineGradientThreshold(string,float)'></a>

## APIPython.setFieldlineGradientThreshold(string, float) Method

Change fieldlines computation gradient threshold

```csharp
public static void setFieldlineGradientThreshold(string selName, float val);
```
#### Parameters

<a name='UMol.API.APIPython.setFieldlineGradientThreshold(string,float).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the selection concerned.

<a name='UMol.API.APIPython.setFieldlineGradientThreshold(string,float).val'></a>

`val` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

New threshold value

<a name='UMol.API.APIPython.setHyperballParam(string,UnityEngine.Vector3,UnityEngine.Vector3,float)'></a>

## APIPython.setHyperballParam(string, Vector3, Vector3, float) Method

Change the hyperball parameters of the selection across a certain duration.
Use coroutine.

```csharp
private void setHyperballParam(string selName, UnityEngine.Vector3 prevScaleShrink, UnityEngine.Vector3 scaleShrink, float duration);
```
#### Parameters

<a name='UMol.API.APIPython.setHyperballParam(string,UnityEngine.Vector3,UnityEngine.Vector3,float).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the selection name concerned

<a name='UMol.API.APIPython.setHyperballParam(string,UnityEngine.Vector3,UnityEngine.Vector3,float).prevScaleShrink'></a>

`prevScaleShrink` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the previous parameters

<a name='UMol.API.APIPython.setHyperballParam(string,UnityEngine.Vector3,UnityEngine.Vector3,float).scaleShrink'></a>

`scaleShrink` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the new parameters

<a name='UMol.API.APIPython.setHyperballParam(string,UnityEngine.Vector3,UnityEngine.Vector3,float).duration'></a>

`duration` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

Duration of the change

<a name='UMol.API.APIPython.setHyperBallShininess(string,float)'></a>

## APIPython.setHyperBallShininess(string, float) Method

Set the shininess for the hyperball representation for a given selection

```csharp
public static void setHyperBallShininess(string selName, float shin);
```
#### Parameters

<a name='UMol.API.APIPython.setHyperBallShininess(string,float).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setHyperBallShininess(string,float).shin'></a>

`shin` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new shininess value

<a name='UMol.API.APIPython.setHyperballShrink(string,float)'></a>

## APIPython.setHyperballShrink(string, float) Method

Set the shrink factor for the hyperball representation for a given selection

```csharp
public static void setHyperballShrink(string selName, float shrink);
```
#### Parameters

<a name='UMol.API.APIPython.setHyperballShrink(string,float).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setHyperballShrink(string,float).shrink'></a>

`shrink` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new shrink factor

<a name='UMol.API.APIPython.setHyperballTexture(string,int)'></a>

## APIPython.setHyperballTexture(string, int) Method

Change all hyperball representations in the selection with a new texture

```csharp
public static void setHyperballTexture(string selName, int idTex);
```
#### Parameters

<a name='UMol.API.APIPython.setHyperballTexture(string,int).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setHyperballTexture(string,int).idTex'></a>

`idTex` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

Texture Index in UnityMolMain.atomColors.textures

<a name='UMol.API.APIPython.setLimitedViewCenter(string,string,UnityEngine.Vector3)'></a>

## APIPython.setLimitedViewCenter(string, string, Vector3) Method

Set the center of the limited view

```csharp
public static void setLimitedViewCenter(string selName, string type, UnityEngine.Vector3 center);
```
#### Parameters

<a name='UMol.API.APIPython.setLimitedViewCenter(string,string,UnityEngine.Vector3).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setLimitedViewCenter(string,string,UnityEngine.Vector3).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

<a name='UMol.API.APIPython.setLimitedViewCenter(string,string,UnityEngine.Vector3).center'></a>

`center` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

The new center

<a name='UMol.API.APIPython.setLimitedViewRadius(string,string,float)'></a>

## APIPython.setLimitedViewRadius(string, string, float) Method

Set the radius (in Angstrom) of the limited view
<remarks>Only works with surface or cartoon types for now</remarks>

```csharp
public static void setLimitedViewRadius(string selName, string type, float radius);
```
#### Parameters

<a name='UMol.API.APIPython.setLimitedViewRadius(string,string,float).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setLimitedViewRadius(string,string,float).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

<a name='UMol.API.APIPython.setLimitedViewRadius(string,string,float).radius'></a>

`radius` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new radius in Angstrom

<a name='UMol.API.APIPython.setLineSize(string,float)'></a>

## APIPython.setLineSize(string, float) Method

Set the size of the line representation in a given selection

```csharp
public static void setLineSize(string selName, float val);
```
#### Parameters

<a name='UMol.API.APIPython.setLineSize(string,float).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setLineSize(string,float).val'></a>

`val` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

The new size

<a name='UMol.API.APIPython.setMeasureMode(int)'></a>

## APIPython.setMeasureMode(int) Method

Set the measure mode for creating annotations
Measure modes : 0 = distance, 1 = angle, 2 = torsion angle

```csharp
public static void setMeasureMode(int newMode);
```
#### Parameters

<a name='UMol.API.APIPython.setMeasureMode(int).newMode'></a>

`newMode` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the measure mode

<a name='UMol.API.APIPython.setMetal(string,string,float)'></a>

## APIPython.setMetal(string, string, float) Method

Set the metal value of a given representation of a given selection
type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
Does nothing if the representation has no metal feature.

```csharp
public static void setMetal(string selName, string type, float val);
```
#### Parameters

<a name='UMol.API.APIPython.setMetal(string,string,float).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setMetal(string,string,float).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

<a name='UMol.API.APIPython.setMetal(string,string,float).val'></a>

`val` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new metal value

<a name='UMol.API.APIPython.setModel(string,int)'></a>

## APIPython.setModel(string, int) Method

Set the current model of the structure
This function is used by ModelPlayers.cs to read the models of a structure like a trajectory

```csharp
public static void setModel(string structureName, int modelId);
```
#### Parameters

<a name='UMol.API.APIPython.setModel(string,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.setModel(string,int).modelId'></a>

`modelId` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

ID of the model in the structure. -1 means current model

<a name='UMol.API.APIPython.setMolParentTransform(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,bool,float)'></a>

## APIPython.setMolParentTransform(Vector3, Vector3, Vector3, Vector3, bool, float) Method

Set the position, scale and rotation of the parent of all loaded molecules (LoadedMolecules GameObject)
Linear interpolation between the current state of the camera to the specified values

```csharp
public static void setMolParentTransform(UnityEngine.Vector3 pos, UnityEngine.Vector3 scale, UnityEngine.Vector3 rot, UnityEngine.Vector3 centerOfRotation, bool lerp=true, float duration=1f);
```
#### Parameters

<a name='UMol.API.APIPython.setMolParentTransform(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,bool,float).pos'></a>

`pos` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the new position

<a name='UMol.API.APIPython.setMolParentTransform(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,bool,float).scale'></a>

`scale` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the new scale

<a name='UMol.API.APIPython.setMolParentTransform(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,bool,float).rot'></a>

`rot` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the new rotation

<a name='UMol.API.APIPython.setMolParentTransform(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,bool,float).centerOfRotation'></a>

`centerOfRotation` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the new center of rotation

<a name='UMol.API.APIPython.setMolParentTransform(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,bool,float).lerp'></a>

`lerp` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Use a linear interpolation between the current of the new values?

<a name='UMol.API.APIPython.setMolParentTransform(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,bool,float).duration'></a>

`duration` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

Duration of the linear interpolation

<a name='UMol.API.APIPython.setMouseMoveSpeed(float)'></a>

## APIPython.setMouseMoveSpeed(float) Method

Change the speed of mouse rotations and translations

```csharp
public static void setMouseMoveSpeed(float val);
```
#### Parameters

<a name='UMol.API.APIPython.setMouseMoveSpeed(float).val'></a>

`val` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new speed

<a name='UMol.API.APIPython.setMouseScrollSpeed(float)'></a>

## APIPython.setMouseScrollSpeed(float) Method

Change the mouse scroll speed

```csharp
public static void setMouseScrollSpeed(float val);
```
#### Parameters

<a name='UMol.API.APIPython.setMouseScrollSpeed(float).val'></a>

`val` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new speed

<a name='UMol.API.APIPython.setOutlineColor(UnityEngine.Color)'></a>

## APIPython.setOutlineColor(Color) Method

Set the color of the outline effect

```csharp
public static void setOutlineColor(UnityEngine.Color col);
```
#### Parameters

<a name='UMol.API.APIPython.setOutlineColor(UnityEngine.Color).col'></a>

`col` [UnityEngine.Color](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Color 'UnityEngine.Color')

the new color

<a name='UMol.API.APIPython.setOutlineThickness(float)'></a>

## APIPython.setOutlineThickness(float) Method

Set the thickness of the outline effect

```csharp
public static void setOutlineThickness(float v);
```
#### Parameters

<a name='UMol.API.APIPython.setOutlineThickness(float).v'></a>

`v` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new thickness

<a name='UMol.API.APIPython.setPosScaleRot(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,bool,float)'></a>

## APIPython.setPosScaleRot(Vector3, Vector3, Vector3, Vector3, bool, float) Method

Set the position, scale and rotation of the parent of all loaded molecules (LoadedMolecules GameObject)
Linear interpolation between the current state of the camera to the specified values

```csharp
private void setPosScaleRot(UnityEngine.Vector3 pos, UnityEngine.Vector3 scale, UnityEngine.Vector3 rot, UnityEngine.Vector3 centerOfRotation, bool lerp, float duration);
```
#### Parameters

<a name='UMol.API.APIPython.setPosScaleRot(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,bool,float).pos'></a>

`pos` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the new position

<a name='UMol.API.APIPython.setPosScaleRot(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,bool,float).scale'></a>

`scale` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the new scale

<a name='UMol.API.APIPython.setPosScaleRot(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,bool,float).rot'></a>

`rot` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the new rotation

<a name='UMol.API.APIPython.setPosScaleRot(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,bool,float).centerOfRotation'></a>

`centerOfRotation` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the new center of rotation

<a name='UMol.API.APIPython.setPosScaleRot(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,bool,float).lerp'></a>

`lerp` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Use a linear interpolation between the current of the new values?

<a name='UMol.API.APIPython.setPosScaleRot(UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,UnityEngine.Vector3,bool,float).duration'></a>

`duration` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

Duration of the linear interpolation

<a name='UMol.API.APIPython.setRepSize(string,string,float)'></a>

## APIPython.setRepSize(string, string, float) Method

Change the size of the representation of type 'type' in the selection
<remarks>Mainly used for hyperball representation</remarks>
type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"

```csharp
public static void setRepSize(string selName, string type, float size);
```
#### Parameters

<a name='UMol.API.APIPython.setRepSize(string,string,float).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setRepSize(string,string,float).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

<a name='UMol.API.APIPython.setRepSize(string,string,float).size'></a>

`size` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new size of the representation

<a name='UMol.API.APIPython.setRepSizes(string,string,System.Collections.Generic.List_float_)'></a>

## APIPython.setRepSizes(string, string, List<float>) Method

Change the size of the representation of type 'type' in the selection for each atom.
The parameters sizes is a list new values for each atom of the selection.
<remarks>Mainly used for hyperball representation</remarks>
type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"

```csharp
public static void setRepSizes(string selName, string type, System.Collections.Generic.List<float> sizes);
```
#### Parameters

<a name='UMol.API.APIPython.setRepSizes(string,string,System.Collections.Generic.List_float_).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setRepSizes(string,string,System.Collections.Generic.List_float_).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

<a name='UMol.API.APIPython.setRepSizes(string,string,System.Collections.Generic.List_float_).sizes'></a>

`sizes` [System.Collections.Generic.List&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')[System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1 'System.Collections.Generic.List`1')

the new size for each atom of the representation

<a name='UMol.API.APIPython.setRTMaterial(string,string,string)'></a>

## APIPython.setRTMaterial(string, string, string) Method

Set a Raytracing material for a representation of a given selection.
Name of the material is taken from the RT material bank

```csharp
public static void setRTMaterial(string selName, string type, string matName);
```
#### Parameters

<a name='UMol.API.APIPython.setRTMaterial(string,string,string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setRTMaterial(string,string,string).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of representation

<a name='UMol.API.APIPython.setRTMaterial(string,string,string).matName'></a>

`matName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the name of the material

<a name='UMol.API.APIPython.setRTMaterialProperty(string,string,string,object)'></a>

## APIPython.setRTMaterialProperty(string, string, string, object) Method

Set Raytracing material property for a given representation of a given selection

```csharp
public static void setRTMaterialProperty(string selName, string type, string propName, object val);
```
#### Parameters

<a name='UMol.API.APIPython.setRTMaterialProperty(string,string,string,object).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setRTMaterialProperty(string,string,string,object).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of representation

<a name='UMol.API.APIPython.setRTMaterialProperty(string,string,string,object).propName'></a>

`propName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the name of the property

<a name='UMol.API.APIPython.setRTMaterialProperty(string,string,string,object).val'></a>

`val` [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object')

the value of the property

<a name='UMol.API.APIPython.setRTMaterialType(string,string,int)'></a>

## APIPython.setRTMaterialType(string, string, int) Method

Set Raytracing material type for a given representation of a given selection
Possible values of 'matType' are : 0 = Principled / 1 = carPaint / 2 = metal / 3 = alloy /
4 = glass / 5 = thinGlass / 6 = metallicPaint / 7 = luminous

```csharp
public static void setRTMaterialType(string selName, string type, int matType);
```
#### Parameters

<a name='UMol.API.APIPython.setRTMaterialType(string,string,int).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setRTMaterialType(string,string,int).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of representation

<a name='UMol.API.APIPython.setRTMaterialType(string,string,int).matType'></a>

`matType` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

the type of material

<a name='UMol.API.APIPython.setShadows(string,string,bool)'></a>

## APIPython.setShadows(string, string, bool) Method

Show or hide representation shadows for a given selection and a given representation.

```csharp
public static void setShadows(string selName, string type, bool enable);
```
#### Parameters

<a name='UMol.API.APIPython.setShadows(string,string,bool).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setShadows(string,string,bool).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

<a name='UMol.API.APIPython.setShadows(string,string,bool).enable'></a>

`enable` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

if True, show the shadows. Hide if false

<a name='UMol.API.APIPython.setSheherasadeTexture(string,int)'></a>

## APIPython.setSheherasadeTexture(string, int) Method

Change all sheherasade representations in the selection with a new texture

```csharp
public static void setSheherasadeTexture(string selName, int idTex);
```
#### Parameters

<a name='UMol.API.APIPython.setSheherasadeTexture(string,int).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setSheherasadeTexture(string,int).idTex'></a>

`idTex` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

Texture Index in UnityMolMain.atomColors.textures

<a name='UMol.API.APIPython.setSmoothness(string,string,float)'></a>

## APIPython.setSmoothness(string, string, float) Method

Set the smoothness of a given representation of a given selection
type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"

```csharp
public static void setSmoothness(string selName, string type, float val);
```
#### Parameters

<a name='UMol.API.APIPython.setSmoothness(string,string,float).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setSmoothness(string,string,float).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

<a name='UMol.API.APIPython.setSmoothness(string,string,float).val'></a>

`val` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new smoothness value

<a name='UMol.API.APIPython.setSolidCartoon(string)'></a>

## APIPython.setSolidCartoon(string) Method

Switch cartoon material from transparent to normal/solid for a selection.

```csharp
public static void setSolidCartoon(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.setSolidCartoon(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setSolidSphere(string)'></a>

## APIPython.setSolidSphere(string) Method

Switch sphere material from transparent to normal/solid for a given selection

```csharp
public static void setSolidSphere(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.setSolidSphere(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setSolidSurface(string)'></a>

## APIPython.setSolidSurface(string) Method

Switch all surface representations in selection to a solid surface material

```csharp
public static void setSolidSurface(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.setSolidSurface(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setStructureGroup(string,int)'></a>

## APIPython.setStructureGroup(string, int) Method

Utility function to set the group of a structure
This group is used to be able to move all the loaded molecules in the same group
Groups can be between 0 and 9 included

```csharp
public static void setStructureGroup(string structureName, int newGroup);
```
#### Parameters

<a name='UMol.API.APIPython.setStructureGroup(string,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.setStructureGroup(string,int).newGroup'></a>

`newGroup` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

ID of the group

<a name='UMol.API.APIPython.setStructurePositionRotation(string,UnityEngine.Vector3,UnityEngine.Vector3)'></a>

## APIPython.setStructurePositionRotation(string, Vector3, Vector3) Method

Set the local position and rotation (euler angles) of the given structure

```csharp
public static void setStructurePositionRotation(string structureName, UnityEngine.Vector3 pos, UnityEngine.Vector3 rot);
```
#### Parameters

<a name='UMol.API.APIPython.setStructurePositionRotation(string,UnityEngine.Vector3,UnityEngine.Vector3).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the structure name concerned

<a name='UMol.API.APIPython.setStructurePositionRotation(string,UnityEngine.Vector3,UnityEngine.Vector3).pos'></a>

`pos` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the new position

<a name='UMol.API.APIPython.setStructurePositionRotation(string,UnityEngine.Vector3,UnityEngine.Vector3).rot'></a>

`rot` [UnityEngine.Vector3](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Vector3 'UnityEngine.Vector3')

the new rotation as euler angles.

<a name='UMol.API.APIPython.setSurfaceWireframe(string,string,float)'></a>

## APIPython.setSurfaceWireframe(string, string, float) Method

Set the surface wireframe size

```csharp
public static void setSurfaceWireframe(string selName, string type, float val);
```
#### Parameters

<a name='UMol.API.APIPython.setSurfaceWireframe(string,string,float).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setSurfaceWireframe(string,string,float).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

<a name='UMol.API.APIPython.setSurfaceWireframe(string,string,float).val'></a>

`val` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

the new wireframe size

<a name='UMol.API.APIPython.setTraceSize(string,float)'></a>

## APIPython.setTraceSize(string, float) Method

Set the size of the trace representation in a given selection

```csharp
public static void setTraceSize(string selName, float val);
```
#### Parameters

<a name='UMol.API.APIPython.setTraceSize(string,float).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setTraceSize(string,float).val'></a>

`val` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

The new size

<a name='UMol.API.APIPython.setTrajFrame(string,int)'></a>

## APIPython.setTrajFrame(string, int) Method

Set the current trajectory frame of the structure to a specific frame.
 frame has to be between 0 and the total number of frames.

```csharp
public static void setTrajFrame(string structureName, int frame);
```
#### Parameters

<a name='UMol.API.APIPython.setTrajFrame(string,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.setTrajFrame(string,int).frame'></a>

`frame` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

ID of the frame

<a name='UMol.API.APIPython.setTransparentCartoon(string,float)'></a>

## APIPython.setTransparentCartoon(string, float) Method

Set the cartoon material to transparent for a selection

```csharp
public static void setTransparentCartoon(string selName, float alpha=0.3f);
```
#### Parameters

<a name='UMol.API.APIPython.setTransparentCartoon(string,float).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setTransparentCartoon(string,float).alpha'></a>

`alpha` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

Value of the transparency

<a name='UMol.API.APIPython.setTransparentSphere(string,float)'></a>

## APIPython.setTransparentSphere(string, float) Method

Set the sphere material to transparent for a selection

```csharp
public static void setTransparentSphere(string selName, float alpha=0.3f);
```
#### Parameters

<a name='UMol.API.APIPython.setTransparentSphere(string,float).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setTransparentSphere(string,float).alpha'></a>

`alpha` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

Value of the transparency

<a name='UMol.API.APIPython.setTransparentSurface(string,System.Nullable_float_)'></a>

## APIPython.setTransparentSurface(string, Nullable<float>) Method

Switch all surface representations in selection to a transparent surface material

```csharp
public static void setTransparentSurface(string selName, System.Nullable<float> alpha=null);
```
#### Parameters

<a name='UMol.API.APIPython.setTransparentSurface(string,System.Nullable_float_).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setTransparentSurface(string,System.Nullable_float_).alpha'></a>

`alpha` [System.Nullable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')[System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')

Value of the transparency.

<a name='UMol.API.APIPython.setTubeSizeCartoon(string,float)'></a>

## APIPython.setTubeSizeCartoon(string, float) Method

Recompute cartoon representation with new tube size

```csharp
public static void setTubeSizeCartoon(string selName, float newVal);
```
#### Parameters

<a name='UMol.API.APIPython.setTubeSizeCartoon(string,float).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setTubeSizeCartoon(string,float).newVal'></a>

`newVal` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

Tube size

<a name='UMol.API.APIPython.setUpdateSelectionTraj(string,bool)'></a>

## APIPython.setUpdateSelectionTraj(string, bool) Method

Activate or deactivate updating the content of the selection 'selName' during a trajectory

```csharp
public static void setUpdateSelectionTraj(string selName, bool v);
```
#### Parameters

<a name='UMol.API.APIPython.setUpdateSelectionTraj(string,bool).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.setUpdateSelectionTraj(string,bool).v'></a>

`v` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

If True, activate the update. If false, deactivate the update

<a name='UMol.API.APIPython.setWireframeSurface(string)'></a>

## APIPython.setWireframeSurface(string) Method

Switch all surface representations in selection to a wireframe surface material when available

```csharp
public static void setWireframeSurface(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.setWireframeSurface(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.show(string)'></a>

## APIPython.show(string) Method

Show the representation type 'type' for all loaded molecules.
Type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"

```csharp
public static void show(string type);
```
#### Parameters

<a name='UMol.API.APIPython.show(string).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Representation type

<a name='UMol.API.APIPython.showAs(string)'></a>

## APIPython.showAs(string) Method

Show *only* the representation type 'type' for all loaded molecules.
type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"

```csharp
public static void showAs(string type);
```
#### Parameters

<a name='UMol.API.APIPython.showAs(string).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Representation type

<a name='UMol.API.APIPython.showBoundingBox(string)'></a>

## APIPython.showBoundingBox(string) Method

Show bounding box lines around the structure
<remarks>This box is based on the max/min coordinates of the atoms. It is not the CRYSTAL box or the simulation box</remarks>

```csharp
public static void showBoundingBox(string structureName);
```
#### Parameters

<a name='UMol.API.APIPython.showBoundingBox(string).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.showDefault(string)'></a>

## APIPython.showDefault(string) Method

Create selections and default representations: all in cartoon, not protein in hyperballs
Also create a selection containing "not protein and not water and not ligand and not ions"

```csharp
public static void showDefault(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.showDefault(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The default selection of the whole structure

<a name='UMol.API.APIPython.showDXLines(string)'></a>

## APIPython.showDXLines(string) Method

Show lines around the Density (DX) map

```csharp
public static void showDXLines(string structureName);
```
#### Parameters

<a name='UMol.API.APIPython.showDXLines(string).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.showHideBackboneInSelection(string)'></a>

## APIPython.showHideBackboneInSelection(string) Method

Show/Hide backbone in representations of the current selection
This only works for lines, hyperballs and sphere representations

```csharp
public static void showHideBackboneInSelection(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.showHideBackboneInSelection(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the selection concerned.

<a name='UMol.API.APIPython.showHideConsole(bool)'></a>

## APIPython.showHideConsole(bool) Method

Show/Hide UnityMol console

```csharp
public static void showHideConsole(bool show);
```
#### Parameters

<a name='UMol.API.APIPython.showHideConsole(bool).show'></a>

`show` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

If True, show the console

<a name='UMol.API.APIPython.showHideHydrogensInSelection(string)'></a>

## APIPython.showHideHydrogensInSelection(string) Method

Show/Hide hydrogens in representations of the provided selection
This only works for lines, hyperballs and sphere representations

```csharp
public static void showHideHydrogensInSelection(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.showHideHydrogensInSelection(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the selection concerned.

<a name='UMol.API.APIPython.showHideSideChainsInSelection(string)'></a>

## APIPython.showHideSideChainsInSelection(string) Method

Show/Hide side chains in representations of the current selection
This only works for lines, hyperballs and sphere representations

```csharp
public static void showHideSideChainsInSelection(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.showHideSideChainsInSelection(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the selection concerned.

<a name='UMol.API.APIPython.showSelection(string,string,object[])'></a>

## APIPython.showSelection(string, string, object[]) Method

Show the selection as 'type'
type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
If the representation is already there, update it if the selection content changed and show it
Surface example: showSelection("all_1kx2", "s", True, True, True, SurfMethod.MSMS) # arguments are cutByChain, AO, cutSurface, computeSurfaceMethod
Iso-surface example: showSelection("all_1kx2", "dxiso", last().dxr, 0.0f)

```csharp
public static void showSelection(string selName, string type, params object[] args);
```
#### Parameters

<a name='UMol.API.APIPython.showSelection(string,string,object[]).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name to show

<a name='UMol.API.APIPython.showSelection(string,string,object[]).type'></a>

`type` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The type of the representation

<a name='UMol.API.APIPython.showSelection(string,string,object[]).args'></a>

`args` [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object')[[]](https://docs.microsoft.com/en-us/dotnet/api/System.Array 'System.Array')

The options for the representation chosen.

<a name='UMol.API.APIPython.showSelection(string)'></a>

## APIPython.showSelection(string) Method

Show all representations of the selection named 'selName'

```csharp
public static void showSelection(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.showSelection(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned.

<a name='UMol.API.APIPython.showStructureAllRepresentations(string)'></a>

## APIPython.showStructureAllRepresentations(string) Method

Show all representations already created for a specified structure

```csharp
public static void showStructureAllRepresentations(string structureName);
```
#### Parameters

<a name='UMol.API.APIPython.showStructureAllRepresentations(string).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.startVideo(string,int,int,int,bool)'></a>

## APIPython.startVideo(string, int, int, int, bool) Method

Start to record a video with FFMPEG at a specific resolution and framerate

```csharp
public static void startVideo(string filePath, int resolutionWidth=1280, int resolutionHeight=720, int frameRate=30, bool pauseAtStart=false);
```
#### Parameters

<a name='UMol.API.APIPython.startVideo(string,int,int,int,bool).filePath'></a>

`filePath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Path to save the video

<a name='UMol.API.APIPython.startVideo(string,int,int,int,bool).resolutionWidth'></a>

`resolutionWidth` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

Width of the video

<a name='UMol.API.APIPython.startVideo(string,int,int,int,bool).resolutionHeight'></a>

`resolutionHeight` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

Height of the video

<a name='UMol.API.APIPython.startVideo(string,int,int,int,bool).frameRate'></a>

`frameRate` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

framerate of the video

<a name='UMol.API.APIPython.startVideo(string,int,int,int,bool).pauseAtStart'></a>

`pauseAtStart` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

<a name='UMol.API.APIPython.stopRotations()'></a>

## APIPython.stopRotations() Method

Stop rotation around all axis

```csharp
public static void stopRotations();
```

<a name='UMol.API.APIPython.stopVideo()'></a>

## APIPython.stopVideo() Method

Stop recording

```csharp
public static void stopVideo();
```

<a name='UMol.API.APIPython.strToColor(string)'></a>

## APIPython.strToColor(string) Method

Convert a color string to a standard Unity Color
Values can be "black", "white", "yellow" ,"green", "red", "blue", "pink", "gray"

```csharp
private static UnityEngine.Color strToColor(string input);
```
#### Parameters

<a name='UMol.API.APIPython.strToColor(string).input'></a>

`input` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the color as a string

#### Returns
[UnityEngine.Color](https://docs.microsoft.com/en-us/dotnet/api/UnityEngine.Color 'UnityEngine.Color')
the color as Unity Color

<a name='UMol.API.APIPython.switchCutSurface(string,bool)'></a>

## APIPython.switchCutSurface(string, bool) Method

Switch between cut surface mode and no-cut surface mode
for a given selection.

```csharp
public static void switchCutSurface(string selName, bool isCut);
```
#### Parameters

<a name='UMol.API.APIPython.switchCutSurface(string,bool).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.switchCutSurface(string,bool).isCut'></a>

`isCut` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Active cut surface mode?

<a name='UMol.API.APIPython.switchDockingMode()'></a>

## APIPython.switchDockingMode() Method

Turn docking mode on and off

```csharp
public static void switchDockingMode();
```

<a name='UMol.API.APIPython.switchRotateAxisX()'></a>

## APIPython.switchRotateAxisX() Method

Switch on or off the rotation around the X axis of all loaded molecules

```csharp
public static void switchRotateAxisX();
```

<a name='UMol.API.APIPython.switchRotateAxisY()'></a>

## APIPython.switchRotateAxisY() Method

Switch on or off the rotation around the Y axis of all loaded molecules

```csharp
public static void switchRotateAxisY();
```

<a name='UMol.API.APIPython.switchRotateAxisZ()'></a>

## APIPython.switchRotateAxisZ() Method

Switch on or off the rotation around the Z axis of all loaded molecules

```csharp
public static void switchRotateAxisZ();
```

<a name='UMol.API.APIPython.switchRTDenoiser(bool)'></a>

## APIPython.switchRTDenoiser(bool) Method

Enable or disable the RayTracing denoiser

```csharp
public static void switchRTDenoiser(bool turnOn);
```
#### Parameters

<a name='UMol.API.APIPython.switchRTDenoiser(bool).turnOn'></a>

`turnOn` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

enable if true, disable if false

<a name='UMol.API.APIPython.switchSheherasadeMethod(string)'></a>

## APIPython.switchSheherasadeMethod(string) Method

Change sheherasade computation method in a given selection

```csharp
public static void switchSheherasadeMethod(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.switchSheherasadeMethod(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.switchSSAssignmentMethod(string,bool)'></a>

## APIPython.switchSSAssignmentMethod(string, bool) Method

Switch between secondary structure information parsed from the file
and the ones from DSSP computation.

```csharp
public static void switchSSAssignmentMethod(string structureName, bool forceDSSP=false);
```
#### Parameters

<a name='UMol.API.APIPython.switchSSAssignmentMethod(string,bool).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.switchSSAssignmentMethod(string,bool).forceDSSP'></a>

`forceDSSP` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

Compute Secondary structure info from DSSP?

<a name='UMol.API.APIPython.switchSurfaceComputeMethod(string)'></a>

## APIPython.switchSurfaceComputeMethod(string) Method

Switch between the 2 types of surface computation methods: EDTSurf and MSMS
for a given selection.

```csharp
public static void switchSurfaceComputeMethod(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.switchSurfaceComputeMethod(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.undo()'></a>

## APIPython.undo() Method

Play the opposite function of the lastly called APIPython function recorded in UnityMolMain.pythonUndoCommands

```csharp
public static void undo();
```

<a name='UMol.API.APIPython.unloadCustomBonds(string,int)'></a>

## APIPython.unloadCustomBonds(string, int) Method

Removes the covBondOrders bonds loaded by loadBondsXML from the model 'modelId' of the structure 'structureName'

```csharp
public static void unloadCustomBonds(string structureName, int modelId);
```
#### Parameters

<a name='UMol.API.APIPython.unloadCustomBonds(string,int).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.unloadCustomBonds(string,int).modelId'></a>

`modelId` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

ID of the model in the structure. -1 means current model

<a name='UMol.API.APIPython.unloadDXmap(string)'></a>

## APIPython.unloadDXmap(string) Method

Unload the density map of the structure

```csharp
public static void unloadDXmap(string structureName);
```
#### Parameters

<a name='UMol.API.APIPython.unloadDXmap(string).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.unloadJSONFieldlines(string)'></a>

## APIPython.unloadJSONFieldlines(string) Method

Remove the json file for fieldlines stored in the currentModel of the specified structure

```csharp
public static void unloadJSONFieldlines(string structureName);
```
#### Parameters

<a name='UMol.API.APIPython.unloadJSONFieldlines(string).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.unloadTraj(string)'></a>

## APIPython.unloadTraj(string) Method

Unload a trajectory for a specific structure

```csharp
public static void unloadTraj(string structureName);
```
#### Parameters

<a name='UMol.API.APIPython.unloadTraj(string).structureName'></a>

`structureName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

Name of the structure concerned

<a name='UMol.API.APIPython.unpauseVideo()'></a>

## APIPython.unpauseVideo() Method

Unpause recording

```csharp
public static void unpauseVideo();
```

<a name='UMol.API.APIPython.updateDXIso(string,float)'></a>

## APIPython.updateDXIso(string, float) Method

Recompute the DX surface with a new iso value for a selection.

```csharp
public static void updateDXIso(string selName, float newVal);
```
#### Parameters

<a name='UMol.API.APIPython.updateDXIso(string,float).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.updateDXIso(string,float).newVal'></a>

`newVal` [System.Single](https://docs.microsoft.com/en-us/dotnet/api/System.Single 'System.Single')

New iso value

<a name='UMol.API.APIPython.updateRepresentations(string)'></a>

## APIPython.updateRepresentations(string) Method

Update all representations of the specified selection, called automatically after a selection content change

```csharp
public static void updateRepresentations(string selName);
```
#### Parameters

<a name='UMol.API.APIPython.updateRepresentations(string).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.updateSelectionWithMDA(string,string,bool,bool,bool,bool)'></a>

## APIPython.updateSelectionWithMDA(string, string, bool, bool, bool, bool) Method

Update the atoms of the selection based on a new MDAnalysis language selection
The selection only applies to the structures of the selection

```csharp
public static bool updateSelectionWithMDA(string selName, string selectionString, bool forceAlteration, bool silent=false, bool recordCommand=true, bool allModels=false);
```
#### Parameters

<a name='UMol.API.APIPython.updateSelectionWithMDA(string,string,bool,bool,bool,bool).selName'></a>

`selName` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

The selection name concerned

<a name='UMol.API.APIPython.updateSelectionWithMDA(string,string,bool,bool,bool,bool).selectionString'></a>

`selectionString` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

the selection query

<a name='UMol.API.APIPython.updateSelectionWithMDA(string,string,bool,bool,bool,bool).forceAlteration'></a>

`forceAlteration` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

make the selection alterable

<a name='UMol.API.APIPython.updateSelectionWithMDA(string,string,bool,bool,bool,bool).silent'></a>

`silent` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

don't print log information.

<a name='UMol.API.APIPython.updateSelectionWithMDA(string,string,bool,bool,bool,bool).recordCommand'></a>

`recordCommand` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

record this command to the history?

<a name='UMol.API.APIPython.updateSelectionWithMDA(string,string,bool,bool,bool,bool).allModels'></a>

`allModels` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

apply the modification to all models of the selection?

#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')
True if success. False otherwise

<a name='UMol.API.APIPython.writeSessionToFile(string)'></a>

## APIPython.writeSessionToFile(string) Method

Write a serialized session file to a JSON file

```csharp
public static void writeSessionToFile(string filePath);
```
#### Parameters

<a name='UMol.API.APIPython.writeSessionToFile(string).filePath'></a>

`filePath` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')

path to write the JSON file
