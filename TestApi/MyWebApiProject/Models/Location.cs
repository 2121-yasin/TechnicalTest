using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyWebApiProject.Models
{
public class Location
{
    [Key]
    public int Id { get; set; }
    [Required(ErrorMessage = "Title is required")]
    public string Title { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
    public string Zip { get; set; }
}
}