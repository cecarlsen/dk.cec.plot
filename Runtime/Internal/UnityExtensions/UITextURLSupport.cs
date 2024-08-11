/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace PlotInternals
{
	public class UITextURLSupport : MonoBehaviour, IPointerClickHandler
	{
		void IPointerClickHandler.OnPointerClick( PointerEventData eventData )
		{
			Text text = GetComponent<Text>();
			string clickedWord = UITextUtilities.FindIntersectingWord( text, eventData.position, Camera.current );

			//Debug.Log( clickedWord + " " + UITextUtilities.HasLinkText( clickedWord ) );

			if( !clickedWord.Contains( "href" ) ) return;

			clickedWord = clickedWord.Substring( 0, clickedWord.LastIndexOf( "\"") );
			int firstQuouteIndex = clickedWord.IndexOf( "\"" );
			clickedWord = clickedWord.Substring( firstQuouteIndex+1, clickedWord.Length - firstQuouteIndex - 1 );

			if( !string.IsNullOrEmpty( clickedWord ) && UITextUtilities.HasLinkText( clickedWord ) ) {
				string actualUrl = UITextUtilities.RemoveHtmlLikeTags( clickedWord );
				Application.OpenURL( actualUrl );
			}
		}
	}
}