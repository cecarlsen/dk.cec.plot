/*
	Copyright © Carl Emil Carlsen 2024
	http://cec.dk

	Loosely based on "Modeling Trees with a Space Colonization Algorithm"
	http://algorithmicbotany.org/papers/colonization.egwnp2007.large.pdf
*/

using System.Collections.Generic;
using UnityEngine;
using static Plot;

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class ExemplifySpaceColonisationTree : MonoBehaviour
	{
		public float sporeSenseRadius = 0.03f;
		public float sporeStep = 0.028f;
		[Range(5f,85f)] public float branchAngleThreshold = 75f;
		[Range(10f,80f)] public float branchAngle = 30f;
		public int seed = 0;
		public int frameRate = 15;

		float _time;

		SpacePoint[] _spacePoints;
		List<Spore> _spores;

		const int spacePointCapacity = 2048;
		const int sporePathCapacity = 128;


		class SpacePoint
		{
			public Vector2 position;
			public bool isConsumed;
		}

		class Spore {
			public bool isAlive;
			public Vector2 direction;
			public Polyline polyline;
			public Vector2 budPosition => polyline.GetPoint( polyline.pointCount-1 ); 

			public Spore( Vector2 position, Vector2 direction ){
				polyline = CreatePolyline( sporePathCapacity );
				polyline.AppendPoint( position );
				this.direction = direction;
				isAlive = true;
			}
		}


		void OnEnable()
		{
			_spacePoints = new SpacePoint[ spacePointCapacity ];
			_spores = new List<Spore>();
			ResetSim();
		}


		void ResetSim()
		{
			var prevSeed = Random.state;
			Random.InitState( seed );
			for( int s = 0; s < spacePointCapacity; s++ ) _spacePoints[ s ] = new SpacePoint(){ position = Random.insideUnitCircle * 0.5f };
			_spores.Clear();
			_spores.Add( new Spore( Vector2.zero, Random.insideUnitCircle.normalized ) );
			Random.state = prevSeed;
			if( Application.isPlaying ) seed++;
		}


		void LateUpdate()
		{
			_time += Time.deltaTime;
			if( _time > 1f / (float) frameRate ) {
				_time -= 1f / frameRate;
				MoveAndBranch();
			}

			Draw();
			ResetWhenAllDead();

		}


		void MoveAndBranch()
		{
			float sporeSenseRadiusSq = sporeSenseRadius*sporeSenseRadius;
			for( int s = _spores.Count-1; s >= 0; s-- ) // Backwards, becase we will be appending new spores.
			{
				var spore = _spores[ s ];
				if( !spore.isAlive || spore.polyline.pointCount == spore.polyline.pointCapacity ) continue;

				// Find non-consumed space points within sporeSenseRadius.
				float angleOffsetMin = float.MaxValue;
				float angleOffsetMax = float.MinValue;
				int consumedCount = 0;
				foreach( var spacePoint in _spacePoints ){
					if( spacePoint.isConsumed ) continue;
					Vector2 spacePointDir = spacePoint.position - spore.budPosition;
					if( spacePointDir.sqrMagnitude < sporeSenseRadiusSq ){
						float angleOfset = Vector2.SignedAngle( spore.direction, spacePointDir );
						if( Mathf.Abs( angleOfset ) >= 90 ) continue; // Ignore space points behind you.
						if( angleOfset < angleOffsetMin ) angleOffsetMin = angleOfset;
						if( angleOfset > angleOffsetMax ) angleOffsetMax = angleOfset;
						spacePoint.isConsumed = true;
						consumedCount++;
					}
				}
				if( consumedCount > 0 ){
					float midAngleOffset = angleOffsetMin + angleOffsetMax;
					if( midAngleOffset < -branchAngle*0.5f ) midAngleOffset = -branchAngle*0.5f;
					else if( midAngleOffset > branchAngle*0.5f ) midAngleOffset = branchAngle*0.5f;
					float angleRad = Mathf.Atan2( spore.direction.y, spore.direction.x ) + midAngleOffset * Mathf.Deg2Rad;
					if( angleOffsetMin < -branchAngleThreshold || angleOffsetMax > branchAngleThreshold ){
						// Branch!
						angleRad += branchAngle * 0.5f * Mathf.Deg2Rad;
						var newSpore = new Spore( spore.budPosition, new Vector2( Mathf.Cos( angleRad ), Mathf.Sin( angleRad ) ) );
						_spores.Add( newSpore );
						newSpore.polyline.AppendPoint( newSpore.budPosition + newSpore.direction * sporeStep );
						angleRad -= branchAngle * Mathf.Deg2Rad;
					}
					spore.direction = new Vector2( Mathf.Cos( angleRad ), Mathf.Sin( angleRad ) );
					spore.polyline.AppendPoint( spore.budPosition + spore.direction * sporeStep );
				} else {
					spore.isAlive = false;
				}
			}
		}


		void Draw()
		{
			PushCanvasAndStyle();
			SetCanvas( transform );
			SetNoStroke();
			SetFillColor( Color.white );
			foreach( var spacePoint in _spacePoints ) if( !spacePoint.isConsumed ) DrawCircle( spacePoint.position, 0.004f );
			SetStrokeColor( Color.white );
			SetNoFill();
			foreach( var spore in _spores ){
				if( spore.isAlive ){
					SetStrokeWidth( 0.001f );
					DrawCircle( spore.budPosition, sporeSenseRadius );
				}
				SetStrokeWidth( 0.005f );
				DrawPolyline( spore.polyline );
			}
			PopCanvasAndStyle();
		}


		void ResetWhenAllDead()
		{
			bool anyAlive = false;
			foreach( var spore in _spores ){
				if( spore.isAlive ){
					anyAlive = true;
					break;
				}
			}
			if( !anyAlive ) ResetSim();
		}
	}	
}