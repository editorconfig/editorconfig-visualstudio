using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO;
using FluentAssertions;

namespace EditorConfig.Managed.Tests.FullConfig
{
	[TestFixture]
    public class FullConfigFileTests : ConfigTestsBase
    {
		private readonly FileSettings _fileSettingsJson;
		private readonly FileSettings _fileSettingsYaml;

		public FullConfigFileTests()
		{
            _fileSettingsJson = this.GetEditorConfigForFile(MethodBase.GetCurrentMethod(), "file.json");
            _fileSettingsYaml = this.GetEditorConfigForFile(MethodBase.GetCurrentMethod(), "temp.yml");
		}

		[Test]
		public void JsonSpecificSettings()
		{
			_fileSettingsJson.IndentStyle.Should().Be("space");
			_fileSettingsJson.IndentSize.Should().Be(2);
			_fileSettingsJson.ContainsKey("yaml_specific_prop").Should().BeFalse();
			_fileSettingsJson.ReadBool("global_prop").Should().BeTrue();
			_fileSettingsJson.ReadBool("global_prop_FAKE").Should().NotHaveValue();
			_fileSettingsYaml["yaml_specific_prop"].Should().Be("Some Value");
		}

		[Test]
		public void YmlSpecificSettings()
		{
			_fileSettingsYaml.IndentStyle.Should().Be("tab");
			_fileSettingsYaml.IndentSize.Should().Be(4);
			_fileSettingsYaml.ContainsKey("yaml_specific_prop").Should().BeTrue();
			_fileSettingsYaml.ReadBool("global_prop").Should().BeTrue();
			_fileSettingsYaml.ReadBool("global_prop_FAKE").Should().NotHaveValue();
			_fileSettingsYaml["yaml_specific_prop"].Should().Be("Some Value");

		}

    }
}
