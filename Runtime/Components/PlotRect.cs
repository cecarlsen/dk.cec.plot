/*
	Copyright © Carl Emil Carlsen 2020-2021
	http://cec.dk
*/

using UnityEngine;
using static Plot;

[ExecuteInEditMode]
public class PlotRect : MonoBehaviour
{
	[Header("Shape")]
	public float width = 1;
	public float height = 1;

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
	public StrokeCornerProfile strokeCornerProfile = StrokeCornerProfile.Round;

	[Header("Rendering")]
	public Blend blend = Blend.Transparent;
	public bool antiAliasing = true;
	public Pivot pivot = Pivot.Center;


	void Update()
	{
		PushStyle();
		PushCanvas();
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
			SetStrokeCornerProfile( strokeCornerProfile );
		} else {
			SetNoStroke();
		}

		DrawRect( 0, 0, width, height );

		PopCanvas();
		PopStyle();
	}


	void OnValidate()
	{
		if( width < 0 ) width = 0;
		if( height < 0 ) height = 0;
		if( strokeWidth < 0 ) strokeWidth = 0;
	}
}