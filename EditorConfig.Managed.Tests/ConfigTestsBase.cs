using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EditorConfig;

namespace EditorConfig.Managed.Tests
{
    public class ConfigTestsBase
    {
        protected FileSettings GetEditorConfigForFile(MethodBase method, string fileName)
        {
            var file = this.GetFileFromMethod(method, fileName);
            var config = new FileSettings(file);
            return config;
        }

        protected string GetFileFromMethod(MethodBase method, string fileName)
        {
            var type = method.DeclaringType;
            var @namespace = type.Namespace;
            var folderSep = Path.DirectorySeparatorChar.ToString();
            var folder = @namespace.Replace("EditorConfig.Managed.Tests.", "").Replace(".", folderSep);
            var file = Path.Combine(folder, (fileName ?? method.Name).Replace(@"\", folderSep));
            file = Path.Combine(Environment.CurrentDirectory.Replace("bin" + folderSep + "Debug", "").Replace("bin" + folderSep + "Release", ""), file);
            return file;
        }
    }
}
