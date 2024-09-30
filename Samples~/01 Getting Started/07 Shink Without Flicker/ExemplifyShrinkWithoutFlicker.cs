/*
	Copyright © Carl Emil Carlsen 2020-2021
	http://cec.dk
*/

using UnityEngine;
using static Plot;

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class ExemplifyShrinkWithoutFlicker : MonoBehaviour
	{
		public int objectCount = 10;
		[Range( 0.005f, 1.0f )] public float sizeMax = 1;
		public float padding = 0.1f;
		[Range(0,360)] public float rotation = 0.0f;
		[Range(0,1)] public float roundness = 0;
		public bool antialias = true;
		public bool scaledCanvasInsteadOfShapeSize = false;
		public bool stroke = false;
		public bool fill = true;
		public bool useGeometricArcRoundness = false;
		public StrokeCap caps = StrokeCap.Round;
		public StrokeAlignment strokeAlignment = StrokeAlignment.Outside;

		Polygon _polygon;
		Polyline _polyline;


		void LateUpdate()
		{
			if( !_polygon ) _polygon = CreatePolygon( 4 );
			if( !_polyline ) _polyline = CreatePolyline( 2 );

			PushCanvasAndStyle();
			SetAntiAliasing( antialias );
			SetStrokeAlignement( strokeAlignment );

			float offsetY = sizeMax * 2.0f;
			float lineExtents = sizeMax * 0.8f;

			float x = 0;
			for( int i = 0; i < objectCount; i++ )
			{
				float thickness = Mathf.Pow( ( 1 - ( i / ((float) objectCount ) ) ), 4.0f ) * sizeMax;

				x += thickness * 0.5f;
				float y = 0;
				float scaledLineExtents = lineExtents / thickness;

				SetStrokeWidth( scaledCanvasInsteadOfShapeSize ? 1 : thickness );
				SetStrokeColor( Color.gray );

				PushCanvas();
				TranslateCanvas( x, 0 );
				RotateCanvas( rotation );
				if( scaledCanvasInsteadOfShapeSize ) ScaleCanvas( thickness );
				DrawLine( 0, scaledCanvasInsteadOfShapeSize ? -scaledLineExtents : -lineExtents, 0, scaledCanvasInsteadOfShapeSize ? scaledLineExtents : lineExtents, caps );
				PopCanvas();
				y -= offsetY * 0.7f;

				PushCanvas();
				TranslateCanvas( x, y -= offsetY );
				RotateCanvas( rotation );
				if( scaledCanvasInsteadOfShapeSize ) ScaleCanvas( thickness );
				_polyline.SetPointCount( 2 );
				_polyline.SetPoint( 0, 0, scaledCanvasInsteadOfShapeSize ? -scaledLineExtents : -lineExtents );
				_polyline.SetPoint( 1, 0, scaledCanvasInsteadOfShapeSize ? scaledLineExtents : lineExtents );
				DrawPolyline( _polyline, caps );
				PopCanvas();
				y -= offsetY * 0.5f;

				if( !stroke ){
					SetNoStroke();
				} else {
					SetStrokeWidth( ( scaledCanvasInsteadOfShapeSize ? 1 : thickness ) * 0.1f );
					SetStrokeColor( fill ? Color.black : Color.white );
				}
				if( fill ) SetFillColor( Color.white );
				else SetNoFill();

				PushCanvas();
				TranslateCanvas( x, y -= offsetY );
				RotateCanvas( rotation );
				if( scaledCanvasInsteadOfShapeSize ) ScaleCanvas( thickness );
				DrawCircle( 0, 0, scaledCanvasInsteadOfShapeSize ? 1 : thickness );
				PopCanvas();

				PushCanvas();
				TranslateCanvas( x, y -= offsetY );
				RotateCanvas( rotation );
				if( scaledCanvasInsteadOfShapeSize ) ScaleCanvas( thickness );
				DrawRing( 0, 0, scaledCanvasInsteadOfShapeSize ? 0.5f : thickness*0.5f, scaledCanvasInsteadOfShapeSize ? 1 : thickness );
				PopCanvas();

				PushCanvas();
				TranslateCanvas( x, y -= offsetY );
				RotateCanvas( rotation );
				if( scaledCanvasInsteadOfShapeSize ) ScaleCanvas( thickness );
				DrawPie( 0, 0, scaledCanvasInsteadOfShapeSize ? 1 : thickness, -150, 150, 0, roundness );
				PopCanvas();

				PushCanvas();
				TranslateCanvas( x, y -= offsetY );
				RotateCanvas( rotation );
				if( scaledCanvasInsteadOfShapeSize ) ScaleCanvas( thickness );
				DrawArc( 0, 0, scaledCanvasInsteadOfShapeSize ? 0.5f : thickness * 0.5f, scaledCanvasInsteadOfShapeSize ? 1 : thickness, -150, 150, 0, roundness, useGeometricArcRoundness );
				PopCanvas();

				PushCanvas();
				TranslateCanvas( x, y -= offsetY );
				RotateCanvas( rotation );
				if( scaledCanvasInsteadOfShapeSize ) ScaleCanvas( thickness );
				DrawSquare( 0, 0, scaledCanvasInsteadOfShapeSize ? 1 : thickness, roundness );
				PopCanvas();

				PushCanvas();
				TranslateCanvas( x, y -= offsetY );
				RotateCanvas( rotation );
				if( scaledCanvasInsteadOfShapeSize ) ScaleCanvas( thickness );
				_polygon.SetAsNGon( ( scaledCanvasInsteadOfShapeSize ? 1f : thickness ) * 1.2f, 5 );
				//_polygon.SetPoint( 0, scaledCanvasInsteadOfShapeSize ? -0.5f : thickness * -0.5f, scaledCanvasInsteadOfShapeSize ? -0.5f : thickness * -0.5f );
				//_polygon.SetPoint( 1, scaledCanvasInsteadOfShapeSize ? -0.5f : thickness * -0.5f, scaledCanvasInsteadOfShapeSize ?  0.5f : thickness *  0.5f );
				//_polygon.SetPoint( 2, scaledCanvasInsteadOfShapeSize ?  0.5f : thickness *  0.5f, scaledCanvasInsteadOfShapeSize ?  0.5f : thickness *  0.5f );
				//_polygon.SetPoint( 3, scaledCanvasInsteadOfShapeSize ?  0.5f : thickness *  0.5f, scaledCanvasInsteadOfShapeSize ? -0.5f : thickness * -0.5f );
				DrawPolygon( _polygon );
				PopCanvas();

				x += thickness * 0.5f + padding;
			}

			PopCanvasAndStyle();
		}
	}
}