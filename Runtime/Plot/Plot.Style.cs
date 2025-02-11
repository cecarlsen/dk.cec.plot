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
		/// Toggle fill visibility state.
		/// Default is true.
		/// </summary>
		public bool fillEnabled;

		/// <summary>
		/// Toggle stroke visibility state.
		/// Default is true.
		/// </summary>
		public bool strokeEnabled;

		/// <summary>
		/// Fill color (the color inside shapes).
		/// Default is Color.white.
		/// </summary>
		public Color fillColor;

		/// <summary>
		/// Stroke color (the color of outlines and lines).
		/// Default is Color.black.
		/// </summary>
		public Color strokeColor;

		/// <summary>
		/// Stroke width (the thickness of outlines and lines).
		/// Default is 0.05f.
		/// </summary>
		public float strokeWidth;

		/// <summary>
		/// Stroke alignment (the alignment of outlines relative to the edge of shapes).
		/// Default is StrokeAlignment.Outside.
		/// </summary>
		public StrokeAlignment strokeAlignment;

		/// <summary>
		/// Stroke coner profile (the corner sharpness of Pie, Arch, Rect, Polygon, and Polyline).
		/// Default is StrokeCornerProfile.Round.
		/// </summary>
		public StrokeCornerProfile strokeCornerProfile;

		/// <summary>
		/// Pivot point (the local zero point on shapes).
		/// Default is Pivot.Center.
		/// </summary>
		public Pivot pivot;

		/// <summary>
		/// The UV rect for textures set using SetFillTexture(). Parameters are ( x, y, width, height ).
		/// Default is ( 0, 0, 1, 1 ).
		/// </summary>
		public Vector4 fillTextureUVRect;

		/// <summary>
		/// The color tint for textures set using SetFillTexture().
		/// Default is Color.white.
		/// </summary>
		public Color fillTextureTint;

		/// <summary>
		/// The text size in Unity meter units.
		/// Default is 0.1f.
		/// </summary>
		public float textSize;

		/// <summary>
		/// The horizontal and vertical text alignment.
		/// Default is TextAlignmentOptions.Center.
		/// </summary>
		public TextAlignmentOptions textAlignment;

		/// <summary>
		/// The text font.
		/// </summary>
		public TMP_FontAsset textFont;

		/// <summary>
		/// Does not work for DrawNow methods, just like Graphics.DrawMeshNow are not regarding layers.
		/// Default is 0, Unity's default layer.
		/// </summary>
		public int layer;

		/// <summary>
		/// The amount of feather to apply to stroke where 0.0 is hard and 1.0 is soft.
		/// Default is -1 (disabled). Also see FeatherMode.
		/// </summary>
		public float strokeFeather;


		// The following are "features" (depends on multi compiled shader)

		/// <summary>
		/// Toggle the antialiasing of shapes. This works independently from Unity's antialiasing.
		/// Default is true.
		/// </summary>
		public bool antialias;

		/// <summary>
		/// The blend mode.
		/// Default is Blend.Transparent.
		/// </summary>
		public Blend blend;

		/// <summary>
		/// The texture to be filled inside shapes.
		/// </summary>
		public Texture fillTexture;

		/// <summary>
		/// The blend of the fill texture onto the fill color.
		/// Default is FillTextureBlend.Overlay.
		/// </summary>
		public FillTextureBlend fillTextureBlend;

		/// <summary>
		/// Feather mode.
		/// Default is All.
		/// </summary>
		//public FeatherMode featherMode;


		static readonly Color defaultFillColor = Color.white;
		static readonly Color defaultStrokeColor = Color.black;
		const float defaultStrokeWidth = 0.05f;
		const StrokeAlignment defaultStrokeAlignment = StrokeAlignment.Outside;
		const bool defaultAntialias = true;
		const Pivot defaultPivot = Pivot.Center;
		const StrokeCornerProfile defaultStrokeCornerProfile = StrokeCornerProfile.Round;
		const Blend defaultBlend = Blend.Transparent;
		const FillTextureBlend defaultFillTextureBlend = FillTextureBlend.Overlay;
		static readonly Vector4 fillTextureUVRectDefault = new Vector4( 0f, 0f, 1f, 1f );
		static readonly Color defaultFillTextureTint = Color.white;
		const float defaultTextSize = 0.1f;
		const TextAlignmentOptions defaultTextAlignment = TextAlignmentOptions.Center;

		// Undocumented on purpose.
		public bool hasVisibleFill =>
			fillEnabled && ( fillColor.a > 0 || 
			( fillTextureTint.a > 0 && (
				fillTextureBlend == FillTextureBlend.Overlay || 
				fillTextureBlend == FillTextureBlend.Replace
			)));
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
				fillTextureUVRect = fillTextureUVRectDefault,
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
				\tfillTextureST: {fillTextureUVRect}\n
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