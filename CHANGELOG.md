##0.0.12 (2022/08/XX)

**Changes**

- Updated to Unity 2022.1.
- Fixed Polygon not rendering stroke.


##0.0.11 (2021/11/18)

**Changes**

- Split the Plot class into partial classes and separated out TextMeshPro.


##0.0.10 (2021/09/19)

**Fixes**

- Added DrawToRenderTexture example.
- Added J. Tarbells Substrate as example.
- Added warning when using Draw() method after calling BeginDrawNowToTexture().
- Added missing TMP define checks in arc diagram example.
- Fixed edge color bug when no stroke.
- Arc angles now follows trigonometry (right is zero, increasing anti-clockwise) rather than mimicking a clock.


##0.0.9 (2021/08/01)

**CHANGES**

- Updated all plot components.
- Added Plot.SetLayer() supporting layers
- Added RotateCanvas() overrides for 3-axus euler and quaternion.


##0.0.8 (2021/07/27)

**CHANGES**

- Added PlotRect and PlotRing components.
- Labels are now hidden in hierarchy by default.
- Fixed vertical pivot for DrawRect.
- Fixed long standing issue with arc anti-aliasing.


##0.0.7 (2021/01/01)

**CHANGES**

 - Added Arc argument constrainAngleSpanToRoundness.
 - Added field size argument for DrawText().
 - Fixed missing default UV rect for the fill texture.
 - Updated SetNoFill() to now also forgets current fill texture.
 - Added more argument options for SetFillTextureTint().


##0.0.6 (2020/12/01)

**CHANGES**

 - Renamed LineRenderer to LinePRenderer to avoid name collision.


##0.0.5 (2020/12/02)

**CHANGES**

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


##0.0.4 (2020/10/05)

**CHANGES**

- Fixed SetNoFill issue.
- Added checks for unnecessary matrix operations.
- Implemented antialiasing for polygons without stroke.


##0.0.3 (2020/10/04)

**CHANGES**

- Optimised polygon triangulation.
- Fixed NoStroke issue.
- Added DrawRing.
- Corrected size of rects when antialiased, so they align perfectly, as expected.
- Removed CapAlignment and changed Cap modes. Alignment now depends on the mode.
- Moved DrawLinePoints, DrawPolylinePoints and DrawPolygonLinePoints into a static DrawDebug class.


##0.0.2 (2020/09/30)

**CHANGES**

- Added SetAntialiasing option.
- Implemented caps and CapAlignment for Polyline.
- Reduced SetPivot methods to a single method and fixed pivot transformations.
- Added DrawLinePoints, DrawPolygonPoints and DrawPolylinePoints.
- Improved the Earcut triangulation for Unity.


##0.0.1 (2020/09/29)

**CHANGES**

- First public version, for educational use.
