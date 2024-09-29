/*
	Copyright © Carl Emil Carlsen 2020-2024
	http://cec.dk
*/

using UnityEngine;
using static Plot;

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class ExemplifyColorSpace : MonoBehaviour
	{
		public int resolution = 10;
		[Range(0,1)] public float chroma = 0.5f;
		public ColorSpace colorSpace = ColorSpace.HSV;

		[System.Serializable]
		public enum ColorSpace { HSV, JCH }


		void LateUpdate()
		{

			PushCanvasAndStyle();
			SetCanvas( transform.localToWorldMatrix );
			PlotColorSpace( colorSpace, resolution, chroma );
			PopCanvasAndStyle();
		}


		public void SetChroma( float chromaValue )
		{
			chroma = chromaValue;
		}


		static void PlotColorSpace( ColorSpace colorSpace, int resolution, float chroma )
		{
			SetNoStroke();
			float step = 1f / ( resolution - 1f );
			float offset = step * resolution * 0.5f - step * 0.5f;
			for( int ny = 0; ny < resolution; ny++ ) {
				float ty = ny * step;
				float lightness = ty;
				for( int nx = 0; nx < resolution; nx++ ) {
					float tx = nx * step;
					float hue = tx;

					Color color = Color.black;
					switch( colorSpace )
					{
						case ColorSpace.HSV: color = Color.HSVToRGB( hue, chroma, lightness ); break;
						case ColorSpace.JCH: color = new JChColor( lightness, chroma, hue ); break;
					}

					SetFillColor( color );
					DrawCircle( tx - offset, ty - offset, step );
				}
			}
		}
	}
}