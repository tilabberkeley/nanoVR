using UnityEngine;
using TubeRendererInternals;

namespace TubeRendererExamples
{
	public class Flower : MonoBehaviour
	{
		const int HAIR_COUNT = 800;
		const int POINT_COUNT = 6;
		const float HAIR_RADIUS = 0.045f; 
		
		
		void Start()
		{
			// Create an array of CombineInstances to hold tube meshes temporarily.
			CombineInstance[] combineInstances = new CombineInstance[ HAIR_COUNT ];
			
			// Temporary variables for calculating positions.
			float revolutions = 0;
			float positionRadius = HAIR_RADIUS;
			
			// Create tubes.
			for( int h = 0; h < HAIR_COUNT; h++ )
			{
				// Create game object and add TubeRenderer component.
				TubeRenderer tube = new GameObject().AddComponent<TubeRenderer>();
				
				// Position object.
				revolutions += (HAIR_RADIUS*2) / ( positionRadius * Mathf.PI * 2 );
				positionRadius = revolutions * (HAIR_RADIUS*2);
				tube.transform.position = Quaternion.Euler( 0, revolutions*360, 0 ) * Vector3.forward * positionRadius;
				
				// No caps please.
				tube.caps = TubeRenderer.CapMode.None;
				
				// Toggle between two different uv mappings.
				tube.uvRect = h%2 == 0 ? new Rect( 0.01f, 0.01f, 0.48f, 0.48f ) : new Rect( 0, 0.51f, 0.48f, 0.48f );
				
				// Create points and radiuses arrays.
				tube.points = new Vector3[ POINT_COUNT ];
				tube.radiuses = new float[ POINT_COUNT ];
				
				// Calculate height.
				float hairNorm = h / (HAIR_COUNT-1f);
				float height = Mathf.Pow( 1-hairNorm, 0.8f ) * 0.5f;
				
				// Define points and radiuses.
				tube.points[0] = Vector3.zero;
				tube.radiuses[0] = HAIR_RADIUS;
				for( int p = 1; p < POINT_COUNT; p++ ){
					float norm = (p-1) / (POINT_COUNT-2f);
					float angle = norm*Mathf.PI*0.5f;
					tube.radiuses[ p ] = Mathf.Cos( angle ) * HAIR_RADIUS;
					float y = height + Mathf.Sin( angle ) * HAIR_RADIUS;
					tube.points[ p ] = new Vector3( 0, y, 0 );
				}
				
				// Force update to generate the tube mesh immediately.
				tube.ForceUpdate();
				
				// Add the tube mesh to the combine instances.
				combineInstances[h].mesh = tube.mesh;
				combineInstances[h].transform = tube.transform.localToWorldMatrix;
				
				// Destroy the TubeRenderer we just used to build the mesh.
				Destroy( tube.gameObject );
			}
			
			// Add mesh rendering components and combine tubes.
			gameObject.AddComponent<MeshRenderer>();
			MeshFilter filter = gameObject.AddComponent<MeshFilter>();
			filter.mesh.CombineMeshes( combineInstances );
			
			// Add a material and a texture.
			Material material = RenderPipelineHelpers.CreateRenderPipelineCompatibleMaterial();
			GetComponent<Renderer>().sharedMaterial = material;
			RenderPipelineHelpers.SetRenderPipelineDependentMainTexture( material, ExampleHelpers.CreateTileTexture( 2 ) );
		}
	}
}