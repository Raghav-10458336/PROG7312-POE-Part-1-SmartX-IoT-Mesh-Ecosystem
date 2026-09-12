using SmartX.Shared.Models;

namespace SmartX.Shared.Validation
{
    public static class DeploymentValidator
    {
        public static bool ValidateNode(DeploymentNode node)
        {
            if (node is null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(node.Name))
            {
                return false;
            }

            if (!node.IsConfigured)
            {
                return false;
            }

            foreach (DeploymentNode child in node.Children)
            {
                if (!ValidateNode(child))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
