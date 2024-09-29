/*
	Copyright © Carl Emil Carlsen 2020-2024
	http://cec.dk
*/

using UnityEngine;

namespace PlotExamples
{
	[ExecuteInEditMode] 								// Make this script work in Edit Mode.
	public class ExamplifyCircleStaticPlot : MonoBehaviour
	{
		public Color color = Color.red;

		void LateUpdate() 								// Draw inside LateUpdate to play nice with Timeline and other scripts that manipulate in Update.
		{
			Plot.PushCanvasAndStyle();					// Save the current canvas matrix and style on stacks.
			Plot.SetCanvas( transform );				// Set this transform's localToWorldMatrix as our canvas.
		
			Plot.SetNoStroke();							// Request no stroke for subsequently drawn shapes.
			Plot.SetFillColor( color );					// Request a fill color for subsequently drawn shapes.
			Plot.DrawCircle( x: 0, y: 0, diameter: 1 );	// Draw a circle at canvas center.
		
			Plot.PopCanvas();							// Load previously saved canvas matrix and styæe from the stacks.
														// We do this to isolate transformations and style changes made in this script.

		}
	}
}