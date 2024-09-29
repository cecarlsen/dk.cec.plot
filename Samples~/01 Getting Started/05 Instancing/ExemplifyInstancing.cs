/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk

	Learn about Phyllotaxis with Daniel Shiffman
	https://www.youtube.com/watch?v=KWoJgHFYWxY
*/

using UnityEngine;
using static Plot;

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class ExemplifyInstancing : MonoBehaviour
	{
		public Mode mode = Mode.SingleShape;
		public int count = 64;
		public Color paletteBegin = Color.red;
		public Color paletteEnd = Color.blue;
		public bool placeShapesAtUniqueDepths = true;

		Mode _selectedMode = Mode.SingleShape;
		int[] _shapeLookup;
		Vector2[] _positions;
		Color[] _colors;

		bool _dirtyPhylotaxis = true;

		const float shapeSize = 0.05f;
		const int shapeGroupCount = 3;

		public enum Mode { SingleShape, AlternatingShapes, MixedShapes, Sorted }


		void LateUpdate()
		{
			PushCanvasAndStyle();

			SetNoStroke();

			if( _positions?.Length != count || _dirtyPhylotaxis ) GeneratePhyllotaxis();
			if( _shapeLookup?.Length != count || _selectedMode != mode ) UpdateShapeLookup();

			for( int i = 0; i < count; i++ )
			{
				Vector2 p = _positions[ i ];
				int shape = _shapeLookup[ i ];
				if( placeShapesAtUniqueDepths ) {
					PushCanvas();
					TranslateCanvas( 0, 0, shape );
				}

				SetFillColor( _colors[ i % _colors.Length ] );
				switch( shape )
				{
					case 0: DrawCircle( p.x, p.y, shapeSize ); break;
					case 1: DrawPie( p.x, p.y, shapeSize, -150, 300 ); break;
					case 2: DrawSquare( p.x, p.y, shapeSize, 0.5f ); break;
				}

				if( placeShapesAtUniqueDepths ) PopCanvas();
			}

			PopCanvasAndStyle();
		}


		void UpdateShapeLookup()
		{
			if( _shapeLookup?.Length != count ) _shapeLookup = new int[ count ];
			Random.InitState( 0 );
			for( int i = 0; i < count; i++ ){
				int shape = (int) mode;
				if( mode == Mode.AlternatingShapes ) shape = i % shapeGroupCount;
				else if( mode == Mode.MixedShapes ) shape = Random.Range( 0, shapeGroupCount );
				else if( mode == Mode.Sorted ) shape = (int) ( shapeGroupCount * Mathf.Pow( i / (float) count, 0.5f ) );
				_shapeLookup[ i ] = shape;
			}
			_selectedMode = mode;
		}


		void OnValidate()
		{
			_dirtyPhylotaxis = true;
		}


		void GeneratePhyllotaxis()
		{
			_colors = JChColor.LerpCreatePalette( paletteBegin, paletteEnd, shapeGroupCount );
			_positions = new Vector2[ count ];
			for( int i = 1; i < count + 1; i++ ) {
				float a = i * 137.5f * Mathf.Deg2Rad;
				float r = shapeSize * Mathf.Sqrt( i );
				float x = Mathf.Cos( a ) * r;
				float y = Mathf.Sin( a ) * r;
				int j = i-1;
				_positions[ j ] = new Vector2( x, y );
			}
		}
	}
}