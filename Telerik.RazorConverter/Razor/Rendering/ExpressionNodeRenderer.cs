namespace Telerik.RazorConverter.Razor.Rendering
{
    using Telerik.RazorConverter.Razor.DOM;

    public class ExpressionNodeRenderer : IRazorNodeRenderer
    {
        public string RenderNode(IRazorNode node)
        {
            var srcNode = node as IRazorExpressionNode;
            var expression = srcNode.Expression;

            return $"@({expression})";
        }

        public bool CanRenderNode(IRazorNode node)
        {
            return node is IRazorExpressionNode;
        }
    }
}
