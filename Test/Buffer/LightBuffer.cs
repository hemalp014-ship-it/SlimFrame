using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using SlimDX.Direct3D11;
using SlimFrame.Buffers;

namespace Demo.Buffer
{
    /// <summary>
    /// The constant buffer base
    /// </summary>
    public struct Light
    {
        public Vector3 pos;
        public float range;
        public Vector3 dir;
        public float cone;
        public Vector3 att;
        public int type;
        public Color4 ambient;
        public Color4 diffuse;

        public static void Write(Light light, DataStream dataStream)
        {
            dataStream.Write(light.pos);
            dataStream.Write(light.range);
            dataStream.Write(light.dir);
            dataStream.Write(light.cone);
            dataStream.Write(light.att);
            dataStream.Write(light.type);
            dataStream.Write(light.ambient);
            dataStream.Write(light.diffuse);
        }
        public static void Write(Light[] lights, DataStream dataStream)
        {
            foreach (Light light in lights)
            {
                dataStream.Write(light.pos);
                dataStream.Write(light.range);
                dataStream.Write(light.dir);
                dataStream.Write(light.cone);
                dataStream.Write(light.att);
                dataStream.Write(light.type);
                dataStream.Write(light.ambient);
                dataStream.Write(light.diffuse);
            }
        }
    }
    public enum LightType : int
    {
        Point,
        Spot,
        Dir,
    }

    public class LightBuffer : ConstantBuffer
    {
        public LightBuffer(Device dev)
        {
            this.dev = dev;

            BufferDescription bd = new BufferDescription();
            bd.BindFlags = BindFlags.ConstantBuffer;
            unsafe
            {
                bd.SizeInBytes = 4 * sizeof(Light);
            }

            bd.Usage = ResourceUsage.Dynamic;
            bd.CpuAccessFlags = CpuAccessFlags.Write; // | CpuAccessFlags.Read;
            buffer = new SlimDX.Direct3D11.Buffer(dev, bd);
        }
        
        public void Update(Light light)
        { 
            var dataBox = dev.ImmediateContext.MapSubresource(buffer, MapMode.WriteDiscard, MapFlags.None);
            ds = dataBox.Data;

            Light.Write(light, ds);
            dev.ImmediateContext.UnmapSubresource(buffer, 0);
        }
        public void Update(Light[] lights)
        { 
            var dataBox = dev.ImmediateContext.MapSubresource(buffer, MapMode.WriteDiscard, MapFlags.None);
            ds = dataBox.Data;

            Light.Write(lights, ds);
            dev.ImmediateContext.UnmapSubresource(buffer, 0);
        }

    }
}
