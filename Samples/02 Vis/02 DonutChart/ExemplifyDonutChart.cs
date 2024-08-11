/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

using System.Collections.Generic;
using UnityEngine;
using static Plot;
#if TMP_PRESENT // Only use TextMeshPro if the user has declared that it is imported.
using TMPro;
#endif

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
#if TMP_PRESENT
		[SerializeField] public TMP_FontAsset _font = null;
#endif
		public float labelSize = 0.03f;
		public float labelOffset = 0.1f;
		public Color labelColor = Color.white;


		bool _dataChanged = true;

#if TMP_PRESENT
		List<TextMeshPro> _labels = new List<TextMeshPro>();
#endif

#if TMP_PRESENT
		public TMP_FontAsset font {
			get { return _font; }
			set {
				_font = value;
				foreach( TextMeshPro l in _labels ) l.font = _font;
			}
		}
#endif


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
		

		void Update()
		{
			// Only update text when data change. Text changes always generate garbage.
			if( _dataChanged ){
#if TMP_PRESENT
				// Ensure that we have the TextMeshPro components we need.
				TextHelper.AdaptTextCount( transform, _entries.Count, _labels, _font );
				// Update text.
				for( int i = 0; i < _entries.Count; i++ ) {
					Entry entry = _entries[ i ];
					_labels[ i ].text = "<b>" + entry.name + "</b> " + entry.value.ToString( "F2" );
				}
#endif
				_dataChanged = false;
			}

			// Sanity check.
			if( _entries?.Count == 0 ) return;

			// Save current plot canvas transform and style sate.
			PushStyle();
			PushCanvas();

			// Draw relative to this transform.
			SetCanvas( transform );

			// Setup shared drawing style.
			SetAntiAliasing( antiAliasing );
			SetStrokeWidth( strokeWidth );
			SetStrokeAlignement( StrokeAlignment.Inside );
			SetStrokeCornerProfile( strokeCornerProfile );
#if TMP_PRESENT
			SetTextSize( labelSize );
#endif

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
				
				// Draw arc.
				Color color = JChColor.Slerp( jchBegin, jchEnd, i * step );
				SetPivot( Pivot.Center );
				SetFillColor( color );
				SetStrokeColor( color, strokeAlpha );
				DrawArc( 0, 0, diameter * innerDiameterFactor, diameter, endAngle, beginAngle, spacingCut, roundness, useGeometricRoundness );

#if TMP_PRESENT
				// Draw label.
				float midAngle = Mathf.LerpAngle( beginAngle, endAngle, 0.5f );
				Vector2 position = Quaternion.AngleAxis( midAngle, Vector3.forward ) * Vector3.right * ( diameter * 0.5f + labelOffset );
				TextAlignmentOptions alignment = TextHelper.OffsetToTextAlignment( position );
				SetTextAlignment( alignment );
				
				SetPivot( TextHelper.AlignmentToPivot( alignment ) );
				SetFillColor( labelColor );
				DrawText( _labels[ i ], position, new Vector2( 1, 0.2f ) );
#endif
			}

			// Recall last plot canvas transform and style.
			PopCanvas();
			PopStyle();
		}


		void OnValidate()
		{
			_dataChanged = true; // User fiddled with the inspector, so we assume a change in data.
#if TMP_PRESENT
			font = _font;
#endif
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