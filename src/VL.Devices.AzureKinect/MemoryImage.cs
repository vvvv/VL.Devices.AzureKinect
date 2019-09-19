using System;
using System.Buffers;
using VL.Lib.Basics.Imaging;

namespace VL.Devices.AzureKinect
{
    class MemoryImage : IImage
    {
        unsafe class Data : IImageData
        {
            readonly Memory<byte> memory;
            readonly MemoryHandle handle;
            public Data(Memory<byte> memory, int scanSize)
            {
                this.memory = memory;
                this.handle = memory.Pin();
                ScanSize = scanSize;
            }

            public IntPtr Pointer => new IntPtr(handle.Pointer);

            public int Size => memory.Length;

            public int ScanSize { get; }

            public void Dispose()
            {
                handle.Dispose();
            }
        }


        public MemoryImage(Memory<byte> memory, ImageInfo info, int stride)
        {
            Memory = memory;
            Info = info;
            Stride = stride;
        }

        public Memory<byte> Memory { get; }

        public ImageInfo Info { get; }

        public int Stride { get; }

        public bool IsVolatile => true;

        public IImageData GetData() => new Data(Memory, Stride);
    }
}
