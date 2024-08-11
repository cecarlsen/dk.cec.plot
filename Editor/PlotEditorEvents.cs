/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class PlotEditorEvents
{

	static PlotEditorEvents()
	{
		EditorSceneManager.sceneOpened += SceneOpenedCallback;
	}


	static void SceneOpenedCallback( Scene scene, OpenSceneMode mode )
	{
		Plot.Reset();
	}
}
