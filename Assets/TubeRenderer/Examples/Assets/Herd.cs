using UnityEngine;
using TubeRendererInternals;

namespace TubeRendererExamples
{
	public class Herd : MonoBehaviour
	{
		const int CRITTER_COUNT = 30;
		const int POINT_COUNT = 40;
		const float POINT_SPACING = 0.05f;
		const float DISPERSION = 0.3f;
		static readonly Vector3 MOVEMENT_RANGE = new Vector3( 5, 3, 5 );

		Critter[] _critters;
		

		void Start()
		{
			// Since we will be animating in a frame dependent (and cheap) fashion, we cap the frame rate.
			Application.targetFrameRate = 60;

			// Create tiled texture.
			Texture2D texture = ExampleHelpers.CreateTileTexture( 12 );
			
			// Create critters.
			_critters = new Critter[ CRITTER_COUNT ];
			for( int s = 0; s < CRITTER_COUNT; s++ ){
				Critter critter = new Critter();
				critter.tube.transform.parent = gameObject.transform;
				Material material = critter.tube.GetComponent<Renderer>().sharedMaterial;
				RenderPipelineHelpers.SetRenderPipelineDependentMainTexture( material, texture ); // URP and HDRP compatibility.
				_critters[ s ] = critter;
			}
		}
		
		
		void Update()
		{
			// Update all critters.
			foreach( Critter critter in _critters ) critter.Update();
		}
		
		
		class Critter
		{
			public TubeRenderer tube;
			
			
			public Critter()
			{
				// Create game object and add TubeRenderer component.
				tube = new GameObject( "Critter" ).AddComponent<TubeRenderer>();
				
				// Optimise for realtime manipulation.
				tube.MarkDynamic();

				// Define uv mapping for the end caps.
				tube.uvRectCap = new Rect( 0, 0, 4/12f, 4/12f );
				
				// Define points and radiuses.
				tube.points = new Vector3[ POINT_COUNT ];
				tube.radiuses = new float[ POINT_COUNT ];
				for( int p = 0; p < POINT_COUNT; p++ ){
					tube.points[p] = SmoothRandom( - p * POINT_SPACING, MOVEMENT_RANGE );
					tube.radiuses[p] = Mathf.Lerp( 0.2f, 0.05f, p/(POINT_COUNT-1f ) );
				}
			}
			
			
			public void Update()
			{
				// Calculate new position and store it in the beginning of the tube.
				tube.points[0] = SmoothRandom( Time.time * 0.2f, MOVEMENT_RANGE );

				// Perform cheap inverse kinematics.
				for( int p=1; p<tube.points.Length; p++ ){
					Vector3 forward = tube.points[p] - tube.points[p-1];
					forward.Normalize();
					forward *= POINT_SPACING;
					tube.points[p] = tube.points[p-1] + forward;
				}

				// Overwrite point array reference to trigger mesh update.
				tube.points = tube.points;
			}
			
			
			// Cheapish frequency modulation noise.
			Vector3 SmoothRandom( float t, Vector3 scale )
			{
				Random.InitState( tube.GetInstanceID() ); // Different random values for each critter.
				float x = Mathf.Sin( ( Random.value*DISPERSION + Mathf.PI * Mathf.Sin( Random.value*DISPERSION + Mathf.PI * Mathf.Sin( Random.value*DISPERSION + t * 0.51f ) ) ) ) * scale.x;
				float y = Mathf.Sin( ( Random.value*DISPERSION + Mathf.PI * Mathf.Sin( Random.value*DISPERSION + Mathf.PI * Mathf.Sin( Random.value*DISPERSION + t * 0.78f ) ) ) ) * scale.y;
				float z = Mathf.Sin( ( Random.value*DISPERSION + Mathf.PI * Mathf.Sin( Random.value*DISPERSION + Mathf.PI * Mathf.Sin( Random.value*DISPERSION + t * 0.28f) ) ) ) * scale.z;
				return new Vector3( x, y, z );
			}
		}
	}
}