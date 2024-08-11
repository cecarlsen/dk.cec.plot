/*
	Copyright © Carl Emil Carlsen 2021
	http://cec.dk
*/

using System.Collections.Generic;
using UnityEngine;
using static Plot;

[ExecuteInEditMode]
public class PlotPolyline : MonoBehaviour
{
	[Header("Shape")]
	public List<Vector2> points = new List<Vector2>(){
		new Vector2( -0.3f, -0.5f ), new Vector2( 0.3f, -0.25f ), new Vector2( -0.3f, 0.25f ), new Vector2( 0.3f, 0.5f )
	};
	public StrokeCap capBegin = StrokeCap.Round;
	public StrokeCap capEnd = StrokeCap.Round;

	[Header("Stroke")]
	public bool stroke = true;
	public float strokeWidth = 0.05f;
	public Color strokeColor = Color.black;
	public StrokeCornerProfile strokeCornerProfile = StrokeCornerProfile.Round;

	[Header( "Rendering" )]
	public Blend blend = Blend.Transparent;
	public bool antiAliasing = true;
	public Pivot pivot = Pivot.Center;

	Polyline _polyline;


	void Update()
	{
		PushStyle();
		PushCanvas();
		SetCanvas( transform );

		SetPivot( pivot );
		SetBlend( blend );
		SetAntiAliasing( antiAliasing );
		SetLayer( gameObject.layer );

		if( stroke ) {
			SetStrokeColor( strokeColor );
			SetStrokeWidth( strokeWidth );
			SetStrokeCornerProfile( strokeCornerProfile );
		} else {
			SetNoStroke();
		}

		if( _polyline == null ) _polyline = new Polyline( points );
		DrawPolyline( _polyline, capBegin, capEnd );

		PopCanvas();
		PopStyle();
	}


	void OnValidate()
	{
		if( _polyline == null ) _polyline = new Polyline( points );
		else _polyline.SetPoints( points );
		if( strokeWidth < 0 ) strokeWidth = 0;
	}
}