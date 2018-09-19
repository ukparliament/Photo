namespace Photo
{
    using System.IO;

    internal class CacheStream : Stream
    {
        private readonly Stream original;
        private readonly Stream other;

        internal CacheStream(Stream original, Stream other)
        {
            this.original = original;
            this.other = other;
        }

        public override bool CanRead => this.original.CanRead;

        public override bool CanSeek => this.original.CanSeek;

        public override bool CanWrite => this.original.CanWrite;

        public override long Length => this.original.Length;

        public override long Position
        {
            get => this.original.Position;
            set
            {
                this.original.Position = value;
                this.other.Position = value;
            }
        }

        public override void Flush()
        {
            this.original.Flush();
            this.other.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            this.other.Read(new byte[buffer.Length], offset, count);
            return this.original.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            this.other.Seek(offset, origin);
            return this.original.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.original.SetLength(value);
            this.other.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.original.Write(buffer, offset, count);
            this.other.Write(buffer, offset, count);
        }
    }
}
