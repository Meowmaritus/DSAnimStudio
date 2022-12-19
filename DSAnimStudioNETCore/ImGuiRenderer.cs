using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DSAnimStudio
{
    /// <summary>
    /// ImGui renderer for use with XNA-likes (FNA & MonoGame)
    /// </summary>
    public class ImGuiRenderer
    {
        public static class DrawVertDeclaration
        {
            public static readonly VertexDeclaration Declaration;

            public static readonly int Size;

            static DrawVertDeclaration()
            {
                unsafe { Size = sizeof(ImDrawVert); }

                Declaration = new VertexDeclaration(
                    Size,

                    // Position
                    new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),

                    // UV
                    new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),

                    // Color
                    new VertexElement(16, VertexElementFormat.Color, VertexElementUsage.Color, 0)
                );
            }
        }

        private Game _game;

        // Graphics
        private GraphicsDevice _graphicsDevice;

        private BasicEffect _effect;
        private RasterizerState _rasterizerState;

        private byte[] _vertexData;
        private VertexBuffer _vertexBuffer;
        private int _vertexBufferSize;

        private byte[] _indexData;
        private IndexBuffer _indexBuffer;
        private int _indexBufferSize;

        // Textures
        private Dictionary<IntPtr, Texture2D> _loadedTextures;

        private int _textureId;
        private IntPtr? _fontTextureId;

        // Input
        private int _scrollWheelValue;

        //private List<int> _keys = new List<int>();

        public ImGuiRenderer(Game game)
        {
            var context = ImGui.CreateContext();

            ImGui.SetCurrentContext(context);

            _game = game ?? throw new ArgumentNullException(nameof(game));
            _graphicsDevice = game.GraphicsDevice;

            _loadedTextures = new Dictionary<IntPtr, Texture2D>();

            _rasterizerState = new RasterizerState()
            {
                CullMode = CullMode.None,
                DepthBias = 0,
                FillMode = FillMode.Solid,
                MultiSampleAntiAlias = true,
                ScissorTestEnable = true,
                SlopeScaleDepthBias = 0
            };

            SetupInput();
        }

        #region ImGuiRenderer

        /// <summary>
        /// Creates a texture and loads the font data from ImGui. Should be called when the <see cref="GraphicsDevice" /> is initialized but before any rendering is done
        /// </summary>
        public virtual unsafe void RebuildFontAtlas()
        {
            // Get font texture from ImGui
            var io = ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out int width, out int height, out int bytesPerPixel);

            // Copy the data to a managed array
            var pixels = new byte[width * height * bytesPerPixel];
            unsafe { Marshal.Copy(new IntPtr(pixelData), pixels, 0, pixels.Length); }

            // Create and register the texture as an XNA texture
            var tex2d = new Texture2D(_graphicsDevice, width, height, false, SurfaceFormat.Color);
            tex2d.SetData(pixels);

            // Should a texture already have been build previously, unbind it first so it can be deallocated
            if (_fontTextureId.HasValue) UnbindTexture(_fontTextureId.Value);

            // Bind the new texture to an ImGui-friendly id
            _fontTextureId = BindTexture(tex2d);

            // Let ImGui know where to find the texture
            io.Fonts.SetTexID(_fontTextureId.Value);
            io.Fonts.ClearTexData(); // Clears CPU side texture data
        }

        /// <summary>
        /// Creates a pointer to a texture, which can be passed through ImGui calls such as <see cref="ImGui.Image" />. That pointer is then used by ImGui to let us know what texture to draw
        /// </summary>
        public virtual IntPtr BindTexture(Texture2D texture)
        {
            var id = new IntPtr(_textureId++);

            _loadedTextures.Add(id, texture);

            return id;
        }

        /// <summary>
        /// Removes a previously created texture pointer, releasing its reference and allowing it to be deallocated
        /// </summary>
        public virtual void UnbindTexture(IntPtr textureId)
        {
            _loadedTextures.Remove(textureId);
        }

        /// <summary>
        /// Sets up ImGui for a new frame, should be called at frame start
        /// </summary>
        public virtual void BeforeLayout(GameTime gameTime, int x, int y, int w, int h, int mouseYOff)
        {
            ImGui.GetIO().DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            UpdateInput(x, y, w, h, mouseYOff);

            ImGui.NewFrame();
        }

        /// <summary>
        /// Asks ImGui for the generated geometry data and sends it to the graphics pipeline, should be called after the UI is drawn using ImGui.** calls
        /// </summary>
        public virtual void AfterLayout(int x, int y, int w, int h, int memeY)
        {
            ImGui.Render();

            unsafe { RenderDrawData(ImGui.GetDrawData(), x, y, w, h, memeY); }
        }

        #endregion ImGuiRenderer

        #region Setup & Update

        /// <summary>
        /// Maps ImGui keys to XNA keys. We use this later on to tell ImGui what keys were pressed
        /// </summary>
        protected virtual void SetupInput()
        {
            var io = ImGui.GetIO();

            //_keys.Add(io.KeyMap[(int)ImGuiKey.Tab] = (int)Keys.Tab);
            //_keys.Add(io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Keys.Left);
            //_keys.Add(io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Keys.Right);
            //_keys.Add(io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Keys.Up);
            //_keys.Add(io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Keys.Down);
            //_keys.Add(io.KeyMap[(int)ImGuiKey.PageUp] = (int)Keys.PageUp);
            //_keys.Add(io.KeyMap[(int)ImGuiKey.PageDown] = (int)Keys.PageDown);
            //_keys.Add(io.KeyMap[(int)ImGuiKey.Home] = (int)Keys.Home);
            //_keys.Add(io.KeyMap[(int)ImGuiKey.End] = (int)Keys.End);
            //_keys.Add(io.KeyMap[(int)ImGuiKey.Delete] = (int)Keys.Delete);
            //_keys.Add(io.KeyMap[(int)ImGuiKey.Backspace] = (int)Keys.Back);
            //_keys.Add(io.KeyMap[(int)ImGuiKey.Enter] = (int)Keys.Enter);
            //_keys.Add(io.KeyMap[(int)ImGuiKey.Escape] = (int)Keys.Escape);
            //_keys.Add(io.KeyMap[(int)ImGuiKey.A] = (int)Keys.A);
            //_keys.Add(io.KeyMap[(int)ImGuiKey.C] = (int)Keys.C);
            //_keys.Add(io.KeyMap[(int)ImGuiKey.V] = (int)Keys.V);
            //_keys.Add(io.KeyMap[(int)ImGuiKey.X] = (int)Keys.X);
            //_keys.Add(io.KeyMap[(int)ImGuiKey.Y] = (int)Keys.Y);
            //_keys.Add(io.KeyMap[(int)ImGuiKey.Z] = (int)Keys.Z);

            // MonoGame-specific //////////////////////
            _game.Window.TextInput += (s, a) =>
            {
                if (a.Character == '\t') return;

                io.AddInputCharacter(a.Character);
            };
            ///////////////////////////////////////////

            // FNA-specific ///////////////////////////
            //TextInputEXT.TextInput += c =>
            //{
            //    if (c == '\t') return;

            //    ImGui.GetIO().AddInputCharacter(c);
            //};
            ///////////////////////////////////////////

            ImGui.GetIO().Fonts.AddFontDefault();
        }

        /// <summary>
        /// Updates the <see cref="Effect" /> to the current matrices and texture
        /// </summary>
        protected virtual Effect UpdateEffect(Texture2D texture, Matrix shiftMatrix)
        {
            _effect = _effect ?? new BasicEffect(_graphicsDevice);

            var io = ImGui.GetIO();

            // MonoGame-specific //////////////////////
            //var offset = .5f;
            var offset = 0f;
            ///////////////////////////////////////////

            // FNA-specific ///////////////////////////
            //var offset = 0f;
            ///////////////////////////////////////////

            _effect.World = shiftMatrix;
            _effect.View = Matrix.Identity;
            _effect.Projection = Matrix.CreateOrthographicOffCenter(offset, io.DisplaySize.X + offset, io.DisplaySize.Y + offset, offset, -1f, 1f);
            _effect.TextureEnabled = true;
            _effect.Texture = texture;
            _effect.VertexColorEnabled = true;

            return _effect;
        }

        /// <summary>
        /// Sends XNA input state to ImGui
        /// </summary>
        protected virtual void UpdateInput(int x, int y, int w, int h, int mouseYOff)
        {
            var io = ImGui.GetIO();

            

            var mouse = GlobalInputState.Mouse;
            var keyboard = GlobalInputState.Keyboard;

            //for (int i = 0; i < _keys.Count; i++)
            //{
            //    io.KeysDown[_keys[i]] = keyboard.IsKeyDown((Keys)_keys[i]);
            //}

            io.AddKeyEvent(ImGuiKey.ModShift, keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift));
            io.AddKeyEvent(ImGuiKey.ModCtrl, keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl));
            io.AddKeyEvent(ImGuiKey.ModAlt, keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt));
            io.AddKeyEvent(ImGuiKey.ModSuper, keyboard.IsKeyDown(Keys.LeftWindows) || keyboard.IsKeyDown(Keys.RightWindows));

            io.AddKeyEvent(ImGuiKey.Enter, keyboard.IsKeyDown(Keys.Enter));

            io.AddKeyEvent(ImGuiKey.Tab, keyboard.IsKeyDown(Keys.Tab));
            io.AddKeyEvent(ImGuiKey.LeftArrow, keyboard.IsKeyDown(Keys.Left));
            io.AddKeyEvent(ImGuiKey.RightArrow, keyboard.IsKeyDown(Keys.Right));
            io.AddKeyEvent(ImGuiKey.UpArrow, keyboard.IsKeyDown(Keys.Up));
            io.AddKeyEvent(ImGuiKey.DownArrow, keyboard.IsKeyDown(Keys.Down));
            io.AddKeyEvent(ImGuiKey.PageUp, keyboard.IsKeyDown(Keys.PageUp));
            io.AddKeyEvent(ImGuiKey.PageDown, keyboard.IsKeyDown(Keys.PageDown));
            io.AddKeyEvent(ImGuiKey.Home, keyboard.IsKeyDown(Keys.Home));
            io.AddKeyEvent(ImGuiKey.End, keyboard.IsKeyDown(Keys.End));
            io.AddKeyEvent(ImGuiKey.Delete, keyboard.IsKeyDown(Keys.Delete));
            io.AddKeyEvent(ImGuiKey.Backspace, keyboard.IsKeyDown(Keys.Back));
            io.AddKeyEvent(ImGuiKey.Enter, keyboard.IsKeyDown(Keys.Enter));
            io.AddKeyEvent(ImGuiKey.Escape, keyboard.IsKeyDown(Keys.Escape));
            io.AddKeyEvent(ImGuiKey.A, keyboard.IsKeyDown(Keys.A));
            io.AddKeyEvent(ImGuiKey.C, keyboard.IsKeyDown(Keys.C));
            io.AddKeyEvent(ImGuiKey.V, keyboard.IsKeyDown(Keys.V));
            io.AddKeyEvent(ImGuiKey.X, keyboard.IsKeyDown(Keys.X));
            io.AddKeyEvent(ImGuiKey.Y, keyboard.IsKeyDown(Keys.Y));
            io.AddKeyEvent(ImGuiKey.Z, keyboard.IsKeyDown(Keys.Z));

            //if (Main.Input.KeyDown(Keys.D1) || Main.Input.KeyDown(Keys.NumPad1))
            //    io.AddInputCharacter('1');
            //if (Main.Input.KeyDown(Keys.D2) || Main.Input.KeyDown(Keys.NumPad2))
            //    io.AddInputCharacter('2');
            //if (Main.Input.KeyDown(Keys.D3) || Main.Input.KeyDown(Keys.NumPad3))
            //    io.AddInputCharacter('3');
            //if (Main.Input.KeyDown(Keys.D4) || Main.Input.KeyDown(Keys.NumPad4))
            //    io.AddInputCharacter('4');
            //if (Main.Input.KeyDown(Keys.D5) || Main.Input.KeyDown(Keys.NumPad5))
            //    io.AddInputCharacter('5');
            //if (Main.Input.KeyDown(Keys.D6) || Main.Input.KeyDown(Keys.NumPad6))
            //    io.AddInputCharacter('6');
            //if (Main.Input.KeyDown(Keys.D7) || Main.Input.KeyDown(Keys.NumPad7))
            //    io.AddInputCharacter('7');
            //if (Main.Input.KeyDown(Keys.D8) || Main.Input.KeyDown(Keys.NumPad8))
            //    io.AddInputCharacter('8');
            //if (Main.Input.KeyDown(Keys.D9) || Main.Input.KeyDown(Keys.NumPad9))
            //    io.AddInputCharacter('9');
            //if (Main.Input.KeyDown(Keys.D0) || Main.Input.KeyDown(Keys.NumPad0))
            //    io.AddInputCharacter('0');
            //if (Main.Input.KeyDown(Keys.OemPeriod))
            //    io.AddInputCharacter('.');
            //if (Main.Input.KeyDown(Keys.OemComma))
            //    io.AddInputCharacter(',');
            //if (Main.Input.KeyDown(Keys.OemMinus) || Main.Input.KeyDown(Keys.Subtract))
            //    io.AddInputCharacter('-');

            //io.KeyShift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
            //io.KeyCtrl = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);
            //io.KeyAlt = keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt);
            //io.KeySuper = keyboard.IsKeyDown(Keys.LeftWindows) || keyboard.IsKeyDown(Keys.RightWindows);

            io.DisplaySize = new System.Numerics.Vector2(w, h);
            io.DisplayFramebufferScale = new System.Numerics.Vector2(1f, 1f);

            //io.MousePos = new System.Numerics.Vector2(mouse.X - x, mouse.Y - y - mouseYOff);

            io.AddMousePosEvent(mouse.X - x, mouse.Y - y - mouseYOff);
            io.AddMouseButtonEvent(0, mouse.LeftButton == ButtonState.Pressed);
            io.AddMouseButtonEvent(1, mouse.RightButton == ButtonState.Pressed);
            io.AddMouseButtonEvent(2, mouse.MiddleButton == ButtonState.Pressed);
            
            

            var scrollDelta = mouse.ScrollWheelValue - _scrollWheelValue;
            //io.MouseWheel = scrollDelta > 0 ? 1 : scrollDelta < 0 ? -1 : 0;
            io.AddMouseWheelEvent(0, scrollDelta > 0 ? 1 : scrollDelta < 0 ? -1 : 0);
            _scrollWheelValue = mouse.ScrollWheelValue;
        }

        #endregion Setup & Update

        #region Internals

        /// <summary>
        /// Gets the geometry as set up by ImGui and sends it to the graphics device
        /// </summary>
        private void RenderDrawData(ImDrawDataPtr drawData, int x, int y, int w, int h, int memeY)
        {
            // Setup render state: alpha-blending enabled, no face culling, no depth testing, scissor enabled, vertex/texcoord/color pointers
            var lastViewport = _graphicsDevice.Viewport;
            var lastScissorBox = _graphicsDevice.ScissorRectangle;

            _graphicsDevice.BlendFactor = Color.White;
            _graphicsDevice.BlendState = BlendState.NonPremultiplied;
            _graphicsDevice.RasterizerState = _rasterizerState;
            _graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            // Handle cases of screen coordinates != from framebuffer coordinates (e.g. retina displays)
            drawData.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

            drawData.DisplayPos = new System.Numerics.Vector2(x, y);

            // Setup projection
            _graphicsDevice.Viewport = new Viewport(x, y, w, h);

            UpdateBuffers(drawData);

            RenderCommandLists(drawData, x, y, w, h, memeY);

            // Restore modified state
            _graphicsDevice.Viewport = lastViewport;
            _graphicsDevice.ScissorRectangle = lastScissorBox;
        }

        private unsafe void UpdateBuffers(ImDrawDataPtr drawData)
        {
            if (drawData.TotalVtxCount == 0)
            {
                return;
            }

            // Expand buffers if we need more room
            if (drawData.TotalVtxCount > _vertexBufferSize)
            {
                _vertexBuffer?.Dispose();

                _vertexBufferSize = (int)(drawData.TotalVtxCount * 1.5f);
                _vertexBuffer = new VertexBuffer(_graphicsDevice, DrawVertDeclaration.Declaration, _vertexBufferSize, BufferUsage.None);
                _vertexData = new byte[_vertexBufferSize * DrawVertDeclaration.Size];
            }

            if (drawData.TotalIdxCount > _indexBufferSize)
            {
                _indexBuffer?.Dispose();

                _indexBufferSize = (int)(drawData.TotalIdxCount * 1.5f);
                _indexBuffer = new IndexBuffer(_graphicsDevice, IndexElementSize.ThirtyTwoBits, _indexBufferSize, BufferUsage.None);
                _indexData = new byte[_indexBufferSize * sizeof(uint)];
            }

            // Copy ImGui's vertices and indices to a set of managed byte arrays
            int vtxOffset = 0;
            int idxOffset = 0;

            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImDrawListPtr cmdList = drawData.CmdListsRange[n];

                fixed (void* vtxDstPtr = &_vertexData[vtxOffset * DrawVertDeclaration.Size])
                fixed (void* idxDstPtr = &_indexData[idxOffset * sizeof(uint)])
                {
                    Buffer.MemoryCopy((void*)cmdList.VtxBuffer.Data, vtxDstPtr, _vertexData.Length, cmdList.VtxBuffer.Size * DrawVertDeclaration.Size);
                    Buffer.MemoryCopy((void*)cmdList.IdxBuffer.Data, idxDstPtr, _indexData.Length, cmdList.IdxBuffer.Size * sizeof(uint));
                }

                vtxOffset += cmdList.VtxBuffer.Size;
                idxOffset += cmdList.IdxBuffer.Size;
            }

            // Copy the managed byte arrays to the gpu vertex- and index buffers
            _vertexBuffer.SetData(_vertexData, 0, drawData.TotalVtxCount * DrawVertDeclaration.Size);
            _indexBuffer.SetData(_indexData, 0, drawData.TotalIdxCount * sizeof(uint));
        }

        private unsafe void RenderCommandLists(ImDrawDataPtr drawData, int x, int y, int w, int h, int memeY)
        {
            _graphicsDevice.SetVertexBuffer(_vertexBuffer);
            _graphicsDevice.Indices = _indexBuffer;

            int vtxOffset = 0;
            int idxOffset = 0;

            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImDrawListPtr cmdList = drawData.CmdListsRange[n];

                for (int cmdi = 0; cmdi < cmdList.CmdBuffer.Size; cmdi++)
                {
                    ImDrawCmdPtr drawCmd = cmdList.CmdBuffer[cmdi];

                    if (!_loadedTextures.ContainsKey(drawCmd.TextureId))
                    {
                        throw new InvalidOperationException($"Could not find a texture with id '{drawCmd.TextureId}', please check your bindings");
                    }

                    _graphicsDevice.ScissorRectangle = new Rectangle(
                        (int)drawCmd.ClipRect.X + x,
                        (int)drawCmd.ClipRect.Y + y,
                        (int)(drawCmd.ClipRect.Z - drawCmd.ClipRect.X),
                        (int)(drawCmd.ClipRect.W - drawCmd.ClipRect.Y)
                    );

                    var effect = UpdateEffect(_loadedTextures[drawCmd.TextureId], Matrix.Identity);

                    if (drawCmd.ElemCount > 0)
                    {
                        foreach (var pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();

#pragma warning disable CS0618 // // FNA does not expose an alternative method.
                            _graphicsDevice.DrawIndexedPrimitives(
                                primitiveType: PrimitiveType.TriangleList,
                                baseVertex: (int)(drawCmd.VtxOffset) + vtxOffset,
                                minVertexIndex: 0,
                                numVertices: cmdList.VtxBuffer.Size,
                                startIndex: (int)(drawCmd.IdxOffset) + idxOffset,
                                primitiveCount: (int)drawCmd.ElemCount / 3
                            );
#pragma warning restore CS0618
                        }
                    }

                    
                }
                idxOffset += cmdList.IdxBuffer.Size;
                vtxOffset += cmdList.VtxBuffer.Size;
            }
        }

        #endregion Internals
    }
}