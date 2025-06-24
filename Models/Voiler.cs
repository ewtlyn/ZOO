using System;
using System.Collections.Generic;

namespace woww.Models;

public partial class Voiler
{
    public int VoilerId { get; set; }

    public string Number { get; set; } = null!;

    public string? Location { get; set; }

    public virtual ICollection<Animal> Animals { get; } = new List<Animal>();
}
