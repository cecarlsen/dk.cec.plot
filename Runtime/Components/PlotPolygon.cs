/*
	Copyright © Carl Emil Carlsen 2021-2024
	http://cec.dk
*/

using System.Collections.Generic;
using UnityEngine;
using static Plot;

[ExecuteInEditMode]
public class PlotPolygon : MonoBehaviour
{
	[Header("Shape")]
	[Tooltip("Clockwise order")] public List<Vector2> points = new List<Vector2>(){
		new Vector2( -0.1f, 0 ), new Vector2( -0.3f, -0.5f ), new Vector2( 0.5f, 0 ), new Vector2( -0.3f, 0.5f )
	};

	[Header( "Fill" )]
	public bool fill = true;
	public Color fillColor = Color.white;

	//[Header( "Fill (not yet working)" )]
	//public Texture fillTexture = null;
	//public Rect fillTextureUVRect = new Rect( 0, 0, 1, 1 );
	//public Color fillTextureTint = Color.white;
	//public FillTextureBlend fillTextureBlend = FillTextureBlend.Overlay;

	[Header("Stroke")]
	public bool stroke = true;
	[Range(0f,1f)]public float strokeWidth = 0.05f;
	public Color strokeColor = Color.black;
	public StrokeAlignment strokeAlignment = StrokeAlignment.Outside;
	public StrokeCornerProfile strokeCornerProfile = StrokeCornerProfile.Round;

	[Header( "Rendering" )]
	public Blend blend = Blend.Transparent;
	public bool antiAliasing = true;
	public Pivot pivot = Pivot.Center;

	[Header( "Gizmos" )]
	public bool showPointOrderGizmos = false;

	Polygon _polygon;


	void Update()
	{
		if( !_polygon ) _polygon = CreatePolygon( points );

		PushStyle();
		PushCanvas();
		SetCanvas( transform );

		SetPivot( pivot );
		SetBlend( blend );
		SetAntiAliasing( antiAliasing );
		SetLayer( gameObject.layer );

		if( fill ) {
			SetFillColor( fillColor );
			//if( fillTexture ) {
			//	SetFillTexture( fillTexture );
			//	SetFillTextureUVRect( fillTextureUVRect );
			//	SetFillTextureTint( fillTextureTint );
			//	SetFillTextureBlend( fillTextureBlend );
			//}
		} else { 
			SetNoFillColor();
		}

		if( stroke ) {
			SetStrokeColor( strokeColor );
			SetStrokeWidth( strokeWidth );
			SetStrokeAlignement( strokeAlignment );
			SetStrokeCornerProfile( strokeCornerProfile );
		} else {
			SetNoStrokeColor();
		}
		DrawPolygon( _polygon );

		PopCanvas();
		PopStyle();
	}


	void OnValidate()
	{
		if( _polygon == null ) _polygon = CreatePolygon( points );
		else _polygon.SetPoints( points );
	}


	void OnDrawGizmos()
	{
#if UNITY_EDITOR
		if( _polygon != null && showPointOrderGizmos ) {
			for( int p = 0; p < _polygon.pointCount; p++ ) {
				UnityEditor.Handles.Label( transform.TransformPoint( _polygon.GetPoint( p ) ), p.ToString() );
			}
		}
#endif
	}
}