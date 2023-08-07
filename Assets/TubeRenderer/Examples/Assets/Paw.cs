using UnityEngine;
using TubeRendererInternals;


namespace TubeRendererExamples
{
	public class Paw : MonoBehaviour
	{
		const int TUBE_COUNT = 5;
		const int POINT_COUNT = 24;
		const float RADIUS = 1;
		
		TubeRenderer _tube;
		
		
		void Start()
		{
			// Add TubeRenderer component.
			_tube = gameObject.AddComponent<TubeRenderer>();
			
			// Optimise for realtime manipulation.
			_tube.MarkDynamic();
			
			// No caps please.
			_tube.caps = TubeRenderer.CapMode.None;
			
			// Create point and radius arrays.
			_tube.points = new Vector3[POINT_COUNT];
			_tube.radiuses = new float[POINT_COUNT];
			
			// Define radiuses.
			for( int p=0; p<POINT_COUNT; p++ ){
				float norm = p / (POINT_COUNT-1f);
				_tube.radiuses[p] = Mathf.Cos( norm * Mathf.PI * 0.5f ) * 0.4f;
			}

			// Create tiled texture and assign it to the tube.
			Material material = _tube.GetComponent<Renderer>().sharedMaterial;
			Texture texture = ExampleHelpers.CreateTileTexture( 12 );
			RenderPipelineHelpers.SetRenderPipelineDependentMainTexture( material, texture ); // URP and HDRP compatibility.
			
			// Position.
			_tube.transform.position = -Vector3.forward * RADIUS;
			
			// Create a bunch of other objects and share the tube mesh.
			for( int t=1; t<TUBE_COUNT; t++ ){
				float angle = ( t / (float) TUBE_COUNT ) * 360;
				GameObject cloneTube = new GameObject( "Clone Tube" );
				cloneTube.transform.rotation = Quaternion.Euler( 0, angle, 0 );
				cloneTube.transform.position = cloneTube.transform.rotation * -Vector3.forward * RADIUS;
				cloneTube.AddComponent<MeshFilter>().sharedMesh = _tube.mesh;
				cloneTube.AddComponent<MeshRenderer>().sharedMaterial = _tube.GetComponent<Renderer>().sharedMaterial;
			}
		}
		
		
		void Update()
		{
			// Rotate forward angle slowly.
			_tube.forwardAngleOffset += Time.deltaTime * 60;

			// Calculate an animated angle.
			float angleStep = Mathf.Lerp( -0.1f, 0.25f, Mathf.Pow( Mathf.Sin( Time.time*0.1f + Mathf.Sin(Time.time*0.8f)*Mathf.PI )*0.5f+0.5f, 1.6f ) );
			
			// Update points.
			for( int p=1; p<POINT_COUNT; p++ ){
				float stepLength = Mathf.Lerp( 0.1f, 0.001f, p / (POINT_COUNT-1f) );
				float angle = angleStep * p;
				_tube.points[p] = _tube.points[p-1] + new Vector3( 0, Mathf.Cos( angle ), Mathf.Sin( angle ) ) * stepLength;
			}
			
			// Overwrite point array reference to trigger mesh update.
			_tube.points = _tube.points;
		}
	}
}