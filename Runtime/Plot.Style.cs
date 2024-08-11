/*
	Copyright © Carl Emil Carlsen 2021
	http://cec.dk
*/

using System;
using UnityEngine;

public partial class Plot
{
	#pragma warning disable CS0282
	[Serializable]
	public partial struct Style
	{
		// Notes to self:
		//	1) We want this struct to be as light weight as possible because it will be copied a lot. Store no temporary computations.
		//  2) This class is public to users, to don't put any rendering related stuff here.

		public StrokeAlignment strokeAlignment;
		public StrokeCornerProfile strokeCornerProfile;
		public Color fillColor;
		public Color strokeColor;
		public float strokeWidth; // Always stored in meters
		public Pivot pivot;
		public Vector4 fillTextureST;
		public Color fillTextureTint;

		/// <summary>
		/// Does not work for DrawNow methods, just like Graphics.DrawMeshNow not regarding layers.
		/// </summary>
		public int layer;

		// These are "features" (depends on multi compiled shader)
		public bool antialias;
		public Blend blend;
		public Texture fillTexture;
		public FillTextureBlend fillTextureBlend;

		public bool fillEnabled { get { return fillColor.a > 0; } }
		public bool strokeEnabled { get { return strokeColor.a > 0 && strokeWidth > 0; } }
		public bool fillOrStrokeEnabled { get { return fillColor.a > 0 || ( strokeColor.a > 0 && strokeWidth > 0 ); } }
		public bool textureEnabled { get { return fillTexture && fillColor.a > 0; } }

		public static Style GetDefault()
		{
			Style s = new Style();
			s.antialias = defaultAntialias;
			s.blend = defaultBlend;
			s.fillColor = defaultFillColor;
			s.strokeColor = defaultStrokeColor;
			s.strokeWidth = defaultStrokeWidth; // Always stored in meters
			s.strokeAlignment = defaultStrokeAlignment;
			s.strokeCornerProfile = defaultStrokeCornerProfile;
			s.pivot = defaultPivot;
			s.fillTextureST = new Vector4( 1, 1, 0, 0 );
			s.fillTextureBlend = defaultFillTextureBlend;
			s.fillTextureTint = defaultFillTextureTint;
#if TMP_PRESENT
			s.tmpFontSize = defaultTextSize;
			s.textAlignment = defaultTextAlignment;
#endif
			return s;
		}

		public override string ToString()
		{
			return
				"Style {\n" +
				"\tstrokeAlignment: " + strokeAlignment + "\n" +
				"\tstrokeCornerProfile: " + strokeCornerProfile + "\n" +
				"\tfillColor: " + fillColor + "\n" +
				"\tstrokeColor: " + strokeColor + "\n" +
				"\tstrokeWidth: " + strokeWidth + "\n" +
				"\tpivot: " + pivot + "\n" +
				"\tfillTextureST: " + fillTextureST + "\n" +
				"\tlayer: " + layer + "\n" +
				"\tantialias: " + antialias + "\n" +
				"\tblend: " + blend + "\n" +
				"\tfillTexture: " + fillTexture + "\n" +
				"\tfillTextureBlend: " + fillTextureBlend + "\n" +
				"\tfillEnabled: " + fillEnabled + "\n" +
				"\tstrokeEnabled: " + strokeEnabled + "\n" +
				"\ttextureEnabled: " + textureEnabled + "\n" +
				"}";
		}
	}
}