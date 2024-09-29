/*
	Copyright © Carl Emil Carlsen 2021-2022
	http://cec.dk
*/

using UnityEngine;
using static Plot;

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class ExemplifyDrawToRenderTexture : MonoBehaviour
	{
		public RenderTexture renderTexture;

		void LateUpdate()
		{
			if( !renderTexture ) return;
			
			PushCanvasAndStyle();
			BeginDrawNowToRenderTexture( renderTexture, Plot.Space.Pixels );

			SetNoStroke();
			SetFillColor( Color.HSVToRGB( 0.5f * Random.value * 0.4f, 1f, 1f ) );
			DrawCircleNow( Random.value * renderTexture.width, Random.value * renderTexture.height, 100f );

			EndDrawNowToRenderTexture();
			PopCanvasAndStyle();
		}
	}
}