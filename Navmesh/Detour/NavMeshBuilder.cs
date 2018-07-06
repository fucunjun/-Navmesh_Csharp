﻿using System;
using System.Collections.Generic;

/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
Recast4J Copyright (c) 2015 Piotr Piastucki piotr@jtilia.org

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.
Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:
1. The origin of this software must not be misrepresented; you must not
 claim that you wrote the original software. If you use this software
 in a product, an acknowledgment in the product documentation would be
 appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
 misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/
namespace org.recast4j.detour
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.recast4j.detour.DetourCommon.vCopy;


	public class NavMeshBuilder
	{

		internal const int MESH_NULL_IDX = 0xffff;

		private class BVItem
		{
			internal readonly int[] bmin = new int[3];
			internal readonly int[] bmax = new int[3];
			internal int i;
		}

		private class CompareItemX : IComparer<BVItem>
		{

			public virtual int Compare(BVItem a, BVItem b)
			{
				if (a.bmin[0] < b.bmin[0])
				{
					return -1;
				}
				if (a.bmin[0] > b.bmin[0])
				{
					return 1;
				}
				return 0;
			}

		}

		private class CompareItemY : IComparer<BVItem>
		{

			public virtual int Compare(BVItem a, BVItem b)
			{
				if (a.bmin[1] < b.bmin[1])
				{
					return -1;
				}
				if (a.bmin[1] > b.bmin[1])
				{
					return 1;
				}
				return 0;
			}

		}

		private class CompareItemZ : IComparer<BVItem>
		{

			public virtual int Compare(BVItem a, BVItem b)
			{
				if (a.bmin[2] < b.bmin[2])
				{
					return -1;
				}
				if (a.bmin[2] > b.bmin[2])
				{
					return 1;
				}
				return 0;
			}

		}

		private static int[][] calcExtends(BVItem[] items, int nitems, int imin, int imax)
		{
			int[] bmin = new int[3];
			int[] bmax = new int[3];
			bmin[0] = items[imin].bmin[0];
			bmin[1] = items[imin].bmin[1];
			bmin[2] = items[imin].bmin[2];

			bmax[0] = items[imin].bmax[0];
			bmax[1] = items[imin].bmax[1];
			bmax[2] = items[imin].bmax[2];

			for (int i = imin + 1; i < imax; ++i)
			{
				BVItem it = items[i];
				if (it.bmin[0] < bmin[0])
				{
					bmin[0] = it.bmin[0];
				}
				if (it.bmin[1] < bmin[1])
				{
					bmin[1] = it.bmin[1];
				}
				if (it.bmin[2] < bmin[2])
				{
					bmin[2] = it.bmin[2];
				}

				if (it.bmax[0] > bmax[0])
				{
					bmax[0] = it.bmax[0];
				}
				if (it.bmax[1] > bmax[1])
				{
					bmax[1] = it.bmax[1];
				}
				if (it.bmax[2] > bmax[2])
				{
					bmax[2] = it.bmax[2];
				}
			}
			return new int[][] {bmin, bmax};
		}

		private static int longestAxis(int x, int y, int z)
		{
			int axis = 0;
			int maxVal = x;
			if (y > maxVal)
			{
				axis = 1;
				maxVal = y;
			}
			if (z > maxVal)
			{
				axis = 2;
				maxVal = z;
			}
			return axis;
		}

		private static int subdivide(BVItem[] items, int nitems, int imin, int imax, int curNode, BVNode[] nodes)
		{

			int inum = imax - imin;
			int icur = curNode;

	/*		try
			{
				FileWriter fw = new FileWriter("test_java.log", true);
				String result = String.format("imin = %d, imax = %d\n", imin, imax);
				fw.write(result);
				for (int iter = imin; iter < imax; iter++)
				{
					result = String.format("items[%d].i = %d, ", iter, items[iter].i);
					fw.write(result);
				}
				result = String.format("\nicur = %d\n\n", icur);
				fw.write(result);
				fw.close();
			}
			catch (Exception e)
			{
				e.printStackTrace();
			}		
	*/		
			BVNode node = new BVNode();
			nodes[curNode++] = node;

			if (inum == 1)
			{
				// Leaf
				node.bmin[0] = items[imin].bmin[0];
				node.bmin[1] = items[imin].bmin[1];
				node.bmin[2] = items[imin].bmin[2];

				node.bmax[0] = items[imin].bmax[0];
				node.bmax[1] = items[imin].bmax[1];
				node.bmax[2] = items[imin].bmax[2];

				node.i = items[imin].i;

	/*			try
				{
					FileWriter fw = new FileWriter("test_java.log", true);
					String result = String.format("icur = %d, imin = %d, node.i = %d\n", icur, imin, node.i);
					fw.write(result);
					fw.close();
				}
				catch (Exception e)
				{
					e.printStackTrace();
				}
	*/
			}			else
			{
				// Split
				int[][] minmax = calcExtends(items, nitems, imin, imax);
				node.bmin = minmax[0];
				node.bmax = minmax[1];

				int axis = longestAxis(node.bmax[0] - node.bmin[0], node.bmax[1] - node.bmin[1], node.bmax[2] - node.bmin[2]);

				if (axis == 0)
				{
					// Sort along x-axis
					Array.Sort(items, imin, inum, new CompareItemX());

	/*				try
					{
						FileWriter fw = new FileWriter("test_java.log", true);
						String result = String.format("first: imin = %d, imax = %d, icur = %d\n", imin, imax, icur);
						fw.write(result);
						for (int iter = imin; iter < imax; iter++)
						{
							result = String.format("items[%d].i = %d, bmin = (%d, %d, %d), bmax = (%d, %d, %d)\n", 
									iter, items[iter].i, items[iter].bmin[0], items[iter].bmin[1], items[iter].bmin[2],
									items[iter].bmax[0], items[iter].bmax[1], items[iter].bmax[2]);
							fw.write(result);
						}
						result = String.format("\n");
						fw.write(result);
						fw.close();
					}
					catch (Exception e)
					{
						e.printStackTrace();
					}
	*/
				}				else if (axis == 1)
				{
					// Sort along y-axis
					Array.Sort(items, imin, inum, new CompareItemY());

		/*			try
					{
						FileWriter fw = new FileWriter("test_java.log", true);
						String result = String.format("second: imin = %d, imax = %d, icur = %d\n", imin, imax, icur);
						fw.write(result);
						for (int iter = imin; iter < imax; iter++)
						{
							result = String.format("items[%d].i = %d, bmin = (%d, %d, %d), bmax = (%d, %d, %d)\n", 
									iter, items[iter].i, items[iter].bmin[0], items[iter].bmin[1], items[iter].bmin[2],
									items[iter].bmax[0], items[iter].bmax[1], items[iter].bmax[2]);
							fw.write(result);
						}
						result = String.format("\n");
						fw.write(result);
						fw.close();
					}
					catch (Exception e)
					{
						e.printStackTrace();
					}	
	*/
				}				else
				{
					// Sort along z-axis
					Array.Sort(items, imin, inum, new CompareItemZ());

	/*				try
					{
						FileWriter fw = new FileWriter("test_java.log", true);
						String result = String.format("third: imin = %d, imax = %d, icur = %d\n", imin, imax, icur);
						fw.write(result);
						for (int iter = imin; iter < imax; iter++)
						{
							result = String.format("items[%d].i = %d, bmin = (%d, %d, %d), bmax = (%d, %d, %d)\n", 
									iter, items[iter].i, items[iter].bmin[0], items[iter].bmin[1], items[iter].bmin[2],
									items[iter].bmax[0], items[iter].bmax[1], items[iter].bmax[2]);
							fw.write(result);
						}
						result = String.format("\n");
						fw.write(result);
						fw.close();
					}
					catch (Exception e)
					{
						e.printStackTrace();
					}	
	*/
				}
				int isplit = imin + inum / 2;

				// Left
				curNode = subdivide(items, nitems, imin, isplit, curNode, nodes);
				// Right
				curNode = subdivide(items, nitems, isplit, imax, curNode, nodes);

				int iescape = curNode - icur;
				// Negative index means escape.
				node.i = -iescape;

	/*			try
				{
					FileWriter fw = new FileWriter("test_java.log", true);
					String result = String.format("imin = %d, isplit = %d, imax = %d, node.i = %d\n", 
							imin, isplit, imax, node.i);
					fw.write(result);
					fw.close();
				}
				catch (Exception e)
				{
					e.printStackTrace();
				}
	*/
			}			return curNode;
		}

		private static int createBVTree(int[] verts, int nverts, int[] polys, int npolys, int nvp, float cs, float ch, BVNode[] nodes)
		{
			// Build tree
			BVItem[] items = new BVItem[npolys];
			for (int i = 0; i < npolys; i++)
			{
				BVItem it = new BVItem();
				items[i] = it;
				it.i = i;
				// Calc polygon bounds.
				int p = i * nvp * 2;
				it.bmin[0] = it.bmax[0] = verts[polys[p] * 3 + 0];
				it.bmin[1] = it.bmax[1] = verts[polys[p] * 3 + 1];
				it.bmin[2] = it.bmax[2] = verts[polys[p] * 3 + 2];

				for (int j = 1; j < nvp; ++j)
				{
					if (polys[p + j] == MESH_NULL_IDX)
					{
						break;
					}
					int x = verts[polys[p + j] * 3 + 0];
					int y = verts[polys[p + j] * 3 + 1];
					int z = verts[polys[p + j] * 3 + 2];

					if (x < it.bmin[0])
					{
						it.bmin[0] = x;
					}
					if (y < it.bmin[1])
					{
						it.bmin[1] = y;
					}
					if (z < it.bmin[2])
					{
						it.bmin[2] = z;
					}

					if (x > it.bmax[0])
					{
						it.bmax[0] = x;
					}
					if (y > it.bmax[1])
					{
						it.bmax[1] = y;
					}
					if (z > it.bmax[2])
					{
						it.bmax[2] = z;
					}
				}
				// Remap y
				it.bmin[1] = (int) Math.Floor(it.bmin[1] * ch / cs);
				it.bmax[1] = (int) Math.Ceiling(it.bmax[1] * ch / cs);
			}

			return subdivide(items, npolys, 0, npolys, 0, nodes);
		}

		internal static readonly int XP = 1 << 0;
		internal static readonly int ZP = 1 << 1;
		internal static readonly int XM = 1 << 2;
		internal static readonly int ZM = 1 << 3;

		private static int classifyOffMeshPoint(VectorPtr pt, float[] bmin, float[] bmax)
		{

			int outcode = 0;
			outcode |= (pt.get(0) >= bmax[0]) ? XP : 0;
			outcode |= (pt.get(2) >= bmax[2]) ? ZP : 0;
			outcode |= (pt.get(0) < bmin[0]) ? XM : 0;
			outcode |= (pt.get(2) < bmin[2]) ? ZM : 0;

			switch (outcode)
			{
			case 1 << 0:
				return 0;
            case 1 << 0 | 1 << 1:
				return 1;
            case 1 << 1:
				return 2;
            case 1 << 2 | 1 << 1:
				return 3;
            case 1 << 2:
				return 4;
            case 1 << 2 | 1 << 3:
				return 5;
            case 1 << 3:
				return 6;
            case 1 << 0 | 1 << 3:
				return 7;
			}

			return 0xff;
		}

		/// <summary>
		/// Builds navigation mesh tile data from the provided tile creation data.
		/// </summary>
		/// <param name="params">
		///            Tile creation data.
		/// </param>
		/// <returns> created tile data </returns>
		public static MeshData createNavMeshData(NavMeshCreateParams @params)
		{
			if (@params.vertCount >= 0xffff)
			{
				return null;
			}
			if (@params.vertCount == 0 || @params.verts == null)
			{
				return null;
			}
			if (@params.polyCount == 0 || @params.polys == null)
			{
				return null;
			}

			int nvp = @params.nvp;

			// Classify off-mesh connection points. We store only the connections
			// whose start point is inside the tile.
			int[] offMeshConClass = null;
			int storedOffMeshConCount = 0;
			int offMeshConLinkCount = 0;

			if (@params.offMeshConCount > 0)
			{
				offMeshConClass = new int[@params.offMeshConCount * 2];

				// Find tight heigh bounds, used for culling out off-mesh start
				// locations.
				float hmin = float.MaxValue;
				float hmax = -float.MaxValue;

				if (@params.detailVerts != null && @params.detailVertsCount != 0)
				{
					for (int i = 0; i < @params.detailVertsCount; ++i)
					{
						float h = @params.detailVerts[i * 3 + 1];
						hmin = Math.Min(hmin, h);
						hmax = Math.Max(hmax, h);
					}
				}
				else
				{
					for (int i = 0; i < @params.vertCount; ++i)
					{
						int iv = i * 3;
						float h = @params.bmin[1] + @params.verts[iv + 1] * @params.ch;
						hmin = Math.Min(hmin, h);
						hmax = Math.Max(hmax, h);
					}
				}
				hmin -= @params.walkableClimb;
				hmax += @params.walkableClimb;
				float[] bmin = new float[3];
				float[] bmax = new float[3];
				DetourCommon.vCopy(bmin, @params.bmin);
                DetourCommon.vCopy(bmax, @params.bmax);
				bmin[1] = hmin;
				bmax[1] = hmax;

				for (int i = 0; i < @params.offMeshConCount; ++i)
				{
					VectorPtr p0 = new VectorPtr(@params.offMeshConVerts, (i * 2 + 0) * 3);
					VectorPtr p1 = new VectorPtr(@params.offMeshConVerts, (i * 2 + 1) * 3);

					offMeshConClass[i * 2 + 0] = classifyOffMeshPoint(p0, bmin, bmax);
					offMeshConClass[i * 2 + 1] = classifyOffMeshPoint(p1, bmin, bmax);

					// Zero out off-mesh start positions which are not even
					// potentially touching the mesh.
					if (offMeshConClass[i * 2 + 0] == 0xff)
					{
						if (p0.get(1) < bmin[1] || p0.get(1) > bmax[1])
						{
							offMeshConClass[i * 2 + 0] = 0;
						}
					}

					// Count how many links should be allocated for off-mesh
					// connections.
					if (offMeshConClass[i * 2 + 0] == 0xff)
					{
						offMeshConLinkCount++;
					}
					if (offMeshConClass[i * 2 + 1] == 0xff)
					{
						offMeshConLinkCount++;
					}

					if (offMeshConClass[i * 2 + 0] == 0xff)
					{
						storedOffMeshConCount++;
					}
				}
			}

			// Off-mesh connectionss are stored as polygons, adjust values.
			int totPolyCount = @params.polyCount + storedOffMeshConCount;
			int totVertCount = @params.vertCount + storedOffMeshConCount * 2;

			// Find portal edges which are at tile borders.
			int edgeCount = 0;
			int portalCount = 0;
			for (int i = 0; i < @params.polyCount; ++i)
			{
				int p = i * 2 * nvp;
				for (int j = 0; j < nvp; ++j)
				{
					if (@params.polys[p + j] == MESH_NULL_IDX)
					{
						break;
					}
					edgeCount++;

					if ((@params.polys[p + nvp + j] & 0x8000) != 0)
					{
						int dir = @params.polys[p + nvp + j] & 0xf;
						if (dir != 0xf)
						{
							portalCount++;
						}
					}
				}
			}

			int maxLinkCount = edgeCount + portalCount * 2 + offMeshConLinkCount * 2;

			// Find unique detail vertices.
			int uniqueDetailVertCount = 0;
			int detailTriCount = 0;
			if (@params.detailMeshes != null)
			{
				// Has detail mesh, count unique detail vertex count and use input
				// detail tri count.
				detailTriCount = @params.detailTriCount;
				for (int i = 0; i < @params.polyCount; ++i)
				{
					int p = i * nvp * 2;
					int ndv = @params.detailMeshes[i * 4 + 1];
					int nv = 0;
					for (int j = 0; j < nvp; ++j)
					{
						if (@params.polys[p + j] == MESH_NULL_IDX)
						{
							break;
						}
						nv++;
					}
					ndv -= nv;
					uniqueDetailVertCount += ndv;
				}
			}
			else
			{
				// No input detail mesh, build detail mesh from nav polys.
				uniqueDetailVertCount = 0; // No extra detail verts.
				detailTriCount = 0;
				for (int i = 0; i < @params.polyCount; ++i)
				{
					int p = i * nvp * 2;
					int nv = 0;
					for (int j = 0; j < nvp; ++j)
					{
						if (@params.polys[p + j] == MESH_NULL_IDX)
						{
							break;
						}
						nv++;
					}
					detailTriCount += nv - 2;
				}
			}

			int bvTreeSize = @params.buildBvTree ? @params.polyCount * 2 : 0;
			MeshHeader header = new MeshHeader();
			float[] navVerts = new float[3 * totVertCount];
			Poly[] navPolys = new Poly[totPolyCount];
			PolyDetail[] navDMeshes = new PolyDetail[@params.polyCount];
			float[] navDVerts = new float[3 * uniqueDetailVertCount];
			int[] navDTris = new int[4 * detailTriCount];
			BVNode[] navBvtree = new BVNode[bvTreeSize];
			OffMeshConnection[] offMeshCons = new OffMeshConnection[storedOffMeshConCount];

			// Store header
			header.magic = MeshHeader.DT_NAVMESH_MAGIC;
			header.version = MeshHeader.DT_NAVMESH_VERSION;
			header.x = @params.tileX;
			header.y = @params.tileY;
			header.layer = @params.tileLayer;
			header.userId = @params.userId;
			header.polyCount = totPolyCount;
			header.vertCount = totVertCount;
			header.maxLinkCount = maxLinkCount;
            DetourCommon.vCopy(header.bmin, @params.bmin);
            DetourCommon.vCopy(header.bmax, @params.bmax);
			header.detailMeshCount = @params.polyCount;
			header.detailVertCount = uniqueDetailVertCount;
			header.detailTriCount = detailTriCount;
			header.bvQuantFactor = 1.0f / @params.cs;
			header.offMeshBase = @params.polyCount;
			header.walkableHeight = @params.walkableHeight;
			header.walkableRadius = @params.walkableRadius;
			header.walkableClimb = @params.walkableClimb;
			header.offMeshConCount = storedOffMeshConCount;
			header.bvNodeCount = bvTreeSize;

			int offMeshVertsBase = @params.vertCount;
			int offMeshPolyBase = @params.polyCount;

			// Store vertices
			// Mesh vertices
			for (int i = 0; i < @params.vertCount; ++i)
			{
				int iv = i * 3;
				int v = i * 3;
				navVerts[v] = @params.bmin[0] + @params.verts[iv] * @params.cs;
				navVerts[v + 1] = @params.bmin[1] + @params.verts[iv + 1] * @params.ch;
				navVerts[v + 2] = @params.bmin[2] + @params.verts[iv + 2] * @params.cs;
			}
			// Off-mesh link vertices.
			int n = 0;
			for (int i = 0; i < @params.offMeshConCount; ++i)
			{
				// Only store connections which start from this tile.
				if (offMeshConClass[i * 2 + 0] == 0xff)
				{
					int linkv = i * 2 * 3;
					int v = (offMeshVertsBase + n * 2) * 3;
					Array.Copy(@params.offMeshConVerts, linkv, navVerts, v, 6);
					n++;
				}
			}

			// Store polygons
			// Mesh polys
			int src = 0;
			for (int i = 0; i < @params.polyCount; ++i)
			{
				Poly p = new Poly(i, nvp);
				navPolys[i] = p;
				p.vertCount = 0;
				p.flags = @params.polyFlags[i];
				p.Area = @params.polyAreas[i];
				p.Type = Poly.DT_POLYTYPE_GROUND;
				for (int j = 0; j < nvp; ++j)
				{
					if (@params.polys[src + j] == MESH_NULL_IDX)
					{
						break;
					}
					p.verts[j] = @params.polys[src + j];
					if ((@params.polys[src + nvp + j] & 0x8000) != 0)
					{
						// Border or portal edge.
						int dir = @params.polys[src + nvp + j] & 0xf;
						if (dir == 0xf) // Border
						{
							p.neis[j] = 0;
						}
						else if (dir == 0) // Portal x-
						{
							p.neis[j] = NavMesh.DT_EXT_LINK | 4;
						}
						else if (dir == 1) // Portal z+
						{
							p.neis[j] = NavMesh.DT_EXT_LINK | 2;
						}
						else if (dir == 2) // Portal x+
						{
							p.neis[j] = NavMesh.DT_EXT_LINK | 0;
						}
						else if (dir == 3) // Portal z-
						{
							p.neis[j] = NavMesh.DT_EXT_LINK | 6;
						}
					}
					else
					{
						// Normal connection
						p.neis[j] = @params.polys[src + nvp + j] + 1;
					}

					p.vertCount++;
				}
				src += nvp * 2;
			}
			// Off-mesh connection vertices.
			n = 0;
			for (int i = 0; i < @params.offMeshConCount; ++i)
			{
				// Only store connections which start from this tile.
				if (offMeshConClass[i * 2 + 0] == 0xff)
				{
					Poly p = new Poly(offMeshPolyBase + n, nvp);
					navPolys[offMeshPolyBase + n] = p;
					p.vertCount = 2;
					p.verts[0] = offMeshVertsBase + n * 2;
					p.verts[1] = offMeshVertsBase + n * 2 + 1;
					p.flags = @params.offMeshConFlags[i];
					p.Area = @params.offMeshConAreas[i];
					p.Type = Poly.DT_POLYTYPE_OFFMESH_CONNECTION;
					n++;
				}
			}

			// Store detail meshes and vertices.
			// The nav polygon vertices are stored as the first vertices on each
			// mesh.
			// We compress the mesh data by skipping them and using the navmesh
			// coordinates.
			if (@params.detailMeshes != null)
			{
				int vbase = 0;
				for (int i = 0; i < @params.polyCount; ++i)
				{
					PolyDetail dtl = new PolyDetail();
					navDMeshes[i] = dtl;
					int vb = @params.detailMeshes[i * 4 + 0];
					int ndv = @params.detailMeshes[i * 4 + 1];
					int nv = navPolys[i].vertCount;
					dtl.vertBase = vbase;
					dtl.vertCount = (ndv - nv);
					dtl.triBase = @params.detailMeshes[i * 4 + 2];
					dtl.triCount = @params.detailMeshes[i * 4 + 3];
					// Copy vertices except the first 'nv' verts which are equal to
					// nav poly verts.
					if (ndv - nv != 0)
					{
						Array.Copy(@params.detailVerts, (vb + nv) * 3, navDVerts, vbase * 3, 3 * (ndv - nv));
						vbase += ndv - nv;
					}
				}
				// Store triangles.
				Array.Copy(@params.detailTris, 0, navDTris, 0, 4 * @params.detailTriCount);
			}
			else
			{
				// Create dummy detail mesh by triangulating polys.
				int tbase = 0;
				for (int i = 0; i < @params.polyCount; ++i)
				{
					PolyDetail dtl = new PolyDetail();
					navDMeshes[i] = dtl;
					int nv = navPolys[i].vertCount;
					dtl.vertBase = 0;
					dtl.vertCount = 0;
					dtl.triBase = tbase;
					dtl.triCount = (nv - 2);
					// Triangulate polygon (local indices).
					for (int j = 2; j < nv; ++j)
					{
						int t = tbase * 4;
						navDTris[t + 0] = 0;
						navDTris[t + 1] = (j - 1);
						navDTris[t + 2] = j;
						// Bit for each edge that belongs to poly boundary.
						navDTris[t + 3] = (1 << 2);
						if (j == 2)
						{
							navDTris[t + 3] |= (1 << 0);
						}
						if (j == nv - 1)
						{
							navDTris[t + 3] |= (1 << 4);
						}
						tbase++;
					}
				}
			}

			// Store and create BVtree.
			// TODO: take detail mesh into account! use byte per bbox extent?
			if (@params.buildBvTree)
			{
				// Do not set header.bvNodeCount set to make it work look exactly the same as in original Detour  
				header.bvNodeCount = createBVTree(@params.verts, @params.vertCount, @params.polys, @params.polyCount, nvp, @params.cs, @params.ch, navBvtree);

			}

			// Store Off-Mesh connections.
			n = 0;
			for (int i = 0; i < @params.offMeshConCount; ++i)
			{
				// Only store connections which start from this tile.
				if (offMeshConClass[i * 2 + 0] == 0xff)
				{
					OffMeshConnection con = new OffMeshConnection();
					offMeshCons[n] = con;
					con.poly = (offMeshPolyBase + n);
					// Copy connection end-points.
					int endPts = i * 2 * 3;
					Array.Copy(@params.offMeshConVerts, endPts, con.pos, 0, 6);
					con.rad = @params.offMeshConRad[i];
					con.flags = @params.offMeshConDir[i] != 0 ? NavMesh.DT_OFFMESH_CON_BIDIR : 0;
					con.side = offMeshConClass[i * 2 + 1];
					if (@params.offMeshConUserID != null)
					{
						con.userId = @params.offMeshConUserID[i];
					}
					n++;
				}
			}

			MeshData nmd = new MeshData();
			nmd.header = header;
			nmd.verts = navVerts;
			nmd.polys = navPolys;
			nmd.detailMeshes = navDMeshes;
			nmd.detailVerts = navDVerts;
			nmd.detailTris = navDTris;
			nmd.bvTree = navBvtree;
			nmd.offMeshCons = offMeshCons;

	/*		try
			{
				FileWriter fw = new FileWriter("test_java.log", true);
				for (int iter = 0; iter < header.bvNodeCount; iter++)
				{
					String result = String.format("node.i = %d\n", navBvtree[iter].i);
					fw.write(result);
				}
				fw.close();
			}
			catch (Exception e)
			{
				e.printStackTrace();
			}
	*/			return nmd;
		}

	}

}