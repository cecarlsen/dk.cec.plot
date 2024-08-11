/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk

	Unity, for fucks sake! Can you pleaes add package dependency support for asset store products already?
	The following seems to be the least shitty solution at the time of writing (2020).

	Proposed solution on the forum:
	https://docs.unity3d.com/ScriptReference/PlayerSettings.SetScriptingDefineSymbolsForGroup.html?_ga=2.85921565.566277820.1606491957-28833553.1571684762
*/

using UnityEngine;
using UnityEngine.UI;

namespace PlotInternals
{
	[ExecuteInEditMode]
	public class ImportTMProOrDefineSymbolNotice : MonoBehaviour
	{
		Image _image;

		void Update()
		{
			// Get image component.
			if( !_image ) _image = GetComponentInChildren<Image>( true );
			if( !_image ) return;

			// In player, destroy this notice.
			if( !Application.isEditor ) {
				Destroy( gameObject );
				Destroy( this );
				return; 
			}

			// If TMP is present, then hide notice.
	#if TMP_PRESENT
			if( _image.gameObject.activeSelf ) _image.gameObject.SetActive( false );
	#else
			if( !_image.gameObject.activeSelf ) _image.gameObject.SetActive( true );
	#endif
		}
	}
}