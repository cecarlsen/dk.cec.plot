/*
	JChColor

		J == Lightness
		C == Chroma
		h == Hue Angle

	A simplified implementation of CIECAM02 that exclude the Q, M and s dimensions.

	The code is based on an C++ implementation by Billy Biggs from 2003,
	published under a MIT-like license (see below). Biggs writes it was tested 
	against spreadsheet example calculations by Mark D. Fairchild.
	http://scanline.ca/ciecam02/

	Abbreviations
		
		CIE	-	International Commission on Illumination.
		CAM	-	Color Appearance Model.
				https://en.wikipedia.org/wiki/Color_appearance_model
		02	-	From 2002.
		CAT	-	Chromatic Adaptation Transform.
		LMS	-	LMS color space.
				Long (560–580nm) Middle (530–540nm) Short (420–440nm).
				The three types of cone cells in the human eye.
				LMS is the tristimulus values in the LMS color space.
		XYZ	-	CIE 1931 XYZ color space (and model), where Y is luminance and XZ is chromaticity.
				Device-invariant representation of color.
				A standard reference against which many other color spaces are defined.
				XYZ are analogous to, but different LMS.
				XYZ is the tristimulus values in the XYZ color space.
				https://en.wikipedia.org/wiki/CIE_1931_color_space
		D65	-	CIE Standard Illuminant D65.
				Roughly to the average midday light in Western Europe.
				Colour temperature of approximately 6500K.
				https://en.wikipedia.org/wiki/Standard_illuminant
				https://en.wikipedia.org/wiki/Illuminant_D65


	Articles on CIECAM02
	
		Wikipedia
		https://en.wikipedia.org/wiki/CIECAM02

		RawPedia
		https://rawpedia.rawtherapee.com/CIECAM02
 
		D3 CIECAM02 Color by Connor Gramazio
		https://gramaz.io/d3-cam02/


	Other implementations
		
		C++ by Billy Biggs
		http://scanline.ca/ciecam02/ciecam02.c
		
		Javascript for D3.js by Connor Gramazio (JavaScript)
		https://github.com/connorgr/d3-cam02
 

	In this version some of the computations from Billy Biggs implementation are precalculated.
		(1.0/4.0) == 0.25
		(1.0/0.9) ~= 1.111111111111111111111
		(460.0/1403.0) ~= 0.3278688524590163934426
		(220.0/1403.0) ~= 0.15680684248039914469
		(27.0/1403.0) ~= 0.01924447612259444048468
		(6300.0/1403.0) ~= 4.490377761938702779758
		(50000.0/13.0) ~= 3846.153846153846153846
		(1.0/3.6) ~= 0.2777777777777777777778
		(21.0/20.0) == 1.05 (p3)
		(21.0/20.0) * (6300.0/1403.0) ~= 4.714896650035637918746
		0.01924447612259444048468 - 4.714896650035637918746 ~= -4.695652173913043478261
		- 0.01924447612259444048468 + 4.714896650035637918746 ~= 4.695652173913043478261
		3.05 * (220.0/1403.0) ~= 0.4782608695652173913043
		3.05 * (460.0/1403.0) == 1.0


	License

		Copyright (c) 2003 Billy Biggs <vektor@dumbterm.net>
		Copyright (c) 2020 Carl Emil Carlsen <public@cec.dk> (Port to C# and modifications)
	
		Permission is hereby granted, free of charge, to any person obtaining
		a copy of this software and associated documentation files (the
		"Software"), to deal in the Software without restriction, including
		without limitation the rights to use, copy, modify, merge, publish,
		distribute, sublicense, and/or sell copies of the Software, and to
		permit persons to whom the Software is furnished to do so, subject to
		the following conditions:
		
		The above copyright notice and this permission notice shall be
		included in all copies or substantial portions of the Software.
		
		THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
		EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
		MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
		NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
		BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
		ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
		CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
		SOFTWARE.
 */

using System;
using UnityEngine;

public struct JChColor
{
	/// <summary>
	/// The J dimension, normalised (0.0-1.0).
	/// </summary>
	public float lightness;

	/// <summary>
	/// The C dimension, normalised (0.0-1.0).
	/// </summary>
	public float chroma;

	/// <summary>
	/// The h dimension, normalised (0.0-1.0).
	/// </summary>
	public float hueAngle;

	/// <summary>
	/// The alpha channel, normalised (0.0-1.0).
	/// </summary>
	public float alpha;

	/// <summary>
	/// Surround condition where 0 is a reflected surface (Average), 1 an emitting screen (Dim) and 2 an emitting video projector in a dark room (Dark).
	/// Default is 
	/// </summary>
	public float surroundCondition;

	/// <summary>
	/// The background on which the color is viewed measured in % percent gray (1-100).
	/// Default is 20%.
	/// </summary>
	public float backgroundGraytone;


	static ViewingConditions vc;

	public static JChColor white { get { return new JChColor( 1, 0, 0 ); } }
	public static JChColor black { get { return new JChColor( 0, 0, 0 ); } }


	const double tau = Mathf.PI * 2;
	const SurroundCondition defaultSurroundCondition = SurroundCondition.Dim;
	const float defaultBackgroundGraytone = 20;


	[ Serializable]
	public enum SurroundCondition
	{
		Average,	// Viewing a surface color.
		Dim,		// Viewing a screen color.
		Dark		// Viewing a video projected color in a dark room.
	}


	public JChColor( float lightness, float chroma, float hueAngle, float alpha = 1 )
	{
		surroundCondition = (int) defaultSurroundCondition;
		backgroundGraytone = defaultBackgroundGraytone;

		this.lightness = lightness;
		this.chroma = chroma;
		this.hueAngle = hueAngle;
		this.alpha = alpha;
	}


	public JChColor( Color color )
	{
		surroundCondition = (int) defaultSurroundCondition;
		backgroundGraytone = defaultBackgroundGraytone;

		if( vc == null ) vc = new ViewingConditions();
		vc.Update( surroundCondition, backgroundGraytone );

		double x, y, z;
		RGBToXYZ( color.r, color.g, color.b, out x, out y, out z );
		double J, C, h;
		CIECAM02XYZToJCh( x, y, z, out J, out C, out h );
		lightness = (float) J;
		chroma = (float) C;
		hueAngle = (float) h;
		alpha = color.a;
	}


	public void SetSurroundCondition( SurroundCondition sc )
	{
		surroundCondition = (int) sc;
	}


	public void SetSurroundCondition( float sc )
	{
		surroundCondition = sc;
	}


	/// <summary>
	/// Circular interpolation along the hue angle the cylendrical JCh color model.
	/// </summary>
	public static JChColor Slerp( JChColor c1, JChColor c2, float t )
	{
		float h = Mathf.LerpAngle( c1.hueAngle * 360, c2.hueAngle * 360, t ) / 360f;
		while( h < 0 ) h++;
		return new JChColor
		(
			Mathf.Lerp( c1.lightness, c2.lightness, t ),
			Mathf.Lerp( c1.chroma, c2.chroma, t ),
			h,
			Mathf.Lerp( c1.alpha, c2.alpha, t )
		);
	}


	/// <summary>
	/// Liniear interpolation through the cylendrical JCh color model.
	/// </summary>
	public static JChColor Lerp( JChColor c1, JChColor c2, float t )
	{
		double hueAngleRad1 = c1.hueAngle * tau;
		double hueAngleRad2 = c2.hueAngle * tau;
		double x1 = Math.Cos( hueAngleRad1 ) * c1.chroma;
		double y1 = Math.Sin( hueAngleRad1 ) * c1.chroma;
		double x2 = Math.Cos( hueAngleRad2 ) * c2.chroma;
		double y2 = Math.Sin( hueAngleRad2 ) * c2.chroma;
		double x = x1 + ( x2 - x1 ) * t;
		double y = y1 + ( y2 - y1 ) * t;
		double h = Math.Atan2( y, x ) / tau;
		while( h < 0 ) h++;
		return new JChColor
		(
			Mathf.Lerp( c1.lightness, c2.lightness, t ),
			Mathf.Sqrt( (float) ( x*x + y*y ) ),
			(float) h,
			Mathf.Lerp( c1.alpha, c2.alpha, t )
		);
	}


	/// <summary>
	/// Create a palette of colors by linear interpolation through the JCh color space.
	/// </summary>
	public static Color[] LerpCreatePalette( Color colorA, Color colorB, int stepCount )
	{
		JChColor jchColorA = new JChColor( colorA );
		JChColor jchColorB = new JChColor( colorB );
		Color[] colors = new Color[ stepCount ];
		float step = 1 / ( stepCount -1f );
		for( int i = 0; i < stepCount; i++ ) colors[ i ] = Lerp( jchColorA, jchColorB, i * step );
		return colors;
	}


	/// <summary>
	/// Create a palette of colors by circular interpolation along the hue angle through the JCh color space.
	/// </summary>
	public static Color[] SlerpCreatePalette( Color colorA, Color colorB, int stepCount )
	{
		JChColor jchColorA = new JChColor( colorA );
		JChColor jchColorB = new JChColor( colorB );
		Color[] colors = new Color[ stepCount ];
		float step = 1 / ( stepCount - 1f );
		for( int i = 0; i < stepCount; i++ ) colors[ i ] = Slerp( jchColorA, jchColorB, i * step );
		return colors;
	}


	public static implicit operator Color( JChColor c )
	{
		if( vc == null ) vc = new ViewingConditions();
		vc.Update( c.surroundCondition, c.backgroundGraytone );

		double x, y, z;
		CIECAM02JChToXYZ( c.lightness, c.chroma, c.hueAngle, out x, out y, out z );
		double r, g, b;
		XYZToRGB( x, y, z, c.lightness * 100, c.chroma * 100, out r, out g, out b );
		return new Color( (float) r, (float) g, (float) b, c.alpha );
	}


	public override string ToString()
	{
		return "(" + lightness + "," + chroma + "," + hueAngle + "," + alpha + ")";
	}


	static void XYZToRGB( double x, double y, double z, double J, double C, out double r, out double g, out double b )
	{
		// Special case for true white.
		if( J == 100 && C == 0 ) {
			r = 1; g = 1; b = 1;
			return;
		}

		// At high chroma values we start to see anomalies int the lower-left part of the hue-lightness (x,y) map.
		// This gets rid of them. (CEC 2020)
		//if( C > 23.0 ) {
		//	double c = 1.0 - C / 100.0;
		//	c = c * c * c;
		//	if( z * c < 0.007 ) {
		//		r = 0; g = 0; b = 0;
		//		return;
		//	}
		//}

		// XYZ -> RGB.
		x /= 100.0;
		y /= 100.0;
		z /= 100.0;
		r = x * 3.2404542 + y * -1.5371385 - z * 0.4985314;
		g = x * -0.9692660 + y * 1.8760108 + z * 0.0415560;
		b = x * 0.0556434 + y * -0.2040259 + z * 1.0572252;

		// RGB -> RGB.
		double exp = 1 / 2.4;
		r = r > 0.0031308 ? 1.055 * ( Math.Pow( r, exp ) ) - 0.055 : 12.92 * r;
		g = g > 0.0031308 ? 1.055 * ( Math.Pow( g, exp ) ) - 0.055 : 12.92 * g;
		b = b > 0.0031308 ? 1.055 * ( Math.Pow( b, exp ) ) - 0.055 : 12.92 * b;

		// Clamp to valid values.
		if( r > 1.0 || g > 1.0 || b > 1.0 || r < 0.0 || g < 0.0 || b < 0.0 ) {
			r = 0; g = 0; b = 0;
		}
	}


	static void RGBToXYZ( double r, double g, double b, out double x, out double y, out double z )
	{
		// sRGB -> RGB.
		r = r > 0.04045 ? Math.Pow( ( ( r + 0.055 ) / 1.055 ), 2.4 ) : ( r / 12.92 );
		g = g > 0.04045 ? Math.Pow( ( ( g + 0.055 ) / 1.055 ), 2.4 ) : ( g / 12.92 );
		b = b > 0.04045 ? Math.Pow( ( ( b + 0.055 ) / 1.055 ), 2.4 ) : ( b / 12.92 );

		// RGB -> XYZ.
		x = ( ( r * 0.4124 ) + ( g * 0.3576 ) + ( b * 0.1805 ) ) * 100.0;
		y = ( ( r * 0.2126 ) + ( g * 0.7152 ) + ( b * 0.0722 ) ) * 100.0;
		z = ( ( r * 0.0193 ) + ( g * 0.1192 ) + ( b * 0.9505 ) ) * 100.0;
	}
	
	/// <summary>
	/// Chromatic adaptation transform for CIECAT02.
	/// </summary>
	static void XYZToCAT02( out double l, out double m, out double s, double x, double y, double z )
	{
		l = ( 0.7328 * x ) + ( 0.4296 * y ) - ( 0.1624 * z );
		m = ( -0.7036 * x ) + ( 1.6975 * y ) + ( 0.0061 * z );
		s = ( 0.0030 * x ) + ( 0.0136 * y ) + ( 0.9834 * z );
	}

	/// <summary>
	/// Inverse chromatic adaptation transform for CIECAT02.
	/// </summary>
	static void CAT02ToXYZ( out double x, out double y, out double z, double l, double m, double s )
	{
		x = ( 1.096124 * l ) - ( 0.278869 * m ) + ( 0.182745 * s );
		y = ( 0.454369 * l ) + ( 0.473533 * m ) + ( 0.072098 * s );
		z = ( -0.009628 * l ) - ( 0.005698 * m ) + ( 1.015326 * s );
	}

	static void HPEToXYZ( out double x, out double y, out double z, double l, double m, double s )
	{
		x = ( 1.910197 * l ) - ( 1.112124 * m ) + ( 0.201908 * s );
		y = ( 0.370950 * l ) + ( 0.629054 * m ) - ( 0.000008 * s );
		z = s;
	}

	static void CAT02ToHPE( out double lh, out double mh, out double sh, double l, double m, double s )
	{
		lh = ( 0.7409792 * l ) + ( 0.2180250 * m ) + ( 0.0410058 * s );
		mh = ( 0.2853532 * l ) + ( 0.6242014 * m ) + ( 0.0904454 * s );
		sh = ( -0.0096280 * l ) - ( 0.0056980 * m ) + ( 1.0153260 * s );
	}


	static double NonlinearAdaptation( double c, double fl )
	{
		double p = Math.Pow( ( fl * c ) / 100.0, 0.42 );
		return ( ( 400.0 * p ) / ( 27.13 + p ) ) + 0.1;
	}


	static double InverseNonlinearAdaptation( double c, double fl )
	{
		return ( 100.0 / fl ) * Math.Pow( ( 27.13 * Math.Abs( c - 0.1 ) ) / ( 400.0 - Math.Abs( c - 0.1 ) ), 1.0 / 0.42 );
	}


	static void CIECAM02XYZToJCh( double x, double y, double z, out double J, out double C, out double h )
	{
		double l, m, s;
		double lw, mw, sw;
		double a, ca, cb;
		double et, t;
		double t1, t2;

		XYZToCAT02( out l, out m, out s, x, y, z );
		XYZToCAT02( out lw, out mw, out sw, ViewingConditions.xw, ViewingConditions.yw, ViewingConditions.zw );

		t1 = ViewingConditions.yw * vc.d;
		t2 = 1.0 - vc.d;
		l *= ( t1 / lw ) + t2;
		m *= ( t1 / mw ) + t2;
		s *= ( t1 / sw ) + t2;

		CAT02ToHPE( out l, out m, out s, l, m, s );

		l = NonlinearAdaptation( l, vc.fl );
		m = NonlinearAdaptation( m, vc.fl );
		s = NonlinearAdaptation( s, vc.fl );

		ca = l - ( 12.0 * m / 11.0 ) + ( s / 11.0 );
		cb = 0.111111111111111 * ( l + m - ( 2.0 * s ) );
		h = Math.Atan2( cb, ca );
		while( h < 0.0 ) h += tau;
		a = ( ( 2.0 * l ) + m + ( 0.05 * s ) - 0.305 ) * vc.nbb;
		J = Math.Pow( a / ViewingConditions.aw, vc.c* vc.z );
		et = 0.25 * ( Math.Cos( h + 2.0 ) + 3.8 );
		t = ( 3846.153846153846154 * vc.nc * vc.nbb * et * Math.Sqrt( ( ca * ca ) + ( cb * cb ) ) ) / ( l + m + 1.05 * s );
		C = Math.Pow( t, 0.9 ) * Math.Sqrt( J ) * Math.Pow( 1.64 - Math.Pow( 0.29, vc.n ), 0.73 );
		C *= 0.01;
		h /= tau;
	}


	static void CIECAM02JChToXYZ( double J, double C, double h, out double x, out double y, out double z )
	{
		double l, m, s;
		double lw, mw, sw;
		double a, ca, cb;
		double et, t;
		double p1, p2, p4;
		double t1, t2;

		h *= tau;
		C *= 100.0;

		t = Math.Pow( C / ( Math.Sqrt( J ) * Math.Pow( 1.64 - Math.Pow( 0.29, vc.n ), 0.73 ) ), 1.11111111111111 ); 
		et = 0.25 * ( Math.Cos( h + 2.0 ) + 3.8 );
		a = Math.Pow( J, 1.0 / ( vc.c * vc.z ) ) * ViewingConditions.aw;

		p1 = ( 3846.15384615385 * vc.nc * vc.nbb ) * et / t;
		p2 = ( a / vc.nbb ) + 0.305;
		double sHr = Math.Sin( h );
		double cHr = Math.Cos( h );
		if( Math.Abs( sHr ) >= Math.Abs( cHr ) ) {
			p4 = p1 / sHr;
			t1 = cHr / sHr;
			cb = p2 / ( p4 + 0.478260869565217 * t1 + 4.695652173913043 );
			ca = cb * t1;
		}
		else {
			p4 = p1 / cHr;
			t1 = sHr / cHr;
			ca = p2 / ( p4 + 0.478260869565217 + 4.695652173913043 * t1 );
			cb = ca * t1;
		}

		AABToCAT02LMS( out l, out m, out s, a, ca, cb, vc.nbb );

		l = InverseNonlinearAdaptation( l, vc.fl );
		m = InverseNonlinearAdaptation( m, vc.fl );
		s = InverseNonlinearAdaptation( s, vc.fl );

		HPEToXYZ( out l, out m, out s, l, m, s );
		XYZToCAT02( out l, out m, out s, l, m, s );
		XYZToCAT02( out lw, out mw, out sw, ViewingConditions.xw, ViewingConditions.yw, ViewingConditions.zw );

		t1 = ViewingConditions.yw * vc.d;
		t2 = 1.0 - vc.d;
		l /= ( t1 / lw ) + t2;
		m /= ( t1 / mw ) + t2;
		s /= ( t1 / sw ) + t2;

		CAT02ToXYZ( out x, out y, out z, l, m, s );
	}


	static void AABToCAT02LMS( out double l, out double m, out double s, double A, double aa, double bb, double nbb )
	{
		double x = ( A / nbb ) + 0.305;
		/*       c1              c2               c3       */
		l = ( 0.32787 * x ) + ( 0.32145 * aa ) + ( 0.20527 * bb );
		/*       c1              c4               c5       */
		m = ( 0.32787 * x ) - ( 0.63507 * aa ) - ( 0.18603 * bb );
		/*       c1              c6               c7       */
		s = ( 0.32787 * x ) - ( 0.15681 * aa ) - ( 4.49038 * bb );
	}


	class ViewingConditions
	{
		/// <summary>
		/// Luminance of the adapting field in cd/m^2.
		/// Billy Biggs recommends setting  this to 4 for sRGB input (D65 whitepoint).
		/// However, in d3 I noticed they use 64 lux as stated in the usage guidelines for CIECAM97 (Moroney 2000).
		/// </summary>
		const double la = 4.07436654315252; // 64.0 / PI / 5.0

		// D65 white point values https://en.wikipedia.org/wiki/Illuminant_D65
		public const double xw = 95.047;
		public const double yw = 100.0;
		public const double zw = 108.883;
		public readonly double fl; // Values that only depend on the above.

		/// <summary>
		/// Luminamce factor for background, measured in percent gray.
		/// Billy Biggs recommends setting this to 20% for sRGB input.
		/// </summary>
		double yb;

		/// <summary>
		/// Background luminous factor.
		/// </summary>
		public double n;

		public double z, nbb; // Note that in Billy Bigg's code we also have 'ncb', but with exact same value as 'nbb'.

		/// <summary>
		/// Achromatic response to white.
		/// </summary>
		public static double aw;

		/// <summary>
		/// Adaptation.
		/// Theoretically, D ranges from
		///		0 = no adaptation to the adopted white point, to
		///		1 = complete adaptation to the adopted white point.
		/// In practice, the minimum D value will not be less than 0.65 for a
		/// dark surround and exponentially converges to 1 for average surrounds
		/// with increasingly large values of la.
		/// </summary>
		public double d;

		/// <summary>
		/// The f, c and nc parameters control the surround. CIECAM02 uses
		/// these values for average ( relative luminance > 20% of scene white),
		/// dim( between 0% and 20%), and dark(0%). In general, use average
		/// for both input and output.
		/// </summary>
		public double f;	// Factor determining degree of adaptation
		public double c;	// Impact of surrounding
		public double nc;	// Chromatic induction factor

		public readonly Vector3[] surroundDefinitions;


		public ViewingConditions()
		{
			double k = 1.0 / ( ( 5.0 * la ) + 1.0 );
			k *= k; k *= k; // pow( k, 4 )
			fl = 0.2 * k * ( 5.0 * la ) + 0.1 * ( Math.Pow( ( 1.0 - k ), 2.0 ) ) * ( Math.Pow( ( 5.0 * la ), 0.333333333333333 ) );

			surroundDefinitions = new Vector3[]{
				new Vector3( 1.0f, 0.690f, 1.00f ), // Average
				new Vector3( 0.9f, 0.590f, 0.95f ), // Dim
				new Vector3( 0.8f, 0.525f, 0.80f ), // Dark
			};

			Update( (int) SurroundCondition.Average, 20 ); // NOTE:  Billy Biggs recommends using use Average in general for both input and output.
		}


		public void Update( float surround, float backgroundGraytone )
		{
			Vector3 v;
			if( surround <= 0 ) {
				v = surroundDefinitions[ 0 ];
			} else if( surround >= 2 ) {
				v = surroundDefinitions[ 2 ];
			} else {
				int i0 = (int) surround;
				float t = surround - i0;
				v = Vector3.Lerp( surroundDefinitions[ i0 ], surroundDefinitions[ i0 + 1 ], t );
			}

			f = v.x;
			c = v.y;
			nc = v.z;

			yb = backgroundGraytone < 1 ? 1 : backgroundGraytone;

			UpdateAmbientIlluminaion();
		}


		void UpdateAmbientIlluminaion()
		{
			double t1, t2;
			n = yb / yw;
			z = 1.48 + Math.Pow( n, 0.5 );
			nbb = 0.725 * Math.Pow( ( 1.0 / n ), 0.2 );
			d = f * ( 1.0 - ( 0.277777777777778 * Math.Exp( ( -la -42.0 ) / 92.0 ) ) );
			double r, g, b;
			XYZToCAT02( out r, out g, out b, xw, yw, zw );
			t1 = yw * d;
			t2 = 1.0 - d;
			r *= ( t1 / r ) + t2;
			g *= ( t1 / g ) + t2;
			b *= ( t1 / b ) + t2;
			CAT02ToHPE( out r, out g, out b, r, g, b );
			r = NonlinearAdaptation( r, fl );
			g = NonlinearAdaptation( g, fl );
			b = NonlinearAdaptation( b, fl );
			aw = ( ( 2.0 * r ) + g + ( 0.05 * b ) - 0.305 ) * nbb;
		}
	};
}