/*
	Copyright © Carl Emil Carlsen 2021
	http://cec.dk
*/

using UnityEngine;
using static Plot;

[ExecuteInEditMode]
public class PlotRing : MonoBehaviour
{
	[Header("Shape")]
	public float innerDiameter = 0.7f;
	public float outerDiameter = 1.0f;

	[Header( "Fill" )]
	public bool fill = true;
	public Color fillColor = Color.white;
	public Texture fillTexture = null;
	public Rect fillTextureUVRect = new Rect( 0, 0, 1, 1 );
	public Color fillTextureTint = Color.white;
	public FillTextureBlend fillTextureBlend = FillTextureBlend.Overlay;

	[Header("Stroke")]
	public bool stroke = true;
	public float strokeWidth = 0.1f;
	public Color strokeColor = Color.black;
	public StrokeAlignment strokeAlignment = StrokeAlignment.Outside;

	[Header( "Rendering" )]
	public Blend blend = Blend.Transparent;
	public bool antiAliasing = true;
	public Pivot pivot = Pivot.Center;


	void LateUpdate()
	{
		PushCanvasAndStyle();
		SetCanvas( transform );

		SetPivot( pivot );
		SetBlend( blend );
		SetAntiAliasing( antiAliasing );
		SetLayer( gameObject.layer );

		if( fill ) {
			SetFillColor( fillColor );
			if( fillTexture ) {
				SetFillTexture( fillTexture );
				SetFillTextureUVRect( fillTextureUVRect );
				SetFillTextureTint( fillTextureTint );
				SetFillTextureBlend( fillTextureBlend );
			}
		} else {
			SetNoFill();
		}

		if( stroke ) {
			SetStrokeColor( strokeColor );
			SetStrokeWidth( strokeWidth );
			SetStrokeAlignement( strokeAlignment );
		} else {
			SetNoStroke();
		}

		DrawRing( 0, 0, innerDiameter, outerDiameter );

		PopCanvasAndStyle();
	}


	void OnValidate()
	{
		if( innerDiameter < 0 ) innerDiameter = 0;
		if( outerDiameter < 0 ) outerDiameter = 0;
		if( strokeWidth < 0 ) strokeWidth = 0;
	}
}