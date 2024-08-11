/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

using UnityEngine;
using static Plot;

public static class PlotDebug
{
	public static void DrawLinePoints( float ax, float ay, float bx, float by, float pointDiameter )
	{
		PushStyle();
		SetPivot( Pivot.Center );

		DrawCircle( new Vector3( ax, ay ), pointDiameter );
		DrawCircle( new Vector3( bx, by ), pointDiameter );

		PopStyle();
	}

	public static void DrawLinePoints( Vector2 positionA, Vector2 positionB, float pointDiameter )
	{
		DrawLinePoints( positionA.x, positionA.y, positionB.x, positionB.y, pointDiameter );
	}


	public static void DrawPolygonPoints( Polygon polygon, float pointDiameter )
	{
		PushStyle();
		SetPivot( Pivot.Center );

		for( int p = 0; p < polygon.pointCount; p++ ) DrawCircle( polygon.GetPoint( p ), pointDiameter );

		PopStyle();
	}


	public static void DrawPolylinePoints( Polyline polyline, float pointDiameter )
	{
		PushStyle();
		SetPivot( Pivot.Center );

		for( int p = 0; p < polyline.pointCount; p++ ) DrawCircle( polyline.GetPoint( p ), pointDiameter );

		PopStyle();
	}
}