/*
	Copyright © Carl Emil Carlsen 2020-2024
	http://cec.dk
*/

using System.Collections.Generic;
using UnityEngine;
using static Plot;
using TMPro;

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class ExemplifyDonutChart : MonoBehaviour
	{
		[Header("Data")]
		[SerializeField] List<Entry> _entries = null;

		[Header("Shape")]
		[Range(0,1)] public float diameter = 1;
		[Range(0,1)] public float innerDiameterFactor = 0.5f;

		[Range(0,0.1f)] public float spacingCut = 0;
		[Range(0,1)] public float roundness = 0;
		public bool useGeometricRoundness = false;
		public bool antiAliasing = true;

		[Header("Stroke")]
		[Range(0,0.1f)] public float strokeWidth = 0.05f;
		[Range(0,1)] public float strokeAlpha = 0.2f;
		public StrokeCornerProfile strokeCornerProfile = StrokeCornerProfile.Round;

		[Header("Color")]
		public Color paletteColorBegin = Color.red;
		public Color paletteColorEnd = Color.blue;

		[Header("Labels")]
		public TMP_FontAsset font;
		public float labelSize = 0.03f;
		public float labelOffset = 0.1f;
		public Color labelColor = Color.white;


		bool _dataChanged = true;

		List<Text> _labels = new List<Text>();


		public int GetEntryCount()
		{
			return _entries.Count;
		}


		public void AddEntry( string name, float value )
		{
			_entries.Add( new Entry( name, value ) );
			_dataChanged = true;
		}


		public void UpdateEntryValue( int index,  float value )
		{
			_entries[index].value = value;
			_dataChanged = true;
		}


		public void RemoveEntry( int index )
		{
			_entries.RemoveAt( index );
			_dataChanged = true;
		}
		

		void LateUpdate()
		{
			// Only update text when data change. Text changes always generate garbage.
			if( _dataChanged )
			{
				// Ensure that we have the TextMeshPro components we need.
				AdaptTextCount( _entries.Count, _labels );
				// Update text.
				for( int i = 0; i < _entries.Count; i++ ) {
					Entry entry = _entries[ i ];
					_labels[ i ].SetContent( "<b>" + entry.name + "</b> " + entry.value.ToString( "F2" ) );
				}
				_dataChanged = false;
			}

			// Sanity check.
			if( _entries?.Count == 0 ) return;

			// Save current plot canvas transform and style sate.
			PushCanvasAndStyle();

			// Draw relative to this transform.
			SetCanvas( transform );

			// Setup shared drawing style.
			SetAntiAliasing( antiAliasing );
			SetStrokeWidth( strokeWidth );
			SetStrokeAlignement( StrokeAlignment.Inside );
			SetStrokeCornerProfile( strokeCornerProfile );
			SetTextFont( font );
			SetTextSize( labelSize );

			// Sum the data.
			float sum = 0;
			foreach( Entry entry in _entries ) sum += entry.value;
			if( sum <= 0 ) return;

			// Draw the segments.
			float beginAngle;
			float endAngle = 90;
			float valueToAngle = sum > 0 ? 360 / sum : 0;
			float step = 1 / (_entries.Count-1f);
			JChColor jchBegin = new JChColor( paletteColorBegin );
			JChColor jchEnd = new JChColor( paletteColorEnd );
			for( int i = 0; i < _entries.Count; i++ )
			{
				Entry entry = _entries[ i ];
				beginAngle = endAngle;
				endAngle = beginAngle - entry.value * valueToAngle;
				float deltaAngle = endAngle - beginAngle;

				// Draw arc.
				Color color = JChColor.Slerp( jchBegin, jchEnd, i * step );
				SetPivot( Pivot.Center );
				SetFillColor( color );
				SetStrokeColor( color, strokeAlpha );
				DrawArc( 0, 0, diameter * innerDiameterFactor, diameter, beginAngle, deltaAngle, spacingCut, roundness, useGeometricRoundness );

				// Draw label.
				float midAngle = Mathf.LerpAngle( beginAngle, endAngle, 0.5f );
				Vector2 position = Quaternion.AngleAxis( midAngle, Vector3.forward ) * Vector3.right * ( diameter * 0.5f + labelOffset );
				TextAlignmentOptions alignment = ConvertOffsetToTextAlignment( position );
				SetTextAlignment( alignment );
				
				SetPivot( ConvertAlignmentToPivot( alignment ) );
				SetFillColor( labelColor );
				PushCanvas();
				TranslateCanvas( position );
				DrawText( _labels[ i ], Vector3.zero, new Vector2( 1, 0.2f ) );
				PopCanvas();
			}

			// Recall last plot canvas transform and style.
			PopCanvasAndStyle();
		}


		void OnValidate()
		{
			_dataChanged = true; // User fiddled with the inspector, so we assume a change in data.
			//font = _font;
		}


		[System.Serializable]
		public class Entry
		{
			public string name;
			[Range(0,100)] public float value = 100;

			public Entry( string name, float value )
			{
				this.name = name;
				this.value = value;
			}
		}
	}
}