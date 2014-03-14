using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRL.Entities
{
    class Backpack
    {
        private List<Item> items;

        public Mob Owner { get; set; }

        public int Size
        {
            get
            {
                return items.Count;
            }
        }
        public Item this[int index]
        {
            get
            {
                return items[index];
            }
        }
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

        public IEnumerable<KeyValuePair<String, Item>> Items
        {
            get
            {
                char itemKey = 'a';
                foreach (Item i in items)
                {
                    yield return new KeyValuePair<String, Item>(itemKey + ")", i);
                    itemKey++;
                }
            }
        }
    }
}
