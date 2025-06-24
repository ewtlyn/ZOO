using System;
using System.Collections.Generic;

namespace woww.Models;

public partial class AnimalView
{
    public int AnimalViewId { get; set; }

    public string View { get; set; } = null!;

    public virtual ICollection<Animal> Animals { get; } = new List<Animal>();
}
