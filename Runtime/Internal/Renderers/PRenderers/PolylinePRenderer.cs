/*
	Copyright © Carl Emil Carlsen 2020-2024
	http://cec.dk
*/

using UnityEngine;

namespace PlotInternals
{
	public class PolylinePRenderer : PRenderer
	{
		static class ShaderIDs
		{
			public static readonly int _StrokeExtents = Shader.PropertyToID( nameof( _StrokeExtents ) );
		}

		public PolylinePRenderer( bool antialias, Plot.Blend blend ) : base ( "Polyline", antialias, blend )
		{
			
		}


		public void Render
		(
			Plot.Polyline polyline, Plot.StrokeCap beginCap, Plot.StrokeCap endCap,
			bool drawNow, ref Matrix4x4 matrix, ref Plot.Style style // Note that matrix and style are passed by reference for performance reasons, they are not changed.
		){
			float strokeExtents = style.strokeWidth * 0.5f;

			Mesh mesh = polyline.AdaptAndGetMesh( drawNow, beginCap, endCap, style.strokeCornerProfile );

			EnsureAvailableMaterialBeforeSubmission( drawNow );

			if( isStrokeColorDirty ) UpdateStrokeColor( style.strokeEnabled ? style.strokeColor : ColorWithAlpha( style.fillColor, 0 ), drawNow );

			if( drawNow ) {
				_material.SetFloat( ShaderIDs._StrokeExtents, strokeExtents );
				_material.SetPass( 0 );
				Graphics.DrawMeshNow( mesh, matrix );
			} else {
				_propBlock.SetFloat( ShaderIDs._StrokeExtents, strokeExtents );
				Graphics.DrawMesh( mesh, matrix, _material, style.layer, null, 0, _propBlock, false, false, false );
			}
		}
	}
}