using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libtcod;

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

        public static readonly int PlayerID = 0;

        public static readonly int LightingThreshold = 5;

        public Stat Sanity { get; private set; }

        public Item LanternSlot;

        public Player(Level l)
            :base(l, "dickbutt", 50, true)
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

        public void Equip()
        {
            DarkRL.WriteMessage("What do you wish to equip?");
            Item i = GetBackpackSlot();
            if (i.Slot == EquipSlot.None)
            {
                DarkRL.WriteMessage("You can't equip that.");
                return;
            }
            else
            {
                if (GetEquippedItem(i.Slot) == null)
                {
                    Equip(i);
                    return;
                }
                else //we've got something already equipped
                {
                    DarkRL.WriteMessage("You already have " + GetEquippedItem(i.Slot) + " equipped. Do you wish to swap items? (y/n)");
                    Key response = InputSystem.WaitForAndReturnInput();
                    if (response.Character == 'y')
                    {
                        String removed = Unequip(i.Slot).Name;
                        Equip(i);
                    }
                }
            }
        }

        public Item GetEquippedItem(EquipSlot e)
        {
            switch (e)
            {
                case EquipSlot.Lantern:
                    return this.LanternSlot;
            }
            return null;
        }

        public void Equip(Item i)
        {
            if (!Backpack.Items.Select(t => t.Value).Contains(i))
                this.AddItem(i);

            switch (i.Slot)
            {
                case EquipSlot.Lantern:
                    this.LanternSlot = i;
                    break;
                case EquipSlot.None:
                    return;
            }
            i.IsEquipped = true;
            DarkRL.WriteMessage("You equip the " + i.Name + " in your " + i.Slot.ToString() + " slot.");
        }

        public List<Item> GetAllEquippedItems()
        {
            List<Item> items = new List<Item>();
            if (this.LanternSlot != null)
                items.Add(this.LanternSlot);
            return items;
        }

        public void Unequip()
        {
            List<Item> equipped = GetAllEquippedItems();
            if(equipped.Count == 0)
            {
                DarkRL.WriteMessage("There's nothing to unequip.");
                return;
            }
            TCODConsole overlay = new TCODConsole(40, equipped.Count);
            char sym = 'a';
            int y = 0;
            //now display them all
            foreach (Item i in equipped)
            {

                overlay.print(0, y, sym.ToString() + ")" + i.ToString());
                ++y;
                ++sym;
            }
            DarkRL.WriteMessage("What do you want to unequip?");
            DarkRL.AddOverlayConsole(overlay, 0, 0, overlay.getWidth(), overlay.getHeight(), TCODConsole.root, Window.StatusPanelWidth,
                Window.MessagePanelHeight);
            Key input = InputSystem.WaitForAndReturnInput();
            char index = (char)(input.Character - 'a');
            if (index >= equipped.Count || index < 0)
            {
                DarkRL.WriteMessage("Couldn't find anything..");
                return;
            }
            //so unequip
            Item item = Unequip(Backpack[index].Slot);
            return;
        }

        public Item Unequip(EquipSlot e)
        {
            Item i = null;
            switch (e)
            {
                case EquipSlot.Lantern:
                    i = this.LanternSlot;
                    this.LanternSlot.IsEquipped = false;
                    this.LanternSlot = null;
                    break;
                case EquipSlot.None:
                    return null;
            }
            DarkRL.WriteMessage("You unequip the " + i.Name + ".");
            return i;
        }

        public void Inspect()
        {
            DarkRL.WriteMessage("Where do you want to inspect?");
            Key input = null;
            Point point = this.Position;
            while (input == null || input.KeyCode != libtcod.TCODKeyCode.Enter)
            {
                input = InputSystem.WaitForAndReturnInput();
                if (DarkRL.LeftMovement.Contains(input))
                    point += DarkRL.Directions[Direction.West];
                else if (DarkRL.RightMovement.Contains(input))
                    point += DarkRL.Directions[Direction.East];
                else if (DarkRL.UpMovement.Contains(input))
                    point += DarkRL.Directions[Direction.North];
                else if (DarkRL.DownMovement.Contains(input))
                    point += DarkRL.Directions[Direction.South];
                else
                    break;
            }
            Inspect(point);
        }

        private void Inspect(Point p)
        {
            //todo: stuff to make it cooler
                //our cases are:
                //not in LoS, unexplored
                //explored
                //in LoS, low light
                //in LoS, good light
            bool isExplored = level.GetLightingInformation(p).IsExplored;

            if (!isExplored)
            {
                DarkRL.WriteMessage("You have no idea what is there.");
                return;
            }

            List<int> entities = level.GetEntities(Tile.PositionToID(p));

            //explored from here on
            //if it's out of our sight or it's really dark
            if (!CanSee(p) || level.GetLightLevel(p) == 0)
            {
                //todo: sanity, more specific
                DarkRL.WriteMessage("You can't see what's over there..but " + (entities.Count == 0 ? "you don't remember anything being there" : 
                    "you vaguely remember something.."));
                return;
            }

            //explored and can see it from here on.
            //nothing here
            if (entities.Count == 0)
            {
                DarkRL.WriteMessage("You can't see anything there..");
                return;
            }

            List<Item> items = new List<Item>();
            Entity ent = null;

            foreach (Entity e in entities.Select(t => level.GetEntity(t)))
            {
                Item i = e as Item;
                if (i == null)
                    ent = e;
                else
                    items.Add(i);
            }

            //if the light is bad
            if (level.GetLightLevel(p) <= Player.LightingThreshold)
            {
                StringBuilder str = new StringBuilder();
                str.Append("You can see ");
                if(ent != null)
                    str.Append("a brooding shadow.");
                if(items.Count > 0)
                    str.Append(items.Count > 1 ? "You see a pile of assorted items on the ground" : "You see some item on the ground");
                str.Append(".");

                DarkRL.WriteMessage(str.ToString());
                return;
            }

            //we can assume the light is good
            StringBuilder goodLightStr = new StringBuilder();
            goodLightStr.Append("You can see ");
            if (ent != null)
                goodLightStr.Append("a " + ent.Name + (items.Count == 0 ? "" : "."));
            if (items.Count > 0)
            {
                if (ent != null)
                    goodLightStr.Append(ent != null ? "There is " : " ");
                if (items.Count == 1)
                    goodLightStr.Append("a " + items[0].Name + " on the ground");
                else
                {
                    for (int i = 0; i < items.Count; ++i)
                    {
                        goodLightStr.Append( (i == items.Count - 1 ? " and " : "") + "a " + items[i].Name +
                            (i != items.Count - 1 && items.Count != 2 ? "," : (items.Count != 2 ? " here" : "")));
                    }
                }
            }
            goodLightStr.Append(".");

            DarkRL.WriteMessage(goodLightStr.ToString());
            return;
        }

        public void PickupItem()
        {
            List<int> items = GetEntitiesSharingTileWithThis();
            if(items.Count == 0)
            {
                DarkRL.WriteMessage("There's nothing to pick up..");
                return;
            }
            else if (items.Count == 1)
                base.PickupItem((Item)level.GetEntity(items[0]));
            else //there's multiple items here
            {
                TCODConsole pickup = new TCODConsole(30, items.Count+1);
                pickup.setBackgroundColor(TCODColor.black);
                pickup.setForegroundColor(TCODColor.white);
                char sym = 'a';
                int y = 0;
                //now display them all
                foreach (Item i in items.Select(t => level.GetEntity(t)))
                {

                    pickup.print(0, y, sym.ToString() + ")" + i.ToString());
                    ++y;
                    ++sym;
                }
                DarkRL.WriteMessage("What do you want to pick up?");
                DarkRL.AddOverlayConsole(pickup, 0, 0, pickup.getWidth(), pickup.getHeight(), TCODConsole.root, Window.StatusPanelWidth, 
                    Window.MessagePanelHeight);
                Key input = InputSystem.WaitForAndReturnInput();
                char index = (char)(input.Character - 'a');
                if (index >= items.Count|| index < 0)
                {
                    DarkRL.WriteMessage("Couldn't find anything..");
                    return;
                }
                //so pick it up
                base.PickupItem((Item)level.GetEntity(items[index]));
                DarkRL.WriteMessage("You pick up the " + level.GetEntity(items[index]).Name);
                return;
            }
            DarkRL.WriteMessage("You pick up the " + level.GetEntity(items[0]).Name);
        }

        public void DropItem()
        {
            DarkRL.WriteMessage("What do you wish to drop?");
            Item i = GetBackpackSlot();
            if (i != null)
            {
                DropItem(i);
                DarkRL.WriteMessage("Dropped the " + i + ".");
            }
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

        private Item GetBackpackSlot()
        {
            Key key = InputSystem.WaitForAndReturnInput();
            char index = (char)(key.Character - 'a');
            if (index >= Backpack.Size || index < 0)
            {
                DarkRL.WriteMessage("Couldn't find anything..");
                return null;
            }
            return this.Backpack[index];
        }

        private List<int> GetEntitiesSharingTileWithThis()
        {
            //see if anything is here
            List<int> entities = new List<int>(level.GetEntities(Tile.PositionToID(this.Position)));
            entities.Remove(0);
            //entities.Remove(Lantern.ID);
            return entities;
        }

        public bool CanSee(Point p)
        {
            //TODO: not have super laser eyes
            return true;
        }

    }
}
