using System;
using System.Collections.Generic;

#nullable disable

namespace DiChoSaiGon.Models
{
    public partial class WishList
    {
        public int IdWishList { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public string ProductName { get; set; }
        public int Price { get; set; }
        public bool Active { get; set; }
        public string Thumb { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Product Product { get; set; }
    }
}
