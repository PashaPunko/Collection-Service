using System.ComponentModel.DataAnnotations;
using PostgresTest.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace PostgresTest.ViewModels
{
    public class CollectionCreateViewModel: Collection
    {
        public IFormFile file { get; set; }
    }
}