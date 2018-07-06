﻿using System;

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
namespace org.recast4j.recast
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.recast4j.recast.RecastConstants.RC_NOT_CONNECTED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.recast4j.recast.RecastConstants.RC_NULL_AREA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.recast4j.recast.RecastConstants.RC_WALKABLE_AREA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.recast4j.recast.RecastVectors.copy;

	public class Recast
	{

		internal virtual void calcBounds(float[] verts, int nv, float[] bmin, float[] bmax)
		{
			for (int i = 0; i < 3; i++)
			{
				bmin[i] = verts[i];
				bmax[i] = verts[i];
			}
			for (int i = 1; i < nv; ++i)
			{
				for (int j = 0; j < 3; j++)
				{
					bmin[j] = Math.Min(bmin[j], verts[i * 3 + j]);
					bmax[j] = Math.Max(bmax[j], verts[i * 3 + j]);
				}
			}
			// Calculate bounding box.
		}

		public static int[] calcGridSize(float[] bmin, float[] bmax, float cs)
		{
			return new int[] {(int)((bmax[0] - bmin[0]) / cs + 0.5f), (int)((bmax[2] - bmin[2]) / cs + 0.5f)};
		}

		public static int[] calcTileCount(float[] bmin, float[] bmax, float cs, int tileSize)
		{
			int[] gwh = Recast.calcGridSize(bmin, bmax, cs);
			int gw = gwh[0];
			int gh = gwh[1];
			int ts = tileSize;
			int tw = (gw + ts - 1) / ts;
			int th = (gh + ts - 1) / ts;
			return new int[]{tw, th};
		}

		/// @par
		///
		/// Only sets the area id's for the walkable triangles.  Does not alter the
		/// area id's for unwalkable triangles.
		/// 
		/// See the #rcConfig documentation for more information on the configuration parameters.
		/// 
		/// @see rcHeightfield, rcClearUnwalkableTriangles, rcRasterizeTriangles
		internal static int[] markWalkableTriangles(Context ctx, float walkableSlopeAngle, float[] verts, int[] tris, int nt)
		{
			int[] areas = new int[nt];
			float walkableThr = (float) Math.Cos(walkableSlopeAngle / 180.0f * Math.PI);
			float[] norm = new float[3];
			for (int i = 0; i < nt; ++i)
			{
				int tri = i * 3;
				calcTriNormal(verts, tris[tri], tris[tri + 1], tris[tri + 2], norm);
				// Check if the face is walkable.
				if (norm[1] > walkableThr)
				{
                    areas[i] = RecastConstants.RC_WALKABLE_AREA;
				}
			}
			return areas;
		}

		internal static void calcTriNormal(float[] verts, int v0, int v1, int v2, float[] norm)
		{
			float[] e0 = new float[3]; float[] e1 = new float[3];
			RecastVectors.sub(e0, verts, v1 * 3, v0 * 3);
			RecastVectors.sub(e1, verts, v2 * 3, v0 * 3);
			RecastVectors.cross(norm, e0, e1);
			RecastVectors.normalize(norm);
		}

		/// @par
		///
		/// Only sets the area id's for the unwalkable triangles.  Does not alter the
		/// area id's for walkable triangles.
		/// 
		/// See the #rcConfig documentation for more information on the configuration parameters.
		/// 
		/// @see rcHeightfield, rcClearUnwalkableTriangles, rcRasterizeTriangles
		internal virtual void clearUnwalkableTriangles(Context ctx, float walkableSlopeAngle, float[] verts, int nv, int[] tris, int nt, int[] areas)
		{
			float walkableThr = (float) Math.Cos(walkableSlopeAngle / 180.0f * Math.PI);

			float[] norm = new float[3];

			for (int i = 0; i < nt; ++i)
			{
				int tri = i * 3;
				calcTriNormal(verts, tris[tri], tris[tri + 1], tris[tri + 2], norm);
				// Check if the face is walkable.
				if (norm[1] <= walkableThr)
				{
                    areas[i] = RecastConstants.RC_NULL_AREA;
				}
			}
		}

		internal static int getHeightFieldSpanCount(Context ctx, Heightfield hf)
		{
			int w = hf.width;
			int h = hf.height;
			int spanCount = 0;
			for (int y = 0; y < h; ++y)
			{
				for (int x = 0; x < w; ++x)
				{
					for (Span s = hf.spans[x + y * w]; s != null; s = s.next)
					{
                        if (s.area != RecastConstants.RC_NULL_AREA)
						{
							spanCount++;
						}
					}
				}
			}
			return spanCount;
		}

		/// @par
		///
		/// This is just the beginning of the process of fully building a compact heightfield.
		/// Various filters may be applied, then the distance field and regions built.
		/// E.g: #rcBuildDistanceField and #rcBuildRegions
		///
		/// See the #rcConfig documentation for more information on the configuration parameters.
		///
		/// @see rcAllocCompactHeightfield, rcHeightfield, rcCompactHeightfield, rcConfig

		public static CompactHeightfield buildCompactHeightfield(Context ctx, int walkableHeight, int walkableClimb, Heightfield hf)
		{

			ctx.startTimer("BUILD_COMPACTHEIGHTFIELD");

			CompactHeightfield chf = new CompactHeightfield();
			int w = hf.width;
			int h = hf.height;
			int spanCount = getHeightFieldSpanCount(ctx, hf);

			// Fill in header.
			chf.width = w;
			chf.height = h;
			chf.spanCount = spanCount;
			chf.walkableHeight = walkableHeight;
			chf.walkableClimb = walkableClimb;
			chf.maxRegions = 0;
            RecastVectors.copy(chf.bmin, hf.bmin);
            RecastVectors.copy(chf.bmax, hf.bmax);
			chf.bmax[1] += walkableHeight * hf.ch;
			chf.cs = hf.cs;
			chf.ch = hf.ch;
			chf.cells = new CompactCell[w * h];
			chf.spans = new CompactSpan[spanCount];
			chf.areas = new int[spanCount];
			int MAX_HEIGHT = 0xffff;
			for (int i = 0; i < chf.cells.Length; i++)
			{
				chf.cells[i] = new CompactCell();
			}
			for (int i = 0; i < chf.spans.Length; i++)
			{
				chf.spans[i] = new CompactSpan();
			}
			// Fill in cells and spans.
			int idx = 0;
			for (int y = 0; y < h; ++y)
			{
				for (int x = 0; x < w; ++x)
				{
					Span s = hf.spans[x + y * w];
					// If there are no spans at this cell, just leave the data to index=0, count=0.
					if (s == null)
					{
						continue;
					}
					CompactCell c = chf.cells[x + y * w];
					c.index = idx;
					c.count = 0;
					while (s != null)
					{
                        if (s.area != RecastConstants.RC_NULL_AREA)
						{
							int bot = s.smax;
							int top = s.next != null ? (int) s.next.smin : MAX_HEIGHT;
							chf.spans[idx].y = RecastCommon.clamp(bot, 0, 0xffff);
							chf.spans[idx].h = RecastCommon.clamp(top - bot, 0, 0xff);
							chf.areas[idx] = s.area;
							idx++;
							c.count++;
						}
						s = s.next;
					}
				}
			}

			// Find neighbour connections.
            int MAX_LAYERS = RecastConstants.RC_NOT_CONNECTED - 1;
			int tooHighNeighbour = 0;
			for (int y = 0; y < h; ++y)
			{
				for (int x = 0; x < w; ++x)
				{
					CompactCell c = chf.cells[x + y * w];
					for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
					{
						CompactSpan s = chf.spans[i];

						for (int dir = 0; dir < 4; ++dir)
						{
                            RecastCommon.SetCon(s, dir, RecastConstants.RC_NOT_CONNECTED);
							int nx = x + RecastCommon.GetDirOffsetX(dir);
							int ny = y + RecastCommon.GetDirOffsetY(dir);
							// First check that the neighbour cell is in bounds.
							if (nx < 0 || ny < 0 || nx >= w || ny >= h)
							{
								continue;
							}

							// Iterate over all neighbour spans and check if any of the is
							// accessible from current cell.
							CompactCell nc = chf.cells[nx + ny * w];
							for (int k = nc.index, nk = nc.index + nc.count; k < nk; ++k)
							{
								CompactSpan ns = chf.spans[k];
								int bot = Math.Max(s.y, ns.y);
								int top = Math.Min(s.y + s.h, ns.y + ns.h);

								// Check that the gap between the spans is walkable,
								// and that the climb height between the gaps is not too high.
								if ((top - bot) >= walkableHeight && Math.Abs(ns.y - s.y) <= walkableClimb)
								{
									// Mark direction as walkable.
									int lidx = k - nc.index;
									if (lidx < 0 || lidx > MAX_LAYERS)
									{
										tooHighNeighbour = Math.Max(tooHighNeighbour, lidx);
										continue;
									}
									RecastCommon.SetCon(s, dir, lidx);
									break;
								}
							}

						}
					}
				}
			}

			if (tooHighNeighbour > MAX_LAYERS)
			{
				throw new Exception("rcBuildCompactHeightfield: Heightfield has too many layers " + tooHighNeighbour + " (max: " + MAX_LAYERS + ")");
			}
			ctx.stopTimer("BUILD_COMPACTHEIGHTFIELD");
			return chf;
		}
	}

}