/*
	Copyright © Carl Emil Carlsen 2024
	http://cec.dk
*/

using UnityEngine;
using static Plot;
using TMPro;

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class ExemplifyText : MonoBehaviour
	{
		public TMP_FontAsset font;
		public Color color = Color.white;
		public bool drawDebugRect = false;

		Text _text;

 
		void LateUpdate() 
		{ 
			// Similar to Polyline and Polygon, we have to create an object for drawing.
			if( !_text ) _text = CreateText( "Hello World" );

			PushCanvasAndStyle();
			SetCanvas( transform );

			SetTextFont( font );
			SetTextSize( 0.15f );
			SetTextAlignment( TextAlignmentOptions.Center );
			SetFillColor( color ); // Text is colored by the fill color.
			DrawText( _text, 0, 0, 1, 1, drawDebugRect );

			PopCanvasAndStyle();
		}
	}
}