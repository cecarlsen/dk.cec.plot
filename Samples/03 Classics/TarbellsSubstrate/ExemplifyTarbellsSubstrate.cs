/*
	Substrate Watercolor
	j.tarbell   June, 2004
	Albuquerque, New Mexico
	complexification.net

	http://www.complexification.net/gallery/machines/substrate/

	Ported to Unity and Plot by Carl Emil Carlsen 2021.
*/


using UnityEngine;
using UnityEngine.Events;
using static Plot;

namespace PlotExamples
{
	public class ExemplifyTarbellsSubstrate : MonoBehaviour
	{
		public Vector2Int dim = new Vector2Int( 1024, 1024 );
		public Texture2D colorPaletteTexture = null;
		public bool drawWaterColor = true;
		public UnityEvent<RenderTexture> textureOut;

		RenderTexture _rt;

		// grid of cracks
		const int maxnum = 100;
		int _num = 0;
		int[] _cgrid;
		Crack[] _cracks;

		// color parameters
		const int maxpal = 512;
		int _numpal = 0;
		Color32[] _goodcolor = new Color32[ maxpal ];

		// sand painters
		SandPainter[] _sands;


		// MAIN METHODS ---------------------------------------------

		void Awake()
		{
			_rt = new RenderTexture( dim.x, dim.y, 0 );
			_rt.name = nameof( ExemplifyTarbellsSubstrate );
			_rt.Create();
			textureOut.Invoke( _rt );

			Background( Color.white );

			Takecolor( colorPaletteTexture );

			_cgrid = new int[ dim.x * dim.y ];
			_cracks = new Crack[ maxnum ];

			Begin();
		}


		void OnDestroy()
		{
			_rt.Release();
		}


		void Update()
		{
			BeginDrawNowToTexture( _rt );
			SetNoStroke();

			// crack all cracks
			for( int n = 0; n < _num; n++ ) _cracks[ n ].Move( this );

			EndDrawNowToTexture();
		}


		// METHODS --------------------------------------------------


		void MakeCrack()
		{
			if( _num < maxnum ) {
				// make a new crack instance
				_cracks[ _num ] = new Crack( this );
				_num++;
			}
		}


		void Begin()
		{
			// erase crack grid
			for( int y = 0; y < dim.y; y++ ) {
				for( int x = 0; x < dim.x; x++ ) {
					_cgrid[ y * dim.x + x ] = 10001;
				}
			}
			// make random crack seeds
			for( int k = 0; k < 16; k++ ) {
				int i = Random.Range( 0, dim.x * dim.y - 1 );
				_cgrid[ i ] = Random.Range( 0, 360 );
			}

			// make just three cracks
			_num = 0;
			for( int k = 0; k < 3; k++ ) {
				MakeCrack();
			}

			Background( Color.white );
		}


		void Background( Color color )
		{
			BeginDrawNowToTexture( _rt, color );
			EndDrawNowToTexture();
		}


		// COLOR METHODS ----------------------------------------------------------------

		Color32 SomeColor()
		{
			// pick some random good color
			return _goodcolor[ Random.Range( 0, _numpal ) ];
		}

		void Takecolor( Texture2D tex )
		{
			Color32[] pixels = tex.GetPixels32();
			for( int x = 0; x < tex.width; x++ ) {
				for( int y = 0; y < tex.height; y++ ) {
					Color32 c = pixels[ y * tex.width + y ];
					bool exists = false;
					for( int n = 0; n < _numpal; n++ ) {
						if( c.Equals( _goodcolor[ n ] ) ) {
							exists = true;
							break;
						}
					}
					if( !exists ) {
						// add color to pal
						if( _numpal < maxpal ) {
							_goodcolor[ _numpal ] = c;
							_numpal++;
						}
					}
				}
			}
		}


		// OBJECTS -------------------------------------------------------

		class Crack
		{
			float _x, _y;
			float _t;    // direction of travel in degrees

			// sand painter
			SandPainter _sp;

			public Crack( ExemplifyTarbellsSubstrate s )
			{
				// find placement along existing crack
				FindStart( s );
				_sp = new SandPainter( s );
			}

			void FindStart( ExemplifyTarbellsSubstrate s )
			{
				// pick random point
				int px = 0;
				int py = 0;

				// shift until crack is found
				bool found = false;
				int timeout = 0;
				while( ( !found ) || ( timeout++ > 1000 ) ) {
					px = Random.Range( 0, s.dim.x );
					py = Random.Range( 0, s.dim.y );
					if( s._cgrid[ py * s.dim.x + px ] < 10000 ) {
						found = true;
					}
				}

				if( found ) {
					// start crack
					int a = s._cgrid[ py * s.dim.x + px ];
					if( Random.value < 0.5f ) {
						a -= 90 + (int) Random.Range( -2, 2.1f );
					} else {
						a += 90 + (int) Random.Range( -2, 2.1f );
					}
					StartCrack( px, py, a );
				} else {
					Debug.Log("timeout: "+timeout);
				}
			}

			void StartCrack( int X, int Y, int T )
			{
				_x = X;
				_y = Y;
				_t = T;//%360;
				_x += 0.61f * Mathf.Cos( _t * Mathf.Deg2Rad );
				_y += 0.61f * Mathf.Sin( _t * Mathf.Deg2Rad );
			}

			public void Move( ExemplifyTarbellsSubstrate s )
			{
				// continue cracking
				_x += 0.42f * Mathf.Cos( _t * Mathf.Deg2Rad );
				_y += 0.42f * Mathf.Sin( _t * Mathf.Deg2Rad );

				// bound check
				const float z = 0.33f;
				int cx = (int) ( _x + Random.Range( -z, z ) );  // add fuzz
				int cy = (int) ( _y + Random.Range( -z, z ) );

				// draw sand painter
				if( s.drawWaterColor ) RegionColor( s );

				// draw black crack
				float paintX = _x + Random.Range( -z, z );
				float paintY = _y + Random.Range( -z, z );
				SetFillColor( Color.black );
				DrawCircleNow( paintX, paintY, 1f );

				if( ( cx >= 0 ) && ( cx < s.dim.x ) && ( cy >= 0 ) && ( cy < s.dim.y ) ) {
					// safe to check
					if( ( s._cgrid[ cy * s.dim.x + cx ] > 10000 ) || ( Mathf.Abs( s._cgrid[ cy * s.dim.x + cx ] - _t ) < 5 ) ) {
						// continue cracking
						s._cgrid[ cy * s.dim.x + cx ] = (int) _t;
					} else if( Mathf.Abs( s._cgrid[ cy * s.dim.x + cx ] - _t ) > 2 ) {
						// crack encountered (not self), stop cracking
						FindStart( s );
						s.MakeCrack();
					}
				} else {
					// out of bounds, stop cracking
					FindStart( s );
					s.MakeCrack();
				}
			}

		
			void RegionColor( ExemplifyTarbellsSubstrate s )
			{
				// start checking one step away
				float rx = _x;
				float ry = _y;
				bool openspace = true;
		
				// find extents of open space
				while( openspace ) {
					// move perpendicular to crack
					rx += 0.81f * Mathf.Sin( _t * Mathf.Deg2Rad );
					ry -= 0.81f * Mathf.Cos( _t * Mathf.Deg2Rad );
					int cx = (int) rx;
					int cy = (int) ry;
					if( ( cx >= 0 ) && ( cx < s.dim.x ) && ( cy >= 0 ) && ( cy < s.dim.y ) ) {
						// safe to check
						if( s._cgrid[ cy * s.dim.x + cx ] > 10000 ) {
							// space is open
						} else {
							openspace = false;
						}
					} else {
						openspace = false;
					}
				}
				// draw sand painter
				_sp.Render( rx, ry, _x, _y );
			}
		}


		class SandPainter
		{
			Color c;
			float g;

			public SandPainter( ExemplifyTarbellsSubstrate s )
			{

				c = s.SomeColor();
				g = Random.Range( 0.01f, 0.1f );
			}

			public void Render( float x, float y, float ox, float oy )
			{
				// modulate gain
				g += Random.Range( -0.050f, 0.050f );
				const float maxg = 1f;
				if( g < 0 ) g = 0;
				if( g > maxg ) g = maxg;

				// calculate grains by distance
				//int grains = int(sqrt((ox-x)*(ox-x)+(oy-y)*(oy-y)));
				int grains = 64;

				// lay down grains of sand (transparent pixels)
				float w = g / ( grains - 1f );
				for( int i = 0; i < grains; i++ ) {
					float a = 0.1f - i / ( grains * 10f );
					SetFillColor( c, a );
					DrawCircleNow( 
						ox + ( x - ox ) * Mathf.Sin( Mathf.Sin( i * w ) ),
						oy + ( y - oy ) * Mathf.Sin( Mathf.Sin( i * w ) ),
						1f
					);
				}
			}
		}
	}

}