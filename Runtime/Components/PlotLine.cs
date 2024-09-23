/*
	Copyright © Carl Emil Carlsen 2020-2021
	http://cec.dk
*/

using UnityEngine;
using static Plot;

[ExecuteInEditMode]
public class PlotLine : MonoBehaviour
{
	[Header("Shape")]
	public Vector2 pointA = new Vector2( 0, -0.5f );
	public Vector2 pointB = new Vector2( 0, 0.5f );
	public StrokeCap capA = StrokeCap.Round;
	public StrokeCap capB = StrokeCap.Round;

	[Header("Stroke")]
	public float strokeWidth = 0.05f;
	public Color strokeColor = Color.black;

	[Header("Rendering")]
	public Blend blend = Blend.Transparent;
	public bool antiAliasing = true;


	void Update()
	{
		PushCanvasAndStyle();
		SetCanvas( transform );

		SetBlend( blend );
		SetAntiAliasing( antiAliasing );
		SetLayer( gameObject.layer );

		SetStrokeColor( strokeColor );
		SetStrokeWidth( strokeWidth );

		DrawLine( pointA, pointB, capA, capB );

		PopCanvasAndStyle();
	}


	void OnValidate()
	{
		if( strokeWidth < 0 ) strokeWidth = 0;
	}
}