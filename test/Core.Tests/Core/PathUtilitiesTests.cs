using System.IO;
using NUnit.Framework;

namespace NDoc3.Core
{
	[TestFixture]
	public class PathUtilitiesTests
	{
		[Test]
		public void ReducePathTests()
		{
			// accepts null and empty
			Assert.AreEqual(null, PathUtilities.ReducePath(null, '\\'));
			Assert.AreEqual(string.Empty, PathUtilities.ReducePath(string.Empty, '\\'));
			// handles root dir
			Assert.AreEqual("\\", PathUtilities.ReducePath("\\", '\\'));
			Assert.AreEqual("\\.", PathUtilities.ReducePath("\\.", '\\'));
			Assert.AreEqual("\\", PathUtilities.ReducePath("\\\\", '\\'));
			Assert.AreEqual("\\.", PathUtilities.ReducePath("\\\\.", '\\'));
			Assert.AreEqual("\\part", PathUtilities.ReducePath("whateverbla\\sfd\\\\part", '\\'));
			// handles dirsep char at the end
			Assert.AreEqual("\\part\\", PathUtilities.ReducePath("whateverbla\\sfd\\\\part\\", '\\'));
			// handles identity dir char at the end
			Assert.AreEqual("\\part\\.", PathUtilities.ReducePath("whateverbla\\sfd\\\\part\\.", '\\'));

			// handles upwalks
			Assert.AreEqual("\\part1\\part2\\.", PathUtilities.ReducePath("\\part1\\..\\..\\dummy\\part1\\part2\\part3\\.\\..\\.", '\\'));
		}
	}
}
