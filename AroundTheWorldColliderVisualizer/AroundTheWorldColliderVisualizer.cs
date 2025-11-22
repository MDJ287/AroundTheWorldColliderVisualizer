using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AroundTheWorldColliderVisualizer
{
    public class AroundTheWorldColliderVisualizer : ModBehaviour
    {
        public static AroundTheWorldColliderVisualizer Instance;

        public void Awake()
        {
            Instance = this;
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
        }

        public void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"My mod {nameof(AroundTheWorldColliderVisualizer)} is loaded!", MessageType.Success);

            new Harmony("MDJ287.AroundTheWorldColliderVisualizer").PatchAll(Assembly.GetExecutingAssembly());

            // Example of accessing game code.
            OnCompleteSceneLoad(OWScene.TitleScreen, OWScene.TitleScreen); // We start on title screen
            LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
        }

        public void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene)
        {
            if (newScene != OWScene.SolarSystem) return;
            ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);
        }

        public void Update()
        {
            if (Keyboard.current.slashKey.wasPressedThisFrame)
            {
                Achievement_AroundTheWorld achievementObj = FindObjectOfType<Achievement_AroundTheWorld>();
                int i = 0;
                foreach (Transform x in achievementObj.transform)
                {
                    shapesToDraw[i] = x.GetComponent<BoxShape>();
                    i++;
                }
            }
        }

        private BoxShape[] shapesToDraw = new BoxShape[4];

        public void OnRenderObject()
        {
            if (shapesToDraw == null) return;

            CreateLineMaterial();
            lineMaterial.SetPass(0);

            RenderShapes(shapesToDraw, 4);

            // DamTrigger, Zone1to2Trigger, CanyonTrigger, WaterRiseTrigger

            /*
            ShapeManager.Layer layer = ShapeManager._detectors;
            this.RenderShapeBounds(layer._array, layer.Count);
            ShapeManager.Layer[] layers = ShapeManager._volumes;
            for (int i = 0; i < layers.Length; i++)
            {
                this.RenderShapeBounds(layers[i]._array, layers[i].Count);
            }
            */
        }

        private void RenderShapes(BoxShape[] shapes, int count)
        {
            Color[] cols = [Color.red, Color.green, Color.blue, Color.yellow];
            for (int i = 0; i < count; i++)
            {
                bool flag = shapes[i] != null;
                if (flag)
                {
                    BoxShape boxShape = shapes[i];
                    Vector3[] boxAxes = new Vector3[3];
                    Vector3[] verts = new Vector3[8];
                    Vector3 boxCenter;
                    Vector3 boxSize;
                    ShapeUtil.Box.CalcWorldSpaceData(boxShape, out boxCenter, out boxSize, ref boxAxes, ref verts);
                    DrawWireframeCube(boxAxes[2] * boxSize.z, boxAxes[1] * boxSize.y, boxAxes[0] * boxSize.x, boxCenter, Color.Lerp(cols[i], Color.black, 0.5f));
                }
            }
        }

        private static void DrawWireframeCube(Vector3 foward, Vector3 up, Vector3 right, Vector3 offset, Color color)
        {
            GL.wireframe = true;

            Vector3[] vertex = new Vector3[]
            {
                (foward + right) / 2f,
                (-foward + right) / 2f,
                (-foward - right) / 2f,
                (foward - right) / 2f
            };
            GL.Begin(2);
            GL.Color(color);
            for (int i = 0; i < 4; i++)
            {
                GL.Vertex(vertex[i] + offset - up / 2f);
            }
            GL.Vertex(vertex[0] - up / 2f);
            GL.End();
            GL.Begin(2);
            GL.Color(color);
            for (int j = 0; j < 4; j++)
            {
                GL.Vertex(vertex[j] + offset + up / 2f);
            }
            GL.Vertex(vertex[0] + offset + up / 2f);
            GL.End();
            GL.Begin(1);
            for (int k = 0; k < 4; k++)
            {
                GL.Color(color);
                GL.Vertex(vertex[k] + offset - up / 2f);
                GL.Vertex(vertex[k] + offset + up / 2f);
            }
            GL.End();

            // mine

            GL.wireframe = false;

            color.a = 0.2f;

            //down
            GL.Begin(GL.QUADS);
            GL.Color(color);
            for (int i = 0; i < 5; i++)
            {
                GL.Vertex(vertex[i % 4] + offset - up / 2f);
            }
            GL.End();

            //up
            GL.Begin(GL.QUADS);
            GL.Color(color);
            for (int i = 0; i < 5; i++)
            {
                GL.Vertex(vertex[i % 4] + offset + up / 2f);
            }
            GL.End();

            //left
            GL.Begin(GL.QUADS);
            GL.Color(color);
            GL.Vertex(vertex[3] + offset - up / 2f);
            GL.Vertex(vertex[3] + offset + up / 2f);
            GL.Vertex(vertex[2] + offset + up / 2f);
            GL.Vertex(vertex[2] + offset - up / 2f);
            GL.Vertex(vertex[3] + offset - up / 2f);
            GL.End();

            // right
            GL.Begin(GL.QUADS);
            GL.Color(color);
            GL.Vertex(vertex[0] + offset - up / 2f);
            GL.Vertex(vertex[0] + offset + up / 2f);
            GL.Vertex(vertex[1] + offset + up / 2f);
            GL.Vertex(vertex[1] + offset - up / 2f);
            GL.Vertex(vertex[0] + offset - up / 2f);
            GL.End();

            // forward
            GL.Begin(GL.QUADS);
            GL.Color(color);
            GL.Vertex(vertex[0] + offset - up / 2f);
            GL.Vertex(vertex[0] + offset + up / 2f);
            GL.Vertex(vertex[3] + offset + up / 2f);
            GL.Vertex(vertex[3] + offset - up / 2f);
            GL.Vertex(vertex[0] + offset - up / 2f);
            GL.End();

            // backward
            GL.Begin(GL.QUADS);
            GL.Color(color);
            GL.Vertex(vertex[1] + offset - up / 2f);
            GL.Vertex(vertex[1] + offset + up / 2f);
            GL.Vertex(vertex[2] + offset + up / 2f);
            GL.Vertex(vertex[2] + offset - up / 2f);
            GL.Vertex(vertex[1] + offset - up / 2f);
            GL.End();
        }

        private static void DrawWireframeSphere(float radius, Vector3 offset, Vector3 foward, Vector3 up, Color color, int resolution = 3)
        {
            Vector3 right = Vector3.Cross(foward, up);
            DrawWireframeCircle(radius, up, foward, offset, color, resolution, 0f, 6.2831855f, true);
            DrawWireframeCircle(radius, foward, right, offset, color, resolution, 0f, 6.2831855f, true);
            DrawWireframeCircle(radius, right, up, offset, color, resolution, 0f, 6.2831855f, true);
            for (int i=0; i<10; i++)
            {
                DrawWireframeCircle(radius, Vector3.Slerp(up, right, i/10f), foward, offset, color, resolution, 0f, 6.2831855f, true);
            }
            for (int i = 0; i < 10; i++)
            {
                DrawWireframeCircle(radius, Vector3.Slerp(right, -up, i / 10f), foward, offset, color, resolution, 0f, 6.2831855f, true);
            }
            for (int i = 0; i < 10; i++)
            {
                DrawWireframeCircle(radius, Vector3.Slerp(-up, -right, i / 10f), foward, offset, color, resolution, 0f, 6.2831855f, true);
            }
            for (int i = 0; i < 10; i++)
            {
                DrawWireframeCircle(radius, Vector3.Slerp(-right, up, i / 10f), foward, offset, color, resolution, 0f, 6.2831855f, true);
            }
        }

        public static void DrawWireframeCircle(float radius, Vector3 normal, Vector3 up, Vector3 offset, Color color, int resolution = 3, float startAngle = 0f, float endAngle = 6.2831855f, bool isWholeCircle = true)
        {
            bool flag = resolution < 3 || radius <= 0f;
            if (!flag)
            {
                normal = normal.normalized;
                up = up.normalized;
                GL.Begin(2);
                float angleStep = (endAngle - startAngle) / (float)resolution;
                int aditionalSteps = isWholeCircle ? 1 : 0;
                GL.Color(color);
                Vector3 rotationVector = Vector3.Cross(normal, up);
                for (int i = 0; i <= resolution + aditionalSteps; i++)
                {
                    Vector3 radiusVector = GetRotatedVectorComponent(rotationVector, up, angleStep * (float)i + startAngle);
                    GL.Vertex(radiusVector * radius + offset);
                }
                GL.End();
            }
        }

        private static Vector3 GetRotatedVectorComponent(Vector3 rotationVector, Vector3 perpendicularComponent, float angle)
        {
            float x = Mathf.Cos(angle) / perpendicularComponent.sqrMagnitude;
            float x2 = Mathf.Sin(angle);
            return perpendicularComponent.sqrMagnitude * (x * perpendicularComponent + x2 * rotationVector);
        }

        private static void CreateLineMaterial()
        {
            bool flag = !lineMaterial;
            if (flag)
            {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                lineMaterial = new Material(shader)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                lineMaterial.SetInt("_SrcBlend", 5);
                lineMaterial.SetInt("_DstBlend", 10);
                lineMaterial.SetInt("_Cull", 0);
                lineMaterial.SetInt("_ZWrite", 0);
            }
        }

        private static Material lineMaterial;
    }

}
