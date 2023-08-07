using UnityEngine;
using TubeRendererInternals;

namespace TubeRendererExamples
{
	public class Volume : MonoBehaviour
	{
		const int POINT_COUNT = 16;
		
		TubeRenderer _outerTube, _innerTube;
		
		
		void Start()
		{	
			// Create tubes for outer and inner surface.
			_outerTube = new GameObject( "Outer Tube" ).AddComponent<TubeRenderer>();
			_innerTube = new GameObject( "Inner Tube" ).AddComponent<TubeRenderer>();
			_outerTube.transform.parent = transform;
			_innerTube.transform.parent = transform;
			
			// Optimise for realtime manipulation.
			_outerTube.MarkDynamic();
			_innerTube.MarkDynamic();
			
			// Invert the mesh of the inner tube.
			_innerTube.invertMesh = true;
			
			// Only cap the beginning of the tubes.
			_outerTube.caps = TubeRenderer.CapMode.Begin;
			_innerTube.caps = TubeRenderer.CapMode.Begin;
			
			// Create point and radius arrays.
			_outerTube.points = new Vector3[ POINT_COUNT ];
			_outerTube.radiuses = new float[ POINT_COUNT ];
			_innerTube.points = new Vector3[ POINT_COUNT+1 ];
			_innerTube.radiuses = new float[ POINT_COUNT+1 ];
			
			// Define points.
			for( int p=0; p<POINT_COUNT; p++ ){
				float norm = p / (POINT_COUNT-1f);
				_outerTube.points[p] = Vector3.right * Mathf.Lerp( 0.6f, -0.4f, norm );
				_innerTube.points[p] = _outerTube.points[p];
			}
			_innerTube.points[POINT_COUNT] = _innerTube.points[POINT_COUNT-1]; // double last point

			// Add a texutre and adjust the uv mapping of the caps.
			Texture texture = ExampleHelpers.CreateTileTexture( 12 );
			Material outerMaterial = _outerTube.GetComponent<Renderer>().sharedMaterial;
			Material innerMaterial = _innerTube.GetComponent<Renderer>().sharedMaterial;
			RenderPipelineHelpers.SetRenderPipelineDependentMainTexture( outerMaterial, texture ); // URP and HDRP compatibility.
			RenderPipelineHelpers.SetRenderPipelineDependentMainTexture( innerMaterial, texture );
			_outerTube.uvRectCap = new Rect( 0, 0, 4/12f, 4/12f );
			_innerTube.uvRectCap = _outerTube.uvRectCap;
		}
		
		
		void Update()
		{
			// Animate radiuses.
			for( int p=0; p<POINT_COUNT; p++ ){
				float norm = p / (POINT_COUNT-1f);
				float shapeRadius = Mathf.Lerp( 0.25f, 0.8f, Mathf.Pow( norm, 3 ) );
				float loudnessRadius = norm * Mathf.PerlinNoise( norm*1.5f - Time.time*12f, 0 ) * 0.2f;
				_outerTube.radiuses[p] = shapeRadius + loudnessRadius;
				_innerTube.radiuses[p] = _outerTube.radiuses[p] - 0.15f;
			}
			_innerTube.radiuses[POINT_COUNT] = _outerTube.radiuses[POINT_COUNT-1];
			
			// Overwrite radius array reference to trigger mesh updates.
			_innerTube.radiuses = _innerTube.radiuses;
			_outerTube.radiuses = _outerTube.radiuses;
		}
	}
}