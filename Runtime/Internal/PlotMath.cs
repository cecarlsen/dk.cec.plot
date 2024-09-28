/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

using UnityEngine;

namespace PlotInternals
{
	public static class PlotMath
	{
		/// <summary>
		/// Finds the intersection point between two continous lines. Returns true on success.
		/// </summary>
		public static bool TryIntersectLineLine( Vector2 lineAp1, Vector2 lineAp2, Vector2 lineBp1, Vector2 lineBp2, out Vector2 intersection )
		{
			intersection = Vector2.zero;

			float x2x1 = lineAp2.x - lineAp1.x;
			float y2y1 = lineAp2.y - lineAp1.y;
			float x4x3 = lineBp2.x - lineBp1.x;
			float y4y3 = lineBp2.y - lineBp1.y;
			float d = y4y3 * x2x1 - x4x3 * y2y1;
			if( d == 0 ) return false;
			float ua = ( x4x3 * ( lineAp1.y - lineBp1.y ) - y4y3 * ( lineAp1.x - lineBp1.x ) ) / d;
			intersection.x = lineAp1.x + ua * x2x1;
			intersection.y = lineAp1.y + ua * y2y1;
			return true;
		}


		/// <summary>
		/// Evalutes quadratic bezier at point t for points a, b, c, d.
		///	t varies between 0 and 1, and a and d are the curve points,
		///	b and c are the control points. this can be done once with the
		///	x coordinates and a second time with the y coordinates to get
		///	the location of a bezier curve at t.
		/// </summary>
		public static float QuadraticInterpolation( float a, float b, float c, float d, float t )
		{
			float t1 = 1f - t;
			return a * t1 * t1 * t1 + 3 * b * t * t1 * t1 + 3 * c * t * t * t1 + d * t * t * t;
		}


		/// <summary>
		/// Takes a series of points and fills an array with normalized directions ponting from one to the next.
		/// </summary>
		public static void ComputeNormalizedDirections( Vector2[] points, ref Vector2[] directions, int count = -1, bool wrap = false )
		{
			if( directions == null || directions.Length != points.Length ) directions = new Vector2[ points.Length ];

			if( count < 0 ) count = points.Length;
			if( count > points.Length ) count = points.Length;

			Vector2 thisPoint = points[ 0 ];
			int lastP = count - 1;
			if( wrap ) {
				Vector2 dir = thisPoint - points[ lastP ];
				dir.Normalize();
				directions[ lastP ] = dir;
			}
			for( int p1 = 1; p1 < count; p1++ ) {
				Vector2 nextPoint = points[ p1 ];
				Vector2 dir = nextPoint - thisPoint;
				dir.Normalize();
				directions[ p1 - 1 ] = dir;
				thisPoint = nextPoint;
			}
			if( !wrap ) {
				directions[ lastP ] = directions[ count - 2 ];
			}
		}


		/// <summary>
		/// Takes a series of points and fills an array with normalized directions ponting from one to the next.
		/// Also output positionsAlongLine since we are computing vector2 lengths anyway.
		/// </summary>
		public static void ComputeNormalizedDirections( Vector2[] points, ref Vector2[] directions, ref float[] positionsAlongLine, int count = -1, bool wrap = false )
		{
			if( directions == null || directions.Length != points.Length ) directions = new Vector2[ points.Length ];
			if( positionsAlongLine == null || positionsAlongLine.Length != points.Length ) positionsAlongLine = new float[ points.Length ];

			if( count < 0 ) count = points.Length;
			if( count > points.Length ) count = points.Length;

			var thisPoint = points[ 0 ];
			int lastP = count - 1;
			float pos = 0;
			positionsAlongLine[ 0 ] = 0;
			if( wrap ) {
				var dir = thisPoint - points[ lastP ];
				dir.Normalize();
				directions[ lastP ] = dir;
			}
			for( int p0 = 0, p1 = 1; p1 < count; p0++, p1++ ) {
				var nextPoint = points[ p1 ];
				var dir = nextPoint - thisPoint;
				float length = dir.magnitude;
				if( length > 0 ) dir /= length;
				pos += length;
				directions[ p0 ] = dir;
				positionsAlongLine[ p1 ] = pos;
				thisPoint = nextPoint;
			}
			if( !wrap ) {
				directions[ lastP ] = directions[ count - 2 ];
			}
		}
	}
}