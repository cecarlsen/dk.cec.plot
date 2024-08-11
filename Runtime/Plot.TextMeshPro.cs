/*
	Copyright © Carl Emil Carlsen 2021
	http://cec.dk
*/

#if TMP_PRESENT

using System.Collections.Generic;
using UnityEngine;
using PlotInternals;
using TMPro;

public partial class Plot
{
	const float defaultTextSize = 0.1f;
	static readonly Color defaultTextColor = Color.white;
	const TextAlignmentOptions defaultTextAlignment = TextAlignmentOptions.Center;

	public partial struct Style
	{
		public float tmpFontSize;
		public TextAlignmentOptions textAlignment;
	}


	/// <summary>
	/// Set the size to be used for subsequently drawn texts in world space scale (Requires TextMeshPro).
	/// </summary>
	public static void SetTextSize( float textSize )
	{
		Instance()._style.tmpFontSize = textSize * 10; // World space text size to TMP font size.
	}


	/// <summary>
	/// Set the alignment to be used for subsequently drawn texts (Requires TextMeshPro).
	/// </summary>
	public static void SetTextAlignment( TextAlignmentOptions alignment )
	{
		Instance()._style.textAlignment = alignment;
	}



	/// <summary>
	/// Draw a text using Graphics.DrawMesh. This supports Unity's culling, and sorting.
	/// </summary>
	public static void DrawText( TextMeshPro text, float x, float y, float fieldwidth, float fieldHeight, bool drawDebugRect = false, bool applyPlotStyle = true ) { Instance().DrawTextInternal( text, x, y, fieldwidth, fieldHeight, drawDebugRect, applyPlotStyle ); }
	public static void DrawText( TextMeshPro text, Vector2 position, Vector2 fieldSize, bool drawDebugRect = false, bool applyPlotStyle = true ) { Instance().DrawTextInternal( text, position.x, position.y, fieldSize.x, fieldSize.y, drawDebugRect, applyPlotStyle ); }


	/// <summary>
	/// Draw a text immediately using Graphics.DrawMeshNow. Call this from OnPostRender or after calling BeginDrawNowToTexture.
	/// </summary>
	public static void DrawTextNow( TextMeshPro text, float x, float y, float fieldwidth, float fieldHeight, bool drawDebugRect = false, bool applyPlotStyle = true ) { Instance().DrawTextInternal( text, x, y, fieldwidth, fieldHeight, drawDebugRect, applyPlotStyle, drawNow: true ); }
	public static void DrawTextNow( TextMeshPro text, Vector2 position, Vector2 fieldSize, bool drawDebugRect = false, bool applyPlotStyle = true ) { Instance().DrawTextInternal( text, position.x, position.y, fieldSize.x, fieldSize.y, drawDebugRect, applyPlotStyle, drawNow: true ); }

	void DrawTextInternal( TextMeshPro text, float x, float y, float fieldWidth, float fieldHeight, bool drawDebugRect, bool applyPlotStyle, bool drawNow = false )
	{
		if( !text ) return;

		if( _drawingToTextureNow && !drawNow ) DebugLogDrawToTextureNowWarning();

		Matrix4x4 matrix = _matrix;
		matrix.Translate3x4( x, y );

		if( applyPlotStyle ) {
			text.color = _style.fillColor;
			text.fontSize = _style.tmpFontSize;
			text.alignment = _style.textAlignment;
			text.rectTransform.localPosition = new Vector3( x, y );
			text.rectTransform.pivot = new Vector2( ( _pivotPosition.x * 0.5f ) + 0.5f, ( _pivotPosition.y * 0.5f ) + 0.5f );
			text.rectTransform.sizeDelta = new Vector2( fieldWidth, fieldHeight ); // TODO check the performance impact of this.
		}

		if( drawNow ) {
			text.fontSharedMaterial.SetPass( 0 );
			Graphics.DrawMeshNow( text.mesh, matrix );
		} else {
			Graphics.DrawMesh( text.mesh, matrix, text.fontSharedMaterial, 0 );
		}

		if( drawDebugRect ) {
			float debugSize = Mathf.Min( fieldWidth, fieldHeight ) * 0.01f;
			PushStyle();
			PushCanvas();
			TranslateCanvas( 0, 0, -0.001f );
			SetNoFill();
			SetStrokeAlignement( StrokeAlignment.Edge );
			SetStrokeCornerProfile( StrokeCornerProfile.Hard );
			SetStrokeColor( Color.green );
			SetStrokeWidth( debugSize );
			// Draw using the current pivot.
			if( drawNow ) DrawRectNow( x, y, fieldWidth, fieldHeight );
			else DrawRect( x, y, fieldWidth, fieldHeight );
			// Then draw pivot.
			TranslateCanvas( 0, 0, -0.001f );
			SetPivot( Pivot.Center );
			SetNoStroke();
			SetFillColor( Color.red );
			DrawCircle( x, y, debugSize * 4 );
			PopCanvas();
			PopStyle();
		}
	}


	public static class TextHelper
	{
		static Font defaultFont;

		public static void AdaptTextCount( Transform parentTransform, int entryCount, List<TextMeshPro> labels, TMP_FontAsset font = null, bool hideNewObjectsInHierarchy = true )
		{
			// If the labels list count is correct, then check if it still contains labels and have fonts.
			// In the case that we have no entries, labels may still be hanging around waiting to be destroyed.
			if( entryCount != 0 && labels.Count == entryCount ) {
				bool allGood = true;
				foreach( TextMeshPro tm in labels ) {
					if( !tm || tm.font == null ) {
						allGood = false;
						break;
					}
				}
				if( allGood ) return;
			}

			// Get the labels (expensive).
			parentTransform.GetComponentsInChildren( true, labels );

			// Destroy excess.
			while( labels.Count > entryCount ) {
				Object.DestroyImmediate( labels[ labels.Count - 1 ].gameObject );
				labels.RemoveAt( labels.Count - 1 );
			}

			// Ensure existing labels has font.
			if( !font ) foreach( TextMeshPro label in labels ) if( !label.font ) label.font = TMP_Settings.defaultFontAsset;

			// Added missing.
			while( labels.Count < entryCount ) {
				TextMeshPro label = new GameObject( "Label" ).AddComponent<TextMeshPro>();
				label.renderer.enabled = false; // Plot renderes text in immediate mode, so we switch off the renderer. It would be lovely to completely deactive the object, but TextMeshPro does not allow that =(
				if( hideNewObjectsInHierarchy ) label.gameObject.hideFlags = HideFlags.HideInHierarchy;
				label.transform.SetParent( parentTransform );
				if( font ) label.font = font;
				labels.Add( label );
			}
		}


		public static TextMeshPro CreateText( Transform parentTransform, TMP_FontAsset font = null, bool hideNewObjectsInHierarchy = true )
		{
			TextMeshPro label = new GameObject( "Label" ).AddComponent<TextMeshPro>();
			label.renderer.enabled = false; // Plot renderes text in immediate mode, so we switch off the renderer. It would be lovely to completely deactive the object, but TextMeshPro does not allow that =(
			if( hideNewObjectsInHierarchy ) label.gameObject.hideFlags = HideFlags.HideInHierarchy;
			label.transform.SetParent( parentTransform );
			if( font ) label.font = font;
			return label;
		}


		public static TextAlignmentOptions OffsetToTextAlignment( Vector2 offset, float axisSnapDistance = 0.01f )
		{
			// Just something more corse than Mathf.Approximately please.
			bool zeroX = Mathf.Abs( offset.x ) < axisSnapDistance;
			bool zeroY = Mathf.Abs( offset.y ) < axisSnapDistance;

			if( zeroY ) {
				if( zeroX ) return TextAlignmentOptions.Midline;
				return offset.x > 0 ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.MidlineRight;
			}

			if( offset.y > 0 ) {
				if( zeroX ) return TextAlignmentOptions.Bottom;
				return offset.x > 0 ? TextAlignmentOptions.BottomLeft : TextAlignmentOptions.BottomRight;
			}

			if( offset.y < 0 ) {
				if( zeroX ) return TextAlignmentOptions.Top;
				return offset.x > 0 ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.TopRight;
			}

			return TextAlignmentOptions.Midline;
		}


		public static Pivot AlignmentToPivot( TextAlignmentOptions alignment )
		{
			switch( alignment ) {
				case TextAlignmentOptions.BottomLeft: return Plot.Pivot.BottomLeft;
				case TextAlignmentOptions.Left: return Plot.Pivot.Left;
				case TextAlignmentOptions.MidlineLeft: return Plot.Pivot.Left;
				case TextAlignmentOptions.TopLeft: return Plot.Pivot.TopLeft;
				case TextAlignmentOptions.Top: return Plot.Pivot.Top;
				case TextAlignmentOptions.TopRight: return Plot.Pivot.TopRight;
				case TextAlignmentOptions.Right: return Plot.Pivot.Right;
				case TextAlignmentOptions.MidlineRight: return Plot.Pivot.Right;
				case TextAlignmentOptions.BottomRight: return Plot.Pivot.BottomRight;
				case TextAlignmentOptions.Bottom: return Plot.Pivot.Bottom;
			}
			return Plot.Pivot.Center;
		}
	}

}

#endif