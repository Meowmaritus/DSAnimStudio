using Assimp;
using System;

namespace SoulsAssetPipeline
{
    public static class AssimpUtilities
    {
        public static bool FindChildNodeAndApplyItsTransform(Node parentNode, string nodeName, ref Matrix4x4 absoluteTransform, out Node foundNode)
        {
            var n = parentNode.FindNode(nodeName);

            if (n != null)
            {
                absoluteTransform *= n.Transform;
            }

            foundNode = n;
            return foundNode != null;
        }

        public static Node FindRootNode(Scene scene, string rootNodeName, out Matrix4x4 absoluteMatrixOfRootNode)
        {
            var rootNode = scene.RootNode;
            var transform = rootNode.Transform;

            if (FindChildNodeAndApplyItsTransform(rootNode, 
                $"{rootNodeName}_$AssimpFbx$_PreRotation", 
                ref transform, out rootNode))
            {
                if (FindChildNodeAndApplyItsTransform(rootNode,
                    $"{rootNodeName}_$AssimpFbx$_Rotation",
                    ref transform, out rootNode))
                {
                    if (FindChildNodeAndApplyItsTransform(rootNode,
                        $"{rootNodeName}",
                        ref transform, out rootNode))
                    {
                        absoluteMatrixOfRootNode = transform;
                        return rootNode;
                    }
                }
            }

            absoluteMatrixOfRootNode = Matrix4x4.Identity;
            return null;

        }
    }
}
