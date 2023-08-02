using UnityEngine;
using TubeRendererInternals;

namespace TubeRendererExamples
{
	public class Skin : MonoBehaviour
	{
		const int POINT_COUNT = 64;

		Transform[] _bones;

		
		void Start()
		{
			// Add a TubeRenderer component.
			TubeRenderer tube = gameObject.AddComponent<TubeRenderer>();
			
			// Define uv mapping.
			tube.uvRectCap = new Rect( 0, 0, 4/12f, 4/12f );
			
			// Create point array.
			tube.points = new Vector3[ POINT_COUNT ];
			
			// Define points.
			for( int p=0; p<POINT_COUNT; p++ ){
				float norm = p / (POINT_COUNT-1f);
				float x = Mathf.Lerp( 0.5f, -0.5f, norm );
				tube.points[p] = new Vector3( x, 0, 0 );
			}
			
			// Generate the mesh immediately.
			tube.ForceUpdate();

			// Create bones.
			_bones = new Transform[2];
			_bones[0] = new GameObject( "Base Bone" ).transform;
			_bones[1] = new GameObject( "End Bone" ).transform;
			_bones[0].position = tube.points[0];
			_bones[1].position = tube.points[POINT_COUNT-1];
			foreach( Transform b in _bones ) b.parent = transform;

			// Create and add bones weight information to mesh.
			// We will interpolate from bone0 (base) to bone1 (tail) along the length of the tube.
			// See 'Digestion' example for information about vertex order.
			BoneWeight[] boneWeights = new BoneWeight[tube.mesh.vertexCount];
			int v = 0;
			// First, the tube.
			for( int p=0; p<POINT_COUNT; p++ ){
				float pNorm = p / (POINT_COUNT-1f);
				for( int e=0; e<tube.edgeCount+1; e++ ){ // + 1 for hidden uv wrapping vertex
					boneWeights[v].boneIndex0 = 0;
					boneWeights[v].boneIndex1 = 1;
					boneWeights[v].weight0 = 1 - pNorm;
					boneWeights[v].weight1 = pNorm;
					v++;
				}
			}
			// Then the begin cap.
			for( int e=0; e<tube.edgeCount+2; e++ ){ // + 2 for hidden uv wrapping vertex and center vertex
				boneWeights[v].boneIndex0 = 0;
				boneWeights[v].boneIndex1 = 1;
				boneWeights[v].weight0 = 1;
				boneWeights[v].weight1 = 0;
				v++;
			}
			// Then the end cap.
			for( int e=0; e<tube.edgeCount+2; e++ ){ // ditto
				boneWeights[v].boneIndex0 = 0;
				boneWeights[v].boneIndex1 = 1;
				boneWeights[v].weight0 = 0;
				boneWeights[v].weight1 = 1;
				v++;
			}
			tube.mesh.boneWeights = boneWeights;

			// Create and add bindpose information to mesh.
			// The bind pose is "the inverse of inverse transformation matrix of the bone".
			Matrix4x4[] bindposes = new Matrix4x4[2];
			bindposes[0] = _bones[0].worldToLocalMatrix * transform.localToWorldMatrix;
			bindposes[1] = _bones[1].worldToLocalMatrix * transform.localToWorldMatrix;
			tube.mesh.bindposes = bindposes;

			// Get the mesh before we ...
			Mesh mesh = tube.mesh;
			
			// Destroy the components that are no longer needed.
			DestroyImmediate( tube ); // Immediate, to ensure that tube is destroyed before MeshRenderer (which it depends on).
			Destroy( GetComponent<MeshFilter>() );
			Destroy( GetComponent<MeshRenderer>() );

			// Add skin and apply the goods.
			SkinnedMeshRenderer skin = gameObject.AddComponent<SkinnedMeshRenderer>();
			skin.sharedMesh = mesh;
			skin.bones = _bones;
			skin.quality = SkinQuality.Bone2; // this is apparently very important for procedural skinned meshes

			// Create a tiled texture and apply it.
			Texture texture = ExampleHelpers.CreateTileTexture( 12 );
			Material material = skin.GetComponent<Renderer>().sharedMaterial;
			RenderPipelineHelpers.SetRenderPipelineDependentMainTexture( material, texture );
		}


		void Update()
		{
			// Create animated values.
			float angleOffset = Mathf.Sin( Time.time * Mathf.PI ) * 40;
			float scaleMult = Mathf.Sin( Mathf.PI * 0.5f + Time.time * Mathf.PI * 2 ) * 0.5f + 0.5f;

			// Wiggle the tail bone.
			_bones[1].localRotation = Quaternion.AngleAxis( angleOffset, Vector3.up );
			_bones[1].localScale = Vector3.one * Mathf.Lerp( 0.8f, 1.5f, scaleMult );

			// Mirror base bone.
			_bones[0].localRotation = Quaternion.AngleAxis( -angleOffset, Vector3.up );
			_bones[0].localScale = Vector3.one * Mathf.Lerp( 1.5f, 0.8f, scaleMult );
		}
	}
	
}