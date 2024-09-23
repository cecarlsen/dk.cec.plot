# Changelog


## [0.1.0] - 2024-09-XX

	- Added dependency to Unity.UI. Used in sample scenes.
	- Fixed stroke color being forgotten when stroke width is set to zero.
	- Fixed pivot aligning to outside instead of edge.
	- Added PushCanvasAndStyle() and PopCanvasAndStyle().
	- Added NoiseTrails example.
	- Added multi submission per update support for Text.
	- Moved Text.font to Plot.SetTextFont().
	- Added GameOfLife example.
	- Added Image and Text examples.
	- Updated how polylines, polygons, and text is created and drawn.


## [0.0.14b] - 2024-09-XX

	- Removed an example.


## [0.0.14] - 2024-09-20

	- Refactored Plot partial scripts.
	- Added scripting reference.
	- Changed name of SetNoStroke and SetNoFill to SetNoStrokeColor and SetNoFillColor.
	- Added DestroyImmediateOrRuntime.


## [0.0.13] - 2024-08-11

	- Updated to Unity 2022.3.
	- Moved project to custom package.


## [0.0.12] - 2022-08-01

	- Updated to Unity 2022.1.
	- Fixed Polygon not rendering stroke.


## [0.0.11] - 2021-11-18

	- Split the Plot class into partial classes and separated out TextMeshPro.


## [0.0.10] 2021-09-19

	- Added DrawToRenderTexture example.
	- Added J. Tarbells Substrate as example.
	- Added warning when using Draw() method after calling BeginDrawNowToTexture().
	- Added missing TMP define checks in arc diagram example.
	- Fixed edge color bug when no stroke.
	- Arc angles now follows trigonometry (right is zero, increasing anti-clockwise) rather than mimicking a clock.


## [0.0.9] 2021-08-01

	- Updated all plot components.
	- Added Plot.SetLayer() supporting layers
	- Added RotateCanvas() overrides for 3-axus euler and quaternion.


## [0.0.8] 2021-07-27

	- Added PlotRect and PlotRing components.
	- Labels are now hidden in hierarchy by default.
	- Fixed vertical pivot for DrawRect.
	- Fixed long standing issue with arc anti-aliasing.


## [0.0.7] 2021-01-01

 	- Added Arc argument constrainAngleSpanToRoundness.
 	- Added field size argument for DrawText().
 	- Fixed missing default UV rect for the fill texture.
 	- Updated SetNoFill() to now also forgets current fill texture.
 	- Added more argument options for SetFillTextureTint().


## [0.0.6] 2020-12-01

 	- Renamed LineRenderer to LinePRenderer to avoid name collision.


## [0.0.5] 2020-12-02

	- Added SetFillTexture support (except for Polygon).
	- Added SetStrokeCornerProfile.
	- Added useGemetricRoundness to Arc and Pie.
	- Added cutoff to Arc.
	- Implemented min size presevation and fade out for all shapes.
	- Added ShiffmansRecurveTree example.
	- Optimised matrix operations.
	- Added support for changing Polygon and Polyline while drawing.
	- Added Polygon.SetAsStar and SetAsNGon methods.
	- Added DrawXNow, BeginDrawNowToTexture() and EndDrawNowToTexture() methods.
	- Added PushStyle() and PopStyle().
	- Renamed the library from Draw to Plot.
	- Added Square shape, drawn using Rect shape.
	- Implemented shape size preservation when antialised.
	- Fixed Polygon stroke not aligning correctly.
	- Fixed transformations with strokes not scaling correctly.


## [0.0.4] 2020-10-05

	- Fixed SetNoFill issue.
	- Added checks for unnecessary matrix operations.
	- Implemented antialiasing for polygons without stroke.


## [0.0.3] 2020-10-04

	- Optimised polygon triangulation.
	- Fixed NoStroke issue.
	- Added DrawRing.
	- Corrected size of rects when antialiased, so they align perfectly, as expected.
	- Removed CapAlignment and changed Cap modes. Alignment now depends on the mode.
	- Moved DrawLinePoints, DrawPolylinePoints and DrawPolygonLinePoints into a static DrawDebug class.


## [0.0.2] 2020-09-30

	- Added SetAntialiasing option.
	- Implemented caps and CapAlignment for Polyline.
	- Reduced SetPivot methods to a single method and fixed pivot transformations.
	- Added DrawLinePoints, DrawPolygonPoints and DrawPolylinePoints.
	- Improved the Earcut triangulation for Unity.


## [0.0.1] 2020-09-29

	- First version, for educational use only.
