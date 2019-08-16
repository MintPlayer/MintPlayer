namespace MintPlayer.Web.Server.ViewModels.Subject
{
    public class SubjectLikeVM
    {
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public bool? Like { get; set; }
        public bool Authenticated { get; set; }
    }
}
