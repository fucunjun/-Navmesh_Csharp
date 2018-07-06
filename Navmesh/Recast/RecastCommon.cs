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

	public class RecastCommon
	{

		/// Gets neighbor connection data for the specified direction.
		///  @param[in]		s		The span to check.
		///  @param[in]		dir		The direction to check. [Limits: 0 <= value < 4]
		///  @return The neighbor connection data for the specified direction,
		///  	or #RC_NOT_CONNECTED if there is no connection.	
		internal static int GetCon(CompactSpan s, int dir)
		{
			int shift = dir * 6;
			return (s.con >> shift) & 0x3f;
		}

		/// Gets the standard width (x-axis) offset for the specified direction.
		///  @param[in]		dir		The direction. [Limits: 0 <= value < 4]
		///  @return The width offset to apply to the current cell position to move
		///  	in the direction.
		internal static int GetDirOffsetX(int dir)
		{
			int[] offset = { -1, 0, 1, 0, };
			return offset[dir & 0x03];
		}

		/// Gets the standard height (z-axis) offset for the specified direction.
		///  @param[in]		dir		The direction. [Limits: 0 <= value < 4]
		///  @return The height offset to apply to the current cell position to move
		///  	in the direction.
		internal static int GetDirOffsetY(int dir)
		{
			int[] offset = { 0, 1, 0, -1 };
			return offset[dir & 0x03];
		}

		/// Sets the neighbor connection data for the specified direction.
		///  @param[in]		s		The span to update.
		///  @param[in]		dir		The direction to set. [Limits: 0 <= value < 4]
		///  @param[in]		i		The index of the neighbor span.
		public static void SetCon(CompactSpan s, int dir, int i)
		{
			int shift = dir * 6;
			int con = s.con;
			s.con = (con & ~(0x3f << shift)) | ((i & 0x3f) << shift);
		}

		public static int clamp(int v, int min, int max)
		{
			return Math.Max(Math.Min(max, v), min);
		}

	}

}