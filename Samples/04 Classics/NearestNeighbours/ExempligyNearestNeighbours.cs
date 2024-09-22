/*
	Copyright © Carl Emil Carlsen 2023
	http://cec.dk
*/

using UnityEngine;
using static Plot;

[ExecuteInEditMode]
public class ExempligyNearestNeighbours : MonoBehaviour
{
	Vector2[] _pos, _vel;

	const int count = 100;
	const float radius = 0.01f;
	const float distMax = 0.12f;

	void Update()
	{
		if( _pos == null || _pos.Length != count ) {
			_pos = new Vector2[ count ];
			_vel = new Vector2[ count ];
			for( int i = 0; i < count; i++ ) {
				_pos[ i ] = Vector2.one * 0.5f + Random.insideUnitCircle;
				_vel[ i ] = Random.insideUnitCircle.normalized * 0.1f;
			}
		}

		for( int i = 0; i < count; i++ ){
			_pos[ i ] += _vel[ i ] * Time.deltaTime;
			_pos[ i ].Set( Mathf.Repeat( _pos[ i ].x, 1f ), Mathf.Repeat( _pos[ i ].y, 1f ) );
		}

		SetStrokeWidth( radius * 0.5f );
		SetStrokeColor( Color.white );
		for( int ia = 0; ia < count; ia++ ) {
			for( int ib = 0; ib < ia; ib++ ) {
				if( ( _pos[ ib ] - _pos[ ia ] ).sqrMagnitude < distMax * distMax ){
					DrawLine( _pos[ ia ], _pos[ ib ] );
				}
			}
		}

		SetNoStrokeColor();
		SetFillColor( Color.white );
		foreach( var p in _pos ) DrawCircle( p, radius*2 );
	}
}