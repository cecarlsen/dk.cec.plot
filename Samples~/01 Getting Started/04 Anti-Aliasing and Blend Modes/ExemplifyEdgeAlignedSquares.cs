/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

using UnityEngine;
using static Plot;

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class ExemplifyEdgeAlignedSquares : MonoBehaviour
	{
		public Color color = Color.white;
		public bool antiAliasing = true;
		public Blend blend = Blend.Transparent;

		const float roundness = 1;
		const float size = 1;
		const float extents = size / 2f;


		void LateUpdate()
		{
			PushCanvasAndStyle();
			SetCanvas( transform );

			SetAntiAliasing( antiAliasing );
			SetBlend( blend );
			SetNoStroke();

			SetFillColor( color );
			DrawSquare( -extents, -extents, size, roundness, 0, 0, 0 );	// Lower-Left
			SetFillColor( color, 0.8f );
			DrawSquare( -extents, extents, size, 0, roundness, 0, 0 );	// Upper-Left
			SetFillColor( color, 0.6f );
			DrawSquare( extents, extents, size, 0, 0, roundness, 0 );	// Upper-Right
			SetFillColor( color, 0.4f );
			DrawSquare( extents, -extents, size, 0, 0, 0, roundness );	// Lower-Right

			PopCanvasAndStyle();
		}
	}
}