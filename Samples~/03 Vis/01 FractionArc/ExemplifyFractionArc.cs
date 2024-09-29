/*
	Copyright © Carl Emil Carlsen 2021-2024
	http://cec.dk
*/

using UnityEngine;
using static Plot;
using TMPro;

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class ExemplifyFractionArc : MonoBehaviour
	{
		[Header( "Data" )]
		[SerializeField,Range(0,1)] float _fraction = 0.5f;
		[SerializeField] string _headerText = "Fraction";

		[Header( "Shape" )] 
		[Range(0,1)] public float diameter = 1;
		[Range(0,1)] public float innerDiameterFactor = 0.5f;
		[Range(0,1)] public float roundness = 0;
		public bool useGeometricRoundness = false;
		public bool constrainAngleSpanToRoundness = true;
		public bool antiAliasing = true;

		[Header( "Color" )]
		public Color backgroundColor = Color.gray;
		public Color arcColor = Color.white;

		[Header( "Labels" )]
		public TMP_FontAsset headerFont;
		public float headerLabelSizeRelative = 0.3f;
		public float headerLabelWidthRelative = 3f;
		public float headerLabelOffsetYRelative = 0.05f;
		public Color headerLabelColor = Color.white;
		public TMP_FontAsset fractionFont;
		[Range(0,1)] public float fractionLabelSizeRelative = 0.3f;
		public Color fractionLabelColor = Color.white;

		[Header( "Debug" )]
		public bool debugLabelRects = false;

		bool _dataChanged = true;

		Text _headerPlotText;
		Text _fractionPlotText;

		public float fraction {
			get { return _fraction; }
			set {
				_fraction = value < 0f ? 0f : value > 1f ? 1f : value;
				_dataChanged = true;
			}
		}

		public string headerText {
			get { return _headerText; }
			set {
				_headerText = value;
				_dataChanged = true;
			}
		}


		void LateUpdate()
		{
			// Only update text when data change. Text changes always generate garbage.
			if( _dataChanged ) {
				// Ensure that we have the Text objects we need.
				if( !_fractionPlotText ) _fractionPlotText = CreateText();
				if( !_headerPlotText ) _headerPlotText = CreateText();
				// Update text.
				_headerPlotText.SetContent( "<b>" + _headerText + "</b>" );
				_fractionPlotText.SetContent( Mathf.RoundToInt( _fraction * 100 ).ToString() + "%" );
				_dataChanged = false;
			}

			// Save current plot canvas transform and style sate.
			PushCanvasAndStyle();

			// Draw relative to this transform.
			SetCanvas( transform );

			// Setup shared drawing style.
			SetNoStroke(); 
			SetAntiAliasing( antiAliasing );

			// Compute proportions.
			float innerDiameter = diameter * innerDiameterFactor;
			float headerLabelSize = diameter * headerLabelSizeRelative;
			float headerLabelHeight = headerLabelSize * 3;

			// Draw elements.
			SetFillColor( backgroundColor );
			DrawRing( Vector2.zero, innerDiameter, diameter );
			TranslateCanvas( 0, 0, -0.001f ); // Make sure next element is rendered in front (elements are sorted by Unity just like regular objects).
			SetFillColor( arcColor );
			DrawArc( Vector2.zero, innerDiameter, diameter, 90 - _fraction * 360, 90, 0, roundness, useGeometricRoundness, constrainAngleSpanToRoundness );
			
			SetFillColor( headerLabelColor );
			SetTextAlignment( TextAlignmentOptions.Bottom );
			SetPivot( Pivot.Bottom );
			SetTextFont( headerFont );
			SetTextSize( headerLabelSize );
			DrawText( _headerPlotText, Vector2.up * ( diameter * 0.5f + diameter * headerLabelOffsetYRelative ), new Vector2( diameter * headerLabelWidthRelative, headerLabelHeight ), debugLabelRects );

			SetFillColor( fractionLabelColor );
			SetTextAlignment( TextAlignmentOptions.Center );
			SetPivot( Pivot.Center );
			SetTextFont( fractionFont );
			SetTextSize( innerDiameter * fractionLabelSizeRelative );
			DrawText( _fractionPlotText, Vector2.zero, Vector2.one * diameter, debugLabelRects );

			// Recall last plot canvas transform and style.
			PopCanvasAndStyle();
		}


		void OnValidate()
		{
			// User fiddled with the inspector, so we assume a change in data.
			fraction = _fraction;
		}
	}
}