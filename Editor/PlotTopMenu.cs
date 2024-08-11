/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

using UnityEngine;
using UnityEditor;

public static class PlotTopMenu
{
	const string tmpDefineSymbol = "TMP_PRESENT";
	const string logPrepend = "<b>[" + nameof( Plot ) + "]</b> ";


	[MenuItem( "Tools/Plot/TextMeshPro/Add " + tmpDefineSymbol + " Define Symbol" )]
	static void AddDefineSymbol()
	{
		string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup( BuildTargetGroup.Standalone );
		if( defines.Contains( tmpDefineSymbol ) ) {
			Debug.Log( logPrepend + "Could not add " + tmpDefineSymbol + " to your define symbols in your Player Settings because it is aldready added.\n" );
			return;
		}

		defines += ";" + tmpDefineSymbol;
		PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.Standalone, defines );
		AssetDatabase.Refresh();


		Debug.Log( logPrepend + "Added " + tmpDefineSymbol + " to your define symbols in your Player Settings.\n" );
	}


	[MenuItem( "Tools/Plot/TextMeshPro/Remove " + tmpDefineSymbol + " Define Symbol" )]
	static void RemoveDefineSymbol()
	{
		string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup( BuildTargetGroup.Standalone );
		if( !defines.Contains( tmpDefineSymbol ) ) {
			Debug.Log( logPrepend + "Could not remove " + tmpDefineSymbol + " from your define symbols in your Player Settings because it is not there.\n" );
			return;
		}

		defines = defines.Replace( tmpDefineSymbol, string.Empty );
		PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.Standalone, defines );
		AssetDatabase.Refresh();

		Debug.Log( logPrepend + "Removed " + tmpDefineSymbol + " from your define symbols in your Player Settings.\n" );
	}
}