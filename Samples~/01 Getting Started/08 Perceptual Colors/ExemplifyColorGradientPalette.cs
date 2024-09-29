/*
	Copyright © Carl Emil Carlsen 2020-2024
	http://cec.dk
*/

using UnityEngine;
using static Plot;

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class ExemplifyColorGradientPalette : MonoBehaviour
	{
		public InterpolationMethod interpolationMethod = InterpolationMethod.RGBLerp;
		public Color beginColor = Color.red;
		public Color endColor = Color.blue;
		public int stepCount = 20;
		public float gradientWidth = 5;
		public float gradientHeight = 2;

		[System.Serializable]
		public enum InterpolationMethod { RGBLerp, HSVSlerp, HSVLerp, JCHSlerp, JCHLerp }


		void LateUpdate()
		{
			PushCanvasAndStyle();
			SetCanvas( transform );

			SetAntiAliasing( false ); // We need perfect alignment between shape edges, and that is not possible when shape antialiasing is enabled and blend mode is Blend.Transparent.
			SetNoStroke();
			SetPivot( Pivot.Center );

			DrawGradient( interpolationMethod );
	
			PopCanvasAndStyle();
		}


		void DrawGradient( InterpolationMethod method )
		{
			float step = 1 / ( stepCount - 1f );
			float stepSize = gradientWidth / (float) stepCount;
			float x = stepSize * 0.5f;
			for( int i = 0; i < stepCount; i++ ) {
				float t = i * step;

				Color color = Color.black;
				switch( method )
				{
					case InterpolationMethod.RGBLerp:
						color = Color.Lerp( beginColor, endColor, t );
						break;
					case InterpolationMethod.HSVSlerp:
						{
							float beginH, beginS, beginV, endH, endS, endV;
							Color.RGBToHSV( beginColor, out beginH, out beginS, out beginV );
							Color.RGBToHSV( endColor, out endH, out endS, out endV );
							float h = Mathf.LerpAngle( beginH * 360, endH * 360, t ) / 360f;
							while( h < 0 ) h++;
							float s = Mathf.Lerp( beginS, endS, t );
							float v = Mathf.Lerp( beginV, endV, t );
							color = Color.HSVToRGB( h, s, v );
						}
						break;
					case InterpolationMethod.HSVLerp:
						{
							float beginH, beginS, beginV, endH, endS, endV;
							Color.RGBToHSV( beginColor, out beginH, out beginS, out beginV );
							Color.RGBToHSV( endColor, out endH, out endS, out endV );
							beginH *= Mathf.PI * 2;
							endH *= Mathf.PI * 2;
							Vector2 beginPoint = new Vector2( Mathf.Cos( beginH ) * beginS, Mathf.Sin( beginH ) * beginS );
							Vector2 endPoint = new Vector2( Mathf.Cos( endH ) * endS, Mathf.Sin( endH ) * endS );
							Vector2 point = Vector2.Lerp( beginPoint, endPoint, t );
							float h = Mathf.Atan2( point.y, point.x ) / ( Mathf.PI * 2 );
							while( h < 0 ) h++;
							float s = point.magnitude;
							color = Color.HSVToRGB( h, s, Mathf.Lerp( beginV, endV, t ) );
						}
						break;
					case InterpolationMethod.JCHSlerp:
						{
							JChColor beginJCh = new JChColor( beginColor );
							JChColor endJCh = new JChColor( endColor );
							color = JChColor.Slerp( beginJCh, endJCh, t );
						}
						break;
					case InterpolationMethod.JCHLerp:
						{
							JChColor beginJCh = new JChColor( beginColor );
							JChColor endJCh = new JChColor( endColor );
							color = JChColor.Lerp( beginJCh, endJCh, t );
						}
						break;
				}

				SetFillColor( color );
				if( i == 0 ) DrawRect( x, 0, stepSize, gradientHeight, 0.5f, 0.5f, 0, 0 );
				else if( i == stepCount-1 ) DrawRect( x, 0, stepSize, gradientHeight, 0, 0, 0.5f, 0.5f );
				else DrawRect( x, 0, stepSize, gradientHeight );

				x += stepSize;
			}
		}
	}

}
