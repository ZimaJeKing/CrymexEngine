using CrymexEngine.UI;
using CrymexEngine.Utils;
using OpenTK.Mathematics;

namespace CrymexEngine.Scripting
{
    public class ShooterGameExample : ScriptableBehaviour
    {
        public static Entity playerEntity;
        Player player;
        public static TextObject text;
        public static int scoreCounter = 0;
        public static UIElement[] livesUI = new UIElement[3];

        public override void Load()
        {
            Entity background = new Entity(Assets.GetTexture("PixelBackground"), Vector2.Zero, new Vector2(1920, 1080), null, "Background");
            background.Renderer.Depth = -50;
            background.interactible = false;

            text = new TextObject(new Vector2(0, Window.HalfSize.Y - 64), new Vector2i(250, 50), "Score: 0", Assets.GetFontFamily("Jakarta"), 20, Alignment.MiddleCenter, SixLabors.Fonts.FontStyle.Regular);
            text.FontColor = Color4.White;

            playerEntity = new Entity(Assets.GetTexture("Player"), new Vector2(0, 0), new Vector2(100), null, "Player");
            playerEntity.AddComponent<Player>();
            playerEntity.Renderer.Depth = 10;

            player = playerEntity.GetComponent<Player>();

            for (int i = 0; i < livesUI.Length; i++)
            {
                livesUI[i] = new UIElement(Assets.GetTexture("EnergyDrink"), new Vector2(-Window.HalfSize.X + 64 * (i + 1), Window.HalfSize.Y - 64), new Vector2(64));
            }

            SpawnEnemy();
            EventSystem.AddEventRepeat("SpawnEnemy", SpawnEnemy, 3);
        }

        public override void Update()
        {
            playerEntity.Position += Axis2D.WSAD.GetValue() * 100 * Time.DeltaTime;
            Camera.position = playerEntity.Position;

            if (player.lives <= 0) Engine.Quit();
        }

        private void SpawnEnemy()
        {
            Entity enemy = new Entity(Assets.GetTexture("PoliceCar"), playerEntity.Position + Random.PointOnUnitCircle() * 300, new Vector2(100), null, "Enemy");
            enemy.AddComponent<EnemyComponent>();
        }
    }

    class EnemyComponent : EntityComponent
    {
        private Player player;

        public override void Load()
        {
            player = Entity.GetEntity("Player").GetComponent<Player>();
        }

        public override void Update()
        {
            Entity.Position = VectorUtility.MoveTowards(Entity.Position, player.Entity.Position, 100 * Time.DeltaTime);
            if (Vector2.Distance(player.Entity.Position, Entity.Position) < 50)
            {
                player.AddLives(-1);
                Entity.Delete();
            }
        }

        public override void OnMouseDown(MouseButton mouseButton)
        {
            if (mouseButton != MouseButton.Left) return;

            ShooterGameExample.scoreCounter++;
            ShooterGameExample.text.Text = "Score: " + ShooterGameExample.scoreCounter.ToString();

            Entity.Delete();
        }

        public override void OnMouseEnter()
        {
            Renderer.color = Color4.Red;
        }

        public override void OnMouseExit()
        {
            Renderer.color = Color4.White;
        }
    }

    class Player : EntityComponent
    {
        public int lives = 3;
        public float speed = 200;
        public Vector2 velocity;

        public void AddLives(int count)
        {
            lives += count;
            for (int i = 0; i < ShooterGameExample.livesUI.Length; i++)
            {
                ShooterGameExample.livesUI[i].enabled = i < lives;
            }
        }

        public override void Load()
        {

        }

        public override void Update()
        {
            Entity.Rotation = VectorUtility.AngleBetween(Vector2.Zero, Input.MousePosition) - 90;
            Entity.Position += Axis2D.WSAD.GetValue() * speed * Time.DeltaTime;
        }
    }

    class Bullet : EntityComponent
    {
        public Vector2 direction;
        public Vector2 speed;

        public override void Load()
        {

        }

        public override void Update()
        {
            Entity.Position += direction * speed * Time.DeltaTime;
        }
    }
}