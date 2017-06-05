using System.Linq;

namespace Telerik.RazorConverter.Razor
{
    public class TemplateSettings
    {
        public TemplateSettings(string masterFolderPath, string layoutFolderPath, string defaultMasterName, string defaultLayoutName, string layoutSuffix, bool showDefaultLayout)
        {
            MasterFolderPath = masterFolderPath;
            LayoutFolderPath = layoutFolderPath;
            DefaultMasterName = defaultMasterName;
            LayoutSuffix = layoutSuffix;
            ShowDefaultLayout = showDefaultLayout;
            DefaultLayoutName = defaultLayoutName;
        }

        public string MasterFolderPath { get; }
        public string LayoutFolderPath { get; }
        public string DefaultMasterName { get; }
        public string DefaultLayoutName { get; }
        public string DefaultLayoutPath => $"{LayoutFolderPath}/{DefaultLayoutName}";
        public string LayoutSuffix { get; }
        public bool ShowDefaultLayout { get; }

        public string MasterToLayoutPath(string masterPageFilePath)
        {
            var masterName = masterPageFilePath.Split('/').Last();
            var layoutName = masterName.Replace(".Master", $"{LayoutSuffix}.cshtml");
            return $"{LayoutFolderPath}/{layoutName}";
        }

        public static TemplateSettings CurrentSettings;
    }
}