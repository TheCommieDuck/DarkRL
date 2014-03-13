﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libtcod;

namespace DarkRL
{
    class Item : Entity
    {
        public static Point InInventory = new Point(-69, -69);

        public static Item NewBarOfFoobium(Level l)
        {
            Item i = new Item(l);
            i.Name = "bar of foobium";
            i.Character = '-';
            i.Color = TCODColor.darkGrey;
            return i;
        }

        public Entity Owner { get; set; }
        public override Point Position
        {
            get
            {
                if (Owner == null)
                    return base.Position;
                else //then just use this
                    return Item.InInventory;
            }
        }

        public Item(Level l)
            :base(l)
        {
            ViewPriority = 0;
        }
    }

    class Backpack
    {
        private List<Item> items;

        public Mob Owner { get; set; }

        public Backpack(Mob owner)
        {
            Owner = owner;
            items = new List<Item>();
        }

        public void AddItem(Item i)
        {
            i.Owner = Owner;
            items.Add(i);
        }

        public void RemoveItem(Item i)
        {
            items.Remove(i);
            i.Owner = null;
        }
    }
}