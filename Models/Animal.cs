using System;
using System.Collections.Generic;

namespace woww.Models;

public partial class Animal
{
    public int AnimalId { get; set; }

    public string Name { get; set; } = null!;

    public int? Age { get; set; }

    public string? Photo { get; set; }

    public int? AnimalTypeId { get; set; }

    public int? AnimalViewId { get; set; }

    public int? VolierId { get; set; }

    public int? GenderId { get; set; }

    public bool? IsHungry { get; set; }

    public TimeSpan? EatingInterval { get; set; }

    public virtual AnimalType? AnimalType { get; set; }

    public virtual AnimalView? AnimalView { get; set; }

    public virtual Gender? Gender { get; set; }

    public virtual Voiler? Volier { get; set; }
}
