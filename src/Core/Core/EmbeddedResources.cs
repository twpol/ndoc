// EmbeddedResources.cs - utilities to write embedded resources
// Copyright (C) 2001  Kral Ferch, Jason Diamond
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

namespace NDoc3.Core
{
    using System;
    using System.IO;
    using System.Reflection;

    /// <summary>Utilties to help reading and writing embedded resources.</summary>
    /// <remarks>This is used to access the stylesheets.</remarks>
    public sealed class EmbeddedResources
    {
        // no public constructor - only static methods...
        private EmbeddedResources() { }

        /// <summary>Writes all the embedded resources with the specified prefix to disk.</summary>
        /// <param name="resourceLocationHint">The type who's assembly and namespace denotes the location to search for embedded resources.</param>
        /// <param name="directory">The directory to write the resources to.</param>
        public static void WriteEmbeddedResources(
            Type resourceLocationHint,
            string directory)
        {
            WriteEmbeddedResources(resourceLocationHint.Assembly, resourceLocationHint.Namespace, directory);
        }

        /// <summary>Writes all the embedded resources with the specified prefix to disk.</summary>
        /// <param name="assembly">The assembly containing the embedded resources.</param>
        /// <param name="prefix">The prefix to search for.</param>
        /// <param name="directory">The directory to write the resources to.</param>
        public static void WriteEmbeddedResources(
            Assembly assembly,
            string prefix,
            string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string[] names = assembly.GetManifestResourceNames();

            foreach (string name in names)
            {
                if (name.StartsWith(prefix))
                {
                    WriteEmbeddedResource(
                        assembly,
                        name,
                        directory,
                        name.Substring(prefix.Length + 1));
                }
            }
        }

        /// <summary>Writes an embedded resource to disk.</summary>
        /// <param name="resourceLocationHint">The type, who's assembly and namespace denote the location containing the embedded resource.</param>
        /// <param name="resourceName">The name of the embedded resource.</param>
        /// <param name="directory">The directory to write the resource to.</param>
        /// <remarks>This essentially copies an embedded resource to a target directory on disk</remarks>
        public static void WriteEmbeddedResource(
            Type resourceLocationHint,
            string resourceName,
            string directory)
        {
            string manifestResourceName = string.Format("{0}.{1}", resourceLocationHint.Namespace, resourceName);
            WriteEmbeddedResource(resourceLocationHint.Assembly, manifestResourceName, directory, resourceName);
        }

        /// <summary>Writes an embedded resource to disk.</summary>
        /// <param name="assembly">The assembly containing the embedded resource.</param>
        /// <param name="manifestResourceName">The name of the embedded resource.</param>
        /// <param name="directory">The directory to write the resource to.</param>
        /// <param name="filename">The filename of the resource on disk.</param>
        public static void WriteEmbeddedResource(
            Assembly assembly,
            string manifestResourceName,
            string directory,
            string filename)
        {
            Stream input = assembly.GetManifestResourceStream(manifestResourceName);
            Stream output = File.Open(Path.Combine(directory, filename), FileMode.Create);

            using (input)
            using (output)
            {
                StreamCopy(input, output);
            }
        }

        /// <summary>
        /// Returns a stream to read data from an embedded resource
        /// </summary>
        /// <param name="resourceLocationHint">The type, who's assembly and namespace denote the location containing the embedded resource.</param>
        /// <param name="resourceName">The name of the embedded resource within <paramref name="resourceLocationHint"/>'s <see cref="Type.Namespace"/>.</param>
        /// <returns></returns>
        public static Stream GetEmbeddedResourceStream(Type resourceLocationHint, string resourceName)
        {
            string manifestResourceName = string.Format("{0}.{1}", resourceLocationHint.Namespace, resourceName);
            Stream input = resourceLocationHint.Assembly.GetManifestResourceStream(manifestResourceName);
            if (input == null)
            {
                throw new FileNotFoundException(string.Format("The resource {0} cannot be found at the location denoated by {1}", resourceName, resourceLocationHint), "res://" + manifestResourceName);
            }
            return input;
        }

        /// <summary>
        /// Returns a stream to read data from an embedded resource
        /// </summary>
        /// <param name="resourceLocationHint">The type, who's assembly and namespace denote the location containing the embedded resource.</param>
        /// <param name="resourceName">The name of the embedded resource within <paramref name="resourceLocationHint"/>'s <see cref="Type.Namespace"/>.</param>
        /// <param name="encoding">the encoding to use for the reader or <c>null</c> to autodetect encoding.</param>
        /// <returns></returns>
        public static TextReader GetEmbeddedResourceReader(Type resourceLocationHint, string resourceName, System.Text.Encoding encoding)
        {
            Stream input = GetEmbeddedResourceStream(resourceLocationHint, resourceName);
            if (encoding != null)
            {
                return new StreamReader(input, encoding);
            }
            return new StreamReader(input, true);
        }

        /// <summary>
        /// Copies bytes from input stream to an output stream
        /// </summary>
        /// <param name="input">the input stream to read from</param>
        /// <param name="output">the output stream to write to</param>
        /// <returns>the number of bytes read</returns>
        public static int StreamCopy(Stream input, Stream output)
        {
            const int bufferSize = 2048;
            byte[] buffer = new byte[bufferSize];
            int count = 0;
            int bytesWritten = 0;

            while ((count = input.Read(buffer, 0, bufferSize)) > 0)
            {
                output.Write(buffer, 0, count);
                bytesWritten += count;
            }
            return bytesWritten;
        }
    }
}
