using OpenTK.Mathematics;

namespace CrymexEngine.Scenes
{
    // WIP
    public static class SceneLoader
    {
        public static bool LoadScene(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            string[] precompiled = File.ReadAllLines(path);

            Scene scene = SceneCompiler.CompileScene(precompiled);

            Scene.Load(scene);

            return true;
        }
    }

    // WIP
    static class SceneCompiler
    {
        static Scene scene;

        public static Scene CompileScene(string[] lines)
        {
            scene = new Scene();
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

        // WIP
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

                float.TryParse(parts[1], out float parsed);
                if (!float.IsNormal(parsed)) parsed = 0;

                switch (parts[0])
                {
                    case "X":
                        {
                            position.X = parsed;
                            break;
                        }
                    case "Y":
                        {
                            position.Y = parsed;
                            break;
                        }
                    case "LocalX":
                        {
                            localPosition.X = parsed;
                            break;
                        }
                    case "LocalY":
                        {
                            localPosition.Y = parsed;
                            break;
                        }
                    case "Width":
                        {
                            scale.X = parsed;
                            break;
                        }
                    case "Height":
                        {
                            scale.Y = parsed;
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

            Entity entity = new Entity(texture, position, scale, scene, Entity.GetEntity(parentName, scene), name);

            entity.LocalPosition = localPosition;

            return entity;
        }
    }
}
