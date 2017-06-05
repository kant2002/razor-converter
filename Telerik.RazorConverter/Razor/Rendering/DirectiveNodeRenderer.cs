using System;
using Telerik.RazorConverter.Razor.Converters;

namespace Telerik.RazorConverter.Razor.Rendering
{
    using Telerik.RazorConverter.Razor.DOM;

    public class DirectiveNodeRenderer : IRazorNodeRenderer
    {
        public string RenderNode(IRazorNode node)
        {
            var directiveNode = (IRazorDirectiveNode) node;
            if (directiveNode.Directive == DirectiveNames.LAYOUT)
                return $@"@{{Layout = ""{directiveNode.Parameters}"";}}";
            return $"@{directiveNode.Directive} {directiveNode.Parameters}".Trim();
        }

        public bool CanRenderNode(IRazorNode node)
        {
            var directiveNode = node as IRazorDirectiveNode;
            return directiveNode != null && (directiveNode.Directive != DirectiveNames.LAYOUT || !string.Equals(directiveNode.Parameters, TemplateSettings.Default.DefaultLayoutPath, StringComparison.OrdinalIgnoreCase));
        }
    }
}
