using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MintPlayer.Dtos.Dtos
{
    public class ExternalLoginResult : LoginResult
    {
        [DataMember]
        [XmlElement(Namespace = "https://mintplayer.com/music")]
        public string Platform { get; set; }
    }
}
