﻿using System.ComponentModel.DataAnnotations;

namespace SampleWebApp.Models
{
    public class Todo : Tada
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = default!;
        public bool IsComplete { get; set; }
    }
}