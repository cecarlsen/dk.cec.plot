/*
	Copyright © Carl Emil Carlsen 2020-2024
	http://cec.dk
*/

using UnityEngine;
using UnityEngine.Rendering;

namespace PlotInternals
{
	public class LinePRenderer : PRenderer
	{
		public Mesh mesh;

		static class ShaderIDs
		{
			public static readonly int _Data = Shader.PropertyToID( nameof( _Data ) );
		}

		public LinePRenderer( bool antialias, Plot.Blend blend ) : base ( "Line", antialias, blend )
		{
			CreateMesh();
		}


		public void Render
		(
			float ax, float ay, float bx, float by, Plot.StrokeCap beginCap, Plot.StrokeCap endCap,
			bool drawNow, Matrix4x4 matrix, ref Plot.Style style // Note that style is passed by reference for performance reasons, it is not changed.
		){
			float strokeExtents = style.strokeWidth * 0.5f;
			Vector2 towardsB = new Vector2( bx - ax, by - ay );
			float length = towardsB.magnitude;
			float xCenter = length * 0.5f;
			if( beginCap != Plot.StrokeCap.None && endCap != Plot.StrokeCap.None ) {
				length += style.strokeWidth;
			} else if( beginCap == Plot.StrokeCap.Round || beginCap == Plot.StrokeCap.Square ) {
				length += strokeExtents;
				xCenter -= strokeExtents * 0.5f;
			} else if( endCap == Plot.StrokeCap.Round || endCap == Plot.StrokeCap.Square ) {
				length += strokeExtents;
				xCenter += strokeExtents * 0.5f;
			}
			float meshExtentsX = length * 0.5f;
			float meshExtentsY = strokeExtents;
			float angle = Mathf.Atan2( towardsB.y, towardsB.x ) * Mathf.Rad2Deg; // Atan2, ouch! But necessary if we want Unity to frustum cull lines.
			
			if( ax != 0 || ay != 0 ) matrix.Translate3x4( ax, ay );
			if( angle != 0 ) matrix.Rotate3x4( angle );
			if( xCenter != 0 ) matrix.Translate3x4( xCenter, 0 );
			if( meshExtentsX != 1 || meshExtentsY != 1 ) matrix.Scale3x4( meshExtentsX, meshExtentsY );

			EnsureAvailableMaterialBeforeSubmission( drawNow );

			if( isStrokeColorDirty ) UpdateStrokeColor( style.hasVisibleStroke ? style.strokeColor : ColorWithAlpha( style.fillColor, 0 ), drawNow );

			Vector4 data = new Vector4( meshExtentsX, meshExtentsY, beginCap == Plot.StrokeCap.Round ? 1 : 0, endCap == Plot.StrokeCap.Round ? 1 : 0 );

			if( drawNow ) {
				_material.SetVector( ShaderIDs._Data, data );
				_material.SetPass( 0 );
				Graphics.DrawMeshNow( mesh, matrix );
			} else {
				_propBlock.SetVector( ShaderIDs._Data, data );
				Graphics.DrawMesh( mesh, matrix, _material, style.layer, null, 0, _propBlock, false, false, false );
			}
		}


		void CreateMesh()
		{
			const int vertexCount = 4;
			const int indexCount = 4;

			mesh = new Mesh();
			mesh.name = "Line";
			mesh.hideFlags = HideFlags.HideAndDontSave;

			// Verticies.
			VertexAttributeDescriptor[] vertexDataLayout = new VertexAttributeDescriptor[] {
				new VertexAttributeDescriptor( VertexAttribute.Position, VertexAttributeFormat.Float32, 2 )
			};
			SubMeshDescriptor meshDescriptor = new SubMeshDescriptor() {
				vertexCount = vertexCount,
				indexCount = indexCount,
				topology = MeshTopology.Quads,
				bounds = new Bounds( Vector3.zero, Vector3.one * 2 )
			};
			const MeshUpdateFlags meshFlags = MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices;
			mesh.SetVertexBufferParams( vertexCount, vertexDataLayout );
			mesh.SetIndexBufferParams( indexCount, IndexFormat.UInt16 );
			mesh.subMeshCount = 1;
			mesh.SetSubMesh( 0, meshDescriptor, meshFlags );

			mesh.SetVertexBufferData(
				new Vector2[]{
					new Vector2( -1, -1 ),
					new Vector2( -1,  1 ),
					new Vector2(  1,  1 ),
					new Vector2(  1, -1 ),
				},
				0, 0, vertexCount, 0,
				meshFlags
			);
			mesh.SetIndexBufferData( new ushort[] { 0, 1, 2, 3 }, 0, 0, indexCount, meshFlags );
			mesh.bounds = meshDescriptor.bounds;

			mesh.UploadMeshData( true );
		}
	}
}