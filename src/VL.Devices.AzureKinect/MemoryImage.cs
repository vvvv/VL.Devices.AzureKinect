using System;
using System.Buffers;
using VL.Lib.Basics.Imaging;

namespace VL.Devices.AzureKinect
{
    class MemoryImage<T> : IImage
        where T : unmanaged
    {
        unsafe class Data : IImageData
        {
            readonly Memory<T> memory;
            readonly MemoryHandle handle;
            public Data(Memory<T> memory, int scanSize)
            {
                this.memory = memory;
                this.handle = memory.Pin();
                ScanSize = scanSize;
            }

            public IntPtr Pointer => new IntPtr(handle.Pointer);

            public int Size => memory.Length * sizeof(T);

            public int ScanSize { get; }

            public void Dispose()
            {
                handle.Dispose();
            }
        }


        public MemoryImage(Memory<T> memory, ImageInfo info, int stride)
        {
            Memory = memory;
            Info = info;
            Stride = stride;
        }

        public Memory<T> Memory { get; }

        public ImageInfo Info { get; }

        public int Stride { get; }

        public bool IsVolatile => true;

        public IImageData GetData() => new Data(Memory, Stride);
    }
}
