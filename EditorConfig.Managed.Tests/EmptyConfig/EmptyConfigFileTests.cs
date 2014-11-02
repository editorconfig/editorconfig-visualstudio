using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO;
using FluentAssertions;

namespace EditorConfig.Managed.Tests.EmptyConfig
{
	[TestFixture]
    public class EmptyConfigFileTests : ConfigTestsBase
    {
		[Test]
		public void FileJson_ExistsInOutputPath()
		{
			var editorConfig = this.GetFileFromMethod(MethodBase.GetCurrentMethod(), "file.json");
            File.Exists(editorConfig).Should().BeTrue();
		}

        [Test]
        public void DoesNotThrow_GettingFileSettings()
        {
            var fileSettings = this.GetEditorConfigForFile(MethodBase.GetCurrentMethod(), "file.json");
            fileSettings.ConfigFile.Should().NotBeNullOrEmpty();
            fileSettings.ConfigFile.Should().EndWith(".editorconfig");
        }

		[Test]
		public void DoesNotThrow_AccessingKnownProperties()
		{
            var fileSettings = this.GetEditorConfigForFile(MethodBase.GetCurrentMethod(), "file.json");
			Assert.DoesNotThrow(() =>
			{
				fileSettings.ConvertTabsToSpaces.Should().NotHaveValue();
				fileSettings.EndOfLine.Should().BeNullOrEmpty();
				fileSettings.IndentStyle.Should().BeNullOrEmpty();
				fileSettings.NewLineCharacter.Should().BeNullOrEmpty();
				fileSettings.IndentSize.Should().NotHaveValue();
				fileSettings.InsertFinalNewLine.Should().NotHaveValue();
				fileSettings.TabWidth.Should().NotHaveValue();
				fileSettings.TrimTrailingWhitespace.Should().NotHaveValue();
			});
		}
    }
}
