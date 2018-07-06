﻿/*
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

using System;
using System.Collections.Generic;

namespace org.recast4j.detour
{

	public class MoveAlongSurfaceResult
	{

		/// <summary>
		/// The result position of the mover. [(x, y, z)] </summary>
		private readonly float[] resultPos;
		/// <summary>
		/// The reference ids of the polygons visited during the move. </summary>
		private readonly List<long> visited;

		public MoveAlongSurfaceResult(float[] resultPos, List<long> visited)
		{
			this.resultPos = resultPos;
			this.visited = visited;
		}

		public virtual float[] ResultPos
		{
			get
			{
				return resultPos;
			}
		}

		public virtual List<long> Visited
		{
			get
			{
				return visited;
			}
		}

	}

}