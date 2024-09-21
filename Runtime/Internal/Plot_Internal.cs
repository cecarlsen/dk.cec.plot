/*
	Copyright © Carl Emil Carlsen 2020-2024
	http://cec.dk
*/

using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using PlotInternals;
using TMPro;

public partial class Plot
{
	static Plot _p;
	static Plot P()
	{
		if( _p == null )
		{
			_p = new Plot();
			SetAntiAliasing( true );
			SetStyle( Style.GetDefault() );
		}
		return _p;
	}

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
	const float defaultTextSize = 0.1f;
	static readonly Color defaultTextColor = Color.white;
	const TextAlignmentOptions defaultTextAlignment = TextAlignmentOptions.Center;


	Plot()
	{
		_allRenderers = new List<PRenderer>();
		_fillRenderers = new List<FillPRenderer>();
		_matrixStack = new Stack<Matrix4x4>( 10 );
		_styleStack = new Stack<Style>( 10 );
	}


	static void BeginDrawNowToTexture( RenderTexture rt, Space space, Color clearColor, bool clear )
	{
		P();

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


	void DrawTextInternal( Text text, float x, float y, float fieldWidth, float fieldHeight, bool drawDebugRect, bool drawNow = false )
	{
		if( !text || !text._tmpText ) return;

		if( _drawingToTextureNow && !drawNow ) DebugLogDrawToTextureNowWarning();

		Matrix4x4 matrix = _matrix;
		matrix.Translate3x4( x, y );

		//if( applyPlotStyle ) {
			text._tmpText.color = _style.fillColor;
			text._tmpText.fontSize = _style.tmpFontSize;
			text._tmpText.alignment = _style.textAlignment;
			text._tmpText.rectTransform.localPosition = new Vector3( x, y );
			text._tmpText.rectTransform.pivot = new Vector2( ( _pivotPosition.x * 0.5f ) + 0.5f, ( _pivotPosition.y * 0.5f ) + 0.5f );
			text._tmpText.rectTransform.sizeDelta = new Vector2( fieldWidth, fieldHeight ); // TODO check the performance impact of this.
		//}
		

		if( drawNow ) {
			text._tmpText.fontSharedMaterial.SetPass( 0 );
			Graphics.DrawMeshNow( text._tmpText.mesh, matrix );
		} else {
			Graphics.DrawMesh( text._tmpText.mesh, matrix, text._tmpText.fontSharedMaterial, layer: 0 );
		}

		if( drawDebugRect ) {
			float debugSize = Mathf.Min( fieldWidth, fieldHeight ) * 0.01f;
			PushStyle();
			PushCanvas();
			TranslateCanvas( 0, 0, -0.001f );
			SetNoFillColor();
			SetStrokeAlignement( StrokeAlignment.Edge );
			SetStrokeCornerProfile( StrokeCornerProfile.Hard );
			SetStrokeColor( Color.green );
			SetStrokeWidth( debugSize );
			// Draw using the current pivot.
			if( drawNow ) DrawRectNow( x, y, fieldWidth, fieldHeight );
			else DrawRect( x, y, fieldWidth, fieldHeight );
			// Then draw pivot.
			TranslateCanvas( 0, 0, -0.001f );
			SetPivot( Pivot.Center );
			SetNoStrokeColor();
			SetFillColor( Color.red );
			DrawCircle( x, y, debugSize * 4 );
			PopCanvas();
			PopStyle();
		}
	}


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