using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRL
{
    class Player : Entity
    {
        public LightSource Lantern;
        public Player(Level l)
            :base(l, true)
        {
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
    }
}
