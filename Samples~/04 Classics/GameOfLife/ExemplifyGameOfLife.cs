/*
	Copyright © Carl Emil Carlsen 2024
	http://cec.dk

	Conway's Game of Life
	https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life
*/

using UnityEngine;
using static Plot;

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class ExemplifyGameOfLife : MonoBehaviour
	{
		bool[,] _isAliveFlags0 = new bool[squareCellCount,squareCellCount]; // Current frame.
		bool[,] _isAliveFlags1 = new bool[squareCellCount,squareCellCount]; // Previous frame.
		bool[,] _isAliveFlags2 = new bool[squareCellCount,squareCellCount]; // Frame before previous frame.
		const int squareCellCount = 32;
		

		void OnEnable() => Application.targetFrameRate = 25;
		void OnDisable() => Application.targetFrameRate = 0;


		void Update()
		{
			if( AllDeadOrNoChangeOrRepeating() ) Seed();
			ApplyConwayRules();
			Draw();
		}


		bool AllDeadOrNoChangeOrRepeating()
		{
			bool anyChange = false;
			bool anyAlive = false;
			bool isRepeating = true;
			for( int y = 0; y < squareCellCount; y++ ){
				for( int x = 0; x < squareCellCount; x++ ){
					if( !anyAlive && _isAliveFlags0[ x, y ] ) anyAlive = true;
					if( _isAliveFlags0[ x, y ] != _isAliveFlags1[ x, y ] ) anyChange = true;
					if(  isRepeating && _isAliveFlags0[ x, y ] != _isAliveFlags2[ x, y ] ) isRepeating = false;
				}
			}
			return !anyAlive || !anyChange || isRepeating;
		}


		void Seed()
		{
			for( int y = 0; y < squareCellCount; y++ ){
				for( int x = 0; x < squareCellCount; x++ ){
					if( Random.value < 0.2f ) _isAliveFlags0[ x, y ] = true;
				}
			}
		}


		void ApplyConwayRules()
		{
			// Store previous frame so can check later for repetition.
			CopyDoubleArray( _isAliveFlags1, _isAliveFlags2 );

			// We copy to a temporary array to avoid writing to and reading from the same flags.
			CopyDoubleArray( _isAliveFlags0, _isAliveFlags1 );

			// For every cell.
			for( int y = 0; y < squareCellCount; y++ ){
				for( int x = 0; x < squareCellCount; x++ )
				{
					// Count alive neighbors.
					int aliveNeighborCount = 0;
					for( int ny = -1; ny <= 1; ny++ ){
						for( int nx = -1; nx <= 1; nx++ ){
							if( nx == 0 && ny == 0 ) continue;
							int wx = x + nx;
							int wy = y + ny;
							wx = wx < 0 ? squareCellCount-1 : wx >= squareCellCount ? 0 : wx;
							wy = wy < 0 ? squareCellCount-1 : wy >= squareCellCount ? 0 : wy;
							if( _isAliveFlags1[ wx, wy ] ) aliveNeighborCount++;
						}
					}

					// The rules of life:
					if( _isAliveFlags1[ x, y ] ){
						// Death by under or over population.
						if( aliveNeighborCount < 2 || aliveNeighborCount > 3 ){
							_isAliveFlags0[ x, y ] = false;
						}
					} else {
						// Polyamorous reproduction.
						if( aliveNeighborCount == 3 ){
							_isAliveFlags0[ x, y ] = true;
						}
					}
				}
			}
		}


		void Draw()
		{
			PushCanvasAndStyle();
			SetCanvas( transform );

			TranslateCanvas( -0.5f, -0.5f );
			ScaleCanvas( 1f / squareCellCount );

			SetNoStroke();

			for( int y = 0; y < squareCellCount; y++ ){
				for( int x = 0; x < squareCellCount; x++ ){
					if( _isAliveFlags0[ x, y ] ){
						DrawRect( x, y, 0.93f, 0.93f, roundness: 0.3f );
					}
				}
			}

			PopCanvasAndStyle();
		}


		// Just a helper method https://stackoverflow.com/a/16437758
		static void CopyDoubleArray<T>( T[,] a, T[,] b )
		{
			int minX = Mathf.Min( a.GetLength(0), b.GetLength(0) );
			int minY = Mathf.Min( a.GetLength(1), b.GetLength(1) );
			for( int i = 0; i < minX; ++i ) System.Array.Copy( a, i * a.GetLength(1), b, i * b.GetLength(1), minY );
		}
	}
}