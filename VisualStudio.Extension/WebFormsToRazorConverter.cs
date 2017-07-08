using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Text;
using Telerik.RazorConverter;
using Telerik.RazorConverter.Razor.DOM;

namespace VisualStudio.Extension
{
    public class WebFormsToRazorConverter
    {
        public WebFormsToRazorConverter()
        {
            var catalog = new AssemblyCatalog(typeof(IWebFormsParser).Assembly);
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }

        [Import]
        private IWebFormsParser Parser { get; set; }

        [Import]
        private IWebFormsConverter<IRazorNode> Converter { get; set; }

        [Import]
        private IRenderer<IRazorNode> Renderer { get; set; }

        public string Convert(string filePath)
        {
            var webFormsPageSource = File.ReadAllText(filePath, Encoding.UTF8);
            var webFormsDocument = Parser.Parse(webFormsPageSource);
            var razorDom = Converter.Convert(webFormsDocument);
            return Renderer.Render(razorDom);
        }
    }
}