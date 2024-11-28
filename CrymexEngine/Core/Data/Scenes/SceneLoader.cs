using OpenTK.Mathematics;
using System;

namespace CrymexEngine.Scenes
{
    public static class SceneLoader
    {
        public static bool LoadScene(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            string[] precompiled = File.ReadAllLines(path);

            Scene scene = SceneLanguageCompiler.CompileScene(precompiled);

            Scene.Current = scene;

            return true;
        }
    }

    static class SceneLanguageCompiler
    {
        static Scene scene;

        public static Scene CompileScene(string[] lines)
        {
            List<Entity> entities = new List<Entity>();
            List<Behaviour> behaviours = new List<Behaviour>();
            List<Collider> colliders = new List<Collider>();

            scene = new Scene(entities, behaviours, colliders);
            for (int l = 0; l < lines.Length; l++) 
            {
                if (string.IsNullOrEmpty(lines[l])) continue;

                string command = lines[l];

                if (command.Length > 5 && command.Substring(0, 6) == "Entity")
                {
                    int index = 6;
                    string entityName = "";

                    command = command.Replace(" ", "");

                    while (command[index] != '{')
                    {
                        entityName += command[index];
                        index++;
                    }

                    int entityEndIndex = l + 1;
                    while (!lines[entityEndIndex].Contains('}'))
                    {
                        entityEndIndex++;
                    }

                    string[] entityParameters = lines[(l + 1)..(entityEndIndex)];
                     
                    l = entityEndIndex + 1;

                    CompileEntity(entityName, entityParameters);
                }
            }

            return scene;
        }

        private static Entity CompileEntity(string name, string[] parameters)
        {
            Texture texture = Texture.Missing;
            Vector2 position = Vector2.Zero;
            Vector2 localPosition = Vector2.Zero;
            Vector2 scale = Vector2.Zero;
            string parentName = "";

            for (int p = 0; p < parameters.Length; p++)
            {
                string[] parts = parameters[p].Split(':', StringSplitOptions.TrimEntries);
                if (parts.Length != 2) continue;

                switch (parts[0])
                {
                    case "X":
                        {
                            if (!int.TryParse(parts[1], out int x)) continue;
                            position.X = x;
                            break;
                        }
                    case "Y":
                        {
                            if (!int.TryParse(parts[1], out int y)) continue;
                            position.Y = y;
                            break;
                        }
                    case "LocalX":
                        {
                            if (!int.TryParse(parts[1], out int x)) continue;
                            localPosition.X = x;
                            break;
                        }
                    case "LocalY":
                        {
                            if (!int.TryParse(parts[1], out int y)) continue;
                            localPosition.Y = y;
                            break;
                        }
                    case "Width":
                        {
                            if (!int.TryParse(parts[1], out int x)) continue;
                            scale.X = x;
                            break;
                        }
                    case "Height":
                        {
                            if (!int.TryParse(parts[1], out int y)) continue;
                            scale.Y = y;
                            break;
                        }
                    case "Texture":
                        {
                            string textureName = parts[1].Replace("\"", "").Replace("'", "");
                            texture = Assets.GetTexture(textureName);
                            break;
                        }
                    case "Parent":
                        {
                            parentName = parts[1].Replace("\"", "").Replace("'", "");
                            break;
                        }
                }
            }

            Entity entity = new Entity(texture, position, scale, scene, name);

            entity.Parent = Entity.GetEntity(parentName, scene);
            if (localPosition != Vector2.Zero) entity.localPosition = localPosition;

            return entity;
        }
    }
}
