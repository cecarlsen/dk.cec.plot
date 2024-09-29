/*
	Copyright © Carl Emil Carlsen 2020-2021
	http://cec.dk
*/

using UnityEngine;
using static Plot;

[ExecuteInEditMode]
public class PlotCircle : MonoBehaviour
{
	[Header("Shape")]
	public float diameter = 1;

	[Header("Fill")]
	public bool fill = true;
	public Color fillColor = Color.white;
	public Texture fillTexture = null;
	public Rect fillTextureUVRect = new Rect( 0, 0, 1, 1 );
	public Color fillTextureTint = Color.white;
	public FillTextureBlend fillTextureBlend = FillTextureBlend.Overlay;

	[Header("Stroke")]
	public bool stroke = true;
	public float strokeWidth = 0.05f;
	public Color strokeColor = Color.black;
	public StrokeAlignment strokeAlignment = StrokeAlignment.Outside;

	[Header("Rendering")]
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

		if( stroke ) {
			SetStrokeColor( strokeColor );
			SetStrokeWidth( strokeWidth );
			SetStrokeAlignement( strokeAlignment );
		} else {
			SetNoStroke();
		}

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

		DrawCircle( 0, 0, diameter );

		PopCanvasAndStyle();
	}


	void OnValidate()
	{
		if( diameter < 0 ) diameter = 0;
		if( strokeWidth < 0 ) strokeWidth = 0;
	}
}