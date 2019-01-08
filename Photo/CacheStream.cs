// MIT License
//
// Copyright (c) 2019 UK Parliament
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

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
