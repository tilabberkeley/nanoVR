using UnityEngine;
using TubeRendererInternals;

namespace TubeRendererExamples
{
	public class Digestion : MonoBehaviour
	{
		const int POINT_COUNT = 61;
		const float DISTORTION_OFFSET = 0.7f;

		TubeRenderer _tube;
		float[] _normalizedRadiuses;
		
		
		void Start()
		{
			// Add TubeRendeder component.
			_tube = gameObject.AddComponent<TubeRenderer>();

			// Optimise for realtime manipulation.
			_tube.MarkDynamic();
			
			// Create point and radius arrays.
			_tube.points = new Vector3[POINT_COUNT];
			_tube.radiuses = new float[POINT_COUNT];
			
			// Define points.
			for( int p=0; p<POINT_COUNT; p++ ){
				float norm = p / (POINT_COUNT-1f);
				float x = Mathf.Lerp( -1.5f, 1.5f, norm );
				float y = Mathf.Lerp( -1f, 1f, Mathf.PerlinNoise( 0, norm ) );
				float z = Mathf.Lerp( -1f, 1f, Mathf.PerlinNoise( norm*2, 0 ) );
				_tube.points[p] = new Vector3( x, y, z );
			}
			
			// Set a texture and a uv mapping.
			Material material = _tube.GetComponent<Renderer>().sharedMaterial;
			Texture texture = ExampleHelpers.CreateTileTexture( 12 );
			RenderPipelineHelpers.SetRenderPipelineDependentMainTexture( material, texture );
			_tube.uvRect = new Rect( 0, 0, 6, 1 );
			_tube.uvRectCap = new Rect( 0, 0, 4/12f, 4/12f );
			
			// Create an array to hold animated radiuses.
			_normalizedRadiuses = new float[ POINT_COUNT ];
			
			// Enable post processing by assigning a callback method.
			_tube.AddPostprocess( Distort );
			
			// Display mesh gizmos for debugging. this is convinient when you write your own post process method.
			_tube.showMeshGizmos = true;
		}
		
		
		void Update()
		{
			// Animate radiuses.
			for( int p=0; p<POINT_COUNT; p++ ){
				float norm = p / (POINT_COUNT-1f);
				_normalizedRadiuses[p] = Mathf.Max( 0, Mathf.Lerp( -0.5f, 1f, Mathf.PerlinNoise( 0, -norm*2 + Time.time*0.5f ) ) );
				_tube.radiuses[p] = Mathf.Lerp( 0.05f, 0.3f, _normalizedRadiuses[p] );
			}
			
			// Overwrite radius array reference to trigger mesh update.
			_tube.radiuses = _tube.radiuses;

			// Change normal mode every third second.
			_tube.normalMode = (TubeRenderer.NormalMode) ( Mathf.FloorToInt( Time.time / 3f ) % 3 );
		}
		
		
		// This method will be called by TubeRenderer, just before the mesh data is uploaded.
		void Distort( Vector3[] vertices, Vector3[] normals, Vector4[] tangents )
		{
			int v = 0;
			Random.InitState( 0 ); // use repetative random lookup

			// So this is where is gets a bit hairy...
			// The vertices are placed in this order: 1) tube, 2) begin cap, and 3) end cap.
			// Caps are mapped in order 1) circle vertices and 2) center point.
			// Tube vertices are mapped differently depending on the 'normalMode'.

			switch( _tube.normalMode ){

			case TubeRenderer.NormalMode.Smooth:

				// SMOOTH normal mode:

				// For every point (including last point) every edge will have a vertex, plus an extra vertex for uv wrapping.
				for( int p=0; p<_tube.points.Length; p++ ){
					for( int e=0; e<_tube.edgeCount; e++ ){
						// Add random offset to vertex along the direction of it's normal.
						vertices[v] += Mathf.Pow( _normalizedRadiuses[p], 2 ) * normals[v] * DISTORTION_OFFSET * Random.value;
						v++;
					}
					// Copy to uv wrapping.
					vertices[v] = vertices[v-_tube.edgeCount];
					v++;
				}
				break;

			case TubeRenderer.NormalMode.Hard:

				// HARD normal mode:

				// For every point that is not the last point every edge will have a quad.
				for( int p=0; p<_tube.points.Length-1; p++ ){
					for( int e=0; e<_tube.edgeCount; e++ ){
						for( int q=0; q<4; q++ ){
							// Add random offset to vertex along the direction of it's normal.
							vertices[v] += Mathf.Pow( _normalizedRadiuses[p], 2 ) * normals[v] * DISTORTION_OFFSET * Random.value;
							v++;
						}
					}
				}

				break;

			case TubeRenderer.NormalMode.HardEdges:
				
				// HARD EDGES normal mode:

				// For every point every edge will have two vertices.
				for( int p=0; p<_tube.points.Length; p++ ){
					for( int e=0; e<_tube.edgeCount; e++ ){
						for( int i=0; i<2; i++ ){
							// Add random offset to vertex along the direction of it's normal.
							vertices[v] += Mathf.Pow( _normalizedRadiuses[p], 2 ) * normals[v] * DISTORTION_OFFSET * Random.value;
							v++;
						}
					}
				}
				break;
			}

			// Add random offset to the vertices of the begin cap along their normals.
			if( _tube.caps == TubeRenderer.CapMode.Both || _tube.caps == TubeRenderer.CapMode.Begin ){
				for( int e=0; e<_tube.edgeCount+2; e++ ){
					vertices[v] += Mathf.Pow( _normalizedRadiuses[0], 2 ) * normals[v] * DISTORTION_OFFSET * Random.value;
					v++;
				}
			}
			// Add random offset to the vertices of the end cap along their normals.
			if( _tube.caps == TubeRenderer.CapMode.Both || _tube.caps == TubeRenderer.CapMode.End ){
				for( int e=0; e<_tube.edgeCount+2; e++ ){
					vertices[v] += Mathf.Pow( _normalizedRadiuses[_tube.points.Length-1], 2 ) * normals[v] * DISTORTION_OFFSET * Random.value;
					v++;
				}
			}
		}
	}
}