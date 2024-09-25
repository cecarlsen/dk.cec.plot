/*
	Copyright © Carl Emil Carlsen 2021-2024
	http://cec.dk

	Notes to self:
		1) We want this struct to be as light weight as possible because it will be copied a lot. Store no temporary computations.
		2) This class is public to users, so don't put any rendering related stuff here.
*/

using System;
using UnityEngine;
using TMPro;

public partial class Plot
{
	#pragma warning disable CS0282

	/// <summary>
	/// Style holds attributes that are applied when shape instances are submitted for rendering.
	/// </summary>
	[Serializable]
	public struct Style
	{
		/// <summary>
		/// Toggle fill visibility state of this style.
		/// Default is true.
		/// </summary>
		public bool fillEnabled;

		/// <summary>
		/// Toggle stroke visibility state of this style.
		/// Default is true.
		/// </summary>
		public bool strokeEnabled;

		/// <summary>
		/// Fill color of this style (the color inside shapes).
		/// Default is Color.white.
		/// </summary>
		public Color fillColor;

		/// <summary>
		/// Stroke color of this style (the color of outlines and lines).
		/// Default is Color.black.
		/// </summary>
		public Color strokeColor;

		/// <summary>
		/// Stroke width of this style (the thickness of outlines and lines).
		/// Default is 0.05f.
		/// </summary>
		public float strokeWidth;

		/// <summary>
		/// Stroke alignment of this style (the alignment of outlines relative to the edge of shapes).
		/// Default is StrokeAlignment.Outside.
		/// </summary>
		public StrokeAlignment strokeAlignment;

		/// <summary>
		/// Stroke coner profile of this style (the corner sharpness of Pie, Arch, Rect, Polygon, and Polyline).
		/// Default is StrokeCornerProfile.Round.
		/// </summary>
		public StrokeCornerProfile strokeCornerProfile;


		public Pivot pivot;
		public Vector4 fillTextureST;
		public Color fillTextureTint;
		public float textSize;
		public TextAlignmentOptions textAlignment;
		public TMP_FontAsset textFont;

		/// <summary>
		/// Does not work for DrawNow methods, just like Graphics.DrawMeshNow are not regarding layers.
		/// </summary>
		public int layer;

		// These are "features" (depends on multi compiled shader)
		public bool antialias;
		public Blend blend;
		public Texture fillTexture;
		public FillTextureBlend fillTextureBlend;

		public bool hasVisibleFill => fillEnabled && fillColor.a > 0;
		public bool hasVisibleStroke => strokeEnabled && strokeColor.a > 0 && strokeWidth > 0;
		public bool hasVisibleFillOrStroke => hasVisibleFill || hasVisibleStroke;
		public bool hasVisibleTextureEnabled => hasVisibleFill && fillTexture && fillTextureTint.a > 0f;


		public static Style GetDefault()
		{
			return new Style()
			{
				fillEnabled = true,
				strokeEnabled = true,
				fillColor = defaultFillColor,
				strokeColor = defaultStrokeColor,
				strokeWidth = defaultStrokeWidth,
				strokeAlignment = defaultStrokeAlignment,
				strokeCornerProfile = defaultStrokeCornerProfile,
				pivot = defaultPivot,
				fillTextureST = new Vector4( 1, 1, 0, 0 ),
				fillTextureTint = defaultFillTextureTint,
				textSize = defaultTextSize,
				textAlignment = defaultTextAlignment,
				textFont = null,
				layer = 0,
				antialias = defaultAntialias,
				blend = defaultBlend,
				fillTexture = null,
				fillTextureBlend = defaultFillTextureBlend,
			};
		}


		public override string ToString()
		{
			return
				$@"Style {{\n
				\fillEnabled: {fillEnabled}\n
				\strokeEnabled: {strokeEnabled}\n
				\tfillColor: {fillColor}\n
				\tstrokeColor: {strokeColor}\n
				\tstrokeWidth: {strokeWidth}\n
				\tstrokeAlignment: {strokeAlignment}\n
				\tstrokeCornerProfile: strokeCornerProfile\n
				\tpivot: {pivot}\n
				\tfillTextureST: {fillTextureST}\n
				\tfillTextureTint: {fillTextureTint}\n
				\ttextSize: {textSize}\n
				\ttextAlignment: {textAlignment}\n
				\ttextFont: {textFont}\n
				\tlayer: {layer}\n
				\tantialias: {antialias}\n
				\tblend: {blend}\n
				\tfillTexture: {fillTexture}\n
				\tfillTextureBlend: {fillTextureBlend}\n
				\thasVisibleFill {{get;}}: {hasVisibleFill}\n
				\thasVisibleStroke {{get;}}: {hasVisibleStroke}\n
				\thasVisibleTextureEnabled {{get;}}: {hasVisibleTextureEnabled}\n
			}}";
		}
	}
}