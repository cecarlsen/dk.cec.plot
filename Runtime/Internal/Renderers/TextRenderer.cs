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
			Matrix4x4 matrixCopy = matrix;
			matrixCopy.Translate3x4( x, y );

			if( style.font ){
				if( text._tmpText.font != style.font ) text._tmpText.font = style.font;
			} else {
				if( !text._tmpText.font ) text._tmpText.font = TMP_Settings.defaultFontAsset;
			}
			text._tmpText.color = style.fillColor;
			text._tmpText.fontSize = style.tmpFontSize;
			text._tmpText.alignment = style.textAlignment;
			text._tmpText.rectTransform.localPosition = new Vector3( x, y );
			text._tmpText.rectTransform.pivot = new Vector2( ( pivotPosition.x * 0.5f ) + 0.5f, ( pivotPosition.y * 0.5f ) + 0.5f );
			text._tmpText.rectTransform.sizeDelta = new Vector2( fieldWidth, fieldHeight ); // TODO check the performance impact of this.

		if( drawNow ) {
			text._tmpText.fontSharedMaterial.SetPass( 0 );
			Graphics.DrawMeshNow( text._tmpText.mesh, matrixCopy );
		} else {
			Graphics.DrawMesh( text._tmpText.mesh, matrixCopy, text._tmpText.fontSharedMaterial, layer: 0 );
		}

		if( drawDebugRect ) {
			float debugSize = Mathf.Min( fieldWidth, fieldHeight ) * 0.01f;
			PushStyle();
			PushCanvas();
			TranslateCanvas( 0, 0, -0.001f );
			SetNoFillColor();
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
			SetNoStrokeColor();
			SetFillColor( Color.red );
			DrawCircle( x, y, debugSize * 4 );
			PopCanvas();
			PopStyle();
		}
		}
	}
}