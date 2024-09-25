
# Plot

An immediate mode (IM) procedural 2D drawing package for [Unity](https://unity.com). Usefull for smaller data visualisations, visual code sketches, and learning how to code. Plot is inspired by [Processing/p5](https://processing.org), so if you're familiar with that, you should be able to jump right in.

Tested with Unity 2023.3 and supporting BiRP, URP, and HDRP.

![Using Static](https://github.com/user-attachments/assets/605211a4-2810-49bf-a33c-02b264ba4915)


## Installation

Install Plot via Unity's Package Manager. Select "Install package from git URL..." and paste in:

	https://github.com/cecarlsen/dk.cec.plot.git

If it doesn't work, check your error message and consult [this page](https://docs.unity3d.com/6000.0/Documentation/Manual/upm-ui-giturl.html).

### Dependencies

Plot depends on TextMeshPro for text rendering and Unity.UI for the samples scenes. Both should be installed automatically when you drop the package in your project.


## Gettings started

Please go through the scenes located in *dk.cec.plot/Samples* in chronological order.


## Known issues

	- SetFillTextureUVRect scaling textures from outer stroke edge instead of shape edge.
	- Texts can vanish temporarily on code reload using ExecuteInEditMode.
	- Image fill changes size inside DrawRect when stroke is enabled and disabled.
	- Shrinking without flicker needs a review. See comment in Ring Shader.
	- SetFillTexture() not implemented for Polygon. 
	- Text ignores SetBlend().
	- Polygons will flicker when very small and moving (won't fix).
	- Text ignores SetAntiAliasing(). It is always on (won't fix).
	- Polygon, Polyline, and Line ignores SetPivot() (by design).


## Implementation

Plot shapes are generated by Signed Distance Fields (SDF) in the fragment shader, which results in infinite resolution, low CPU usage, and practically free anti-aliasing. Each topologically different shape has its own mesh, which is manipulated in the vertex shader to minimize overdraw. When shapes are drawn using any of the DrawX() methods, the meshes are submitted for rendering via Unity’s Graphics.DrawMesh(), meaning they won’t receive lighting but will still be sorted and rendered into the 3D scene.

When multiple instances of the same type of mesh are drawn in succession, Unity will automatically attempt to render them as "instanced," meaning multiple shapes are drawn in a single draw call, with properties stored in a shared constant buffer. Plot also provides DrawXNow() methods, which uses Unity's Graphics.DrawNow(). This is useful for drawing directly and immediately to RenderTextures, when placing code between BeginDrawNowToRenderTexture() and EndDrawNowToRenderTexture(). These textures can then be applied to shapes using SetFillTexture() and drawn into the scene with the DrawX() methods.


## Author

Plot is written and maintained by [Carl Emil Carlsen](https://cec.dk) since 2020.


## License

Please read the [LICENSE.md](https://github.com/cecarlsen/dk.cec.plot/blob/main/LICENSE.md) file.


## Screenshots

Shape functions: Rect, Circle, Ring, Arc, Polygon, Polyline, Line.
![Components](https://github.com/user-attachments/assets/e2534cac-e30a-460e-97af-e8c8ee2c213d)

Shrinking without flicker (except for the polygon).
![Shrink Without Flicker](https://github.com/user-attachments/assets/43f7dc8a-fe3b-4956-b32f-c8b3affb0f44)

Perceptual Colors.
![Perceptually Uniform Colors](https://github.com/user-attachments/assets/abd7df86-be0d-4f75-81d3-3f3ea52aa27a)

Examples of donut chart style variations.
![Donut Charts](https://github.com/user-attachments/assets/81435328-fab2-4d63-9d56-42d483210e4f)

Recursive Tree.
![Shink Without Flicker](https://github.com/user-attachments/assets/bfadbb8a-2d61-4d7d-a3e8-f9bbd405fd6b)
