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

using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;

namespace FallingBombs.Windows
{
    class PrimitiveDrawing
    {
        private static Color defaultColor = new Color(0, 0, (int)byte.MaxValue);
        private static List<VertexPositionColor> TriangleVerts = new List<VertexPositionColor>();
        private static List<VertexPositionColor> LineVerts = new List<VertexPositionColor>();
        private const int circleSideAmounts = 20;
        private static BasicEffect basicEffect;
        private static World world;
        private static VertexPositionColor firstVPC;
        private static VertexPositionColor secondVPC;
        private static VertexPositionColor thirdVPC;
        private static bool shaking;
        private static float shakeMagnitude;
        private static float shakeDuration;
        private static float shakeTimer;

        public static void SetUp(World startWorld)
        {
            PrimitiveDrawing.world = startWorld;
        }

        public static void Shake(float magnitude, float duration)
        {
            if (!PrimitiveDrawing.shaking)
            {
                PrimitiveDrawing.shaking = true;
                PrimitiveDrawing.shakeTimer = 0.0f;
                PrimitiveDrawing.shakeDuration = duration;
                PrimitiveDrawing.shakeMagnitude = magnitude;
            }
            else
                PrimitiveDrawing.shakeMagnitude += magnitude;
        }

        public static void Draw(float yPos, float worldScreenWidth, GraphicsDevice gd, Game1 game, GameTime gameTime, Random rand, Viewport viewport)
        {
            float xPosition = 0.0f;
            float yPosition = -yPos;
            float zPosition = 0.0f;
            if (PrimitiveDrawing.shaking)
            {
                PrimitiveDrawing.shakeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if ((double)PrimitiveDrawing.shakeTimer >= (double)PrimitiveDrawing.shakeDuration)
                {
                    PrimitiveDrawing.shaking = false;
                    PrimitiveDrawing.shakeTimer = PrimitiveDrawing.shakeDuration;
                }
                float num1 = PrimitiveDrawing.shakeTimer / PrimitiveDrawing.shakeDuration;
                float num2 = PrimitiveDrawing.shakeMagnitude * (float)(1.0 - (double)num1 * (double)num1);
                xPosition = (float)(rand.NextDouble() * 2.0 - 1.0) * num2;
                yPosition += (float)(rand.NextDouble() * 2.0 - 1.0) * num2;
            }
            PrimitiveDrawing.basicEffect = new BasicEffect(gd)
            {
                Projection = Matrix.CreateOrthographic(worldScreenWidth, worldScreenWidth / gd.Viewport.AspectRatio, 0.0f, 1f),
                View = Matrix.CreateTranslation(xPosition, yPosition, zPosition),
                VertexColorEnabled = true
            };
            foreach (Body body in PrimitiveDrawing.world.BodyList)
            {
                if (body != game.highScoreLine || body.UserData == (ValueType)Mine.MineColor)
                    PrimitiveDrawing.DrawBody(body);
            }
            PrimitiveDrawing.DrawBody(game.highScoreLine);
            gd.Viewport = viewport;
            foreach (EffectPass effectPass in PrimitiveDrawing.basicEffect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                if (PrimitiveDrawing.TriangleVerts.Count > 0)
                    gd.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, PrimitiveDrawing.TriangleVerts.ToArray(), 0, PrimitiveDrawing.TriangleVerts.Count / 3);
                if (PrimitiveDrawing.LineVerts.Count > 0)
                    gd.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, PrimitiveDrawing.LineVerts.ToArray(), 0, PrimitiveDrawing.LineVerts.Count / 2);
            }
            PrimitiveDrawing.LineVerts.Clear();
            PrimitiveDrawing.TriangleVerts.Clear();
        }

        private static void DrawBody(Body body)
        {
            foreach (Fixture fixture in body.FixtureList)
            {
                Vertices vertsFromFixture = PrimitiveDrawing.GetVertsFromFixture(fixture);
                if (fixture.UserData is Color)
                    PrimitiveDrawing.DrawShape(vertsFromFixture, (Color)fixture.UserData, fixture.Body.Position, fixture.Body.Rotation, fixture.Shape.ShapeType == ShapeType.Circle);
                else if (!(fixture.UserData is Texture))
                    PrimitiveDrawing.DrawShape(vertsFromFixture, PrimitiveDrawing.defaultColor, fixture.Body.Position, fixture.Body.Rotation, fixture.Shape.ShapeType == ShapeType.Circle);
            }
        }

        public static void DrawShape(Vertices fixtureVerts, Color color, Vector2 position, float rotation, bool drawRadius)
        {
            bool flag1 = false;
            Vector2 vector1 = Vector2.Zero;
            bool flag2 = false;
            Vector2 vector2 = Vector2.Zero;
            Matrix translation = Matrix.CreateTranslation(PrimitiveDrawing.MakeVector3(position));
            Matrix rotationZ = Matrix.CreateRotationZ(rotation);
            Matrix matrix = rotationZ * translation;
            foreach (Vector2 vector3 in (List<Vector2>)fixtureVerts)
            {
                if (!flag1)
                {
                    flag1 = true;
                    vector1 = vector3;
                }
                else if (!flag2)
                {
                    flag2 = true;
                    vector2 = vector3;
                }
                else
                {
                    Vector3 vector3_1 = Vector3.Transform(PrimitiveDrawing.MakeVector3(vector1), matrix);
                    Vector3 vector3_2 = Vector3.Transform(PrimitiveDrawing.MakeVector3(vector2), matrix);
                    Vector3 vector3_3 = Vector3.Transform(PrimitiveDrawing.MakeVector3(vector3), matrix);
                    PrimitiveDrawing.firstVPC.Position = vector3_1;
                    PrimitiveDrawing.firstVPC.Color = color;
                    PrimitiveDrawing.secondVPC.Position = vector3_2;
                    PrimitiveDrawing.secondVPC.Color = color;
                    PrimitiveDrawing.thirdVPC.Position = vector3_3;
                    PrimitiveDrawing.thirdVPC.Color = color;
                    PrimitiveDrawing.TriangleVerts.Add(PrimitiveDrawing.thirdVPC);
                    PrimitiveDrawing.TriangleVerts.Add(PrimitiveDrawing.secondVPC);
                    PrimitiveDrawing.TriangleVerts.Add(PrimitiveDrawing.firstVPC);
                    vector2 = vector3;
                }
            }
            if (!drawRadius)
                return;
            Vector3 position1 = Vector3.Transform(Vector3.Transform(PrimitiveDrawing.MakeVector3(fixtureVerts[0]), rotationZ), translation);
            PrimitiveDrawing.LineVerts.Add(new VertexPositionColor(position1, Color.White));
            PrimitiveDrawing.LineVerts.Add(new VertexPositionColor(PrimitiveDrawing.MakeVector3(position), Color.White));
        }

        private static Vector3 MakeVector3(Vector2 vector)
        {
            return new Vector3(vector.X, vector.Y, 0.0f);
        }

        private static Vertices GetVertsFromFixture(Fixture fixture)
        {
            if (fixture.Shape.ShapeType == ShapeType.Circle)
                return PolygonTools.CreateCircle(fixture.Shape.Radius, 20);
            return ((PolygonShape)fixture.Shape).Vertices;
        }
    }
}
