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
	public static class Vector2Extensions
	{
		/// <summary>
		/// Returns true if vector is almost zero.
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool IsAlmostZero( this Vector2 v, float precision = 0.000001f )
		{
			return
				v.x > -precision && v.x < precision &&
				v.y > -precision && v.y < precision;
		}

		/// <summary>
		/// Add vector b to this vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Add( ref this Vector2 v, Vector2 b )
		{
			v.x += b.x;
			v.y += b.y;
		}


		/// <summary>
		/// Add value to all components of this vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Add( ref this Vector2 v, float value )
		{
			v.x += value;
			v.y += value;
		}


		/// <summary>
		/// Add two vectors and assign the result to this vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Added( ref this Vector2 v, Vector2 a, Vector2 b )
		{
			v.x = a.x + b.x;
			v.y = a.y + b.y;
		}


		/// <summary>
		/// Add value to all components of a vector and assign the result to this vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Added( ref this Vector2 v, Vector2 a, float value )
		{
			v.x = a.x + value;
			v.y = a.y + value;
		}


		/// <summary>
		/// Subtract a vector from this vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Subtract( ref this Vector2 v, Vector2 b )
		{
			v.x -= b.x;
			v.y -= b.y;
		}


		/// <summary>
		/// Subtract value a from all components of this vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Subtract( ref this Vector2 v, float value )
		{
			v.x -= value;
			v.y -= value;
		}


		/// <summary>
		/// Subtract vector b from vector a and assign the result to this vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Subtracted( ref this Vector2 v, Vector2 a, Vector2 b )
		{
			v.x = a.x - b.x;
			v.y = a.y - b.y;
		}


		/// <summary>
		/// Subtract value b from all components of vector a and assign the result to this vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Subtracted( ref this Vector2 v, Vector2 a, float value )
		{
			v.x = a.x - value;
			v.y = a.y - value;
		}


		/// <summary>
		/// Subtract this vector from a vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SubtractSwapped( ref this Vector2 v, Vector2 b )
		{
			v.x = b.x - v.x;
			v.y = b.y - v.y;
		}


		/// <summary>
		/// Subtract this vector from a value. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SubtractSwapped( ref this Vector2 v, float value )
		{
			v.x = value - v.x;
			v.y = value - v.y;
		}


		/// <summary>
		/// Multiply this vector by a vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Multiply( ref this Vector2 v, Vector2 b )
		{
			v.x *= b.x;
			v.y *= b.y;
		}


		/// <summary>
		/// Multiply this vector by a value. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Multiply( ref this Vector2 v, float value )
		{
			v.x *= value;
			v.y *= value;
		}


		/// <summary>
		/// Multiply two vectors and assign the result to this vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Multiplied( ref this Vector2 v, Vector2 a, Vector2 b )
		{
			v.x = a.x * b.x;
			v.y = a.y * b.y;
		}


		/// <summary>
		/// Multiply a vector by a value and assign the result to this vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Multiplied( ref this Vector2 v, Vector2 a, float value )
		{
			v.x = a.x * value;
			v.y = a.y * value;
		}


		/// <summary>
		/// Devide this vector by a denominator. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Divide( ref this Vector2 v, Vector2 denominator )
		{
			v.x /= denominator.x;
			v.y /= denominator.y;
		}


		/// <summary>
		/// Devide this vector by a denominator. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Divide( ref this Vector2 v, float denominator )
		{
			v.x /= denominator;
			v.y /= denominator;
		}


		/// <summary>
		/// Divide a vector by a denominator and assign the result to this vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Divided( ref this Vector2 v, Vector2 a, Vector2 denominator )
		{
			v.x = a.x / denominator.x;
			v.y = a.y / denominator.y;
		}


		/// <summary>
		/// Divide a vector by a denominator and assign the result to this vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Divided( ref this Vector2 v, Vector2 a, float denominator )
		{
			v.x = a.x / denominator;
			v.y = a.y / denominator;
		}
	}
}