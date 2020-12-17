using SlimDX;
using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using static System.Diagnostics.Debug;

namespace SlimFrame.Models
{
	public class Object3D : IDisposable
	{
		public Matrix Tranformation;

		private bool disposedValue = false;

		public int VertexCount
		{
			get;
			protected set;
		}

		public int IndexCount
		{
			get;
			protected set;
		}

		public SlimDX.Direct3D11.Buffer VertexBuffer
		{
			get;
			protected set;
		}

		public SlimDX.Direct3D11.Buffer IndexBuffer
		{
			get;
			protected set;
		}

		public List<int> Subsets
		{
			get;
			protected set;
		}

		public int SubsetCount
		{
			get;
			protected set;
		}

		public List<int> MaterialIDs
		{
			get;
			protected set;
		}

		public VertexBufferBinding vertexBufferBinding
		{
			get;
			protected set;
		}

		public List<string> SubsetNames
		{
			get;
			protected set;
		}

		public Object3D()
		{
		}

		public Object3D(Device dev, Vertex[] Vertices, uint[] Indices, int MaterialID, string meshName, bool computeNormal = false, bool isRHCoordSys = false, bool Debug = false)
		{
			IndexCount = Indices.Length;
			VertexCount = Vertices.Length;
			MaterialIDs = new List<int>();
			Subsets = new List<int>();
			SubsetNames = new List<string>();

			Vertex[] temp =  new Vertex[Vertices.Length];
			Array.Copy(Vertices, temp, Vertices.Length);

			if (computeNormal)
				ObjectBase.ComputeNormal(ref temp, Indices, isRHCoordSys);

			DataStream dataStream = new DataStream(Vertex.Size * temp.Length, true, true);
			for (int i = 0; i < temp.Length; i++)
			{
				dataStream.Write(temp[i]);
			}
			dataStream.Position = 0L;
			VertexBuffer = new SlimDX.Direct3D11.Buffer(dev, dataStream, new BufferDescription
			{
				Usage = ResourceUsage.Default,
				SizeInBytes = Vertex.Size * temp.Length,
				BindFlags = BindFlags.VertexBuffer,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None,
				StructureByteStride = Vertex.Size
			});
			dataStream.Dispose();
			dataStream = new DataStream(sizeof(uint) * Indices.Length, true, true);
			for (int j = 0; j < Indices.Length; j++)
			{
				int value2 = (int)Indices[j];
				dataStream.Write(value2);
			}
			dataStream.Position = 0L;
			IndexBuffer = new SlimDX.Direct3D11.Buffer(dev, dataStream, new BufferDescription
			{
				Usage = ResourceUsage.Default,
				SizeInBytes = 4 * Indices.Length,
				BindFlags = BindFlags.IndexBuffer,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None,
				StructureByteStride = 4,
			});
			vertexBufferBinding = new VertexBufferBinding(VertexBuffer, Vertex.Size, 0);

			if (Debug)
				WriteLine("mat count :- " + 1);

			if (MaterialID >= 0)
				MaterialIDs.Add(MaterialID);
			if (Debug)
				WriteLine("mat id :- " + MaterialID);

			VertexCount = Vertices.Length;
			IndexCount = Indices.Length;

			Subsets.Add(0);
			Subsets.Add(IndexCount);

			if (meshName != null)
				SubsetNames.Add(meshName);

			SubsetCount = 1;

			Tranformation = Matrix.Scaling(1f, 1f, 1f);
		}
		
		public Object3D(Device dev, Vertex[] Vertices, uint[] Indices, int[] Materials = null, int[] meshSubsets = null, string[] meshSubsetNames = null, bool computeNormal = false, bool Debug = false)
		{
			IndexCount = Indices.Length;
			VertexCount = Vertices.Length;
			MaterialIDs = new List<int>();
			Subsets = new List<int>();
			SubsetNames = new List<string>();

			if (computeNormal)
				 ObjectBase.ComputeNormal(ref Vertices, Indices);

			DataStream dataStream = new DataStream((long)Vertex.Size * Vertices.Length, true, true);
			for (int i = 0; i < Vertices.Length; i++)
			{
				Vertex value = Vertices[i];
				dataStream.Write(value);
			}
			dataStream.Position = 0L;
			VertexBuffer = new SlimDX.Direct3D11.Buffer(dev, dataStream, new BufferDescription
			{
				Usage = ResourceUsage.Default,
				SizeInBytes = Vertex.Size * Vertices.Length,
				BindFlags = BindFlags.VertexBuffer,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None,
				StructureByteStride = Vertex.Size
			});
			dataStream.Dispose();
			dataStream = new DataStream(4 * Indices.Length, true, true);
			for (int j = 0; j < Indices.Length; j++)
			{
				int value2 = (int)Indices[j];
				dataStream.Write(value2);
			}
			dataStream.Position = 0L;
			IndexBuffer = new SlimDX.Direct3D11.Buffer(dev, dataStream, new BufferDescription
			{
				Usage = ResourceUsage.Default,
				SizeInBytes = 4 * Indices.Length,
				BindFlags = BindFlags.IndexBuffer,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None,
				StructureByteStride = 4
			});
			vertexBufferBinding = new VertexBufferBinding(VertexBuffer, Vertex.Size, 0);
			if (Materials != null)
			{

				if (Debug)
				{
					WriteLine("mat count :- " + Materials.Length.ToString());
				}
				for (int k = 0; k < Materials.Length; k++)
				{
					int item = Materials[k];
					MaterialIDs.Add(item);
					if (Debug)
					{
						WriteLine("mat id :- " + item.ToString());
					}
				}
			}
			Subsets.Add(0);
			if (meshSubsets != null)
			{
				if (Debug)
				{
					WriteLine("sub count :- " + (meshSubsets.Length - 1).ToString());
				}
				for (int l = 0; l < meshSubsets.Length; l++)
				{
					int subsetIndex = meshSubsets[l];
					if (!Subsets.Contains(subsetIndex) && (subsetIndex < Vertices.Length && subsetIndex > 0))
					{
						Subsets.Add(subsetIndex);
					}
				}
			}

			Subsets.Add(Vertices.Length);

            if (meshSubsetNames != null)
            {
                for (int m = 0; m < meshSubsetNames.Length; m++)
                {
                    string item3 = meshSubsetNames[m];
                    SubsetNames.Add(item3);
                }
            }
            SubsetCount = Subsets.Count - 1;

            Tranformation = Matrix.Scaling(1f, 1f, 1f);
		}

		public Object3D(Device dev, ObjectBase objectData, bool ComputeNormal,bool isRHCoordSys = false, bool Debug = false)
		{
			IndexCount = objectData.indices.Length;
			VertexCount = objectData.vertices.Length;
			MaterialIDs = new List<int>();
			Subsets = new List<int>();
			DataStream dataStream = new DataStream((long)(Vertex.Size * objectData.vertices.Length), true, true);

			if (ComputeNormal)
				objectData.ComputeNormal(isRHCoordSys);

			Vertex[] vertices = objectData.vertices;
			for (int i = 0; i < vertices.Length; i++)
			{
				Vertex value = vertices[i];
				dataStream.Write<Vertex>(value);
			}
			dataStream.Position = 0L;
			VertexBuffer = new SlimDX.Direct3D11.Buffer(dev, dataStream, new BufferDescription
			{
				Usage = ResourceUsage.Default,
				SizeInBytes = Vertex.Size * objectData.vertices.Length,
				BindFlags = BindFlags.VertexBuffer,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None,
				StructureByteStride = Vertex.Size
			});
			dataStream.Dispose();
			dataStream = new DataStream(4L * objectData.indices.Length, true, true);
			uint[] indices = objectData.indices;
			for (int j = 0; j < indices.Length; j++)
			{
				int value2 = (int)indices[j];
				dataStream.Write<int>(value2);
			}
			dataStream.Position = 0L;
			IndexBuffer = new SlimDX.Direct3D11.Buffer(dev, dataStream, new BufferDescription
			{
				Usage = ResourceUsage.Default,
				SizeInBytes = 4 * objectData.indices.Length,
				BindFlags = BindFlags.IndexBuffer,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None,
				StructureByteStride = 4
			});
			vertexBufferBinding = new VertexBufferBinding(this.VertexBuffer, Vertex.Size, 0);
			Tranformation = objectData.Tranformation;
			if (Debug)
			{
				WriteLine("mat count :- " + objectData.MaterialIDs.Count.ToString());
			}
			foreach (int current in objectData.MaterialIDs)
			{
				MaterialIDs.Add(current);
				if (Debug)
				{
					WriteLine("mat id :- " + current.ToString());
				}
			}
			if (Debug)
			{
				WriteLine("sub count :- " + (objectData.meshSubsets.Count - 1).ToString());
			}

			Subsets.Add(0);
			foreach (int index in objectData.meshSubsets)
			{
				if (index > 0 && index < objectData.indices.Length)
				{
					Subsets.Add(index);
					if (Debug)
					{
						WriteLine("sub index :- " + index.ToString());
					}
				}
			}
			Subsets.Add(objectData.indices.Length);

			SubsetCount = Subsets.Count - 1;
			Tranformation = Matrix.Scaling(1f, 1f, 1f);
		}

		public void Draw(DeviceContext devCon, int startIndexLocation = 0, int baseVertexLocation = 0)
		{
			devCon.DrawIndexed(this.IndexCount, 0, 0);
		}

		public void DrawSubset(DeviceContext devCon, int subset)
		{
			devCon.DrawIndexed(this.Subsets[subset + 1] - this.Subsets[subset], this.Subsets[subset], 0);
		}

		public void DrawInstanced(DeviceContext devCon, int instanceCount, int startIndexLocation = 0, int baseVertexLocation = 0, int startInstanceLocation = 0)
		{
			devCon.DrawIndexedInstanced(this.IndexCount, instanceCount, startIndexLocation, baseVertexLocation, startInstanceLocation);
		}

		public void DrawInstancedSubset(DeviceContext devCon, int subset, int instanceCount, int baseVertexLocation = 0, int startInstanceLocation = 0)
		{
			devCon.DrawIndexedInstanced(this.Subsets[subset + 1] - this.Subsets[subset], instanceCount, this.Subsets[subset], baseVertexLocation, startInstanceLocation);
		}

		public void Write(DeviceContext devCon, BinaryWriter writer)
		{
			Object3D.Write(devCon, writer, this);
		}

		public void Read(Device device, BinaryReader reader, long ChunkEnding)
		{
			Object3D.Read(this, device, reader, ChunkEnding);
		}

		public static Object3D FromStream(Device device, BinaryReader reader, long ChunkEnding)
		{
			Object3D object3D = new Object3D();
			Read(object3D, device, reader, ChunkEnding);
			return object3D;
		}

		public static void Read(Object3D obj, Device device, BinaryReader reader, long ChunkEnding)
		{
			obj.Subsets = new List<int>();
			Chunk chunk = new Chunk();
			obj.MaterialIDs = new List<int>();
			obj.Subsets = new List<int>();
			DataStream dataStream;

			/* 0xB001 : vertices ID
			 * 0xB001 : Indices ID
			 * 0xB003 : Materials ID
			 * 0xB004 : Subsets ID
			 * 0xB005 : Undefined
			 * 0xB006 : Transfromation		 
			 */
			do
			{				
				chunk.Read(reader);

                switch (chunk.id)
                {
                    case 0xB001:
                        int VertexCount = reader.ReadInt32();

                        dataStream = new DataStream(Vertex.Size * VertexCount, true, true);
                        for (int i = 0; i < VertexCount; i++)
                        {
                            dataStream.Write(Vertex.Read(reader));
                        }
                        dataStream.Position = 0L;
                        obj.VertexBuffer = new SlimDX.Direct3D11.Buffer(device, dataStream, new BufferDescription
                        {
                            Usage = ResourceUsage.Default,
                            SizeInBytes = Vertex.Size * VertexCount,
                            BindFlags = BindFlags.VertexBuffer,
                            CpuAccessFlags = CpuAccessFlags.None,
                            OptionFlags = ResourceOptionFlags.None,
                            StructureByteStride = Vertex.Size
                        });
                        obj.vertexBufferBinding = new VertexBufferBinding(obj.VertexBuffer, Vertex.Size, 0);
                        dataStream.Dispose();
                        obj.VertexCount = VertexCount;
                        break;
                    case 0xB002:
                        int indexCount = reader.ReadInt32();

                        dataStream = new DataStream(4 * indexCount, true, true);
                        for (int j = 0; j < indexCount; j++)
                        {
                            dataStream.Write(reader.ReadUInt32());
                        }
                        dataStream.Position = 0L;
                        obj.IndexBuffer = new SlimDX.Direct3D11.Buffer(device, dataStream, new BufferDescription
                        {
                            Usage = ResourceUsage.Default,
                            SizeInBytes = 4 * indexCount,
                            BindFlags = BindFlags.IndexBuffer,
                            CpuAccessFlags = CpuAccessFlags.None,
                            OptionFlags = ResourceOptionFlags.None,
                            StructureByteStride = 4
                        });
                        dataStream.Dispose();
                        obj.IndexCount = indexCount;
                        break;
                    case 0xB003:
                        int materials = reader.ReadInt32();
                        for (int k = 0; k < materials; k++)
                        {
                            obj.MaterialIDs.Add(reader.ReadInt32());
                        }
                        break;
                    case 0xB004:
                        int subsets = reader.ReadInt32();
                        for (int l = 0; l < subsets; l++)
                        {
                            obj.Subsets.Add(reader.ReadInt32());
                        }
                        break;
                    case 0xB005:
                        reader.BaseStream.Seek(chunk.End, SeekOrigin.Begin);
                        break;
                    case 0xB006:
                        ReadMatrix(reader, ref obj.Tranformation);
                        break;
                    default:
						break;
				}
				chunk.SeektoEnd();
            } while (reader.BaseStream.Position + 6 <= ChunkEnding);

			obj.SubsetCount = obj.Subsets.Count - 1;
		}

		public unsafe static void Write(DeviceContext devCon, BinaryWriter writer, Object3D obj)
		{
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();

			writer.Write((ushort)0xB000);
			list.Add((int)writer.BaseStream.Position);
			writer.Write(0);

			writer.Write((ushort)0xB001);
			list.Add((int)writer.BaseStream.Position);
			writer.Write(0);
			writer.Write(obj.VertexCount);

			BufferDescription description = default(BufferDescription);
			description.Usage = ResourceUsage.Dynamic;
			description.SizeInBytes = Vertex.Size * obj.VertexCount;
			description.BindFlags = BindFlags.VertexBuffer;
			description.CpuAccessFlags = CpuAccessFlags.Write;
			description.OptionFlags = ResourceOptionFlags.None;
			description.StructureByteStride = Vertex.Size;
			DataStream dataStream = new DataStream((long)(Vertex.Size * obj.VertexCount), true, true);
			SlimDX.Direct3D11.Buffer buffer = new SlimDX.Direct3D11.Buffer(devCon.Device, dataStream, description);
			devCon.CopyResource(obj.VertexBuffer, buffer);
			DataBox dataBox = devCon.MapSubresource(buffer, MapMode.WriteNoOverwrite, MapFlags.None);
			Vertex* ptr = (Vertex*)((void*)dataBox.Data.DataPointer);
			for (int i = 0; i < obj.VertexCount; i++)
			{
				Vertex vertex = ptr[i];
				vertex.Write(writer);
			}
			devCon.UnmapSubresource(buffer, 0);
			dataStream.Dispose();
			buffer.Dispose();
			dataBox.Data.Dispose();

			list2.Add((int)writer.BaseStream.Position);
			writer.Write((ushort) 0xB002);
			list.Add((int)writer.BaseStream.Position);
			writer.Write(0);
			writer.Write(obj.IndexCount);

			description = default(BufferDescription);
			description.Usage = ResourceUsage.Dynamic;
			description.SizeInBytes = 4 * obj.IndexCount;
			description.BindFlags = BindFlags.IndexBuffer;
			description.CpuAccessFlags = CpuAccessFlags.Write;
			description.OptionFlags = ResourceOptionFlags.None;
			description.StructureByteStride = 4;
			dataStream = new DataStream(4 * obj.IndexCount, true, true);
			SlimDX.Direct3D11.Buffer buffer2 = new SlimDX.Direct3D11.Buffer(devCon.Device, dataStream, description);
			devCon.CopyResource(obj.IndexBuffer, buffer2);
			dataBox = devCon.MapSubresource(buffer2, MapMode.WriteNoOverwrite, MapFlags.None);
			uint* ptr2 = (uint*)((void*)dataBox.Data.DataPointer);
			for (int j = 0; j < obj.IndexCount; j++)
			{
				uint value = ptr2[j];
				writer.Write(value);
			}
			devCon.UnmapSubresource(buffer2, 0);
			dataStream.Dispose();
			buffer2.Dispose();
			dataBox.Data.Dispose();

			list2.Add((int)writer.BaseStream.Position);
			writer.Write((ushort)0xB003);
			list.Add((int)writer.BaseStream.Position);
			writer.Write(0);
			writer.Write(obj.MaterialIDs.Count);
			foreach (int current in obj.MaterialIDs)
			{
				writer.Write(current);
			}
			list2.Add((int)writer.BaseStream.Position);
			writer.Write((ushort)0xB004);
			list.Add((int)writer.BaseStream.Position);
			writer.Write(0);
			writer.Write(obj.Subsets.Count);
			foreach (int current2 in obj.Subsets)
			{
				writer.Write(current2);
			}
			list2.Add((int)writer.BaseStream.Position);
			writer.Write((ushort)0xB006);
			list.Add((int)writer.BaseStream.Position);
			writer.Write(70);
			WriteMatrix(writer, obj.Tranformation);
			list2.Add((int)writer.BaseStream.Position);

			for (int k = 1; k < list.Count; k++)
			{
				writer.Seek(list[k], SeekOrigin.Begin);
				writer.Write(list2[k - 1] - list[k] + 2);
			}

			writer.Seek(list.First(), SeekOrigin.Begin);
			writer.Write(list2.Last() - list.First() + 2);
			writer.Seek(list2.Last(), SeekOrigin.Begin);
		}

		public static void WriteMatrix(BinaryWriter writer, Matrix mat)
		{
			writer.Write(mat.M11);
			writer.Write(mat.M12);
			writer.Write(mat.M13);
			writer.Write(mat.M14);
			writer.Write(mat.M21);
			writer.Write(mat.M22);
			writer.Write(mat.M23);
			writer.Write(mat.M24);
			writer.Write(mat.M31);
			writer.Write(mat.M32);
			writer.Write(mat.M33);
			writer.Write(mat.M34);
			writer.Write(mat.M41);
			writer.Write(mat.M42);
			writer.Write(mat.M43);
			writer.Write(mat.M44);
		}

		public static void ReadMatrix(BinaryReader reader, ref Matrix mat)
		{
			mat.M11 = reader.ReadSingle();
			mat.M12 = reader.ReadSingle();
			mat.M13 = reader.ReadSingle();
			mat.M14 = reader.ReadSingle();
			mat.M21 = reader.ReadSingle();
			mat.M22 = reader.ReadSingle();
			mat.M23 = reader.ReadSingle();
			mat.M24 = reader.ReadSingle();
			mat.M31 = reader.ReadSingle();
			mat.M32 = reader.ReadSingle();
			mat.M33 = reader.ReadSingle();
			mat.M34 = reader.ReadSingle();
			mat.M41 = reader.ReadSingle();
			mat.M42 = reader.ReadSingle();
			mat.M43 = reader.ReadSingle();
			mat.M44 = reader.ReadSingle();

			if (mat.M11 + mat.M12 + mat.M13 + mat.M14 == 0f)
			{
				if (mat.M21 + mat.M22 + mat.M23 + mat.M24 == 0f)
				{
					if (mat.M31 + mat.M32 + mat.M33 + mat.M34 == 0f)
					{
						if (mat.M41 + mat.M42 + mat.M43 == 0f)
						{
							mat = Matrix.Scaling(1f, 1f, 1f);
						}
					}
				}
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			bool flag = !disposedValue;
			if (flag)
			{
				if (disposing)
				{
					VertexBuffer?.Dispose();
					IndexBuffer?.Dispose();
				}
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
	}
}
