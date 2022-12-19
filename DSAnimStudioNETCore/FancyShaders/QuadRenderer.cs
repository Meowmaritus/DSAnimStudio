
#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace DSAnimStudio.FancyShaders
{
    public class QuadRenderer
    {
        //buffers for rendering the quad
        private readonly VertexPositionTexture[] _vertexBuffer;
        private readonly short[] _indexBuffer;

        public QuadRenderer()
        {
            _vertexBuffer = new VertexPositionTexture[4];
            _vertexBuffer[0] = new VertexPositionTexture(new Vector3(-1, 1, 1), new Vector2(0, 0));
            _vertexBuffer[1] = new VertexPositionTexture(new Vector3(1, 1, 1), new Vector2(1, 0));
            _vertexBuffer[2] = new VertexPositionTexture(new Vector3(-1, -1, 1), new Vector2(0, 1));
            _vertexBuffer[3] = new VertexPositionTexture(new Vector3(1, -1, 1), new Vector2(1, 1));

            _indexBuffer = new short[] { 0, 3, 2, 0, 1, 3 };
        }

        public void RenderQuad(GraphicsDevice graphicsDevice, Vector2 v1, Vector2 v2)
        {
            _vertexBuffer[0].Position.X = v1.X;
            _vertexBuffer[0].Position.Y = v2.Y;

            _vertexBuffer[1].Position.X = v2.X;
            _vertexBuffer[1].Position.Y = v2.Y;

            _vertexBuffer[2].Position.X = v1.X;
            _vertexBuffer[2].Position.Y = v1.Y;

            _vertexBuffer[3].Position.X = v2.X;
            _vertexBuffer[3].Position.Y = v1.Y;

            graphicsDevice.DrawUserIndexedPrimitives
                (PrimitiveType.TriangleList, _vertexBuffer, 0, 4, _indexBuffer, 0, 2);
        }

    }
}
