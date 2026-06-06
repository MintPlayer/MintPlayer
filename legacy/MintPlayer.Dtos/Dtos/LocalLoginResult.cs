using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MintPlayer.Dtos.Dtos
{
    public class LocalLoginResult : LoginResult
    {
        [DataMember]
        [XmlElement(Namespace = "https://mintplayer.com/music")]
        public string Token { get; set; }
    }
}
