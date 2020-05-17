using System.Collections.Generic;

namespace MintPlayer.Dtos.Dtos
{
    public class Tag
    {
        public int Id { get; set; }

        public string Description { get; set; }
        public TagCategory Category { get; set; }

        public List<Subject> Subjects { get; set; }

        public Tag Parent { get; set; }
        public List<Tag> Children { get; set; }

        public string Text => Description;
    }
}
