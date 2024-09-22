/*
	Copyright © Carl Emil Carlsen 2024
	http://cec.dk
*/

using UnityEngine;
using static Plot;

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class ExemplifyGameOfLife : MonoBehaviour
	{
		bool[,] _isAliveFlags = new bool[squareCellCount,squareCellCount];
		bool[,] _isAliveFlagsPrev = new bool[squareCellCount,squareCellCount];
		const int squareCellCount = 32;
		

		void Start()
		{
			Application.targetFrameRate = 25;
		}


		void Update()
		{
			if( AllDeadOrNoChange() ) Seed();
			ApplyConwayRules();
			Draw();
		}


		bool AllDeadOrNoChange()
		{
			bool anyChange = false;
			bool anyAlive = false;
			for( int y = 0; y < squareCellCount; y++ ){
				for( int x = 0; x < squareCellCount; x++ ){
					if( !anyAlive && _isAliveFlags[ x, y ] ) anyAlive = true;
					if( _isAliveFlags[ x, y ] != _isAliveFlagsPrev[ x, y ] ) anyChange = true;
				}
			}
			return !anyAlive || !anyChange;
		}


		void Seed()
		{
			for( int y = 0; y < squareCellCount; y++ ){
				for( int x = 0; x < squareCellCount; x++ ){
					if( Random.value < 0.3f ) _isAliveFlags[ x, y ] = true;
				}
			}
		}


		void ApplyConwayRules()
		{
			// We copy to a temporary array to avoid writing to and reading from the same flags.
			for( int y = 0; y < squareCellCount; y++ ){
				for( int x = 0; x < squareCellCount; x++ ){
					_isAliveFlagsPrev[ x, y ] = _isAliveFlags[ x, y ];
				}
			}

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
							if( _isAliveFlagsPrev[ wx, wy ] ) aliveNeighborCount++;
						}
					}

					// The rules of life:
					if( _isAliveFlagsPrev[ x, y ] ){
						// Death by under or over population.
						if( aliveNeighborCount < 2 || aliveNeighborCount > 3 ){
							_isAliveFlags[ x, y ] = false;
						}
					} else {
						// Reproduction.
						if( aliveNeighborCount == 3 ){
							_isAliveFlags[ x, y ] = true;
						}
					}
				}
			}
		}


		void Draw()
		{
			PushStyle();
			PushCanvas();
			SetCanvas( transform );
			TranslateCanvas( -0.5f, -0.5f );
			ScaleCanvas( 1f / squareCellCount );

			SetNoStrokeColor();

			for( int y = 0; y < squareCellCount; y++ ){
				for( int x = 0; x < squareCellCount; x++ ){
					if( _isAliveFlags[ x, y ] ){
						DrawRect( x, y, 0.95f, 0.95f, roundness: 0.2f );
					}
				}
			}

			PopCanvas();
			PopStyle();
		}
	}
}