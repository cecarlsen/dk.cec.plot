
# Plot

An immediate mode (IM) procedural 2D drawing package for Unity. Usefull for smaller data visualisations, visual code sketches, and for learning how to code. Plot is inspired by [Processing](https://processing.org), so if you are familiar with that, you should be able to jump right in.

Tested with Unity 2023.3 and supporting BiRP, URP, and HDRP.

![Using Static](https://github.com/user-attachments/assets/605211a4-2810-49bf-a33c-02b264ba4915)


## Installment

This repository is structured as a custom Unity package, so just download the entire thing and place it inside */Packages* in your Unity project.

### Package dependencies

Plot depends on TextMeshPro, which should be installed automatically when you drop the package in your project.

### Included dependencies

Plot contains adapted and stripped down versions of these additional sources:  

- [Earcut.net](https://github.com/oberbichler/earcut.net) by Thomas Oberbichler, for triangulation (ISC license).
- [Chroma.js](https://github.com/gka/chroma.js) by Gregor Aisch, for JCh color conversion (BSD license).


## Gettings started

Please go through the scenes located in *dk.cec.plot/Samples* in chronological order.

API docs are in the making!


## Known issues

- The combination of StrokeCornerProfile.Round and StrokeAlignment.Inside or Edge does not work for Rects and Polygons.
- Shrinking without flicker needs a review. See comment in Ring Shader.
- SetFillTexture not implemented for Polygon. 
- Polygons will flicker when very small and moving (won't fix).


## Author

Plot is written by [Carl Emil Carlsen](https://cec.dk).


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
