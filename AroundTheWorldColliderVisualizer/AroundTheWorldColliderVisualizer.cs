using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AroundTheWorldColliderVisualizer
{
    public class AroundTheWorldColliderVisualizer : ModBehaviour
    {
        public static AroundTheWorldColliderVisualizer Instance;
        
        private bool inSolarSystem = false;

        private Shape[] shapesToDraw = new Shape[1024];
        private Collider[] collidersToDraw = new Collider[1024];
        private static Material lineMaterial;

        private int currentShapeIndex = 0;
        private int currentColliderIndex = 0;

        private bool viewAroundTheWorld = false;
        private bool viewTakeMeAlive = false;
        private bool viewTubular = false;
        private bool viewAlphaPilot = false;

        private bool viewLoopExtension = false;
        private bool viewOxygenVolume = false;
        private bool viewEntrywayTrigger = false;
        private bool viewShipLogFactTriggerVolume = false;

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
            if (newScene != OWScene.SolarSystem)
            {
                inSolarSystem = false;
                return;
            }
            inSolarSystem = true;
            ReloadShapes();
        }
        
        public override void Configure(IModConfig config)
        {
            viewAroundTheWorld = config.GetSettingsValue<bool>("viewAroundTheWorld");
            viewTakeMeAlive = config.GetSettingsValue<bool>("viewTakeMeAlive");
            viewTubular = config.GetSettingsValue<bool>("viewTubular");
            viewAlphaPilot = config.GetSettingsValue<bool>("viewAlphaPilot");

            viewLoopExtension = config.GetSettingsValue<bool>("viewLoopExtension");
            viewOxygenVolume = config.GetSettingsValue<bool>("viewOxygenVolume");
            viewEntrywayTrigger = config.GetSettingsValue<bool>("viewEntrywayTrigger");
            viewShipLogFactTriggerVolume = config.GetSettingsValue<bool>("viewShipLogFactTriggerVolume");

            if (inSolarSystem) ReloadShapes();
        }

        public void Update()
        {
            
        }

        private void ReloadShapes()
        {
            ModHelper.Console.WriteLine("Reloading shapes", MessageType.Info);

            shapesToDraw = new Shape[shapesToDraw.Length];
            collidersToDraw = new Collider[collidersToDraw.Length];
            currentShapeIndex = 0;
            currentColliderIndex = 0;

            // achievements

            Transform[] achievementTransforms = new Transform[8];
            if (viewAroundTheWorld) achievementTransforms[0] = FindObjectOfType<Achievement_AroundTheWorld>()?.transform;
            if (viewTakeMeAlive) achievementTransforms[1] = FindObjectOfType<Achievement_TakeMeAlive>()?.transform;
            if (viewTubular) achievementTransforms[2] = FindObjectOfType<Achievement_Tubular>()?.transform;
            if (viewAlphaPilot) achievementTransforms[3] = FindObjectOfType<Achievement_AlphaPilot>()?.transform;

            for (int i = 0; i < achievementTransforms.Length; i++)
            {
                if (achievementTransforms[i] != null)
                    foreach (Transform x in achievementTransforms[i])
                    {
                        ModHelper.Console.WriteLine($"Visualizing achievement trigger {x.name} from {achievementTransforms[i].name}");
                        AddShape(x.GetComponent<Shape>());
                    }
            }

            // timeloop extension

            if (viewLoopExtension)
            {
                TimeLoopExtensionTrigger[] extTriggers = FindObjectsOfType<TimeLoopExtensionTrigger>();

                for (int i = 0; i < extTriggers.Length; i++)
                {
                    ModHelper.Console.WriteLine($"Visualizing time loop extension trigger {extTriggers[i].name}");
                    AddShape(extTriggers[i].GetComponentInChildren<Shape>());
                }
            }

            // OxygenVolume

            if (viewOxygenVolume)
            {
                OxygenVolume[] oxygenVolumes = FindObjectsOfType<OxygenVolume>();

                for (int i=0; i<oxygenVolumes.Length; i++)
                {
                    ModHelper.Console.WriteLine($"Visualizing oxygen volume {oxygenVolumes[i].name}");
                    Shape shape = oxygenVolumes[i].GetComponent<Shape>();
                    if (shape != null)
                    {
                        AddShape(shape);
                    }
                    else
                    {
                        AddCollider(oxygenVolumes[i].GetComponent<Collider>());
                    }
                }
            }

            // EntrywayTrigger

            if (viewEntrywayTrigger)
            {
                EntrywayTrigger[] entrywayTriggers = FindObjectsOfType<EntrywayTrigger>();

                for (int i = 0; i < entrywayTriggers.Length; i++)
                {
                    ModHelper.Console.WriteLine($"Visualizing entryway trigger {entrywayTriggers[i].name}");
                    Shape shape = entrywayTriggers[i].GetComponent<Shape>();
                    if (shape != null)
                    {
                        AddShape(shape);
                    }
                    else
                    {
                        AddCollider(entrywayTriggers[i].GetComponent<Collider>());
                    }
                }
            }

            // ship log

            if (viewShipLogFactTriggerVolume)
            {
                ShipLogFactTriggerVolume[] logTriggers = FindObjectsOfType<ShipLogFactTriggerVolume>();

                for (int i=0; i < logTriggers.Length; i++)
                {
                    ModHelper.Console.WriteLine($"Visualizing log location {logTriggers[i].name}");
                    Shape shape = logTriggers[i].GetComponent<Shape>();
                    if (shape != null)
                    {
                        AddShape(shape);
                    }
                    else
                    {
                        AddCollider(logTriggers[i].GetComponent<Collider>());
                    }
                }
            }
        }

        private bool AddShape(Shape shape)
        {
            if (shape == null)
            {
                ModHelper.Console.WriteLine($"Failed to add shape", MessageType.Warning);
                return false;
            }
            shapesToDraw[currentShapeIndex] = shape;
            currentShapeIndex++;
            return true;
        }

        private bool AddCollider(Collider collider)
        {
            if (collider == null)
            {
                ModHelper.Console.WriteLine($"Failed to add collider", MessageType.Warning);
                return false;
            }
            collidersToDraw[currentColliderIndex] = collider;
            currentColliderIndex++;
            return true;
        }

        public void OnRenderObject()
        {
            if (shapesToDraw[0] == null && collidersToDraw[0] == null)
            {
                return;
            }

            CreateLineMaterial();
            lineMaterial.SetPass(0);

            RenderShapes(shapesToDraw);
            RenderColliders(collidersToDraw);
        }

        private void RenderShapes(Shape[] shapes)
        {
            Color[] cols;
            int realLength = shapes.Length;
            for (int i=0; i<shapes.Length; i++)
            {
                if (shapes[i] == null)
                {
                    realLength = i;
                    break;
                }
            }
            cols = new Color[realLength];
            for (int i=0; i<realLength; i++)
            {
                cols[i] = Color.HSVToRGB((float)i/realLength, 1f, 1f);
            }
            for (int i = 0; i < realLength; i++)
            {
                if (shapes[i] != null)
                {
                    if (shapes[i].GetType() == typeof(BoxShape))
                    {
                        BoxShape boxShape = (BoxShape)shapes[i];
                        Vector3[] boxAxes = new Vector3[3];
                        Vector3[] verts = new Vector3[8];
                        Vector3 boxCenter;
                        Vector3 boxSize;
                        ShapeUtil.Box.CalcWorldSpaceData(boxShape, out boxCenter, out boxSize, ref boxAxes, ref verts);
                        DrawWireframeCube(boxAxes[2] * boxSize.z, boxAxes[1] * boxSize.y, boxAxes[0] * boxSize.x, boxCenter, cols[i]);
                        DrawFilledCube(boxAxes[2] * boxSize.z, boxAxes[1] * boxSize.y, boxAxes[0] * boxSize.x, boxCenter, cols[i]);
                    }
                    else if (shapes[i].GetType() == typeof(CapsuleShape))
                    {
                        CapsuleShape capsuleShape = (CapsuleShape)shapes[i];
                        float capsuleRadius;
                        Vector3 capsuleStart;
                        Vector3 capsuleEnd;
                        ShapeUtil.Capsule.CalcWorldSpaceEndpoints(capsuleShape, out capsuleRadius, out capsuleStart, out capsuleEnd);
                        DrawWireframeCapsule(capsuleRadius, capsuleStart, capsuleEnd, cols[i], 12);
                    }
                    else if (shapes[i].GetType() == typeof(SphereShape))
                    {
                        SphereShape sphereShape = (SphereShape)shapes[i];
                        Vector3 sphereCenter = ShapeUtil.Sphere.CalcWorldSpaceCenter(sphereShape);
                        float sphereRadius = ShapeUtil.Sphere.CalcWorldSpaceRadius(sphereShape);
                        DrawWireframeSphere(sphereRadius, sphereCenter, sphereShape.transform.forward, sphereShape.transform.up, cols[i], 12);
                    }
                    else if (shapes[i].GetType() == typeof(CylinderShape))
                    {
                        CylinderShape cylinderShape = (CylinderShape)shapes[i];
                        float cylinderRadius;
                        Vector3 cylinderStart;
                        Vector3 cylinderEnd;
                        ShapeUtil.Cylinder.CalcWorldSpaceEndpoints(cylinderShape, out cylinderRadius, out cylinderStart, out cylinderEnd);
                        DrawWireframeCone(cylinderRadius, cylinderRadius, cylinderStart, cylinderEnd, cols[i], 12);
                    }
                }
            }
        }

        private void RenderColliders(Collider[] colliders)
        {
            Color[] cols;
            int realLength = colliders.Length;
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] == null)
                {
                    realLength = i;
                    break;
                }
            }
            cols = new Color[realLength];
            for (int i = 0; i < realLength; i++)
            {
                cols[i] = Color.HSVToRGB((float)i / realLength, 1f, 1f);
            }
            for (int i = 0; i < realLength; i++)
            {
                if (colliders[i] != null)
                {
                    if (colliders[i].GetType() == typeof(BoxCollider))
                    {
                        BoxCollider boxShape = (BoxCollider)colliders[i];
                        Vector3[] boxAxes = new Vector3[3];
                        Vector3[] verts = new Vector3[8];
                        Vector3 boxCenter;
                        Vector3 boxSize;
                        CalcWorldSpaceData(boxShape, out boxCenter, out boxSize, ref boxAxes, ref verts);
                        DrawWireframeCube(boxAxes[2] * boxSize.z, boxAxes[1] * boxSize.y, boxAxes[0] * boxSize.x, boxCenter, cols[i]);
                        DrawFilledCube(boxAxes[2] * boxSize.z, boxAxes[1] * boxSize.y, boxAxes[0] * boxSize.x, boxCenter, cols[i]);
                    }
                    else if (colliders[i].GetType() == typeof(CapsuleCollider))
                    {
                        CapsuleCollider capsuleShape = (CapsuleCollider)colliders[i];
                        float capsuleRadius;
                        Vector3 capsuleStart;
                        Vector3 capsuleEnd;
                        CalcWorldSpaceEndpoints(capsuleShape, out capsuleRadius, out capsuleStart, out capsuleEnd);
                        DrawWireframeCapsule(capsuleRadius, capsuleStart, capsuleEnd, cols[i], 12);
                    }
                    else if (colliders[i].GetType() == typeof(SphereCollider))
                    {
                        SphereCollider sphereShape = (SphereCollider)colliders[i];
                        Vector3 sphereCenter = sphereShape.transform.TransformPoint(sphereShape.center); ;
                        float sphereRadius = CalcWorldSpaceRadius(sphereShape);
                        DrawWireframeSphere(sphereRadius, sphereCenter, sphereShape.transform.forward, sphereShape.transform.up, cols[i], 12);
                    }
                }
            }
        }

        private static void CalcWorldSpaceData(BoxCollider boxShape, out Vector3 center, out Vector3 size, ref Vector3[] axes, ref Vector3[] verts)
        {
            center = boxShape.transform.TransformPoint(boxShape.center);
            size = Vector3.Scale(boxShape.size, boxShape.transform.lossyScale);
            Quaternion rotation = boxShape.transform.rotation;
            axes[0] = rotation * Vector3.right;
            axes[1] = rotation * Vector3.up;
            axes[2] = rotation * Vector3.forward;
            Vector3 vector = size * 0.5f;
            Vector3 b = axes[0] * vector.x;
            Vector3 b2 = axes[1] * vector.y;
            Vector3 b3 = axes[2] * vector.z;
            verts[0] = center + b + b2 + b3;
            verts[1] = center - b + b2 + b3;
            verts[2] = center + b - b2 + b3;
            verts[3] = center - b - b2 + b3;
            verts[4] = center + b + b2 - b3;
            verts[5] = center - b + b2 - b3;
            verts[6] = center + b - b2 - b3;
            verts[7] = center - b - b2 - b3;
        }

        private static void CalcWorldSpaceEndpoints(CapsuleCollider capsuleShape, out float worldSpaceRadius, out Vector3 worldSpaceP1, out Vector3 worldSpaceP2)
        {
            float num = 0f;
            Vector3 lossyScale = capsuleShape.transform.lossyScale;
            int direction = capsuleShape.direction;
            for (int i = 0; i < 3; i++)
            {
                if (i != direction)
                {
                    num = Mathf.Max(num, Mathf.Abs(lossyScale[i]));
                }
            }
            worldSpaceRadius = capsuleShape.radius * num;
            float num2 = Mathf.Max(Mathf.Abs(lossyScale[direction]) * capsuleShape.height - worldSpaceRadius * 2f, 0f);
            Vector3 a = capsuleShape.transform.TransformPoint(capsuleShape.center);
            Vector3 zero = Vector3.zero;
            zero[direction] = num2 * 0.5f;
            Vector3 b = capsuleShape.transform.rotation * zero;
            worldSpaceP1 = a + b;
            worldSpaceP2 = a - b;
        }
        
        private static float CalcWorldSpaceRadius(SphereCollider sphereShape)
        {
            float num = 0f;
            Vector3 lossyScale = sphereShape.transform.lossyScale;
            for (int i = 0; i < 3; i++)
            {
                num = Mathf.Max(num, Mathf.Abs(lossyScale[i]));
            }
            return sphereShape.radius * num;
        }

        private static void DrawWireframeCube(Vector3 foward, Vector3 up, Vector3 right, Vector3 offset, Color color)
        {
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
            GL.Vertex(vertex[0] + offset - up / 2f);
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
        }

        private static void DrawFilledCube(Vector3 foward, Vector3 up, Vector3 right, Vector3 offset, Color color)
        {
            Vector3[] vertex = new Vector3[]
            {
                (foward + right) / 2f,
                (-foward + right) / 2f,
                (-foward - right) / 2f,
                (foward - right) / 2f
            };

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

        private static Vector3 GetArbitraryPerpendicularVector(Vector3 v)
        {
            bool flag = v.magnitude == 0f;
            Vector3 result;
            if (flag)
            {
                result = Vector3.forward;
            }
            else
            {
                Vector3 firstAttempt = Vector3.Cross(v, Vector3.forward);
                bool flag2 = firstAttempt.magnitude != 0f;
                if (flag2)
                {
                    result = firstAttempt.normalized;
                }
                else
                {
                    result = Vector3.Cross(v, Vector3.up).normalized;
                }
            }
            return result;
        }

        private static void DrawWireframeHemisphere(float radius, Vector3 offset, Vector3 foward, Vector3 up, Color color, int resolution = 3)
        {
            Vector3 right = Vector3.Cross(foward, up);
            DrawWireframeCircle(radius, up, foward, offset, color, resolution, -1.5707964f, 1.5707964f, false);
            DrawWireframeCircle(radius, foward, right, offset, color, resolution, 0f, 6.2831855f, true);
            DrawWireframeCircle(radius, right, foward, offset, color, resolution, -1.5707964f, 1.5707964f, false);
            Vector3 temp;
            for (int i=0; i<40; i++)
            {
                temp = -up;
                up = Vector3.Slerp(up, right, 0.1f);
                right = Vector3.Slerp(right, temp, 0.1f);
                DrawWireframeCircle(radius, up, foward, offset, color, resolution, -1.5707964f, 1.5707964f, false);
            }
        }

        private static void DrawWireframeCapsule(float radius, Vector3 startPoint, Vector3 endPoint, Color color, int resolution = 3)
        {
            Vector3 direction = startPoint - endPoint;
            Vector3 randomUpVector = GetArbitraryPerpendicularVector(direction);
            DrawWireframeHemisphere(radius, startPoint, direction, randomUpVector, color, resolution);
            DrawWireframeHemisphere(radius, endPoint, -direction, -randomUpVector, color, resolution);
            GL.Begin(GL.LINES);
            float angleStep = 6.2831855f / (float)40;
            Vector3 rotationVector = Vector3.Cross(direction.normalized, randomUpVector);
            for (int i = 0; i <= 40; i++)
            {
                Vector3 radiusVector = GetRotatedVectorComponent(rotationVector, randomUpVector, angleStep * (float)i);
                Vector3 vertex = radiusVector * radius + startPoint;
                Vector3 vertex2 = radiusVector * radius + endPoint;
                GL.Color(color);
                GL.Vertex(vertex);
                GL.Vertex(vertex2);
            }
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

        private static void DrawWireframeCircle(float radius, Vector3 normal, Vector3 up, Vector3 offset, Color color, int resolution = 3, float startAngle = 0f, float endAngle = 6.2831855f, bool isWholeCircle = true)
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

        private static void DrawWireframeCircleWithCrossbeams(float radius, Vector3 normal, Vector3 up, Vector3 offset, Color color, int resolution = 3, float startAngle = 0f, float endAngle = 6.2831855f, bool isWholeCircle = true)
        {
            DrawWireframeCircle(radius, normal, up, offset, color, resolution, startAngle, endAngle, isWholeCircle);
            GL.Begin(GL.LINES);
            GL.Color(color);
            normal = normal.normalized;
            up = up.normalized;
            for (int i = 0; i < 12; i++)
            {
                Vector3 sideways = Vector3.Cross(normal, up);

                Vector3 radiusVector = GetRotatedVectorComponent(sideways, up, 1/3f * (float)i + startAngle);
                GL.Vertex(radiusVector * radius + offset);
                GL.Vertex(-radiusVector * radius + offset);

                up = Vector3.Slerp(up, sideways, 1/3f);
            }
            GL.End();
        }

        private static void DrawWireframeCone(float coneRadiusStart, float coneRadiusEnd, Vector3 coneStart, Vector3 coneEnd, Color color, int resolution = 3)
        {
            Vector3 direction = coneEnd - coneStart;
            Vector3 randomFowardVector = GetArbitraryPerpendicularVector(direction);
            DrawWireframeCircleWithCrossbeams(coneRadiusStart, direction, randomFowardVector, coneStart, color, resolution, 0f, 6.2831855f, true);
            DrawWireframeCircleWithCrossbeams(coneRadiusEnd, direction, randomFowardVector, coneEnd, color, resolution, 0f, 6.2831855f, true);
            GL.Begin(1);
            float angleStep = 6.2831855f / (float)resolution;
            Vector3 rotationVector = Vector3.Cross(direction.normalized, randomFowardVector);
            for (int i = 0; i <= resolution; i++)
            {
                Vector3 radiusVector = GetRotatedVectorComponent(rotationVector, randomFowardVector, angleStep * (float)i);
                Vector3 vertex = radiusVector * coneRadiusStart + coneStart;
                Vector3 vertex2 = radiusVector * coneRadiusEnd + coneEnd;
                GL.Color(color);
                GL.Vertex(vertex);
                GL.Vertex(vertex2);
            }
            GL.End();
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
    }

}
