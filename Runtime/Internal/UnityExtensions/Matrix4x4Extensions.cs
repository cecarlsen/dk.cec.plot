/*
	Copyright © Carl Emil Carlsen 2019-2020
	http://cec.dk

	Custom extensions that are faster than then default implementations.
	This is mainly achived by avoiding constructor class.
*/

using UnityEngine;
using System.Runtime.CompilerServices;

namespace PlotInternals
{
	public static class Matrix4x4Extensions
	{
		/// <summary>
		/// Multiply two matrices. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Multiply( ref this Matrix4x4 lhs, Matrix4x4 rhs )
		{
			float m00 = (float) ( (double) lhs.m00 * (double) rhs.m00 + (double) lhs.m01 * (double) rhs.m10 + (double) lhs.m02 * (double) rhs.m20 + (double) lhs.m03 * (double) rhs.m30 );
			float m01 = (float) ( (double) lhs.m00 * (double) rhs.m01 + (double) lhs.m01 * (double) rhs.m11 + (double) lhs.m02 * (double) rhs.m21 + (double) lhs.m03 * (double) rhs.m31 );
			float m02 = (float) ( (double) lhs.m00 * (double) rhs.m02 + (double) lhs.m01 * (double) rhs.m12 + (double) lhs.m02 * (double) rhs.m22 + (double) lhs.m03 * (double) rhs.m32 );
			float m03 = (float) ( (double) lhs.m00 * (double) rhs.m03 + (double) lhs.m01 * (double) rhs.m13 + (double) lhs.m02 * (double) rhs.m23 + (double) lhs.m03 * (double) rhs.m33 );
			float m10 = (float) ( (double) lhs.m10 * (double) rhs.m00 + (double) lhs.m11 * (double) rhs.m10 + (double) lhs.m12 * (double) rhs.m20 + (double) lhs.m13 * (double) rhs.m30 );
			float m11 = (float) ( (double) lhs.m10 * (double) rhs.m01 + (double) lhs.m11 * (double) rhs.m11 + (double) lhs.m12 * (double) rhs.m21 + (double) lhs.m13 * (double) rhs.m31 );
			float m12 = (float) ( (double) lhs.m10 * (double) rhs.m02 + (double) lhs.m11 * (double) rhs.m12 + (double) lhs.m12 * (double) rhs.m22 + (double) lhs.m13 * (double) rhs.m32 );
			float m13 = (float) ( (double) lhs.m10 * (double) rhs.m03 + (double) lhs.m11 * (double) rhs.m13 + (double) lhs.m12 * (double) rhs.m23 + (double) lhs.m13 * (double) rhs.m33 );
			float m20 = (float) ( (double) lhs.m20 * (double) rhs.m00 + (double) lhs.m21 * (double) rhs.m10 + (double) lhs.m22 * (double) rhs.m20 + (double) lhs.m23 * (double) rhs.m30 );
			float m21 = (float) ( (double) lhs.m20 * (double) rhs.m01 + (double) lhs.m21 * (double) rhs.m11 + (double) lhs.m22 * (double) rhs.m21 + (double) lhs.m23 * (double) rhs.m31 );
			float m22 = (float) ( (double) lhs.m20 * (double) rhs.m02 + (double) lhs.m21 * (double) rhs.m12 + (double) lhs.m22 * (double) rhs.m22 + (double) lhs.m23 * (double) rhs.m32 );
			float m23 = (float) ( (double) lhs.m20 * (double) rhs.m03 + (double) lhs.m21 * (double) rhs.m13 + (double) lhs.m22 * (double) rhs.m23 + (double) lhs.m23 * (double) rhs.m33 );
			float m30 = (float) ( (double) lhs.m30 * (double) rhs.m00 + (double) lhs.m31 * (double) rhs.m10 + (double) lhs.m32 * (double) rhs.m20 + (double) lhs.m33 * (double) rhs.m30 );
			float m31 = (float) ( (double) lhs.m30 * (double) rhs.m01 + (double) lhs.m31 * (double) rhs.m11 + (double) lhs.m32 * (double) rhs.m21 + (double) lhs.m33 * (double) rhs.m31 );
			float m32 = (float) ( (double) lhs.m30 * (double) rhs.m02 + (double) lhs.m31 * (double) rhs.m12 + (double) lhs.m32 * (double) rhs.m22 + (double) lhs.m33 * (double) rhs.m32 );
			float m33 = (float) ( (double) lhs.m30 * (double) rhs.m03 + (double) lhs.m31 * (double) rhs.m13 + (double) lhs.m32 * (double) rhs.m23 + (double) lhs.m33 * (double) rhs.m33 );

			lhs.m00 = m00;
			lhs.m01 = m01;
			lhs.m02 = m02;
			lhs.m03 = m03;
			lhs.m10 = m10;
			lhs.m11 = m11;
			lhs.m12 = m12;
			lhs.m13 = m13;
			lhs.m20 = m20;
			lhs.m21 = m21;
			lhs.m22 = m22;
			lhs.m23 = m23;
			lhs.m30 = m30;
			lhs.m31 = m31;
			lhs.m32 = m32;
			lhs.m33 = m33;
		}


		/// <summary>
		/// Multiply two matrices without skew/projection. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Multiply3x4( ref this Matrix4x4 lhs, Matrix4x4 rhs )
		{
			float m00 = (float) ( (double) lhs.m00 * (double) rhs.m00 + (double) lhs.m01 * (double) rhs.m10 + (double) lhs.m02 * (double) rhs.m20 + (double) lhs.m03 * (double) rhs.m30 );
			float m01 = (float) ( (double) lhs.m00 * (double) rhs.m01 + (double) lhs.m01 * (double) rhs.m11 + (double) lhs.m02 * (double) rhs.m21 + (double) lhs.m03 * (double) rhs.m31 );
			float m02 = (float) ( (double) lhs.m00 * (double) rhs.m02 + (double) lhs.m01 * (double) rhs.m12 + (double) lhs.m02 * (double) rhs.m22 + (double) lhs.m03 * (double) rhs.m32 );
			float m03 = (float) ( (double) lhs.m00 * (double) rhs.m03 + (double) lhs.m01 * (double) rhs.m13 + (double) lhs.m02 * (double) rhs.m23 + (double) lhs.m03 * (double) rhs.m33 );
			float m10 = (float) ( (double) lhs.m10 * (double) rhs.m00 + (double) lhs.m11 * (double) rhs.m10 + (double) lhs.m12 * (double) rhs.m20 + (double) lhs.m13 * (double) rhs.m30 );
			float m11 = (float) ( (double) lhs.m10 * (double) rhs.m01 + (double) lhs.m11 * (double) rhs.m11 + (double) lhs.m12 * (double) rhs.m21 + (double) lhs.m13 * (double) rhs.m31 );
			float m12 = (float) ( (double) lhs.m10 * (double) rhs.m02 + (double) lhs.m11 * (double) rhs.m12 + (double) lhs.m12 * (double) rhs.m22 + (double) lhs.m13 * (double) rhs.m32 );
			float m13 = (float) ( (double) lhs.m10 * (double) rhs.m03 + (double) lhs.m11 * (double) rhs.m13 + (double) lhs.m12 * (double) rhs.m23 + (double) lhs.m13 * (double) rhs.m33 );
			float m20 = (float) ( (double) lhs.m20 * (double) rhs.m00 + (double) lhs.m21 * (double) rhs.m10 + (double) lhs.m22 * (double) rhs.m20 + (double) lhs.m23 * (double) rhs.m30 );
			float m21 = (float) ( (double) lhs.m20 * (double) rhs.m01 + (double) lhs.m21 * (double) rhs.m11 + (double) lhs.m22 * (double) rhs.m21 + (double) lhs.m23 * (double) rhs.m31 );
			float m22 = (float) ( (double) lhs.m20 * (double) rhs.m02 + (double) lhs.m21 * (double) rhs.m12 + (double) lhs.m22 * (double) rhs.m22 + (double) lhs.m23 * (double) rhs.m32 );
			float m23 = (float) ( (double) lhs.m20 * (double) rhs.m03 + (double) lhs.m21 * (double) rhs.m13 + (double) lhs.m22 * (double) rhs.m23 + (double) lhs.m23 * (double) rhs.m33 );

			lhs.m00 = m00;
			lhs.m01 = m01;
			lhs.m02 = m02;
			lhs.m03 = m03;
			lhs.m10 = m10;
			lhs.m11 = m11;
			lhs.m12 = m12;
			lhs.m13 = m13;
			lhs.m20 = m20;
			lhs.m21 = m21;
			lhs.m22 = m22;
			lhs.m23 = m23;
		}




		/// <summary>
		/// Translate the matrix, ignoring perspective transformations.
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Translate3x4( ref this Matrix4x4 m, float x, float y )
		{
			m.m03 = (float) ( (double) m.m00 * (double) x + (double) m.m01 * (double) y + (double) m.m03 );
			m.m13 = (float) ( (double) m.m10 * (double) x + (double) m.m11 * (double) y + (double) m.m13 );
			m.m23 = (float) ( (double) m.m20 * (double) x + (double) m.m21 * (double) y + (double) m.m23 );
		}


		/// <summary>
		/// Translate the matrix, ignoring perspective transformations.
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Translate3x4( ref this Matrix4x4 m, float x, float y, float z )
		{
			m.m03 = (float) ( (double) m.m00 * (double) x + (double) m.m01 * (double) y + (double) m.m02 * (double) z + (double) m.m03 );
			m.m13 = (float) ( (double) m.m10 * (double) x + (double) m.m11 * (double) y + (double) m.m12 * (double) z + (double) m.m13 );
			m.m23 = (float) ( (double) m.m20 * (double) x + (double) m.m21 * (double) y + (double) m.m22 * (double) z + (double) m.m23 );
		}


		/// <summary>
		/// Scale the matrix, ignoring perspective transformations.
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Scale3x4( ref this Matrix4x4 m, float scaleXYZ )
		{
			m.m00 = (float) ( (double) m.m00 * (double) scaleXYZ );
			m.m01 = (float) ( (double) m.m01 * (double) scaleXYZ );
			m.m02 = (float) ( (double) m.m02 * (double) scaleXYZ );
			m.m10 = (float) ( (double) m.m10 * (double) scaleXYZ );
			m.m11 = (float) ( (double) m.m11 * (double) scaleXYZ );
			m.m12 = (float) ( (double) m.m12 * (double) scaleXYZ );
			m.m20 = (float) ( (double) m.m20 * (double) scaleXYZ );
			m.m21 = (float) ( (double) m.m21 * (double) scaleXYZ );
			m.m22 = (float) ( (double) m.m22 * (double) scaleXYZ );
		}


		/// <summary>
		/// Scale the matrix, ignoring perspective transformations.
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Scale3x4( ref this Matrix4x4 m, float scaleX, float scaleY )
		{
			m.m00 = (float) ( (double) m.m00 * (double) scaleX );
			m.m01 = (float) ( (double) m.m01 * (double) scaleY );
			m.m10 = (float) ( (double) m.m10 * (double) scaleX );
			m.m11 = (float) ( (double) m.m11 * (double) scaleY );
			m.m20 = (float) ( (double) m.m20 * (double) scaleX );
			m.m21 = (float) ( (double) m.m21 * (double) scaleY );
		}



		/// <summary>
		/// Scale the matrix, ignoring perspective transformations.
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Scale3x4( ref this Matrix4x4 m, float scaleX, float scaleY, float scaleZ )
		{
			m.m00 = (float) ( (double) m.m00 * (double) scaleX );
			m.m01 = (float) ( (double) m.m01 * (double) scaleY );
			m.m02 = (float) ( (double) m.m02 * (double) scaleZ );
			m.m10 = (float) ( (double) m.m10 * (double) scaleX );
			m.m11 = (float) ( (double) m.m11 * (double) scaleY );
			m.m12 = (float) ( (double) m.m12 * (double) scaleZ );
			m.m20 = (float) ( (double) m.m20 * (double) scaleX );
			m.m21 = (float) ( (double) m.m21 * (double) scaleY );
			m.m22 = (float) ( (double) m.m22 * (double) scaleZ );
		}


		/// <summary>
		/// Rotate the matrix around the z-axis by angle (in degrees, anti-clockwise), ignoring perspective transformations.
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Rotate3x4( ref this Matrix4x4 m, float angleZ )
		{
			angleZ *= Mathf.Deg2Rad;
			float c = Mathf.Cos( angleZ );
			float s = Mathf.Sin( angleZ );

			float m00 = (float) ( (double) m.m00 * (double)  c + (double) m.m01 * (double) s );
			float m01 = (float) ( (double) m.m00 * (double) -s + (double) m.m01 * (double) c );
			float m10 = (float) ( (double) m.m10 * (double)  c + (double) m.m11 * (double) s );
			float m11 = (float) ( (double) m.m10 * (double) -s + (double) m.m11 * (double) c );
			float m20 = (float) ( (double) m.m20 * (double)  c + (double) m.m21 * (double) s );
			float m21 = (float) ( (double) m.m20 * (double) -s + (double) m.m21 * (double) c );

			m.m00 = m00;
			m.m01 = m01;
			m.m10 = m10;
			m.m11 = m11;
			m.m20 = m20;
			m.m21 = m21;
		}
	}
}