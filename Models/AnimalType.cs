using System;
using System.Collections.Generic;

namespace woww.Models;

public partial class AnimalType
{
    public int AnimalTypeId { get; set; }

    public string Type { get; set; } = null!;

    public TimeSpan? EatingInterval { get; set; }

    public virtual ICollection<Animal> Animals { get; } = new List<Animal>();
}
