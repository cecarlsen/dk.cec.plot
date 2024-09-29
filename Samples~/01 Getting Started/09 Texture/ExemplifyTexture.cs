/*
	Copyright © Carl Emil Carlsen 2024
	http://cec.dk
*/

using UnityEngine;
using static Plot;

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class ExemplifyTexture : MonoBehaviour
	{
		public Texture texture;
		public FillTextureBlend textureBlend = FillTextureBlend.Overlay;
		public Color fillColor = Color.black;
		public Color textureTint = Color.white;
		public Color strokeColor = Color.white;

 
		void LateUpdate() 
		{
			PushCanvasAndStyle();
			SetCanvas( transform );

			SetFillTexture( texture );
			SetFillTextureBlend( textureBlend );
			SetFillTextureTint( textureTint );
			SetFillColor( fillColor );
			SetStrokeWidth( 0.1f );
			SetStrokeColor( strokeColor );
			DrawRect( 0, 0, 1, 1, 0.2f, 0.5f, 0.2f, 0.5f );

			PopCanvasAndStyle();
		}
	}
}