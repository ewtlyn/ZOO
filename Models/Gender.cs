using System;
using System.Collections.Generic;

namespace woww.Models;

public partial class Gender
{
    public int GenderId { get; set; }

    public string Gender1 { get; set; } = null!;

    public virtual ICollection<Animal> Animals { get; } = new List<Animal>();
}
