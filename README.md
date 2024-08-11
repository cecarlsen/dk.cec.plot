
# Plot

An immediate mode (IM) procedural 2D drawing package for Unity. It's usefull for smaller data visualisations, visual code sketches, and for learning how to code.

Tested with Unity 2023.3 and supporting BiRP, URP, and HDRP.

![Using Static](https://raw.githubusercontent.com/cecarlsen/Plot_DEV/master/ReadmeImages/Plot_01API_01UsingStatic.png?token=GHSAT0AAAAAACVKYLKP5QYPSFLWPEMB63G2ZVY2ANA)

## Installment

Plot is contained in a custom Unity package. Download the whole reposity and place the folder inside */Packages* in your Unity project. Then have a look at the scenes inside */Packages/Plot/Samples*.

### Package dependencies

Plot depends on TextMeshPro, which should be installed automatically when you drop the package in your project.

### Included dependencies

Plot contains adaped and stripped down versions of these additional sources:  

- [Earcut.net](https://github.com/oberbichler/earcut.net) by Thomas Oberbichler, for triangulation (ISC license).
- [Chroma.js](https://github.com/gka/chroma.js) by Gregor Aisch, for JCh color conversion (BSD license).


## Gettings started

Please go through the scenes located in *dk.cec.plot/Samples* in chronological order.

API docs are in the making!

## Author

Plot is written by [Carl Emil Carlsen](https://cec.dk).


## License

Please read the LICENSE.md file.

## Screenshots

Shape functions: Rect, Circle, Ring, Arc, Polygon, Polyline, Line.
![Components](https://raw.githubusercontent.com/cecarlsen/Plot_DEV/master/ReadmeImages/Plot_01API_02PlotComponents.png?token=GHSAT0AAAAAACVKYLKPA4SY6AVSA3XE5VOUZVY2CUA)

Shinking shapes without flicker.
![Shink Without Flicker](https://raw.githubusercontent.com/cecarlsen/Plot_DEV/master/ReadmeImages/Plot_01API_07%20ShinkWithoutFlicker.png?token=GHSAT0AAAAAACVKYLKPCFMYZ4K5QQESXFKWZVY2E4Q)

Perceptual Colors.
![Shink Without Flicker](https://raw.githubusercontent.com/cecarlsen/Plot_DEV/master/ReadmeImages/Plot_01API_08PerceptualColors_02PerceptuallyUniformColors.png?token=GHSAT0AAAAAACVKYLKPSMO22KBBKGTNFLR4ZVY2GKQ)

Examples of donut chart variations.
![Shink Without Flicker](https://github.com/cecarlsen/Plot_DEV/blob/master/ReadmeImages/Plot_02Vis_02DonutChart.png)

https://github.com/cecarlsen/Plot_DEV/blob/master/ReadmeImages/Plot_02Vis_02DonutChart.png