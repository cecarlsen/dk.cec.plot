/*
	Copyright © Carl Emil Carlsen 2024
	http://cec.dk
*/

using TMPro;
using UnityEngine;
using static Plot;

namespace PlotInternals
{
	public class TextRenderer
	{

		public void Render
		(
			Text text, float x, float y, float fieldWidth, float fieldHeight, Vector2 pivotPosition, 
			ref Matrix4x4 matrix, ref Style style, // Note that matrix and style are passed by reference for performance reasons, they are not changed.
			bool drawDebugRect, bool drawNow
		){
			var tmp = text.GetTextMeshPro( drawNow );

			Matrix4x4 matrixCopy = matrix;
			matrixCopy.Translate3x4( x, y );

			if( style.textFont ){
				if( tmp.font != style.textFont ) tmp.font = style.textFont;
			} else {
				if( !tmp.font ) tmp.font = TMP_Settings.defaultFontAsset;
			}
			tmp.color = style.fillColor;
			tmp.fontSize = style.textSize;
			tmp.alignment = style.textAlignment;
			tmp.rectTransform.localPosition = new Vector3( x, y );
			tmp.rectTransform.pivot = new Vector2( ( pivotPosition.x * 0.5f ) + 0.5f, ( pivotPosition.y * 0.5f ) + 0.5f );
			tmp.rectTransform.sizeDelta = new Vector2( fieldWidth, fieldHeight ); // TODO check the performance impact of this.

		if( drawNow ) {
			tmp.fontSharedMaterial.SetPass( 0 );
			Graphics.DrawMeshNow( tmp.mesh, matrixCopy );
		} else {
			Graphics.DrawMesh( tmp.mesh, matrixCopy, tmp.fontSharedMaterial, layer: 0 );
		}

		if( drawDebugRect ) {
			float debugSize = Mathf.Min( fieldWidth, fieldHeight ) * 0.01f;
			PushStyle();
			PushCanvas();
			TranslateCanvas( 0, 0, -0.001f );
			SetNoFill();
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
			SetNoStroke();
			SetFillColor( Color.red );
			DrawCircle( x, y, debugSize * 4 );
			PopCanvas();
			PopStyle();
		}
		}
	}
}