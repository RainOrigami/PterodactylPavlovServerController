﻿using System.ComponentModel.DataAnnotations;

namespace PterodactylPavlovServerController.Models;

public class TTTKarma
{
    [Required]
    public ulong? UniqueId { get; set; }

    public int? Amount { get; set; }

    [Required]
    public bool? Set { get; set; }
}
