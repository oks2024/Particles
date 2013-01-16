using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
//using Microsoft.Kinect;

namespace ParticlesVTF
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    /// 

    enum RenderMode
    {
        color,
        texture,
        fluid
    };

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        QuadRenderComponent quadRender;
        Random rand;
        /*
        KinectSensor kinectSensor;

        Skeleton[] skeletonList;
        Skeleton skeleton;
        */
        Camera camera;

        int particleCount = 1024;

        // r : position.x, g : position.y, b : acceleration.x, a : acceleration.y;
        RenderTarget2D physicsRT;
        RenderTarget2D temporaryRT;
        RenderTarget2D particlesRT;
        RenderTarget2D backgroundRT;
        RenderTarget2D playerRT;

        RenderTarget2D kinectRGBVideo;
        RenderTarget2D kinectDepthVideo;

        Effect physicsPassEffect;
        Effect renderParticlesEffect;
        Effect solidParticlesEffect;
        Effect playerMaskEffect;

        Texture2D circleTexture;
        Texture2D randomTexture;
        Texture2D imageTexture;
        Texture2D imageToDraw;
        Texture2D flowMap;

        SpriteFont spriteFont;

        VertexBuffer[] particlesVB = new VertexBuffer[10];

        Vector2 rightHandPosition;
        Vector2 leftHandPosition;

        bool isMouseControl = false;
        bool showBuffers = false;
        bool isPhysicsReset = false;
        bool isPaused = false;
        bool isKeyPressed = false;
        bool isRandomBounceActivated = false;
        bool showData = false;
        bool isMonochrome = true;
        bool isGame = false;
        bool isTyping = false;

        bool isGravityEnabled = true;
        bool isFlow = false;
        bool isInertia = false;

        RenderMode renderMode = RenderMode.color;

        int verticesPerParticles = 3;
        int height;
        int width;
        float isAttracting = 1.0f;

        string particleString = String.Empty;
        string currentString = "Alexandre Pestana";

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            width = 1280;
            height = 800;
            graphics.IsFullScreen = false;
            
            graphics.PreferredBackBufferWidth = width;
            graphics.PreferredBackBufferHeight = height;
            
            Content.RootDirectory = "Content";
            camera = new Camera(this, true);
            Components.Add(camera);

            quadRender = new QuadRenderComponent(this);

            //IsFixedTimeStep = false;
            //graphics.SynchronizeWithVerticalRetrace = false;

            Components.Add(quadRender);
            
        }


        protected override void Initialize()
        {

            rand = new Random();
            /*
            //this.IsMouseVisible = true;
            //TransformSmoothParameters smoothParameters = new TransformSmoothParameters();
            smoothParameters.Correction = 0.75f;
            //smoothParameters.Smoothing = 0.75f;
            smoothParameters.Correction = 0.0f; 
            smoothParameters.Prediction = 0.75f;
            smoothParameters.JitterRadius = 0.05f;
            smoothParameters.MaxDeviationRadius = 0.04f;

            if (KinectSensor.KinectSensors.Count != 0)
            {
                kinectSensor = KinectSensor.KinectSensors[0];
                kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
                kinectSensor.SkeletonStream.Enable(smoothParameters);

                kinectSensor.Start();
                kinectSensor.ElevationAngle = 15;
                //kinectSensor.ColorFrameReady += ColorImageReady;
                //kinectSensor.SkeletonFrameReady += SkeletonDataReady;

                kinectSensor.AllFramesReady += AllDataReady;
            }
            */
            base.Initialize();
        }

        protected override void LoadContent()
        {
            
            
            
            spriteBatch = new SpriteBatch(GraphicsDevice);

            physicsRT = new RenderTarget2D(graphics.GraphicsDevice, particleCount, particleCount, false, SurfaceFormat.Vector4, DepthFormat.None);
            temporaryRT = new RenderTarget2D(graphics.GraphicsDevice, particleCount, particleCount, false, SurfaceFormat.Vector4, DepthFormat.None);
            particlesRT = new RenderTarget2D(graphics.GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None);
            backgroundRT = new RenderTarget2D(graphics.GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None);
            playerRT = backgroundRT = new RenderTarget2D(graphics.GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None);


            randomTexture = new Texture2D(GraphicsDevice, particleCount, particleCount, false, SurfaceFormat.Vector4);
            circleTexture = Content.Load<Texture2D>(@"circle");
            imageTexture = Content.Load<Texture2D>(@"cnam-enjim1");
            imageToDraw = Content.Load<Texture2D>(@"CV4");
            flowMap = Content.Load<Texture2D>(@"flow3");

            physicsPassEffect = Content.Load<Effect>(@"PhysicsPass");
            renderParticlesEffect = Content.Load<Effect>(@"Particles");
            solidParticlesEffect = Content.Load<Effect>(@"SolidParticles");
            playerMaskEffect = Content.Load<Effect>(@"playerMask");

            spriteFont = Content.Load<SpriteFont>(@"SpriteFont");

            for (int nbPass = 0; nbPass < ((int)Math.Floor(particleCount / 1024.0f)); nbPass++)
            {
                VertexPositionColor[] vertices = new VertexPositionColor[1024 * 1024 * verticesPerParticles];

                for (int i = 0; i < 1024; i++)
                {
                    for (int j = 0; j < 1024; j++)
                    {

                        Color colorTmp = new Color(50, 50, /*(byte)(200 + rand.Next(50))*/200, 10);
                        //colorTmp = Color.Yellow;
                        VertexPositionColor vert1 = new VertexPositionColor();
                        vert1.Color = colorTmp;
                        vert1.Position = new Vector3();
                        vert1.Position.X = ((float)i + particleCount*nbPass) / (float)particleCount;
                        vert1.Position.Y = ((float)j + particleCount*nbPass) / (float)particleCount;
                        vert1.Position.Z = 0;
                        vertices[i * 1024 * verticesPerParticles + j * verticesPerParticles] = vert1;

                        VertexPositionColor vert2 = new VertexPositionColor();
                        vert2.Color = colorTmp;
                        vert2.Position = new Vector3();
                        vert2.Position.X = ((float)i + particleCount*nbPass) / (float)particleCount;
                        vert2.Position.Y = ((float)j + particleCount*nbPass) / (float)particleCount;
                        vert2.Position.Z = 1;
                        vertices[i * 1024 * verticesPerParticles + j * verticesPerParticles + 1] = vert2;

                        VertexPositionColor vert3 = new VertexPositionColor();
                        vert3.Color = colorTmp;
                        vert3.Position = new Vector3();
                        vert3.Position.X = ((float)i + particleCount*nbPass) / (float)particleCount;
                        vert3.Position.Y = ((float)j + particleCount*nbPass) / (float)particleCount;
                        vert3.Position.Z = 2;
                        vertices[i * 1024 * verticesPerParticles + j * verticesPerParticles + 2] = vert3;

                        /*VertexPositionColor vert4 = new VertexPositionColor();
                        vert4.Color = colorTmp;
                        vert4.Position = new Vector3();
                        vert4.Position.X = (float)i / (float)particleCount;
                        vert4.Position.Y = (float)j / (float)particleCount;
                        vert4.Position.Z = 3;
                        vertices[i * particleCount * 4 + j * 4 + 3] = vert4;*/
                    }
                }
                particlesVB[nbPass] = new VertexBuffer(graphics.GraphicsDevice, typeof(VertexPositionColor), 1024 * 1024 * verticesPerParticles, BufferUsage.WriteOnly);
                particlesVB[nbPass].SetData<VertexPositionColor>(vertices);

            }


            int textureSize = particleCount;
            //randomTexture = new Texture2D(graphics.GraphicsDevice, textureSize, textureSize, 1, TextureUsage.None, SurfaceFormat.Vector4);
            Vector4[] pointsarray = new Vector4[textureSize * textureSize];
            for (int i = 0; i < textureSize * textureSize; i++)
            {
                pointsarray[i] = new Vector4();
                pointsarray[i].X = (float)rand.NextDouble();//-0.5f;
                pointsarray[i].Y = (float)rand.NextDouble();// *800 / 1280;//*/ -0.5f;
                pointsarray[i].Z = (((float)rand.NextDouble() - 0.5f) /5.0f);
                pointsarray[i].W = ((float)rand.NextDouble());// - 0.5f);
            }
            randomTexture.SetData<Vector4>(pointsarray);
            
        }


        protected override void UnloadContent()
        {

        }

   
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)|| Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            rightHandPosition = new Vector2(-10000, -10000);
            if (!isKeyPressed && !isTyping)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.M))
                    isMouseControl = true;
                if (Keyboard.GetState().IsKeyDown(Keys.K))
                    isMouseControl = false;
                /*if (Keyboard.GetState().IsKeyDown(Keys.B))
                {
                    showBuffers = !showBuffers;
                    isKeyPressed = true;
                }*/
                if (Keyboard.GetState().IsKeyDown(Keys.C))
                    renderMode = RenderMode.color;
                if (Keyboard.GetState().IsKeyDown(Keys.T))
                    renderMode = RenderMode.texture;
                if (Keyboard.GetState().IsKeyDown(Keys.F))
                    renderMode = RenderMode.fluid;
                if (Keyboard.GetState().IsKeyDown(Keys.P))
                {
                    isPaused = !isPaused;
                    isKeyPressed = true;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.R))
                {
                    isRandomBounceActivated = !isRandomBounceActivated;
                    isKeyPressed = true;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.B))
                {
                    showData = !showData;
                    isKeyPressed = true;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.V))
                {
                    isMonochrome = !isMonochrome;
                    isKeyPressed = true;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.G))
                {
                    isGame = !isGame;
                    isKeyPressed = true;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Back))
                {
                    isPhysicsReset = false;
                    isKeyPressed = true;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Q))
                {
                    isGravityEnabled = !isGravityEnabled;
                    isKeyPressed = true;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.S))
                {
                    isInertia = !isInertia;
                    isKeyPressed = true;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.D))
                {
                    isFlow = !isFlow;
                    isKeyPressed = true;
                }
            }

            if (!isKeyPressed)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                {
                    isTyping = false;
                    isKeyPressed = true;
                    currentString = particleString;
                    particleString = String.Empty;
                }
                if (isTyping)
                {
                    if (Keyboard.GetState().GetPressedKeys().Length != 0)
                    {
                        if (Keyboard.GetState().IsKeyDown(Keys.Space))
                        {
                            particleString += " ";
                        }
                        else
                        {
                            particleString += Keyboard.GetState().GetPressedKeys()[0].ToString();
                        }
                    }
                    isKeyPressed = true;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.N))
                {
                    isTyping = true;
                    isKeyPressed = true;
                }
            }

            if (Keyboard.GetState().GetPressedKeys().Length == 0)
                isKeyPressed = false;

            if (isMouseControl)
            {
                if ((Mouse.GetState().X > 0) && (Mouse.GetState().X < width)
                    && (Mouse.GetState().Y > 0) && (Mouse.GetState().Y < height))
                {
                    rightHandPosition.X = Mouse.GetState().X;
                    rightHandPosition.Y = height - Mouse.GetState().Y;
                }

                isAttracting = 0.0f;

                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    isAttracting = 1.0f;
                }
                else
                {
                    if (Mouse.GetState().RightButton == ButtonState.Pressed)
                    {
                        isAttracting = -1.0f;
                    }
                }
            }
            else
            {
                isAttracting = 1.0f;
                /*
                if (skeleton != null)
                {
                    if (skeleton.TrackingState != SkeletonTrackingState.NotTracked)
                    {
                        //rightHandPosition.X = (((skeleton.Joints[JointType.HandRight].Position.X + 1.0f) / 2.0f) * width) * 2.0f - width/2.0f;
                        //rightHandPosition.Y = (((skeleton.Joints[JointType.HandRight].Position.Y + 1.0f) / 2.0f) * height) * 2.0f - height/2.0f;

                        //leftHandPosition.X = (((skeleton.Joints[JointType.HandLeft].Position.X + 1.0f) / 2.0f) * width) * 2.0f - width / 2.0f;
                        //leftHandPosition.Y = (((skeleton.Joints[JointType.HandLeft].Position.Y + 1.0f) / 2.0f) * height) * 2.0f - height / 2.0f;

                        rightHandPosition.X = (((skeleton.Joints[JointType.HandRight].Position.X + 1.0f) / 2.0f) * width) * 2.0f - width / 2.0f;
                        rightHandPosition.Y = (((skeleton.Joints[JointType.HandRight].Position.Y + 1.0f) / 2.0f) * height) * 2.0f - height / 2.0f;

                        leftHandPosition.X = (((skeleton.Joints[JointType.HandLeft].Position.X + 1.0f) / 2.0f) * width) * 2.0f - width / 2.0f;
                        leftHandPosition.Y = (((skeleton.Joints[JointType.HandLeft].Position.Y + 1.0f) / 2.0f) * height) * 2.0f - height / 2.0f;
                        //Console.Write(rightHandPosition + " " + leftHandPosition);

                        if ((rightHandPosition.X < 0) || (rightHandPosition.X > width)
                         || (rightHandPosition.Y < 0) || (rightHandPosition.Y > height))
                        {
                            rightHandPosition = new Vector2(-10000, -10000);
                        }

                        if ((leftHandPosition.X < 0) || (leftHandPosition.X > width)
                         || (leftHandPosition.Y < 0) || (leftHandPosition.Y > height))
                        {
                            leftHandPosition = new Vector2(-10000, -10000);
                        }

                        if (Vector2.Distance(rightHandPosition, leftHandPosition) > 200.0f)
                        {
                            isAttracting = 0.0f;
                        }

                    }
                    
                }*/
            }

            double fps = (1000 / gameTime.ElapsedGameTime.TotalMilliseconds);
            fps = Math.Round(fps, 0);
            Window.Title = "Drawing " + particleCount * particleCount + " particles at " + fps.ToString() + " FPS";

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (isGame)
            {
                GraphicsDevice.SetRenderTarget(backgroundRT);
                GraphicsDevice.Clear(new Color(0, 0, 0, 0));
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
                //spriteBatch.Draw(circleTexture, new Rectangle((int)playerPosition.X, (int)playerPosition.Y, 300, 10), Color.White);
                //spriteBatch.Draw(circleTexture, new Rectangle((int)playerPosition.X, (int)playerPosition.Y, 100, 10), Color.White);
                //spriteBatch.Draw(circleTexture, new Rectangle((int)playerPosition.X + 90, (int)playerPosition.Y /*+ 190*/, 10, 200), Color.White);
                //spriteBatch.Draw(circleTexture, new Rectangle((int)playerPosition.X, (int)playerPosition.Y /*+ 190*/, 10, 200), Color.White);
                spriteBatch.Draw(imageToDraw, new Rectangle(0, 0, 1280, 800), Color.White);
                //spriteBatch.DrawString(spriteFont, currentString, new Vector2(300, 200), Color.Black, 0, new Vector2(0, 0), new Vector2(1.0f, 1.0f), SpriteEffects.None, 0);
                //spriteBatch.Draw(imageTexture, new Rectangle(400, 400, 400, 400), Color.White);
                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
                //spriteBatch.Draw(circleTexture, new Rectangle((int)rightHandPosition.X - 50, (height - (int)rightHandPosition.Y) - 5, 100, 10), Color.White);
                spriteBatch.End();
                GraphicsDevice.SetRenderTarget(null);
            }
            else
            {
                GraphicsDevice.SetRenderTarget(backgroundRT);
                GraphicsDevice.Clear(new Color(0, 0, 0, 0));
                GraphicsDevice.SetRenderTarget(null);
            }

            GraphicsDevice.Clear(Color.Black);

            SimulateParticles(gameTime);
            if ( renderMode != RenderMode.fluid)
                GraphicsDevice.SetRenderTarget(null);
            else
                GraphicsDevice.SetRenderTarget(particlesRT);

            GraphicsDevice.Clear(Color.Black);

            renderParticlesEffect.Parameters["World"].SetValue(Matrix.Identity);
            renderParticlesEffect.Parameters["View"].SetValue(camera.View);
            renderParticlesEffect.Parameters["Projection"].SetValue(camera.Projection);
            renderParticlesEffect.Parameters["physicsMap"].SetValue(physicsRT);
            renderParticlesEffect.Parameters["isMonochrome"].SetValue(isMonochrome);

            if (renderMode == RenderMode.color)
            {
                renderParticlesEffect.CurrentTechnique = renderParticlesEffect.Techniques["colorParticles"];
            }
            if (renderMode == RenderMode.texture)
            {
                renderParticlesEffect.Parameters["image"].SetValue(imageTexture);
                renderParticlesEffect.CurrentTechnique = renderParticlesEffect.Techniques["textureParticles"];
            }

            BlendState blendState = new BlendState();
            blendState.AlphaBlendFunction = BlendFunction.Add;
            blendState.AlphaSourceBlend = Blend.SourceAlpha;
            blendState.ColorSourceBlend = Blend.SourceAlpha;
            blendState.ColorDestinationBlend = Blend.One;

            GraphicsDevice.BlendState = blendState;
            //BepthStencilState.DepthBufferEnable = false;
            /*
            GraphicsDevice.BlendState.AlphaBlendFunction = BlendFunction.Add;
            GraphicsDevice.BlendState.AlphaSourceBlend = Blend.SourceAlpha;
            GraphicsDevice.BlendState.ColorSourceBlend = Blend.SourceAlpha;
            GraphicsDevice.BlendState.ColorDestinationBlend = Blend.One;
            GraphicsDevice.DepthStencilState.DepthBufferEnable = false;
            */
            //using (VertexDeclaration decl = VertexPositionColor.VertexDeclaration)
            for (int nbPass = 0; nbPass < ((int)Math.Floor(particleCount / 1024.0f)); nbPass++)
            {
                //GraphicsDevice.V
                renderParticlesEffect.CurrentTechnique.Passes[0].Apply();
                GraphicsDevice.SetVertexBuffer(particlesVB[nbPass]);
                //GraphicsDevice.SetVertexBuffers(particlesVB[0], particlesVB[1], particlesVB[2]);

                //GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, particleCount * particleCount * verticesPerParticles);
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 512 * 512 * verticesPerParticles);
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 512 * 512 * verticesPerParticles, 512 * 512 * verticesPerParticles);
                //GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 1024 * 1024 * verticesPerParticles, 512 * 512 * verticesPerParticles);
                //GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 1536 * 1536 * verticesPerParticles, 512 * 512 * verticesPerParticles);
                //GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 2048 * 2048 * verticesPerParticles, 512 * 512 * verticesPerParticles);
                //GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 2560 * 2560 * verticesPerParticles, 512 * 512 * verticesPerParticles);



            }


            if (renderMode == RenderMode.fluid)
            {
                GraphicsDevice.SetRenderTarget(null);
                /*spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
                spriteBatch.End();*/

                blendState = new BlendState();
                blendState.AlphaBlendFunction = BlendFunction.Add;
                blendState.AlphaSourceBlend = Blend.SourceAlpha;
                blendState.AlphaDestinationBlend = Blend.One;
                blendState.ColorSourceBlend = Blend.SourceAlpha;
                blendState.ColorBlendFunction = BlendFunction.Add;
                blendState.ColorDestinationBlend = Blend.One;

                GraphicsDevice.BlendState = blendState;

                GraphicsDevice.Clear(Color.Black);
                solidParticlesEffect.Parameters["particles"].SetValue(particlesRT);
                solidParticlesEffect.CurrentTechnique.Passes[0].Apply();

                quadRender.Render(Vector2.One * -1, Vector2.One);
            }


            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            spriteBatch.Draw(circleTexture, new Rectangle((int)rightHandPosition.X - 5, (height - (int)rightHandPosition.Y) - 5, 10, 10), Color.White);
            //spriteBatch.Draw(circleTexture, new Rectangle((int)leftHandPosition.X - 5, (height - (int)leftHandPosition.Y) - 5, 10, 10), Color.White);
            spriteBatch.End();



            //temporaryRT = physicsRT;
            //GraphicsDevice.SetRenderTarget(physicsRT);


            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);

            if (showData)
            {
                spriteBatch.Draw(physicsRT, new Rectangle(0, 0, 256, 256), Color.White);
                spriteBatch.Draw(particlesRT, new Rectangle(0, 256, 256, 256), Color.White);
                spriteBatch.Draw(randomTexture, new Rectangle(0, 512, 256, 256), Color.White);
            }



            if (showBuffers)
            {
                if (kinectRGBVideo != null)
                    spriteBatch.Draw(kinectRGBVideo, new Rectangle(0, 0, 320, 240), Color.White);
                if (kinectDepthVideo != null)
                    spriteBatch.Draw(kinectDepthVideo, new Rectangle(0, 240, 320, 240), Color.White);
            }
            spriteBatch.End();
            
            ///////////////////// TEST ///////////////////////////

            //playerMaskEffect.Parameters["depthMap"].SetValue(kinectDepthVideo);
            //playerMaskEffect.CurrentTechnique.Passes[0].Apply();
            //quadRender.Render(Vector2.One * -1, Vector2.One);



            //GraphicsDevice.SetRenderTarget(null);
            ///////////////////// TEST ///////////////////////////


            base.Draw(gameTime);
        }

        private void DoPhysicsPass(string technique, RenderTarget2D resultTarget)
        {
            //RenderTarget2D oldRT = graphics.GraphicsDevice.GetRenderTargets()[0] as RenderTarget2D;


            GraphicsDevice.SetRenderTarget(temporaryRT);
            GraphicsDevice.Clear(ClearOptions.Target, Color.White, 1, 0);

            physicsPassEffect.CurrentTechnique = physicsPassEffect.Techniques[technique];

            if (isPhysicsReset)
            {
                physicsPassEffect.Parameters["physicsMap"].SetValue(physicsRT);
                //physicsPassEffect.Parameters["randomMap"].SetValue(randomTexture);
            }

            Vector4 GravityFlow = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);

            if (isGravityEnabled)
                GravityFlow.Y = -100.0f;

            if (isFlow)
                GravityFlow.Z = 8.0f;

            if (isInertia)
                GravityFlow.W = 0.97f;


            physicsPassEffect.Parameters["randomMap"].SetValue(randomTexture);
            physicsPassEffect.Parameters["width"].SetValue(this.GraphicsDevice.PresentationParameters.BackBufferWidth);
            physicsPassEffect.Parameters["height"].SetValue(this.GraphicsDevice.PresentationParameters.BackBufferHeight);
            physicsPassEffect.Parameters["mousePosition"].SetValue(rightHandPosition);
            physicsPassEffect.Parameters["isAttracting"].SetValue(isAttracting);
            physicsPassEffect.Parameters["randomBounce"].SetValue(isRandomBounceActivated);
            physicsPassEffect.Parameters["textureSize"].SetValue(particleCount);
            physicsPassEffect.Parameters["backgroundMap"].SetValue(backgroundRT);
            physicsPassEffect.Parameters["flowMap"].SetValue(flowMap);
            physicsPassEffect.Parameters["GravityFlow"].SetValue(GravityFlow);
            
            //physicsPassEffect.Parameters["physicsMap"].SetValue(physicsRT);

            //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque,SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, physicsPassEffect);

            
            physicsPassEffect.CurrentTechnique.Passes[0].Apply();

            //spriteBatch.Draw(randomTexture, new Rectangle(0, 0, particleCount, particleCount), Color.White);
            quadRender.Render(Vector2.One * -1, Vector2.One);

            //spriteBatch.End();

            //GraphicsDevice.SetRenderTarget(null);

            /// Copy to the result RT;
            GraphicsDevice.SetRenderTarget(resultTarget);

            physicsPassEffect.CurrentTechnique = physicsPassEffect.Techniques["CopyTexture"];

            physicsPassEffect.Parameters["temporaryMap"].SetValue(temporaryRT);

            GraphicsDevice.Clear(Color.White);

            physicsPassEffect.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            quadRender.Render(Vector2.One * -1, Vector2.One);


            GraphicsDevice.SetRenderTarget(null);


        }

        private void SimulateParticles(GameTime gameTime)
        {
            if ( !isPaused)
                physicsPassEffect.Parameters["elapsedTime"].SetValue((float)gameTime.ElapsedGameTime.TotalSeconds);
            else
                physicsPassEffect.Parameters["elapsedTime"].SetValue(0.0f);

            physicsPassEffect.Parameters["currentTime"].SetValue(gameTime.TotalGameTime.Milliseconds);

            if (!isPhysicsReset)
            {
                DoPhysicsPass("ResetPositions", physicsRT);
                //DoPhysicsPass("ResetVelocities", velocityRT);

                isPhysicsReset = true;
            }

            //DoPhysicsPass("UpdateVelocities", velocityRT);
            DoPhysicsPass("UpdatePhysics", physicsRT);
        }

        /*
        private void AllDataReady(object sender, AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame imageFrame = e.OpenColorImageFrame())
            {

                if (imageFrame != null)
                {
                    byte[] pixelData;
                    pixelData = new byte[imageFrame.PixelDataLength];
                    imageFrame.CopyPixelDataTo(pixelData);
                    kinectRGBVideo = new RenderTarget2D(GraphicsDevice, 640, 480, false, SurfaceFormat.Color, DepthFormat.None);
                    //kinectRGBVideo.
                    kinectRGBVideo.SetData<byte>(pixelData);
                }
                else
                {
                    // imageFrame is null because the request did not arrive in time          }
                }
            }

            using (DepthImageFrame imageFrame = e.OpenDepthImageFrame())
            {

                if (imageFrame != null)
                {
                    short[] pixelData;
                    pixelData = new short[imageFrame.PixelDataLength];
                    imageFrame.CopyPixelDataTo(pixelData);
                    kinectDepthVideo = new RenderTarget2D(GraphicsDevice, 320, 240, false, SurfaceFormat.HalfSingle, DepthFormat.None);
                    //kinectRGBVideo.
                    kinectDepthVideo.SetData<short>(pixelData);
                }
                else
                {
                    // imageFrame is null because the request did not arrive in time          }
                }
            }

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {

                    skeletonList = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletonList);
                    /*if (skeletonList[0].TrackingState == SkeletonTrackingState.Tracked)
                    {
                        Console.Write(skeletonList[0].Joints[JointType.HandRight].Position.X + " " + skeletonList[0].Joints[JointType.HandRight].Position.Y + " " + skeletonList[0].Joints[JointType.HandRight].Position.Z + "\n");
                    }
                    Console.Write(skeletonList[0].Joints[JointType.HandRight].Position.X + " " + skeletonList[0].Joints[JointType.HandRight].Position.Y + " " + skeletonList[0].Joints[JointType.HandRight].Position.Z + "\n");
                    if (skeletonList[0].TrackingState == SkeletonTrackingState.PositionOnly)
                    {
                        Console.Write(skeletonList[0].Joints[JointType.HandRight].Position.X + " " + skeletonList[0].Joints[JointType.HandRight].Position.Y + " " + skeletonList[0].Joints[JointType.HandRight].Position.Z + "\n");
                    }*//*
                    skeleton = (from s in skeletonList
                                where s.TrackingState == SkeletonTrackingState.Tracked
                                select s).FirstOrDefault();

                    
                }
            }
        }*/

    }
}
