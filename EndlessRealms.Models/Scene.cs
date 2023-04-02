using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EndlessRealms.Models
{
    public enum Direction
    {
        East = 0,
        North = 1,
        West = 2,
        South = 3        
    }    

    public class Scene
    {
        public string Id { get; set; } = null!;

        public string RegionId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string Description { get; set; } = null!;

        public Dictionary<Direction, string?> ConnectedScenes { get; } = new Dictionary<Direction, string?>();
        public List<Something> Things { get; set; } = new List<Something>();

        public List<CharactorInfo> Charactors { get; set; } = new List<CharactorInfo>();
        public Scene()
        {
            Id = Guid.NewGuid().ToString();            
        }                
    }
}


