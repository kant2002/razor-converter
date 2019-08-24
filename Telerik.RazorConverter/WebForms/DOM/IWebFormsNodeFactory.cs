using System.Text.RegularExpressions;

namespace Telerik.RazorConverter.WebForms.DOM
{
    public interface IWebFormsNodeFactory
    {
        IWebFormsNode CreateNode(Match match, NodeType type);
    }
}
