//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CodeSharp.Core.Utils
{
    /// <summary>提供文件读写能力
    /// </summary>
    public sealed class FileHelper
    {
        /// <summary>将文件流写入指定路径
        /// </summary>
        /// <param name="fileStream">文件流</param>
        /// <param name="directory">目录</param>
        /// <param name="fileName">文件名</param>
        /// <param name="mode">文件模式</param>
        public static void WriteTo(Stream fileStream, string directory, string fileName, FileMode mode)
        {
            if (fileStream == null)
                throw new ArgumentNullException("fileStream");

            Directory.CreateDirectory(directory);

            using (var file = new FileStream(directory + @"\" + fileName, mode))
            {
                var read = 0;
                var count = 1024;
                var buffer = new byte[count];
                do
                {
                    read = fileStream.Read(buffer, 0, count);
                    file.Write(buffer, 0, read);
                }
                while (read > 0);
                file.Flush();
                fileStream.Seek(0, SeekOrigin.Begin);
            }
        }
        /// <summary>将文本写入指定路径
        /// </summary>
        /// <param name="text">文件文本内容</param>
        /// <param name="directory">目录</param>
        /// <param name="fileName">文件名</param>
        /// <param name="mode">文件模式</param>
        public static void WriteTo(string text, string directory, string fileName, FileMode mode)
        {
            Directory.CreateDirectory(directory);
            using (var file = new FileStream(directory + @"\" + fileName, mode))
            using (var writer = new StreamWriter(file, Encoding.UTF8))
                writer.Write(text ?? "");
        }
    }
}