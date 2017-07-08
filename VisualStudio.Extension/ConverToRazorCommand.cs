using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Telerik.RazorConverter.Razor;

namespace VisualStudio.Extension
{
    internal sealed class ConverToRazorCommand
    {
        public static DTE2 _dte;

        public string MasterFolderPath { get; set; } = "~/Views/Shared/Master";
        public string LayoutFolderPath { get; set; } = "~/Views/Shared/Layout";
        public string DefaultMasterName { get; set; } = "Default.Master";
        public string DefaultLayoutName { get; set; } = "DefaultLayout.cshtml";
        public string LayoutSuffix { get; set; } = "";
        public bool ShowDefaultLayout { get; set; }


        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("ac904529-8f84-4cf5-b288-a02f1274bfd8");
        private readonly Package package;
        private readonly WebFormsToRazorConverter converter;

        private IServiceProvider ServiceProvider => package;
        private ConverToRazorCommand(Package package)
        {
            converter = new WebFormsToRazorConverter();
            TemplateSettings.CurrentSettings = new TemplateSettings(MasterFolderPath,
                LayoutFolderPath, DefaultMasterName, DefaultLayoutName, LayoutSuffix, ShowDefaultLayout);

            this.package = package ?? throw new ArgumentNullException(nameof(package));
            _dte = ServiceProvider.GetService(typeof(DTE)) as DTE2;

            var commandService =
                ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            commandService?.AddCommand(new MenuCommand(MenuItemCallback, new CommandID(CommandSet, CommandId)));
        }

        public static ConverToRazorCommand Instance { get; private set; }
        public static void Initialize(Package package)
        {
            Instance = new ConverToRazorCommand(package);
        }

        private readonly string[] webFormsFileExtensions = {"ascx", "aspx", "Master"};

        private void MenuItemCallback(object sender, EventArgs eventArgs)
        {
            var selectedItems = ((UIHierarchy) _dte.Windows
                .Item($"{{{VSConstants.StandardToolWindows.SolutionExplorer}}}").Object).SelectedItems as object[];
            var projectItems = selectedItems?
                                   .Where(t => (t as UIHierarchyItem)?.Object is ProjectItem)
                                   .Select(t => (ProjectItem) ((UIHierarchyItem) t).Object)
                                   .ToArray() ?? new ProjectItem[0];
            foreach (var item in projectItems)
            {
                var filePath = item.FileNames[1];
                var extension = Path.GetExtension(filePath);
                if (webFormsFileExtensions.Contains(extension))
                {
                    ShowWrongExtensionWarning(filePath);
                    break;
                }

                var razorFileInfo = new FileInfo(Path.Combine(Path.GetDirectoryName(filePath), $"{Path.GetFileNameWithoutExtension(filePath)}.cshtml"));
                if (razorFileInfo.Exists)
                {
                    var result = ShowFileAlreadyExistWarning(razorFileInfo);
                    if (result != (int)VSConstants.MessageBoxResult.IDOK)
                    {
                        break;
                    }
                }
                var razorContent = converter.Convert(filePath);
                File.WriteAllText(razorFileInfo.FullName, razorContent, Encoding.UTF8);

                item.ContainingProject.AddFileToProject(razorFileInfo);
                //item.Delete();
                //иногда падает ошибка при попытке его открыть
                //VsShellUtilities.OpenDocument(ServiceProvider, razorFileInfo.Name);
            }
        }

        private int ShowFileAlreadyExistWarning(FileInfo razorFileName)
        {
            return VsShellUtilities.ShowMessageBox(
                ServiceProvider,
                $"Файл {razorFileName.Name} уже существует. Перезаписать файл?",
                "Ошибка конвертации",
                OLEMSGICON.OLEMSGICON_WARNING,
                OLEMSGBUTTON.OLEMSGBUTTON_OKCANCEL,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private int ShowWrongExtensionWarning(string filePath)
        {
            return VsShellUtilities.ShowMessageBox(
                ServiceProvider,
                $"Не возможно конвертировать файл {Path.GetFileName(filePath)}. Файл не в списке поддерживаемых форматов '{string.Join(", ", webFormsFileExtensions)}'",
                "Ошибка конвертации",
                OLEMSGICON.OLEMSGICON_WARNING,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
