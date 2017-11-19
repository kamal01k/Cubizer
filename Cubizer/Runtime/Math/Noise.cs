﻿using UnityEngine;

namespace Cubizer
{
	namespace Math
	{
		public class Noise
		{
			public static float F2 = 0.3660254037844386f;
			public static float G2 = 0.21132486540518713f;
			public static float F3 = 1.0f / 3.0f;
			public static float G3 = 1.0f / 6.0f;

			public static float[,] GRAD3 = new float[16, 3]
			{
				{ 1, 1, 0 }, { -1, 1, 0 }, { 1,-1, 0 }, {-1,-1, 0 },
				{ 1, 0, 1 }, { -1, 0, 1 }, { 1, 0,-1 }, {-1, 0,-1 },
				{ 0, 1, 1 }, { 0, -1, 1 }, { 0, 1,-1 }, { 0,-1,-1 },
				{ 1, 0,-1 }, { -1, 0,-1 }, { 0,-1, 1 }, { 0, 1, 1 }
			};

			public static byte[] PERM = new byte[]
			{
				151, 160, 137, 91, 90, 15, 131, 13,
				201, 95, 96, 53, 194, 233, 7, 225,
				140, 36, 103, 30, 69, 142, 8, 99,
				37, 240, 21, 10, 23, 190, 6, 148,
				247, 120, 234, 75, 0, 26, 197, 62,
				94, 252, 219, 203, 117, 35, 11, 32,
				57, 177, 33, 88, 237, 149, 56, 87,
				174, 20, 125, 136, 171, 168, 68, 175,
				74, 165, 71, 134, 139, 48, 27, 166,
				77, 146, 158, 231, 83, 111, 229, 122,
				60, 211, 133, 230, 220, 105, 92, 41,
				55, 46, 245, 40, 244, 102, 143, 54,
				65, 25, 63, 161, 1, 216, 80, 73,
				209, 76, 132, 187, 208, 89, 18, 169,
				200, 196, 135, 130, 116, 188, 159, 86,
				164, 100, 109, 198, 173, 186, 3, 64,
				52, 217, 226, 250, 124, 123, 5, 202,
				38, 147, 118, 126, 255, 82, 85, 212,
				207, 206, 59, 227, 47, 16, 58, 17,
				182, 189, 28, 42, 223, 183, 170, 213,
				119, 248, 152, 2, 44, 154, 163, 70,
				221, 153, 101, 155, 167, 43, 172, 9,
				129, 22, 39, 253, 19, 98, 108, 110,
				79, 113, 224, 232, 178, 185, 112, 104,
				218, 246, 97, 228, 251, 34, 242, 193,
				238, 210, 144, 12, 191, 179, 162, 241,
				81, 51, 145, 235, 249, 14, 239, 107,
				49, 192, 214, 31, 181, 199, 106, 157,
				184, 84, 204, 176, 115, 121, 50, 45,
				127, 4, 150, 254, 138, 236, 205, 93,
				222, 114, 67, 29, 24, 72, 243, 141,
				128, 195, 78, 66, 215, 61, 156, 180,
				151, 160, 137, 91, 90, 15, 131, 13,
				201, 95, 96, 53, 194, 233, 7, 225,
				140, 36, 103, 30, 69, 142, 8, 99,
				37, 240, 21, 10, 23, 190, 6, 148,
				247, 120, 234, 75, 0, 26, 197, 62,
				94, 252, 219, 203, 117, 35, 11, 32,
				57, 177, 33, 88, 237, 149, 56, 87,
				174, 20, 125, 136, 171, 168, 68, 175,
				74, 165, 71, 134, 139, 48, 27, 166,
				77, 146, 158, 231, 83, 111, 229, 122,
				60, 211, 133, 230, 220, 105, 92, 41,
				55, 46, 245, 40, 244, 102, 143, 54,
				65, 25, 63, 161, 1, 216, 80, 73,
				209, 76, 132, 187, 208, 89, 18, 169,
				200, 196, 135, 130, 116, 188, 159, 86,
				164, 100, 109, 198, 173, 186, 3, 64,
				52, 217, 226, 250, 124, 123, 5, 202,
				38, 147, 118, 126, 255, 82, 85, 212,
				207, 206, 59, 227, 47, 16, 58, 17,
				182, 189, 28, 42, 223, 183, 170, 213,
				119, 248, 152, 2, 44, 154, 163, 70,
				221, 153, 101, 155, 167, 43, 172, 9,
				129, 22, 39, 253, 19, 98, 108, 110,
				79, 113, 224, 232, 178, 185, 112, 104,
				218, 246, 97, 228, 251, 34, 242, 193,
				238, 210, 144, 12, 191, 179, 162, 241,
				81, 51, 145, 235, 249, 14, 239, 107,
				49, 192, 214, 31, 181, 199, 106, 157,
				184, 84, 204, 176, 115, 121, 50, 45,
				127, 4, 150, 254, 138, 236, 205, 93,
				222, 114, 67, 29, 24, 72, 243, 141,
				128, 195, 78, 66, 215, 61, 156, 180
			};

			public static float noise2(float x, float y)
			{
				int i1, j1, I, J;
				float s = (x + y) * F2;
				float i = Mathf.Floor(x + s);
				float j = Mathf.Floor(y + s);
				float t = (i + j) * G2;

				float[] xx = new float[3], yy = new float[3], f = new float[3];
				float[] noise = new float[3] { 0.0f, 0.0f, 0.0f };
				int[] g = new int[3];

				xx[0] = x - (i - t);
				yy[0] = y - (j - t);

				i1 = xx[0] > yy[0] ? 1 : 0;
				j1 = xx[0] <= yy[0] ? 1 : 0;

				xx[2] = xx[0] + G2 * 2.0f - 1.0f;
				yy[2] = yy[0] + G2 * 2.0f - 1.0f;
				xx[1] = xx[0] - i1 + G2;
				yy[1] = yy[0] - j1 + G2;

				I = (int)i & 255;
				J = (int)j & 255;
				g[0] = PERM[I + PERM[J]] % 12;
				g[1] = PERM[I + i1 + PERM[J + j1]] % 12;
				g[2] = PERM[I + 1 + PERM[J + 1]] % 12;

				for (byte c = 0; c <= 2; c++)
				{
					f[c] = 0.5f - xx[c] * xx[c] - yy[c] * yy[c];
				}

				for (byte c = 0; c <= 2; c++)
				{
					if (f[c] > 0)
					{
						noise[c] = f[c] * f[c] * f[c] * f[c] * (GRAD3[g[c], 0] * xx[c] + GRAD3[g[c], 1] * yy[c]);
					}
				}

				return (noise[0] + noise[1] + noise[2]) * 70.0f;
			}

			public static float noise3(float x, float y, float z)
			{
				int I, J, K;
				int[] o1 = new int[3], o2 = new int[3], g = new int[4];
				float[] f = new float[4], noise = new float[4] { 0.0f, 0.0f, 0.0f, 0.0f };
				float s = (x + y + z) * F3;
				float i = Mathf.Floor(x + s);
				float j = Mathf.Floor(y + s);
				float k = Mathf.Floor(z + s);
				float t = (i + j + k) * G3;

				float[,] pos = new float[4, 3];

				pos[0, 0] = x - (i - t);
				pos[0, 1] = y - (j - t);
				pos[0, 2] = z - (k - t);

				if (pos[0, 0] >= pos[0, 1])
				{
					if (pos[0, 1] >= pos[0, 2])
					{
						o1[0] = 1; o1[1] = 0; o1[1] = 0;
						o2[0] = 1; o2[1] = 1; o2[1] = 0;
					}
					else if (pos[0, 0] >= pos[0, 2])
					{
						o1[0] = 1; o1[1] = 0; o1[1] = 0;
						o2[0] = 1; o2[1] = 0; o2[1] = 1;
					}
					else
					{
						o1[0] = 0; o1[1] = 0; o1[1] = 1;
						o2[0] = 1; o2[1] = 0; o2[1] = 1;
					}
				}
				else
				{
					if (pos[0, 1] < pos[0, 2])
					{
						o1[0] = 0; o1[1] = 0; o1[1] = 1;
						o2[0] = 0; o2[1] = 1; o2[1] = 1;
					}
					else if (pos[0, 0] < pos[0, 2])
					{
						o1[0] = 0; o1[1] = 1; o1[1] = 0;
						o2[0] = 0; o2[1] = 1; o2[1] = 1;
					}
					else
					{
						o1[0] = 0; o1[1] = 1; o1[1] = 0;
						o2[0] = 1; o2[1] = 1; o2[1] = 0;
					}
				}

				for (byte c = 0; c <= 2; c++)
				{
					pos[3, c] = pos[0, c] - 1.0f + 3.0f * G3;
					pos[2, c] = pos[0, c] - o2[c] + 2.0f * G3;
					pos[1, c] = pos[0, c] - o1[c] + G3;
				}

				I = (int)i & 255;
				J = (int)j & 255;
				K = (int)k & 255;
				g[0] = PERM[I + PERM[J + PERM[K]]] % 12;
				g[1] = PERM[I + o1[0] + PERM[J + o1[1] + PERM[o1[2] + K]]] % 12;
				g[2] = PERM[I + o2[0] + PERM[J + o2[1] + PERM[o2[2] + K]]] % 12;
				g[3] = PERM[I + 1 + PERM[J + 1 + PERM[K + 1]]] % 12;

				for (byte c = 0; c <= 3; c++)
				{
					f[c] = 0.6f - pos[c, 0] * pos[c, 0] - pos[c, 1] * pos[c, 1] -
						pos[c, 2] * pos[c, 2];
				}

				for (byte c = 0; c <= 3; c++)
				{
					if (f[c] > 0)
					{
						noise[c] = f[c] * f[c] * f[c] * f[c] * (pos[c, 0] * GRAD3[g[c], 0] + pos[c, 1] * GRAD3[g[c], 1] + pos[c, 2] * GRAD3[g[c], 2]);
					}
				}

				return (noise[0] + noise[1] + noise[2] + noise[3]) * 32.0f;
			}

			public static float simplex2(float x, float y, int octaves, float persistence, float lacunarity)
			{
				float freq = 1.0f;
				float amp = 1.0f;
				float max = 1.0f;
				float total = noise2(x, y);

				for (int i = 1; i < octaves; i++)
				{
					freq *= lacunarity;
					amp *= persistence;
					max += amp;
					total += noise2(x * freq, y * freq) * amp;
				}

				return (total / max) * 0.5f + 0.5f;
			}

			public static float simplex3(float x, float y, float z, int octaves, float persistence, float lacunarity)
			{
				float freq = 1.0f;
				float amp = 1.0f;
				float max = 1.0f;
				float total = noise3(x, y, z);

				for (int i = 1; i < octaves; ++i)
				{
					freq *= lacunarity;
					amp *= persistence;
					max += amp;
					total += noise3(x * freq, y * freq, z * freq) * amp;
				}

				return (1 + total / max) / 2;
			}

			public static void simplex_seed(int seed)
			{
				UnityEngine.Random.InitState(seed);

				for (int i = 0; i < 256; i++)
					PERM[i] = (byte)i;

				for (int i = 255; i > 0; i--)
				{
					int j;
					int n = i + 1;
					while (n <= (j = (int)(UnityEngine.Random.Range(0.0f, float.MaxValue) / (float.MaxValue / n)))) { }
					byte a = PERM[i];
					byte b = PERM[j];
					PERM[i] = b;
					PERM[j] = a;
				}

				System.Buffer.BlockCopy(PERM, 0, PERM, 256, 255);
			}
		}
	}
}