// Copyright (C) 2004  Kevin Downs
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

using System;
using System.Collections.Generic;
using System.IO;

namespace NDoc3.Core {
	/// <summary>
	/// Utility Routines for path handling
	/// </summary>
	public static class PathUtilities {
		// no public constructor - only static methods...


		/// <summary>
		/// Combines the specified path with basePath 
		/// to form a full path to file or directory.
		/// </summary>
		/// <param name="basePath">The reference path.</param>
		/// <param name="path">The relative or absolute path.</param>
		/// <returns>
		/// A rooted path.
		/// </returns>
		public static string GetFullPath(string basePath, string path) {
			if (!string.IsNullOrEmpty(path)) {
				if (!Path.IsPathRooted(path)) {
					path = Path.GetFullPath(Path.Combine(basePath, path));
				}
			}

			return path;
		}

		/// <summary>
		/// Gets the relative path of the passed path with respect to basePath
		/// </summary>
		/// <param name="basePath">The reference path.</param>
		/// <param name="path">The relative or absolute path.</param>
		/// <returns>
		/// A relative path.
		/// </returns>
		public static string GetRelativePath(string basePath, string path) {
			if (!string.IsNullOrEmpty(path)) {
				if (Path.IsPathRooted(path)) {
					path = AbsoluteToRelativePath(basePath, path);
				}
			}

			return path;
		}

		/// <summary>
		/// Converts an absolute path to one relative to the given base directory path
		/// </summary>
		/// <param name="basePath">The base directory path</param>
		/// <param name="absolutePath">An absolute path</param>
		/// <returns>A path to the given absolute path, relative to the base path</returns>
		public static string AbsoluteToRelativePath(string basePath, string absolutePath) {
			char[] separators = {
									Path.DirectorySeparatorChar, 
									Path.AltDirectorySeparatorChar, 
									Path.VolumeSeparatorChar 
								};

			//split the paths into their component parts
			string[] basePathParts = basePath.Split(separators);
			string[] absPathParts = absolutePath.Split(separators);
			int indx = 0;

			//work out how much they have in common
			int minLength = Math.Min(basePathParts.Length, absPathParts.Length);
			for (; indx < minLength; ++indx) {
				if (String.Compare(basePathParts[indx], absPathParts[indx], true) != 0)
					break;
			}

			//if they have nothing in common, just return the absolute path
			if (indx == 0) {
				return absolutePath;
			}


			//start constructing the relative path
			string relPath = "";

			if (indx == basePathParts.Length) {
				// the entire base path is in the abs path
				// so the rel path starts with "./"
				relPath += "." + Path.DirectorySeparatorChar;
			} else {
				//step up from the base to the common root 
				for (int i = indx; i < basePathParts.Length; ++i) {
					relPath += ".." + Path.DirectorySeparatorChar;
				}
			}
			//add the path from the common root to the absPath
			relPath += String.Join(Path.DirectorySeparatorChar.ToString(), absPathParts, indx, absPathParts.Length - indx);

			return relPath;
		}


		/// <summary>
		/// Converts a given base and relative path to an absolute path
		/// </summary>
		/// <param name="basePath">The base directory path</param>
		/// <param name="relativePath">A path to the base directory path</param>
		/// <returns>An absolute path</returns>
		public static string RelativeToAbsolutePath(string basePath, string relativePath) {
			//if the relativePath isn't... 
			if (Path.IsPathRooted(relativePath)) {
				return relativePath;
			}

			//split the paths into their component parts
			string[] basePathParts = basePath.Split(Path.DirectorySeparatorChar);
			string[] relPathParts = relativePath.Split(Path.DirectorySeparatorChar);

			//determine how many we must go up from the base path
			int indx = 0;
			for (; indx < relPathParts.Length; ++indx) {
				if (!relPathParts[indx].Equals("..")) {
					break;
				}
			}

			//if the rel path contains no ".." it is below the base
			//therefor just concatonate the rel path to the base
			if (indx == 0) {
				int offset = 0;
				//ingnore the first part, if it is a rooting "."
				if (relPathParts[0] == ".")
					offset = 1;

				return basePath + Path.DirectorySeparatorChar + String.Join(Path.DirectorySeparatorChar.ToString(), relPathParts, offset, relPathParts.Length - offset);
			}

			string absPath = String.Join(Path.DirectorySeparatorChar.ToString(), basePathParts, 0, Math.Max(0, basePathParts.Length - indx));

			absPath += Path.DirectorySeparatorChar + String.Join(Path.DirectorySeparatorChar.ToString(), relPathParts, indx, relPathParts.Length - indx);

			return absPath;
		}

		///<summary>
		/// Normalize the given path, so that paths pointing to the same location can be compared using == operator.
		///</summary>
		/// <remarks>
		/// Handles only local paths (e.g. no UNC paths!)
		/// </remarks>
		public static string NormalizePath(string path) {
			return NormalizePath(path, Path.DirectorySeparatorChar);
		}

		///<summary>
		/// Normalize the given path, so that paths pointing to the same location can be compared using == operator.
		///</summary>
		/// <remarks>
		/// Handles only local paths (e.g. no UNC paths!)
		/// </remarks>
		public static string NormalizePath(string path, char directorySeparatorChar) {
			path = path
				.Replace("\\", "" + directorySeparatorChar)
				.Replace("/", "" + directorySeparatorChar);

			// reduce path (remove "..")
			path = ReducePath(path, directorySeparatorChar);

			switch (Environment.OSVersion.Platform) {
				case PlatformID.Win32NT:
				case PlatformID.Win32S:
				case PlatformID.Win32Windows:
				case PlatformID.WinCE:
					path = path.ToLowerInvariant();
					break;
					// noop
				default:
					throw new ArgumentException(string.Format("unknow Platform identifier{0}", Environment.OSVersion.Platform));
			}

			return path;
		}

		/// <summary>
		/// Removes "." and ".." parts in the path
		/// </summary>
		/// <remarks>
		/// Handles only local paths (e.g. no UNC paths!)
		/// </remarks>
		public static string ReducePath(string path, char directorySeparatorChar) {
			// handle corner cases
			if (string.IsNullOrEmpty(path)
				|| path == "\\" || path == "\\.") {
				return path;
			}
			if (path == "\\\\" || path == "\\\\.") {
				return path.Replace("\\\\", "\\");
			}

			string absoluteRoot = "" + directorySeparatorChar + directorySeparatorChar;

			int ixAbsoluteRoot = path.LastIndexOf(absoluteRoot);
			if (ixAbsoluteRoot > -1) {
				// from "sfsfdsf\\some" to rooted "\some"
				path = path.Substring(ixAbsoluteRoot + 1);
			}

			bool isAbsolute = path[0] == directorySeparatorChar;
			bool endsWithSeparator = path[path.Length - 1] == directorySeparatorChar;
			bool endsWithDirIdentity = path.Length > 1
										&& path[path.Length - 2] == directorySeparatorChar
										&& path[path.Length - 1] == '.';

			string[] parts = path.Split(new[] { directorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
			List<string> directoryHierarchy = new List<string>();
			int currentDirectoryIndex = 0;
			for (int i = 0; i < parts.Length; i++) {
				if (parts[i] == "..") {
					if (directoryHierarchy.Count > 0) {
						directoryHierarchy.RemoveAt(directoryHierarchy.Count - 1);
					}
					currentDirectoryIndex--;
				} else if (parts[i] == ".") {
					// do nothing
				} else if (parts[i] == absoluteRoot) {
					currentDirectoryIndex = 0;
					directoryHierarchy.Clear();
				} else {
					if (currentDirectoryIndex >= 0) {
						directoryHierarchy.Add(parts[i]);
					}
					currentDirectoryIndex++;
				}
			}
			if (currentDirectoryIndex < 0) {
				throw new ArgumentException(string.Format("too many directory upwalks in {0}", path));
			}
			path = string.Join("" + directorySeparatorChar, directoryHierarchy.ToArray());
			if (isAbsolute) {
				path = directorySeparatorChar + path;
			}
			if (endsWithSeparator) {
				path = path + directorySeparatorChar;
			} else if (endsWithDirIdentity) {
				path = path + directorySeparatorChar + ".";
			}
			return path;
		}
	}
}
