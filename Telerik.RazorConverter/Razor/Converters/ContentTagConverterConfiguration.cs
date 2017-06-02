namespace Telerik.RazorConverter.Razor.Converters
{
    using System.ComponentModel.Composition;

    [Export(typeof(IContentTagConverterConfiguration))]
    public class ContentTagConverterConfiguration : IContentTagConverterConfiguration
    {
        public string[] BodyContentPlaceHolderIDs
        {
            get;
            set;
        }

        public ContentTagConverterConfiguration()
        {
            BodyContentPlaceHolderIDs = new []{"MainContent", "MasterContent"};
        }
    }
}
