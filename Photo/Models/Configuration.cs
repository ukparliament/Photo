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
    using System;
    using ImageMagick;
    using Microsoft.OpenApi.Writers;

    internal class Configuration
    {
        internal static readonly (string MediaType, string Extension, MagickFormat Format)[] PhotoMappings = new[] {
            ("image/jpeg", "jpg", MagickFormat.Jpg),
            ("image/png", "png", MagickFormat.Png),
            ("image/webp", "webp", MagickFormat.WebP),
            ("image/gif", "gif", MagickFormat.Gif),
            ("image/tiff", "tif", MagickFormat.Tif),
            ("image/x-icon", "ico", MagickFormat.Ico),
            ("application/pdf", "pdf", MagickFormat.Pdf)
        };

        internal static readonly (string MediaType, string Extension, Type WriterType)[] OpenApiMappings = new[] {
            ("application/json", "json", typeof(OpenApiJsonWriter)),
            ("text/vnd.yaml", "yaml", typeof(OpenApiYamlWriter))
        };

        internal static readonly (string Name, int? OffsetX, int OffsetY, int Width, int Height)[] Crops = new[] {
            ("MCU_3:2", 1553, 789, 3108, 2072),
            ("MCU_3:4", 789, 789, 1554, 2072),
            ("CU_1:1", 789, 706, 1554, 1554),
            ("CU_5:2", null as int?, 670, 3795, 1518)
        };

        public StorageConfiguration Storage { get; set; }

        public StorageConfiguration Cache { get; set; }

        public QueryConfiguration Query { get; set; }

        internal class StorageConfiguration
        {
            public string ConnectionString { get; set; }

            public string Container { get; set; }
        }

        internal class QueryConfiguration
        {
            public string Endpoint { get; set; }

            public string ApiVersion { get; set; }

            public string SubscriptionKey { get; set; }
        }
    }
}