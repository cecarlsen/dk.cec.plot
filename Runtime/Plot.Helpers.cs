/*
	Copyright © Carl Emil Carlsen 2024
	http://cec.dk
*/

using UnityEngine;

public partial class Plot
{
	
	/// <summary>
	/// Calls Object.Destroy if in Play Mode and Object.DestroyImmediate is in Edit Mode.
	/// </summary>
	public static void DestroyImmediateOrRuntime( Object o )
	{
		if( Application.isPlaying ) Object.Destroy( o );
		else Object.DestroyImmediate( o );
	}
}