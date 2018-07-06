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
namespace org.recast4j.recast
{

	/// <summary>
	/// Represents a heightfield layer within a layer set. </summary>
	public class Heightfield
	{

		/// <summary>
		/// The width of the heightfield. (Along the x-axis in cell units.) </summary>
		public readonly int width;
		/// <summary>
		/// The height of the heightfield. (Along the z-axis in cell units.) </summary>
		public readonly int height;
		/// <summary>
		/// The minimum bounds in world space. [(x, y, z)] </summary>
		public readonly float[] bmin;
		/// <summary>
		/// The maximum bounds in world space. [(x, y, z)] </summary>
		public readonly float[] bmax;
		/// <summary>
		/// The size of each cell. (On the xz-plane.) </summary>
		public readonly float cs;
		/// <summary>
		/// The height of each cell. (The minimum increment along the y-axis.) </summary>
		public readonly float ch;
		/// <summary>
		/// Heightfield of spans (width*height). </summary>
		public readonly Span[] spans;

		public Heightfield(int width, int height, float[] bmin, float[] bmax, float cs, float ch)
		{
			this.width = width;
			this.height = height;
			this.bmin = bmin;
			this.bmax = bmax;
			this.cs = cs;
			this.ch = ch;
			this.spans = new Span[width * height];

		}
	}

}