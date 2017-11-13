﻿using System;
using System.IO;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;

namespace Cubizer
{
	[DisallowMultipleComponent]
	[AddComponentMenu("Cubizer/Terrain")]
	public class Terrain : MonoBehaviour
	{
		[SerializeField, Range(16, 32)]
		private int _chunkSize = 24;

		[SerializeField, Range(256, 2048)]
		private int _chunkNumLimits = 1024;

		[SerializeField] private int _chunkHeightLimitLow = -10;
		[SerializeField] private int _chunkHeightLimitHigh = 20;

		[SerializeField] private int _terrainSeed = 255;
		[SerializeField] private LiveResources _liveResources;
		[SerializeField] private BiomeGeneratorManager _biomeManager;

		private ChunkDataManager _chunks;

		private TerrainDelegates _events;

		public int chunkSize
		{
			get { return _chunkSize; }
		}

		public int chunkNumLimits
		{
			set { _chunkNumLimits = value; }
			get { return _chunkNumLimits; }
		}

		public int chunkHeightLimitLow
		{
			set { _chunkHeightLimitLow = value; }
			get { return _chunkHeightLimitLow; }
		}

		public int chunkHeightLimitHigh
		{
			set { _chunkHeightLimitHigh = value; }
			get { return _chunkHeightLimitHigh; }
		}

		public LiveResources liveResources
		{
			get { return _liveResources; }
		}

		public BiomeGeneratorManager biomeManager
		{
			get { return _biomeManager; }
		}

		public ChunkDataManager chunks
		{
			get { return _chunks; }
		}

		public TerrainDelegates events
		{
			get { return _events; }
		}

		public Terrain(int chunkSize)
		{
			Debug.Assert(chunkSize > 0);

			_chunks = new ChunkDataManager(chunkSize);
			_chunkSize = chunkSize;
		}

		public Terrain(int chunkSize, ChunkDataManager chunks)
		{
			Debug.Assert(chunkSize > 0 && chunks != null);

			_chunks = chunks;
			_chunkSize = chunkSize;
		}

		public void Awake()
		{
			if (_liveResources == null)
				Debug.LogError("Please drag a LiveResources into Hierarchy View.");

			if (_biomeManager == null)
				Debug.LogError("Please drag a TerrainBiome into Hierarchy View.");
		}

		public void Start()
		{
			Debug.Assert(_chunkSize > 0);

			Math.Noise.simplex_seed(_terrainSeed);

			_chunks = new ChunkDataManager(_chunkSize);
			_events = new TerrainDelegates();
		}

		public void OnDestroy()
		{
			this.DestroyChunks();
		}

		public short CalculateChunkPosByWorld(float x)
		{
			return (short)Mathf.FloorToInt(x / _chunkSize);
		}

		public bool HitTestByRay(Ray ray, int hitDistance, ref ChunkPrimer chunk, out byte outX, out byte outY, out byte outZ, ref ChunkPrimer lastChunk, out byte lastX, out byte lastY, out byte lastZ)
		{
			var chunkX = CalculateChunkPosByWorld(ray.origin.x);
			var chunkY = CalculateChunkPosByWorld(ray.origin.y);
			var chunkZ = CalculateChunkPosByWorld(ray.origin.z);

			lastChunk = null;
			lastX = lastY = lastZ = outX = outY = outZ = 255;

			if (!_chunks.Get(chunkX, chunkY, chunkZ, ref chunk))
				return false;

			Vector3 origin = ray.origin;
			origin.x -= chunk.position.x * _chunkSize;
			origin.y -= chunk.position.y * _chunkSize;
			origin.z -= chunk.position.z * _chunkSize;

			VoxelMaterial block = null;

			for (int i = 0; i < hitDistance && block == null; i++)
			{
				int ix = Mathf.RoundToInt(origin.x);
				int iy = Mathf.RoundToInt(origin.y);
				int iz = Mathf.RoundToInt(origin.z);

				if (outX == ix && outY == iy && outZ == iz)
					continue;

				bool isOutOfChunk = false;
				if (ix < 0) { ix = ix + _chunkSize; origin.x += _chunkSize; chunkX--; isOutOfChunk = true; }
				if (iy < 0) { iy = iy + _chunkSize; origin.y += _chunkSize; chunkY--; isOutOfChunk = true; }
				if (iz < 0) { iz = iz + _chunkSize; origin.z += _chunkSize; chunkZ--; isOutOfChunk = true; }
				if (ix + 1 > _chunkSize) { ix = ix - _chunkSize; origin.x -= _chunkSize; chunkX++; isOutOfChunk = true; }
				if (iy + 1 > _chunkSize) { iy = iy - _chunkSize; origin.y -= _chunkSize; chunkY++; isOutOfChunk = true; }
				if (iz + 1 > _chunkSize) { iz = iz - _chunkSize; origin.z -= _chunkSize; chunkZ++; isOutOfChunk = true; }

				lastX = outX;
				lastY = outY;
				lastZ = outZ;
				lastChunk = chunk;

				if (isOutOfChunk)
				{
					if (!_chunks.Get(chunkX, chunkY, chunkZ, ref chunk))
						return false;
				}

				chunk.voxels.Get((byte)ix, (byte)iy, (byte)iz, ref block);

				origin += ray.direction;

				outX = (byte)ix;
				outY = (byte)iy;
				outZ = (byte)iz;
			}

			return block != null;
		}

		public bool HitTestByRay(Ray ray, int hitDistance, ref ChunkPrimer chunk, out byte outX, out byte outY, out byte outZ)
		{
			byte lx, ly, lz;
			ChunkPrimer chunkLast = null;

			return this.HitTestByRay(ray, hitDistance, ref chunk, out outX, out outY, out outZ, ref chunkLast, out lx, out ly, out lz);
		}

		public bool HitTestByScreenPos(Vector3 pos, int hitDistance, ref ChunkPrimer chunk, out byte outX, out byte outY, out byte outZ, ref ChunkPrimer lastChunk, out byte lastX, out byte lastY, out byte lastZ)
		{
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			ray.origin = Camera.main.transform.position;
			return this.HitTestByRay(ray, hitDistance, ref chunk, out outX, out outY, out outZ, ref lastChunk, out lastX, out lastY, out lastZ);
		}

		public bool HitTestByScreenPos(Vector3 pos, int hitDistance, ref ChunkPrimer chunk, out byte outX, out byte outY, out byte outZ)
		{
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			ray.origin = Camera.main.transform.position;
			return this.HitTestByRay(ray, hitDistance, ref chunk, out outX, out outY, out outZ);
		}

		public bool AddBlockByRay(Ray ray, int hitDistance, VoxelMaterial block)
		{
			Debug.Assert(block != null);

			byte x, y, z, lx, ly, lz;
			ChunkPrimer chunkNow = null;
			ChunkPrimer chunkLast = null;

			if (HitTestByRay(ray, hitDistance, ref chunkNow, out x, out y, out z, ref chunkLast, out lx, out ly, out lz))
			{
				var chunk = chunkLast != null ? chunkLast : chunkNow;
				chunk.voxels.Set(lx, ly, lz, block);
				chunk.OnChunkChange();

				return true;
			}

			return false;
		}

		public bool AddBlockByScreenPos(Vector3 pos, int hitDistance, VoxelMaterial block)
		{
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			ray.origin = Camera.main.transform.position;
			return this.AddBlockByRay(ray, hitDistance, block);
		}

		public bool RemoveBlockByRay(Ray ray, int hitDistance)
		{
			byte x, y, z;
			ChunkPrimer chunk = null;

			if (HitTestByRay(ray, hitDistance, ref chunk, out x, out y, out z))
			{
				chunk.voxels.Set(x, y, z, null);
				chunk.OnChunkChange();

				return true;
			}

			return false;
		}

		public bool RemoveBlockByScreenPos(Vector3 pos, int hitDistance)
		{
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			ray.origin = Camera.main.transform.position;
			return this.RemoveBlockByRay(ray, hitDistance);
		}

		public bool GetEmptryChunkPos(Vector3 translate, Plane[] planes, Vector2Int[] radius, out Vector3Int position)
		{
			int x = CalculateChunkPosByWorld(translate.x);
			int y = CalculateChunkPosByWorld(translate.y);
			int z = CalculateChunkPosByWorld(translate.z);

			int bestX = 0, bestY = 0, bestZ = 0;
			int bestScore = int.MaxValue;

			int start = bestScore;

			Vector3 _chunkOffset = (Vector3.one * _chunkSize - Vector3.one) * 0.5f;

			for (int ix = radius[0].x; ix <= radius[0].y; ix++)
			{
				for (int iy = radius[1].x; iy <= radius[1].y; iy++)
				{
					for (int iz = radius[2].x; iz <= radius[2].y; iz++)
					{
						int dx = x + ix;
						int dy = y + iy;
						int dz = z + iz;

						if (dy < _chunkHeightLimitLow || dy > _chunkHeightLimitHigh)
							continue;

						var hit = _chunks.Exists((short)dx, (short)dy, (short)dz);
						if (hit)
							continue;

						var p = _chunkOffset + new Vector3(dx, dy, dz) * _chunkSize;

						int invisiable = GeometryUtility.TestPlanesAABB(planes, new Bounds(p, Vector3.one * _chunkSize)) ? 0 : 1;
						int distance = Mathf.Max(Mathf.Max(Mathf.Abs(ix), Mathf.Abs(iy)), Mathf.Abs(iz));
						int score = (invisiable << 24) | distance;

						if (score < bestScore)
						{
							bestScore = score;
							bestX = dx;
							bestY = dy;
							bestZ = dz;
						}
					}
				}
			}

			position = new Vector3Int(bestX, bestY, bestZ);
			return start != bestScore;
		}

		public void DestroyChunks(bool is_save = true)
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				var transformChild = transform.GetChild(i);
				var child = transformChild.gameObject;

				if (is_save)
				{
					if (_events.onSaveChunkData != null)
						_events.onSaveChunkData(child);
				}

				Destroy(child);
			}
		}

		public void DestroyChunksImmediate(bool is_save = true)
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				var transformChild = transform.GetChild(i);
				var child = transformChild.gameObject;

				if (is_save)
				{
					if (_events.onSaveChunkData != null)
						_events.onSaveChunkData(child);
				}

				DestroyImmediate(child);
			}
		}

		public void UpdateChunkForDestroy(Camera player, float maxDistance)
		{
			var length = transform.childCount;

			for (int i = 0; i < length; i++)
			{
				var transformChild = transform.GetChild(i);

				var distance = Vector3.Distance(transformChild.position, player.transform.position);
				if (distance > maxDistance * _chunkSize)
				{
					if (_events.onSaveChunkData != null)
						_events.onSaveChunkData(transformChild.gameObject);

					Destroy(transformChild.gameObject);
					break;
				}
			}
		}

		public void UpdateChunkForCreate(Camera camera, Vector2Int[] radius)
		{
			if (_chunks.count > _chunkNumLimits)
				return;

			Vector3Int position;
			if (!GetEmptryChunkPos(camera.transform.position, GeometryUtility.CalculateFrustumPlanes(camera), radius, out position))
				return;

			ChunkPrimer chunk = null;
			if (_events.onLoadChunkData != null)
				_events.onLoadChunkData(position, out chunk);

			if (chunk == null)
				chunk = _biomeManager.buildChunk((short)position.x, (short)position.y, (short)position.z);

			if (chunk != null)
			{
				var gameObject = new GameObject("Chunk");
				gameObject.transform.parent = transform;
				gameObject.transform.position = position * _chunkSize;
				gameObject.AddComponent<TerrainData>().chunk = chunk;
			}
		}

		public bool Save(string path)
		{
			Debug.Assert(path != null);

			using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
			{
				var serializer = new BinaryFormatter();
				serializer.Serialize(stream, _chunks);

				return true;
			}
		}

		public bool Load(string path, bool is_save = true)
		{
			Debug.Assert(path != null);

			using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				if (stream == null)
					return false;

				var serializer = new BinaryFormatter();

				_chunks = serializer.Deserialize(stream) as ChunkDataManager;
				_chunkSize = _chunks.chunkSize;

				DestroyChunksImmediate(is_save);

				foreach (var chunk in _chunks.GetEnumerator())
				{
					var gameObject = new GameObject("Chunk");
					gameObject.transform.parent = transform;
					gameObject.transform.position = new Vector3(chunk.position.x, chunk.position.y, chunk.position.z) * _chunkSize;
					gameObject.AddComponent<TerrainData>().chunk = chunk.value;
				}

				return true;
			}
		}
	}
}