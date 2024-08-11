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
	public static class Vector4Extensions
	{
		/// <summary>
		/// Set x and y component only.
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetXY( ref this Vector4 v, float x, float y )
		{
			v.x = x;
			v.y = y;
		}


		/// <summary>
		/// Set z and w component only.
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetZW( ref this Vector4 v, float z, float w )
		{
			v.z = z;
			v.w = w;
		}


		/// <summary>
		/// Add vector b to this vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Add( ref this Vector4 v, Vector4 b )
		{
			v.x += b.x;
			v.y += b.y;
			v.z += b.z;
			v.w += b.w;
		}


		/// <summary>
		/// Add value to all components of this vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Add( ref this Vector4 v, float value )
		{
			v.x += value;
			v.y += value;
			v.z += value;
			v.w += value;
		}


		/// <summary>
		/// Add two vectors and assign the result to this vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Added( ref this Vector4 v, Vector4 a, Vector4 b )
		{
			v.x = a.x + b.x;
			v.y = a.y + b.y;
			v.z = a.z + b.z;
			v.w = a.w + b.w;
		}


		/// <summary>
		/// Add value to all components of a vector and assign the result to this vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Added( ref this Vector4 v, Vector4 a, float value )
		{
			v.x = a.x + value;
			v.y = a.y + value;
			v.z = a.z + value;
			v.w = a.w + value;
		}


		/// <summary>
		/// Subtract a vector from this vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Subtract( ref this Vector4 v, Vector4 b )
		{
			v.x -= b.x;
			v.y -= b.y;
			v.z -= b.z;
			v.w -= b.w;
		}


		/// <summary>
		/// Subtract value a from all components of this vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Subtract( ref this Vector4 v, float value )
		{
			v.x -= value;
			v.y -= value;
			v.z -= value;
			v.w -= value;
		}


		/// <summary>
		/// Subtract vector b from vector a and assign the result to this vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Subtracted( ref this Vector4 v, Vector4 a, Vector4 b )
		{
			v.x = a.x - b.x;
			v.y = a.y - b.y;
			v.z = a.z - b.z;
			v.w = a.w - b.w;
		}


		/// <summary>
		/// Subtract value b from all components of vector a and assign the result to this vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Subtracted( ref this Vector4 v, Vector4 a, float value )
		{
			v.x = a.x - value;
			v.y = a.y - value;
			v.z = a.z - value;
			v.w = a.w - value;
		}


		/// <summary>
		/// Subtract this vector from a vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SubtractSwapped( ref this Vector4 v, Vector4 b )
		{
			v.x = b.x - v.x;
			v.y = b.y - v.y;
			v.z = b.z - v.z;
			v.w = b.w - v.w;
		}


		/// <summary>
		/// Subtract this vector from a value. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SubtractSwapped( ref this Vector4 v, float value )
		{
			v.x = value - v.x;
			v.y = value - v.y;
			v.z = value - v.z;
			v.w = value - v.w;
		}


		/// <summary>
		/// Multiply this vector by a vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Multiply( ref this Vector4 v, Vector4 b )
		{
			v.x *= b.x;
			v.y *= b.y;
			v.z *= b.z;
			v.w *= b.w;
		}


		/// <summary>
		/// Multiply this vector by a value. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Multiply( ref this Vector4 v, float value )
		{
			v.x *= value;
			v.y *= value;
			v.z *= value;
			v.w *= value;
		}


		/// <summary>
		/// Multiply two vectors and assign the result to this vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Multiplied( ref this Vector4 v, Vector4 a, Vector4 b )
		{
			v.x = a.x * b.x;
			v.y = a.y * b.y;
			v.z = a.z * b.z;
			v.w = a.w * b.w;
		}


		/// <summary>
		/// Multiply a vector by a value and assign the result to this vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Multiplied( ref this Vector4 v, Vector4 a, float value )
		{
			v.x = a.x * value;
			v.y = a.y * value;
			v.z = a.z * value;
			v.w = a.w * value;
		}


		/// <summary>
		/// Devide this vector by a denominator. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Divide( ref this Vector4 v, Vector4 denominator )
		{
			v.x /= denominator.x;
			v.y /= denominator.y;
			v.z /= denominator.z;
			v.w /= denominator.w;
		}


		/// <summary>
		/// Devide this vector by a denominator. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Divide( ref this Vector4 v, float denominator )
		{
			v.x /= denominator;
			v.y /= denominator;
			v.z /= denominator;
			v.w /= denominator;
		}


		/// <summary>
		/// Divide a vector by a denominator and assign the result to this vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Divided( ref this Vector4 v, Vector4 a, Vector4 denominator )
		{
			v.x = a.x / denominator.x;
			v.y = a.y / denominator.y;
			v.z = a.z / denominator.z;
			v.w = a.w / denominator.w;
		}


		/// <summary>
		/// Divide a vector by a denominator and assign the result to this vector. Faster than the default operator (Unity 2020.1).
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Divided( ref this Vector4 v, Vector4 a, float denominator )
		{
			v.x = a.x / denominator;
			v.y = a.y / denominator;
			v.z = a.z / denominator;
			v.w = a.w / denominator;
		}

	}
}