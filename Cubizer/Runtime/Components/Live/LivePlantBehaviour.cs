﻿using UnityEngine;
using Cubizer.Chunk;

namespace Cubizer
{
	[RequireComponent(typeof(MeshRenderer))]
	[AddComponentMenu("Cubizer/LivePlantBehaviour")]
	public class LivePlantBehaviour : LiveBehaviour
	{
		public readonly static Vector3[,] _vertices = new Vector3[4, 4]
		{
			{ new Vector3( 0.0f, -0.5f, -0.5f), new Vector3( 0.0f, -0.5f, +0.5f), new Vector3( 0.0f, +0.5f, -0.5f), new Vector3( 0.0f, +0.5f, +0.5f)},
			{ new Vector3( 0.0f, -0.5f, -0.5f), new Vector3( 0.0f, -0.5f, +0.5f), new Vector3( 0.0f, +0.5f, -0.5f), new Vector3( 0.0f, +0.5f, +0.5f)},
			{ new Vector3(-0.5f, -0.5f,  0.0f), new Vector3(-0.5f, +0.5f,  0.0f), new Vector3(+0.5f, -0.5f,  0.0f), new Vector3(+0.5f, +0.5f,  0.0f)},
			{ new Vector3(-0.5f, -0.5f,  0.0f), new Vector3(-0.5f, +0.5f,  0.0f), new Vector3(+0.5f, -0.5f,  0.0f), new Vector3(+0.5f, +0.5f,  0.0f)}
		};

		public readonly static Vector3[] _normals = new Vector3[4]
		{
			new Vector3(-1, 0, 0),
			new Vector3(+1, 0, 0),
			new Vector3(0, 0, -1),
			new Vector3(0, 0, +1)
		};

		public readonly static Vector2[,] _uvs = new Vector2[4, 4]
		{
			{ new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1)},
			{ new Vector2(1, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 1)},
			{ new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1)},
			{ new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 1)}
		};

		public readonly static int[,] _indices = new int[4, 6]
		{
			{0, 3, 2, 0, 1, 3},
			{0, 3, 1, 0, 2, 3},
			{0, 3, 2, 0, 1, 3},
			{0, 3, 1, 0, 2, 3}
		};

		private LODGroup _lodGroup;
		private MeshRenderer _renderer;

		private MeshCollider _meshCollider;

		public void Start()
		{
			_lodGroup = GetComponent<LODGroup>();
			_meshCollider = GetComponent<MeshCollider>();
			_renderer = GetComponent<MeshRenderer>();
		}

		public override void OnBuildChunk(ChunkDataContext context)
		{
			var writeCount = 0;
			var data = new LiveMesh((context.faceCount / 6) * 16, (context.faceCount / 6) * 24);

			foreach (VoxelPrimitive it in context.model.GetEnumerator(this.material.GetInstanceID()))
			{
				Vector3 pos, scale;
				it.GetTranslateScale(out pos, out scale);
				OnBuildBlock(ref data, ref writeCount, pos, scale, it.faces);
			}

			if (data.indices.Length > 0)
			{
				var actors = new GameObject(this.name);
				actors.isStatic = this.gameObject.isStatic;
				actors.tag = gameObject.tag;
				actors.layer = this.gameObject.layer;
				actors.transform.parent = context.parent.transform;
				actors.transform.position = context.parent.transform.position;

				var mesh = data.mesh;
				actors.AddComponent<MeshFilter>().mesh = mesh;

				if (_renderer != null)
				{
					var clone = actors.AddComponent<MeshRenderer>();
					clone.material = _renderer.material;
					clone.receiveShadows = _renderer.receiveShadows;
					clone.shadowCastingMode = _renderer.shadowCastingMode;

					if (_lodGroup != null)
					{
						var lods = _lodGroup.GetLODs();
						for (int i = 0; i < lods.Length; i++)
						{
							if (lods[i].renderers.Length > 0)
								lods[i].renderers[0] = clone;
						}

						actors.AddComponent<LODGroup>().SetLODs(lods);
					}
				}

				if (_meshCollider && _meshCollider.enabled)
				{
					var meshCollider = actors.AddComponent<MeshCollider>();
					meshCollider.sharedMesh = _meshCollider.sharedMesh ? _meshCollider.sharedMesh : mesh;
					meshCollider.material = _meshCollider.material;
				}
			}
		}

		public void OnBuildBlock(ref LiveMesh mesh, ref int index, Vector3 pos, Vector3 scale, VoxelVisiableFaces faces)
		{
			for (int i = 0; i < 4; i++)
			{
				for (int n = index * 4, k = 0; k < 4; k++, n++)
				{
					Vector3 v = _vertices[i, k];
					v.x *= scale.x;
					v.y *= scale.y;
					v.z *= scale.z;
					v.x += pos.x;
					v.y += pos.y;
					v.z += pos.z;

					mesh.vertices[n] = v;
					mesh.normals[n] = _normals[i];
					mesh.uv[n] = _uvs[i, k];
				}

				for (int j = index * 6, k = 0; k < 6; k++, j++)
					mesh.indices[j] = index * 4 + _indices[i, k];

				index++;
			}
		}
	}
}