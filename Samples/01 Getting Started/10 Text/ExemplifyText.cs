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

		Text _text;

 
		void Update() 
		{
			// Similar to Polyline and Polygon, we have to create an object for drawing.
			if( !_text ) _text = CreateText( "Hello World" );
			//text.SetContent( "Hello world" ); // BUG TESTING. Text won't vanish on code reload if we keep setting the content.

			PushStyle();
			PushCanvas();
			SetCanvas( transform );

			SetTextFont( font );
			SetTextSize( 0.1f );
			SetTextAlignment( TextAlignmentOptions.Center );
			SetFillColor( Color.blue );
			DrawText( _text, 0, 0, 1, 1, true );

			PopCanvas();
			PopStyle();
		}
	}
}