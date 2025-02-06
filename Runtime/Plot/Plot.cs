/*
	Copyright © Carl Emil Carlsen 2020-2024
	http://cec.dk


	Structure
	=========
	
	Shapes are Rendered using PRenderers, each having their own shader. For exampe, 
	circles and rings are rendered using RingPRenderer (Ring.shader). To handle multiple 
	shader features (multi compile), each PRenderer keeps a pool of materials. Some style
	modifiers, like SetFillTexture, will enable a shader feature. When features change
	from one draw call to the next, naturally, instancing won't work.
*/

using UnityEngine;
using System;
using System.Collections.Generic;
using PlotInternals;
using TMPro;

[Serializable]
public partial class Plot
{
	#region Constants

	public const float Pi = Mathf.PI;
	public const float HalfPi = Pi * 0.5f;
	public const float Tau = Pi * 2;

	#endregion // Constants


	#region Enums

	/// <summary>
	/// Stroke alignment options.
	/// </summary>
	[Serializable] public enum StrokeAlignment { Inside, Edge, Outside }

	/// <summary>
	/// Stroke cap options, applied to DrawLine() and DrawPolyline().
	/// </summary>
	[Serializable] public enum StrokeCap { None, Square, Round }

	/// <summary>
	/// Stroke corner profile options.
	/// </summary>
	[Serializable] public enum StrokeCornerProfile { Hard, Round }

	/// <summary>
	/// Pivot options.
	/// </summary>
	[Serializable] public enum Pivot { Center, TopLeft, Top, TopRight, Right, BottomRight, Bottom, BottomLeft, Left }

	/// <summary>
	/// Shape blending options.
	/// </summary>
	[Serializable] public enum Blend { Transparent, TransparentAdditive }

	/// <summary>
	/// Fill texture onto fill color blend options.
	/// </summary>
	[Serializable] public enum FillTextureBlend { Overlay, Multiply, Replace }

	/// <summary>
	/// Spatial coordinate metrics.
	/// </summary>
	[Serializable] public enum Space { Pixels, Normalized }

	/// <summary>
	///	Roundness design, used for DrawArc().
	/// </summary>
	[Serializable] public enum RoundnessDesign { Geometric, Organic }

	#endregion // Types


	#region Initiation


	public static void Reset()
	{
		if( _p == null ) return; // Will reset on create anyway.

		_p._style = Style.GetDefault();
	}


	/// <summary>
	/// Creates a new Polygon to be drawn using Plot.DrawPolygon(). Points must be provided in clockwise order. 
	/// </summary>
	public static Polygon CreatePolygon()
	{
		return ScriptableObject.CreateInstance<Polygon>();
	}
	public static Polygon CreatePolygon( int pointCapacity )
	{
		var polygon = ScriptableObject.CreateInstance<Polygon>();
		polygon.SetPointCapacity( pointCapacity );
		return polygon;
	}
	public static Polygon CreatePolygon( Vector2[] points )
	{
		var polygon = ScriptableObject.CreateInstance<Polygon>();
		polygon.SetPoints( points );
		return polygon;
	}
	public static Polygon CreatePolygon( List<Vector2> points )
	{
		var polygon = ScriptableObject.CreateInstance<Polygon>();
		polygon.SetPoints( points );
		return polygon;
	}

	
	/// <summary>
	/// Creates a new Polyline to be drawn using Plot.DrawPolyline(). Points must be provided in clockwise order. 
	/// </summary>
	public static Polyline CreatePolyline()
	{
		return ScriptableObject.CreateInstance<Polyline>();
	}
	public static Polyline CreatePolyline( int pointCapacity )
	{
		var polyline = ScriptableObject.CreateInstance<Polyline>();
		polyline.SetPointCapacity( pointCapacity );
		return polyline;
	}
	public static Polyline CreatePolyline( Vector2[] points )
	{
		var polyline = ScriptableObject.CreateInstance<Polyline>();
		polyline.SetPoints( points );
		return polyline;
	}
	public static Polyline CreatePolyline( List<Vector2> points )
	{
		var polyline = ScriptableObject.CreateInstance<Polyline>();
		polyline.SetPoints( points );
		return polyline;
	}


	/// <summary>
	/// Creates a new Text to be drawn using Plot.DrawText().
	/// </summary>
	public static Text CreateText( string content = "" )
	{
		var text = ScriptableObject.CreateInstance<Text>();

		if( !string.IsNullOrEmpty( content ) ) text.SetContent( content );
		return text;
	}


	/// <summary>
	/// Adapts a list of Texts by destroying and creating new ones as needed.
	/// </summary>
	public static void AdaptTextCount( int count, List<Text> texts )
	{
		// If the labels list count is correct, then check if it still contains labels and have fonts.
		// In the case that we have no entries, labels may still be hanging around waiting to be destroyed.
		if( count != 0 && texts.Count == count ) {
			bool allGood = true;
			foreach( Text tm in texts ) {
				//if( !tm || tm.font == null ) {
				if( !tm ) {
					allGood = false;
					break;
				}
			}
			if( allGood ) return;
		}

		// Destroy excess.
		while( texts.Count > count ) {
			UnityEngine.Object.DestroyImmediate( texts[ texts.Count - 1 ] );
			texts.RemoveAt( texts.Count - 1 );
		}

		// Ensure existing labels has font.
		//if( !font ) foreach( var label in labels ) if( label && !label.font ) label.font = TMP_Settings.defaultFontAsset;

		// Added missing.
		while( texts.Count < count ) {
			var text = CreateText();
			//if( font ) text.font = font;
			texts.Add( text );
		}
	}


	#endregion


	#region Draw


	/// <summary>
	/// Submit a circle shape instance for rendering.
	/// </summary>
	public static void DrawCircle() => P().DrawRingInternal( 0, 0, -_p._style.strokeWidth-1f, 1f );
	public static void DrawCircle( float x, float y, float diameter ) => P().DrawRingInternal( x, y, -_p._style.strokeWidth-diameter, diameter );
	public static void DrawCircle( Vector2 position, float diameter ) => P().DrawRingInternal( position.x, position.y, -_p._style.strokeWidth - diameter, diameter );


	/// <summary>
	/// Submit a ring shape instance for rendering.
	/// </summary>
	public static void DrawRing( float x, float y, float innerDiameter, float outerDiameter ) => P().DrawRingInternal( x, y, innerDiameter, outerDiameter );
	public static void DrawRing( Vector2 position, float innerDiameter, float OuterDiameter ) => P().DrawRingInternal( position.x, position.y, innerDiameter, OuterDiameter );


	/// <summary>
	/// Submit a pie shape instance for rendering.
	/// </summary>
	public static void DrawPie( float x, float y, float diameter, float beginAngle, float deltaAngle, float cutOff = 0, float roundness = 0 ) => P().DrawArcInternal( x, y, -_p._style.strokeWidth-diameter, diameter, beginAngle, deltaAngle, cutOff, roundness );
	public static void DrawPie( Vector2 position, float diameter, float beginAngle, float deltaAngle, float cutOff = 0, float roundness = 0 ) => P().DrawArcInternal( position.x, position.y, -_p._style.strokeWidth-diameter, diameter, beginAngle, deltaAngle, cutOff, roundness );


	/// <summary>
	/// Submit an arc shape instance for rendering.
	/// </summary>
	public static void DrawArc( float x, float y, float innerDiameter, float outerDiameter, float beginAngle, float deltaAngle, float cutOff = 0, float roundness = 0, bool useGeometricRoundness = false, bool constrainAngleSpanToRoundness = false ) => P().DrawArcInternal( x, y, innerDiameter, outerDiameter, beginAngle, deltaAngle, cutOff, roundness, useGeometricRoundness, constrainAngleSpanToRoundness );
	public static void DrawArc( Vector2 position, float innerDiameter, float outerDiameter, float beginAngle, float deltaAngle, float cutOff = 0, float roundness = 0, bool useGeometricRoundness = false, bool constrainAngleSpanToRoundness = false ) => P().DrawArcInternal( position.x, position.y, innerDiameter, outerDiameter, beginAngle, deltaAngle, cutOff, roundness, useGeometricRoundness, constrainAngleSpanToRoundness );


	/// <summary>
	/// Submit a rect shape instance for rendering.
	/// </summary>
	public static void DrawRect( float x, float y, float width, float height ) => P().DrawRectInternal( x, y, width, height, 0, 0, 0, 0 );
	public static void DrawRect( float x, float y, float width, float height, float roundness ) => P().DrawRectInternal( x, y, width, height, roundness, roundness, roundness, roundness );
	public static void DrawRect( float x, float y, float width, float height, float lowerLeftRoundness, float upperLeftRoundness, float upperRightRoundness, float lowerRightRoundness ) => P().DrawRectInternal( x, y, width, height, lowerLeftRoundness, upperLeftRoundness, upperRightRoundness, lowerRightRoundness );
	public static void DrawRect( Vector2 position, float width, float height, float lowerLeftRoundness, float upperLeftRoundness, float upperRightRoundness, float lowerRightRoundness ) => P().DrawRectInternal( position.x, position.y, width, height, lowerLeftRoundness, upperLeftRoundness, upperRightRoundness, lowerRightRoundness );
	public static void DrawRect( Vector2 position, float width, float height, float roundness = 0 ) => P().DrawRectInternal( position.x, position.y, width, height, roundness, roundness, roundness, roundness );


	/// <summary>
	/// Submit a square shape instance for rendering.
	/// </summary>
	public static void DrawSquare( float x, float y, float size ) => P().DrawRectInternal( x, y, size, size, 0f, 0f, 0f, 0f );
	public static void DrawSquare( float x, float y, float size, float roundness ) => P().DrawRectInternal( x, y, size, size, roundness, roundness, roundness, roundness );
	public static void DrawSquare( float x, float y, float size, float lowerLeftRoundness, float upperLeftRoundness, float upperRightRoundness, float lowerRightRoundness ) => P().DrawRectInternal( x, y, size, size, lowerLeftRoundness, upperLeftRoundness, upperRightRoundness, lowerRightRoundness );
	public static void DrawSquare( Vector2 position, float size, float lowerLeftRoundness, float upperLeftRoundness, float upperRightRoundness, float lowerRightRoundness ) => P().DrawRectInternal( position.x, position.y, size, size, lowerLeftRoundness, upperLeftRoundness, upperRightRoundness, lowerRightRoundness );
	public static void DrawSquare( Vector2 position, float size, float roundness = 0 ){ P().DrawRectInternal( position.x, position.y, size, size, roundness, roundness, roundness, roundness ); }


	/// <summary>
	/// Submit a line shape instance for rendering.
	/// </summary>
	public static void DrawLine( float ax, float ay, float bx, float by ) => P().DrawLineInternal( ax, ay, bx, by, StrokeCap.Round, StrokeCap.Round );
	public static void DrawLine( float ax, float ay, float bx, float by, StrokeCap caps ) => P().DrawLineInternal( ax, ay, bx, by, caps, caps );
	public static void DrawLine( float ax, float ay, float bx, float by, StrokeCap beginCap, StrokeCap endCap ) => P().DrawLineInternal( ax, ay, bx, by, beginCap, endCap );
	public static void DrawLine( Vector2 positionA, Vector2 positionB, StrokeCap beginCap, StrokeCap endCap ) => P().DrawLineInternal( positionA.x, positionA.y, positionB.x, positionB.y, beginCap, endCap );
	public static void DrawLine( Vector2 positionA, Vector2 positionB, StrokeCap caps ) => P().DrawLineInternal( positionA.x, positionA.y, positionB.x, positionB.y, caps, caps );
	public static void DrawLine( Vector2 positionA, Vector2 positionB ) => P().DrawLineInternal( positionA.x, positionA.y, positionB.x, positionB.y, StrokeCap.Round, StrokeCap.Round );


	/// <summary>
	/// Submit a polygon shape instance for rendering.
	/// </summary>
	public static void DrawPolygon( Polygon polygon ) => P().DrawPolygonInternal( polygon );


	/// <summary>
	/// Submit a polyline shape instance for rendering.
	/// </summary>
	public static void DrawPolyline( Polyline polyline, StrokeCap beginCap, StrokeCap endCap ) => P().DrawPolylineInternal( polyline, beginCap, endCap );
	public static void DrawPolyline( Polyline polyline, StrokeCap caps = StrokeCap.Round ) => P().DrawPolylineInternal( polyline, caps, caps );


	/// <summary>
	/// Submit a text instance for rendering.
	/// </summary>
	public static void DrawText( Text text, float x, float y, float fieldwidth, float fieldHeight, bool drawFieldDebugRect = false ) => P().DrawTextInternal( text, x, y, fieldwidth, fieldHeight, drawFieldDebugRect );
	public static void DrawText( Text text, Vector2 position, Vector2 fieldSize, bool drawFieldDebugRect = false ) => P().DrawTextInternal( text, position.x, position.y, fieldSize.x, fieldSize.y, drawFieldDebugRect );

	#endregion // Draw



	#region StyleModifiers


	/// <summary>
	/// Set the fill color to be used for subsequently drawn shapes.
	/// </summary>
	public static void SetFillColor( float brightness ) => SetFillColor( new Color( brightness, brightness, brightness ) );
	public static void SetFillColor( float brightness, float alpha ) => SetFillColor( new Color( brightness, brightness, brightness, alpha ) );
	public static void SetFillColor( float red, float green, float blue ) => SetFillColor( new Color( red, green, blue ) );
	public static void SetFillColor( float red, float green, float blue, float alpha ) => SetFillColor( new Color( red, green, blue, alpha ) );
	public static void SetFillColor( Color color, float alphaOverride ) => SetFillColor( ColorWithAlpha( color, alphaOverride ) );
	public static void SetFillColor( Color color )
	{
		P();
		_p._style.fillColor = color;
		_p._style.fillEnabled = true;
		foreach( FillPRenderer r in _p._fillRenderers ) r.isFillColorDirty = true;
	}


	/// <summary>
	/// Set no fill for subsequently drawn shapes.
	/// </summary>
	public static void SetNoFill()
	{
		P();
		_p._style.fillEnabled = false;
		//_p._style.fillColor = Color.clear;
		//_p._style.fillTexture = null;
		//foreach( FillPRenderer r in _p._fillRenderers ) r.isFillColorDirty = true;
		//foreach( FillPRenderer r in _p._fillRenderers ) r.SetFillTextureFeature( null );
	}


	/// <summary>
	/// Set the stroke color to be used for subsequently drawn shapes.
	/// </summary>
	public static void SetStrokeColor( float brightness ) => SetStrokeColor( new Color( brightness, brightness, brightness ) );
	public static void SetStrokeColor( float brightness, float alpha ) => SetStrokeColor( new Color( brightness, brightness, brightness, alpha ) );
	public static void SetStrokeColor( float red, float green, float blue ) => SetStrokeColor( new Color( red, green, blue ) );
	public static void SetStrokeColor( float red, float green, float blue, float alpha ) => SetStrokeColor( new Color( red, green, blue, alpha ) );
	public static void SetStrokeColor( Color color, float alphaOverride ) => SetStrokeColor( ColorWithAlpha( color, alphaOverride ) );
	public static void SetStrokeColor( Color color )
	{
		P();
		_p._style.strokeEnabled = true;
		_p._style.strokeColor = color;
		foreach( PRenderer r in _p._allRenderers ) r.isStrokeColorDirty = true;
	}


	/// <summary>
	/// Set the stroke width (thickness) to be used for subsequently drawn shapes.
	/// </summary>
	public static void SetStrokeWidth( float width )
	{
		P();
		bool strokeEnabedChange = width > 0 != _p._style.strokeWidth > 0;
		//if( strokeEnabedChange ) foreach( PRenderer r in _p._allRenderers ) r.isStrokeColorDirty = true; // We need to update color when we change from no stroke to stroke.
		_p._style.strokeEnabled = true;
		_p._style.strokeWidth = width;
	}


	/// <summary>
	/// Set no stroke for subsequently drawn shapes.
	/// </summary>
	public static void SetNoStroke()
	{
		P();
		_p._style.strokeEnabled = false;
		//_p._style.strokeColor = Color.clear;
		//foreach( PRenderer r in _p._allRenderers ) r.isStrokeColorDirty = true;
	}


	/// <summary>
	/// Set the stroke alignment to be used for subsequently drawn shapes.
	/// </summary>
	public static void SetStrokeAlignement( StrokeAlignment alignment ){ P()._style.strokeAlignment = alignment; }


	/// <summary>
	/// Set the stroke corner profile to be used for subsequently drawn shapes.
	/// </summary>
	public static void SetStrokeCornerProfile( StrokeCornerProfile cornerStyle ) { P()._style.strokeCornerProfile = cornerStyle; }


	/// <summary>
	/// Enable or disable pixel shader SDF based antialisaing for all subsequently drawn shapes.
	/// Note that edge alignment between shapes will not be seamless when anti-alisation is enabled.
	/// </summary>
	public static void SetAntiAliasing( bool isOn )
	{
		P();
		foreach( PRenderer r in  _p._allRenderers ) r.SetShapeAntialiasingFeature( isOn );
		_p._style.antialias = isOn;
	}


	/// <summary>
	/// Set the blend mode used for subsequently drawn shapes.
	/// </summary>
	public static void SetBlend( Blend blend )
	{
		P();
		if( _p._style.blend == blend ) return;
		foreach( PRenderer r in _p._allRenderers ) r.SetBlendFeature( blend );
		_p._style.blend = blend;
	}


	/// <summary>
	/// Set the layer used for subsequently drawn shapes. Does not work for DrawNow methods, just like Graphics.DrawMeshNow not regarding layers.
	/// </summary>
	public static void SetLayer( int layer )
	{
		P();
		_p._style.layer = layer;
	}


	/// <summary>
	/// Set the point from which Circle will be drawn. Default is Pivot.Center.
	/// </summary>
	public static void SetPivot( Pivot pivot ){
		P();
		_p._style.pivot = pivot;
		_p._pivotPosition = GetPivotPosition( pivot );
	}


	/// <summary>
	/// Set the font used for subsequently drawn texts.
	/// </summary>
	public static void SetTextFont( TMP_FontAsset font )
	{
		P()._style.textFont = font;
	}


	/// <summary>
	/// Set the size to be used for subsequently drawn texts in world space scale.
	/// </summary>
	public static void SetTextSize( float textSize )
	{
		P()._style.textSize = textSize * 10; // World space text size to TMP font size.
	}


	/// <summary>
	/// Set the alignment to be used for subsequently drawn texts.
	/// </summary>
	public static void SetTextAlignment( TextAlignmentOptions alignment )
	{
		P()._style.textAlignment = alignment;
	}


	/// <summary>
	/// Set the fill texture to be used for subsequently drawn shapes.
	/// See also SetFillTextureUVRect, SetFillTextureBlend and SetFillTextureTint.
	/// </summary>
	public static void SetFillTexture( Texture texture )
	{
		P();
		foreach( FillPRenderer r in _p._fillRenderers ) r.SetFillTextureFeature( texture );
		_p._style.fillTexture = texture;
	}


	/// <summary>
	/// Disable fill texture for subsequently drawn shapes.
	/// </summary>
	public static void SetNoFillTexture() { SetFillTexture( null ); }



	/// <summary>
	/// Set the uv rect to be used for subsequently drawn shapes that has a fill texture.
	/// </summary>
	public static void SetFillTextureUVRect( Rect uvRect ) { SetFillTextureUVRect( uvRect.x, uvRect.y, uvRect.width, uvRect.height ); }
	public static void SetFillTextureUVRect( float x, float y, float width, float height )
	{
		P();
		_p._style.fillTextureUVRect = new Vector4( x, y, width, height );
	}


	/// <summary>
	/// Set the texture blend mode to be used for subsequently drawn shapes that has a fill texture.
	/// </summary>
	public static void SetFillTextureBlend( FillTextureBlend blend )
	{
		P();
		foreach( FillPRenderer r in _p._fillRenderers ) r.SetFillTextureBlendFeature( blend );
		_p._style.fillTextureBlend = blend;
	}


	/// <summary>
	/// Push (save) the current style to the stack.
	/// </summary>
	public static void PushStyle()
	{
		P();
		_p._styleStack.Push( _p._style );
	}


	/// <summary>
	/// Pop (load) the last pushed style from the stack.
	/// </summary>
	public static void PopStyle()
	{
		P();
		Style s = _p._styleStack.Pop();
		_p._style = s;
		_p._pivotPosition = GetPivotPosition( s.pivot );
		_p.ApplyStyleFeaturesToAllRenderer( ref s );
	}


	/// <summary>
	/// Copy and return the current style.
	/// </summary>
	public static Style GetStyle() => P()._style;


	/// <summary>
	/// Overwrite the current style.
	/// </summary>
	public static void SetStyle( Style style ) {
		P();
		_p._style = style;
		_p._pivotPosition = GetPivotPosition( style.pivot );
		_p.ApplyStyleFeaturesToAllRenderer( ref style );
	}


	/// <summary>
	/// Shorthand for PushCanvas() and PushStyle().
	/// </summary>
	public static void PushCanvasAndStyle(){
		PushCanvas();
		PushStyle();
	}


	/// <summary>
	/// Shorthand for PushCanvas() and PushStyle().
	/// </summary>
	public static void PopCanvasAndStyle(){
		PopCanvas();
		PopStyle();
	}


	/// <summary>
	/// Set the texture tint to be used for subsequently drawn shapes that has a fill texture.
	/// </summary>
	public static void SetFillTextureTint( float brightness ) => SetFillTextureTint( new Color( brightness, brightness, brightness ) );
	public static void SetFillTextureTint( float brightness, float alpha ) => SetFillTextureTint( new Color( brightness, brightness, brightness, alpha ) );
	public static void SetFillTextureTint( float red, float green, float blue ) => SetFillTextureTint( new Color( red, green, blue ) );
	public static void SetFillTextureTint( float red, float green, float blue, float alpha ) => SetFillTextureTint( new Color( red, green, blue, alpha ) );
	public static void SetFillTextureTint( Color color, float alphaOverride ) => SetFillTextureTint( ColorWithAlpha( color, alphaOverride ) );
	public static void SetFillTextureTint( Color tint )
	{
		P();
		_p._style.fillTextureTint = tint;
	}


	#endregion // StyleModifiers


	#region TransformModifiers


	/// <summary>
	/// Push (save) the current canvas transformation matrix to the stack.
	/// </summary>
	public static void PushCanvas(){
		P();
		_p._matrixStack.Push( _p._matrix );
	}


	/// <summary>
	/// Pop (load) the last pushed canvas transformation matrix from the stack.
	/// </summary>
	public static void PopCanvas()
	{
		P();
		_p._matrix = _p._matrixStack.Pop();
	}


	/// <summary>
	/// Get the current canvas transformation matrix.
	/// </summary>
	public static Matrix4x4 GetCanvas(){ return P()._matrix; }


	/// <summary>
	/// Overwrite the current canvas transformation matrix.
	/// </summary>
	public static void SetCanvas( Matrix4x4 matrix ){ P()._matrix = matrix; }


	/// <summary>
	/// Overwrite the current canvas transformation matrix with a transform (localToWorldMatrix).
	/// </summary>
	public static void SetCanvas( Transform transform ) { P()._matrix = transform.localToWorldMatrix; }


	/// <summary>
	/// Translate the current canvas transformation matrix.
	/// </summary>
	public static void TranslateCanvas( float x, float y ) => P()._matrix.Translate3x4( x, y );
	public static void TranslateCanvas( float x, float y, float z ) => P()._matrix.Translate3x4( x, y, z );
	public static void TranslateCanvas( Vector2 translation ) => P()._matrix.Translate3x4( translation.x, translation.y );
	public static void TranslateCanvas( Vector3 translation ) => P()._matrix.Translate3x4( translation.x, translation.y, translation.z );


	/// <summary>
	/// Rotate the current canvas transformation matrix by angle (in degrees).
	/// </summary>
	public static void RotateCanvas( float angleZ ) => P()._matrix.Rotate3x4( angleZ );
	public static void RotateCanvas( float angleX, float angleY, float angleZ ) => P()._matrix *= Matrix4x4.Rotate( Quaternion.Euler( angleX, angleY, angleZ ) );
	public static void RotateCanvas( Quaternion rotation ) => P()._matrix *= Matrix4x4.Rotate( rotation );


	/// <summary>
	/// Scale the current canvas transformation matrix.
	/// </summary>
	public static void ScaleCanvas( float scaleXYZ ) => P()._matrix.Scale3x4( scaleXYZ );
	public static void ScaleCanvas( float scaleX, float scaleY ) => P()._matrix.Scale3x4( scaleX, scaleY );
	public static void ScaleCanvas( float scaleX, float scaleY, float scaleZ ) => P()._matrix.Scale3x4( scaleX, scaleY, scaleZ );
	public static void ScaleCanvas( Vector2 scale ) => P()._matrix.Scale3x4( scale.x, scale.y );
	public static void ScaleCanvas( Vector3 scale ) => P()._matrix.Scale3x4( scale.x, scale.y, scale.z );

	

	#endregion // TransformModifiers



	#region DrawNow

	/// <summary>
	/// Begin a DrawNowToRenderTexture session. Call DrawXNow subsequently (for example DrawCircleNow) and don't forget to call EndDrawNowToTexture when you are done.
	/// For Space.Normalized 0,0 is center. Left, right, top and bottom is ( -aspect, aspect, -1, 1 ). For Space.Pixels 0,0 is in upper left corner.
	/// </summary>
	public static void BeginDrawNowToRenderTexture( RenderTexture rt ) => BeginDrawNowToRenderTextureInternal( rt, Space.Pixels, Color.clear, false );
	public static void BeginDrawNowToRenderTexture( RenderTexture rt, Space space ) => BeginDrawNowToRenderTextureInternal( rt, space, Color.clear, false );
	public static void BeginDrawNowToRenderTexture( RenderTexture rt, Color clearColor ) => BeginDrawNowToRenderTextureInternal( rt, Space.Pixels, clearColor, true );
	public static void BeginDrawNowToRenderTexture( RenderTexture rt, Space space, Color clearColor ) => BeginDrawNowToRenderTextureInternal( rt, space, clearColor, true );

	
	/// <summary>
	/// End a DrawNowToTexture session
	/// </summary>
	public static void EndDrawNowToRenderTexture() => EndDrawNowToRenderTextureInternal();


	/// <summary>
	/// Clear a RenderTexture with a color immediately.
	/// </summary>
	public static void ClearRenderTextureNow( RenderTexture rt, Color clearColor ) => ClearRenderTextureNowInternal( rt, clearColor );


	/// <summary>
	/// Draw a circle immediately using Graphics.DrawMeshNow. Call this from OnPostRender or after calling BeginDrawNowToTexture.
	/// </summary>
	public static void DrawCircleNow() => P().DrawRingInternal( 0f, 0f, -_p._style.strokeWidth - 1f, 1f, drawNow: true );
	public static void DrawCircleNow( float x, float y, float diameter ) => P().DrawRingInternal( x, y, -_p._style.strokeWidth - diameter, diameter, drawNow: true );
	public static void DrawCircleNow( Vector2 position, float diameter ) => P().DrawRingInternal( position.x, position.y, -_p._style.strokeWidth - diameter, diameter, drawNow: true );


	/// <summary>
	/// Draw a ring immediately using Graphics.DrawMeshNow. Call this from OnPostRender or after calling BeginDrawNowToTexture.
	/// </summary>
	public static void DrawRingNow( float x, float y, float innerDiameter, float OuterDiameter ) => P().DrawRingInternal( x, y, innerDiameter, OuterDiameter, drawNow: true );
	public static void DrawRingNow( Vector2 position, float innerDiameter, float OuterDiameter ) => P().DrawRingInternal( position.x, position.y, innerDiameter, OuterDiameter, drawNow: true );


	/// <summary>
	/// Draw a pie immediately using Graphics.DrawMeshNow. Call this from OnPostRender or after calling BeginDrawNowToTexture.
	/// </summary>
	public static void DrawPieNow( float x, float y, float diameter, float beginAngle, float deltaAngle, float cutOff = 0, float roundness = 0 ) => P().DrawArcInternal( x, y, -_p._style.strokeWidth-diameter, diameter, beginAngle, deltaAngle, cutOff, roundness, false, false, drawNow: true );
	public static void DrawPieNow( Vector2 position, float diameter, float beginAngle, float deltaAngle, float cutOff = 0, float roundness = 0 ) => P().DrawArcInternal( position.x, position.y, -_p._style.strokeWidth-diameter, diameter, beginAngle, deltaAngle, cutOff, roundness, false, false, drawNow: true );


	/// <summary>
	/// Draw a pie immediately using Graphics.DrawMeshNow. Call this from OnPostRender or after calling BeginDrawNowToTexture. Angles in degrees. AngleBegin must be smaller than AngleEnd.
	/// </summary>
	public static void DrawArcNow( float x, float y, float innerDiameter, float outerDiameter, float beginAngle, float deltaAngle, float cutOff = 0, float roundness = 0, bool useGeometricRoundness = false, bool constrainAngleSpanToRoundness = false ) => P().DrawArcInternal( x, y, innerDiameter, outerDiameter, beginAngle, deltaAngle, cutOff, roundness, useGeometricRoundness, constrainAngleSpanToRoundness, drawNow: true );
	public static void DrawArcNow( Vector2 position, float innerDiameter, float outerDiameter, float beginAngle, float deltaAngle, float cutOff = 0, float roundness = 0, bool useGeometricRoundness = false, bool constrainAngleSpanToRoundness = false ) => P().DrawArcInternal( position.x, position.y, innerDiameter, outerDiameter, beginAngle, deltaAngle, cutOff, roundness, useGeometricRoundness, constrainAngleSpanToRoundness, drawNow: true );


	/// <summary>
	/// Draw a rectangle immediately using Graphics.DrawMeshNow. Call this from OnPostRender or after calling BeginDrawNowToTexture.
	/// </summary>
	public static void DrawRectNow( float x, float y, float width, float height ) => P().DrawRectInternal( x, y, width, height, 0, 0, 0, 0, true );
	public static void DrawRectNow( float x, float y, float width, float height, float roundness ) => P().DrawRectInternal( x, y, width, height, roundness, roundness, roundness, roundness, drawNow: true );
	public static void DrawRectNow( float x, float y, float width, float height, float lowerLeftRoundness, float upperLeftRoundness, float upperRightRoundness, float lowerRightRoundness ) => P().DrawRectInternal( x, y, width, height, lowerLeftRoundness, upperLeftRoundness, upperRightRoundness, lowerRightRoundness, drawNow: true );
	public static void DrawRectNow( Vector2 position, float width, float height, float lowerLeftRoundness, float upperLeftRoundness, float upperRightRoundness, float lowerRightRoundness ) => P().DrawRectInternal( position.x, position.y, width, height, lowerLeftRoundness, upperLeftRoundness, upperRightRoundness, lowerRightRoundness, drawNow: true );
	public static void DrawRectNow( Vector2 position, float width, float height, float roundness = 0 ) => P().DrawRectInternal( position.x, position.y, width, height, roundness, roundness, roundness, roundness, drawNow: true );


	/// <summary>
	/// Draw a square immediately using Graphics.DrawMeshNow. Call this from OnPostRender or after calling BeginDrawNowToTexture.
	/// </summary>
	public static void DrawSquareNow( float x, float y, float size ) => P().DrawRectInternal( x, y, size, size, 0, 0, 0, 0, true );
	public static void DrawSquareNow( float x, float y, float size, float roundness ) => P().DrawRectInternal( x, y, size, size, roundness, roundness, roundness, roundness, drawNow: true );
	public static void DrawSquareNow( float x, float y, float size, float lowerLeftRoundness, float upperLeftRoundness, float upperRightRoundness, float lowerRightRoundness ) => P().DrawRectInternal( x, y, size, size, lowerLeftRoundness, upperLeftRoundness, upperRightRoundness, lowerRightRoundness, drawNow: true );
	public static void DrawSquareNow( Vector2 position, float size, float lowerLeftRoundness, float upperLeftRoundness, float upperRightRoundness, float lowerRightRoundness ) => P().DrawRectInternal( position.x, position.y, size, size, lowerLeftRoundness, upperLeftRoundness, upperRightRoundness, lowerRightRoundness, drawNow: true );
	public static void DrawSquareNow( Vector2 position, float size, float roundness = 0 ) => P().DrawRectInternal( position.x, position.y, size, size, roundness, roundness, roundness, roundness, drawNow: true );


	/// <summary>
	/// Draw a line immediately using Graphics.DrawMeshNow. Call this from OnPostRender or after calling BeginDrawNowToTexture.
	/// </summary>
	public static void DrawLineNow( float ax, float ay, float bx, float by ) => P().DrawLineInternal( ax, ay, bx, by, StrokeCap.Round, StrokeCap.Round, drawNow: true );
	public static void DrawLineNow( float ax, float ay, float bx, float by, StrokeCap caps ) => P().DrawLineInternal( ax, ay, bx, by, caps, caps, drawNow: true );
	public static void DrawLineNow( float ax, float ay, float bx, float by, StrokeCap beginCap, StrokeCap endCap ) => P().DrawLineInternal( ax, ay, bx, by, beginCap, endCap, drawNow: true );
	public static void DrawLineNow( Vector2 positionA, Vector2 positionB, StrokeCap beginCap, StrokeCap endCap ) => P().DrawLineInternal( positionA.x, positionA.y, positionB.x, positionB.y, beginCap, endCap, drawNow: true );
	public static void DrawLineNow( Vector2 positionA, Vector2 positionB, StrokeCap caps ) => P().DrawLineInternal( positionA.x, positionA.y, positionB.x, positionB.y, caps, caps, drawNow: true );
	public static void DrawLineNow( Vector2 positionA, Vector2 positionB ) => P().DrawLineInternal( positionA.x, positionA.y, positionB.x, positionB.y, StrokeCap.Round, StrokeCap.Round, drawNow: true );


	/// <summary>
	/// Draw a polygon immediately using Graphics.DrawMeshNow. Call this from OnPostRender or after calling BeginDrawNowToTexture.
	/// </summary>
	public static void DrawPolygonNow( Polygon polygon ) => P().DrawPolygonInternal( polygon, true );


	/// <summary>
	/// Draw a polygon immediately using Graphics.DrawMeshNow. Call this from OnPostRender or after calling BeginDrawNowToTexture.
	/// </summary>
	public static void DrawPolylineNow( Polyline polyline, StrokeCap beginCap, StrokeCap endCap ) => P().DrawPolylineInternal( polyline, beginCap, endCap, drawNow: true );
	public static void DrawPolylineNow( Polyline polyline, StrokeCap caps = StrokeCap.Round ) => P().DrawPolylineInternal( polyline, caps, caps, drawNow: true );


	/// <summary>
	/// Draw a text immediately using Graphics.DrawMeshNow. Call this from OnPostRender or after calling BeginDrawNowToTexture.
	/// </summary>
	public static void DrawTextNow( Text text, float x, float y, float fieldwidth, float fieldHeight, bool drawDebugRect = false ) => P().DrawTextInternal( text, x, y, fieldwidth, fieldHeight, drawDebugRect, drawNow: true );
	public static void DrawTextNow( Text text, Vector2 position, Vector2 fieldSize, bool drawDebugRect = false ) => P().DrawTextInternal( text, position.x, position.y, fieldSize.x, fieldSize.y, drawDebugRect, drawNow: true );


	#endregion // DrawNow


	#region HelperMethods


	public static TextAlignmentOptions ConvertOffsetToTextAlignment( Vector2 offset, float axisSnapDistance = 0.01f )
	{
		// Just something more corse than Mathf.Approximately please.
		bool zeroX = Mathf.Abs( offset.x ) < axisSnapDistance;
		bool zeroY = Mathf.Abs( offset.y ) < axisSnapDistance;

		if( zeroY ) {
			if( zeroX ) return TextAlignmentOptions.Midline;
			return offset.x > 0 ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.MidlineRight;
		}

		if( offset.y > 0 ) {
			if( zeroX ) return TextAlignmentOptions.Bottom;
			return offset.x > 0 ? TextAlignmentOptions.BottomLeft : TextAlignmentOptions.BottomRight;
		}

		if( offset.y < 0 ) {
			if( zeroX ) return TextAlignmentOptions.Top;
			return offset.x > 0 ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.TopRight;
		}

		return TextAlignmentOptions.Midline;
	}


	public static Pivot ConvertAlignmentToPivot( TextAlignmentOptions alignment )
	{
		switch( alignment )
		{
			case TextAlignmentOptions.BottomLeft: return Pivot.BottomLeft;
			case TextAlignmentOptions.Left: return Pivot.Left;
			case TextAlignmentOptions.MidlineLeft: return Pivot.Left;
			case TextAlignmentOptions.TopLeft: return Pivot.TopLeft;
			case TextAlignmentOptions.Top: return Pivot.Top;
			case TextAlignmentOptions.TopRight: return Pivot.TopRight;
			case TextAlignmentOptions.Right: return Pivot.Right;
			case TextAlignmentOptions.MidlineRight: return Pivot.Right;
			case TextAlignmentOptions.BottomRight: return Pivot.BottomRight;
			case TextAlignmentOptions.Bottom: return Pivot.Bottom;
		}
		return Pivot.Center;
	}

	#endregion HelperMethods
}
