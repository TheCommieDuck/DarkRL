using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRL
{
    class Player : Mob
    {

        public static Dictionary<int, String> HPDesc = new Dictionary<int,string>()
        {
            {20, "feeling perfectly fine" },
            {0, "*coughs up blood*"}
        };

        public static Player MainPlayer;
        public LightSource Lantern;

        public Player(Level l)
            :base(l, "dickbutt", 50, true)
        {
            MainPlayer = this;
            this.Character = '@';
            this.ViewPriority = Int32.MaxValue;
            Lantern = new LightSource(l);
            Lantern.LightRadius = 10;
            l.AddLightSource(Lantern);
        }

        public override void SetPosition(int x, int y)
        {
            l.NeedsLightingUpdate();
            base.SetPosition(x, y);
            Lantern.SetPosition(x, y);
            List<int> entities = GetEntitiesSharingTileWithThis();
            if (entities.Count > 0)
            {
                if (entities.Count == 1)
                {
                    DarkRL.WriteMessage("You see a " + l.GetEntity(entities[0]).Name + " here.");
                }
                else
                    DarkRL.WriteMessage("You see a whole load of various crap here.");
            }
        }

        public void PickupItem()
        {
            List<int> items = GetEntitiesSharingTileWithThis();
            if(items.Count == 0)
            {
                DarkRL.WriteMessage("There's nothing to pick up..");
                return;
            }
            else if(items.Count == 1)
                base.PickupItem((Item)l.GetEntity(items[0]));
            DarkRL.WriteMessage("You pick up the " + l.GetEntity(items[0]).Name);
        }

        public void Open()
        {
            DarkRL.WriteMessage("Which direction do you want to open in?");
            Key input = InputSystem.WaitForAndReturnInput();
            if (DarkRL.LeftMovement.Contains(input))
                OpenInDirection(Direction.West);
            else if (DarkRL.RightMovement.Contains(input))
                OpenInDirection(Direction.East);
            else if (DarkRL.UpMovement.Contains(input))
                OpenInDirection(Direction.North);
            else if (DarkRL.DownMovement.Contains(input))
                OpenInDirection(Direction.South);
            else
                DarkRL.WriteMessage("That's not a valid direction.");
        }

        private void OpenInDirection(Direction dir)
        {
            Point tryPos = new Point(this.Position.X + DarkRL.Directions[dir].X, this.Position.Y + DarkRL.Directions[dir].Y);
            Tile trying = l[tryPos];
            if (trying.Type == TileType.ClosedDoor)
            {
                if(l[tryPos.X-1, tryPos.Y].Type != TileType.Floor) //it's a wall that side
                    trying.Data = Tile.OpenLeftRightDoor;
                else
                    trying.Data = Tile.OpenUpDownDoor;
                l.SetLightingCellObscured(tryPos, trying.IsObscuring);
                l.NeedsLightingUpdate();
                DarkRL.WriteMessage("You open the door.");
            }
            else
                DarkRL.WriteMessage("There's nothing there to open..");
        }

        private List<int> GetEntitiesSharingTileWithThis()
        {
            //see if anything is here
            List<int> entities = new List<int>(l.GetEntities(Tile.PositionToID(this.Position)));
            entities.Remove(0);
            entities.Remove(Lantern.ID);
            return entities;
        }
    }
}
