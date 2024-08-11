/*
	Copyright © Carl Emil Carlsen 2020-2021
	http://cec.dk


	Structure
	=========
	
	Shapes are Rendered using PRenderers, each having their own shader. For exampe, 
	circles and rings are rendered using RingPRenderer (Ring.shader). To handle multipe 
	shader features (multi compile), each PRenderer keeps a pool of materials. Some style
	modifiers, like SetFillTexture, will enable a shader feature. When features change
	from one draw call to the next, naturally, instancing won't work.
*/

using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Collections.Generic;
using PlotInternals;

public partial class Plot
{
	static Plot _p;

	RingPRenderer _ringRenderer;
	ArcPRenderer _arcRenderer;
	RectPRenderer _rectRenderer;
	LinePRenderer _lineRenderer;
	PolygonPRenderer _polygonRenderer;
	PolylinePRenderer _polylineRenderer;
	List<PRenderer> _allRenderers;
	List<FillPRenderer> _fillRenderers; // Fill renderers are different because they can display textures.

	Matrix4x4 _matrix = Matrix4x4.identity;
	Stack<Matrix4x4> _matrixStack;

	Style _style;
	Stack<Style> _styleStack;

	// Derived from current style.
	Vector2 _pivotPosition;

	bool _drawingToTextureNow;

	const MeshUpdateFlags meshFlags = MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices;
	const string logPrepend = "<b>[" + nameof( Plot ) + "]</b> ";

	public const float Pi = Mathf.PI;
	public const float HalfPi = Pi * 0.5f;
	public const float Tau = Pi * 2;
	const string antialiasKeyword = "_ANTIALIAS";

	static readonly Color defaultFillColor = Color.white;
	static readonly Color defaultStrokeColor = Color.black;
	const float defaultStrokeWidth = 0.05f;
	const StrokeAlignment defaultStrokeAlignment = StrokeAlignment.Outside;
	const bool defaultAntialias = true;
	const Pivot defaultPivot = Pivot.Center;
	const StrokeCornerProfile defaultStrokeCornerProfile = StrokeCornerProfile.Round;
	const Blend defaultBlend = Blend.Transparent;
	const FillTextureBlend defaultFillTextureBlend = FillTextureBlend.Overlay;
	static readonly Color defaultFillTextureTint = Color.white;


	#region Types

	[Serializable] public enum StrokeAlignment { Inside, Edge, Outside }
	[Serializable] public enum StrokeCap { None, Square, Round }
	[Serializable] public enum StrokeCornerProfile { Hard, Round }
	[Serializable] public enum Pivot { Center, TopLeft, Top, TopRight, Right, BottomRight, Bottom, BottomLeft, Left }
	[Serializable] public enum RoundnessDesign { Geometric, Organic }
	[Serializable] public enum Blend { Transparent, TransparentAdditive }
	[Serializable] public enum FillTextureBlend { Overlay, Multiply }
	[Serializable] public enum Space { Pixels, Normalized }

	#endregion // Types


	#region Initiation


	Plot()
	{
		_allRenderers = new List<PRenderer>();
		_fillRenderers = new List<FillPRenderer>();
		_matrixStack = new Stack<Matrix4x4>( 10 );
		_styleStack = new Stack<Style>( 10 );
	}


	static Plot Instance()
	{
		if( _p == null ) {
			_p = new Plot();
			SetAntiAliasing( true );
			SetStyle( Style.GetDefault() );
		}
		return _p;
	}


	public static void Reset()
	{
		if( _p == null ) return; // Will reset on create anyway.

		SetFillColor( defaultFillColor );
		SetStrokeColor( defaultStrokeColor );
		SetStrokeWidth( defaultStrokeWidth );
		SetStrokeAlignement( defaultStrokeAlignment );
		SetPivot( defaultPivot );
		SetAntiAliasing( defaultAntialias );
	}


	#endregion


	#region FrameModifiers


	/// <summary>
	/// Begin a DrawNowToTexture session. Call DrawXNow subsequently (for example DrawCircleNow) and don't forget to call EndDrawNowToTexture when you are done.
	/// For Space.Normalized 0,0 is center. Left, right, top and bottom is ( -aspect, aspect, -1, 1 ). For Space.Pixels 0,0 is in upper left corner.
	/// </summary>
	public static void BeginDrawNowToTexture( RenderTexture rt ){ BeginDrawNowToTexture( rt, Space.Pixels, Color.clear, false ); }
	public static void BeginDrawNowToTexture( RenderTexture rt, Space space ) { BeginDrawNowToTexture( rt, space, Color.clear, false ); }
	public static void BeginDrawNowToTexture( RenderTexture rt, Color clearColor ) { BeginDrawNowToTexture( rt, Space.Pixels, clearColor, true ); }
	public static void BeginDrawNowToTexture( RenderTexture rt, Space space, Color clearColor ){ BeginDrawNowToTexture( rt, space, clearColor, true ); }

	static void BeginDrawNowToTexture( RenderTexture rt, Space space, Color clearColor, bool clear )
	{
		Instance();

		// Setup render target and GL.
		Graphics.SetRenderTarget( rt );

		// Clear if requested.
		if( clear ) GL.Clear( true, true, clearColor );

		// Setup GL.
		GL.PushMatrix();
		GL.modelview = Matrix4x4.identity; // This is important since other scripts may have change this global matrix.
		Matrix4x4 projectionMatrix;
		if( space == Space.Pixels ) {
			projectionMatrix = Matrix4x4.Ortho( 0, rt.width, rt.height, 0, 100, -100 );
			GL.invertCulling = true; // Because top is bottom.
		} else {
			float aspect = rt.width / (float) rt.height;
			projectionMatrix = Matrix4x4.Ortho( -aspect, aspect, -1, 1, -100, 100 );
		}
		if( Camera.current ) projectionMatrix *= Camera.current.worldToCameraMatrix.inverse; // This fixes flickering (by @guycalledfrank). Unity is s switching back and forth between cameras.
		GL.LoadProjectionMatrix( projectionMatrix );

		_p._drawingToTextureNow = true;
	}

	/// <summary>
	/// End a DrawNowToTexture session
	/// </summary>
	public static void EndDrawNowToTexture()
	{
		Instance();

		GL.PopMatrix();
		GL.invertCulling = false;
		Graphics.SetRenderTarget( null );


		_p._drawingToTextureNow = false;
	}


	#endregion // FrameModifiers


	#region StyleModifiers


	/// <summary>
	/// Enable or disable pixel shader SDF based antialisaing for all subsequently drawn shapes.
	/// Note that edge alignment between shapes will not be seamless when anti-alisation is enabled.
	/// </summary>
	public static void SetAntiAliasing( bool isOn )
	{
		Instance();
		foreach( PRenderer r in  _p._allRenderers ) r.SetShapeAntialiasingFeature( isOn );
		_p._style.antialias = isOn;
	}


	/// <summary>
	/// Set the blend mode used for subsequently drawn shapes.
	/// </summary>
	public static void SetBlend( Blend blend )
	{
		Instance();
		foreach( PRenderer r in _p._allRenderers ) r.SetBlendFeature( blend );
		_p._style.blend = blend;
	}


	/// <summary>
	/// Set the layer used for subsequently drawn shapes. Does not work for DrawNow methods, just like Graphics.DrawMeshNow not regarding layers.
	/// </summary>
	public static void SetLayer( int layer )
	{
		Instance();
		_p._style.layer = layer;
	}


	/// <summary>
	/// Set the fill color to be used for subsequently drawn shapes.
	/// </summary>
	public static void SetFillColor( float brightness ) { SetFillColor( new Color( brightness, brightness, brightness ) ); }
	public static void SetFillColor( float brightness, float alpha ) { SetFillColor( new Color( brightness, brightness, brightness, alpha ) ); }
	public static void SetFillColor( float red, float green, float blue ) { SetFillColor( new Color( red, green, blue ) ); }
	public static void SetFillColor( float red, float green, float blue, float alpha ) { SetFillColor( new Color( red, green, blue, alpha ) ); }
	public static void SetFillColor( Color color, float alphaOverride ) { SetFillColor( ColorWithAlpha( color, alphaOverride ) ); }
	public static void SetFillColor( Color color )
	{
		Instance();
		_p._style.fillColor = color;
		foreach( FillPRenderer r in _p._fillRenderers ) r.isFillColorDirty = true;
	}


	/// <summary>
	/// Set no fill for subsequently drawn shapes.
	/// This will effectively set the fill color to (0,0,0,0) and forget the fill texture;
	/// </summary>
	public static void SetNoFill()
	{
		Instance();
		_p._style.fillColor = Color.clear;
		_p._style.fillTexture = null;
		foreach( FillPRenderer r in _p._fillRenderers ) r.isFillColorDirty = true;
		foreach( FillPRenderer r in _p._fillRenderers ) r.SetFillTextureFeature( null );
	}


	/// <summary>
	/// Set the stroke color to be used for subsequently drawn shapes.
	/// </summary>
	public static void SetStrokeColor( float brightness ) { SetStrokeColor( new Color( brightness, brightness, brightness ) ); }
	public static void SetStrokeColor( float brightness, float alpha ) { SetStrokeColor( new Color( brightness, brightness, brightness, alpha ) ); }
	public static void SetStrokeColor( float red, float green, float blue ) { SetStrokeColor( new Color( red, green, blue ) ); }
	public static void SetStrokeColor( float red, float green, float blue, float alpha ) { SetStrokeColor( new Color( red, green, blue, alpha ) ); }
	public static void SetStrokeColor( Color color, float alphaOverride ){ SetStrokeColor( ColorWithAlpha( color, alphaOverride ) ); }
	public static void SetStrokeColor( Color color )
	{
		Instance();
		_p._style.strokeColor = color;
		foreach( PRenderer r in _p._allRenderers ) r.isStrokeColorDirty = true;
	}


	/// <summary>
	/// Set no stroke for subsequently drawn shapes.
	/// </summary>
	public static void SetNoStroke()
	{
		Instance();
		_p._style.strokeColor = Color.clear;
		foreach( PRenderer r in _p._allRenderers ) r.isStrokeColorDirty = true;
	}


	/// <summary>
	/// Set the stroke width (thickness) to be used for subsequently drawn shapes.
	/// </summary>
	public static void SetStrokeWidth( float width ){
		Instance();
		bool strokeEnabedChange = width > 0 != _p._style.strokeWidth > 0;
		if( strokeEnabedChange ) foreach( PRenderer r in _p._allRenderers ) r.isStrokeColorDirty = true; // We need to update color when we change from no stroke to stroke.
		_p._style.strokeWidth = width;
	}


	/// <summary>
	/// Set the stroke alignment to be used for subsequently drawn shapes.
	/// </summary>
	public static void SetStrokeAlignement( StrokeAlignment alignment ){ Instance()._style.strokeAlignment = alignment; }


	/// <summary>
	/// Set the stroke corner profile to be used for subsequently drawn shapes.
	/// </summary>
	public static void SetStrokeCornerProfile( StrokeCornerProfile cornerStyle ) { Instance()._style.strokeCornerProfile = cornerStyle; }


	/// <summary>
	/// Set the point from which Circle will be drawn. Default is Pivot.Center.
	/// </summary>
	public static void SetPivot( Pivot pivot ){
		Instance();
		_p._style.pivot = pivot;
		_p._pivotPosition = GetPivotPosition( pivot );
	}


	/// <summary>
	/// Push (save) the current style to the stack.
	/// </summary>
	public static void PushStyle()
	{
		Instance();
		_p._styleStack.Push( _p._style );
	}


	/// <summary>
	/// Pop (load) the last pushed style from the stack.
	/// </summary>
	public static void PopStyle()
	{
		Instance();
		Style s = _p._styleStack.Pop();
		_p._style = s;
		_p._pivotPosition = GetPivotPosition( s.pivot );
		_p.ApplyStyleFeaturesToAllRenderer( ref s );
	}


	/// <summary>
	/// Copy and return the current style.
	/// </summary>
	public static Style GetStyle(){ return Instance()._style; }


	/// <summary>
	/// Overwrite the current style.
	/// </summary>
	public static void SetStyle( Style style ) {
		Instance();
		_p._style = style;
		_p._pivotPosition = GetPivotPosition( style.pivot );
		_p.ApplyStyleFeaturesToAllRenderer( ref style );
	}


	/// <summary>
	/// Set the fill texture to be used for subsequently drawn shapes.
	/// See also SetFillTextureUVRect, SetFillTextureBlend and SetFillTextureTint.
	/// </summary>
	public static void SetFillTexture( Texture texture )
	{
		Instance();
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
		Instance();
		_p._style.fillTextureST = new Vector4( width, height, x, y );
	}


	/// <summary>
	/// Set the texture blend mode to be used for subsequently drawn shapes that has a fill texture.
	/// </summary>
	public static void SetFillTextureBlend( FillTextureBlend blend )
	{
		Instance();
		foreach( FillPRenderer r in _p._fillRenderers ) r.SetFillTextureBlendFeature( blend );
		_p._style.fillTextureBlend = blend;
	}


	/// <summary>
	/// Set the texture tint to be used for subsequently drawn shapes that has a fill texture.
	/// </summary>
	public static void SetFillTextureTint( float brightness ) { SetFillTextureTint( new Color( brightness, brightness, brightness ) ); }
	public static void SetFillTextureTint( float brightness, float alpha ) { SetFillTextureTint( new Color( brightness, brightness, brightness, alpha ) ); }
	public static void SetFillTextureTint( float red, float green, float blue ) { SetFillTextureTint( new Color( red, green, blue ) ); }
	public static void SetFillTextureTint( float red, float green, float blue, float alpha ) { SetFillTextureTint( new Color( red, green, blue, alpha ) ); }
	public static void SetFillTextureTint( Color color, float alphaOverride ) { SetFillTextureTint( ColorWithAlpha( color, alphaOverride ) ); }
	public static void SetFillTextureTint( Color tint )
	{
		Instance();
		_p._style.fillTextureTint = tint;
	}



	void ApplyStyleFeaturesToAllRenderer( ref Style s )
	{
		foreach( PRenderer r in _p._allRenderers ) {
			r.SetShapeAntialiasingFeature( s.antialias );
			r.SetBlendFeature( s.blend );
		}
		foreach( FillPRenderer r in _p._fillRenderers ) {
			r.SetFillTextureFeature( s.fillTexture );
			r.SetFillTextureBlendFeature( s.fillTextureBlend );
		}
	}

	#endregion // StyleModifiers


	#region TransformModifiers


	/// <summary>
	/// Push (save) the current canvas transformation matrix to the stack.
	/// </summary>
	public static void PushCanvas(){
		Instance();
		_p._matrixStack.Push( _p._matrix );
	}


	/// <summary>
	/// Pop (load) the last pushed canvas transformation matrix from the stack.
	/// </summary>
	public static void PopCanvas()
	{
		Instance();
		_p._matrix = _p._matrixStack.Pop();
	}


	/// <summary>
	/// Get the current canvas transformation matrix.
	/// </summary>
	public static Matrix4x4 GetCanvas(){ return Instance()._matrix; }


	/// <summary>
	/// Overwrite the current canvas transformation matrix.
	/// </summary>
	public static void SetCanvas( Matrix4x4 matrix ){ Instance()._matrix = matrix; }


	/// <summary>
	/// Overwrite the current canvas transformation matrix with a transform (localToWorldMatrix).
	/// </summary>
	public static void SetCanvas( Transform transform ) { Instance()._matrix = transform.localToWorldMatrix; }


	/// <summary>
	/// Translate the current canvas transformation matrix.
	/// </summary>
	public static void TranslateCanvas( float x, float y ){ Instance()._matrix.Translate3x4( x, y ); }
	public static void TranslateCanvas( float x, float y, float z ){ Instance()._matrix.Translate3x4( x, y, z ); }
	public static void TranslateCanvas( Vector2 translation ) { Instance()._matrix.Translate3x4( translation.x, translation.y ); }
	public static void TranslateCanvas( Vector3 translation ) { Instance()._matrix.Translate3x4( translation.x, translation.y, translation.z ); }


	/// <summary>
	/// Rotate the current canvas transformation matrix by angle (in degrees).
	/// </summary>
	public static void RotateCanvas( float angleZ ){ Instance()._matrix.Rotate3x4( angleZ ); }
	public static void RotateCanvas( float angleX, float angleY, float angleZ ) { Instance()._matrix *= Matrix4x4.Rotate( Quaternion.Euler( angleX, angleY, angleZ ) ); }
	public static void RotateCanvas( Quaternion rotation ){ Instance()._matrix *= Matrix4x4.Rotate( rotation ); }


	/// <summary>
	/// Scale the current canvas transformation matrix.
	/// </summary>
	public static void ScaleCanvas( float scaleXYZ ){ Instance()._matrix.Scale3x4( scaleXYZ ); }
	public static void ScaleCanvas( float scaleX, float scaleY ) { Instance()._matrix.Scale3x4( scaleX, scaleY ); }
	public static void ScaleCanvas( float scaleX, float scaleY, float scaleZ ) { Instance()._matrix.Scale3x4( scaleX, scaleY, scaleZ ); }
	public static void ScaleCanvas( Vector2 scale ) { Instance()._matrix.Scale3x4( scale.x, scale.y ); }
	public static void ScaleCanvas( Vector3 scale ){ Instance()._matrix.Scale3x4( scale.x, scale.y, scale.z ); }

	

	#endregion // TransformModifiers


	#region Draw methods


	/// <summary>
	/// Draw a circle using Graphics.DrawMesh. This supports Unity's instancing, culling, and sorting.
	/// </summary>
	public static void DrawCircle( float x, float y, float diameter ){ Instance().DrawRingInternal( x, y, -_p._style.strokeWidth-diameter, diameter ); }
	public static void DrawCircle( Vector2 position, float diameter ){ Instance().DrawRingInternal( position.x, position.y, -_p._style.strokeWidth - diameter, diameter ); }


	/// <summary>
	/// Draw a circle immediately using Graphics.DrawMeshNow. Call this from OnPostRender or after calling BeginDrawNowToTexture.
	/// </summary>
	public static void DrawCircleNow( float x, float y, float diameter ) { Instance().DrawRingInternal( x, y, -_p._style.strokeWidth - diameter, diameter, true ); }
	public static void DrawCircleNow( Vector2 position, float diameter ) { Instance().DrawRingInternal( position.x, position.y, -_p._style.strokeWidth - diameter, diameter, true ); }


	/// <summary>
	/// Draw a ring using Graphics.DrawMesh. This supports Unity's instancing, culling, and sorting.
	/// </summary>
	public static void DrawRing( float x, float y, float innerDiameter, float OuterDiameter ) { Instance().DrawRingInternal( x, y, innerDiameter, OuterDiameter ); }
	public static void DrawRing( Vector2 position, float innerDiameter, float OuterDiameter ){ Instance().DrawRingInternal( position.x, position.y, innerDiameter, OuterDiameter ); }

	/// <summary>
	/// Draw a ring immediately using Graphics.DrawMeshNow. Call this from OnPostRender or after calling BeginDrawNowToTexture.
	/// </summary>
	public static void DrawRingNow( float x, float y, float innerDiameter, float OuterDiameter ) { Instance().DrawRingInternal( x, y, innerDiameter, OuterDiameter, true ); }
	public static void DrawRingNow( Vector2 position, float innerDiameter, float OuterDiameter ) { Instance().DrawRingInternal( position.x, position.y, innerDiameter, OuterDiameter, true ); }

	void DrawRingInternal( float x, float y, float innerDiameter, float outerDiameter, bool drawNow = false )
	{
		if( !_style.fillOrStrokeEnabled ) return;

		if( _drawingToTextureNow && !drawNow ) DebugLogDrawToTextureNowWarning();

		if( _ringRenderer == null ) {
			_ringRenderer = new RingPRenderer( _style.antialias, _style.blend, _style.fillTexture, _style.fillTextureBlend );
			_allRenderers.Add( _ringRenderer );
			_fillRenderers.Add( _ringRenderer );
		}

		_ringRenderer.Render(
			x, y, innerDiameter, outerDiameter,
			drawNow, _matrix, ref _style, ref _pivotPosition
		);
	}


	/// <summary>
	/// Draw a pie using Graphics.DrawMesh. This supports Unity's instancing, culling, and sorting.
	/// </summary>
	public static void DrawPie( float x, float y, float diameter, float angleBegin, float angleEnd, float cutOff = 0, float roundness = 0 ) { Instance().DrawArcInternal( x, y, -_p._style.strokeWidth-diameter, diameter, angleBegin, angleEnd, cutOff, roundness ); }
	public static void DrawPie( Vector2 position, float diameter, float angleBegin, float angleEnd, float cutOff = 0, float roundness = 0 ){ Instance().DrawArcInternal( position.x, position.y, -_p._style.strokeWidth-diameter, diameter, angleBegin, angleEnd, cutOff, roundness ); }


	/// <summary>
	/// Draw a pie immediately using Graphics.DrawMeshNow. Call this from OnPostRender or after calling BeginDrawNowToTexture.
	/// </summary>
	public static void DrawPieNow( float x, float y, float diameter, float angleBegin, float angleEnd, float cutOff = 0, float roundness = 0 ) { Instance().DrawArcInternal( x, y, -_p._style.strokeWidth-diameter, diameter, angleBegin, angleEnd, cutOff, roundness, false, false, true ); }
	public static void DrawPieNow( Vector2 position, float diameter, float angleBegin, float angleEnd, float cutOff = 0, float roundness = 0 ){ Instance().DrawArcInternal( position.x, position.y, -_p._style.strokeWidth-diameter, diameter, angleBegin, angleEnd, cutOff, roundness, false, false, true ); }


	/// <summary>
	/// Draw an arc using Graphics.DrawMesh. This supports Unity's instancing, culling, and sorting. Angles in degrees. AngleBegin must be smaller than AngleEnd.
	/// </summary>
	public static void DrawArc( float x, float y, float innerDiameter, float outerDiameter, float beginAngle, float endAngle, float cutOff = 0, float roundness = 0, bool useGeometricRoundness = false, bool constrainAngleSpanToRoundness = false ) { Instance().DrawArcInternal( x, y, innerDiameter, outerDiameter, beginAngle, endAngle, cutOff, roundness, useGeometricRoundness, constrainAngleSpanToRoundness ); }
	/// <summary>
	/// Draw an arc using Graphics.DrawMesh. This supports Unity's instancing, culling, and sorting. Angles in degrees. AngleBegin must be smaller than AngleEnd.
	/// </summary>
	public static void DrawArc( Vector2 position, float innerDiameter, float outerDiameter, float beginAngle, float endAngle, float cutOff = 0, float roundness = 0, bool useGeometricRoundness = false, bool constrainAngleSpanToRoundness = false ) { Instance().DrawArcInternal( position.x, position.y, innerDiameter, outerDiameter, beginAngle, endAngle, cutOff, roundness, useGeometricRoundness, constrainAngleSpanToRoundness ); }


	/// <summary>
	/// Draw a pie immediately using Graphics.DrawMeshNow. Call this from OnPostRender or after calling BeginDrawNowToTexture. Angles in degrees. AngleBegin must be smaller than AngleEnd.
	/// </summary>
	public static void DrawArcNow( float x, float y, float innerDiameter, float outerDiameter, float beginAngle, float endAngle, float cutOff = 0, float roundness = 0, bool useGeometricRoundness = false, bool constrainAngleSpanToRoundness = false ) { Instance().DrawArcInternal( x, y, innerDiameter, outerDiameter, beginAngle, endAngle, cutOff, roundness, useGeometricRoundness, constrainAngleSpanToRoundness, true ); }
	/// <summary>
	/// Draw a pie immediately using Graphics.DrawMeshNow. Call this from OnPostRender or after calling BeginDrawNowToTexture. Angles in degrees. AngleBegin must be smaller than AngleEnd.
	/// </summary>
	public static void DrawArcNow( Vector2 position, float innerDiameter, float outerDiameter, float beginAngle, float endAngle, float cutOff = 0, float roundness = 0, bool useGeometricRoundness = false, bool constrainAngleSpanToRoundness = false ) { Instance().DrawArcInternal( position.x, position.y, innerDiameter, outerDiameter, beginAngle, endAngle, cutOff, roundness, useGeometricRoundness, constrainAngleSpanToRoundness, true ); }


	void DrawArcInternal( float x, float y, float innerDiameter, float outerDiameter, float beginAngle, float endAngle, float cutOff, float roundness, bool useGeometricRoundness = false, bool constrainAngleSpanToRoundness = false, bool drawNow = false )
	{
		if( !_style.fillOrStrokeEnabled || ( beginAngle > endAngle && ( !_style.strokeEnabled || _style.strokeAlignment == StrokeAlignment.Inside ) ) ) return;

		if( _drawingToTextureNow && !drawNow ) DebugLogDrawToTextureNowWarning();

		if( _arcRenderer == null ) {
			_arcRenderer = new ArcPRenderer( _style.antialias, _style.blend, _style.fillTexture, _style.fillTextureBlend );
			_allRenderers.Add( _arcRenderer );
			_fillRenderers.Add( _arcRenderer );
		}

		_arcRenderer.Render(
			x, y, innerDiameter, outerDiameter, beginAngle, endAngle, cutOff, roundness, useGeometricRoundness, constrainAngleSpanToRoundness, 
			drawNow, _matrix, ref _style, ref _pivotPosition
		);
	}


	/// <summary>
	/// Draw a rectangle using Graphics.DrawMesh. This supports Unity's instancing, culling, and sorting.
	/// </summary>
	public static void DrawRect( float x, float y, float width, float height ){ Instance().DrawRectInternal( x, y, width, height, 0, 0, 0, 0 ); }
	public static void DrawRect( float x, float y, float width, float height, float roundness ){ Instance().DrawRectInternal( x, y, width, height, roundness, roundness, roundness, roundness ); }
	public static void DrawRect( float x, float y, float width, float height, float lowerLeftRoundness, float upperLeftRoundness, float upperRightRoundness, float lowerRightRoundness ) { Instance().DrawRectInternal( x, y, width, height, lowerLeftRoundness, upperLeftRoundness, upperRightRoundness, lowerRightRoundness ); }
	public static void DrawRect( Vector2 position, float width, float height, float lowerLeftRoundness, float upperLeftRoundness, float upperRightRoundness, float lowerRightRoundness ){ Instance().DrawRectInternal( position.x, position.y, width, height, lowerLeftRoundness, upperLeftRoundness, upperRightRoundness, lowerRightRoundness ); }
	public static void DrawRect( Vector2 position, float width, float height, float roundness = 0 ) { Instance().DrawRectInternal( position.x, position.y, width, height, roundness, roundness, roundness, roundness ); }


	/// <summary>
	/// Draw a rectangle immediately using Graphics.DrawMeshNow. Call this from OnPostRender or after calling BeginDrawNowToTexture.
	/// </summary>
	public static void DrawRectNow( float x, float y, float width, float height ) { Instance().DrawRectInternal( x, y, width, height, 0, 0, 0, 0, true ); } 
	public static void DrawRectNow( float x, float y, float width, float height, float roundness ) { Instance().DrawRectInternal( x, y, width, height, roundness, roundness, roundness, roundness, true ); }
	public static void DrawRectNow( float x, float y, float width, float height, float lowerLeftRoundness, float upperLeftRoundness, float upperRightRoundness, float lowerRightRoundness ) { Instance().DrawRectInternal( x, y, width, height, lowerLeftRoundness, upperLeftRoundness, upperRightRoundness, lowerRightRoundness, true ); }
	public static void DrawRectNow( Vector2 position, float width, float height, float lowerLeftRoundness, float upperLeftRoundness, float upperRightRoundness, float lowerRightRoundness ) { Instance().DrawRectInternal( position.x, position.y, width, height, lowerLeftRoundness, upperLeftRoundness, upperRightRoundness, lowerRightRoundness, true ); }
	public static void DrawRectNow( Vector2 position, float width, float height, float roundness = 0 ) { Instance().DrawRectInternal( position.x, position.y, width, height, roundness, roundness, roundness, roundness, true ); }


	/// <summary>
	/// Draw an arc using Graphics.DrawMesh. This supports Unity's instancing, culling, and sorting.
	/// </summary>
	public static void DrawSquare( float x, float y, float size ){ Instance().DrawRectInternal( x, y, size, size, 0, 0, 0, 0 ); }
	public static void DrawSquare( float x, float y, float size, float roundness ){ Instance().DrawRectInternal( x, y, size, size, roundness, roundness, roundness, roundness ); }
	public static void DrawSquare( float x, float y, float size, float lowerLeftRoundness, float upperLeftRoundness, float upperRightRoundness, float lowerRightRoundness ){ Instance().DrawRectInternal( x, y, size, size, lowerLeftRoundness, upperLeftRoundness, upperRightRoundness, lowerRightRoundness ); }
	public static void DrawSquare( Vector2 position, float size, float lowerLeftRoundness, float upperLeftRoundness, float upperRightRoundness, float lowerRightRoundness ){ Instance().DrawRectInternal( position.x, position.y, size, size, lowerLeftRoundness, upperLeftRoundness, upperRightRoundness, lowerRightRoundness ); }
	public static void DrawSquare( Vector2 position, float size, float roundness = 0 ){ Instance().DrawRectInternal( position.x, position.y, size, size, roundness, roundness, roundness, roundness ); }


	/// <summary>
	/// Draw a square immediately using Graphics.DrawMeshNow. Call this from OnPostRender or after calling BeginDrawNowToTexture.
	/// </summary>
	public static void DrawSquareNow( float x, float y, float size ) { Instance().DrawRectInternal( x, y, size, size, 0, 0, 0, 0, true ); }
	public static void DrawSquareNow( float x, float y, float size, float roundness ) { Instance().DrawRectInternal( x, y, size, size, roundness, roundness, roundness, roundness, true ); }
	public static void DrawSquareNow( float x, float y, float size, float lowerLeftRoundness, float upperLeftRoundness, float upperRightRoundness, float lowerRightRoundness ) { Instance().DrawRectInternal( x, y, size, size, lowerLeftRoundness, upperLeftRoundness, upperRightRoundness, lowerRightRoundness, true ); }
	public static void DrawSquareNow( Vector2 position, float size, float lowerLeftRoundness, float upperLeftRoundness, float upperRightRoundness, float lowerRightRoundness ) { Instance().DrawRectInternal( position.x, position.y, size, size, lowerLeftRoundness, upperLeftRoundness, upperRightRoundness, lowerRightRoundness, true ); }
	public static void DrawSquareNow( Vector2 position, float size, float roundness = 0 ) { Instance().DrawRectInternal( position.x, position.y, size, size, roundness, roundness, roundness, roundness, true ); }


	void DrawRectInternal( float x, float y, float width, float height, float lowerLeftRoundness, float upperLeftRoundness, float upperRightRoundness, float lowerRightRoundness, bool drawNow = false )
	{
		if( !_style.fillOrStrokeEnabled ) return;

		if( _drawingToTextureNow && !drawNow ) DebugLogDrawToTextureNowWarning();

		if( _rectRenderer == null ) {
			_rectRenderer = new RectPRenderer( _style.antialias, _style.blend, _style.fillTexture, _style.fillTextureBlend );
			_allRenderers.Add( _rectRenderer );
			_fillRenderers.Add( _rectRenderer );
		}

		_rectRenderer.Render(
			x, y, width, height, lowerLeftRoundness, upperLeftRoundness, upperRightRoundness, lowerRightRoundness,
			drawNow, _matrix, ref _style, ref _pivotPosition
		);
	}


	/// <summary>
	/// Draw a line using Graphics.DrawMesh. This supports Unity's instancing, culling, and sorting.
	/// </summary>
	public static void DrawLine( float ax, float ay, float bx, float by ){ Instance().DrawLineInternal( ax, ay, bx, by, StrokeCap.Round, StrokeCap.Round ); }
	public static void DrawLine( float ax, float ay, float bx, float by, StrokeCap caps ){ Instance().DrawLineInternal( ax, ay, bx, by, caps, caps ); }
	public static void DrawLine( float ax, float ay, float bx, float by, StrokeCap beginCap, StrokeCap endCap ) { Instance().DrawLineInternal( ax, ay, bx, by, beginCap, endCap ); }
	public static void DrawLine( Vector2 positionA, Vector2 positionB, StrokeCap beginCap, StrokeCap endCap ){ Instance().DrawLineInternal( positionA.x, positionA.y, positionB.x, positionB.y, beginCap, endCap ); }
	public static void DrawLine( Vector2 positionA, Vector2 positionB, StrokeCap caps ){ Instance().DrawLineInternal( positionA.x, positionA.y, positionB.x, positionB.y, caps, caps ); }
	public static void DrawLine( Vector2 positionA, Vector2 positionB ){ Instance().DrawLineInternal( positionA.x, positionA.y, positionB.x, positionB.y, StrokeCap.Round, StrokeCap.Round ); }


	/// <summary>
	/// Draw a line immediately using Graphics.DrawMeshNow. Call this from OnPostRender or after calling BeginDrawNowToTexture.
	/// </summary>
	public static void DrawLineNow( float ax, float ay, float bx, float by ) { Instance().DrawLineInternal( ax, ay, bx, by, StrokeCap.Round, StrokeCap.Round, true ); }
	public static void DrawLineNow( float ax, float ay, float bx, float by, StrokeCap caps ) { Instance().DrawLineInternal( ax, ay, bx, by, caps, caps, true ); }
	public static void DrawLineNow( float ax, float ay, float bx, float by, StrokeCap beginCap, StrokeCap endCap ) { Instance().DrawLineInternal( ax, ay, bx, by, beginCap, endCap, true ); }
	public static void DrawLineNow( Vector2 positionA, Vector2 positionB, StrokeCap beginCap, StrokeCap endCap ) { Instance().DrawLineInternal( positionA.x, positionA.y, positionB.x, positionB.y, beginCap, endCap, true ); }
	public static void DrawLineNow( Vector2 positionA, Vector2 positionB, StrokeCap caps ) { Instance().DrawLineInternal( positionA.x, positionA.y, positionB.x, positionB.y, caps, caps, true ); }
	public static void DrawLineNow( Vector2 positionA, Vector2 positionB ) { Instance().DrawLineInternal( positionA.x, positionA.y, positionB.x, positionB.y, StrokeCap.Round, StrokeCap.Round, true ); }


	void DrawLineInternal( float ax, float ay, float bx, float by, StrokeCap beginCap, StrokeCap endCap, bool drawNow = false )
	{
		if( !_style.strokeEnabled ) return;

		if( _drawingToTextureNow && !drawNow ) DebugLogDrawToTextureNowWarning();

		if( _lineRenderer == null ) {
			_lineRenderer = new LinePRenderer( _style.antialias, _style.blend );
			_allRenderers.Add( _lineRenderer );
		}

		_lineRenderer.Render(
			ax, ay, bx, by, beginCap, endCap,
			drawNow, _matrix, ref _style
		);
	}


	/// <summary>
	/// Draw a polygon using Graphics.DrawMesh. This supports Unity's instancing, culling, and sorting.
	/// </summary>
	public static void DrawPolygon( Polygon polygon ) { Instance().DrawPolygonInternal( polygon ); }


	/// <summary>
	/// Draw a polygon immediately using Graphics.DrawMeshNow. Call this from OnPostRender or after calling BeginDrawNowToTexture.
	/// </summary>
	public static void DrawPolygonNow( Polygon polygon ) { Instance().DrawPolygonInternal( polygon, true ); }


	void DrawPolygonInternal( Polygon polygon, bool drawNow = false )
	{
		if( polygon == null ) {
			Debug.LogWarning( logPrepend + "DrawPolygon failed. The polygon is null.\n" );
			return;
		}

		if( _drawingToTextureNow && !drawNow ) DebugLogDrawToTextureNowWarning();

		if( !_style.fillOrStrokeEnabled ) return;

		if( _polygonRenderer == null ) {
			_polygonRenderer = new PolygonPRenderer( _style.antialias, _style.blend, _style.fillTexture, _style.fillTextureBlend );
			_allRenderers.Add( _polygonRenderer );
			_fillRenderers.Add( _polygonRenderer );
		}

		_polygonRenderer.Render( polygon, drawNow, ref _matrix, ref _style );
	}


	/// <summary>
	/// Draw a polygon using Graphics.DrawMesh. This supports Unity's instancing, culling, and sorting.
	/// </summary>
	public static void DrawPolyline( Polyline polyline, StrokeCap beginCap, StrokeCap endCap ) { Instance().DrawPolylineInternal( polyline, beginCap, endCap ); }
	public static void DrawPolyline( Polyline polyline, StrokeCap caps = StrokeCap.Round ){ Instance().DrawPolylineInternal( polyline, caps, caps );}


	/// <summary>
	/// Draw a polygon immediately using Graphics.DrawMeshNow. Call this from OnPostRender or after calling BeginDrawNowToTexture.
	/// </summary>
	public static void DrawPolylineNow( Polyline polyline, StrokeCap beginCap, StrokeCap endCap ) { Instance().DrawPolylineInternal( polyline, beginCap, endCap, true ); }
	public static void DrawPOlylineNow( Polyline polyline, StrokeCap caps = StrokeCap.Round ) { Instance().DrawPolylineInternal( polyline, caps, caps, true ); }


	void DrawPolylineInternal( Polyline polyline, StrokeCap beginCap, StrokeCap endCap, bool drawNow = false )
	{
		if( polyline == null ) {
			Debug.LogWarning( logPrepend + "DrawPolyline failed. The polyline is null.\n" );
			return;
		}

		if( _drawingToTextureNow && !drawNow ) DebugLogDrawToTextureNowWarning();

		if( !_style.strokeEnabled ) return;

		if( _polylineRenderer == null ) {
			_polylineRenderer = new PolylinePRenderer( _style.antialias, _style.blend );
			_allRenderers.Add( _polylineRenderer );
		}

		_polylineRenderer.Render( polyline, beginCap, endCap, drawNow, ref _matrix, ref _style );
	}


	#endregion // Shapes


	static Vector2 GetPivotPosition( Pivot pivot )
	{
		switch( pivot )
		{
			case Pivot.Center:		return new Vector2(  0,  0  );
			case Pivot.TopLeft:		return new Vector2( -1,  1 );
			case Pivot.Top:			return new Vector2(  0,  1 );
			case Pivot.TopRight:	return new Vector2(  1,  1 );
			case Pivot.Right:		return new Vector2(  1,  0 );
			case Pivot.BottomRight: return new Vector2(  1, -1 );
			case Pivot.Bottom:		return new Vector2(  0, -1 );
			case Pivot.BottomLeft:	return new Vector2( -1, -1 );
			case Pivot.Left:		return new Vector2( -1,  0 );
		}
		return Vector2.zero;
	}

	
	static Color ColorWithAlpha( Color color, float alpha )
	{
		return new Color( color.r, color.g, color.b, alpha );
	}


	void DebugLogDrawToTextureNowWarning()
	{
		Debug.LogWarning( logPrepend + "You have called a Draw() method after calling BeginDrawNowToTexture(). Perhaps you meant to call DrawNow() ?\n" );
	}
}