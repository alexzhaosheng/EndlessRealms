using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EndlessRealms.Models
{
    public class World
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string[] AdjectiveWords { get; set; } = null!;        
        public Region[] Regions { get; set; } = null!;
        public List<Scene> Scenes { get; set; } = new List<Scene>();

        public World()
        {
            Id = Guid.NewGuid().ToString();
            
        }
    }
}
