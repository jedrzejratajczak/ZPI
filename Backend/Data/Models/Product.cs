﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Data.Models
{
    public class Product
    {
        [Key]
        public Guid Id { get; set; }
        
        public int Version { get; set; }

        [Required]
        public string Name { get; set; }

        public DateTime CreationDate { get; set; }

        public virtual ICollection<TestPlan> TestPlans { get; set; }
        public virtual ICollection<TestCase> TestCases { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
