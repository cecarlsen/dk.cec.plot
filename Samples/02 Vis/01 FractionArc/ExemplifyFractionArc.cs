/*
	Copyright © Carl Emil Carlsen 2021
	http://cec.dk
*/

using UnityEngine;
using static Plot;
#if TMP_PRESENT // Only use TextMeshPro if the user has declared that it is imported.
using TMPro;
#endif

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
#if TMP_PRESENT
		[SerializeField] TMP_FontAsset _headerFont = null;
#endif
		public float headerLabelSizeRelative = 0.3f;
		public float headerLabelWidthRelative = 3f;
		public float headerLabelOffsetYRelative = 0.05f;
		public Color headerLabelColor = Color.white;
#if TMP_PRESENT
		[SerializeField] public TMP_FontAsset _fractionFont = null;
#endif
		[Range(0,1)] public float fractionLabelSizeRelative = 0.3f;
		public Color fractionLabelColor = Color.white;

		[Header( "Debug" )]
		public bool debugLabelRects = false;

		bool _dataChanged = true;

#if TMP_PRESENT
		[SerializeField,HideInInspector] TextMeshPro _headerLabel = null;
		[SerializeField,HideInInspector] TextMeshPro _fractionLabel = null;
#endif

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

		#if TMP_PRESENT
		public TMP_FontAsset headerFont {
			get { return _headerFont; }
			set {
				_headerFont = value;
				if( _headerLabel ) _headerLabel.font = _headerFont;
			}
		}

		public TMP_FontAsset fractionFont {
			get { return _fractionFont; }
			set {
				_fractionFont = value;
				if( _fractionLabel ) _fractionLabel.font = _fractionFont;
			}
		}
		#endif


		void Update()
		{
			// Only update text when data change. Text changes always generate garbage.
			if( _dataChanged ) {
#if TMP_PRESENT
				// Ensure that we have the TextMeshPro components we need.
				if( !_headerLabel ) _headerLabel = TextHelper.CreateText( transform, _headerFont );
				if( !_fractionLabel ) _fractionLabel = TextHelper.CreateText( transform, _fractionFont );
				// Update text.
				_headerLabel.text = "<b>" + _headerText + "</b>";
				_fractionLabel.text = Mathf.RoundToInt( _fraction * 100 ).ToString() + "%";
#endif
				_dataChanged = false;
			}

			// Save current plot canvas transform and style sate.
			PushStyle();
			PushCanvas();

			// Draw relative to this transform.
			SetCanvas( transform );

			// Setup shared drawing style.
			SetNoStrokeColor();
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
			
			#if TMP_PRESENT

			SetFillColor( headerLabelColor );
			SetTextAlignment( TextAlignmentOptions.Bottom );
			SetPivot( Pivot.Bottom );
			SetTextSize( headerLabelSize );
			DrawText( _headerLabel, Vector2.up * ( diameter * 0.5f + diameter * headerLabelOffsetYRelative ), new Vector2( diameter * headerLabelWidthRelative, headerLabelHeight ), debugLabelRects );

			SetFillColor( fractionLabelColor );
			SetPivot( Pivot.Center );
			SetTextAlignment( TextAlignmentOptions.Center );
			SetTextSize( innerDiameter * fractionLabelSizeRelative );
			DrawText( _fractionLabel, Vector2.zero, Vector2.one * diameter, debugLabelRects );

			#endif

			// Recall last plot canvas transform and style.
			PopCanvas();
			PopStyle();
		}


		void OnValidate()
		{
			// User fiddled with the inspector, so we assume a change in data.
			fraction = _fraction;
			#if TMP_PRESENT
			headerFont = _headerFont;
			fractionFont = _fractionFont;
			#endif
		}
	}
}