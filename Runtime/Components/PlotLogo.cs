/*
	Copyright © Carl Emil Carlsen 2024
	http://cec.dk
*/

using UnityEngine;
using static Plot;

[ExecuteInEditMode]
public class PlotLogo : MonoBehaviour
{
	public float scale = 1f;
	public float baseHeight = 1f;
	public float verticalExtension = 0.5f;
	public float thickness = 0.05f;
	public float kerning = 0.5f;
	[Range(0f,1f)] public float alpha = 1f;


	void Update()
	{
		const float tCurveFactor = 0.3f;

		PushCanvasAndStyle();
		SetCanvas( transform );
		ScaleCanvas( scale );

		SetStrokeColor( Color.white, alpha );
		SetStrokeWidth( thickness );
		SetPivot( Pivot.BottomLeft );
		SetStrokeAlignement( StrokeAlignment.Edge );

		float x = thickness * 0.5f;
		float totalWidth = baseHeight * 2f + kerning * 3f + thickness * 4f + baseHeight * tCurveFactor;
		TranslateCanvas( - totalWidth / 2f, - baseHeight / 2f );
		SetNoFill();

		// p.
		DrawLine( x, baseHeight, x, -verticalExtension );
		DrawCircle( x, 0f, baseHeight );
		x += baseHeight + kerning + thickness;
		// l.
		DrawLine( x, baseHeight + verticalExtension, x, 0 );
		x += kerning + thickness;
		// o.
		DrawCircle( x, 0f, baseHeight );
		x += baseHeight + kerning + thickness;
		// t.
		DrawLine( x, baseHeight + verticalExtension, x, baseHeight * tCurveFactor );
		DrawLine( x-baseHeight*0.2f, baseHeight, x+baseHeight*0.3f, baseHeight );
		DrawArc( x, 0f, baseHeight*tCurveFactor*2, baseHeight*tCurveFactor*2, 180f, 90f, 0f, 1f, true );

		PopCanvasAndStyle();
	}
}
