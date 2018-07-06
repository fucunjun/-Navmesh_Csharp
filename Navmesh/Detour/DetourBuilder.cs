﻿namespace org.recast4j.detour
{

	public class DetourBuilder
	{

		public virtual MeshData build(NavMeshCreateParams @params, int tileX, int tileY)
		{
			MeshData data = NavMeshBuilder.createNavMeshData(@params);
			if (data != null)
			{
				data.header.x = tileX;
				data.header.y = tileY;
			}
			return data;
		}
	}

}