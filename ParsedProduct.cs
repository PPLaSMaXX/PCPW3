using System;
using System.Windows.Forms.VisualStyles;

namespace PCPW3
{
    class ParsedProduct
    {
        public ParsedProduct(string category, string price, string name)
        {
            this.Category = category;
            this.Name = name;
            this.Price = Int32.Parse(price);
        }
        public string Category { get; }
        public string Name { get; }
        public int Price { get; }
    }
}
