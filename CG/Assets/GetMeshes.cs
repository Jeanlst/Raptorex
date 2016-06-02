using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GetMeshes : MonoBehaviour {

	// Use this for initialization
	void Start () {
		/*Vector3 position = transform.position;
		transform.position = Vector3.zero;
		Quaternion rotation = transform.rotation;
		transform.rotation = Quaternion.identity;
		Vector3 scale = transform.localScale;
		transform.localScale = Vector3.zero;

		MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
		CombineInstance[] combine = new CombineInstance[meshFilters.Length];
		for (int i = 0; i < meshFilters.Length; i++) {
			combine [i].mesh = meshFilters [i].sharedMesh;
			combine [i].transform = meshFilters [i].transform.localToWorldMatrix;
			meshFilters [i].GetComponent<Renderer> ().enabled = false;
		}
		GetComponent<MeshFilter> ().mesh = new Mesh ();
		GetComponent<MeshFilter> ().mesh.CombineMeshes (combine, true, true);

		transform.position = position;
		transform.rotation = rotation;
		transform.localScale = scale;

		gameObject.AddComponent<MeshCollider> ();*/
		CombineMeshes (gameObject);
		gameObject.AddComponent<MeshCollider> ();
	}

	public Mesh CombineMeshes(GameObject aGo) {
		MeshRenderer[] meshRenderers = aGo.GetComponentsInChildren<MeshRenderer>(false);
		int totalVertexCount = 0;
		int totalMeshCount = 0;

		if(meshRenderers != null && meshRenderers.Length > 0) {
			foreach(MeshRenderer meshRenderer in meshRenderers) {
				MeshFilter filter = meshRenderer.gameObject.GetComponent<MeshFilter>();
				if(filter != null && filter.sharedMesh != null) {
					totalVertexCount += filter.sharedMesh.vertexCount;
					totalMeshCount++;
				}
			}
		}

		if(totalMeshCount == 0) {
			Debug.Log("No meshes found in children. There's nothing to combine.");
			return null;
		}
		if(totalMeshCount == 1) {
			Debug.Log("Only 1 mesh found in children. There's nothing to combine.");
			return null;
		}
		if(totalVertexCount > 65535) {
			Debug.Log("There are too many vertices to combine into 1 mesh ("+totalVertexCount+"). The max. limit is 65535");
			return null;
		}

		Mesh mesh = new Mesh();
		Matrix4x4 myTransform = aGo.transform.worldToLocalMatrix;
		List<Vector3> vertices = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();
		List<Vector2> uv1s = new List<Vector2>();
		List<Vector2> uv2s = new List<Vector2>();
		Dictionary<Material, List<int>> subMeshes = new Dictionary<Material, List<int>>();

		if(meshRenderers != null && meshRenderers.Length > 0) {
			foreach(MeshRenderer meshRenderer in meshRenderers) {
				MeshFilter filter = meshRenderer.gameObject.GetComponent<MeshFilter>();
				if(filter != null && filter.sharedMesh != null) {
					MergeMeshInto(filter.sharedMesh, meshRenderer.sharedMaterials, myTransform * filter.transform.localToWorldMatrix, vertices, normals, uv1s, uv2s, subMeshes);
					if(filter.gameObject != aGo) {
						filter.gameObject.SetActive(false);
					}
				}
			}
		}

		mesh.vertices = vertices.ToArray();
		if(normals.Count>0) mesh.normals = normals.ToArray();
		if(uv1s.Count>0) mesh.uv = uv1s.ToArray();
		if(uv2s.Count>0) mesh.uv2 = uv2s.ToArray();
		mesh.subMeshCount = subMeshes.Keys.Count;
		Material[] materials = new Material[subMeshes.Keys.Count];
		int mIdx = 0;
		foreach(Material m in subMeshes.Keys) {
			materials[mIdx] = m;
			mesh.SetTriangles(subMeshes[m].ToArray(), mIdx++);
		}

		if(meshRenderers != null && meshRenderers.Length > 0) {
			MeshRenderer meshRend = aGo.GetComponent<MeshRenderer>();
			if(meshRend == null) meshRend = aGo.AddComponent<MeshRenderer>();
			meshRend.sharedMaterials = materials;

			MeshFilter meshFilter = aGo.GetComponent<MeshFilter>();
			if(meshFilter == null) meshFilter = aGo.AddComponent<MeshFilter>();
			meshFilter.sharedMesh = mesh;
		}
		return mesh;
	}

	private void MergeMeshInto(Mesh meshToMerge, Material[] ms, Matrix4x4 transformMatrix, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uv1s, List<Vector2> uv2s, Dictionary<Material, List<int>> subMeshes) {
		if(meshToMerge == null) return;
		int vertexOffset = vertices.Count;
		Vector3[] vs = meshToMerge.vertices;

		for(int i=0;i<vs.Length;i++) {
			vs[i] = transformMatrix.MultiplyPoint3x4(vs[i]);
		}
		vertices.AddRange(vs);

		Quaternion rotation = Quaternion.LookRotation(transformMatrix.GetColumn(2), transformMatrix.GetColumn(1));
		Vector3[] ns = meshToMerge.normals;
		if(ns!=null && ns.Length>0) {
			for(int i=0;i<ns.Length;i++) ns[i] = rotation * ns[i];
			normals.AddRange(ns);
		}

		Vector2[] uvs = meshToMerge.uv;
		if(uvs!=null && uvs.Length>0) uv1s.AddRange(uvs);
		uvs = meshToMerge.uv2;
		if(uvs!=null && uvs.Length>0) uv2s.AddRange(uvs);

		for(int i=0;i<ms.Length;i++) {
			if(i<meshToMerge.subMeshCount) {
				int[] ts = meshToMerge.GetTriangles(i);
				if(ts.Length>0) {
					if(ms[i]!=null && !subMeshes.ContainsKey(ms[i])) {
						subMeshes.Add(ms[i], new List<int>());
					}
					List<int> subMesh = subMeshes[ms[i]];
					for(int t=0;t<ts.Length;t++) {
						ts[t] += vertexOffset;
					}
					subMesh.AddRange(ts);
				}
			}
		}
	}
}

/**
We scan the gameobject and search through it's hierarchy for MeshRenderers.
For each meshrenderer we find the MeshFilter and count the vertices.
We then check to see if we have more than 1 mesh and less than 64K vertices, because this is the max Unity will allow.

We make a new Mesh and create empty Lists for vertices, normals, etc.
We read the transform matrix for the gameObject 
We make a Dictionary with a List of triangles for each unique material we find

We go through all the MeshRenderers a second time, find the Meshes and for each mesh we call the function MergeMeshInto(). As parameters we use:
the found child mesh,
the shared materials found in the renderer
a transform matrix computed as parent world to local transform matrix * child local to world transform matrix
the lists we created for vertices, normals etc
the dictionary with triangles per material

After this loop we have all the vertices, normals, and triangles store in our lists and dictionary. So now we can populate the new Mesh with them.
Setting the vertices, normals and uv is easy.
We go through all the keys of the dictionary and add the material to a new Materials array. We also set the triangles for the sub mesh.
And finally we add a MeshRenderer and MeshFilter if they were not yet present and we set the materials and new Mesh in them. 
**/