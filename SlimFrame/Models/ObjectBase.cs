using SlimDX;
using SlimDX.Direct3D10;
using System;
using System.Collections.Generic;
using System.IO;
using static System.Diagnostics.Debug;
using static System.Math;

namespace SlimFrame.Models
{
	public class ObjectBase
	{
		public const ushort ObjectID = 45056;

		public const ushort VerticesID = 45057;

		public const ushort IndicesID = 45058;

		public const ushort MaterialID = 45059;

		public const ushort SubsetsID = 45060;

		public const ushort TransformationID = 45062;

		public const ushort SubsetNamesID = 45063;

		public Vertex[] vertices;

		public uint[] indices;

		public Matrix Tranformation;

		public List<int> meshSubsets;

		public List<string> SubsetNames;

		public List<int> MaterialIDs;

		public ObjectBase()
		{
			meshSubsets = new List<int>();
			MaterialIDs = new List<int>();

			SubsetNames = new List<string>();
		}

		public void Write(BinaryWriter writer)
		{
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			writer.Write(45056);
			list.Add((int)writer.BaseStream.Position);
			writer.Write(0);
			writer.Write(45057);
			list.Add((int)writer.BaseStream.Position);
			writer.Write(0);
			writer.Write(vertices.Length);
			Vertex[] array = vertices;
			for (int i = 0; i < array.Length; i++)
			{
				Vertex vertex = array[i];
				vertex.Write(writer);
			}
			list2.Add((int)writer.BaseStream.Position);
			writer.Write(45058);
			list.Add((int)writer.BaseStream.Position);
			writer.Write(0);
			writer.Write(indices.Length);
			uint[] array2 = indices;
			for (int j = 0; j < array2.Length; j++)
			{
				int value = (int)array2[j];
				writer.Write(value);
			}
			list2.Add((int)writer.BaseStream.Position);
			writer.Write(45059);
			list.Add((int)writer.BaseStream.Position);
			writer.Write(0);
			writer.Write(MaterialIDs.Count);
			foreach (int current in MaterialIDs)
			{
				writer.Write(current);
			}
			list2.Add((int)writer.BaseStream.Position);
			writer.Write(45060);
			list.Add((int)writer.BaseStream.Position);
			writer.Write(0);
			writer.Write(meshSubsets.Count);
			foreach (int current2 in meshSubsets)
			{
				writer.Write(current2);
			}
			writer.Write(45063);
			list.Add((int)writer.BaseStream.Position);
			writer.Write(0);
			foreach (string current3 in SubsetNames)
			{
				writer.Write(current3);
			}
			list2.Add((int)writer.BaseStream.Position);
			list2.Add((int)writer.BaseStream.Position);
			writer.Write(45062);
			list.Add((int)writer.BaseStream.Position);
			writer.Write(70);
			Object3D.WriteMatrix(writer, Tranformation);
			list2.Add((int)writer.BaseStream.Position);
			for (int k = 1; k < list.Count; k++)
			{
				writer.Seek(list[k], SeekOrigin.Begin);
				writer.Write(list2[k - 1] - list[k] + 2);
			}
			writer.Seek(list[0], SeekOrigin.Begin);
			writer.Write(list2[list2.Count - 1] - list[0] + 2);
			writer.Seek(list2[list2.Count - 1], SeekOrigin.Begin);
		}

		public void ApplySubsets(int[] subsets, MaterialBase[] materials)
		{
			for (int i = 0; i < subsets.Length; i++)
			{
				int num = subsets[i];
				bool flag = num == 0;
				if (!flag)
				{
					this.meshSubsets.Insert(1, num);
				}
			}
			for (int j = 0; j < materials.Length; j++)
			{
				MaterialBase materialBase = materials[j];
				this.MaterialIDs.Add(materialBase.ID);
			}
		}

		public void Read(BinaryReader reader, long ChunkEnding)
		{
			Chunk chunk = default(Chunk);
			while (true)
			{
				chunk.Read(reader);
				switch (chunk.id)
				{
					case 45057:
						{
							int num = reader.ReadInt32();
							vertices = new Vertex[num];
							for (int i = 0; i < num; i++)
							{
								vertices[i] = Vertex.Read(reader);
							}
							break;
						}
					case 45058:
						{
							int num2 = reader.ReadInt32();
							indices = new uint[num2];
							for (int j = 0; j < num2; j++)
							{
								indices[j] = reader.ReadUInt32();
							}
							break;
						}
					case 45059:
						{
							int num3 = reader.ReadInt32();
							for (int k = 0; k < num3; k++)
							{
								MaterialIDs.Add(reader.ReadInt32());
							}
							break;
						}
					case 45060:
						{
							int num4 = reader.ReadInt32();
							for (int l = 0; l < num4; l++)
							{
								meshSubsets.Add(reader.ReadInt32());
							}
							break;
						}
					case 45061:
						goto IL_13C;
					case 45062:
						Object3D.ReadMatrix(reader, ref Tranformation);
						break;
					default:
						goto IL_13C;
				}
				IL_152:
				if (reader.BaseStream.Position + 6L >= ChunkEnding)
				{
					break;
				}
				continue;
				IL_13C:
				reader.BaseStream.Seek(chunk.End, SeekOrigin.Begin);
				goto IL_152;
			}
			meshSubsets.Add(0);
			meshSubsets.Add(indices.Length);
		}

		public void ComputeNormal(bool isRHCoord = false)
        {
			List<Vector3> tempNormal = new List<Vector3>();
			int meshTriangles = indices.Length / 3;

			//normalized and unnormalized normals
			Vector3 unnormalized = new Vector3();

			//Two edges of our triangle
			Vector3 edge1 = new Vector3();
			Vector3 edge2 = new Vector3();

			//Compute face normals
			for (int i = 0; i < meshTriangles; ++i)
			{
				//Get the vector describing one edge of our triangle (edge 0,2)
				edge1 = vertices[(int)indices[(i * 3) + 0]].Position - vertices[(int)indices[(i * 3) + 2]].Position;    //Create our first edge

				//Get the vector describing another edge of our triangle (edge 2,1)
				edge2 = vertices[(int)indices[(i * 3) + 2]].Position - vertices[(int)indices[(i * 3) + 1]].Position;    //Create our first edge

				//Cross multiply the two edge vectors to get the un-normalized face normal
				unnormalized = (isRHCoord ? -1 : 1) * Vector3.Cross(edge1, edge2);
				tempNormal.Add(unnormalized);            //Save unormalized normal (for normal averaging)
			}

			//Compute vertex normals (normal Averaging)
			Vector3 normalSum = new Vector3();
			int facesUsing = 0;

			//Go through each vertex
			for (int i = 0; i < vertices.Length; ++i)
			{
				//Check which triangles use this vertex
				for (int j = 0; j < meshTriangles; ++j)
				{
					if (indices[j * 3] == i ||
						indices[(j * 3) + 1] == i ||
						indices[(j * 3) + 2] == i)
					{
						normalSum += tempNormal[j];
						//If a face is using the vertex, add the unormalized face normal to the normalSum
						facesUsing++;
					}
				}

				//Get the actual normal by dividing the normalSum by the number of faces sharing the vertex
				normalSum = normalSum / Max(facesUsing, 1);

				//Store the normal in our current vertex
				vertices[i].Normal = Vector3.Normalize(normalSum);

				//Clear normalSum and facesUsing for next vertex
				normalSum = new Vector3();
				facesUsing = 0;
			}
		}
		
		public static void ComputeNormal(ref Vertex[] vertices, uint[] indices, bool isRHCoord = false)
        {
			int triangles = indices.Length / 3;

			List<Vector3> tempNormal = new List<Vector3>();

			//normalized and unnormalized normals
			Vector3 unnormalized = new Vector3();

			//Two edges of our triangle
			Vector3 edge1 = new Vector3();
			Vector3 edge2 = new Vector3();

			//WriteLine("Raw Normals");

			//Compute face normals
			for (int i = 0; i < triangles; ++i)
			{
				//Get the vector describing one edge of our triangle (edge 0,2)
				edge1 = vertices[(int)indices[(i * 3) + 0]].Position - vertices[(int)indices[(i * 3) + 2]].Position;    //Create our first edge

				//Get the vector describing another edge of our triangle (edge 2,1)
				edge2 = vertices[(int)indices[(i * 3) + 2]].Position - vertices[(int)indices[(i * 3) + 1]].Position;    //Create our first edge

				//Cross multiply the two edge vectors to get the un-normalized face normal
				unnormalized = (isRHCoord ? -1 : 1) * Vector3.Cross(edge1, edge2);
				//WriteLine(unnormalized);

				tempNormal.Add(unnormalized);            //Save unormalized normal (for normal averaging)
			}

			//Compute vertex normals (normal Averaging)
			Vector3 normalSum = new Vector3();
			int facesUsing = 0;
			Vector3 t;

			//WriteLine("Normals");
			//Go through each vertex
			for (int i = 0; i < vertices.Length; ++i)
			{
				//Check which triangles use this vertex
				for (int j = 0; j < triangles; ++j)
				{
					if (indices[j * 3] == i ||
						indices[(j * 3) + 1] == i ||
						indices[(j * 3) + 2] == i)
					{
						t = normalSum + tempNormal[j];

						normalSum = t;    //If a face is using the vertex, add the unormalized face normal to the normalSum
						facesUsing++;
					}
				}

				//Get the actual normal by dividing the normalSum by the number of faces sharing the vertex
				normalSum = normalSum / Max(facesUsing, 1);

				//Store the normal in our current vertex
				vertices[i].Normal = Vector3.Normalize(normalSum);
				//WriteLine(vertices[i].Normal);

				//Clear normalSum and facesUsing for next vertex
				normalSum = new Vector3();
				facesUsing = 0;
			}
		}

		public void Clear()
		{
			vertices = null;
			indices = null;
			Tranformation = Matrix.Identity;
			MaterialIDs.Clear();
			meshSubsets.Clear();
			SubsetNames.Clear();
		}
	}
}
