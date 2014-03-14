using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRL.Entities
{
    class Player : Mob
    {
        public static Dictionary<int, String> HPDesc = new Dictionary<int,string>()
        {
            {20, "Feeling perfectly fine." },
            {0, "*coughs up blood*"}
        };

        public static Dictionary<int, String> SanityDesc = new Dictionary<int, string>()
        {
            {30, "Everything is fine." },
            {0, "ohgod ohgod ohgod ohgod"}
        };

        public static Player MainPlayer;

        public static int PlayerID = 0;

        public Stat Sanity { get; private set; }

        public Player(Level l)
            :base(l, "playa", 50, true)
        {
            Sanity = new Stat("Sanity", 30, true, Player.SanityDesc);
            Sanity.Value -= 5;
            MainPlayer = this;
            this.Character = '@';
            this.ViewPriority = Int32.MaxValue;
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            List<int> entities = GetEntitiesSharingTileWithThis();
            if (entities.Count > 0)
            {
                if (entities.Count == 1)
                {
                    DarkRL.WriteMessage("You see a " + level.GetEntity(entities[0]).Name + " here.");
                }
                else
                    DarkRL.WriteMessage("You see a whole load of various crap here.");
            }
            level.NeedsLightingUpdate();
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
                base.PickupItem((Item)level.GetEntity(items[0]));
            DarkRL.WriteMessage("You pick up the " + level.GetEntity(items[0]).Name);
        }

        public void DropItem()
        {
            DarkRL.WriteMessage("What do you wish to drop?");
            Key key = InputSystem.WaitForAndReturnInput();
            char index = (char)(key.Character - 'a');
            if (index >= Backpack.Size || index < 0)
            {
                DarkRL.WriteMessage("Couldn't find anything..");
                return;
            }
            DropItem(Backpack[index]);
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
            Tile trying = level[tryPos];
            if (trying.Type == TileType.ClosedDoor)
            {
                if (level[tryPos.X - 1, tryPos.Y].Type != TileType.Floor) //it's a wall that side
                    trying.Data = Tile.OpenLeftRightDoor;
                else
                    trying.Data = Tile.OpenUpDownDoor;
                level.SetLightingCellObscured(tryPos, trying.IsObscuring);
                level.NeedsLightingUpdate();
                DarkRL.WriteMessage("You open the door.");
            }
            else
                DarkRL.WriteMessage("There's nothing there to open..");
        }

        private List<int> GetEntitiesSharingTileWithThis()
        {
            //see if anything is here
            List<int> entities = new List<int>(level.GetEntities(Tile.PositionToID(this.Position)));
            entities.Remove(0);
            //entities.Remove(Lantern.ID);
            return entities;
        }
    }
}
