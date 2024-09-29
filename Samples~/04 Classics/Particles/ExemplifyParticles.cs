/*
	Copyright © Carl Emil Carlsen 2024
	http://cec.dk

	A simple particle system without classes and objects.
*/

using UnityEngine;
using static Plot;

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class ExemplifyParticles : MonoBehaviour
	{
		public float spawnSpeed = 1f;
		public float gravityForce = 1f;
		public AnimationCurve _sizeOverLifetime = AnimationCurve.Linear( 0f, 1f, 1f, 0f );

		Vector2[] _pos, _vel, _acc;
		float[] _age;
		bool[] _alive;

		float _spawnTime;

		const int capacity = 100;
		const float lifetime = 1f;
		const float spawnRate = (capacity-1) / lifetime; // Spawn at max capacity (particles per second).


		void OnEnable()
		{
			_pos = new Vector2[ capacity ];
			_vel = new Vector2[ capacity ];
			_acc = new Vector2[ capacity ];
			_age = new float[ capacity ];
			_alive = new bool[ capacity ];
		}


		void LateUpdate()
		{
			// Spawn.
			_spawnTime += Time.deltaTime;
			if( _spawnTime > 1f / spawnRate ){
				int spawnCount = (int) ( _spawnTime * spawnRate );
				_spawnTime -= spawnCount / spawnRate;
				if( spawnCount > 0 ){
					for( int p = 0; p < capacity; p++ ){
						if( _alive[ p ]) continue;
						_pos[ p ] = Vector2.zero;
						_vel[ p ] = Random.insideUnitCircle.normalized * Random.Range( 0.5f, 1f ) * spawnSpeed;
						_acc[ p ] = Vector2.zero;
						_age[ p ] = 0f;
						_alive[ p ] = true;
						if( --spawnCount == 0 ) break;
					}
				}
			}

			// Update.
			for( int p = 0; p < capacity; p++ ){
				if( !_alive[ p ] ) continue;
				if( _age[ p ] > lifetime ){
					_alive[ p ] = false;
					continue;
				}
				_acc[ p ] = Vector2.zero;
				//_acc[ p ] += ... other forces here.
				_acc[ p ] += Vector2.down * gravityForce;
				_vel[ p ] += _acc[ p ] * Time.deltaTime;
				_pos[ p ] += _vel[ p ] * Time.deltaTime; 
				_age[ p ] += Time.deltaTime;
			}

			// Draw.
			PushCanvasAndStyle();
			SetCanvas( transform );
			SetNoStroke();
			SetFillColor( Color.white );
			for( int p = 0; p < capacity; p++ ){
				if( !_alive[ p ] ) continue;
				float sizeFactor = _sizeOverLifetime.Evaluate( _age[ p ] / lifetime );
				PushCanvas();
				TranslateCanvas( _pos[ p ] );
				RotateCanvas( Mathf.Atan2( _vel[ p ].y, _vel[ p ].x ) * Mathf.Rad2Deg );
				DrawPie( Vector2.zero, 0.1f * sizeFactor, -30, 60 );
				PopCanvas();
			}
			PopCanvasAndStyle();
		}
	}
}