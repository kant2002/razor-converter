namespace Telerik.RazorConverter.Razor.Converters
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Telerik.RazorConverter.Razor.DOM;
    using Telerik.RazorConverter.WebForms.DOM;

    public class DirectiveConverter : INodeConverter<IRazorNode>
    {
        private IRazorDirectiveNodeFactory DirectiveNodeFactory { get; set; }
        private IRazorTextNodeFactory TextNodeFactory { get; set; }

        public DirectiveConverter(IRazorDirectiveNodeFactory nodeFactory, IRazorTextNodeFactory textNodeFactory)
        {
            DirectiveNodeFactory = nodeFactory;
            TextNodeFactory = textNodeFactory;
        }

        public IList<IRazorNode> ConvertNode(IWebFormsNode node)
        {
            var result = new List<IRazorNode>();

            var directiveNode = node as IWebFormsDirectiveNode;

            if (directiveNode != null)
            {
                var attributes = directiveNode.Attributes;
                if (attributes.ContainsKey("inherits"))
                {
                    AddModelOrInheritsDirective(attributes, result);
                }
                else if (attributes.ContainsKey("namespace") &&
                         directiveNode.Directive == DirectiveType.Import)
                {
                    AddUsingDirective(attributes, result);
                }

                if (attributes.ContainsKey("masterpagefile"))
                {
                    AddLayoutDirective(attributes, result);
                }
            }

            return result;
        }

        private void AddModelOrInheritsDirective(IDictionary<string, string> attributes, List<IRazorNode> result)
        {
            var inheritsFrom = attributes["inherits"];
            var viewPageGenericType = new Regex("System.Web.Mvc.(?:ViewPage|ViewUserControl)<(?<type>.*)>");
            var typeMatch = viewPageGenericType.Match(inheritsFrom);
            if (typeMatch.Success)
            {
                result.Add(DirectiveNodeFactory.CreateDirectiveNode(DirectiveNames.MODEL, typeMatch.Result("${type}")));
            }
            else if (inheritsFrom != "System.Web.Mvc.ViewPage" && inheritsFrom != "System.Web.Mvc.ViewUserControl")
            {
                result.Add(DirectiveNodeFactory.CreateDirectiveNode(DirectiveNames.INHERITS, attributes["inherits"]));
            }
        }

        private void AddUsingDirective(IDictionary<string, string> attributes, List<IRazorNode> result)
        {
            var imports = attributes["namespace"];

            if (!string.IsNullOrEmpty(imports))
            {
                result.Add(DirectiveNodeFactory.CreateDirectiveNode(DirectiveNames.USING, attributes["namespace"]));
            }
        }

        private void AddLayoutDirective(IDictionary<string, string> attributes, List<IRazorNode> result)
        {
            var layoutPath = TemplateSettings.Default.MasterToLayoutPath(attributes["masterpagefile"]);

            result.Add(TextNodeFactory.CreateTextNode("\r\n"));
            result.Add(DirectiveNodeFactory.CreateDirectiveNode(DirectiveNames.LAYOUT, layoutPath));
        }

        public bool CanConvertNode(IWebFormsNode node)
        {
            return node as IWebFormsDirectiveNode != null;
        }
    }
}